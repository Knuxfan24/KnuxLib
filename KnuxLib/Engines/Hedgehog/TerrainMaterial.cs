namespace KnuxLib.Engines.Hedgehog
{
    public class TerrainMaterial : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public TerrainMaterial() { }
        public TerrainMaterial(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.terrain-material.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Material[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.terrain-material");
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
        public class Material
        {
            /// <summary>
            /// Some sort of type indicator for this material, doesn't seem to do anything?
            /// </summary>
            public string Type { get; set; } = "grass";

            /// <summary>
            /// The index used to identify this material, presumably in the heightfield files?
            /// </summary>
            public uint Index { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// The detailed diffuse map for this material, used when close to the material.
            /// </summary>
            public string DetailDiffuse { get; set; } = "terrain_detail_abd";

            /// <summary>
            /// The detailed normal map for this material, used when close to the material.
            /// </summary>
            public string DetailNormal { get; set; } = "terrain_detail_nrm";

            /// <summary>
            /// The detailed height map for this material, used when close to the material.
            /// </summary>
            public string DetailHeight { get; set; } = "terrain_detail_hgt";

            /// <summary>
            /// The base diffuse map for this material, used at distance.
            /// </summary>
            public string BaseDiffuse { get; set; } = "terrain_base_abd";

            /// <summary>
            /// The base PRM map for this material, used at distance.
            /// TODO: Is this actually just used as the PRM map for this material at all times?
            /// </summary>
            public string BasePRM { get; set; } = "terrain_base_prm";

            /// <summary>
            /// Displays this material's detailed diffuse map in the debugger.
            /// </summary>
            public override string ToString() => DetailDiffuse;

            /// <summary>
            /// Initialises this material with default data.
            /// </summary>
            public Material() { }

            /// <summary>
            /// Initialises this material with the provided data.
            /// </summary>
            public Material(string type, uint index, uint unknownUInt32_1, uint unknownUInt32_2, string detailDiffuse, string detailNormal, string detailHeight, string baseDiffuse, string basePRM)
            {
                Type = type;
                Index = index;
                UnknownUInt32_1 = unknownUInt32_1;
                UnknownUInt32_2 = unknownUInt32_2;
                DetailDiffuse = detailDiffuse;
                DetailNormal = detailNormal;
                DetailHeight = detailHeight;
                BaseDiffuse = baseDiffuse;
                BasePRM = basePRM;
            }

            /// <summary>
            /// Initialises this instance by reading its data from a BINAReader.
            /// </summary>
            public Material(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this material.
            /// </summary>
            public void Read(BINAReader reader)
            {
                Type = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                Index = reader.ReadUInt32();
                UnknownUInt32_1 = reader.ReadUInt32();
                UnknownUInt32_2 = reader.ReadUInt32();
                reader.JumpAhead(0x04); // Always 0.
                DetailDiffuse = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                DetailNormal = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                DetailHeight = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                BaseDiffuse = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                reader.JumpAhead(0x08); // Offset to empty string.
                BasePRM = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
            }

            /// <summary>
            /// Writes the data for this material.
            /// </summary>
            public void Write(BINAWriter writer, int index)
            {
                writer.AddString($"Material{index}Type", Type, 0x08);
                writer.Write(Index);
                writer.Write(UnknownUInt32_1);
                writer.Write(UnknownUInt32_2);
                writer.Write(0x00);
                writer.AddString($"Material{index}DetailDiffuse", DetailDiffuse, 0x08);
                writer.AddString($"Material{index}DetailNormal", DetailNormal, 0x08);
                writer.AddString($"Material{index}DetailHeight", DetailHeight, 0x08);
                writer.AddString($"Material{index}BaseDiffuse", BaseDiffuse, 0x08);
                writer.AddString($"Material{index}Unused", "", 0x08, true);
                writer.AddString($"Material{index}BasePRM", BasePRM, 0x08);
            }
        }

        // Actual data presented to the end user.
        public Material[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "MTDN");

            // Skip an unknown value that is always 1.
            reader.JumpAhead(0x04);

            // Read the offset to this file's material table.
            long materialTableOffset = reader.ReadInt64();

            // Initialise the data array.
            Data = new Material[reader.ReadInt64()];

            // Jump to this file's material table.
            reader.JumpTo(materialTableOffset, false);

            // Loop through and read each material in this file.
            for (int materialIndex = 0; materialIndex < Data.Length; materialIndex++)
                Data[materialIndex] = new(reader);

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(210);

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Write this file's signature.
            writer.Write("MTDN");

            // Write an unknown value that is always 1.
            writer.Write(0x01);

            // Add an offset for this file's material table.
            writer.AddOffset("MaterialTableOffset", 0x08);

            // Write the amount of materials in this file.
            writer.Write((ulong)Data.Length);

            // Fill in the offset for this file's material table.
            writer.FillInOffset("MaterialTableOffset", false);

            // Loop through and write each material.
            for (int materialIndex = 0; materialIndex < Data.Length; materialIndex++)
                Data[materialIndex].Write(writer, materialIndex);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
