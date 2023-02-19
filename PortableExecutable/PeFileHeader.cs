namespace ApiSetTool.PortableExecutable;

public class PeFileHeader
{
    public Machine Machine { get; init; }
    public ushort NumberOfSections { get; init; }
    public uint TimeDateStamp { get; init; }
    public uint PointerToSymbolTable { get; init; }
    public uint NumberOfSymbols { get; init; }
    public ushort SizeOfOptionalHeader { get; init; }
    public Characteristics Characteristics { get; init; }

    public PeFileHeader(ReadOnlySpan<byte> bytes)
    {
        Machine = (Machine)BinaryPrimitives.ReadUInt16LittleEndian(bytes[0..2]);
        NumberOfSections = BinaryPrimitives.ReadUInt16LittleEndian(bytes[2..4]);
        TimeDateStamp = BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..8]);
        PointerToSymbolTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..12]);
        NumberOfSymbols = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..16]);
        SizeOfOptionalHeader = BinaryPrimitives.ReadUInt16LittleEndian(bytes[16..18]);
        Characteristics = (Characteristics)BinaryPrimitives.ReadUInt16LittleEndian(bytes[18..20]);

        if (Characteristics.HasFlag(Characteristics.BytesReversedHi) ||
            Characteristics.HasFlag(Characteristics.BytesReversedLo))
        {
            throw new NotSupportedException();
        }
    }
}

[Flags]
public enum Characteristics : ushort
{

    // Relocation info stripped from file.
    RelocsStripped = 0x0001,

    // File is executable  (i.e. no unresolved external references).
    ExecutableImage = 0x0002,

    // Line nunbers stripped from file.
    LineNumsStripped = 0x0004,

    // Local symbols stripped from file.
    LocalSymsStripped = 0x0008,

    // Aggressively trim working set
    AggressiveWsTrim = 0x0010,

    // App can handle >2gb addresses
    LargeAddressAware = 0x0020,

    // Bytes of machine word are reversed.
    BytesReversedLo = 0x0080,

    // 32 bit word machine.
    Is32BitMachine = 0x0100,

    // Debugging info stripped from file in .DBG file
    DebugStripped = 0x0200,

    // If Image is on removable media, copy and run from the swap file.
    RemovableRunFromSwap = 0x0400,

    // If Image is on Net, copy and run from the swap file.
    NetRunFromSwap = 0x0800,

    // System File.
    System = 0x1000,

    // File is a DLL.
    Dll = 0x2000,

    // File should only be run on a UP machine
    UpSystemOnly = 0x4000,

    // Bytes of machine word are reversed.
    BytesReversedHi = 0x8000,
}

public enum Machine : ushort
{
    Unknown = 0,

    // Useful for indicating we want to interact with the host and not a WoW guest.
    TargetHost = 0x0001,

    // Intel 386.
    I386 = 0x014c,

    // MIPS big-endian
    R3000BigEndian = 0x160,

    // MIPS little-endian
    R3000 = 0x0162,

    // MIPS little-endian
    R4000 = 0x0166,

    // MIPS little-endian
    R10000 = 0x0168,

    // MIPS little-endian WCE v2
    WCeMipsV2 = 0x0169,

    // Alpha_AXP
    Alpha = 0x0184,

    // SH3 little-endian
    Sh3 = 0x01a2,

    Sh3Dsp = 0x01a3,

    // SH3E little-endian
    Sh3E = 0x01a4,

    // SH4 little-endian
    Sh4 = 0x01a6,

    // SH5
    Sh5 = 0x01a8,

    // ARM Little-Endian
    Arm = 0x01c0,

    // ARM Thumb/Thumb-2 Little-Endian
    Thumb = 0x01c2,

    // ARM Thumb-2 Little-Endian
    ArmNt = 0x01c4,

    Am33 = 0x01d3,

    // IBM PowerPC Little-Endian
    PowerPc = 0x01F0,

    PowerPcFp = 0x01f1,

    // Intel 64
    Ia64 = 0x0200,

    // MIPS
    Mips16 = 0x0266,

    // ALPHA64
    Alpha64 = 0x0284,

    // MIPS
    MipsFpu = 0x0366,

    // MIPS
    MipsFpu16 = 0x0466,

    Axp64 = Alpha64,

    // Infineon
    Tricore = 0x0520,

    Cef = 0x0CEF,

    // EFI Byte Code
    Ebc = 0x0EBC,

    // AMD64 (K8)
    Amd64 = 0x8664,

    // M32R little-endian
    M32R = 0x9041,

    // ARM64 Little-Endian
    Arm64 = 0xAA64,

    Cee = 0xC0EE,
}
