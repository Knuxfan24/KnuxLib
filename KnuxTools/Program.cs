using System.Globalization;
using System.Numerics;
using System.Text;

namespace KnuxTools
{
    // TODO: Make a version of the NoVersionChecker function for extensions so I stop duping it?
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

                // Tell the user we're done.
                Console.WriteLine("Done!");
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
                ColourConsole("Assets Container Archive Pair (.gfc/.gob) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("Collision (.hke) - Converts to a set of OBJs in a directory with the same name as the input file (importing and saving not yet possible).\n");

                Console.WriteLine("Engine Black:");
                ColourConsole("Data Archive (.data) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("Volume Blob (.vol) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Capcom MT Framework Engine:");
                Console.WriteLine("Archive (.arc) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag (importing) - capcomv7", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (importing) - capcomv9", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (importing) - capcomv9_uncompressed\n", true, ConsoleColor.Yellow);

                Console.WriteLine("CarZ Engine:");
                Console.WriteLine("Material Library (.mat) - Exports to the MTL material library standard and imports from an Assimp compatible model.");
                Console.WriteLine("3D Model (.sco) - Exports to the Wavefront OBJ model standard and imports from an Assimp compatible model.");
                ColourConsole("    Version Flag (importing, shared with material library) - carz\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Cobra Engine:");
                ColourConsole("Zap Archive (.zap) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Crash6 Engine:");
                ColourConsole("Data Header Pair (.bh/.bd) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Criware:");
                ColourConsole("Archive File System (.afs) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Flipnic Engine:");
                ColourConsole("Binary Archive (.bin) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Gods Engine:");
                ColourConsole("WAD Archive (.wad) - Extracts to a directory of the same name as the input archive (importing not yet possible).");
                ColourConsole("    Version Flag (Ninjabread Man (PC/PS2)) - ninjabread_pc", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Ninjabread Man (Wii)) - ninjabread_wii\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Hasbro Wii Engine:");
                Console.WriteLine("Big File Archive (.big) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag (importing) - hasbro_big", true, ConsoleColor.Yellow);
                ColourConsole("X Package (.xpac) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Hedgehog Engine:");
                Console.WriteLine("Archive Info (.arcinfo)");
                Console.WriteLine("Bullet Skeleton (.skl.pxd)");
                ColourConsole("Cloud Instance");
                ColourConsole("2010 Collision (.orc) - Converts to an OBJ with the same name as the input file (importing and saving not yet possible). ");
                ColourConsole("Rangers Collision (.btmesh) - Converts to an OBJ with the same name as the input file (importing and saving not yet possible, point cloud instancing is also ignored).");
                ColourConsole("Rangers Skinned Collision (.btsmc) - Converts to an OBJ with the same name as the input file (importing not yet possible).");
                Console.WriteLine("Density Point Cloud (.densitypointcloud)");
                Console.WriteLine("Density Setting (.densitysetting)");
                Console.WriteLine("Gismo V3 (.gismod/.gismop)");
                ColourConsole("Grass Instance (.grass.bin)");
                Console.WriteLine("Instance Info (.terrain-instanceinfo) - Import a folder containing files to generate a Sonic Frontiers point cloud file. ");
                ColourConsole("    Version Flag (point cloud conversion) - hh_instance2pointcloud", true, ConsoleColor.Yellow);
                Console.WriteLine("Light Field (Rangers) (.lf)");
                Console.WriteLine("2010 Map (.map.bin)");
                Console.WriteLine("Master Level Table (.mlevel)");
                Console.WriteLine("Message Table (sonic2010/blueblur/william) (.xtb)");
                ColourConsole("    Version Flag (Sonic Colours) - sonic2010", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic Generations) - blueblur", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Mario and Sonic at the London 2012 Olympic Games) - william", true, ConsoleColor.Yellow);
                Console.WriteLine("Message Table (sonic2013) (.xtb2)");
                Console.WriteLine("Path Spline (.path) (.path2.bin)");
                ColourConsole("    Version Flag (Sonic Lost World) - sonic2013", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic Forces) - wars", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic Frontiers) - rangers", true, ConsoleColor.Yellow);
                Console.WriteLine("Point Cloud (.pccol/.pcmodel/.pcrt)");
                ColourConsole("    Extension Flag (Collision Instance) - pccol", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Terrain Instance) - pcmodel", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Lighting Instance) - pcrt", true, ConsoleColor.Yellow);
                ColourConsole("Scene Effect Collision (.fxcol.bin)");
                Console.WriteLine("Sector Visibility Collision (.svcol.bin)");
                Console.WriteLine("Terrain Material (.terrain-material)\n");

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

                Console.WriteLine("OpenSpace Engine:");
                Console.WriteLine("Big File Archive (.bf/.dsc) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - openspace_big\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Project M Engine:");
                Console.WriteLine("Message Table (.dat) \n");

                Console.WriteLine("Rockman X7 Engine:");
                ColourConsole("Stage Entity Table (.328f438b/.osd)");
                ColourConsole("    Version Flag (Final) - final", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Preview Trial) - preview", true, ConsoleColor.Yellow);
                ColourConsole("SLD Spline (.sld)\n");

                Console.WriteLine("Rockman X8 Engine:");
                ColourConsole("Stage Entity Table (.31bf570e/.set)");
                ColourConsole("    Version Flag - rockx8", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Original) - SET", true, ConsoleColor.Yellow);
                ColourConsole("    Extension Flag (Legacy Collection 2) - 31BF570E\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic Storybook Engine:");
                Console.WriteLine("Light Field (.bin)");
                ColourConsole("    Version Flag - storybook_lightfield", true, ConsoleColor.Yellow);
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
                ColourConsole("    Version Flag (Sonic and the Secret Rings) - storybook_set_sr", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic and the Black Knight) - storybook_set_bk", true, ConsoleColor.Yellow);
                Console.WriteLine("Stage Entity Table Object Table (.bin)");
                ColourConsole("    Version Flag (Sonic and the Secret Rings) - storybook_setitems_sr", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic and the Black Knight) - storybook_setitems_bk", true, ConsoleColor.Yellow);
                Console.WriteLine("Texture Directory (.txd) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - storybook_texture", true, ConsoleColor.Yellow);
                ColourConsole("Visibility Table (.bin)");
                ColourConsole("    Version Flag (Sonic and the Secret Rings) - storybook_visibility_sr", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Sonic and the Black Knight) - storybook_visibility_bk\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic The Portable Engine:");
                Console.WriteLine("AMB Archive (.amb) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag - portable", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Big Endian, used by the Wii) - portable_big-endian\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Sonic World Adventure Wii Engine:");
                Console.WriteLine("Area Points Table (.wap)");
                ColourConsole("    Version Flag (PlayStation 2) - ps2", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Wii) - wii", true, ConsoleColor.Yellow);
                Console.WriteLine("ONE Archive (.one/.onz) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.");
                ColourConsole("    Version Flag (Importing as .one) - swawii", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Importing as .onz) - swawii_compressed", true, ConsoleColor.Yellow);
                ColourConsole("Stage Entity Table (.set) - Converts to a HSON with the same name as the input file (importing and saving not yet possible).");
                ColourConsole("    Version Flag (PlayStation 2) - ps2", true, ConsoleColor.Yellow);
                ColourConsole("    Version Flag (Wii) - wii\n", true, ConsoleColor.Yellow);

                Console.WriteLine("Space Channel Engine:");
                Console.WriteLine("Caption Table (.bin)\n");

                Console.WriteLine("Wayforward Engine:");
                ColourConsole("Collision (.clb) - Converts to an OBJ format and imports from an Assimp compatible model.");
                ColourConsole("    Version Flag (Ducktales Remastered) - wayforward_collision_duck", true, ConsoleColor.Yellow);
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

                Console.WriteLine("Yacht Club Engine:");
                Console.WriteLine("Package Archive (.pak) - Extracts to a directory of the same name as the input archive and creates a (currently broken) archive from an input directory.");
                ColourConsole("    Version Flag - yachtclub", true, ConsoleColor.Yellow);
                Console.WriteLine("String Translation Table (.stl)\n");

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
            // Check for an archive type identifier if a version wasn't specified, set the version if one was found.
            if (string.IsNullOrEmpty(version) && File.Exists($@"{arg}\knuxtools_archivetype.txt"))
                version = File.ReadAllText($@"{arg}\knuxtools_archivetype.txt");

            // Carry out a version check.
            version = NoVersionChecker(version,
                                       "Please specify the archive type to pack this directory into, valid options are:",
                                       new List<string> { "capcomv7\t\t\t(Capcom MT Framework Engine (Version 7))",
                                                          "capcomv9\t\t\t(Capcom MT Framework Engine (Version 9))",
                                                          "capcomv9_uncompressed\t(Capcom MT Framework Engine (Version 9, No Compression))",
                                                          "hasbro_big\t\t\t(Hasbro Wii Engine Big File Archive)",
                                                          "hh_instance2pointcloud\t(Convert Hedgehog Engine Terrain Instances into a Hedgehog Engine Point Cloud)",
                                                          "openspace_big\t\t(OpenSpace Engine Big File Archive)",
                                                          "nights2\t\t\t(NiGHTS 2 Engine ONE File)",
                                                          "portable\t\t\t(Sonic The Portable Engine AMB File)",
                                                          "portable_big-endian\t\t(Sonic The Portable Engine AMB File)",
                                                          "storybook\t\t\t(Sonic Storybook Engine ONE File)",
                                                          "storybook_texture\t\t(Sonic Storybook Engine TXD File)",
                                                          "swawii\t\t\t(Sonic World Adventure Wii Engine ONE File)",
                                                          "swawii_compressed\t\t(Sonic World Adventure Wii Engine Compressed ONZ File)",
                                                          "wayforward\t\t\t(Wayforward Engine PAK File)",
                                                          "yachtclub\t\t\t(Yacht Club Engine PAK File)"},
                                       new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false, false, false, true },
                                       "Archive Type");

            // If the version is still null or empty, then abort.
            if (string.IsNullOrEmpty(version))
                return;

            // Decide what to do based on the version value.
            // In most cases this will be writing a line for user feedback then running the ImportAndSaveArchive with the right type and extension.
            switch (version.ToLower())
            {
                // Capcom MT Framework Arc Archives.
                case "capcomv7":
                    Console.WriteLine("Packing directory for Capcom MT Framework Engine.");
                    using (KnuxLib.Engines.CapcomMT.Archive arc = new())
                    {
                        arc.Import(arg);
                        arc.Save($@"{arg}.arc", 0x07, true);
                    }
                    break;
                case "capcomv9":
                    Console.WriteLine("Packing directory for Capcom MT Framework Engine.");
                    using (KnuxLib.Engines.CapcomMT.Archive arc = new())
                    {
                        arc.Import(arg);
                        arc.Save($@"{arg}.arc", 0x09, true);
                    }
                    break;
                case "capcomv9_uncompressed":
                    Console.WriteLine("Packing directory for Capcom MT Framework Engine.");
                    using (KnuxLib.Engines.CapcomMT.Archive arc = new())
                    {
                        arc.Import(arg);
                        arc.Save($@"{arg}.arc", 0x09, false);
                    }
                    break;

                // Hasbro Wii Engine Big File Archives.
                case "hasbro_big":
                    Console.WriteLine("Packing directory for Hasbro Wii Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.HasbroWii.BigFileArchive), arg, "big");
                    break;

                // Hedgehog Engine Terrain Instance Conversion.
                case "hh_instance2pointcloud":
                    Console.WriteLine("Converting Hedgehog Engine Terrain Instance Info files to Hedgehog Engine Point Cloud files.");
                    KnuxLib.Engines.Hedgehog.InstanceInfo.ConvertDirectoryToPointCloud(arg);
                    break;

                // NiGHTS 2 Engine ONE Archives.
                case "nights2":
                    Console.WriteLine("Packing directory for NiGHTS 2 Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.NiGHTS2.ONE), arg, "one");
                    break;

                // OpenSpace Engine Big File Archives.
                case "openspace_big":
                    Console.WriteLine("Packing directory for OpenSpace Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.OpenSpace.BigFileArchive), arg, "BF");
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
                case "portable_big-endian":
                    Console.WriteLine("Packing directory for Sonic The Portable Engine.");
                    using (KnuxLib.Engines.Portable.AMB amb = new())
                    {
                        amb.Import(arg);
                        amb.bigEndian = true;
                        amb.Save($@"{arg}.amb");
                    }
                    break;

                // Sonic World Adventure Wii ONE Archives.
                case "swawii":
                    Console.WriteLine("Packing directory for Sonic World Adventure Wii Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.WorldAdventureWii.ONE), arg, "one");
                    break;

                case "swawii_compressed":
                    Console.WriteLine("Packing directory for Sonic World Adventure Wii Engine.");
                    using (KnuxLib.Engines.WorldAdventureWii.ONE one = new())
                    {
                        one.Import(arg);
                        one.Save($@"{arg}.onz", true);
                    }
                    break;

                // Wayforward Engine Packages.
                case "wayforward":
                    Console.WriteLine("Packing directory for Wayforward Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.Wayforward.Package), arg, "pak");
                    break;

                // Yacht Club Engine Packages.
                case "yachtclub":
                    Console.WriteLine("Packing directory for Yacht Club Engine.");
                    ImportAndSaveArchive(typeof(KnuxLib.Engines.YachtClub.Package), arg, "pak");
                    break;

                // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                default:
                    Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported archive types.\nPress any key to continue.");
                    Console.ReadKey();
                    return;
            }
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
            // Determine the file type that the user has passed.
            switch (KnuxLib.Helpers.GetExtension(arg).ToLower())
            {
                #region Generic Seralised Models.
                case ".fbx":
                case ".dae":
                case ".obj":
                    // Carry out a version check.
                    // TODO: This is dumb.
                    if (KnuxLib.Helpers.GetExtension(arg).ToLower() == ".obj")
                    version = NoVersionChecker(version,
                                               "This file is a generic model, please specifiy what format to import and save it as:",
                                               new List<string> { "carz\t\t\t(CarZ Engine SCO model & MAT material library)",
                                                                  "sonic2013\t\t\t(Hedgehog Engine Path Spline for Sonic Lost World)",
                                                                  "wars\t\t\t(Hedgehog Engine Path Spline for Sonic Forces)",
                                                                  "rangers\t\t\t(Hedgehog Engine Path Spline for Sonic Frontiers)",
                                                                  "wayforward\t\t\t(Wayforward Engine WF3D Mesh)",
                                                                  "wayforward_collision_duck\t(Wayforward Engine Collision for Ducktales Remastered)",
                                                                  "wayforward_collision_hgh\t(Wayforward Engine Collision for Half-Genie Hero)",
                                                                  "wayforward_collision_ss\t(Wayforward Engine Collision for Seven Sirens)"},
                                               new List<bool> { false, false, false, false, true, true, true, true });
                    else
                    version = NoVersionChecker(version,
                                               "This file is a generic model, please specifiy what format to import and save it as:",
                                               new List<string> { "carz\t\t\t(CarZ Engine SCO model & MAT material library)",
                                                                  "wayforward\t\t\t(Wayforward Engine WF3D Mesh)",
                                                                  "wayforward_collision_duck\t(Wayforward Engine Collision for Ducktales Remastered)",
                                                                  "wayforward_collision_hgh\t(Wayforward Engine Collision for Half-Genie Hero)",
                                                                  "wayforward_collision_ss\t(Wayforward Engine Collision for Seven Sirens)"},
                                               new List<bool> { false, true, true, true, true });
                    
                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

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

                        // Hedgehog Engine Path Splines (Sonic_2013).
                        case "sonic2013":
                            if (KnuxLib.Helpers.GetExtension(arg).ToLower() == ".obj")
                            {
                                Console.WriteLine("Converting OBJ to Hedgehog Engine (Sonic_2013) Path Spline.");
                                using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline = new())
                                {
                                    pathSpline.ImportOBJ(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013);
                                    pathSpline.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.path2.bin", KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported model types.\nPress any key to continue.");
                                Console.ReadKey();
                            }
                            break;

                        // Hedgehog Engine Path Splines (Wars).
                        case "wars":
                            if (KnuxLib.Helpers.GetExtension(arg).ToLower() == ".obj")
                            {
                                Console.WriteLine("Converting OBJ to Hedgehog Engine (Wars) Path Spline.");
                                using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline = new())
                                {
                                    pathSpline.ImportOBJ(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars);
                                    pathSpline.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.path", KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported model types.\nPress any key to continue.");
                                Console.ReadKey();
                            }
                            break;

                        // Hedgehog Engine Path Splines (Rangers).
                        case "rangers":
                            if (KnuxLib.Helpers.GetExtension(arg).ToLower() == ".obj")
                            {
                                Console.WriteLine("Converting OBJ to Hedgehog Engine (Rangers) Path Spline.");
                                using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline = new())
                                {
                                    pathSpline.ImportOBJ(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Rangers);
                                    pathSpline.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.path", KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Rangers);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported model types.\nPress any key to continue.");
                                Console.ReadKey();
                            }
                            break;

                        // Wayforward Engine Meshes.
                        // TODO: I really don't like this code, but it'll do for now.
                        case "wayforward":
                            // Tell the user about the need for a .gpu file.
                            Console.WriteLine("Wayforward Meshes need a level .gpu file.\nPlease specify the filepath for the desired .gpu file to inject this model's data into.");
                            
                            // Ask for the user's input.
                            Console.Write("\n.gpu file path: ");

                            // Read the gpu path from the user's input.
                            string gpuPath = Console.ReadLine().Replace("\"", "");

                            // Check if the .gpu file exists.
                            if (File.Exists(gpuPath))
                            {
                                // Tell the user about the need for a vertex table index.
                                Console.WriteLine("Wayforward Meshes need a vertex table index.\nPlease specify a numerical value for this mesh's vertex table(s).\nIf none is provided then 155225170944 (0x2424242400) will be used as a default.");

                                // Ask for the user's input.
                                Console.Write("\nVertex Table Index: ");

                                // Read the vertex table index from the user's input.
                                if (!ulong.TryParse(Console.ReadLine(), out ulong vertexTableIndex))
                                    vertexTableIndex = 0x2424242400;

                                // Tell the user about the need for a face table index.
                                Console.WriteLine("\nWayforward Meshes need a face table index.\nPlease specify a numerical value for this mesh's face table(s).\nIf none is provided then 39737643761664 (0x242424240000) will be used as a default.");

                                // Ask for the user's input.
                                Console.Write("\nFace Table Index: ");

                                // Read the face table index from the user's input.
                                if (!ulong.TryParse(Console.ReadLine(), out ulong faceTableIndex))
                                    faceTableIndex = 0x242424240000;

                                // Backup the original gpu file.
                                File.Copy(gpuPath, $"{gpuPath}.original", true);

                                // Create the wf3d file and inject the data into the gpu file.
                                using (KnuxLib.Engines.Wayforward.Mesh mesh = new())
                                    mesh.ImportAssimp(arg, Path.ChangeExtension(arg, ".wf3d"), gpuPath, vertexTableIndex, faceTableIndex);
                            }

                            // If the .gpu file doesn't exist, then inform the user and abort.
                            else
                            {
                                Console.WriteLine($"Couldn't find {gpuPath}! Aborting...\nPress any key to continue.");
                                Console.ReadKey();
                                return;
                            }
                            break;

                        // Wayforward Engine Collision (Ducktales Remastered Version).
                        case "wayforward_collision_duck":
                            Console.WriteLine("Converting model to a Wayforward Engine collision file.");
                            using (KnuxLib.Engines.Wayforward.Collision collision = new())
                            {
                                collision.ImportAssimp(arg);
                                collision.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.clb", KnuxLib.Engines.Wayforward.Collision.FormatVersion.duck);
                            }
                            break;

                        // Wayforward Engine Collision (Shantae: Half-Genie Hero Version).
                        case "wayforward_collision_hgh":
                            Console.WriteLine("Converting model to a Wayforward Engine collision file.");
                            using (KnuxLib.Engines.Wayforward.Collision collision = new())
                            {
                                collision.ImportAssimp(arg);
                                collision.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.clb", KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero);
                            }
                            break;

                        // Wayforward Engine Collision (Shantae and the Seven Sirens Version).
                        case "wayforward_collision_ss":
                            Console.WriteLine("Converting model to a Wayforward Engine collision file.");
                            using (KnuxLib.Engines.Wayforward.Collision collision = new())
                            {
                                collision.ImportAssimp(arg);
                                collision.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.clb", KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported model types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }

                    break;
                #endregion

                #region Generic File Formats.
                case ".bin":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "flipnic\t\t\t(Flipnic Binary Archive File)",
                                                                  "storybook_lightfield\t(Sonic Storybook Engine Light Field File)",
                                                                  "storybook_motion\t\t(Sonic Storybook Engine Player Motion File)",
                                                                  "storybook_set_sr\t\t(Sonic Storybook Engine Stage Entity Table File (Secret Rings)",
                                                                  "storybook_set_bk\t\t(Sonic Storybook Engine Stage Entity Table File (Black Knight)",
                                                                  "storybook_setitems_sr\t(Sonic Storybook Engine Stage Entity Table Object Table File (Secret Rings))",
                                                                  "storybook_setitems_bk\t(Sonic Storybook Engine Stage Entity Table Object Table File (Black Knight))",
                                                                  "storybook_visibility_sr\t(Sonic Storybook Engine Visibility Table File (Secret Rings))",
                                                                  "storybook_visibility_bk\t(Sonic Storybook Engine Visibility Table File (Black Knight))",
                                                                  "spacechannel_caption\t(Space Channel Engine Caption Table)"},
                                               new List<bool> { true, false, true, true, true, false, false, true, true, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        // Flipnic Engine Binary Archives.
                        case "flipnic":
                            Console.WriteLine("Extracting Flipnic Engine binary archive.");
                            using (KnuxLib.Engines.Flipnic.BinaryArchive bin = new(arg, true))
                            break;

                        // Sonic Storybook Engine Player Light Field.
                        case "storybook_lightfield":
                            Console.WriteLine("Converting Sonic Storybook Engine Light Field to JSON.");
                            using (KnuxLib.Engines.Storybook.LightField lightField = new(arg, true))
                            break;

                        // Sonic Storybook Engine Motion Tables.
                        case "storybook_motion":
                            Console.WriteLine("Converting Sonic Storybook Engine Motion Table to JSON.");
                            using (KnuxLib.Engines.Storybook.MotionTable motion = new(arg, true))
                            break;

                        // Sonic Storybook Stage Entity Tables (Sonic and the Secret Rings Version).
                        case "storybook_set_sr":
                            // Check that the template sheet exists. Inform the user and abort if its missing.
                            if (!File.Exists($@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\secret_rings.json"))
                            {
                                Console.WriteLine("\nsecret_rings.json not found in the HSON Templates directory! Aborting...\nPress any key to continue.");
                                Console.ReadKey();
                                return;
                            }

                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table to HSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTable set = new(arg, $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\secret_rings.json"))
                            {
                                set.ExportHSON(Path.ChangeExtension(arg, ".hson"),
                                               $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\secret_rings.json",
                                               Path.GetFileNameWithoutExtension(arg),
                                               Environment.UserName,
                                               $"Sonic and the Secret Rings set file {Path.GetFileName(arg)} converted to the HSON format.");
                            }
                            break;

                        // Sonic Storybook Stage Entity Tables (Sonic and the Black Knight Version).
                        case "storybook_set_bk":
                            // Check that the template sheet exists. Inform the user and abort if its missing.
                            if (!File.Exists($@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\black_knight.json"))
                            {
                                Console.WriteLine("\nblack_knight.json not found in the HSON Templates directory! Aborting...\nPress any key to continue.");
                                Console.ReadKey();
                                return;
                            }

                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table to HSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTable set = new(arg, $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\black_knight.json"))
                            {
                                set.ExportHSON(Path.ChangeExtension(arg, ".hson"),
                                               $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\black_knight.json",
                                               Path.GetFileNameWithoutExtension(arg),
                                               Environment.UserName,
                                               $"Sonic and the Black Knight set file {Path.GetFileName(arg)} converted to the HSON format.");
                            }
                            break;

                        // Sonic Storybook Stage Entity Table Items Tables (Sonic and the Secret Rings Version).
                        case "storybook_setitems_sr":
                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table Object Table to JSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings, true))
                            break;

                        // Sonic Storybook Stage Entity Table Items Tables (Sonic and the Black Knight Version).
                        case "storybook_setitems_bk":
                            Console.WriteLine("Converting Sonic Storybook Engine Stage Entity Table Object Table to JSON.");
                            using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight, true))
                            break;

                        // Sonic Storybook Visibility Tables (Sonic and the Secret Rings Version).
                        case "storybook_visibility_sr":
                            Console.WriteLine("Converting Sonic Storybook Visibility Table to JSON.");
                            using (KnuxLib.Engines.Storybook.VisibilityTable visibilityTable = new(arg, KnuxLib.Engines.Storybook.VisibilityTable.FormatVersion.SecretRings, true))
                            break;

                        // Sonic Storybook Visibility Tables (Sonic and the Black Knight Version).
                        case "storybook_visibility_bk":
                            Console.WriteLine("Converting Sonic Storybook Visibility Table to JSON.");
                            using (KnuxLib.Engines.Storybook.VisibilityTable visibilityTable = new(arg, KnuxLib.Engines.Storybook.VisibilityTable.FormatVersion.BlackKnight, true))
                            break;

                        case "spacechannel_caption":
                            // Reset the version indicator.
                            version = null;

                            // Carry out a version check.
                            version = NoVersionChecker(version,
                                                       "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                                       new List<string> { "international\t(English and other languages)",
                                                                          "japanese\t\t(Japanese)"},
                                                       new List<bool> { false, false });

                            // If the version is still null or empty, then abort.
                            if (string.IsNullOrEmpty(version))
                                return;

                            Console.WriteLine("Converting Space Channel Engine Caption Table to JSON.");

                            // Decide what to do based on the version value.
                            switch (version.ToLower())
                            {
                                case "international":
                                    using (KnuxLib.Engines.SpaceChannel.CaptionTable captionTable = new(arg, false, true))
                                        break;

                                case "japanese":
                                    using (KnuxLib.Engines.SpaceChannel.CaptionTable captionTable = new(arg, true, true))
                                        break;

                                // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                                default:
                                    Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Space Channel Caption Table types.\nPress any key to continue.");
                                    Console.ReadKey();
                                    return;
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported .bin file types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".one":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "nights2\t\t(NiGHTS 2 Engine ONE File)",
                                                                  "storybook\t\t(Sonic Storybook Engine ONE File)",
                                                                  "swawii\t\t(Sonic World Adventure Wii Engine ONE File)"},
                                               new List<bool> { false, false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        // NiGHTS 2 Engine Archives.
                        case "nights2":
                            Console.WriteLine("Extracting NiGHTS2 Engine archive.");
                            using (KnuxLib.Engines.NiGHTS2.ONE one = new(arg, true))
                            break;

                        // Sonic Storybook Engine Archives.
                        case "storybook":
                            Console.WriteLine("Extracting Sonic Storybook Engine archive.");
                            using (KnuxLib.Engines.Storybook.ONE one = new(arg, true))
                            break;

                        // Sonic World Adventure Wii Engine Archives.
                        case "swawii":
                            Console.WriteLine("Extracting Sonic World Adventure Wii Engine archive.");
                            using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported .one archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".pak":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "wayforward\t\t(Wayforward Engine PAK File)",
                                                                  "yachtclub\t\t(Yacht Club Engine PAK File)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        // Wayforward Engine Archives.
                        case "wayforward":
                            Console.WriteLine("Extracting Wayforward Engine package archive.");
                            using (KnuxLib.Engines.Wayforward.Package pak = new(arg, true))
                                break;

                        // Sonic Storybook Engine Archives.
                        case "yachtclub":
                            Console.WriteLine("Extracting Yacht Club Engine package archive.");
                            using (KnuxLib.Engines.YachtClub.Package pak = new(arg, true))
                                break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported .one archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".set":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "rockx8\t(Rockman X8 Engine Stage Entity Table File)",
                                                                  "swa_wii\t(Sonic World Adventure Wii Stage Entity Table File (Wii))",
                                                                  "swa_ps2\t(Sonic World Adventure Wii Stage Entity Table File (PS2))"},
                                               new List<bool> { true, true, true });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        // Rockman X8 Engine Stage Entity Tables.
                        case "rockx8":
                            Console.WriteLine("Converting Rockman X8 Engine Stage Entity Table to JSON.");
                            using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new(arg, KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.Original, true))
                            break;

                        // Sonic World Adventure Wii Stage Entity Tables (Wii Version).
                        case "swa_wii":
                            // Check that the template sheet exists. Inform the user and abort if its missing.
                            if (!File.Exists($@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json"))
                            {
                                Console.WriteLine("\nswa_wii.json not found in the HSON Templates directory! Aborting...\nPress any key to continue.");
                                Console.ReadKey();
                                return;
                            }

                            Console.WriteLine("Converting Sonic World Adventure Wii Engine Stage Entity Table to HSON.");
                            using (KnuxLib.Engines.WorldAdventureWii.StageEntityTable set = new(arg, $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json", KnuxLib.Engines.WorldAdventureWii.StageEntityTable.FormatVersion.Wii))
                            {
                                set.ExportHSON(Path.ChangeExtension(arg, ".hson"),
                                               $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json",
                                               Path.GetFileNameWithoutExtension(arg),
                                               Environment.UserName,
                                               $"Sonic Unleashed (Wii) set file {Path.GetFileName(arg)} converted to the HSON format.");
                            }
                            break;

                        // Sonic World Adventure Wii Stage Entity Tables (PS2 Version).
                        case "swa_ps2":
                            // Check that the template sheet exists. Inform the user and abort if its missing.
                            if (!File.Exists($@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json"))
                            {
                                Console.WriteLine("\nswa_wii.json not found in the HSON Templates directory! Aborting...\nPress any key to continue.");
                                Console.ReadKey();
                                return;
                            }

                            Console.WriteLine("Converting Sonic World Adventure Wii Engine Stage Entity Table to HSON.");
                            using (KnuxLib.Engines.WorldAdventureWii.StageEntityTable set = new(arg, $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json", KnuxLib.Engines.WorldAdventureWii.StageEntityTable.FormatVersion.PlayStation2))
                            {
                                set.ExportHSON(Path.ChangeExtension(arg, ".hson"),
                                               $@"{Path.GetDirectoryName(System.AppContext.BaseDirectory)}\HSON Templates\swa_wii.json",
                                               Path.GetFileNameWithoutExtension(arg),
                                               Environment.UserName,
                                               $"Sonic Unleashed (PS2) set file {Path.GetFileName(arg)} converted to the HSON format.");
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported .set file types.\nPress any key to continue.");
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

                #region Capcom MT Framework Engine formats.
                case ".arc":
                    try
                    {
                        Console.WriteLine("Extracting Capcom MT Framework Engine archive.");
                        using KnuxLib.Engines.CapcomMT.Archive arc = new(arg, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e}\n\nAttempting to extract as a Marathon Engine archive instead.");
                        using (Marathon.Formats.Archive.U8Archive arc = new(arg, Marathon.IO.ReadMode.IndexOnly))
                        {
                            arc.Extract($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}");
                        }
                    }
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

                #region Cobra Engine formats.
                case ".zap":
                    Console.WriteLine("Extracting Cobra Engine Zap Archive.");
                    using (KnuxLib.Engines.Cobra.ZapArchive zap = new(arg, true))
                    break;
                #endregion

                #region Crash6 Engine formats.
                case ".bd":
                case ".bh":
                    Console.WriteLine("Extracting Crash6 Engine Data Header Pair.");
                    using (KnuxLib.Engines.Crash6.DataHeaderPair dataHeader = new(arg, true))
                    break;
                #endregion

                #region Criware formats.
                case ".afs":
                    Console.WriteLine("Extracting Criware Archive File System.");
                    using (KnuxLib.Engines.Criware.ArchiveFileSystem afs = new(arg, true))
                    break;
                #endregion

                #region GODS Engine formats.
                case ".wad":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "ninjabread_pc\t(Ninjabread Man (PC/PS2))",
                                                                  "ninjabread_wii\t(Ninjabread Man (Wii))"},
                                               new List<bool> { true, true });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    // Decide what to do based on the version value.
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

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported .wad archive types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Hasbro Wii Engine formats.
                case ".big":
                    Console.WriteLine("Extracting Hasbro Wii Engine Big File Archive.");
                    using (KnuxLib.Engines.HasbroWii.BigFileArchive big = new(arg, true))
                        break;

                case ".xpac":
                    Console.WriteLine("Extracting Hasbro Wii Engine X Package.");
                    using (KnuxLib.Engines.HasbroWii.XPackage xPac = new(arg, true))
                        break;
                #endregion

                #region Hedgehog Engine formats.
                case "":
                    Console.WriteLine("Converting Hedgehog Engine Cloud Instance to JSON.");
                    using (KnuxLib.Engines.Hedgehog.Cloud cloud = new(arg, true))
                    break;

                case ".hedgehog.cloud.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Cloud Instance.");
                    using (KnuxLib.Engines.Hedgehog.Cloud cloud = new())
                    {
                        cloud.Data = cloud.JsonDeserialise<KnuxLib.Engines.Hedgehog.Cloud.FormatData>(arg);
                        cloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}");
                    }
                    break;

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

                case ".btmesh":
                    Console.WriteLine("Converting Hedgehog Engine Rangers Collision to OBJ.");
                    using (KnuxLib.Engines.Hedgehog.Collision_Rangers collision = new(arg, true))
                    break;

                case ".btsmc":
                    Console.WriteLine("Converting Hedgehog Engine Skinned Rangers Collision to OBJ.");
                    using (KnuxLib.Engines.Hedgehog.SkinnedCollision_Rangers skinnedCollision = new(arg, true))
                    break;

                case ".densitypointcloud":
                    Console.WriteLine("Converting Hedgehog Engine Density Point Cloud to JSON.");
                    using (KnuxLib.Engines.Hedgehog.DensityPointCloud densityPointCloud = new(arg, true))
                    break;

                case ".hedgehog.densitypointcloud.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Density Point Cloud.");
                    using (KnuxLib.Engines.Hedgehog.DensityPointCloud densityPointCloud = new())
                    {
                        densityPointCloud.Data = densityPointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.DensityPointCloud.Instance>>(arg);
                        densityPointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.densitypointcloud");
                    }
                    break;

                case ".densitysetting":
                    Console.WriteLine("Converting Hedgehog Engine Density Setting to JSON.");
                    using (KnuxLib.Engines.Hedgehog.DensitySetting densitySetting = new(arg, true))
                        break;

                case ".hedgehog.densitysetting.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Density Setting.");
                    using (KnuxLib.Engines.Hedgehog.DensitySetting densitySetting = new())
                    {
                    }
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

                case ".grass.bin":
                    Console.WriteLine("Converting Hedgehog Engine Grass Instance to JSON.");
                    using (KnuxLib.Engines.Hedgehog.GrassInstance grassInstance = new(arg, true))
                        break;

                case ".hedgehog.grassinstance.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Grass Instance.");
                    using (KnuxLib.Engines.Hedgehog.GrassInstance grassInstance = new())
                    {
                        grassInstance.Data = grassInstance.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.GrassInstance.Grass>>(arg);
                        grassInstance.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.grass.bin");
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

                case ".map.bin":
                    Console.WriteLine("Converting Hedgehog Engine 2010 Map to JSON.");
                    using (KnuxLib.Engines.Hedgehog.Map_2010 map = new(arg, true))
                    break;

                case ".hedgehog.map_2010.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine 2010 Map Table.");
                    using (KnuxLib.Engines.Hedgehog.Map_2010 map = new())
                    {
                        map.Data = map.JsonDeserialise<KnuxLib.Engines.Hedgehog.Map_2010.FormatData>(arg);
                        map.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.map.bin");
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
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "sonic2010\t(Sonic Colours)",
                                                                  "blueblur\t(Sonic Generations)",
                                                                  "william\t(Mario and Sonic at the London 2012 Olympic Games)"},
                                               new List<bool> { false, false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Hedgehog Engine 2010 Message Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "sonic2010": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.sonic_2010, true)) break;
                        case "blueblur": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur, true)) break;
                        case "william": using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Hedgehog Engine 2010 Message Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".hedgehog.messagetable_2010.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "sonic2010\t(Sonic Colours)",
                                                                  "blueblur\t(Sonic Generations)",
                                                                  "william\t(Mario and Sonic at the London 2012 Olympic Games)"},
                                               new List<bool> { false, false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Hedgehog Engine 2010 Message Table.");

                    // Decide what to do based on the version value.
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

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Hedgehog Engine 2010 Message Table types.\nPress any key to continue.");
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
                        messageTable_2013.Data = messageTable_2013.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2013.Sheet[]>(arg);
                        messageTable_2013.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb2");
                    }
                    break;

                case ".path2.bin":
                    Console.WriteLine("Extracting Hedgehog Engine Path Spline to OBJ.");
                    using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline_WarsRangers = new(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013, true))
                    break;

                case ".path":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "wars\t(Sonic Forces)",
                                                                  "rangers\t(Sonic Frontiers)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Extracting Hedgehog Engine Path Spline to OBJ.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "wars": using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline_WarsRangers = new(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars, true)) break;
                        case "rangers": using (KnuxLib.Engines.Hedgehog.PathSpline pathSpline_Wars = new(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Hedgehog Engine Path Spline (Wars/Rangers) types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".pcmodel":
                case ".pccol":
                case ".pcrt":
                case ".pointcloud":
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
                            "3. .pcrt (Lighting Instance)\n" +
                            "4. .pointcloud (Generic Instance)"
                        );

                        // Wait for the user to input an option from the list.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "pccol"; break;
                            case '2': extension = "pcmodel"; break;
                            case '3': extension = "pcrt"; break;
                            case '4': extension = "pointcloud"; break;
                        }

                        // Sanity check the input, inform the user and abort if its still null or empty.
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

                case ".fxcol":
                case ".fxcol.bin":
                    Console.WriteLine("Converting Hedgehog Engine Scene Effect Collision to JSON.");
                    using (KnuxLib.Engines.Hedgehog.SceneEffectCollision sceneEffectCollision = new(arg, true))
                    break;

                case ".hedgehog.sceneeffectcollision.json":
                    // If an extension isn't specified, then ask the user which fxcol extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .fxcol.bin (Sonic Forces)\n" +
                            "2. .fxcol (Sonic Frontiers)"
                        );

                        // Wait for the user to input an option from the list.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "fxcol.bin"; break;
                            case '2': extension = "fxcol"; break;
                        }

                        // Sanity check the input, inform the user and abort if its still null or empty.
                        if (string.IsNullOrEmpty(extension))
                        {
                            Console.WriteLine("\nNo format extension specified! Aborting...\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                        }

                        // Add a line break.
                        Console.WriteLine();
                    }

                    Console.WriteLine("Converting JSON to Hedgehog Engine Scene Effect Collision.");
                    using (KnuxLib.Engines.Hedgehog.SceneEffectCollision sceneEffectCollision = new())
                    {
                        sceneEffectCollision.Data = sceneEffectCollision.JsonDeserialise<KnuxLib.Engines.Hedgehog.SceneEffectCollision.FormatData>(arg);
                        sceneEffectCollision.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                    }
                    break;

                case ".svcol.bin":
                    Console.WriteLine("Converting Hedgehog Engine Sector Visiblity Collision to JSON.");
                    using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_Wars sectorVisibilityCollision = new(arg, true))
                    break;

                case ".hedgehog.sectorvisiblitycollision.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Sector Visiblity Collision.");
                    using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_Wars sectorVisibilityCollision = new())
                    {
                        sectorVisibilityCollision.Data = sectorVisibilityCollision.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_Wars.SectorVisibilityShape>>(arg);
                        sectorVisibilityCollision.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.svcol.bin");
                    }
                    break;

                case ".terrain-material":
                    Console.WriteLine("Converting Hedgehog Engine Terrain Material to JSON.");
                    using (KnuxLib.Engines.Hedgehog.TerrainMaterial terrainMaterial = new(arg, true))
                    break;

                case ".hedgehog.terrain-material.json":
                    Console.WriteLine("Converting JSON to Hedgehog Engine Terrain Material.");
                    using (KnuxLib.Engines.Hedgehog.TerrainMaterial terrainMaterial = new())
                    {
                        terrainMaterial.Data = terrainMaterial.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.TerrainMaterial.Material>>(arg);
                        terrainMaterial.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.terrain-material");
                    }
                    break;
                #endregion

                #region Nu2 Engine formats.
                case ".ai":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "gcn\t\t(GameCube)",
                                                                  "ps2\t\t(PlayStation 2/Xbox)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Nu2 Engine AI Entity Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "gcn": using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube, true)) break;
                        case "ps2": using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox, true)) break;
                        
                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Nu2 Engine Ai Entity Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".nu2.aientitytable.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "gcn\t\t(GameCube)",
                                                                  "ps2\t\t(PlayStation 2/Xbox)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Nu2 Engine AI Entity Table.");

                    // Decide what to do based on the version value.
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
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Nu2 Engine Ai Entity Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".wmp":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "gcn\t\t(GameCube)",
                                                                  "ps2\t\t(PlayStation 2/Xbox)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Nu2 Engine Wumpa Fruit Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "gcn": using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube, true)) break;
                        case "ps2": using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Nu2 Engine Wumpa Fruit Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".nu2.wumpatable.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "gcn\t\t(GameCube)",
                                                                  "ps2\t\t(PlayStation 2/Xbox)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Nu2 Engine Wumpa Fruit Table.");

                    // Decide what to do based on the version value.
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
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Nu2 Engine Wumpa Fruit Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region OpenSpace Engine formats.
                case ".bf":
                case ".dsc":
                    Console.WriteLine("Extracting OpenSpace Big File Archive.");
                    using (KnuxLib.Engines.OpenSpace.BigFileArchive bf = new(arg, true))
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
                case ".osd":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "final\t(Final Retail Release)",
                                                                  "preview\t(Preview Trial Demo)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Rockman X7 Engine Stage Entity Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "final":
                            using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new(arg, false, true))
                                break;

                        case "preview":
                            using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new(arg, true, true))
                                break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Rockman X7 Engine Stage Entity Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".sld":
                    Console.WriteLine("Converting Rockman X7 Engine SLD Spline to OBJ.");
                    using (KnuxLib.Engines.RockmanX7.SLDSpline sldSpline = new(arg, true))
                        break;

                case ".rockmanx7.stageentitytable.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "final\t(Final Retail Release)",
                                                                  "preview\t(Preview Trial Demo)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Rockman X7 Engine Stage Entity Table.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "final":
                            using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                            {
                                stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                                stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.osd", false);
                            }
                            break;

                        case "preview":
                            using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                            {
                                stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                                stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.osd", true);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Rockman X7 Engine Stage Entity Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
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

                        // Wait for the user to input an option from the list.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "SET"; break;
                            case '2': extension = "31BF570E"; break;
                        }

                        // Sanity check the input, inform the user and abort if its still null or empty.
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

                    // Decide what to do based on the extension value.
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
                case ".storybook.lightfield.json":
                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Light Field.");
                    using (KnuxLib.Engines.Storybook.LightField lightField = new())
                    {
                        lightField.Data = lightField.JsonDeserialise<KnuxLib.Engines.Storybook.LightField.FormatData>(arg);
                        lightField.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.BIN");
                    }
                    break;

                case ".mtx":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "international\t(English and other languages)",
                                                                  "japanese\t\t(Japanese)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Sonic Storybook Engine Secret Rings Message Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "international":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new(arg, false, true))
                            break;

                        case "japanese":
                            using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new(arg, true, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic Storybook Engine Message Table (Secret Rings) types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".storybook.messagetable_secretrings.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "international\t(English and other languages)",
                                                                  "japanese\t\t(Japanese)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Secret Rings Message Table.");

                    // Decide what to do based on the version value.
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

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic Storybook Engine Message Table (Secret Rings) types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".pth":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "secretrings\t\t(Sonic and the Secret Rings)",
                                                                  "blackknight\t\t(Sonic and the Black Knight)"},
                                               new List<bool> { true, true });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Sonic Storybook Engine Path Spline to OBJ.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "secretrings":
                            using (KnuxLib.Engines.Storybook.PathSpline pathSpline = new(arg, KnuxLib.Engines.Storybook.PathSpline.FormatVersion.SecretRings, true))
                            break;

                        case "blackknight":
                            using (KnuxLib.Engines.Storybook.PathSpline pathSpline = new(arg, KnuxLib.Engines.Storybook.PathSpline.FormatVersion.BlackKnight, true))
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic Storybook Engine path types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".storybook.motion.json":
                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Player Motion Table.");
                    using (KnuxLib.Engines.Storybook.MotionTable motion = new())
                    {
                        motion.Data = motion.JsonDeserialise<List<KnuxLib.Engines.Storybook.MotionTable.MotionEntry>>(arg);
                        motion.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin");
                    }
                    break;

                case ".storybook.stageentitytableitems.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "secretrings\t\t(Sonic and the Secret Rings)",
                                                                  "blackknight\t\t(Sonic and the Black Knight)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Secret Rings Stage Entity Table Object Table.");

                    // Decide what to do based on the version value.
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

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic Storybook Engine Stage Entity Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".storybook.visibilitytable.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "secretrings\t\t(Sonic and the Secret Rings)",
                                                                  "blackknight\t\t(Sonic and the Black Knight)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Sonic Storybook Engine Visibility Table.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "secretrings":
                            using (KnuxLib.Engines.Storybook.VisibilityTable visibilityTable = new())
                            {
                                visibilityTable.Data = visibilityTable.JsonDeserialise<KnuxLib.Engines.Storybook.VisibilityTable.VisibilityBlock[]>(arg);
                                visibilityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.VisibilityTable.FormatVersion.SecretRings);
                            }
                            break;

                        case "blackknight":
                            using (KnuxLib.Engines.Storybook.VisibilityTable visibilityTable = new())
                            {
                                visibilityTable.Data = visibilityTable.JsonDeserialise<KnuxLib.Engines.Storybook.VisibilityTable.VisibilityBlock[]>(arg);
                                visibilityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.VisibilityTable.FormatVersion.BlackKnight);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic Storybook Engine Stage Entity Table types.\nPress any key to continue.");
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
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "ps2\t\t(PlayStation 2)",
                                                                  "wii\t\t(Wii)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Sonic World Adventure Wii Engine Area Points Table to JSON.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "ps2": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.PlayStation2, true)) break;
                        case "wii": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.Wii, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic World Adventure Wii Engine Area Point types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".worldadventurewii.areapoints.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "ps2\t\t(PlayStation 2)",
                                                                  "wii\t\t(Wii)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Sonic World Adventure Wii Engine Area Points Table.");

                    // Decide what to do based on the version value.
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

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Sonic World Adventure Wii Engine Area Point types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;

                case ".onz":
                    Console.WriteLine("Extracting Sonic World Adventure Wii Engine archive.");
                    using (KnuxLib.Engines.WorldAdventureWii.ONE onz = new(arg, true))
                    break;
                #endregion

                #region Space Channel Engine formats.
                case ".spacechannel.caption.json":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant to save with:",
                                               new List<string> { "international\t(English and other languages)",
                                                                  "japanese\t\t(Japanese)"},
                                               new List<bool> { false, false });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting JSON to Space Channel Engine Caption Table.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "international":
                            using (KnuxLib.Engines.SpaceChannel.CaptionTable captionTable = new())
                            {
                                captionTable.Data = captionTable.JsonDeserialise<List<string>>(arg);
                                captionTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", false);
                            }
                            break;

                        case "japanese":
                            using (KnuxLib.Engines.SpaceChannel.CaptionTable captionTable = new())
                            {
                                captionTable.Data = captionTable.JsonDeserialise<List<string>>(arg);
                                captionTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", true);
                            }
                            break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Space Channel Engine Caption Table types.\nPress any key to continue.");
                            Console.ReadKey();
                            return;
                    }
                    break;
                #endregion

                #region Wayforward Engine formats.
                case ".clb":
                    // Carry out a version check.
                    version = NoVersionChecker(version,
                                               "This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                               new List<string> { "duck\t(Ducktales Remastered)",
                                                                  "hgh\t\t(Half-Genie Hero)",
                                                                  "ss\t\t(Seven Sirens)"},
                                               new List<bool> { true, false, true });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(version))
                        return;

                    Console.WriteLine("Converting Wayforward Engine collision to OBJ.");

                    // Decide what to do based on the version value.
                    switch (version.ToLower())
                    {
                        case "duck": using (KnuxLib.Engines.Wayforward.Collision clb = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.duck, true)) break;
                        case "hgh": using (KnuxLib.Engines.Wayforward.Collision clb = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero, true)) break;
                        case "ss": using (KnuxLib.Engines.Wayforward.Collision clb = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens, true)) break;

                        // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                        default:
                            Console.WriteLine($"Format identifer '{version}' is not valid for any currently supported Wayforward Engine collision types.\nPress any key to continue.");
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

                    // Read the user's input. If there's nothing, then set gpuFile to null.
                    string? gpuFile = Console.ReadLine().Replace("\"", "");
                    if (gpuFile == "")
                        gpuFile = null;

                    using (KnuxLib.Engines.Wayforward.Mesh mesh = new())
                    {
                        mesh.Load(arg);
                        mesh.ExportOBJTemp($@"{Path.ChangeExtension(arg, ".obj")}", gpuFile);
                    }
                    break;
                #endregion

                #region Westwood Engine Formats.
                case ".tre":
                case ".tru":
                    Console.WriteLine("Converting Westwood Engine Message Table to JSON.");
                    using (KnuxLib.Engines.Westwood.MessageTable messageTable = new(arg, true))
                    break;

                case ".westwood.messagetable.json":
                    // If an extension isn't specified, then ask the user which message table extension to save with.
                    if (string.IsNullOrEmpty(extension))
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please select the extension to save with:\n" +
                            "1. .tre (USA)\n" +
                            "2. .tru (UK)"
                        );

                        // Wait for the user to input an option from the list.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "tre"; break;
                            case '2': extension = "tru"; break;
                        }

                        // Sanity check the input, inform the user and abort if its still null or empty.
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

                #region Yacht Club Engine Formats
                case ".stl":
                    Console.WriteLine("Converting Yacht Club Engine String Translation List to JSON.");
                    using (KnuxLib.Engines.YachtClub.StringTranslationList stl = new(arg, true))
                        break;

                case ".yachtclub.stringtranslationlist.json":
                    Console.WriteLine("Converting JSON to Yacht Club Engine String Translation List.");
                    using (KnuxLib.Engines.YachtClub.StringTranslationList stl = new())
                    {
                        stl.Data = stl.JsonDeserialise<List<string>>(arg);
                        stl.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.stl");
                    }
                    break;
                #endregion

                // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                default:
                    Console.WriteLine($"Format extension {KnuxLib.Helpers.GetExtension(arg).ToLower()} is not valid for any currently supported formats.\nPress any key to continue.");
                    Console.ReadKey();
                    return;
            }
        }
    
        /// <summary>
        /// Asks the user for a version type for formats that need it.
        /// </summary>
        /// <param name="version">The version argument.</param>
        /// <param name="noticeLine">What should be printed to inform the user.</param>
        /// <param name="formats">The list of valid values for this version check.</param>
        /// <param name="redColours">Whether or not to colour each value red.</param>
        /// <param name="userInput">What to write in the section where the user should type.</param>
        /// <returns></returns>
        private static string? NoVersionChecker(string? version, string noticeLine, List<string> formats, List<bool> redColours, string userInput = "Format Type")
        {
            // Only do all the console stuff if the user hasn't already passed in a version value.
            if (string.IsNullOrEmpty(version))
            {
                // Inform the user of the need for a version value.
                Console.WriteLine(noticeLine);

                // Loop through each value and write it, either using the ColourConsole function or the normal WriteLine one.
                for (int format = 0; format < formats.Count; format++)
                {
                    if (redColours[format])
                        ColourConsole($"    {formats[format]}");
                    else
                        Console.WriteLine($"    {formats[format]}");
                }

                // Ask for the user's input.
                Console.Write($"\n{userInput}: ");

                // Wait for the user's input.
                version = Console.ReadLine().ToLower();

                // Sanity check the input, inform the user if its still empty or null. If not, add a line break.
                if (string.IsNullOrEmpty(version))
                {
                    Console.WriteLine($"\nNo {userInput.ToLower()} specified! Aborting...\nPress any key to continue.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine();
                }
            }

            // Return the (hopefully updated) version value.
            return version;
        }
    }
}