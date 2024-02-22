namespace KnuxLib.Engines.Hedgehog
{
    public class TerrainMaterial : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public TerrainMaterial() { }
        public TerrainMaterial(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.terrain-material.json", Data);
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
        }

        // Actual data presented to the end user.
        public List<Material> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "MTDN";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value that is always 1.
            reader.JumpAhead(0x04);

            // Read the offset to this file's material table.
            long materialTableOffset = reader.ReadInt64();

            // Read the amount of materials in this file.
            ulong materialCount = reader.ReadUInt64();

            // Jump to this file's material table.
            reader.JumpTo(materialTableOffset, false);

            // Loop through each material in this file.
            for (ulong materialIndex = 0; materialIndex < materialCount; materialIndex++)
            {
                // Set up a new material entry.
                Material material = new();
                
                // Read this material's type.
                material.Type = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this material's index value.
                material.Index = reader.ReadUInt32();

                // Read this material's first unknown interger value.
                material.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this material's second unknown interger value.
                material.UnknownUInt32_2 = reader.ReadUInt32();

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x04);

                // Read this material's detail diffuse map.
                material.DetailDiffuse = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this material's detail normal map.
                material.DetailNormal = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this material's detail height map.
                material.DetailHeight = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this material's base diffuse map.
                material.BaseDiffuse = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Skip an unknown offset that always points to an empty string.
                // TODO: Fiddle with this and see if the game can pull a texture from here.
                reader.JumpAhead(0x08);

                // Read this material's base PRM map.
                material.BasePRM = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Save this material.
                Data.Add(material);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write the MTDN signature.
            writer.WriteSignature(Signature);

            // Write an unknown value that is always 1.
            writer.Write(0x01);

            // Add an offset for this file's material table.
            writer.AddOffset("MaterialTableOffset", 0x08);

            // Write the amount of materials in this file.
            writer.Write((ulong)Data.Count);

            // Fill in the offset for this file's material table.
            writer.FillInOffset("MaterialTableOffset", false);

            // Loop through each material.
            for (int materialIndex = 0; materialIndex < Data.Count; materialIndex++)
            {
                // Add a string for this material's type.
                writer.AddString($"Material{materialIndex}Type", Data[materialIndex].Type, 0x08);

                // Write this material's index value.
                writer.Write(Data[materialIndex].Index);

                // Write this material's first unknown integer value.
                writer.Write(Data[materialIndex].UnknownUInt32_1);

                // Write this material's second unknown integer value.
                writer.Write(Data[materialIndex].UnknownUInt32_2);

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Add a string for this material's detail diffuse map.
                writer.AddString($"Material{materialIndex}DetailDiffuse", Data[materialIndex].DetailDiffuse, 0x08);

                // Add a string for this material's detail normal map.
                writer.AddString($"Material{materialIndex}DetailNormal", Data[materialIndex].DetailNormal, 0x08);

                // Add a string for this material's detail height map.
                writer.AddString($"Material{materialIndex}DetailHeight", Data[materialIndex].DetailHeight, 0x08);

                // Add a string for this material's base diffuse map.
                writer.AddString($"Material{materialIndex}BaseDiffuse", Data[materialIndex].BaseDiffuse, 0x08);

                // Add a string for this material's empty value.
                // HedgeLib# doesn't write this string and just leaves the offset null, is that a problem?
                writer.AddString($"Material{materialIndex}Unused", "", 0x08);

                // Add a string for this material's base PRM map.
                writer.AddString($"Material{materialIndex}BasePRM", Data[materialIndex].BasePRM, 0x08);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
