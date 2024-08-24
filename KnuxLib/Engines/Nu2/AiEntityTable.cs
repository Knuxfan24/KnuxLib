namespace KnuxLib.Engines.Nu2
{
    public class AIEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AIEntityTable() { }
        public AIEntityTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".nu2.aientitytable.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Entity[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.ai", version);
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

        public class Entity
        {
            // The type of entity this is.
            public string Type { get; set; } = "";

            // The points in space this entity needs to reference.
            public Vector3[] Positions { get; set; } = [];

            /// <summary>
            /// Displays this entity's type in the debugger.
            /// </summary>
            public override string ToString() => Type;

            /// <summary>
            /// Initialises this entity with default data.
            /// </summary>
            public Entity() { }

            /// <summary>
            /// Initialises this entity with the provided data.
            /// </summary>
            public Entity(string type, Vector3[] positions)
            {
                Type = type;
                Positions = positions;
            }

            /// <summary>
            /// Initialises this entity by reading its data from a BinaryReader.
            /// </summary>
            public Entity(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this entity.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Type = reader.ReadNullPaddedString(0x10);
                Positions = new Vector3[reader.ReadUInt32()];
                for (int positionIndex = 0; positionIndex < Positions.Length; positionIndex++)
                    Positions[positionIndex] = reader.ReadVector3();
            }

            /// <summary>
            /// Writes the data for this entity.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.WriteNullPaddedString(Type, 0x10);
                writer.Write(Positions.Length);
                foreach (Vector3 position in Positions)
                    writer.Write(position);
            }
        }

        // Actual data presented to the end user.
        public Entity[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // If this is a GameCube ai file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Red this file's entity count.
            Data = new Entity[reader.ReadUInt32()];

            // Loop through and read each entity in this file.
            for (int entityIndex = 0; entityIndex < Data.Length; entityIndex++)
                Data[entityIndex] = new(reader);

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

            // If this is a GameCube ai file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write the count of entities in this file.
            writer.Write(Data.Length);

            // Loop through and write each entity.
            for (int entityIndex = 0; entityIndex < Data.Length; entityIndex++)
                Data[entityIndex].Write(writer);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
