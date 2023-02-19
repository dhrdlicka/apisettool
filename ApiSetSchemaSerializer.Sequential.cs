using System.Runtime.InteropServices;

namespace ApiSetTool;

public sealed partial class ApiSetSchemaSerializer
{
    private void SerializeSequential(Stream stream, ApiSetSchema schema)
    {
        var startPosition = stream.Position;

        var header = new NamespaceArray()
        {
            Version = schema.Version,
            HashFactor = schema.HashFactor,
            Flags = (uint)schema.Flags,
            Count = schema.Entries.Count,
            EntryOffset = Marshal.SizeOf<NamespaceArray>()
        };

        var namespaces = new List<NamespaceEntry>(header.Count);
        var values = new List<ValueEntry>();

        var valuesOffset = Marshal.SizeOf<NamespaceArray>() + header.Count * Marshal.SizeOf<NamespaceEntry>();

        var stringMapLength = 0;
        var stringMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [""] = 0
        };

        var hashMap = new Dictionary<uint, int>();

        foreach (var (nsEntryName, nsEntry) in schema.Entries)
        {
            if (!stringMap.ContainsKey(nsEntryName))
            {
                stringMap.Add(nsEntryName, stringMapLength);
                stringMapLength += Encoding.Unicode.GetByteCount(nsEntryName);
            }

            var hashedName = nsEntryName.Remove(nsEntryName.LastIndexOf('-')).ToLowerInvariant();
            hashMap.Add(HashName(hashedName, header.HashFactor), namespaces.Count);

            namespaces.Add(new()
            {
                Flags = (uint)nsEntry.Flags,
                NameOffset = stringMap[nsEntryName],
                NameLength = Encoding.Unicode.GetByteCount(nsEntryName),
                HashedLength = Encoding.Unicode.GetByteCount(hashedName),
                ValueOffset = valuesOffset + values.Count * Marshal.SizeOf<ValueEntry>(),
                ValueCount = nsEntry.OtherValues.Count + 1
            });

            foreach (var (vName, vEntry) in nsEntry.OtherValues.Prepend(new("", nsEntry.DefaultValue)))
            {
                if (!stringMap.ContainsKey(vName))
                {
                    stringMap.Add(vName, stringMapLength);
                    stringMapLength += Encoding.Unicode.GetByteCount(vName);
                }

                if (!stringMap.ContainsKey(vEntry.Value))
                {
                    stringMap.Add(vEntry.Value, stringMapLength);
                    stringMapLength += Encoding.Unicode.GetByteCount(vEntry.Value);
                }

                values.Add(new()
                {
                    Flags = (uint)vEntry.Flags,
                    NameLength = Encoding.Unicode.GetByteCount(vName),
                    NameOffset = stringMap[vName],
                    ValueLength = Encoding.Unicode.GetByteCount(vEntry.Value),
                    ValueOffset = stringMap[vEntry.Value]
                });
            }
        }

        var stringOffset = valuesOffset + values.Count * Marshal.SizeOf<ValueEntry>();

        header.HashOffset = Align(stringOffset + stringMapLength, 4);
        header.Size = header.HashOffset + header.Count * 8;

        var buffer = new byte[Marshal.SizeOf<NamespaceArray>()].AsSpan();
        header.WriteBytes(buffer);
        stream.Write(buffer);

        stream.Position = startPosition + header.EntryOffset;

        buffer = new byte[Marshal.SizeOf<NamespaceEntry>()].AsSpan();
        for (var i = 0; i < namespaces.Count; i++)
        {
            var ns = namespaces[i];

            ns.NameOffset += stringOffset;

            namespaces[i] = ns;

            ns.WriteBytes(buffer);
            stream.Write(buffer);
        }

        buffer = new byte[Marshal.SizeOf<ValueEntry>()].AsSpan();
        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];

            if (v.NameOffset > 0)
                v.NameOffset += stringOffset;

            if (v.ValueOffset > 0)
                v.ValueOffset += stringOffset;

            values[i] = v;

            v.WriteBytes(buffer);
            stream.Write(buffer);
        }

        foreach (var (name, _) in stringMap.OrderBy(x => x.Value))
        {
            stream.Write(Encoding.Unicode.GetBytes(name));
        }

        stream.Position = startPosition + header.HashOffset;
        buffer = new byte[8].AsSpan();
        foreach (var (k, v) in hashMap.OrderBy(x => x.Key))
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[0..4], k);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..8], v);

            stream.Write(buffer);
        }

        stream.Flush();
    }
}
