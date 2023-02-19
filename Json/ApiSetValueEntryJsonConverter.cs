namespace ApiSetTool.Json;

public class ApiSetValueEntryJsonConverter : JsonConverter<ApiSetValueEntry>
{
    public override bool HandleNull => true;

    public override ApiSetValueEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();

            if (value is null)
                throw new JsonException();

            return new ApiSetValueEntry()
            {
                Value = value
            };
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            reader.Read();

            uint flags = 0;
            string? value = null;

            bool hasValue = false;

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "flags":
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException();
                        flags = reader.GetUInt32();
                        break;

                    case "value":
                        if (reader.TokenType == JsonTokenType.String)
                            value = reader.GetString();
                        else if (reader.TokenType != JsonTokenType.Null)
                            throw new JsonException();
                        hasValue = true;
                        break;
                }

                reader.Read();
            }

            if (hasValue)
                return new()
                {
                    Flags = flags,
                    Value = value ?? ""
                };
            else throw new JsonException();
        }
        else
            throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, ApiSetValueEntry value, JsonSerializerOptions options)
    {
        if (value.Flags != 0)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("flags");
            writer.WriteNumberValue(value.Flags);

            writer.WritePropertyName("value");
        }

        if (!string.IsNullOrEmpty(value.Value))
            writer.WriteStringValue(value.Value);
        else
            writer.WriteNullValue();

        if (value.Flags != 0)
        {
            writer.WriteEndObject();
        }
    }
}
