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
                                //Console.WriteLine
                                //(
                                //    "This file is a generic seralised type, please specify what format it is;\n" +
                                //    "1. CarZ Engine Model"
                                //);

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

                            // CarZ Engine Formats
                            case ".mat": using (KnuxLib.Engines.CarZ.MaterialLibrary mat = new(arg, true)) break;
                            case ".sco": using (KnuxLib.Engines.CarZ.SCO sco = new(arg, true)) break;

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