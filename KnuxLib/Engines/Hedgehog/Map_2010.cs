namespace KnuxLib.Engines.Hedgehog
{
    public class Map_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Map_2010() { }
        public Map_2010(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.map_2010.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<FormatData>(filepath);

                //If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.map.bin");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The sector references that make up this map.
            /// </summary>
            public Sector[] Sectors { get; set; } = [];
        }

        public class Sector
        {
            /// <summary>
            /// The name of this sector.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The position of this sector in the map.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Displays this sector's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this sector with default data.
            /// </summary>
            public Sector() { }

            /// <summary>
            /// Initialises this sector with the provided data.
            /// </summary>
            public Sector(string name, Vector3 position)
            {
                Name = name;
                Position = position;
            }

            /// <summary>
            /// Initialises this sector by reading its data from a BINAReader.
            /// </summary>
            public Sector(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this sector.
            /// </summary>
            public void Read(BINAReader reader)
            {
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                reader.JumpAhead(0x04); // Index of this sector, always sequential.
                reader.JumpAhead(0x04); // Always 0.
                Position = reader.ReadVector3();
            }

            /// <summary>
            /// Writes the data for this instance.
            /// </summary>
            public void Write(BINAWriter writer, int index)
            {
                writer.AddString($"Sector{index}Name", Name, 0x04);
                writer.Write(index);
                writer.WriteNulls(0x04);
                writer.Write(Position);
            }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read the offset to this file's sector table.
            int sectorTableOffset = reader.ReadInt32();

            // Initalise the sectors array.
            Data.Sectors = new Sector[reader.ReadInt32()];

            // Read the unknown value in this data.
            Data.UnknownUInt32_1 = reader.ReadUInt32();

            // Jump to the sector table (should already be here but lets play it safe).
            reader.JumpTo(sectorTableOffset, false);

            // Loop through and read each sector.
            for (int sectorIndex = 0; sectorIndex < Data.Sectors.Length; sectorIndex++)
                Data.Sectors[sectorIndex] = new(reader);

            // Close our BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 1 Header.
            BINAv1Header header = new();

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Add an offset for the sector table.
            writer.AddOffset("sectorTableOffset", 0x04);

            // Write how many sectors are in this file.
            writer.Write(Data.Sectors.Length);

            // Write this file's unknown integer.
            writer.Write(Data.UnknownUInt32_1);

            // Fill in the offset for the sector table.
            writer.FillInOffset("sectorTableOffset");

            // Loop through and write each sector.
            for (int sectorIndex = 0; sectorIndex < Data.Sectors.Length; sectorIndex++)
                Data.Sectors[sectorIndex].Write(writer, sectorIndex);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
