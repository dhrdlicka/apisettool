using System.Runtime.InteropServices;

namespace ApiSetTool;

public sealed partial class ApiSetSchemaSerializer
{
    private void SerializeAuthentic(Stream stream, ApiSetSchema schema)
    {
        var header = new NamespaceArray()
        {
            Version = schema.Version,
            HashFactor = schema.HashFactor,
            Flags = (uint)schema.Flags,
            Count = schema.Entries.Count,
            EntryOffset = Marshal.SizeOf<NamespaceArray>()
        };

        var valuesOffset = Marshal.SizeOf<NamespaceArray>() + header.Count * Marshal.SizeOf<NamespaceEntry>();
        var currentOffset = valuesOffset;

        var stringMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [""] = 0
        };

        var hashMap = new Dictionary<uint, int>();

        var namespaces = new List<NamespaceEntry>(header.Count);
        var data = new List<(string? name, List<ValueEntry> value, List<string> strings)>();
        foreach (var (nsName, nsEntry) in schema.Entries)
        {
            string? name = null;

            if (!stringMap.ContainsKey(nsName))
            {
                stringMap.Add(nsName, currentOffset);
                name = nsName;

                currentOffset = Align(currentOffset + Encoding.Unicode.GetByteCount(nsName), 4);
            }

            currentOffset = Align(currentOffset, 4);

            var hashedName = nsName.Remove(nsName.LastIndexOf('-')).ToLowerInvariant();
            hashMap.Add(HashName(hashedName, header.HashFactor), namespaces.Count);

            namespaces.Add(new()
            {
                Flags = (uint)nsEntry.Flags,
                NameOffset = stringMap[nsName],
                NameLength = Encoding.Unicode.GetByteCount(nsName),
                HashedLength = Encoding.Unicode.GetByteCount(hashedName),
                ValueOffset = currentOffset,
                ValueCount = nsEntry.OtherValues.Count + 1
            });

            currentOffset += Marshal.SizeOf<ValueEntry>() * (nsEntry.OtherValues.Count + 1);

            var values = new List<ValueEntry>();
            var strings = new List<string>();

            foreach (var (key, value) in nsEntry.OtherValues.Prepend(new("", nsEntry.DefaultValue)))
            {
                if (!stringMap.ContainsKey(key))
                {
                    stringMap.Add(key, currentOffset);
                    strings.Add(key);

                    currentOffset = Align(currentOffset + Encoding.Unicode.GetByteCount(key), 4);
                }

                if (!stringMap.ContainsKey(value.Value))
                {
                    stringMap.Add(value.Value, currentOffset);
                    strings.Add(value.Value);

                    currentOffset = Align(currentOffset + Encoding.Unicode.GetByteCount(value.Value), 4);
                }

                values.Add(new()
                {
                    Flags = (uint)value.Flags,
                    NameLength = Encoding.Unicode.GetByteCount(key),
                    NameOffset = stringMap[key],
                    ValueLength = Encoding.Unicode.GetByteCount(value.Value),
                    ValueOffset = stringMap[value.Value]
                });
            }

            data.Add((name, values, strings));
        }

        header.HashOffset = Align((int)currentOffset, 4);
        header.Size = header.HashOffset + header.Count * 8;

        var buffer = new byte[Marshal.SizeOf<NamespaceArray>()].AsSpan();
        header.WriteBytes(buffer);
        stream.Write(buffer);

        stream.Position = header.EntryOffset;

        buffer = new byte[Marshal.SizeOf<NamespaceEntry>()].AsSpan();
        foreach (var ns in namespaces)
        {
            ns.WriteBytes(buffer);
            stream.Write(buffer);
        }

        buffer = new byte[Marshal.SizeOf<ValueEntry>()].AsSpan();
        foreach (var (name, values, strings) in data)
        {
            stream.Position = Align((int)stream.Position, 4);

            if (name is not null)
                stream.Write(Encoding.Unicode.GetBytes(name));

            stream.Position = Align((int)stream.Position, 4);

            foreach (var value in values)
            {
                value.WriteBytes(buffer);
                stream.Write(buffer);
            }

            foreach (var str in strings)
            {
                stream.Write(Encoding.Unicode.GetBytes(str));
                stream.Position = Align((int)stream.Position, 4);
            }
        }

        stream.Position = header.HashOffset;
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
