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
                Console.WriteLine("are considered experimental and may be missing elements or functionality.");

                FormatPrints.CapcomMT();
                FormatPrints.Hedgehog();
                FormatPrints.Nintendo();
                FormatPrints.Nu2();
                FormatPrints.OpenSpace();
                FormatPrints.ProjectM();
                FormatPrints.StellarStone();
                FormatPrints.SonicStorybook();
                FormatPrints.SonicWorldAdventureWii();
                FormatPrints.SpaceChannel();
                FormatPrints.Twinsanity();
                FormatPrints.Wayforward();

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
                                       { "nintendo_u8\t\t\t(Nintendo U8 Archive File", false },
                                       { "nintendo_u8_marathon\t(Nintendo U8 Archive File (Sonic '06))", false },
                                       { "openspace_big\t\t(OpenSpace Engine Big File Archive)", true },
                                       { "storybook\t\t\t(Sonic Storybook Engine ONE File)", false },
                                       { "swa_sd\t\t\t(Sonic World Adventure Wii Engine ONE File)", false },
                                       { "swa_sd_compressed\t\t(Sonic World Adventure Wii Engine Compressed ONZ File)", false },
                                       { "twinsanity\t\t\t(Twinsanity Engine Data Header Pair)", false },
                                       { "wayforward\t\t\t(Wayforward Engine Package File)", false },
                                       { "wayforward_bigendian\t(Wayforward Engine Package File)", false },
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

                // OpenSpace Engine Big File Archive.
                case "openspace_big": _ = new KnuxLib.Engines.OpenSpace.BigFileArchive(arg, true); break;

                // Nintendo U8 Archive.
                case "nintendo_u8": _ = new KnuxLib.Engines.Nintendo.U8(arg, false, true); break;
                case "nintendo_u8_marathon": _ = new KnuxLib.Engines.Nintendo.U8(arg, true, true); break;

                // Sonic Storybook ONE Archive.
                case "storybook": _ = new KnuxLib.Engines.SonicStorybook.ONE(arg, true); break;

                // Sonic World Adventure Wii ONE Archive.
                case "swa_sd": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.ONE(arg, false, true); break;
                case "swa_sd_compressed": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.ONE(arg, true, true); break;

                // Twinsanity Engine Data Header Pair.
                case "twinsanity": _ = new KnuxLib.Engines.Twinsanity.DataHeaderPair(arg, true); break;

                // Wayforward Engine Package.
                case "wayforward": _ = new KnuxLib.Engines.Wayforward.Package(arg, false, true); break;
                case "wayforward_bigendian": _ = new KnuxLib.Engines.Wayforward.Package(arg, true, true); break;
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
                case ".ai":
                case ".nu2.aientitytable.json":
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
                        case "gcn": _ = new KnuxLib.Engines.Nu2.AIEntityTable(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube, true); break;
                        case "ps2": _ = new KnuxLib.Engines.Nu2.AIEntityTable(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox, true); break;
                        default: Helpers.InvalidFormatVersion("Nu2 Engine AI Entity Table"); return;
                    }

                    break;

                case ".arc":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "capcom\t(Capcom MT Engine Archive)", false },
                                                { "marathon\t(Nintendo U8 (Sonic '06))", false },
                                                { "nintendo\t(Nintendo U8)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "capcom": _ = new KnuxLib.Engines.CapcomMT.Archive(arg, true); break;
                        case "marathon": _ = new KnuxLib.Engines.Nintendo.U8(arg, true, true); break;
                        case "nintendo": _ = new KnuxLib.Engines.Nintendo.U8(arg, false, true); break;
                        default: Helpers.InvalidFormatVersion("Generic .arc Archive"); return;
                    }

                    break;

                case ".arcinfo": case ".hedgehog.archiveinfo.json": _ = new KnuxLib.Engines.Hedgehog.ArchiveInfo(arg, true); break;

                case ".bd": case ".bh": _ = new KnuxLib.Engines.Twinsanity.DataHeaderPair(arg, true); break;

                case ".bf": case ".dsc": _ = new KnuxLib.Engines.OpenSpace.BigFileArchive(arg, true); break;

                case ".bin":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "spacechannel_caption\t(Space Channel Engine Caption Table)", false },
                                                { "spacechannel_caption_jpn\t(Space Channel Engine Caption Table (Japanese))", false },
                                                { "storybook_lightfield\t(Sonic Storybook Engine Light Field)", false },
                                                { "storybook_motion\t\t(Sonic Storybook Engine Motion Table)", false },
                                                { "storybook_setitems_sr\t(Sonic Storybook Engine Stage Entity Table Object Table File (Secret Rings))", false },
                                                { "storybook_setitems_bk\t(Sonic Storybook Engine Stage Entity Table Object Table File (Black Knight))", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "spacechannel_caption": _ = new KnuxLib.Engines.SpaceChannel.CaptionTable(arg, false, true); break;
                        case "spacechannel_caption_jpn": _ = new KnuxLib.Engines.SpaceChannel.CaptionTable(arg, true, true); break;
                        case "storybook_lightfield": _ = new KnuxLib.Engines.SonicStorybook.LightField(arg, true); break;
                        case "storybook_motion": _ = new KnuxLib.Engines.SonicStorybook.MotionTable(arg, true); break;
                        case "storybook_setitems_sr": _ = new KnuxLib.Engines.SonicStorybook.StageEntityTableItems(arg, KnuxLib.Engines.SonicStorybook.StageEntityTableItems.FormatVersion.SecretRings, true); break;
                        case "storybook_setitems_bk": _ = new KnuxLib.Engines.SonicStorybook.StageEntityTableItems(arg, KnuxLib.Engines.SonicStorybook.StageEntityTableItems.FormatVersion.BlackKnight, true); break;
                        default: Helpers.InvalidFormatVersion("Generic Binary"); return;
                    }

                    break;
                case ".sonicstorybook.motion.json": _ = new KnuxLib.Engines.SonicStorybook.MotionTable(arg, true); break;
                case ".sonicstorybook.lightfield.json": _ = new KnuxLib.Engines.SonicStorybook.LightField(arg, true); break;
                case ".sonicstorybook.setitems.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "storybook_setitems_sr\t(Sonic Storybook Engine Stage Entity Table Object Table File (Secret Rings))", false },
                                                { "storybook_setitems_bk\t(Sonic Storybook Engine Stage Entity Table Object Table File (Black Knight))", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "storybook_setitems_sr": _ = new KnuxLib.Engines.SonicStorybook.StageEntityTableItems(arg, KnuxLib.Engines.SonicStorybook.StageEntityTableItems.FormatVersion.SecretRings, true); break;
                        case "storybook_setitems_bk": _ = new KnuxLib.Engines.SonicStorybook.StageEntityTableItems(arg, KnuxLib.Engines.SonicStorybook.StageEntityTableItems.FormatVersion.BlackKnight, true); break;
                        default: Helpers.InvalidFormatVersion("Sonic Storybook Stage Entity Table Object Table"); return;
                    }

                    break;
                case ".spacechannel.caption.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "spacechannel_caption\t(Space Channel Engine Caption Table)", false },
                                                { "spacechannel_caption_jpn\t(Space Channel Engine Caption Table (Japanese))", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "spacechannel_caption": _ = new KnuxLib.Engines.SpaceChannel.CaptionTable(arg, false, true); break;
                        case "spacechannel_caption_jpn": _ = new KnuxLib.Engines.SpaceChannel.CaptionTable(arg, true, true); break;
                        default: Helpers.InvalidFormatVersion("Space Channel Engine Caption Table"); return;
                    }

                    break;

                case ".clb":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "duck\t(Ducktales Remastered)", false },
                                                { "duck_cafe\t(Ducktales Remastered (Wii U))", true },
                                                { "hero\t(Shantae: Half-Genie Hero)", false },
                                                { "hero_cafe\t(Shantae: Half-Genie Hero (Wii U))", false },
                                                { "sevensirens\t(Shantae and the Seven Sirens)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "duck": _ = new KnuxLib.Engines.Wayforward.Collision(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.duck, false, true); break;
                        case "duck_cafe": _ = new KnuxLib.Engines.Wayforward.Collision(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.duck, true, true); break;
                        case "hero": _ = new KnuxLib.Engines.Wayforward.Collision(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero, false, true); break;
                        case "hero_cafe": _ = new KnuxLib.Engines.Wayforward.Collision(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero, true, true); break;
                        case "sevensirens": _ = new KnuxLib.Engines.Wayforward.Collision(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens, false, true); break;
                        default: Helpers.InvalidFormatVersion("Wayforward Engine Collision"); return;
                    }

                    break;

                case ".crt":
                case ".nu2.cratetable.json":
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
                        case "gcn": _ = new KnuxLib.Engines.Nu2.CrateTable(arg, KnuxLib.Engines.Nu2.CrateTable.FormatVersion.GameCube, true); break;
                        case "ps2": _ = new KnuxLib.Engines.Nu2.CrateTable(arg, KnuxLib.Engines.Nu2.CrateTable.FormatVersion.PlayStation2Xbox, true); break;
                        default: Helpers.InvalidFormatVersion("Nu2 Engine Crate Table"); return;
                    }

                    break;

                case ".dat": case ".projectm.messagetable.json": _ = new KnuxLib.Engines.ProjectM.MessageTable(arg, true); break;

                case ".densitypointcloud": case ".hedgehog.densitypointcloud.json": _ = new KnuxLib.Engines.Hedgehog.DensityPointCloud(arg, true); break;

                case ".env":
                case ".wayforward.environment.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "wayforward", false },
                                                { "wayforward_bigendian", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "wayforward": _ = new KnuxLib.Engines.Wayforward.Environment(arg, false, true); break;
                        case "wayforward_bigendian": _ = new KnuxLib.Engines.Wayforward.Environment(arg, true, true); break;
                        default: Helpers.InvalidFormatVersion("Wayforward Engine Environment Table"); return;
                    }

                    break;

                case ".map.bin": case ".hedgehog.map_2010.json": _ = new KnuxLib.Engines.Hedgehog.Map_2010(arg, true); break;

                case ".mat": _ = new KnuxLib.Engines.StellarStone.MaterialLibrary(arg, true); break;

                case ".mlevel": case ".hedgehog.masterlevels.json": _ = new KnuxLib.Engines.Hedgehog.MasterLevels(arg, true); break;

                // TODO: Assimp for Stellar Stone Engine.
                case ".obj":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "sonic2013_cafe\t(Sonic Lost World (Wii U))", false },
                                                { "sonic2013\t(Sonic Lost World)", false },
                                                { "wars\t(Sonic Forces)", false },
                                                { "rangers\t(Sonic Rangers)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "sonic2013_cafe": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013, true, ".path2.bin", true); break;
                        case "sonic2013": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013, true, ".path2.bin"); break;
                        case "wars": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Wars, true); break;
                        case "rangers": _ = new KnuxLib.Engines.Hedgehog.PathSpline(arg, KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.Rangers, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Path Spline"); return;
                    }

                    break;

                case ".one":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "storybook\t\t(Sonic Storybook Engine ONE File)", false },
                                                { "swa_sd\t\t(Sonic World Adventure Wii Engine ONE File)", false },
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "storybook": _ = new KnuxLib.Engines.SonicStorybook.ONE(arg, true); break;
                        case "swa_sd": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.ONE(arg, false, true); break;
                        default: Helpers.InvalidFormatVersion("Generic .one Archive"); return;
                    }

                    break;

                case ".onz": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.ONE(arg, true, true); break;

                case ".pak":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "wayforward\t\t\t(Wayforward Engine Package File)", false },
                                                { "wayforward_bigendian\t(Wayforward Engine Package File)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "wayforward": _ = new KnuxLib.Engines.Wayforward.Package(arg, false, true); break;
                        case "wayforward_bigendian": _ = new KnuxLib.Engines.Wayforward.Package(arg, true, true); break;
                        default: Helpers.InvalidFormatVersion("Wayforward Engine Package File"); return;
                    }

                    break;

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

                case ".pth":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "storybook_pathspline_sr\t(Sonic Storybook Engine Path Spline File (Secret Rings))", false },
                                                { "storybook_pathspline_bk\t(Sonic Storybook Engine Path Spline File (Black Knight))", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "storybook_pathspline_sr": _ = new KnuxLib.Engines.SonicStorybook.PathSpline(arg, KnuxLib.Engines.SonicStorybook.PathSpline.FormatVersion.SecretRings, true); break;
                        case "storybook_pathspline_bk": _ = new KnuxLib.Engines.SonicStorybook.PathSpline(arg, KnuxLib.Engines.SonicStorybook.PathSpline.FormatVersion.BlackKnight, true); break;
                        default: Helpers.InvalidFormatVersion("Sonic Storybook Engine Path Spline"); return;
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

                case ".svcol.bin":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "sonic2013\t(Sonic Lost World)", true },
                                                { "wars\t(Sonic Forces)", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "sonic2013": _ = new KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_2013(arg, true); break;
                        case "wars": _ = new KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_Wars(arg, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Sector Visibility Collision"); return;
                    }
                    break;

                case ".hedgehog.sectorvisiblitycollision_2013.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "sonic2013_cafe\t(Sonic Lost World (Wii U))", true },
                                                { "sonic2013\t(Sonic Lost World)", true }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "sonic2013_cafe": _ = new KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_2013(arg, true, true); break;
                        case "sonic2013": _ = new KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_2013(arg, true); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Sector Visibility Collision"); return;
                    }

                    break;

                case ".hedgehog.sectorvisiblitycollision_wars.json": _ = new KnuxLib.Engines.Hedgehog.SectorVisibilityCollision_Wars(arg, true); break;

                case ".terrain-instanceinfo": case ".hedgehog.instanceinfo.json": _ = new KnuxLib.Engines.Hedgehog.InstanceInfo(arg, true); break;

                case ".terrain-material": case ".hedgehog.terrain-material.json": _ = new KnuxLib.Engines.Hedgehog.TerrainMaterial(arg, true); break;

                case ".wap": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.AreaPoints(arg, true); break;
                case ".sonicworldadventure_sd.areapoints.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "wii", false },
                                                { "ps2", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "wii": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.AreaPoints(arg, true, true); break;
                        case "ps2": _ = new KnuxLib.Engines.SonicWorldAdventure_SD.AreaPoints(arg, true, false); break;
                        default: Helpers.InvalidFormatVersion("Sonic World Adventure (SD) Area Points Table"); return;
                    }

                    break;

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

                case ".xtb2": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2013(arg, true); break;
                case ".hedgehog.messagetable_2013.json":
                    // Check for a format version.
                    Helpers.VersionChecker("This file has multiple variants that can't be auto detected, please specifiy the variant:",
                                            new()
                                            {
                                                { "wiiu\t(Sonic Lost World (Wii U))", false },
                                                { "pc\t(Sonic Lost World (PC))", false }
                                            });

                    // If the version is still null or empty, then abort.
                    if (string.IsNullOrEmpty(Version))
                        return;

                    switch (Version.ToLower())
                    {
                        case "wiiu": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2013(arg, true, true); break;
                        case "pc": _ = new KnuxLib.Engines.Hedgehog.MessageTable_2013(arg, true, false); break;
                        default: Helpers.InvalidFormatVersion("Hedgehog Engine Message Table (2013)"); return;
                    }

                    break;

                // If a command line argument without a corresponding format has been passed, then inform the user and abort.
                default:
                    Console.WriteLine($"Format extension {KnuxLib.Helpers.GetExtension(arg).ToLower()} is not valid for any currently supported formats.\nPress any key to continue.");
                    Console.ReadKey();
                    return;
            }
        }
    }
}
