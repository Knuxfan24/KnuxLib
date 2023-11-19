using System.Globalization;
using System.Numerics;
using System.Text;

namespace KnuxTools
{
    // TOOO: Settle on consistent version options and list them somewhere (Wiki page or a seperate MD file?).
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
                Console.WriteLine("Command line tool used to convert the following supported file types to various other formats. \n" +
                                  "Each format converts to and from a JSON file unless otherwise specified. \n");

                Console.WriteLine("Alchemy Engine: \n" +
                                  "Assets Container Archive Pair (.gfc/gob) - Extracts to a directory of the same name as the input archive (importing not yet possible). \n");

                Console.WriteLine("CarZ Engine: \n" +
                                  "Material Library (.mat) - Exports to the MTL material library standard and imports from an Assimp compatible model. \n" +
                                  "3D Model (.sco) - Exports to the Wavefront OBJ model standard and imports from an Assimp compatible model. \n");

                Console.WriteLine("Gods Engine: \n" +
                                  "WAD Archive (.wad) - Extracts to a directory of the same name as the input archive (importing not yet possible). \n");

                Console.WriteLine("Hedgehog Engine: \n" +
                                  "Archive Info (.arcinfo) \n" +
                                  "Bullet Skeleton (.skl.pxd) \n" +
                                  "Gismo V3 (.gismod/.gismop) \n" +
                                  "Instance Info (.terrain-instanceinfo) - Import a folder containing files to generate a Sonic Frontiers point cloud file. \n" +
                                  "Light Field (Rangers) (.lf) \n" +
                                  "Master Level Table (.mlevel) \n" +
                                  "Message Table (sonic2010/blueblur/william) (.xtb) \n" +
                                  "Message Table (sonic2013) (.xtb2) \n" +
                                  "Point Cloud (.pccol/.pcmodel/.pcrt) \n" +
                                  "Sector Visibility Collision (.bin.svcol) \n");

                Console.WriteLine("NiGHTS 2 Engine: \n" +
                                  "ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n");

                Console.WriteLine("Nu2 Engine: \n" +
                                  "AI Entity Table (.ai) \n" +
                                  "Wumpa Fruit Table (.wmp) \n");

                Console.WriteLine("Project M Engine: \n" +
                                  "Message Table (.dat) \n");

                Console.WriteLine("Rockman X7 Engine: \n" +
                                  "Stage Entity Table (.328f438b/.osd) \n");

                Console.WriteLine("Rockman X8 Engine: \n" +
                                  "Stage Entity Table (.31bf570e/.set) \n");

                Console.WriteLine("Sonic Storybook Engine: \n" +
                                  "Message Table (Secret Rings) (.mtx) \n" +
                                  "ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n" +
                                  "Stage Entity Table Object Table (.bin) \n" +
                                  "Texture Directory (.txd) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n");

                Console.WriteLine("Sonic The Portable Engine: \n" +
                                  "AMB Archive (.amb) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n");

                Console.WriteLine("Sonic World Adventure Wii Engine: \n" +
                                  "Area Points Table (.wap) \n" +
                                  "ONE Archive (.one/.onz) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n");

                Console.WriteLine("Wayforward Engine: \n" +
                                  "Collision (.clb) - Converts to an OBJ format and imports (Half-Genie Hero only) from an Assimp compatible model." +
                                  "Environment (.env) \n" +
                                  "Layer List (.lgb) \n" +
                                  "List Table (.ltb) \n" +
                                  "Package Archive (.pak) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory. \n");

                Console.WriteLine("Westwood Engine: \n" +
                                  "Message Table (.tre/.tru) \n");

