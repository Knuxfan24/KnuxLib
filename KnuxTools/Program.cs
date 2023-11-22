using System.Globalization;
using System.Numerics;
using System.Text;

namespace KnuxTools
{
    // TODO: Implement:
    //       Storybook SET -> HSON
    //       World Adventure Wii SET -> HSON
    //       Wayforward Model Importing
    // TODO: Tidy up duplicated comments and parts where they are sorely lacking.
    internal class Program
    {
        static void Main(string[] args)
        {
            // Force culture info 'en-GB' to prevent errors with values altered by culture-specific differences.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

            // Enable shift-jis for HedgeLib# stuff.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Check for arguments.
            if (args.Length > 0)
            {
                // Set up the extension and version values.
                string? extension = null;
                string? version = null;

                // Loop through and set the extension and version, if the parameters are passed.
                foreach (string arg in args)
                {
                    if (arg.StartsWith("-extension=") || arg.StartsWith("-e="))
                        extension = arg.Split('=')[1];

                    if (arg.StartsWith("-version=") || arg.StartsWith("-v="))
                        version = arg.Split('=')[1];
                }

                // Loop through each argument.
                foreach (string arg in args)
                {
                    // Directory based checks.
                    if (Directory.Exists(arg))
                    {
                        // Print the directory name.
                        Console.WriteLine($"Directory: {arg} \n");

                        // Pass the argument and version onto the SaveArchives function.
                        HandleDirectory(arg, version);
                    }

                    // File based checks.
                    if (File.Exists(arg))
                    {
                        // Print the file name.
                        Console.WriteLine($"File: {arg} \n");

                        // Pass the argument, extension and version onto the HandleFile function.
                        HandleFile(arg, extension, version);
                    }
                }
            }

            // If there are no arguments, then inform the user.
            else
            {
                Console.WriteLine("Command line tool used to convert the following supported file types to various other formats.");
                Console.WriteLine("Each format converts to and from a JSON file unless otherwise specified.");
                Console.Write("File formats listed in");
                ColourConsole(" red ", false);
                Console.Write("are considered experimental and may be missing elements or functionality (see the Experimental Formats document at");
                ColourConsole(" https://github.com/Knuxfan24/KnuxLib/blob/master/Experimental_Formats.md ", false, ConsoleColor.Cyan); 
                Console.Write("for information on the individual issues).\n\n");

                Console.WriteLine("Alchemy Engine:");
                ColourConsole("Assets Container Archive Pair (.gfc/gob) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("Collision (.hke) - Converts to a set of OBJs in a directory with the same name as the input file (importing and saving not yet possible).\n");

                Console.WriteLine("Engine Black:");
                ColourConsole("Data Archive (.data) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("Volume Blob (.vol) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("CarZ Engine:");
                Console.WriteLine("Material Library (.mat) - Exports to the MTL material library standard and imports from an Assimp compatible model.");
                Console.WriteLine("3D Model (.sco) - Exports to the Wavefront OBJ model standard and imports from an Assimp compatible model.");
                ColourConsole("    Version Flag (importing, shared with material library) - carz\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Flipnic Engine:");
                ColourConsole("Binary Archive (.bin) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Gods Engine:");
                ColourConsole("WAD Archive (.wad) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("    Version Flag (Ninjabread Man (PC/PS2)) - ninjabread_pc", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Ninjabread Man (Wii)) - ninjabread_wii\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Hedgehog Engine:");
                Console.WriteLine("Archive Info (.arcinfo)");
                Console.WriteLine("Bullet Skeleton (.skl.pxd)");
                ColourConsole("2010 Collision (.orc) - Converts to an OBJ with the same name as the input file (importing and saving not yet possible).");
                Console.WriteLine("Gismo V3 (.gismod/.gismop)");
                Console.WriteLine("Instance Info (.terrain-instanceinfo) - Import a folder containing files to generate a Sonic Frontiers point cloud file.");
                ColourConsole("    Version Flag (point cloud conversion) - hh_instance2pointcloud", true, ConsoleColor.Yellow);
                Console.WriteLine("Light Field (Rangers) (.lf)");
                Console.WriteLine("Master Level Table (.mlevel)");
                Console.WriteLine("Message Table (sonic2010/blueblur/william) (.xtb)");
                ColourConsole("    Version Flag (Sonic Colours) - sonic2010", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic Generations) - blueblur", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Mario and Sonic at the London 2012 Olympic Games) - william", true, ConsoleColor.Yellow);
                Console.WriteLine("Message Table (sonic2013) (.xtb2)");
                Console.WriteLine("Point Cloud (.pccol/.pcmodel/.pcrt)");
                ColourConsole("    Extension Flag (Collision Instance) - pccol", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Terrain Instance) - pcmodel", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Lighting Instance) - pcrt", true, ConsoleColor.Yellow);
                ColourConsole("Scene Effect Collision (.fxcol.bin)");
                Console.WriteLine("Sector Visibility Collision (.svcol.bin)\n");

                Console.WriteLine("NiGHTS 2 Engine:");
                Console.WriteLine("ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - nights2\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Nu2 Engine:");
                Console.WriteLine("AI Entity Table (.ai)");
                ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2", true, ConsoleColor.Yellow);
                Console.WriteLine("Wumpa Fruit Table (.wmp)");
                ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Project M Engine:");
                Console.WriteLine("Message Table (.dat) \n");

                Console.WriteLine("Rockman X7 Engine:");
                ColourConsole("Stage Entity Table (.328f438b/.osd)");
                ColourConsole("    Extension Flag (Original) - OSD", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Legacy Collection 2) - 328F438B\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Rockman X8 Engine:");
                ColourConsole("Stage Entity Table (.31bf570e/.set)");
                ColourConsole("    Version Flag - rockx8", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Original) - SET", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Legacy Collection 2) - 31BF570E\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic Storybook Engine:");
                Console.WriteLine("Message Table (Secret Rings) (.mtx)");
                ColourConsole("    Version Flag (English (and other languages)) - international", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Japanese) - japanese", true, ConsoleColor.Yellow);
                Console.WriteLine("ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - storybook", true, ConsoleColor.Yellow);
                ColourConsole("Path Spline (.pth) - Converts to an OBJ with the same name as the input file (importing and saving not yet possible).");
                ColourConsole("    Version Flag (Sonic and the Secret Rings) - secretrings", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic and the Black Knight) - blackknight", true, ConsoleColor.Yellow);
                ColourConsole("Player Motion Table (.bin)");
                ColourConsole("    Version Flag - storybook_motion", true, ConsoleColor.Yellow);
                ColourConsole("Stage Entity Table (.bin) - Converts to a HSON with the same name as the input file (importing and saving not yet possible).");
                ColourConsole("    Version Flag - storybook_set", true, ConsoleColor.Yellow);
                Console.WriteLine("Stage Entity Table Object Table (.bin)");
                ColourConsole("    Version Flag (Sonic and the Secret Rings) - storybook_setitems_sr", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic and the Black Knight) - storybook_setitems_bk", true, ConsoleColor.Yellow);
                Console.WriteLine("Texture Directory (.txd) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - storybook_texture\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic The Portable Engine:");
                Console.WriteLine("AMB Archive (.amb) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - portable\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic World Adventure Wii Engine:");
                Console.WriteLine("Area Points Table (.wap)");
                ColourConsole("    Version Flag (PlayStation 2) - ps2", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Wii) - wii", true, ConsoleColor.Yellow);
                Console.WriteLine("ONE Archive (.one/.onz) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag (.one) - swawii", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (.onz) - swawii_compressed", true, ConsoleColor.Yellow);
                ColourConsole("Stage Entity Table (.set) - Converts to a HSON with the same name as the input file (importing and saving not yet possible).");
                ColourConsole("    Version Flag (PlayStation 2) - ps2", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Wii) - wii\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Wayforward Engine:");
                ColourConsole("Collision (.clb) - Converts to an OBJ format and imports from an Assimp compatible model.");
                ColourConsole("    Version Flag (Half-Genie Hero) - wayforward_collision_hgh", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Seven Sirens) - wayforward_collision_ss", true, ConsoleColor.Yellow);
                Console.WriteLine("Environment (.env)");
                Console.WriteLine("Layer List (.lgb)");
                Console.WriteLine("List Table (.ltb)");
                ColourConsole("Mesh (.wf3d) - Converts to an OBJ format and imports from an Assimp compatible model.");
                ColourConsole("    Version Flag (importing) - wayforward", true, ConsoleColor.Yellow);
                Console.WriteLine("Package Archive (.pak) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - wayforward\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Westwood Engine:");
                Console.WriteLine("Message Table (.tre/.tru)");
                ColourConsole("    Extension Flag (USA) - tre", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (UK) - tru\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Usage:");
                Console.WriteLine("KnuxTools.exe \"path\\to\\supported\\file\" [-version={VERSION}] [-extension={EXTENSION}]");
                Console.WriteLine("Arguments surrounded by square brackets are optional and only affect certain formats (highlighted in yellow), if they aren't specified then they will be selected through manual input when required.\n");
                Console.WriteLine("Alternatively, simply drag a supported file onto this application in Windows Explorer.\n");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Colours the console for a string.
        /// </summary>
        /// <param name="message">The message to print in red.</param>
        /// <param name="isLine">Whether the message should be a WriteLine or a standard Write.</param>
        /// <param name="color">The colour to use, defaulting to red.</param>
        private static void ColourConsole(string message, bool isLine = true, ConsoleColor color = ConsoleColor.Red)
        {
            // Colour the console.
            Console.ForegroundColor = color;

            // Write the message depending on line type.
            if (isLine)
                Console.WriteLine(message);
            else
                Console.Write(message);

            // Return the console colour to grey.
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Function for handling directories.
        /// </summary>
        /// <param name="arg">The path of the directory that is currently being processed.</param>
        /// <param name="version">The version parameter to use.</param>
        private static void HandleDirectory(string arg, string? version)
        {
            // If a version isn't specified, then ask the user which archive to save as.
            if (string.IsNullOrEmpty(version))
            {
                // List the supported archive types.
                Console.WriteLine("Please specify the archive type to pack this directory into, valid options are:");
                Console.WriteLine("    hh_instance2pointcloud\t(Convert Hedgehog Engine Terrain Instances into a Hedgehog Engine Point Cloud)");
                Console.WriteLine("    nights2\t\t\t(NiGHTS 2 Engine ONE File)");
                Console.WriteLine("    storybook\t\t\t(Sonic Storybook Engine ONE File)");
                Console.WriteLine("    storybook_texture\t\t(Sonic Storybook Engine TXD File)");
                Console.WriteLine("    portable\t\t\t(Sonic The Portable Engine AMB File)");
                Console.WriteLine("    swawii\t\t\t(Sonic World Adventure Wii Engine ONE File)");
                Console.WriteLine("    swawii_compressed\t\t(Sonic World Adventure Wii Engine Compressed ONZ File)");
                Console.WriteLine("    wayforward\t\t\t(Wayforward Engine PAK File)");

                // Ask for the user's input.
                Console.Write("\nArchive Type: ");

                // Wait for the user's input.
                version = Console.ReadLine().ToLower();

                // Sanity check the input, abort if its still null or empty.
                if (string.IsNullOrEmpty(version))
                {
                    Console.WriteLine("\nNo archive type specified! Aborting...\nPress any key to continue.");
                    Console.ReadKey();
                    return;
                }

                // Add a line break.
                Console.WriteLine();
            }

            // Decide what to do based on the version value.
            // In most cases this will be writing a line for user feedback then running the ImportAndSaveArchive with the right type and extension.
            switch (version.ToLower())
            {
                // NiGHTS 2 Engine ONE Archives.
                case "nights2":
                    Console.WriteLine("Packing directory for NiGHTS 2 Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.NiGHTS2.ONE), arg, "one");
                    break;

                // Hedgehog Engine Terrain Instance Conversion.
                case "hh_instance2pointcloud":
                    Console.WriteLine("Converting Hedgehog Engine Terrain Instance Info files to Hedgehog Engine Point Cloud files.");
                    KnuxLib.Engines.Hedgehog.InstanceInfo.ConvertDirectoryToPointCloud(arg);
                    break;

                // Sonic Storybook Series ONE Archives.
                case "storybook":
                    Console.WriteLine("Packing directory for Sonic Storybook Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.Storybook.ONE), arg, "one");
                    break;

                // Sonic Storybook Series Texture Directories.
                case "storybook_texture":
                    Console.WriteLine("Packing directory for Sonic Storybook Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.Storybook.TextureDirectory), arg, "txd");
                    break;

                // Sonic The Portable Engine AMB Archives.
                case "portable":
                    Console.WriteLine("Packing directory for Sonic The Portable Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.Portable.AMB), arg, "amb");
                    break;

                // Sonic World Adventure Wii ONE Archives.
                case "swawii":
                case "swawii_compressed":
                    Console.WriteLine("Packing directory for Sonic World Adventure Wii Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.WorldAdventureWii.ONE), arg, "one");

                    // If the version indicates that the archive needs to be compressed, then compress it.
                    if (version == "swawii_compressed")
                    {
                        // Inform the user of the compression.
                        Console.WriteLine("Compressing generated archive for Sonic World Adventure Wii Engine.");

                        // Set up PuyoTools' LZ11 Compression.
                        PuyoTools.Core.Compression.Lz11Compression lz11 = new();

                        // Set up a file stream of the uncompressed archive.
                        var stream = File.OpenRead($@"{arg}.one");

                        // Compress the previously saved ONE archive into a buffer.
                        MemoryStream buffer = lz11.Compress(stream);

                        // Write the buffer to disk.
                        buffer.WriteTo(File.Create($@"{arg}.onz"));

                        // Close the file stream so the uncompressed archive can be deleted.
                        stream.Close();

                        // Delete the temporary uncompressed file.
                        File.Delete($@"{arg}.one");
                    }
                    break;

                // Wayforward Engine Packages.
                case "wayforward":
                    Console.WriteLine("Packing directory for Wayforward Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.Wayforward.Package), arg, "pak");
                    break;

                // If a command line argument without a corresponding format has been passed, then inform the user.
                default:
                    Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported archive types.\nPress any key to continue.");
                    Console.ReadKey();
                    return;
            }

            // Tell the user we're done (for if the cmd window is left open).
            Console.WriteLine("Done!");
        }

        /// <summary>
        /// Handles importing files to an archive class and saving it.
        /// </summary>
        /// <param name="type">The type of archive to use</param>
        /// <param name="path">The path to the directory we're packing.</param>
        /// <param name="extension">The extension to save the packaged archive with.</param>
        private static void ImportAndSaveArchive(Type type, string path, string extension)
        {
            // Create an object for the specified archive type.
            object archive = Activator.CreateInstance(type);

            // Invoke this archive's import method with the specified directory as an argument.
            type.GetMethod("Import").Invoke(archive, new string[1] { path });

            // Invoke this archive's save method with the specified directory and extension as arguments.
            type.GetMethod("Save", new[] {typeof(string)}).Invoke(archive, new string[1] { $@"{path}.{extension}" });
            
        }

        /// <summary>
        /// Function for handling files.
        /// </summary>
        /// <param name="arg">The path of the file that is currently being processed.</param>
        /// <param name="extension">The extension parameter to use.</param>
        /// <param name="version">The version parameter to use.</param>
        private static void HandleFile(string arg, string? extension, string? version)
        {
            switch (KnuxLib.Helpers.GetExtension(arg).ToLower())
            {
                #region Generic Seralised Models.
                case ".fbx":
                case ".dae":
                case ".obj":
                    // If a version isn't specified, then ask the user which format to import for.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file is a generic model, please specifiy what format to import and save it as:");
                        Console.WriteLine("    carz\t\t\t(CarZ Engine SCO model & MAT material library)");
                        ColourConsole("    wayforward\t\t\t(Wayforward Engine WF3D Mesh)");
                        ColourConsole("    wayforward_collision_hgh\t(Wayforward Engine Collision for Half-Genie Hero)");
                        ColourConsole("    wayforward_collision_ss\t(Wayforward Engine Collision for Seven Sirens)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        // CarZ Engine SCO Models (and Material Libraries).
                        case "carz":
                            Console.WriteLine("Converting model to a CarZ Engine SCO/MAT pair.");
                            using (KnuxLib.Engines.CarZ.SCO sco = new())
                            {
                                sco.ImportAssimp(arg);
                                sco.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.sco");
                            }
                            using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new())
                            {
                                mat.ImportAssimp(arg);
                                mat.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.mat");
                            }
                            break;

                        case "wayforward":
                            // TODO: Figure this out later.
                            break;

                        case "wayforward_collision_hgh":
                            Console.WriteLine("Converting model to a Wayforward Engine collision file.");
                            using (KnuxLib.Engines.Wayforward.Collision collision = new())
                            {
                                collision.ImportAssimp(arg);
                                collision.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.clb", KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero);
                            }
                            break;

                        case "wayforward_collision_ss":
                            Console.WriteLine("Converting model to a Wayforward Engine collision file.");
                            using (KnuxLib.Engines.Wayforward.Collision collision = new())
                            {
                                collision.ImportAssimp(arg);
                                collision.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.clb", KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported model types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }

                    break;
                #endregion

                #region Generic File Formats.
                case ".bin":
                    // If a version isn't specified, then ask the user which ONE format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        ColourConsole("    flipnic\t\t\t(Flipnic Binary Archive File)");
                        ColourConsole("    storybook_motion\t\t(Sonic Storybook Engine Player Motion File)");
                        ColourConsole("    storybook_set\t\t(Sonic Storybook Engine Stage Entity Table File)");
                        Console.WriteLine("    storybook_setitems_sr\t(Sonic Storybook Engine Stage Entity Table Object Table File (Secret Rings))");
                        Console.WriteLine("    storybook_setitems_bk\t(Sonic Storybook Engine Stage Entity Table Object Table File (Black Knight))");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    switch (version.ToLower())
                    {
                        case "flipnic":
                            Console.WriteLine("Extracting Flipnic Engine binary archive.");
                            using (KnuxLib.Engines.Flipnic.BinaryArchive bin = new(arg, true))
                            break;

                        case "storybook_motion":
                            Console.WriteLine("Converting Sonic Storybook Engine Player Motion Table to JSON.");
                            using (KnuxLib.Engines.Storybook.PlayerMotionTable playerMotion = new(arg, true))
                            break;

                        case "storybook_set":
                            // TODO: Figure this out later.
                            break;

                        case "storybook_setitems_sr":
                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table Object Table to JSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings, true))
                            break;

                        case "storybook_setitems_bk":
                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table Object Table to JSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".one":
                    // If a version isn't specified, then ask the user which ONE format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    nights2\t\t\t(NiGHTS 2 Engine ONE File)");
                        Console.WriteLine("    storybook\t\t\t(Sonic Storybook Engine ONE File)");
                        Console.WriteLine("    swawii\t\t\t(Sonic World Adventure Wii Engine ONE File)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    switch (version.ToLower())
                    {
                        case "nights2":
                            Console.WriteLine("Extracting NiGHTS2 Engine archive.");
                            using (KnuxLib.Engines.NiGHTS2.ONE one = new(arg, true))
                            break;

                        case "storybook":
                            Console.WriteLine("Extracting Sonic Storybook Engine archive.");
                            using (KnuxLib.Engines.Storybook.ONE one = new(arg, true))
                            break;

                        case "swawii":
                            Console.WriteLine("Extracting Sonic World Adventure Wii Engine archive.");
                            using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".set":
                    // If a version isn't specified, then ask the user which ONE format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    rockx8\t\t\t(Rockman X8 Engine Stage Entity Table File)");
                        Console.WriteLine("    swa\t\t\t(Sonic World Adventure Wii Stage Entity Table File)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    switch (version.ToLower())
                    {
                        case "rockx8":
                            Console.WriteLine("Converting Rockman X8 Engine Stage Entity Table to JSON.");
                            using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new(arg, KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.Original, true))
                            break;

                        case "swawii":
                            // TODO: Figure this out later.
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Alchemy Engine formats.
                case ".gfc":
                    Console.WriteLine("Extracting Alchemy Engine assests container.");
                    using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new(arg, true))
                    break;

                case ".gob":
                    Console.WriteLine("Extracting Alchemy Engine assests container.");
                    using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.gfc", true))
                    break;

                case ".hke":
                    Console.WriteLine("Extracting Alchemy Engine collision file.");
                    using (KnuxLib.Engines.Alchemy.Collision collision = new(arg, true))
                    break;
                #endregion

                #region Engine Black formats.
                case ".data":
                    Console.WriteLine("Extracting Engine Black data archive.");
                    using (KnuxLib.Engines.Black.DataArchive data = new(arg, true))
                    break;

                case ".vol":
                    Console.WriteLine("Extracting Engine Black volume blob.");
                    using (KnuxLib.Engines.Black.VolumeBlob vol = new(arg, true))
                    break;
                #endregion

                #region CarZ Engine formats.
                case ".mat":
                    Console.WriteLine("Converting CarZ material library to MTL.");
                    using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new(arg, true))
                    break;

                case ".sco":
                    Console.WriteLine("Converting CarZ model to OBJ.");
                    using (KnuxLib.Engines.CarZ.SCO sco = new(arg, true))
                    break;
                #endregion

                #region GODS Engine formats.
                case ".wad":
                    // If a version isn't specified, then ask the user which WAD format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    ninjabread_pc\t\t\t(Ninjabread Man (PC/PS2))");
                        Console.WriteLine("    ninjabread_wii\t\t\t(Ninjabread Man (Wii))");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    switch (version.ToLower())
                    {
                        case "ninjabread_pc":
                            Console.WriteLine("Extracting GODS Engine WAD archive.");
                            using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_PCPS2, true))
                            break;

                        case "ninjabread_wii":
                            Console.WriteLine("Extracting GODS Engine WAD archive.");
                            using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_Wii, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Hedgehog Engine formats.
                case ".arcinfo":
                    Console.WriteLine("Converting Hedgehog Engine Archive Info to JSON.");
                    using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new(arg, true))
                    break;

                case ".hedgehog.archiveinfo.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Archive Info.");
                    using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new())
                    {
                        archiveInfo.Data = archiveInfo.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.ArchiveInfo.ArchiveEntry>>(arg);
                        archiveInfo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.arcinfo");
                    }
                    break;

                case ".skl.pxd":
                    Console.WriteLine("Converting Hedgehog Engine Bullet Skeleton to JSON.");
                    using (KnuxLib.Engines.Hedgehog.BulletSkeleton bulletSkeleton = new(arg, true))
                    break;

                case ".hedgehog.bulletskeleton.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Bullet Skeleton.");
                    using (KnuxLib.Engines.Hedgehog.BulletSkeleton bulletSkeleton = new())
                    {
                        bulletSkeleton.Data = bulletSkeleton.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.BulletSkeleton.Node>>(arg);
                        bulletSkeleton.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.skl.pxd");
                    }
                    break;

                case ".orc":
                    Console.WriteLine("Converting Hedgehog Engine 2010 Collision to OBJ.");
                    using (KnuxLib.Engines.Hedgehog.Collision_2010 collision = new(arg, true))
                    break;

                case ".gismod":
                    Console.WriteLine("Converting Hedgehog Engine Rangers Gismo to JSON.");
                    using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new(arg, true))
                    break;

                case ".gismop":
                    Console.WriteLine("Converting Hedgehog Engine Rangers Gismo to JSON.");
                    using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg).Replace("_pln", "")}.gismod", true))
                    break;

                case ".hedgehog.gismo_rangers.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Rangers Gismo.");
                    using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new())
                    {
                        gismo_rangers.Data = gismo_rangers.JsonDeserialise<KnuxLib.Engines.Hedgehog.Gismo_Rangers.FormatData>(arg);
                        gismo_rangers.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.gismod");
                    }
                    break;

                case ".terrain-instanceinfo":
                    Console.WriteLine("Converting Hedgehog Engine Instance Info to JSON.");
                    using (KnuxLib.Engines.Hedgehog.InstanceInfo instanceInfo = new(arg, true)) 
                    break;

                case ".hedgehog.instanceinfo.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Instance Info.");
                    using (KnuxLib.Engines.Hedgehog.InstanceInfo instanceInfo = new())
                    {
                        instanceInfo.Data = instanceInfo.JsonDeserialise<KnuxLib.Engines.Hedgehog.InstanceInfo.FormatData>(arg);
                        instanceInfo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.terrain-instanceinfo");
                    }
                    break;

                case ".lf":
                    Console.WriteLine("Converting Hedgehog Engine Rangers Light Field to JSON."); 
                    using (KnuxLib.Engines.Hedgehog.LightField_Rangers lightfield_rangers = new(arg, true))
                    break;

                case ".hedgehog.lightfield_rangers.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Rangers Light Field.");
                    using (KnuxLib.Engines.Hedgehog.LightField_Rangers lightfield_rangers = new())
                    {
                        lightfield_rangers.Data = lightfield_rangers.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.LightField_Rangers.LightField>>(arg);
                        lightfield_rangers.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.lf");
                    }
                    break;

                case ".mlevel":
                    Console.WriteLine("Converting Hedgehog Engine Master Levels Table to JSON.");
                    using (KnuxLib.Engines.Hedgehog.MasterLevels mlevel = new(arg, true))
                        break;

                case ".hedgehog.masterlevels.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Master Levels Table.");
                    using (KnuxLib.Engines.Hedgehog.MasterLevels mlevel = new())
                    {
                        mlevel.Data = mlevel.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.MasterLevels.Level>>(arg);
                        mlevel.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.mlevel");
                    }
                    break;

