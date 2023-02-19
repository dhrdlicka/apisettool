var context = new ApiSetSchemaContext(new()
{
    WriteIndented = true,
    Converters =
    {
        new ApiSetSchemaJsonConverter(),
        new ApiSetValueEntryJsonConverter(),
        new ApiSetNamespaceEntryJsonConverter(),
    }
});

var rootCommand = new RootCommand("Windows API set schema manipulation utility");

var jsonFileArgument = new Argument<FileInfo>("file", "JSON dump of an API set schema").ExistingOnly();

var binFileArgument = new Argument<FileInfo>("file", "ApiSetSchema DLL or an extracted .apiset section").ExistingOnly();
if (OperatingSystem.IsWindowsVersionAtLeast(10))
{
    if (Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
    {
        binFileArgument.SetDefaultValue(new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "apisetschema.dll")));
    }
    else
    {
        binFileArgument.SetDefaultValue(new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysNative/apisetschema.dll")));
    }
}

var outputOption = new Option<FileInfo?>("--output", "Output file");
outputOption.AddAlias("-o");
outputOption.SetDefaultValue(null);

var formatOption = new Option<ApiSetSchemaFormat>("--format", "API set format");
formatOption.AddAlias("-f");
formatOption.SetDefaultValue(ApiSetSchemaFormat.Sequential);

var buildCommand = new Command("build", "Generate an API set section from a JSON dump");
buildCommand.AddArgument(jsonFileArgument);
buildCommand.AddOption(outputOption);
buildCommand.AddOption(formatOption);
buildCommand.SetHandler(Build, jsonFileArgument, outputOption, formatOption);

var decompileCommand = new Command("decompile", "Decompile the contents of an API set section into JSON format");
decompileCommand.AddArgument(binFileArgument);
decompileCommand.AddOption(outputOption);
decompileCommand.SetHandler(Decompile, binFileArgument, outputOption);

var apiArgument = new Argument<string>("api", "API set name");

var queryCommand = new Command("query", "Query API set data");
queryCommand.AddArgument(binFileArgument);
queryCommand.AddArgument(apiArgument);
queryCommand.SetHandler(Query, binFileArgument, apiArgument);

rootCommand.AddCommand(buildCommand);
rootCommand.AddCommand(decompileCommand);
rootCommand.AddCommand(queryCommand);

return await rootCommand.InvokeAsync(args);

void Build(FileInfo input, FileInfo? output, ApiSetSchemaFormat options)
{
    using var inputStream = new FileStream(input.FullName, FileMode.Open, FileAccess.Read);
    using var outputStream = new FileStream(output?.FullName ?? input.FullName + ".apiset", FileMode.Create, FileAccess.Write);

    var schema = (ApiSetSchema)JsonSerializer.Deserialize(inputStream, typeof(ApiSetSchema), context)!;

    new ApiSetSchemaSerializer().Serialize(outputStream, schema, options);

    outputStream.SetLength((outputStream.Length + 4093) & ~4093);
}

bool TryReadDll(Stream stream, [NotNullWhen(true)] out PortableExecutable? exe)
{
    stream.Position = 0;

    try
    {
        exe = new PortableExecutable(stream);
        return true;
    }
    catch (InvalidDataException)
    {
        exe = null;
        return false;
    }
}

ApiSetSchema? ReadSchemaFromStream(Stream stream)
{
    stream.Position = 0;

    var buffer = new byte[stream.Length];
    stream.Read(buffer);

    try
    {
        return new ApiSetSchemaSerializer().Deserialize(buffer);
    }
    catch (InvalidDataException)
    {
        return null;
    }
    catch (NotSupportedException)
    {
        return null;
    }
}

ApiSetSchema? ReadSchemaFromDll(Stream stream)
{
    if (TryReadDll(stream, out var exe))
    {
        Stream? sectionStream = null;

        foreach (var section in exe.SectionHeaders)
        {
            if (section.Name == ".apiset")
            {
                sectionStream = section.GetStream();
                break;
            }
        }

        if (sectionStream is not null)
        {
            return ReadSchemaFromStream(sectionStream);
        }
    }

    return null;
}

void Decompile(FileInfo input, FileInfo? output)
{
    using var inputStream = new FileStream(input.FullName, FileMode.Open, FileAccess.Read);
    using var outputStream = new FileStream(output?.FullName ?? input.Name + ".json", FileMode.Create, FileAccess.Write);

    var schema = ReadSchemaFromStream(inputStream) ?? ReadSchemaFromDll(inputStream) ?? throw new InvalidDataException();

    JsonSerializer.Serialize(outputStream, schema, typeof(ApiSetSchema), context);
}

void Query(FileInfo file, string api)
{
    using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
    var schema = ReadSchemaFromStream(stream) ?? ReadSchemaFromDll(stream) ?? throw new InvalidDataException();

    if (schema.Entries.TryGetValue(api, out var ns))
    {
        Console.WriteLine($"{api} => {ns.DefaultValue.Value}");

        foreach (var (name, value) in ns.OtherValues.Where(x => !x.Value.IsEmpty))
        {
            Console.WriteLine($"  {name} => {value.Value}");
        }
    }
    else
    {
        Console.WriteLine($"{api}: not found");
    }

    Console.WriteLine();
}
