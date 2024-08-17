namespace KnuxTools
{
    public class FormatPrints
    {
        public static void CapcomMT()
        {
            Console.WriteLine("===");
            Console.WriteLine("Capcom MT Framework Engine:");
            Console.WriteLine("Archive (.arc) - Extracts to a directory of the same name as the input archive and creates an archive from an input\r\ndirectory.");
            Helpers.ColourConsole("    Version Flag (importing) - capcomv7", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (importing) - capcomv9", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (importing) - capcomv9_uncompressed", true, ConsoleColor.Yellow);
        }

        public static void Hedgehog()
        {
            Console.WriteLine("===");
            Console.WriteLine("Hedgehog Engine:");
            Console.WriteLine("Archive Info (.arcinfo)");
            Console.WriteLine("Density Point Cloud (.densitypointcloud)");
            Console.WriteLine("Instance Info (.terrain-instanceinfo) - Import a folder containing files to generate a Sonic Frontiers point cloud file.");
            Console.WriteLine("2010 Map (.map.bin)");
            Console.WriteLine("Point Cloud (.pccol/.pcmodel/.pcrt/.pointcloud)");
            Helpers.ColourConsole("    Extension Flag (Collision Instance) - pccol", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Terrain Instance) - pcmodel", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Lighting Instance) - pcrt", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Generic) - pointcloud", true, ConsoleColor.Yellow);
        }

        public static void Nu2()
        {
            Console.WriteLine("===");
            Console.WriteLine("Nu2 Engine:");
            Console.WriteLine("Wumpa Fruit Table (.wmp)");
            Helpers.ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2", true, ConsoleColor.Yellow);
        }

        public static void StellarStone()
        {
            Console.WriteLine("===");
            Console.WriteLine("Stellar Stone Engine:");
            Console.WriteLine("Material Library (.mat) - Exports to the MTL material library standard and imports from an Assimp compatible model.");
            Console.WriteLine("Mesh Object (.sco) - Exports to the Wavefront OBJ model standard and imports from an Assimp compatible model.");
            Helpers.ColourConsole("    Version Flag (importing, shared with material library) - stellar", true, ConsoleColor.Yellow);
        }

        public static void SonicStorybook()
        {
            Console.WriteLine("===");
            Console.WriteLine("Sonic Storybook Engine:");
            Console.WriteLine("ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input\r\ndirectory.");
            Helpers.ColourConsole("    Version Flag - storybook", true, ConsoleColor.Yellow);
        }

        public static void Twinsanity()
        {
            Console.WriteLine("===");
            Console.WriteLine("Twinsanity Engine:");
            Console.WriteLine("Data Header Pair (.bd/.bh) - Extracts to a directory of the same name as the input archive and creates an archive from\r\nan input directory.");
            Helpers.ColourConsole("    Version Flag - twinsanity", true, ConsoleColor.Yellow);
        }
    }
}
