using System.Globalization;
using System.Numerics;
using System.Text;

namespace KnuxTools
{
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
                // Loop through each argument.
                foreach (string arg in args)
                {
                    // Directory based checks.
                    if (Directory.Exists(arg))
                    {
                        // Print the directory name.
                        Console.WriteLine($"Directory: {arg}\n");

                        // Ask the user what to pack this folder as.
                        Console.WriteLine
                        (
                            "Please specify the archive type to pack this directory into;\n" +
                            "1. Alchemy Engine GFC/GOB File Pair\n" +
                            "2. Gods Engine WAD File\n" +
                            "3. Sonic The Portable AMB File\n" +
                            "4. Sonic Storybook Engine ONE File\n" +
                            "5. Sonic Storybook Engine TXD File\n" +
                            "6. Sonic World Adventure Wii ONE File."
                        );
                        switch (Console.ReadKey().KeyChar)
                        {
                            case '1':
                                using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new())
                                {
                                    assetsContainer.Import(arg);
                                    assetsContainer.Save($@"{arg}");
                                }
                                break;
                            case '2':
                                // Ask the user for the WAD version to save as.
                                Console.WriteLine
                                (
                                    "\n\nThis file has multiple file version options, please specifiy the version to save with;\n" +
                                    "1. Ninjabread Man PC/PS2\n" +
                                    "2. Ninjabread Man Wii"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.Gods.WAD wad = new())
                                        {
                                            wad.Import(arg);
                                            wad.Save($@"{Path.GetDirectoryName(arg)}.wad", KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_PCPS2);
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.Gods.WAD wad = new())
                                        {
                                            wad.Import(arg);
                                            wad.Save($@"{Path.GetDirectoryName(arg)}.wad", KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_Wii);
                                        }
                                        break;
                                }
                                break;
                            case '3':
                                using (KnuxLib.Engines.Portable.AMB amb = new())
                                {
                                    Console.WriteLine("\n");
                                    amb.Import(arg);
                                    amb.Save($@"{arg}.amb");
                                }
                                break;
                            case '4':
                                using (KnuxLib.Engines.Storybook.ONE one = new())
                                {
                                    Console.WriteLine("\n");
                                    one.Import(arg);
                                    one.Save($@"{arg}.one");
                                }
                                break;
                            case '5':
                                using (KnuxLib.Engines.Storybook.TextureDirectory textureDirectory = new())
                                {
                                    Console.WriteLine("\n");
                                    textureDirectory.Import(arg);
                                    textureDirectory.Save($@"{arg}.txd");
                                }
                                break;
                            case '6':
                                using (KnuxLib.Engines.WorldAdventureWii.ONE one = new())
                                {
                                    Console.WriteLine("\n");
                                    one.Import(arg);
                                    one.Save($@"{arg}.one");
                                }
                                break;
                        }
                        break;
                    }

                    // File based checks.
                    if (File.Exists(arg))
                    {
                        // Print the file name.
                        Console.WriteLine($"File: {arg}\n");

                        // Treat this file differently depending on type.
                        switch (KnuxLib.Helpers.GetExtension(arg).ToLower())
                        {
                            #region Seralised Models
                            case ".fbx":
                            case ".dae":
                            case ".obj":
                                // Ask the user what to convert this model to.
                                Console.WriteLine
                                (
                                    "This file is a generic seralised type, please specify what format it is;\n" +
                                    "1. CarZ Engine Model"
                                );

                                // Convert the model to the selected format.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
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
                                break;
                            #endregion

                            #region Seralised Data
                            case ".hedgehog.archiveinfo.json":
                                using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new())
                                {
                                    archiveInfo.Data = archiveInfo.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.ArchiveInfo.ArchiveEntry>>(arg);
                                    archiveInfo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.arcinfo");
                                }
                                break;

                            case ".hedgehog.gismov3.json":
                                using (KnuxLib.Engines.Hedgehog.GismoV3 gismo = new())
                                {
                                    gismo.Data = gismo.JsonDeserialise<KnuxLib.Engines.Hedgehog.GismoV3.FormatData>(arg);
                                    gismo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.gismod");
                                }
                                break;

                            case ".hedgehog.pointcloud.json":
                                // Ask the user for the extension to save with.
                                Console.WriteLine
                                (
                                    "This file has multiple file extension options, please specifiy the extension to save with;\n" +
                                    "1. .pccol (Collision Instance)\n" +
                                    "2. .pcmodel (Terrain Instance)\n" +
                                    "3. .pcrt (Lighting Instance)"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new())
                                        {
                                            pointCloud.Data = pointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.PointCloud.Instance>>(arg);
                                            pointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.pccol");
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new())
                                        {
                                            pointCloud.Data = pointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.PointCloud.Instance>>(arg);
                                            pointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.pcmodel");
                                        }
                                        break;
                                    case '3':
                                        using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new())
                                        {
                                            pointCloud.Data = pointCloud.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.PointCloud.Instance>>(arg);
                                            pointCloud.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.pcrt");
                                        }
                                        break;
                                }
                                break;

                            case ".hedgehog.sectorvisiblitycollision.json":
                                using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision gismo = new())
                                {
                                    gismo.Data = gismo.JsonDeserialise<List<KnuxLib.Engines.Hedgehog.SectorVisibilityCollision.SectorVisibilityShape>>(arg);
                                    gismo.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin.svcol");
                                }
                                break;

                            case ".nu2.aientitytable.json":
                                // Ask the user for the version to save with.
                                Console.WriteLine
                                (
                                    "This file has multiple file version options, please specifiy the version to save with;\n" +
                                    "1. GameCube\n" +
                                    "2. PlayStation2/Xbox"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                                        {
                                            aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                            aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube);
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new())
                                        {
                                            aiEntityTable.Data = aiEntityTable.JsonDeserialise<List<KnuxLib.Engines.Nu2.AIEntityTable.AIEntity>>(arg);
                                            aiEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.ai", KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox);
                                        }
                                        break;
                                }
                                break;

                            case ".nu2.wumpatable.json":
                                // Ask the user for the version to save with.
                                Console.WriteLine
                                (
                                    "This file has multiple file version options, please specifiy the version to save with;\n" +
                                    "1. GameCube\n" +
                                    "2. PlayStation2/Xbox"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                                        {
                                            wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                            wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube);
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new())
                                        {
                                            wumpaTable.Data = wumpaTable.JsonDeserialise<List<Vector3>>(arg);
                                            wumpaTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.wmp", KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox);
                                        }
                                        break;
                                }
                                break;

                            case ".projectm.messagetable.json":
                                using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new())
                                {
                                    messageTable.Data = messageTable.JsonDeserialise<KnuxLib.Engines.ProjectM.MessageTable.FormatData>(arg);
                                    messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.dat");
                                }
                                break;

                            case ".rockmanx7.stageentitytable.json":
                                // Ask the user for the extension to save with.
                                Console.WriteLine
                                (
                                    "This file has multiple file extension options, please specifiy the extension to save with;\n" +
                                    "1. .OSD (PlayStation 2/PC)\n" +
                                    "2. .328F438B (Legacy Collection)"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                                        {
                                            stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                                            stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.OSD");
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new())
                                        {
                                            stageEntityTable.Data = stageEntityTable.JsonDeserialise<List<KnuxLib.Engines.RockmanX7.StageEntityTable.SetObject>>(arg);
                                            stageEntityTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.328F438B");
                                        }
                                        break;
                                }
                                break;

                            case ".storybook.stageentitytableitems.json":
                                // Ask the user for the version to save with.
                                Console.WriteLine
                                (
                                    "This file has multiple file version options, please specifiy the version to save with;\n" +
                                    "1. Sonic and the Secret Rings\n" +
                                    "2. Sonic and the Black Knight"
                                );
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                                        {
                                            setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                            setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings);
                                        }
                                        break;
                                    case '2':
                                        using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new())
                                        {
                                            setItems.Data = setItems.JsonDeserialise<KnuxLib.Engines.Storybook.StageEntityTableItems.FormatData>(arg);
                                            setItems.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.bin", KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight);
                                        }
                                        break;
                                }
                                break;

                            case ".westwood.messagetable.json":
                                using (KnuxLib.Engines.Westwood.MessageTable messageTable = new())
                                {
                                    messageTable.Data = messageTable.JsonDeserialise<List<string>>(arg);
                                    messageTable.Save($@"{KnuxLib.Helpers.GetExtension(arg, true)}.tru");
                                }
                                break;
                            #endregion

                            #region Generic Extensions
                            case ".bin":
                                // Ask the user what to read this file as.
                                Console.WriteLine
                                (
                                    "This file is a generic type, please specify what format it is;\n" +
                                    "1. Sonic Storybook Engine Stage Entity Table Object Table"
                                );

                                // Read this file with the selected format.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1':
                                        // Ask the user for the file version.
                                        Console.WriteLine
                                                (
                                                    "\n\nThis file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                                                    "1. Sonic and the Secret Rings\n" +
                                                    "2. Sonic and the Black Knight"
                                                );

                                        // Read the file according to the selected version.
                                        switch (Console.ReadKey().KeyChar)
                                        {
                                            case '1': using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.SecretRings, true)) break;
                                            case '2': using (KnuxLib.Engines.Storybook.StageEntityTableItems setItems = new(arg, KnuxLib.Engines.Storybook.StageEntityTableItems.FormatVersion.BlackKnight, true)) break;
                                        }
                                        break;
                                }
                                break;

                            case ".one":
                                // Ask the user for the file version.
                                Console.WriteLine
                                        (
                                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                                            "1. Sonic Storybook Engine ONE Archive\n" +
                                            "2. Sonic World Adventure Wii Uncompressed ONE Archive\n" +
                                            "3. Compress to Sonic World Adventure ONZ Archive."
                                        );

                                // Read the file according to the selected version.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1': using (KnuxLib.Engines.Storybook.ONE one = new(arg, true)) break;
                                    case '2': using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true)) break;
                                    case '3':
                                        MemoryStream buffer = new();
                                        PuyoTools.Core.Compression.Lz11Compression lz11 = new();
                                        buffer = lz11.Compress(File.OpenRead(arg));
                                        buffer.WriteTo(File.Create($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.onz"));
                                        break;
                                }
                                break;

                            #endregion

                            #region Alchemy Engine Formats
                            case ".gfc": using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new(arg, true)) break;
                            case ".gob": using (KnuxLib.Engines.Alchemy.AssetsContainer assetsContainer = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.gfc", true)) break;
                            #endregion

                            #region CarZ Engine Formats
                            case ".mat": using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new(arg, true)) break;
                            case ".sco": using (KnuxLib.Engines.CarZ.SCO sco = new(arg, true)) break;
                            #endregion

                            #region Gods Engine Formats
                            case ".wad":
                                // Ask the user for the WAD version.
                                Console.WriteLine
                                        (
                                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                                            "1. Ninjabread Man PC/PS2\n" +
                                            "2. Ninjabread Man Wii"
                                        );

                                // Extract the WAD according to the selected version.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1': using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_PCPS2, true)) break;
                                    case '2': using (KnuxLib.Engines.Gods.WAD wad = new(arg, KnuxLib.Engines.Gods.WAD.FormatVersion.NinjabreadMan_Wii, true)) break;
                                }
                                break;
                            #endregion

                            #region Hedgehog Engine Formats
                            case ".arcinfo": using (KnuxLib.Engines.Hedgehog.ArchiveInfo archiveInfo = new(arg, true)) break;
                            case ".gismod": using (KnuxLib.Engines.Hedgehog.GismoV3 gismo = new(arg, true)) break;
                            case ".gismop": using (KnuxLib.Engines.Hedgehog.GismoV3 gismo = new($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg).Replace("_pln", "")}.gismod", true)) break;
                            case ".pcmodel": case ".pccol": case ".pcrt": using (KnuxLib.Engines.Hedgehog.PointCloud pointCloud = new(arg, true)) break;
                            case ".svcol.bin": using (KnuxLib.Engines.Hedgehog.SectorVisibilityCollision sectorVisibilityCollision = new(arg, true)) break;
                            #endregion

                            #region Nu2 Engine Formats
                            case ".ai":
                                // Ask the user for the ai version.
                                Console.WriteLine
                                        (
                                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                                            "1. GameCube\n" +
                                            "2. PlayStation 2/Xbox"
                                        );

                                // Seralise the ai file according to the selected version.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1': using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.GameCube, true)) break;
                                    case '2': using (KnuxLib.Engines.Nu2.AIEntityTable aiEntityTable = new(arg, KnuxLib.Engines.Nu2.AIEntityTable.FormatVersion.PlayStation2Xbox, true)) break;
                                }
                                break;
                            case ".wmp":
                                // Ask the user for the wmp version.
                                Console.WriteLine
                                        (
                                            "This file has multiple variants that can't be auto detected, please specifiy the variant;\n" +
                                            "1. GameCube\n" +
                                            "2. PlayStation 2/Xbox"
                                        );

                                // Seralise the wmp file according to the selected version.
                                switch (Console.ReadKey().KeyChar)
                                {
                                    case '1': using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.GameCube, true)) break;
                                    case '2': using (KnuxLib.Engines.Nu2.WumpaTable wumpaTable = new(arg, KnuxLib.Engines.Nu2.WumpaTable.FormatVersion.PlayStation2Xbox, true)) break;
                                }
                                break;
                            #endregion

                            #region Project M Engine Formats
                            case ".dat": using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new(arg, true)) break;
                            #endregion

                            #region Rockman X7 Engine Formats
                            case ".328f438b": case ".osd": using (KnuxLib.Engines.RockmanX7.StageEntityTable stageEntityTable = new(arg, true)) break;
                            #endregion

                            #region Sonic Storybook Engine Formats
                            case ".txd": using (KnuxLib.Engines.Storybook.TextureDirectory textureDirectory = new(arg, true)) break;
                            #endregion

                            #region Sonic The Portable Engine Formats
                            case ".amb": using (KnuxLib.Engines.Portable.AMB amb = new(arg, true)) break;
                            #endregion

                            #region Sonic World Adventure Wii Engine Formats
                            case ".onz": using (KnuxLib.Engines.WorldAdventureWii.ONE one = new(arg, true)) break;
                            #endregion

                            #region Westwood Engine Formats
                            case ".tru": using (KnuxLib.Engines.Westwood.MessageTable messageTable = new(arg, true)) break;
                            #endregion
                        }
                    }
                }
            }

            // If there are no arguments, then inform the user.
            else
            {
                Console.WriteLine("Command line tool used to convert the following supported file types to various other formats.\n" +
                                  "Each format converts to and from a JSON file unless otherwise specified.\n");

                Console.WriteLine("Alchemy Engine:\n" +
                                  "Assets Container Archive Pair (.gfc/gob) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("CarZ Engine:\n" +
                                  "Material Library (.mat) - Exports to the MTL material library standard and imports from an Assimp compatible model.\n" +
                                  "3D Model (.sco) - Exports to the Wavefront OBJ model standard and imports from an Assimp compatible model.\n");

                Console.WriteLine("Gods Engine:\n" +
                                  "WAD Archive (.wad) - Extracts to a directory of the same name as the input archive (importing not yet possible).\n");

                Console.WriteLine("Hedgehog Engine:\n" +
                                  "Archive Info (.arcinfo)\n" +
                                  "Point Cloud (.pccol/.pcmodel/.pcrt)\n" +
                                  "Gismo V3 (.gismod/.gismop)\n" +
                                  "Sector Visibility Collision (.bin.svcol)\n");

                Console.WriteLine("Nu2 Engine:\n" +
                                  "Wumpa Fruit Table (.wmp)\n");

                Console.WriteLine("Project M Engine:\n" +
                                  "Message Table (.dat)\n");

                Console.WriteLine("Rockman X7 Engine:\n" +
                                  "Stage Entity Table (.328f438b/.osd)\n");

                Console.WriteLine("Sonic The Portable Engine:\n" +
                                  "AMB Archive (.amb) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.\n");

                Console.WriteLine("Sonic Storybook Engine:\n" +
                                  "ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.\n" +
                                  "Stage Entity Table Object Table (.bin)\n" +
                                  "Texture Directory (.txd) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.\n");

                Console.WriteLine("Sonic World Adventure Wii Engine:\n" +
                                  "ONE Archive (.one/.onz) - Extracts to a directory of the same name as the input archive and creates an archive from an input directory.\n");

                Console.WriteLine("Westwood Engine:\n" +
                                  "Message Table (.tru)\n");

                Console.WriteLine("Usage:\n" +
                                  "KnuxTools.exe \"path\\to\\supported\\file\"\n" +
                                  "Alternatively, simply drag a supported file onto this application in Windows Explorer.\n\n" +
                                  "Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
}