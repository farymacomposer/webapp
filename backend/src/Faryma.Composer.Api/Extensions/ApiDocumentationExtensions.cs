using Saunter;
using Scalar.AspNetCore;

namespace Faryma.Composer.Api.Extensions
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

                // SignalR
                app.MapAsyncApiDocuments();
                app.MapAsyncApiUi();
            }
        }
    }
}
