namespace ApiSetTool.PortableExecutable;

public class PeOptionalHeader
{
    public OptionalHeaderMagic Magic { get; init; }

    public byte MajorLinkerVersion { get; init; }
    public byte MinorLinkerVersion { get; init; }

    public uint SizeOfCode { get; init; }
    public uint SizeOfInitializedData { get; init; }
    public uint SizeOfUninitializedData { get; init; }

    public uint AddressOfEntryPoint { get; init; }

    public uint BaseOfCode { get; init; }

    public uint? BaseOfData { get; init; }

    public uint? BaseOfBss { get; init; }
    public uint? GprMask { get; init; }
    public uint[]? CprMask { get; init; }
    public uint? GpValue { get; init; }

    public ulong? ImageBase { get; init; }

    public uint? SectionAlignment { get; init; }
    public uint? FileAlignment { get; init; }

    public ushort? MajorOperatingSystemVersion { get; init; }
    public ushort? MinorOperatingSystemVersion { get; init; }

    public ushort? MajorImageVersion { get; init; }
    public ushort? MinorImageVersion { get; init; }

    public ushort? MajorSubsystemVersion { get; init; }
    public ushort? MinorSubsystemVersion { get; init; }

    public uint? Win32VersionValue { get; init; }

    public uint? SizeOfImage { get; init; }
    public uint? SizeOfHeaders { get; init; }

    public uint? CheckSum { get; init; }

    public Subsystem? Subsystem { get; init; }

    public DllCharacteristics? DllCharacteristics { get; init; }

    public ulong? SizeOfStackReserve { get; init; }
    public ulong? SizeOfStackCommit { get; init; }
    public ulong? SizeOfHeapReserve { get; init; }
    public ulong? SizeOfHeapCommit { get; init; }

    public uint? LoaderFlags { get; init; }

    public uint? NumberOfRvaAndSizes { get; init; }
    public Dictionary<DirectoryEntry, (uint VirtualAddress, uint Size)>? DataDirectory { get; init; }

    public PeOptionalHeader(ReadOnlySpan<byte> bytes)
    {
        Magic = (OptionalHeaderMagic)BinaryPrimitives.ReadUInt16LittleEndian(bytes[0..2]);

        if (Magic is not (OptionalHeaderMagic.Pe32 or OptionalHeaderMagic.Pe32Plus or OptionalHeaderMagic.Rom))
        {
            throw new InvalidDataException();
        }

        MajorLinkerVersion = bytes[2];
        MinorLinkerVersion = bytes[3];

        SizeOfCode = BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..8]);
        SizeOfInitializedData = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..12]);
        SizeOfUninitializedData = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..16]);

        AddressOfEntryPoint = BinaryPrimitives.ReadUInt32LittleEndian(bytes[16..20]);

        BaseOfCode = BinaryPrimitives.ReadUInt32LittleEndian(bytes[20..24]);

        if (Magic is OptionalHeaderMagic.Pe32Plus)
        {
            ImageBase = BinaryPrimitives.ReadUInt64LittleEndian(bytes[24..32]);
        }
        else
        {
            BaseOfData = BinaryPrimitives.ReadUInt32LittleEndian(bytes[20..24]);

            if (Magic is OptionalHeaderMagic.Rom)
            {
                BaseOfBss = BinaryPrimitives.ReadUInt32LittleEndian(bytes[28..32]);
                GprMask = BinaryPrimitives.ReadUInt32LittleEndian(bytes[32..36]);

                CprMask = new uint[4];
                for (var i = 0; i < CprMask.Length; i++)
                {
                    var cprOffset = 36 + (i * 4);
                    CprMask[i] = BinaryPrimitives.ReadUInt32LittleEndian(bytes[cprOffset..(cprOffset += 4)]);
                }

                GpValue = BinaryPrimitives.ReadUInt32LittleEndian(bytes[52..56]);

                return;
            }
            else
            {
                ImageBase = BinaryPrimitives.ReadUInt32LittleEndian(bytes[28..32]);
            }
        }

        SectionAlignment = BinaryPrimitives.ReadUInt32LittleEndian(bytes[32..36]);
        FileAlignment = BinaryPrimitives.ReadUInt32LittleEndian(bytes[36..40]);

        MajorOperatingSystemVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[40..42]);
        MinorOperatingSystemVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[42..44]);

        MajorImageVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[44..46]);
        MinorImageVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[46..48]);

        MajorSubsystemVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[48..50]);
        MinorSubsystemVersion = BinaryPrimitives.ReadUInt16LittleEndian(bytes[50..52]);

        Win32VersionValue = BinaryPrimitives.ReadUInt32LittleEndian(bytes[52..56]);

        SizeOfImage = BinaryPrimitives.ReadUInt32LittleEndian(bytes[56..60]);
        SizeOfHeaders = BinaryPrimitives.ReadUInt32LittleEndian(bytes[60..64]);

        CheckSum = BinaryPrimitives.ReadUInt32LittleEndian(bytes[64..68]);

        Subsystem = (Subsystem)BinaryPrimitives.ReadUInt16LittleEndian(bytes[68..70]);

        DllCharacteristics = (DllCharacteristics)BinaryPrimitives.ReadUInt16LittleEndian(bytes[70..72]);

        int offset;

        if (Magic is OptionalHeaderMagic.Pe32)
        {
            SizeOfStackReserve = BinaryPrimitives.ReadUInt32LittleEndian(bytes[72..76]);
            SizeOfStackCommit = BinaryPrimitives.ReadUInt32LittleEndian(bytes[76..80]);
            SizeOfHeapReserve = BinaryPrimitives.ReadUInt32LittleEndian(bytes[80..84]);
            SizeOfHeapCommit = BinaryPrimitives.ReadUInt32LittleEndian(bytes[84..88]);

            offset = 88;
        }
        else
        {
            SizeOfStackReserve = BinaryPrimitives.ReadUInt64LittleEndian(bytes[72..80]);
            SizeOfStackCommit = BinaryPrimitives.ReadUInt64LittleEndian(bytes[80..88]);
            SizeOfHeapReserve = BinaryPrimitives.ReadUInt64LittleEndian(bytes[88..96]);
            SizeOfHeapCommit = BinaryPrimitives.ReadUInt64LittleEndian(bytes[96..104]);

            offset = 104;
        }

        LoaderFlags = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..(offset += 4)]);
        NumberOfRvaAndSizes = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..(offset += 4)]);

        DataDirectory = new();
        for (var i = 0; i < NumberOfRvaAndSizes; i++)
        {
            DataDirectory[(DirectoryEntry)i] = new()
            {
                VirtualAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..(offset += 4)]),
                Size = BinaryPrimitives.ReadUInt32LittleEndian(bytes[offset..(offset += 4)])
            };
        }
    }
}

