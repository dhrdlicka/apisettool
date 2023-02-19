namespace ApiSetTool.PortableExecutable;

public class DosExecutableHeader
{
    public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D; // MZ

    // Magic number
    public ushort Magic { get; init; }

    // Bytes on last page of file
    public ushort CBlp { get; init; }

    // Pages in file
    public ushort CP { get; init; }

    // Relocations
    public ushort CRlc { get; init; }

    // Size of header in paragraphs
    public ushort CParHdr { get; init; }

    // Minimum extra paragraphs needed
    public ushort MinAlloc { get; init; }

    // Maximum extra paragraphs needed
    public ushort MaxAlloc { get; init; }

    // Initial (relative) SS value
    public ushort SS { get; init; }

    // Initial SP value
    public ushort SP { get; init; }

    // Checksum
    public ushort Csum { get; init; }

    // Initial IP value
    public ushort IP { get; init; }

    // Initial (relative) CS value
    public ushort CS { get; init; }

    // File address of relocation table
    public ushort LfaRlc { get; init; }

    // Overlay number
    public ushort OvNo { get; init; }

    // Reserved words
    public byte[] Res { get; init; } = new byte[8];

    // OEM identifier (for e_oeminfo)
    public ushort OemId { get; init; }

    // OEM information; e_oemid specific
    public ushort OemInfo { get; init; }

    // Reserved words
    public byte[] Res2 { get; init; } = new byte[20];

    // File address of new exe header
    public uint LfaNew { get; init; }

    public DosExecutableHeader(ReadOnlySpan<Byte> bytes)
    {
        Magic = BinaryPrimitives.ReadUInt16LittleEndian(bytes[0..2]);
        CBlp = BinaryPrimitives.ReadUInt16LittleEndian(bytes[2..4]);
        CP = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..6]);
        CRlc = BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]);
        CParHdr = BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]);
        MinAlloc = BinaryPrimitives.ReadUInt16LittleEndian(bytes[10..12]);
        MaxAlloc = BinaryPrimitives.ReadUInt16LittleEndian(bytes[12..14]);
        SS = BinaryPrimitives.ReadUInt16LittleEndian(bytes[14..16]);
        SP = BinaryPrimitives.ReadUInt16LittleEndian(bytes[16..18]);
        Csum = BinaryPrimitives.ReadUInt16LittleEndian(bytes[18..20]);
        IP = BinaryPrimitives.ReadUInt16LittleEndian(bytes[20..22]);
        CS = BinaryPrimitives.ReadUInt16LittleEndian(bytes[22..24]);
        LfaRlc = BinaryPrimitives.ReadUInt16LittleEndian(bytes[24..26]);
        OvNo = BinaryPrimitives.ReadUInt16LittleEndian(bytes[26..28]);
        Res = bytes[28..36].ToArray();
        OemId = BinaryPrimitives.ReadUInt16LittleEndian(bytes[36..38]);
        OemInfo = BinaryPrimitives.ReadUInt16LittleEndian(bytes[38..40]);
        Res2 = bytes[40..60].ToArray();
        LfaNew = BinaryPrimitives.ReadUInt32LittleEndian(bytes[60..64]);

        if (Magic is not IMAGE_DOS_SIGNATURE)
            throw new InvalidDataException();
    }
}
