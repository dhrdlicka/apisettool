namespace ApiSetTool;

[Flags]
public enum ApiSetNamespaceEntryFlags : uint
{
    Sealed = 0x1,
    Extension = 0x2
}

public class ApiSetNamespaceEntry
{
    public ApiSetNamespaceEntryFlags Flags { get; set; }

    public ApiSetValueEntry DefaultValue { get; set; } = new();
    public Dictionary<string, ApiSetValueEntry> OtherValues { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    public bool IsEnabled => !DefaultValue.IsEmpty;
}
