namespace ApiSetTool;

public class ApiSetValueEntry
{
    public uint Flags { get; set; }
    public string Value { get; set; } = string.Empty;

    public bool IsEmpty => Flags == 0 && string.IsNullOrEmpty(Value);
}
