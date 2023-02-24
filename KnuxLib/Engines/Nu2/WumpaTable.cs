namespace KnuxLib.Engines.Nu2
{
    public class WumpaTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public WumpaTable() { }
        public WumpaTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool extract = false)
        {
            Load(filepath, version);

            if (extract)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            PlayStation2Xbox = 1
        }

        // Actual data presented to the end user.
        public List<Vector3> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a GameCube wmp file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Read the count of Wumpa Fruit in this file.
            uint wumpaFruitCount = reader.ReadUInt32();

            // Read and save each Wumpa Fruit's coordinates.
            for (int i = 0; i < wumpaFruitCount; i++)
                Data.Add(reader.ReadVector3());

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If this is a GameCube wmp file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write the count of Wumpa Fruit in this file.
            writer.Write(Data.Count);

            // Write each Wumpa Fruit's coordinates.
            for (int i = 0; i < Data.Count; i++)
                writer.Write(Data[i]);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
