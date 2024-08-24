// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AxToolkit.Json;

public class JsonDateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DateOnly.ParseExact(reader.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateOnly dateTimeValue, JsonSerializerOptions options) =>
            writer.WriteStringValue(dateTimeValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
