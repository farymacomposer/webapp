using Saunter;
using Scalar.AspNetCore;

namespace Faryma.Composer.Api.Common.Extensions
{
    public static class ApiDocumentationExtensions
    {
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
    }
}
