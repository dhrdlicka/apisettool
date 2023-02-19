namespace ApiSetTool;

[Flags]
public enum ApiSetSchemaFlags : uint
{
    Sealed = 0x1,
    HostExtension = 0x2
}

public class ApiSetSchema
{
    public int Version { get; init; } = 6;
    public ApiSetSchemaFlags Flags { get; set; }
    public int HashFactor { get; set; } = 31;

    public Dictionary<string, ApiSetNamespaceEntry> Entries { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
}
