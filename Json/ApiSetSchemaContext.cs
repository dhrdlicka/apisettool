namespace ApiSetTool.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApiSetSchema))]
[JsonSerializable(typeof(ApiSetNamespaceEntry))]
[JsonSerializable(typeof(ApiSetValueEntry))]
internal partial class ApiSetSchemaContext : JsonSerializerContext
{
}