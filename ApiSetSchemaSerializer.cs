using System.Runtime.InteropServices;

namespace ApiSetTool;

public sealed partial class ApiSetSchemaSerializer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NamespaceArray
    {
        public int Version { get; set; }
        public int Size { get; set; }
        public uint Flags { get; set; }
        public int Count { get; set; }
        public int EntryOffset { get; set; }
        public int HashOffset { get; set; }
        public int HashFactor { get; set; }

        public NamespaceArray(ReadOnlySpan<byte> bytes)
        {
            Version = BinaryPrimitives.ReadInt32LittleEndian(bytes[0..4]);
            Size = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
            Flags = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..12]);
            Count = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..16]);
            EntryOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[16..20]);
            HashOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[20..24]);
            HashFactor = BinaryPrimitives.ReadInt32LittleEndian(bytes[24..28]);
        }

        public void WriteBytes(Span<byte> buffer)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer[0..4], Version);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], Size);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[8..12], Flags);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[12..16], Count);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[16..20], EntryOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[20..24], HashOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[24..28], HashFactor);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NamespaceEntry
    {
        public uint Flags { get; set; }
        public int NameOffset { get; set; }
        public int NameLength { get; set; }
        public int HashedLength { get; set; }
        public int ValueOffset { get; set; }
        public int ValueCount { get; set; }

        public NamespaceEntry(ReadOnlySpan<byte> bytes)
        {
            Flags = BinaryPrimitives.ReadUInt32LittleEndian(bytes[0..4]);
            NameOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
            NameLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..12]);
            HashedLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..16]);
            ValueOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[16..20]);
            ValueCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[20..24]);
        }

        public void WriteBytes(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[0..4], Flags);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], NameOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..12], NameLength);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[12..16], HashedLength);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[16..20], ValueOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[20..24], ValueCount);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ValueEntry
    {
        public uint Flags { get; set; }
        public int NameOffset { get; set; }
        public int NameLength { get; set; }
        public int ValueOffset { get; set; }
        public int ValueLength { get; set; }

        public ValueEntry(ReadOnlySpan<byte> bytes)
        {
            Flags = BinaryPrimitives.ReadUInt32LittleEndian(bytes[0..4]);
            NameOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
            NameLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[8..12]);
            ValueOffset = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..16]);
            ValueLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[16..20]);
        }

        public void WriteBytes(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[0..4], Flags);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], NameOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..12], NameLength);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[12..16], ValueOffset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[16..20], ValueLength);
        }
    }

    public ApiSetSchema Deserialize(ReadOnlySpan<byte> bytes)
    {
        var header = new NamespaceArray(bytes[0..]);

        if (header.Size > bytes.Length)
            throw new InvalidDataException();

        if (header.Version != 6)
            throw new NotSupportedException();

        var schema = new ApiSetSchema()
        {
            Version = header.Version,
            Flags = (ApiSetSchemaFlags)header.Flags,
            HashFactor = header.HashFactor
        };

        for (var i = 0; i < header.Count; i++)
        {
            var nsStruct = new NamespaceEntry(bytes[(header.EntryOffset + i * Marshal.SizeOf<NamespaceEntry>())..]);
            var nsName = Encoding.Unicode.GetString(bytes.Slice(nsStruct.NameOffset, nsStruct.NameLength));

            var ns = new ApiSetNamespaceEntry()
            {
                Flags = (ApiSetNamespaceEntryFlags)nsStruct.Flags,
                DefaultValue = null! // using `null` as a marker whether the value has been initialized.
                                     // technically an API set with no default value should be invalid
                                     // so this should always get initialized
            };

            schema.Entries.Add(nsName, ns);

            for (var j = 0; j < nsStruct.ValueCount; j++)
            {
                var valueStruct = new ValueEntry(bytes[(nsStruct.ValueOffset + j * Marshal.SizeOf<ValueEntry>())..]);
                var name = Encoding.Unicode.GetString(bytes.Slice(valueStruct.NameOffset, valueStruct.NameLength));

                var value = new ApiSetValueEntry()
                {
                    Flags = valueStruct.Flags,
                    Value = Encoding.Unicode.GetString(bytes.Slice(valueStruct.ValueOffset, valueStruct.ValueLength))
                };

                if (string.IsNullOrEmpty(name))
                {
                    if (ns.DefaultValue is not null)
                        throw new InvalidDataException();

                    ns.DefaultValue = value;
                }
                else
                {
                    ns.OtherValues.Add(name, value);
                }
            }

            if (ns.DefaultValue is null)
            {
                throw new InvalidDataException();
            }
        }

        return schema;
    }

    public void Serialize(Stream stream, ApiSetSchema schema, ApiSetSchemaFormat format)
    {
        switch (format)
        {
            case ApiSetSchemaFormat.Sequential:
                SerializeSequential(stream, schema);
                return;
            case ApiSetSchemaFormat.Authentic:
                SerializeAuthentic(stream, schema);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(format));
        }
    }

    uint HashName(string name, int hashFactor)
    {
        var hash = 0u;

        foreach (var c in name)
        {
            hash *= (uint)hashFactor;
            hash += c;
        }

        return hash;
    }

    int Align(int value, int alignment)
        => (value + alignment - 1) & ~(alignment - 1);
}
