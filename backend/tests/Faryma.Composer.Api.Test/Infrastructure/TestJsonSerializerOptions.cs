using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    public static class TestJsonSerializerOptions
    {
        public static JsonSerializerOptions Web { get; } = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };
    }
}
