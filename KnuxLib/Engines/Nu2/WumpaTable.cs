namespace KnuxLib.Engines.Nu2
{
    public class WumpaTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public WumpaTable() { }
        public WumpaTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".nu2.wumpatable.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Vector3[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.wmp", version);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            PlayStation2Xbox = 1
        }

        // Actual data presented to the end user.
        public Vector3[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // If this is a GameCube wmp file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Initialise the data array.
            Data = new Vector3[reader.ReadInt32()];

            // Loop through and read each Wumpa Fruit's coordinate values. 
            for (int wumpaFruitIndex = 0; wumpaFruitIndex < Data.Length; wumpaFruitIndex++)
                Data[wumpaFruitIndex] = reader.ReadVector3();

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // If this is a GameCube wmp file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write the count of Wumpa Fruit in this file.
            writer.Write(Data.Length);

            // Write each Wumpa Fruit's coordinates.
            for (int wumpaFruitIndex = 0; wumpaFruitIndex < Data.Length; wumpaFruitIndex++)
                writer.Write(Data[wumpaFruitIndex]);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