                case ".xtb":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    sonic2010\t\t\t(Sonic Colours)");
                        Console.WriteLine("    blueblur\t\t\t(Sonic Generations)");
                        Console.WriteLine("    william\t\t\t(Mario and Sonic at the London 2012 Olympic Games)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Hedgehog Engine 2010 Message Table to JSON.");
                    switch (version.ToLower())
                    {
                        case "sonic2010": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.sonic_2010, true)) break;
                        case "blueblur": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur, true)) break;
                        case "william": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".hedgehog.messagetable_2010.json":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants, please specifiy the variant to save with:");
                        Console.WriteLine("    sonic2010\t\t\t(Sonic Colours)");
                        Console.WriteLine("    blueblur\t\t\t(Sonic Generations)");
                        Console.WriteLine("    william\t\t\t(Mario and Sonic at the London 2012 Olympic Games)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Hedgehog Engine 2010 Message Table.");
                    switch (version.ToLower())
                    {
                        case "sonic2010":
                            using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                            {
                                messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb");
                            }
                            break;
                        case "blueblur":
                            using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                            {
                                messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb", KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur);
                            }
                            break;
                        case "william":
                            using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                            {
                                messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb", KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".xtb2":
                    Console.WriteLine("Converting Hedgehog Engine 2010 Message Table to JSON.");
                    using (KnuxLib.Engines.Hedgehog.MessageTable_2013 messageTable_2013 = new(arg, true)) 
                    break;

                case ".hedgehog.messagetable_2013.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine 2013 Message Table.");
                    using (KnuxLib.Engines.Hedgehog.MessageTable_2013 messageTable_2013 = new())
                    {
                        messageTable_2013.Data = messageTable_2013.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2013.FormatData>(arg);
                        messageTable_2013.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb2");
                    }
                    break;

                case ".pcmodel":
                case ".pccol":
                case ".pcrt":
                    Console.WriteLine("Converting Hedgehog Engine Point Cloud to JSON.");
                    using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new(arg, true))
                    break;

                case ".hedgehog.pointcloud.json":
                    // If an extension isn't specified, then ask the user which point cloud extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .pccol (Collision Instance)\n" +
                            "2. .pcmodel (Terrain Instance)\n" +
                            "3. .pcrt (Lighting Instance)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "pccol"; break;
                            case '2': extension = "pcmodel"; break;
                            case '3': extension = "pcrt"; break;
                        }

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(extension))
                        {
                            Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Hedgehog Engine Point Cloud.");
                    using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new())
                    {
                        pointCloud.Data = pointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.PointCloud.Instance>>(arg);
                        pointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                    }
                    break;

                case ".fxcol.bin":
                    Console.WriteLine("Converting Hedgehog Engine Scene Effect Collision to JSON.");
                    using (KnuxLib.Engines.Hedgehog.SceneEffectCollision sceneEffectCollision = new(arg, true))
                    break;

                case ".hedgehog.sceneeffectcollision.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Scene Effect Collision.");
                    using (KnuxLib.Engines.Hedgehog.SceneEffectCollision sceneEffectCollision = new())
                    {
                        sceneEffectCollision.Data = sceneEffectCollision.JsonDeserialise<KnuxLib.Engines.Hedgehog.SceneEffectCollision.FormatData>(arg);
                        sceneEffectCollision.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.fxcol.bin");
                    }
                    break;

                case ".svcol.bin":
                    Console.WriteLine("Converting Hedgehog Engine Sector Visiblity Collision to JSON.");
                    using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision sectorVisibilityCollision = new(arg, true))
                    break;

                case ".hedgehog.sectorvisiblitycollision.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Sector Visiblity Collision.");
                    using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision sectorVisibilityCollision = new())
                    {
                        sectorVisibilityCollision.Data = sectorVisibilityCollision.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.SectorVisibilityCollision.SectorVisibilityShape>>(arg);
                        sectorVisibilityCollision.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.svcol.bin");
                    }
                    break;
                #endregion

                #region Nu2 Engine formats.
                case ".ai":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    gcn\t\t\t(GameCube)");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2/Xbox)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Nu2 Engine AI Entity Table to JSON.");
                    switch (version.ToLower())
                    {
                        case "gcn": using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube, true)) break;
                        case "ps2": using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox, true)) break;
                        
                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".nu2.aientitytable.json":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    gcn\t\t\t(GameCube)");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2/Xbox)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Nu2 Engine AI Entity Table.");
                    switch (version.ToLower())
                    {
                        case "gcn":
                            using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                            {
                                aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube);
                            }
                            break;
                        case "ps2":
                            using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                            {
                                aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".wmp":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    gcn\t\t\t(GameCube)");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2/Xbox)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Nu2 Engine Wumpa Fruit Table to JSON.");
                    switch (version.ToLower())
                    {
                        case "gcn": using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube, true)) break;
                        case "ps2": using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox, true)) break;
                        
                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".nu2.wumpatable.json":
                    // If a version isn't specified, then ask the user which message table format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    gcn\t\t\t(GameCube)");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2/Xbox)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Nu2 Engine Wumpa Fruit Table.");
                    switch (version.ToLower())
                    {
                        case "gcn":
                            using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                            {
                                wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube);
                            }
                            break;
                        case "ps2":
                            using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                            {
                                wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Project M Engine formats.
                case ".dat":
                    Console.WriteLine("Converting Project M Engine Message Table to JSON.");
                    using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new(arg, true))
                    break;

                case ".projectm.messagetable.json":
                    Console.WriteLine("Converting JSON to Project M Engine Message Table.");
                    using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new())
                    {
                        messageTable.Data = messageTable.JsonDeserialise<KnuxLib.Engines.ProjectM.MessageTable.FormatData>(arg);
                        messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.dat");
                    }
                    break;
                #endregion

                #region Rockman X7 Engine Formats.
                case ".328f438b":
                case ".osd":
                    Console.WriteLine("Converting Rockman X7 Engine Stage Entity Table to JSON.");
                    using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new(arg, true))
                        break;

                case ".rockmanx7.stageentitytable.json":
                    // If an extension isn't specified, then ask the user which stage entity table extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .OSD (Original)\n" +
                            "2. .328F438B (Legacy Collection 2)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "OSD"; break;
                            case '2': extension = "328F438B"; break;
                        }

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(extension))
                        {
                            Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Rockman X7 Engine Stage Entity Table.");
                    using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                    {
                        stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                        stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                    }
                    break;
                #endregion

                #region Rockman X8 Engine Formats.
                case ".31bf570e":
                    Console.WriteLine("Converting Rockman X8 Engine Stage Entity Table to JSON.");
                    using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new(arg, KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.LegacyCollection, true))
                    break;

                case ".rockmanx8.stageentitytable.json":
                    // If an extension isn't specified, then ask the user which stage entity table extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .SET (Original)\n" +
                            "2. .31BF570E (Legacy Collection 2)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "SET"; break;
                            case '2': extension = "31BF570E"; break;
                        }

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(extension))
                        {
                            Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Rockman X8 Engine Stage Entity Table.");

                    switch (extension)
                    {
                        case "SET":
                            using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new())
                            {
                                stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX8.StageEntityTable.SetObject>>(arg);
                                stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.SET", KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.Original);
                            }
                            break;
                        case "31BF570E":
                            using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new())
                            {
                                stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX8.StageEntityTable.SetObject>>(arg);
                                stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.31BF570E", KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.LegacyCollection);
                            }
                            break;
                    }
                    break;
                #endregion

                #region Sonic Storybook Engine formats.
                case ".mtx":
                    // If a version isn't specified, then ask the user which path format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    international\t\t(English and other languages)");
                        Console.WriteLine("    japanese\t\t\t(Japanese)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Sonic Storybook Engine Secret Rings Message Table to JSON.");
                    switch (version.ToLower())
                    {
                        case "international":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new(arg, false, true))
                            break;

                        case "japanese":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new(arg, true, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".storybook.messagetable_secretrings.json":
                    // If a version isn't specified, then ask the user which path format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    international\t\t\t(English and other languages)");
                        Console.WriteLine("    japanese\t\t\t(Japanese)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Secret Rings Message Table.");
                    switch (version.ToLower())
                    {
                        case "international":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new())
                            {
                                messageTable.Data = messageTable.JsonDeserialise<List<KnuxLib.Engines.Storybook.MessageTable_SecretRings.MessageEntry>>(arg);
                                messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.mtx", false);
                            }
                            break;

                        case "japanese":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new())
                            {
                                messageTable.Data = messageTable.JsonDeserialise<List<KnuxLib.Engines.Storybook.MessageTable_SecretRings.MessageEntry>>(arg);
                                messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.mtx", true);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".pth":
                    // If a version isn't specified, then ask the user which path format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    secretrings\t\t\t(Sonic and the Secret Rings)");
                        Console.WriteLine("    blackknight\t\t\t(Sonic and the Black Knight)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Sonic Storybook Engine Path Spline to OBJ.");
                    switch (version.ToLower())
                    {
                        case "secretrings":
                            using (KnuxLib.Engines.Storybook.PathSpline pathSpline = new(arg, KnuxLib.Engines.Storybook.PathSpline.FormatVersion.SecretRings, true))
                            break;

                        case "blackknight":
                            using (KnuxLib.Engines.Storybook.PathSpline pathSpline = new(arg, KnuxLib.Engines.Storybook.PathSpline.FormatVersion.BlackKnight, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".storybook.playermotion.json":
                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Player Motion Table.");
                    using (KnuxLib.Engines.Storybook.PlayerMotionTable playerMotion = new())
                    {
                        playerMotion.Data = playerMotion.JsonDeserialise<List<KnuxLib.Engines.Storybook.PlayerMotionTable.MotionEntry>>(arg);
                        playerMotion.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin");
                    }
                    break;

                case ".storybook.stageentitytableitems.json":
                    // If a version isn't specified, then ask the user which path format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    secretrings\t\t\t(Sonic and the Secret Rings)");
                        Console.WriteLine("    blackknight\t\t\t(Sonic and the Black Knight)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Secret Rings Stage Entity Table Object Table.");
                    switch (version.ToLower())
                    {
                        case "secretrings":
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                            {
                                setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings);
                            }
                            break;

                        case "blackknight":
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                            {
                                setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".txd":
                    Console.WriteLine("Extracting Sonic Storybook texture archive.");
                    using (KnuxLib.Engines.Storybook.TextureDirectory txd = new(arg, true))
                    break;
                #endregion

                #region Sonic The Portable Engine formats.
                case ".amb":
                    Console.WriteLine("Extracting Sonic The Portable AMB archive.");
                    using (KnuxLib.Engines.Portable.AMB amb = new(arg, true))
                    break;
                #endregion

                #region Sonic World Adventure Wii Engine formats.
                case ".wap":
                    // If a version isn't specified, then ask the user which area points format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2)");
                        Console.WriteLine("    wii\t\t\t(Wii)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Sonic World Adventure Wii Engine Area Points Table to JSON.");
                    switch (version.ToLower())
                    {
                        case "ps2": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.PlayStation2, true)) break;
                        case "wii": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.Wii, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".worldadventurewii.areapoints.json":
                    // If a version isn't specified, then ask the user which area points format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    ps2\t\t\t(PlayStation 2)");
                        Console.WriteLine("    wii\t\t\t(Wii)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Sonic World Adventure Wii Engine Area Points Table.");
                    switch (version.ToLower())
                    {
                        case "ps2":
                            using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new())
                            {
                                areaPoints.Data = areaPoints.JsonDeserialise<List<KnuxLib.Engines.WorldAdventureWii.AreaPoints.Area>>(arg);
                                areaPoints.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wap", KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.PlayStation2);
                            }
                            break;

                        case "wii":
                            using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new())
                            {
                                areaPoints.Data = areaPoints.JsonDeserialise<List<KnuxLib.Engines.WorldAdventureWii.AreaPoints.Area>>(arg);
                                areaPoints.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wap", KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.Wii);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Wayforward Engine formats.
                case ".clb":
                    // If a version isn't specified, then ask the user which collision format this is.
                    if (string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine("This file has multiple variants that can't be auto detected, please specifiy the variant:");
                        Console.WriteLine("    hgh\t\t\t(Half-Genie Hero)");
                        Console.WriteLine("    ss\t\t\t(Seven Sirens)");

                        // Ask for the user's input.
                        Console.Write("\nFormat Type: ");

                        // Wait for the user's input.
                        version = Console.ReadLine().ToLower();

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(version))
                        {
                            Console.WriteLine("\nNo format type specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting Wayforward Engine collision to OBJ.");
                    switch (version.ToLower())
                    {
                        case "hgh": using (KnuxLib.Engines.Wayforward.Collision clb = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero, true)) break;
                        case "ss": using (KnuxLib.Engines.Wayforward.Collision clb = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported message table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;


                case ".env":
                    Console.WriteLine("Converting Wayforward Engine Environment Table to JSON.");
                    using (KnuxLib.Engines.Wayforward.Environment env = new(arg, true))
                    break;

                case ".wayforward.environment.json":
                    Console.WriteLine("Converting JSON to Wayforward Engine Environment Table.");
                    using (KnuxLib.Engines.Wayforward.Environment env = new())
                    {
                        env.Data = env.JsonDeserialise<KnuxLib.Engines.Wayforward.Environment.Entity[]>(arg);
                        env.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.env");
                    }
                    break;

                case ".lgb":
                    Console.WriteLine("Converting Wayforward Engine Layers Table to JSON.");
                    using (KnuxLib.Engines.Wayforward.Layers lgb = new(arg, true))
                    break;

                case ".wayforward.layers.json":
                    Console.WriteLine("Converting JSON to Wayforward Engine Layers Table.");
                    using (KnuxLib.Engines.Wayforward.Layers lgb = new())
                    {
                        lgb.Data = lgb.JsonDeserialise<List<string>>(arg);
                        lgb.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.lgb");
                    }
                    break;

                case ".ltb":
                    Console.WriteLine("Converting Wayforward Engine List Table to JSON.");
                    using (KnuxLib.Engines.Wayforward.ListTable ltb = new(arg, true))
                    break;

                case ".wayforward.listtable.json":
                    Console.WriteLine("Converting JSON to Wayforward Engine List Table.");
                    using (KnuxLib.Engines.Wayforward.ListTable ltb = new())
                    {
                        ltb.Data = ltb.JsonDeserialise<KnuxLib.Engines.Wayforward.ListTable.FormatData>(arg);
                        ltb.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ltb");
                    }
                    break;

                case ".wf3d":
                    Console.WriteLine("This format usually relies on a stage based .gpu file, please provide the path to the approriate .gpu file for this model:");

                    // Ask for the user's input.
                    Console.Write("\n.gpu file: ");

                    using (KnuxLib.Engines.Wayforward.Mesh mesh = new())
                    {
                        mesh.Load(arg);
                        mesh.ExportOBJTemp($@"{Path.ChangeExtension(arg, ".obj")}", Console.ReadLine().Replace("\"", ""));
                    }
                    break;

                case ".pak":
                    Console.WriteLine("Extracting Wayforward Engine package archive.");
                    using (KnuxLib.Engines.Wayforward.Package pak = new(arg, true))
                    break;
                #endregion

                #region Westwood Engine Formats.
                case ".tre":
                case ".tru":
                    Console.WriteLine("Converting Westwood Engine Message Table to JSON.");
                    using (KnuxLib.Engines.Westwood.MessageTable messageTable = new(arg, true))
                    break;

                case ".westwood.messagetable.json":
                    // If an extension isn't specified, then ask the user which point cloud extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .tre (USA)\n" +
                            "2. .tru (UK)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "tre"; break;
                            case '2': extension = "tru"; break;
                        }

                        // Sanity check the input, abort if its still null or empty.
                        if (string.IsNullOrEmpty(extension))
                        {
                            Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Westwood Engine Message Table.");
                    using (KnuxLib.Engines.Westwood.MessageTable messageTable = new())
                    {
                        messageTable.Data = messageTable.JsonDeserialise<List<KnuxLib.Engines.Westwood.MessageTable.Message>>(arg);
                        messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                    }
                    break;
                #endregion
            }
        }
    }
}