using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Faryma.Composer.Api.Common.Attributes;
using Faryma.Composer.Contracts.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    [ApiController]
    [Authorize]
    [Route("api/_test/idempotency")]
    public sealed class TestIdempotencyController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, int> _executions = new(StringComparer.Ordinal);
        private static readonly ConcurrentDictionary<string, byte> _failures = new(StringComparer.Ordinal);

        public static void Reset(string scenario)
        {
            _executions.TryRemove(scenario, out _);
            _failures.TryRemove(scenario, out _);
        }

        public static void FailNext(string scenario) => _failures[scenario] = 0;
        public static int GetExecutionCount(string scenario) => _executions.GetValueOrDefault(scenario);

        [HttpPost]
        [Idempotent]
        public async Task<ActionResult<TestIdempotencyResponse>> Execute(TestIdempotencyRequest request, CancellationToken ct)
        {
            if (_failures.TryRemove(request.Scenario, out _))
            {
                throw new TestIdempotencyException();
            }

            if (request.DelayMilliseconds > 0)
            {
                await Task.Delay(request.DelayMilliseconds, ct);
            }

            int executions = _executions.AddOrUpdate(request.Scenario, 1, (_, current) => current + 1);

            return Ok(new TestIdempotencyResponse(request.Scenario, request.Value, executions));
        }

        [HttpPost("unsupported-result")]
        [Idempotent]
        public IActionResult UnsupportedResult() => NoContent();
    }

    public sealed record TestIdempotencyRequest(string Scenario, string Value, int DelayMilliseconds = 0);

    public sealed record TestIdempotencyResponse(string Scenario, string Value, int Executions);

    internal sealed class TestIdempotencyException([CallerMemberName] string callerMemberName = "")
        : AppException("Тестовая ошибка идемпотентного endpoint", callerMemberName);
}
