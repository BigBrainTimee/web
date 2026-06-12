using System.Text.Json;
using System.Text.Json.Serialization;

namespace TravelService.Json;

public sealed class NullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("activityTime must be a string or null.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TimeOnly.TryParse(value, out var time))
        {
            return time;
        }

        throw new JsonException($"Invalid time format: {value}");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString("HH:mm:ss"));
    }
}
