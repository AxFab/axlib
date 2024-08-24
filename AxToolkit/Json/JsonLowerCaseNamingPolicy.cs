using System.Text.Json;

namespace AxToolkit.Json
{
    public class JsonLowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => name[0..1].ToLowerInvariant() + name[1..];
    }
}
