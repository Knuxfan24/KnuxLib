namespace KnuxLib.Engines.Hedgehog
{
    // TODO: The BINA Footer in files I write is slightly inaccurate, is this a problem?
    public class Map_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Map_2010() { }
        public Map_2010(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".map", "")}.hedgehog.map_2010.json", Data);
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
            public Sector[] Sectors { get; set; } = Array.Empty<Sector>();
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
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Skip an unknown value of 0x0C.
            reader.JumpAhead(0x04);

            // Read the count of sectors in this map.
            uint sectorCount = reader.ReadUInt32();

            // Read the unknown value in this data.
            Data.UnknownUInt32_1 = reader.ReadUInt32();

            // Initalise the sectors array.
            Data.Sectors = new Sector[sectorCount];

            // Loop through each sector.
            for (int sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                // Initalise a new sector.
                Sector sector = new();
                
                // Read this sector's name.
                sector.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, true);

                // Skip this sector's sequential index.
                reader.JumpAhead(0x04);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this sector's position.
                sector.Position = reader.ReadVector3();

                // Save this sector.
                Data.Sectors[sectorIndex] = sector;
            }

            // Close Marathon's BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BINAWriter.
            BINAWriter writer = new(File.Create(filepath));

            // Write an unknown value of 0x0C.
            writer.Write(0x0C);

            // Write the count of sectors in this map.
            writer.Write(Data.Sectors.Length);

            // Write this map's unknown value.
            writer.Write(Data.UnknownUInt32_1);

            // Loop through each sector.
            for (int sectorIndex = 0; sectorIndex < Data.Sectors.Length; sectorIndex++)
            {
                // Write this sector's name.
                writer.AddString($"Sector{sectorIndex}Name", Data.Sectors[sectorIndex].Name);

                // Write this sector's index.
                writer.Write(sectorIndex);

                // Write an unknown value of 0.
                writer.Write(0x00);

                // Write this sector's position.
                writer.Write(Data.Sectors[sectorIndex].Position);
            }

            // Finish writing the BINA information.
            writer.FinishWrite();

            // Close Marathon's BINAWriter.
            writer.Close();
        }
    }
}