                Console.WriteLine("Usage: \n" +
                                  "KnuxTools.exe \"path\\to\\supported\\file\" \n" +
                                  "Alternatively, simply drag a supported file onto this application in Windows Explorer. \n \n" +
                                  "Press any key to continue.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Function for handling directories.
        /// </summary>
        /// <param name="arg">The path of the directory that is currently being processed.</param>
        /// <param name="version">The version parameter to use.</param>
        private static void HandleDirectory(string arg, string? version)
        {
            // If a version isn't specified, then ask the user which archive to save as.
            if (version == null)
            {
                // List our supported archive types.
                Console.WriteLine
                (
                    "Please specify the archive type to pack this directory into;\n" +
                    "1. NiGHTS 2 Engine ONE File\n" +
                    "2. Sonic The Portable Engine AMB File\n" +
                    "3. Sonic Storybook Engine ONE File\n" +
                    "4. Sonic Storybook Engine TXD File\n" +
                    "5. Sonic World Adventure Wii ONE File.\n" +
                    "6. Wayforward Engine PAK File.\n" +
                    "R. Convert Hedgehog Engine Terrain Instances into a Hedgehog Engine Point Cloud."
                );

                // Wait for the user to input an option.
                switch (Console.ReadKey().KeyChar)
                {
                    // NiGHTS 2 Engine ONE Archives.
                    case '1': version = "nights2_one"; break;

                    // Sonic The Portable Engine AMB Archives.
                    case '2': version = "portable_amb"; break;

                    // Sonic Storybook Series Engine ONE Archives.
                    case '3': version = "storybook_one"; break;

                    // Sonic Storybook Series Engine Texture Directories.
                    case '4': version = "storybook_txd"; break;

                    // Sonic World Adventure Wii Engine ONE Archives.
                    case '5':
                        // List our supported versions for the Sonic World Adventure Wii ONE format.
                        Console.WriteLine
                        (
                            "\n\nThis file has multiple file version options, please specifiy the version to save with;\n" +
                            "1. Uncompressed ONE Archive.\n" +
                            "2. Compressed ONZ Archive"
                        );

                        // Set the version to either swawii_one or swawii_onz depending on the user's selection.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "swawii_one"; break;
                            case '2': version = "swawii_onz"; break;
                        }
                        break;

                    // Wayforward Engine Packages.
                    case '6': version = "wayforward_pak"; break;

                    // Hedgehog Engine Terrain Instance Conversion.
                    case 'r': version = "hedgehog_terrain2pointcloud"; break;
                }
            }

            // Sanity check that version actually has a value.
            if (version != null)
            {
                switch (version)
                {
                    // NiGHTS 2 Engine ONE Archives.
                    case "nights2_one":
                        using (KnuxLib.Engines.NiGHTS2.ONE one = new())
                        {
                            Console.WriteLine("\n");
                            one.Import(arg);
                            one.Save($@"{arg}.one");
                        }
                        break;

                    // Sonic The Portable Engine AMB Archives.
                    case "portable_amb":
                        using (KnuxLib.Engines.Portable.AMB amb = new())
                        {
                            Console.WriteLine("\n");
                            amb.Import(arg);
                            amb.Save($@"{arg}.amb");
                        }
                        break;

                    // Sonic Storybook Series ONE Archives.
                    case "storybook_one":
                        using (KnuxLib.Engines.Storybook.ONE one = new())
                        {
                            Console.WriteLine("\n");
                            one.Import(arg);
                            one.Save($@"{arg}.one");
                        }
                        break;

                    // Sonic Storybook Series Texture Directories.
                    case "storybook_txd":
                        using (KnuxLib.Engines.Storybook.TextureDirectory textureDirectory = new())
                        {
                            Console.WriteLine("\n");
                            textureDirectory.Import(arg);
                            textureDirectory.Save($@"{arg}.txd");
                        }
                        break;

                    // Sonic World Adventure Wii ONE Archives.
                    case "swawii_one":
                    case "swawii_onz":
                        using (KnuxLib.Engines.WorldAdventureWii.ONE one = new())
                        {
                            Console.WriteLine("\n");
                            one.Import(arg);
                            one.Save($@"{arg}.one");
                        }

                        // If the version indicates that the archive needs to be compressed, then compress it.
                        if (version == "swawii_onz")
                        {
                            // Set up a buffer.
                            MemoryStream buffer = new();

                            // Set up PuyoTools' LZ11 Compression.
                            PuyoTools.Core.Compression.Lz11Compression lz11 = new();

                            // Set up a file stream of the uncompressed archive.
                            var stream = File.OpenRead($@"{arg}.one");

                            // Compress the previously saved ONE archive into the buffer.
                            buffer = lz11.Compress(stream);

                            // Write the buffer to disk.
                            buffer.WriteTo(File.Create($@"{arg}.onz"));

                            // Close the file stream so the uncompressed archive can be deleted.
                            stream.Close();

                            // Delete the temporary uncompressed file.
                            File.Delete($@"{arg}.one");
                        }
                        break;

                    // Wayforward Engine Packages.
                    case "wayforward_pak":
                        using (KnuxLib.Engines.Wayforward.Package pak = new())
                        {
                            Console.WriteLine("\n");
                            pak.Import(arg);
                            pak.Save($@"{arg}.pak");
                        }
                        break;

                    // Hedgehog Engine Terrain Instance Conversion.
                    case "hedgehog_terrain2pointcloud":
                        KnuxLib.Engines.Hedgehog.InstanceInfo.ConvertDirectoryToPointCloud(arg);
                        break;

                    // If a command line argument without a corresponding format has been passed, then inform the user.
                    default:
                        Console.WriteLine($"\n\nFormat identifer {version} is not valid for any currently supported archive types.\nPress any key to continue.");
                        Console.ReadKey();
                        break;
                }
            }
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
                #region Seralised Models.
                case ".fbx":
                case ".dae":
                case ".obj":
                    // If a version isn't specified, then ask the user what to convert to.
                    if (version == null)
                    {
                        // List our supported model formats.
                        Console.WriteLine
                        (
                            "This file is a generic seralised type, please specify what format it is;\n" +
                            "1. CarZ Engine Model"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            // CarZ Engine SCO Models (and Material Libraries).
                            case '1': version = "carz_sco"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            // CarZ Engine SCO Models (and Material Libraries).
                            case "carz_sco":
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
                        }
                    }
                break;
                #endregion

                #region Generic Extensions.
                case ".one":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. NiGHTS 2 Engine ONE Archive\n" +
                            "2. Sonic Storybook Engine ONE Archive\n" +
                            "3. Sonic World Adventure Wii Uncompressed ONE Archive\n"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "nights2_one"; break;
                            case '2': version = "storybook_one"; break;
                            case '3': version = "swawii_one"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "nights2_one": using (KnuxLib.Engines.NiGHTS2.ONE one = new(arg, true)) break;
                            case "storybook_one": using (KnuxLib.Engines.Storybook.ONE one = new(arg, true)) break;
                            case "swawii_one": using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true)) break;
                        }
                    }
                    break;
                #endregion

                #region Alchemy Engine Formats.
                case ".gfc": using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new(arg, true)) break;
                case ".gob": using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.gfc", true)) break;
                #endregion

                #region CarZ Engine Formats.
                case ".mat": using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new(arg, true)) break;
                case ".sco": using (KnuxLib.Engines.CarZ.SCO sco = new(arg, true)) break;
                #endregion

                #region GODS Engine Formats.
                case ".wad":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Ninjabread Man PC/PS2\n" +
                            "2. Ninjabread Man Wii"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "gods_ninjabreadpc"; break;
                            case '2': version = "gods_ninjabreadwii"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "gods_pc":
                            case "gods_playstation2":
                            case "gods_ps2":
                            case "gods_ninjabreadpc":
                            case "gods_ninjabreadplaystation2":
                            case "gods_ninjabreadps2":
                                using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_PCPS2, true))
                                break;
                            case "gods_wii":
                            case "gods_ninjabreadwii":
                                using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_Wii, true))
                                break;
                        }
                    }
                    break;
                #endregion

                #region Hedgehog Engine Formats.
                case ".arcinfo": using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new(arg, true)) break;
                case ".hedgehog.archiveinfo.json":
                    using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new())
                    {
                        archiveInfo.Data = archiveInfo.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.ArchiveInfo.ArchiveEntry>>(arg);
                        archiveInfo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.arcinfo");
                    }
                    break;

                case ".skl.pxd": using (KnuxLib.Engines.Hedgehog.BulletSkeleton bulletSkeleton = new(arg, true)) break;
                case ".hedgehog.bulletskeleton.json":
                    using (KnuxLib.Engines.Hedgehog.BulletSkeleton bulletSkeleton = new())
                    {
                        bulletSkeleton.Data = bulletSkeleton.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.BulletSkeleton.Node>>(arg);
                        bulletSkeleton.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.skl.pxd");
                    }
                    break;

                case ".gismod": using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new(arg, true)) break;
                case ".gismop": using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg).Replace("_pln", "")}.gismod", true)) break;
                case ".hedgehog.gismo_rangers.json":
                    using (KnuxLib.Engines.Hedgehog.Gismo_Rangers gismo_rangers = new())
                    {
                        gismo_rangers.Data = gismo_rangers.JsonDeserialise<KnuxLib.Engines.Hedgehog.Gismo_Rangers.FormatData>(arg);
                        gismo_rangers.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.gismod");
                    }
                    break;

                case ".terrain-instanceinfo": using (KnuxLib.Engines.Hedgehog.InstanceInfo instanceInfo = new(arg, true)) break;
                case ".hedgehog.instanceinfo.json":
                    using (KnuxLib.Engines.Hedgehog.InstanceInfo instanceInfo = new())
                    {
                        instanceInfo.Data = instanceInfo.JsonDeserialise<KnuxLib.Engines.Hedgehog.InstanceInfo.FormatData>(arg);
                        instanceInfo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.terrain-instanceinfo");
                    }
                    break;

                case ".lf": using (KnuxLib.Engines.Hedgehog.LightField_Rangers lightfield_rangers = new(arg, true)) break;
                case ".hedgehog.lightfield_rangers.json":
                    using (KnuxLib.Engines.Hedgehog.LightField_Rangers lightfield_rangers = new())
                    {
                        lightfield_rangers.Data = lightfield_rangers.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.LightField_Rangers.LightField>>(arg);
                        lightfield_rangers.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.lf");
                    }
                    break;

                case ".pcmodel": case ".pccol": case ".pcrt": using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new(arg, true)) break;
                case ".hedgehog.pointcloud.json":
                    // If an extension isn't specified, then ask the user what to save as.
                    if (extension == null)
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please specifiy the extension to save with;\n" +
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
                    }

                    // Sanity check that extension actually has a value.
                    if (extension != null)
                    {
                        using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new())
                        {
                            pointCloud.Data = pointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.PointCloud.Instance>>(arg);
                            pointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                        }
                    }
                    break;

                case ".svcol.bin": using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision sectorVisibilityCollision = new(arg, true)) break;
                case ".hedgehog.sectorvisiblitycollision.json":
                    using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision sectorVisibilityCollision = new())
                    {
                        sectorVisibilityCollision.Data = sectorVisibilityCollision.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.SectorVisibilityCollision.SectorVisibilityShape>>(arg);
                        sectorVisibilityCollision.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.svcol.bin");
                    }
                    break;

                case ".xtb":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Sonic Colours\n" +
                            "2. Sonic Generations\n" +
                            "3. Mario and Sonic at the London 2012 Olympic Games"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "xtb_sonic2010"; break;
                            case '2': version = "xtb_blueblur"; break;
                            case '3': version = "xtb_william"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "xtb_sonic2010":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.sonic_2010, true))
                                break;
                            case "xtb_blueblur":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur, true))
                                break;
                            case "xtb_william":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new(arg, KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william, true))
                                break;
                        }
                    }
                    break;
                case ".hedgehog.messagetable_2010.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Sonic Colours\n" +
                            "2. Sonic Generations\n" +
                            "3. Mario and Sonic at the London 2012 Olympic Games."
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "xtb_sonic2010"; break;
                            case '2': version = "xtb_blueblur"; break;
                            case '3': version = "xtb_william"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "xtb_sonic2010":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                                {
                                    messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                    messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb");
                                }
                                break;
                            case "xtb_blueblur":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                                {
                                    messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                    messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb", KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.blueblur);
                                }
                                break;
                            case "xtb_william":
                                using (KnuxLib.Engines.Hedgehog.MessageTable_2010 messageTable_2010 = new())
                                {
                                    messageTable_2010.Data = messageTable_2010.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatData>(arg);
                                    messageTable_2010.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.xtb", KnuxLib.Engines.Hedgehog.MessageTable_2010.FormatVersion.william);
                                }
                                break;
                        }
                    }
                    break;

                case ".xtb2": using (KnuxLib.Engines.Hedgehog.MessageTable_2013 messageTable_2013 = new(arg, true)) break;
                case ".hedgehog.messagetable_2013.json":
                    using (KnuxLib.Engines.Hedgehog.MessageTable_2013 messageTable_2013 = new())
                    {
                        messageTable_2013.Data = messageTable_2013.JsonDeserialise<KnuxLib.Engines.Hedgehog.MessageTable_2013.FormatData>(arg);
                        messageTable_2013.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.svcol.bin");
                    }
                    break;

                case ".mlevel": using (KnuxLib.Engines.Hedgehog.MasterLevels masterLevelTable = new(arg, true)) break;
                case ".hedgehog.masterlevels.json":
                    using (KnuxLib.Engines.Hedgehog.MasterLevels masterLevelTable = new())
                    {
                        masterLevelTable.Data = masterLevelTable.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.MasterLevels.Level>>(arg);
                        masterLevelTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.mlevel");
                    }
                    break;
                #endregion

                #region Nu2 Formats.
                case ".ai":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. GameCube\n" +
                            "2. PlayStation 2/Xbox"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "nu2_gamecube"; break;
                            case '2': version = "nu2_playstation2"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "nu2_gamecube":
                            case "nu2_gc":
                            case "nu2_ngc":
                            case "nu2_gcn":
                                using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube, true))
                                break;
                            case "nu2_playstation2":
                            case "nu2_ps2":
                            case "nu2_xbox":
                            case "nu2_xb":
                                using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox, true))
                                break;
                        }
                    }
                    break;
                case ".nu2.aientitytable.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. GameCube\n" +
                            "2. PlayStation 2/Xbox"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "nu2_gamecube"; break;
                            case '2': version = "nu2_playstation2"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "nu2_gamecube":
                            case "nu2_gc":
                            case "nu2_ngc":
                            case "nu2_gcn":
                                using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                                {
                                    aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                    aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube);
                                }
                                break;
                            case "nu2_playstation2":
                            case "nu2_ps2":
                            case "nu2_xbox":
                            case "nu2_xb":
                                using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                                {
                                    aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                    aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox);
                                }
                                break;
                        }
                    }
                    break;

                case ".wmp":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. GameCube\n" +
                            "2. PlayStation 2/Xbox"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "nu2_gamecube"; break;
                            case '2': version = "nu2_playstation2"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "nu2_gamecube":
                            case "nu2_gc":
                            case "nu2_ngc":
                            case "nu2_gcn":
                                using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube, true))
                                break;
                            case "nu2_playstation2":
                            case "nu2_ps2":
                            case "nu2_xbox":
                            case "nu2_xb":
                                using (KnuxLib.Engines.Nu2.WumpaTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox, true))
                                break;
                        }
                    }
                    break;
                case ".nu2.wumpatable.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. GameCube\n" +
                            "2. PlayStation 2/Xbox"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "nu2_gamecube"; break;
                            case '2': version = "nu2_playstation2"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "nu2_gamecube":
                            case "nu2_gc":
                            case "nu2_ngc":
                            case "nu2_gcn":
                                using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                                {
                                    wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                    wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube);
                                }
                                break;
                            case "nu2_playstation2":
                            case "nu2_ps2":
                            case "nu2_xbox":
                            case "nu2_xb":
                                using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                                {
                                    wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                    wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox);
                                }
                                break;
                        }
                    }
                    break;
                #endregion

                #region Project M Engine Formats.
                case ".dat": using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new(arg, true)) break;
                case ".projectm.messagetable.json":
                    using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new())
                    {
                        messageTable.Data = messageTable.JsonDeserialise<KnuxLib.Engines.ProjectM.MessageTable.FormatData>(arg);
                        messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.dat");
                    }
                    break;
                #endregion

                #region Rockman X7 Engine Formats.
                case ".328f438b": case ".osd": using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new(arg, true)) break;
                case ".rockmanx7.stageentitytable.json":
                    // If an extension isn't specified, then ask the user what to save as.
                    if (extension == null)
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please specifiy the extension to save with;\n" +
                            "1. .OSD (PlayStation 2/PC)\n" +
                            "2. .328F438B (Legacy Collection)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "OSD"; break;
                            case '2': extension = "328F438B"; break;
                        }
                    }

                    // Sanity check that extension actually has a value.
                    if (extension != null)
                    {
                        using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                        {
                            stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                            stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                        }
                        break;
                    }
                    break;
                #endregion

                #region Rockman X8 Engine Formats.
                case ".31bf570e": using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new(arg, KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.LegacyCollection, true)) break;
                case ".set": using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new(arg, KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.Original, true)) break;
                case ".rockmanx8.stageentitytable.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. PC\n" +
                            "2. Legacy Collection"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "rx8_pc"; break;
                            case '2': version = "rx8_legacycollection"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "rx8_pc":
                                using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new())
                                {
                                    stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX8.StageEntityTable.SetObject>>(arg);
                                    stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.SET", KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.Original);
                                }
                                break;
                            case "rx8_legacycollection":
                                using (KnuxLib.Engines.RockmanX8.StageEntityTable stageEntityTable = new())
                                {
                                    stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX8.StageEntityTable.SetObject>>(arg);
                                    stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.31BF570E", KnuxLib.Engines.RockmanX8.StageEntityTable.FormatVersion.LegacyCollection);
                                }
                                break;
                        }
                    }
                    break;
                #endregion

                #region Sonic The Portable Engine Formats.
                case ".amb": using (KnuxLib.Engines.Portable.AMB amb = new(arg, true)) break;
                #endregion

                #region Sonic Storybook Engine Formats.
                case ".bin":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Sonic and the Secret Rings\n" +
                            "2. Sonic and the Black Knight"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "storybook_secretrings"; break;
                            case '2': version = "storybook_blackknight"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "storybook_secretrings":
                                using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings, true))
                                break;
                            case "storybook_blackknight":
                                using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight, true))
                                break;
                        }
                    }
                    break;
                case ".storybook.stageentitytableitems.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Sonic and the Secret Rings\n" +
                            "2. Sonic and the Black Knight"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "storybook_secretrings"; break;
                            case '2': version = "storybook_blackknight"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "storybook_secretrings":
                                using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                                {
                                    setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                    setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings);
                                }
                                break;
                            case "storybook_blackknight":
                                using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                                {
                                    setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                    setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight);
                                }
                                break;
                        }
                    }
                    break;

                case ".mtx":
                    bool isJapanese = false;
                    if (arg.ToUpper().Contains("JAPANESE"))
                        isJapanese = true;

                    using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new(arg, isJapanese, true))

                    break;
                case ".storybook.messagetable_secretrings.json":
                    bool isJapaneseSave = false;
                    if (arg.ToUpper().Contains("JAPANESE"))
                        isJapaneseSave = true;

                    using (KnuxLib.Engines.Storybook.MessageTable_SecretRings messageTable = new())
                    {
                        messageTable.Data = messageTable.JsonDeserialise<List<KnuxLib.Engines.Storybook.MessageTable_SecretRings.MessageEntry>>(arg);
                        messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.MTX", isJapaneseSave);
                    }
                    break;

                case ".txd": using (KnuxLib.Engines.Storybook.TextureDirectory textureDirectory = new(arg, true)) break;
                #endregion

                #region Sonic World Adventure Wii Engine Formats.
                case ".onz": using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true)) break;

                case ".wap":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. PlayStation 2\n" +
                            "2. Wii"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "swawii_ps2"; break;
                            case '2': version = "swawii_wii"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "swawii_ps2": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.PlayStation2, true)) break;
                            case "swawii_wii": using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new(arg, KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.Wii, true)) break;
                        }
                    }
                    break;
                case ".worldadventurewii.areapoints.json":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. PlayStation 2\n" +
                            "2. Wii"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "swawii_ps2"; break;
                            case '2': version = "swawii_wii"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "swawii_ps2":
                                using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new())
                                {
                                    areaPoints.Data = areaPoints.JsonDeserialise<List<KnuxLib.Engines.WorldAdventureWii.AreaPoints.Area>>(arg);
                                    areaPoints.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wap", KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.PlayStation2);
                                }
                                break;
                            case "swawii_wii":
                                using (KnuxLib.Engines.WorldAdventureWii.AreaPoints areaPoints = new())
                                {
                                    areaPoints.Data = areaPoints.JsonDeserialise<List<KnuxLib.Engines.WorldAdventureWii.AreaPoints.Area>>(arg);
                                    areaPoints.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wap", KnuxLib.Engines.WorldAdventureWii.AreaPoints.FormatVersion.Wii);
                                }
                                break;
                        }
                    }
                    break;
                #endregion

                #region Wayforward Engine Formats.
                case ".clb":
                    // If a version isn't specified, then ask the user what to read as.
                    if (version == null)
                    {
                        Console.WriteLine
                        (
                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                            "1. Shantae: Half-Genie Hero\n" +
                            "2. Shantae and the Seven Sirens"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': version = "clb_hero"; break;
                            case '2': version = "clb_sevensirens"; break;
                        }
                    }

                    // Sanity check that version actually has a value.
                    if (version != null)
                    {
                        switch (version)
                        {
                            case "clb_hero":
                                using (KnuxLib.Engines.Wayforward.Collision collision = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.hero, true))
                                    break;
                            case "clb_sevensirens":
                                using (KnuxLib.Engines.Wayforward.Collision collision = new(arg, KnuxLib.Engines.Wayforward.Collision.FormatVersion.sevensirens, true))
                                    break;
                        }
                    }
                    break;

                case ".env": using (KnuxLib.Engines.Wayforward.Environment env = new(arg, true)) break;
                case ".wayforward.environment.json":
                    using (KnuxLib.Engines.Wayforward.Environment env = new())
                    {
                        env.Data = env.JsonDeserialise<KnuxLib.Engines.Wayforward.Environment.Entity[]>(arg);
                        env.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.env");
                    }
                    break;

                case ".lgb": using (KnuxLib.Engines.Wayforward.Layers lgb = new(arg, true)) break;
                case ".wayforward.layers.json":
                    using (KnuxLib.Engines.Wayforward.Layers lgb = new())
                    {
                        lgb.Data = lgb.JsonDeserialise<List<string>>(arg);
                        lgb.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.lgb");
                    }
                    break;

                case ".ltb": using (KnuxLib.Engines.Wayforward.ListTable ltb = new(arg, true)) break;
                case ".wayforward.listtable.json":
                    using (KnuxLib.Engines.Wayforward.ListTable ltb = new())
                    {
                        ltb.Data = ltb.JsonDeserialise<KnuxLib.Engines.Wayforward.ListTable.FormatData>(arg);
                        ltb.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ltb");
                    }
                    break;

                case ".pak": using (KnuxLib.Engines.Wayforward.Package pak = new(arg, true)) break;
                #endregion

                #region Westwood Engine Formats.
                case ".tre":
                case ".tru":
                    using (KnuxLib.Engines.Westwood.MessageTable messageTable = new(arg, true))
                    break;
                case ".westwood.messagetable.json":
                    // If an extension isn't specified, then ask the user what to save as.
                    if (extension == null)
                    {
                        // List our supported extension options.
                        Console.WriteLine
                        (
                            "This file has multiple file extension options, please specifiy the extension to save with;\n" +
                            "1. .tre (USA)\n" +
                            "2. .tru (UK)"
                        );

                        // Wait for the user to input an option.
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1': extension = "tre"; break;
                            case '2': extension = "tru"; break;
                        }
                    }

                    // Sanity check that extension actually has a value.
                    if (extension != null)
                    {
                        using (KnuxLib.Engines.Westwood.MessageTable messageTable = new())
                        {
                            messageTable.Data = messageTable.JsonDeserialise<List<KnuxLib.Engines.Westwood.MessageTable.Message>>(arg);
                            messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.{extension}");
                        }
                    }
                    break;
                #endregion

                // If a command line argument without a corresponding format has been passed, then inform the user.
                default:
                    Console.WriteLine($"\n\nFormat extension {KnuxLib.Helpers.GetExtension(arg).ToLower()} is not valid for any currently supported file formats.\nPress any key to continue.");
                    Console.ReadKey();
                    break;
            }
        }
    }
}