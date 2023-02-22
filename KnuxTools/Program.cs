using System.Globalization;

namespace KnuxTools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Force culture info 'en-GB' to prevent errors with values altered by culture-specific differences.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

            // Check for arguments.
            if (args.Length > 0)
            {
                // Loop through each argument.
                foreach (string arg in args)
                {
                    // Directory based checks.
                    if (Directory.Exists(arg))
                    {
                        // Ask the user what to pack this folder as (currently disabled as the only archive format I support is the Gods Engine WADs and I haven't written import stuff yet).
                        //Console.WriteLine
                        //(
                        //    "Please specify the archive type to pack this directory into;\n" +
                        //    "1. Gods Engine WAD File"
                        //);
                        break;
                    }

                    // File based checks.
                    if (File.Exists(arg))
                    {
                        // Print the file name.
                        Console.WriteLine($"File: {arg}\n");

                        // Treat this file differently depending on type.
                        switch (Path.GetExtension(arg).ToLower())
                        {
                            // Seralised Models
                            case ".fbx":
                            case ".dae":
                            case ".obj":
                                // Ask the user what to convert this model to (currently disabled as only the CarZ SCO format has this).
                                //Console.WriteLine
                                //(
                                //    "This file is a generic seralised type, please specify what format it is;\n" +
                                //    "1. CarZ Engine Model"
                                //);

                                // Convert the model to the selected format.
                                //switch (Console.ReadKey().KeyChar)
                                //{
                                //    case '1':
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
                                //        break;
                                //}
                                break;

                            // Seralised Data
                            case ".json":
                                // Ask the user what to convert this json to (currently disabled as only the Project M Message Table format has this).
                                //Console.WriteLine
                                //(
                                //    "This file is a generic seralised type, please specify what format it is;\n" +
                                //    "1. CarZ Engine Model"
                                //);

                                // Convert the model to the selected format.
                                //switch (Console.ReadKey().KeyChar)
                                //{
                                //    case '1':
                                using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new())
                                {
                                    messageTable.Data = messageTable.JsonDeserialise<KnuxLib.Engines.ProjectM.MessageTable.FormatData>(arg);
                                    messageTable.Save($@"{Path.GetDirectoryName(arg)}\{Path.GetFileNameWithoutExtension(arg)}.dat");
                                }
                                //        break;
                                //}
                                break;

                            // CarZ Engine Formats
                            case ".mat": using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new(arg, true)) break;
                            case ".sco": using (KnuxLib.Engines.CarZ.SCO sco = new(arg, true)) break;

                            // Gods Engine Formats
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

                            // ProjectM Engine Formats
                            case ".dat": using (KnuxLib.Engines.ProjectM.MessageTable messageTable = new(arg, true)) break;
                        }
                    }
                }
            }
            else
            {
                // TODO: Do something if there isn't any files passed.
            }
        }
    }
}