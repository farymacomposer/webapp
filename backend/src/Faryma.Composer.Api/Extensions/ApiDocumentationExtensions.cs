using Saunter;

namespace Faryma.Composer.Api.Extensions
{
    public static class ApiDocumentationExtensions
    {
        public static void UseApiDocumentation(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapAsyncApiDocuments();
                app.MapAsyncApiUi();

                app.UseSwagger(x => x.RouteTemplate = "api/swagger/{documentname}/swagger.json");
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/api/swagger/v1/swagger.json", $"{app.Environment.ApplicationName} API v1");
                    options.RoutePrefix = "api/swagger";
                });
            }
        }
    }
}