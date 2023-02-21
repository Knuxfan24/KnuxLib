using System.Diagnostics;

namespace KnuxTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (string scoFile in Directory.GetFiles(@"D:\Standalone Games\Big Rigs", "*.sco", SearchOption.AllDirectories))
            {
                Console.WriteLine(scoFile);
                KnuxLib.Engines.CarZ.SCO sco = new(scoFile);
                sco.Save($@"{Path.GetDirectoryName(scoFile)}\{Path.GetFileNameWithoutExtension(scoFile)}.resave_sco");
                sco.ExportOBJ($@"{Path.GetDirectoryName(scoFile)}\{Path.GetFileNameWithoutExtension(scoFile)}.obj");
            }
        }
    }
}