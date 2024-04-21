# KnuxLib

A repository for me to push various bits of C# code for random file formats in random games I'm tinkering with, alongside a command line tool for extracting/building supported archives/converting supported formats to and from JSON plain text formats.

## Projects

- KnuxLib - The actual C# library itself. For fully supported formats see the [Supported_Formats.md](Supported_Formats.md) file. For formats that need work see either the Issues Tab or the [Experimental_Formats.md](Experimental_Formats.md) file.

- KnuxTest - A template CLI program designed for me to use when writing test code, should be left unedited with commits.

- KnuxTools - A CLI program intended for use by the end user.

## Building

To build any of the projects here, simply clone the repository and its submodules (either using a tool such as GitHub Desktop or Git CMD) then open the .sln file in a modern version of Visual Studio (anything supporting .NET 6 C# development).

For the average end user, they'll want to right click on the KnuxTools project and choose to `Build`, then find the compiled executable in `KnuxTools/bin/Debug/net6.0`.

A developer looking to experiment with or contribute to the project will want to use the KnuxTest project for development, only editing the KnuxTools project to add support for a newly implemented format.

## Used Libraries

This repository also uses various other libraries as part of it, included as either a Nuget Package or a Git Submodule. These projects are:

- [AuroraLib.Compression](https://github.com/Venomalia/AuroraLib.Compression) - Used for decompressing/recompressing PRS data in Sonic Storybook Engine ONE Archives and decompressing/recompressing LZ11 data in Sonic World Adventure Wii Engine ONZ Archives.

- [libHSON-csharp](https://github.com/hedge-dev/libHSON-csharp) - Used for reading and writing HSON files.

- [HedgeLib](https://github.com/Radfordhound/HedgeLib/tree/master) - Used for its BINAV2 Reader and Writer.

- [Marathon](https://github.com/Big-Endian-32/Marathon) - Used for its BinaryReader and Writer.

- [SixLabors Image Sharp](https://github.com/SixLabors/ImageSharp) - Used for extracting and converting textures from Nu2 Scenes.