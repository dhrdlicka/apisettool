namespace ApiSetTool.PortableExecutable;

public class PortableExecutable
{
    Stream stream;

    public DosExecutableHeader? DosExecutableHeader { get; }
    public PeFileHeader FileHeader { get; }
    public PeOptionalHeader OptionalHeader { get; }
    public PeSectionHeader[] SectionHeaders { get; }

    public PortableExecutable(Stream stream)
    {
        this.stream = stream;

        var buffer = new byte[256].AsSpan();

        // DOS stub header
        stream.Read(buffer[0..64]);

        try
        {
            DosExecutableHeader = new(buffer[0..64]);
        }
        catch (InvalidDataException)
        {
            // This is not a valid MZ header.
            stream.Position = 0;
        }

        if (DosExecutableHeader is not null)
        {
            // PE magic number
            stream.Position = DosExecutableHeader.LfaNew;
            stream.Read(buffer[0..4]);

            var magic = Encoding.ASCII.GetString(buffer[0..4]);
            if (magic != "PE\0\0")
            {
                throw new InvalidDataException();
            }
        }

        // File header
        stream.Read(buffer[0..20]);
        FileHeader = new(buffer[0..20]);

        // Optional header
        stream.Read(buffer[0..FileHeader.SizeOfOptionalHeader]);
        OptionalHeader = new(buffer[0..FileHeader.SizeOfOptionalHeader]);

        // Section headers
        SectionHeaders = new PeSectionHeader[FileHeader.NumberOfSections];
        for (var i = 0; i < FileHeader.NumberOfSections; i++)
        {
            stream.Read(buffer[0..40]);
            SectionHeaders[i] = new(this, buffer[0..40]);
        }
    }

    public PortableExecutable(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }

    internal Stream GetFileStream() => stream;
}
