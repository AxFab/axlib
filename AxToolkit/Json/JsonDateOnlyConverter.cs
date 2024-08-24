using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AxToolkit.Json
{
    public class JsonDateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                DateOnly.ParseExact(reader.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateOnly dateTimeValue, JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
