using System.Security.Cryptography;
using System.Text.Json;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Npgsql;

namespace Faryma.Composer.Api.Common.Filters
{
    public sealed class IdempotentFilter(
        AppDbContext appDbContext,
        DateTimeService dateTimeService,
        IOptions<JsonOptions> jsonOptions) : IAsyncActionFilter
    {
        private static readonly TimeSpan _expiration = TimeSpan.FromHours(1);
        private readonly JsonSerializerOptions _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;

        public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            if (!TryReadIdempotencyKey(actionContext, out Guid idempotencyKey))
            {
                return;
            }

            Guid userId = actionContext.HttpContext.User.GetUserId();
            string endpointKey = GetEndpointKey(actionContext.HttpContext);
            string requestHash = ComputeRequestHash(actionContext);
            DateTime now = dateTimeService.Now;
            CancellationToken ct = actionContext.HttpContext.RequestAborted;

            IdempotencyRecordEntity? existing = await FindExistingRecord(endpointKey, userId, idempotencyKey, ct);
            if (existing is not null)
            {
                if (existing.ExpiresAt > now)
                {
                    actionContext.Result = CreateReplayResult(existing, requestHash);

                    return;
                }

                await DeleteExpiredRecord(endpointKey, userId, idempotencyKey, now, ct);
            }

            await using IDbContextTransaction transaction = await appDbContext.Database.BeginTransactionAsync(ct);

            IdempotencyRecordEntity record = new()
            {
                EndpointKey = endpointKey,
                UserId = userId,
                IdempotencyKey = idempotencyKey,
                RequestHash = requestHash,
                CreatedAt = now,
                ExpiresAt = now.Add(_expiration),
            };

            appDbContext.IdempotencyRecords.Add(record);

            try
            {
                await appDbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                appDbContext.Entry(record).State = EntityState.Detached;
                await transaction.RollbackAsync(ct);
                IdempotencyRecordEntity? conflicting = await FindExistingRecord(endpointKey, userId, idempotencyKey, ct);
                actionContext.Result = conflicting is null
                    ? new ConflictObjectResult("Не удалось обработать ключ идемпотентности")
                    : CreateReplayResult(conflicting, requestHash);

                return;
            }

            ActionExecutedContext executedContext = await next();
            if (executedContext.Canceled || executedContext.Exception is not null)
            {
                await transaction.RollbackAsync(ct);

                return;
            }

            if (!TryCreateStoredResponse(executedContext.Result, out int statusCode, out string responseJson))
            {
                await transaction.RollbackAsync(ct);

                throw new InvalidOperationException("Атрибут [Idempotent] поддерживает только успешные JSON ObjectResult ответы");
            }

            record.StatusCode = statusCode;
            record.ResponseJson = responseJson;

            await appDbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        private static string GetEndpointKey(HttpContext httpContext)
        {
            Endpoint? endpoint = httpContext.GetEndpoint();

            return (endpoint as RouteEndpoint)?.RoutePattern?.RawText
                ?? httpContext.Request.Path.Value
                ?? "unknown";
        }

        private static bool TryReadIdempotencyKey(ActionExecutingContext context, out Guid idempotencyKey)
        {
            idempotencyKey = default;

            if (!context.HttpContext.Request.Headers.TryGetValue(Globals.IdempotencyKey, out StringValues raw))
            {
                context.Result = new BadRequestObjectResult($"Требуется заголовок {Globals.IdempotencyKey}");

                return false;
            }

            if (!Guid.TryParse(raw, out idempotencyKey))
            {
                context.Result = new BadRequestObjectResult($"Некорректный заголовок {Globals.IdempotencyKey}");

                return false;
            }

            if (idempotencyKey == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult($"Пустой заголовок {Globals.IdempotencyKey}");

                return false;
            }

            return true;
        }

        private static bool ShouldSkipActionArgument(string name, object? value)
        {
            return string.Equals(name, "idempotencyKey", StringComparison.OrdinalIgnoreCase)
                || value is CancellationToken;
        }

        private static IActionResult CreateReplayResult(IdempotencyRecordEntity record, string requestHash)
        {
            if (record.StatusCode is null || record.ResponseJson is null)
            {
                return new ConflictObjectResult("Запрос с этим ключом идемпотентности еще не завершен");
            }

            if (!string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return new ConflictObjectResult("Ключ идемпотентности уже использован с другим запросом");
            }

            return new ContentResult
            {
                Content = record.ResponseJson,
                ContentType = "application/json",
                StatusCode = record.StatusCode,
            };
        }

        private async Task DeleteExpiredRecord(
            string endpointKey,
            Guid userId,
            Guid idempotencyKey,
            DateTime now,
            CancellationToken ct)
        {
            await appDbContext.IdempotencyRecords
                .Where(x => x.EndpointKey == endpointKey
                    && x.UserId == userId
                    && x.IdempotencyKey == idempotencyKey
                    && x.ExpiresAt <= now)
                .ExecuteDeleteAsync(ct);
        }

        private string ComputeRequestHash(ActionExecutingContext context)
        {
            SortedDictionary<string, object?> payload = new(StringComparer.Ordinal);
            foreach ((string name, object? value) in context.ActionArguments)
            {
                if (!ShouldSkipActionArgument(name, value))
                {
                    payload[name] = value;
                }
            }

            byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

            return Convert.ToHexString(SHA256.HashData(json)).ToLowerInvariant();
        }

        private bool TryCreateStoredResponse(IActionResult? result, out int statusCode, out string responseJson)
        {
            statusCode = default;
            responseJson = string.Empty;

            if (result is not ObjectResult objectResult)
            {
                return false;
            }

            statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (statusCode is < 200 or >= 300)
            {
                return false;
            }

            responseJson = JsonSerializer.Serialize(objectResult.Value, _jsonSerializerOptions);

            return true;
        }

        private Task<IdempotencyRecordEntity?> FindExistingRecord(
            string endpointKey,
            Guid userId,
            Guid idempotencyKey,
            CancellationToken ct)
        {
            return appDbContext.IdempotencyRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.EndpointKey == endpointKey
                    && x.UserId == userId
                    && x.IdempotencyKey == idempotencyKey, ct);
        }
    }
}
