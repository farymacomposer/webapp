using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Templates;

namespace Faryma.Composer.Api.Common.Logging
{
    public static class LoggingHostBuilderExtensions
    {
        private const string _jsonTemplate = "{ {"
            + "timestamp: UtcDateTime(@t),"
            + "level: @l,"
            + "traceId: @tr,"
            + "spanId: @sp,"
            + "message: if @mt = '' or @mt is null then undefined() else @mt,"
            + "properties: @p,"
            + "exception: @x"
            + "} }\n";

        public static IHostBuilder UseLogging(this IHostBuilder host)
        {
            return host.UseSerilog((context, config) =>
            {
                LogEventLevel level = Enum.TryParse(context.Configuration["LOG_LEVEL"], ignoreCase: true, out LogEventLevel parsed)
                    ? parsed
                    : LogEventLevel.Information;

                config
                    .MinimumLevel.Is(level)
                    .MinimumLevel.Override("System", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Fatal)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers()
                        .WithDestructurers([new DbUpdateExceptionDestructurer()]))
                    .Destructure.With<AttributeBasedDestructuringPolicy>()
                    .WriteTo.Console(new ExpressionTemplate(_jsonTemplate));

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.WriteTo.Seq("http://localhost:5341");
                }
            });
        }
    }
}
