namespace ApiSetTool.PortableExecutable;

public class PeSectionHeader
{
    public PortableExecutable Executable { get; }

    public string Name { get; init; }
    public uint VirtualSize { get; init; }
    public uint VirtualAddress { get; init; }
    public uint SizeOfRawData { get; init; }
    public uint PointerToRawData { get; init; }
    public uint PointerToRelocations { get; init; }
    public uint PointerToLineNumbers { get; init; }
    public ushort NumberOfRelocations { get; init; }
    public ushort NumberOfLineNumbers { get; init; }
    public SectionCharacteristics Characteristics { get; init; }

    public PeSectionHeader(PortableExecutable pe, ReadOnlySpan<byte> bytes)
    {
        Executable = pe;

        Name = Encoding.UTF8.GetString(bytes[0..8]).Replace("\0", string.Empty).TrimEnd();

        VirtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..12]);
        VirtualAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..16]);

        SizeOfRawData = BinaryPrimitives.ReadUInt32LittleEndian(bytes[16..20]);
        PointerToRawData = BinaryPrimitives.ReadUInt32LittleEndian(bytes[20..24]);

        PointerToRelocations = BinaryPrimitives.ReadUInt32LittleEndian(bytes[24..28]);
        PointerToLineNumbers = BinaryPrimitives.ReadUInt32LittleEndian(bytes[28..32]);

        NumberOfRelocations = BinaryPrimitives.ReadUInt16LittleEndian(bytes[32..34]);
        NumberOfLineNumbers = BinaryPrimitives.ReadUInt16LittleEndian(bytes[34..36]);

        Characteristics = (SectionCharacteristics)BinaryPrimitives.ReadUInt32LittleEndian(bytes[36..40]);
    }

    public Stream GetStream()
        => new PeSectionStream(this);
}

[Flags]
public enum SectionCharacteristics : uint
{

    // Reserved.
    TypeNoPad = 0x00000008,


    // Section contains code.
    CntCode = 0x00000020,

    // Section contains initialized data.
    CntInitializedData = 0x00000040,

    // Section contains uninitialized data.
    CntUninitializedData = 0x00000080,


    // Reserved.
    LnkOther = 0x00000100,

    // Section contains comments or some other type of information.
    LnkInfo = 0x00000200,

    // Section contents will not become part of image.
    LnkRemove = 0x00000800,

    // Section contents comdat.
    LnkComdat = 0x00001000,

    // Reset speculative exceptions handling bits in the TLB entries for this section.
    NoDeferSpecExe = 0x00004000,

    // Section content can be accessed relative to GP
    GpRel = 0x00008000,

    MemFarData = 0x00008000,
    MemPurgeable = 0x00020000,
    Mem16Bit = 0x00020000,
    MemLocked = 0x00040000,
    MemPreload = 0x00080000,

    Align1Bytes = 0x00100000,
    Align2Bytes = 0x00200000,
    Align4Bytes = 0x00300000,
    Align8Bytes = 0x00400000,
    Align16Bytes = 0x00500000,  // Default alignment if no others are specified.
    Align32Bytes = 0x00600000,
    Align64Bytes = 0x00700000,
    Align128Bytes = 0x00800000,
    Align256Bytes = 0x00900000,
    Align512Bytes = 0x00A00000,
    Align1024Bytes = 0x00B00000,
    Align2048Bytes = 0x00C00000,
    Align4096Bytes = 0x00D00000,
    Align8192Bytes = 0x00E00000,

    AlignMask = 0x00F00000,


    // Section contains extended relocations.
    LnkNRelocOvfl = 0x01000000,

    // Section can be discarded.
    MemDiscardable = 0x02000000,

    // Section is not cachable.
    MemNotCached = 0x04000000,

    // Section is not pageable.
    MemNotPaged = 0x08000000,

    // Section is shareable.
    MemShared = 0x10000000,

    // Section is executable.
    MemExecute = 0x20000000,

    // Section is readable.
    MemRead = 0x40000000,

    // Section is writeable.
    MemWrite = 0x80000000,
}