public enum OptionalHeaderMagic : ushort
{
    Pe32 = 0x10b,
    Pe32Plus = 0x20b,
    Rom = 0x107
}

public enum Subsystem : ushort
{
    // An unknown subsystem
    Unknown,

    // Device drivers and native Windows processes
    Native,

    // The Windows graphical user interface (GUI) subsystem
    WindowsGui,

    // The Windows character subsystem
    WindowsCui,

    // The OS/2 character subsystem
    Os2Cui = 5,

    // The Posix character subsystem
    PosixCui = 7,

    // Native Win9x driver
    NativeWindows,

    // Windows CE
    WindowsCeGui,

    // An Extensible Firmware Interface (EFI) application
    EfiApplication,

    // An EFI driver with boot services
    EfiBootServiceDriver,

    // An EFI driver with run-time services
    EfiRuntimeDriver,

    // An EFI ROM image
    EfiRom,

    // XBOX
    Xbox,

    // Windows boot application.
    WindowsBootApplication = 16,

    XboxCodeCatalog
}

[Flags]
public enum DllCharacteristics
{
    // Image can handle a high entropy 64-bit virtual address space.
    HighEntropyVA = 0x0020,

    // DLL can move.
    DynamicBase = 0x0040,

    // Code Integrity Image
    ForceIntegrity = 0x0080,

    // Image is NX compatible
    NxCompat = 0x0100,

    // Image understands isolation and doesn't want it
    NoIsolation = 0x0200,

    // Image does not use SEH.  No SE handler may reside in this image
    NoSeh = 0x0400,

    // Do not bind this image.
    NoBind = 0x0800,

    // Image should execute in an AppContainer
    AppContainer = 0x1000,

    // Driver uses WDM model
    WdmDriver = 0x2000,

    // Image supports Control Flow Guard.
    GuardCF = 0x4000,

    TerminalServerAware = 0x8000
}

public enum DirectoryEntry
{
    // Export Directory
    Export = 0,

    // Import Directory
    Import = 1,

    // Resource Directory
    Resource = 2,

    // Exception Directory
    Exception = 3,

    // Security Directory
    Security = 4,

    // Base Relocation Table
    BaseReloc = 5,

    // Debug Directory
    Debug = 6,

    // Architecture Specific Data
    Architecture = 7,

    // RVA of GP
    GlobalPtr = 8,

    // TLS Directory
    Tls = 9,

    // Load Configuration Directory
    LoadConfig = 10,

    // Bound Import Directory in headers
    BoundImport = 11,

    // Import Address Table
    Iat = 12,

    // Delay Load Import Descriptors
    DelayImport = 13,

    // COM Runtime descriptor
    ComDescriptor = 14,

    // Reserved, must be zero
    Reserved = 15
}
