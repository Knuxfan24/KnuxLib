using System.Globalization;
using System.Text;

namespace KnuxTools
{
    internal class Program
    {
        // Set up the extension and version values.
        public static string? Extension = null;
        public static string? Version = null;

        static void Main(string[] args)
        {
            // Force culture info 'en-GB' to prevent errors with values altered by culture-specific differences.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

            // Enable shift-jis for BINA stuff.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Check for arguments.
            if (args.Length > 0)
            {
                // Loop through and set the extension and version, if the parameters are passed.
                foreach (string arg in args)
                {
                    if (arg.StartsWith("-extension=") || arg.StartsWith("-e="))
                        Extension = arg.Split('=')[1];

                    if (arg.StartsWith("-version=") || arg.StartsWith("-v="))
                        Version = arg.Split('=')[1];
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
                        HandleDirectory(arg);
                    }

                    // File based checks.
                    if (File.Exists(arg))
                    {
                        // Print the file name.
                        Console.WriteLine($"File: {arg} \n");

                        // Pass the argument, extension and version onto the HandleFile function.
                        HandleFile(arg);
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
                Helpers.ColourConsole(" red ", false);
                Console.Write("are considered experimental and may be missing elements or functionality (see the\r\nExperimental Formats document at");
                Helpers.ColourConsole(" https://github.com/Knuxfan24/KnuxLib/blob/master/Experimental_Formats.md ", false, ConsoleColor.Cyan);
                Console.WriteLine("for\r\ninformation on the individual issues).");

                FormatPrints.CapcomMT();
                FormatPrints.Hedgehog();
                FormatPrints.Nu2();
                FormatPrints.StellarStone();
                FormatPrints.SonicStorybook();
                FormatPrints.Twinsanity();

                Console.WriteLine("===\r\nUsage:");
                Console.WriteLine("KnuxTools.exe \"path\\to\\supported\\file\" [-version={VERSION}] [-extension={EXTENSION}]\r\n");
                Console.Write("Arguments surrounded by square brackets are optional and only affect certain formats (highlighted in ");
                Helpers.ColourConsole("yellow", false, ConsoleColor.Yellow);
                Console.WriteLine("),\r\nif they aren't specified then they will be selected through manual input when required.\r\n");
                Console.WriteLine("Alternatively, simply drag a supported file onto this application in Windows Explorer.\r\n");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Function for handling directories.
        /// </summary>
        /// <param name="arg">The path of the directory that is currently being processed.</param>
        private static void HandleDirectory(string arg)
        {
            // Check for an archive type identifier if a version wasn't specified, set the version if one was found.
            if (string.IsNullOrEmpty(Version) && File.Exists($@"{arg}\knuxtools_archivetype.txt"))
                Version = File.ReadAllText($@"{arg}\knuxtools_archivetype.txt");

            // Check for a format version.
            Helpers.VersionChecker("Please specify the archive type to pack this directory into, valid options are:",
                                   new()
                                   {
                                       { "capcomv7\t\t\t(Capcom MT Framework Engine (Version 7))", false },
                                       { "capcomv9\t\t\t(Capcom MT Framework Engine (Version 9))", false },
                                       { "capcomv9_uncompressed\t(Capcom MT Framework Engine (Version 9, No Compression))", false },
                                       { "hh_instance2pointcloud\t(Convert Hedgehog Engine Terrain Instances into a Hedgehog Engine Point Cloud)", false },
                                       { "storybook\t\t\t(Sonic Storybook Engine ONE File)", false },
                                       { "twinsanity\t\t\t(Twinsanity Engine Data Header Pair)", false },
                                   },
                                   "Archive Type");

            // If the version is still null or empty, then abort.
            if (string.IsNullOrEmpty(Version))
                return;

            // Handle packaging this directory into an archive.
            switch (Version)
            {
                // Capcom MT Archive.
                case "capcomv7": _ = new KnuxLib.Engines.CapcomMT.Archive(arg, true, 0x07, true); break;
                case "capcomv9": _ = new KnuxLib.Engines.CapcomMT.Archive(arg, true, 0x09, true); break;
                case "capcomv9_uncompressed": _ = new KnuxLib.Engines.CapcomMT.Archive(arg, true, 0x09, false); break;

                // Hedgehog Engine Instance Info to Point Cloud conversion.
                case "hh_instance2pointcloud": _ = new KnuxLib.Engines.Hedgehog.InstanceInfo(arg); break;

                // Sonic Storybook ONE Archive.
                case "storybook": _ = new KnuxLib.Engines.SonicStorybook.ONE(arg, true); break;

                // Twinsanity Engine Data Header Pair.
                case "twinsanity": _ = new KnuxLib.Engines.Twinsanity.DataHeaderPair(arg, true); break;
            }
        }
    
        /// <summary>
        /// Function for handling files.
        /// </summary>
        /// <param name="arg">The file that is currently being processed.</param>
        private static void HandleFile(string arg)
        {
            // Determine the full file extension.
            switch (KnuxLib.Helpers.GetExtension(arg).ToLower())
            {
                case ".arc": _ = new KnuxLib.Engines.CapcomMT.Archive(arg, true); break;

                case ".arcinfo": case ".hedgehog.archiveinfo.json": _ = new KnuxLib.Engines.Hedgehog.ArchiveInfo(arg, true); break;

                case ".bd": case ".bh": _ = new KnuxLib.Engines.Twinsanity.DataHeaderPair(arg, true); break;

                case ".densitypointcloud": case ".hedgehog.densitypointcloud.json": _ = new KnuxLib.Engines.Hedgehog.DensityPointCloud(arg, true); break;

                case ".map.bin": case ".hedgehog.map_2010.json": _ = new KnuxLib.Engines.Hedgehog.Map_2010(arg, true); break;

                case ".mat": _ = new KnuxLib.Engines.StellarStone.MaterialLibrary(arg, true); break;

                case ".mlevel": case ".hedgehog.masterlevels.json": _ = new KnuxLib.Engines.Hedgehog.MasterLevels(arg, true); break;

                // TODO: Assimp for Stellar Stone Engine.
                case ".obj":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "sonic_2013\t(Sonic Lost World)", false },
                                                { "wars\t(Sonic Forces)", false },
                                                { "rangers\t(Sonic Rangers)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "sonic_2013": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013, true, ".path2.bin"); break;
                        case "wars": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars, true); break;
                        case "rangers": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Rangers, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Path Spline"); return;
                    }

                    break;

                case ".one": _ = new KnuxLib.Engines.SonicStorybook.ONE(arg, true); break;

                case ".path":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "wars\t(Sonic Forces)", false },
                                                { "rangers\t(Sonic Rangers)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "wars": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars, true); break;
                        case "rangers": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Rangers, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Path Spline"); return;
                    }

                    break;

                case ".path2.bin": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013, true); break;

                case ".pcmodel":
                case ".pccol":
                case ".pcrt":
                case ".pointcloud":
                case ".hedgehog.pointcloud.json":
                    // If this is a JSON, then do an extension check.
                    if (Path.GetExtension(arg) == ".json")
                    {
                        Helpers.ExtensionChecker(new()
                        {
                            { ".pccol", "Collision Instance" },
                            { ".pcmodel", "Terrain Instance" },
                            { ".pcrt", "Lighting Instance" },
                            { ".pointcloud", "Generic Instance" },
                        });

                        // If the extension is still null or empty, then abort.
                        if (string.IsNullOrEmpty(Extension))
                            return;
                    }
                    _ = new KnuxLib.Engines.Hedgehog.PointCloud(arg, true, Extension);
                    break;

                case ".sco": _ = new KnuxLib.Engines.StellarStone.MeshObject(arg, true); break;

                case ".terrain-instanceinfo": case ".hedgehog.instanceinfo.json": _ = new KnuxLib.Engines.Hedgehog.InstanceInfo(arg, true); break;

                case ".wmp":
                case ".nu2.wumpatable.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "gcn\t\t(GameCube)", false },
                                                { "ps2\t\t(PlayStation 2/Xbox)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "gcn": _ = new KnuxLib.Engines.Nu2.WumpaTable(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube, true); break;
                        case "ps2": _ = new KnuxLib.Engines.Nu2.WumpaTable(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox, true); break;
                        default: Helpers.InvalidFormatVersion("Nu2 Engine Wumpa Fruit Table"); return;
                    }

                    break;

                case ".xtb":
                case ".hedgehog.messagetable_2010.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "sonic2010\t(Sonic Colours)", false },
                                                { "blueblur\t(Sonic Generations)", false },
                                                { "william\t(Mario and Sonic at the London 2012 Olympic Games)", false },
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "sonic2010": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2010(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.sonic_2010, true); break;
                        case "blueblur": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2010(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur, true); break;
                        case "william": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2010(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine 2010 Message Table"); return;
                    }

                    break;

                case ".xtb2": case ".hedgehog.messagetable_2013.json": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2013(arg, true); break;

                // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                default:
                    Console.WriteLine($"Format extension {KnuxLib.Helpers.GetExtension(arg).ToLower()} is not valid for any currently supported formats.\nPress any key to continue.");
                    Console.ReadKey();
                    return;
            }
        }
    }
}
