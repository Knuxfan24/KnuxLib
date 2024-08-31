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
            Console.WriteLine("Master Level Table (.mlevel)");
            Console.WriteLine("Message Table (sonic2010/blueblur/william) (.xtb)");
            Helpers.ColourConsole("    Version Flag (Sonic Colours) - sonic2010", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Generations) - blueblur", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Mario and Sonic at the London 2012 Olympic Games) - william", true, ConsoleColor.Yellow);
            Console.WriteLine("Message Table (sonic2013) (.xtb2)");
            Helpers.ColourConsole("    Version Flag (importing) - wiiu", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (importing) - pc", true, ConsoleColor.Yellow);
            Console.WriteLine("Path Spline (.path) (.path2.bin)");
            Helpers.ColourConsole("    Version Flag (Sonic Lost World (Wii U)) - sonic2013_cafe", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Lost World) - sonic2013", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Forces) - wars", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Frontiers) - rangers", true, ConsoleColor.Yellow);
            Console.WriteLine("Point Cloud (.pccol/.pcmodel/.pcrt/.pointcloud)");
            Helpers.ColourConsole("    Extension Flag (Collision Instance) - pccol", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Terrain Instance) - pcmodel", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Lighting Instance) - pcrt", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Extension Flag (Generic) - pointcloud", true, ConsoleColor.Yellow);
            Console.WriteLine("Sector Visibility Collision (.svcol.bin)");
            Helpers.ColourConsole("    Version Flag (Sonic Lost World (Wii U)) - sonic2013_cafe", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Lost World) - sonic2013", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic Forces) - wars", true, ConsoleColor.Yellow);
            Console.WriteLine("Terrain Material (.terrain-material)");
        }

        public static void Nintendo()
        {
            Console.WriteLine("===");
            Console.WriteLine("Nintendo Generic:");
            Console.WriteLine("U8 Archive (.arc) - Extracts to a directory of the same name as the input archive and creates an archive from an input\r\ndirectory.");
            Helpers.ColourConsole("    Version Flag - nintendo_u8", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic '06) - nintendo_u8_marathon", true, ConsoleColor.Yellow);
        }

        public static void Nu2()
        {
            Console.WriteLine("===");
            Console.WriteLine("Nu2 Engine:");
            Console.WriteLine("AI Entity Table (.ai)");
            Helpers.ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("Crate Table (.crt)");
            Helpers.ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2", true, ConsoleColor.Yellow);
            Console.WriteLine("Wumpa Fruit Table (.wmp)");
            Helpers.ColourConsole("    Version Flag (Gamecube) - gcn", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (PlayStation 2/Xbox) - ps2", true, ConsoleColor.Yellow);
        }

        public static void OpenSpace()
        {
            Console.WriteLine("===");
            Console.WriteLine("OpenSpace Engine:");
            Helpers.ColourConsole("Big File Archive (.bf/.dsc) - Extracts to a directory of the same name as the input archive and creates an archive from\r\nan input directory.");
            Helpers.ColourConsole("    Version Flag - openspace_big", true, ConsoleColor.Yellow);
        }

        public static void ProjectM()
        {
            Console.WriteLine("===");
            Console.WriteLine("Project M Engine:");
            Console.WriteLine("Message Table (.dat)");
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
            Console.WriteLine("Light Field (.bin)");
            Helpers.ColourConsole("    Version Flag - storybook_lightfield", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("Motion Table (.bin)");
            Helpers.ColourConsole("    Version Flag - storybook_motion", true, ConsoleColor.Yellow);
            Console.WriteLine("ONE Archive (.one) - Extracts to a directory of the same name as the input archive and creates an archive from an input\r\ndirectory.");
            Helpers.ColourConsole("    Version Flag - storybook", true, ConsoleColor.Yellow);
            Console.WriteLine("Stage Entity Table Object Table (.bin)");
            Helpers.ColourConsole("    Version Flag (Sonic and the Secret Rings) - storybook_setitems_sr", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Sonic and the Black Knight) - storybook_setitems_bk", true, ConsoleColor.Yellow);
        }

        public static void SonicWorldAdventureWii()
        {
            Console.WriteLine("===");
            Console.WriteLine("Sonic World Adventure Wii Engine:");
            Console.WriteLine("ONE Archive (.one/.onz) - Extracts to a directory of the same name as the input archive and creates an archive from an\r\ninput directory.");
            Helpers.ColourConsole("    Version Flag - swawii", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (Importing as .onz) - swawii_compressed", true, ConsoleColor.Yellow);
        }

        public static void SpaceChannel()
        {
            Console.WriteLine("===");
            Console.WriteLine("Space Channel Engine:");
            Console.WriteLine("Caption Table (.bin)");
            Helpers.ColourConsole("    Version Flag (Japanese) - spacechannel_caption_jpn", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag (International) - spacechannel_caption", true, ConsoleColor.Yellow);
        }

        public static void Twinsanity()
        {
            Console.WriteLine("===");
            Console.WriteLine("Twinsanity Engine:");
            Console.WriteLine("Data Header Pair (.bd/.bh) - Extracts to a directory of the same name as the input archive and creates an archive from\r\nan input directory.");
            Helpers.ColourConsole("    Version Flag - twinsanity", true, ConsoleColor.Yellow);
        }

        public static void Wayforward()
        {
            Console.WriteLine("===");
            Console.WriteLine("Wayforward Engine:");
            Console.WriteLine("Environment Table (.env)");
            Helpers.ColourConsole("    Version Flag - wayforward", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag - wayforward_bigendian", true, ConsoleColor.Yellow);
            Console.WriteLine("Package Archive (.pak) - Extracts to a directory of the same name as the input archive and creates an archive from an\r\ninput directory.");
            Helpers.ColourConsole("    Version Flag - wayforward", true, ConsoleColor.Yellow);
            Helpers.ColourConsole("    Version Flag - wayforward_bigendian", true, ConsoleColor.Yellow);
        }
    }
}
