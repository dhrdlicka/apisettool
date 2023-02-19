namespace ApiSetTool.Json;

public class ApiSetSchemaJsonConverter : JsonConverter<ApiSetSchema>
{
    public override ApiSetSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        reader.Read();

        int? version = null, hashFactor = null;
        ApiSetSchemaFlags flags = 0;

        Dictionary<string, ApiSetNamespaceEntry>? entries = null;

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLowerInvariant())
            {
                case "version":
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException();
                    version = reader.GetInt32();
                    break;

                case "hashfactor":
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException();
                    hashFactor = reader.GetInt32();
                    break;

                case "sealed":
                    if (reader.TokenType is not JsonTokenType.True or JsonTokenType.False)
                        throw new JsonException();
                    if (reader.GetBoolean())
                        flags |= ApiSetSchemaFlags.Sealed;
                    break;

                case "hostextension":
                    if (reader.TokenType is not JsonTokenType.True or JsonTokenType.False)
                        throw new JsonException();
                    if (reader.GetBoolean())
                        flags |= ApiSetSchemaFlags.HostExtension;
                    break;

                case "flags":
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException();
                    flags |= (ApiSetSchemaFlags)reader.GetUInt32();
                    break;

                case "entries":
#pragma warning disable IL3050,IL2026
                    entries = JsonSerializer.Deserialize<Dictionary<string, ApiSetNamespaceEntry>>(ref reader, options) ?? throw new JsonException();
#pragma warning restore
                    break;
            }

            reader.Read();
        }

        return new()
        {
            Version = version ?? throw new JsonException(),
            HashFactor = hashFactor ?? 31,
            Entries = entries ?? throw new JsonException(),
            Flags = flags
        };
    }

    public override void Write(Utf8JsonWriter writer, ApiSetSchema value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("version");
        writer.WriteNumberValue(value.Version);

        if (value.Flags != 0)
        {
            if (value.Flags.HasFlag(ApiSetSchemaFlags.Sealed))
            {
                writer.WritePropertyName("sealed");
                writer.WriteBooleanValue(value.Flags.HasFlag(ApiSetSchemaFlags.Sealed));
            }

            if (value.Flags.HasFlag(ApiSetSchemaFlags.HostExtension))
            {
                writer.WritePropertyName("hostExtension");
                writer.WriteBooleanValue(value.Flags.HasFlag(ApiSetSchemaFlags.HostExtension));
            }

            if ((value.Flags & ~(ApiSetSchemaFlags.Sealed | ApiSetSchemaFlags.HostExtension)) != 0)
            {
                writer.WritePropertyName("flags");
                writer.WriteNumberValue((uint)(value.Flags & ~(ApiSetSchemaFlags.Sealed | ApiSetSchemaFlags.HostExtension)));
            }
        }

        writer.WritePropertyName("hashFactor");
        writer.WriteNumberValue(value.HashFactor);

        writer.WritePropertyName("entries");
#pragma warning disable IL3050,IL2026
        JsonSerializer.Serialize(writer, value.Entries, options);
#pragma warning restore

        writer.WriteEndObject();
    }
}
