namespace ApiSetTool.Json;

public class ApiSetNamespaceEntryJsonConverter : JsonConverter<ApiSetNamespaceEntry>
{
    public override bool HandleNull => true;

    public override ApiSetNamespaceEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ApiSetNamespaceEntryFlags flags = 0;
        Dictionary<string, ApiSetValueEntry>? otherValues = null;
        ApiSetValueEntry? defaultValue = null;

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "sealed":
                        if (reader.TokenType is not JsonTokenType.True or JsonTokenType.False)
                            throw new JsonException();
                        if (reader.GetBoolean())
                            flags |= ApiSetNamespaceEntryFlags.Sealed;
                        break;

                    case "extension":
                        if (reader.TokenType is not JsonTokenType.True or JsonTokenType.False)
                            throw new JsonException();
                        if (reader.GetBoolean())
                            flags |= ApiSetNamespaceEntryFlags.Extension;
                        break;

                    case "flags":
                        if (reader.TokenType != JsonTokenType.Number)
                            throw new JsonException();
                        flags |= (ApiSetNamespaceEntryFlags)reader.GetUInt32();
                        break;

                    case "value":
#pragma warning disable IL3050,IL2026
                        defaultValue = JsonSerializer.Deserialize<ApiSetValueEntry>(ref reader, options);
#pragma warning restore
                        break;

                    case "others":
#pragma warning disable IL3050,IL2026
                        otherValues = JsonSerializer.Deserialize<Dictionary<string, ApiSetValueEntry>>(ref reader, options);
#pragma warning restore
                        break;
                }

                reader.Read();
            }
        }
        else
        {
#pragma warning disable IL3050,IL2026
            defaultValue = JsonSerializer.Deserialize<ApiSetValueEntry>(ref reader, options);
#pragma warning restore
        }

        return new()
        {
            Flags = flags,
            DefaultValue = defaultValue ?? throw new JsonException(),
            OtherValues = otherValues ?? new()
        };
    }

    public override void Write(Utf8JsonWriter writer, ApiSetNamespaceEntry value, JsonSerializerOptions options)
    {
        if (value.Flags != 0 || value.OtherValues.Count > 0)
        {
            writer.WriteStartObject();

            if (value.Flags.HasFlag(ApiSetNamespaceEntryFlags.Sealed))
            {
                writer.WritePropertyName("sealed");
                writer.WriteBooleanValue(value.Flags.HasFlag(ApiSetNamespaceEntryFlags.Sealed));
            }

            if (value.Flags.HasFlag(ApiSetNamespaceEntryFlags.Extension))
            {
                writer.WritePropertyName("extension");
                writer.WriteBooleanValue(value.Flags.HasFlag(ApiSetNamespaceEntryFlags.Extension));
            }

            if ((value.Flags & ~(ApiSetNamespaceEntryFlags.Sealed | ApiSetNamespaceEntryFlags.Extension)) != 0)
            {
                writer.WritePropertyName("flags");
                writer.WriteNumberValue((uint)(value.Flags & ~(ApiSetNamespaceEntryFlags.Sealed | ApiSetNamespaceEntryFlags.Extension)));
            }

            writer.WritePropertyName("value");
        }

        if (value.IsEnabled)
#pragma warning disable IL3050,IL2026
            JsonSerializer.Serialize(writer, value.DefaultValue, options);
#pragma warning restore
        else
            writer.WriteNullValue();

        if (value.Flags != 0 || value.OtherValues.Count > 0)
        {
            if (value.OtherValues.Count > 0)
            {
                writer.WritePropertyName("others");
#pragma warning disable IL3050,IL2026
                JsonSerializer.Serialize(writer, value.OtherValues, options);
#pragma warning restore
            }
            writer.WriteEndObject();
        }
    }
}
