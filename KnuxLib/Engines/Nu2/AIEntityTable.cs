namespace KnuxLib.Engines.Nu2
{
    public class AIEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AIEntityTable() { }
        public AIEntityTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool extract = false)
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

        public class AIEntity
        {
            public string Type { get; set; } = "";

            public List<Vector3> Positions { get; set; } = new();
        }

        // Actual data presented to the end user.
        public List<AIEntity> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a GameCube ai file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Red this file's entity count.
            uint entityCount = reader.ReadUInt32();

            // Loop through each entity in this file.
            for (int i = 0; i < entityCount; i++)
            {
                // Set up a new entity entry.
                AIEntity entity = new();

                // Read this entity's type.
                entity.Type = reader.ReadNullPaddedString(0x10);

                // Read this entity's position count.
                uint entityPositionCount = reader.ReadUInt32();

                // Read each of this entity's position values.
                for (int p = 0; p < entityPositionCount; p++)
                    entity.Positions.Add(reader.ReadVector3());

                // Save this entity.
                Data.Add(entity);
            }

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

            // If this is a GameCube ai file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write the count of entities in this file.
            writer.Write(Data.Count);

            // Loop through each entity in this file.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write this entity's type.
                writer.WriteNullPaddedString(Data[i].Type, 0x10);

                // Write the amount of position values this entity uses.
                writer.Write(Data[i].Positions.Count);
                
                // Write each of this entity's position values.
                foreach (Vector3 position in Data[i].Positions)
                    writer.Write(position);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
