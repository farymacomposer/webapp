using Saunter;
using Scalar.AspNetCore;

namespace Faryma.Composer.Api.Common.Extensions
{
    public static class ApiDocumentationExtensions
    {
        private static readonly PathString[] _apiDocumentationPaths =
        [
            new("/asyncapi"),
            new("/openapi"),
            new("/redoc"),
            new("/scalar"),
            new("/swagger"),
        ];

        public static void UseApiDocumentation(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // REST
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.UseSwaggerUI(options =>
                {
                    options.RoutePrefix = "swagger";
                    options.DocumentTitle = "Faryma Composer API";
                    options.SwaggerEndpoint("/openapi/v1.json", "Faryma Composer API v1");
                });
                app.UseReDoc(options =>
                {
                    options.RoutePrefix = "redoc";
                    options.DocumentTitle = "Faryma Composer API";
                    options.SpecUrl("/openapi/v1.json");
                });

                // SignalR
                app.MapAsyncApiDocuments();
                app.MapAsyncApiUi();
            }
        }

        public static void UseHttpsRedirectionExceptApiDocumentation(this WebApplication app)
        {
            app.UseWhen(
                context => !app.Environment.IsDevelopment() || !IsApiDocumentationRequest(context.Request),
                branch => branch.UseHttpsRedirection());
        }

        private static bool IsApiDocumentationRequest(HttpRequest request)
        {
            return _apiDocumentationPaths.Any(path =>
                request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}
