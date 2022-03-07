using System.Text.Json;

namespace BiblioMit.Extensions
{
    public static class JsonCase
    {
        public static JsonSerializerOptions Camel => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
        public static JsonSerializerOptions CamelMin => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };
        public static JsonSerializerOptions Snake => new()
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            WriteIndented = true,
        };
    }
    public sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToSnakeCase();
    }
}
