namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: Most of the data in the materials are unknowns, figure them out.
    // TODO: This data is different in the PlayStation 2 version, figure out how that version is structured.
    public class MaterialSet
    {
        // Classes for this NuObject chunk.
        public class MaterialData
        {
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
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_4 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

            /// <summary>
            /// The RGB value to colour this material with.
            /// </summary>
            public Vector3 Colour { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_6 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_7 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_4 { get; set; }

            /// <summary>
            /// The index of the texture in the file's texture set that this material should use.
            /// If this is set to -1, then it doesn't use a texture at all.
            /// </summary>
            public int TextureIndex { get; set; } = -1;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_5 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_6 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_7 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_8 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_9 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_10 { get; set; }
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static List<MaterialData> Read(BinaryReaderEx reader, Scene.FormatVersion version)
        {
            // Set up our list of materials.
            List<MaterialData> Materials = new();

            // Read the amount of materials in this file.
            uint materialCount = reader.ReadUInt32();

            // Loop through and read each material.
            for (int i = 0; i < materialCount; i++)
            {
                MaterialData material = new()
                {
                    UnknownUInt32_1 = reader.ReadUInt32(),
                    UnknownUInt32_2 = reader.ReadUInt32(),
                    UnknownUInt32_3 = reader.ReadUInt32(),
                    UnknownUInt32_4 = reader.ReadUInt32(),
                    UnknownUInt32_5 = reader.ReadUInt32(),
                    Colour = reader.ReadVector3(),
                    UnknownFloat_1 = reader.ReadSingle(),
                    UnknownFloat_2 = reader.ReadSingle(),
                    UnknownUInt32_6 = reader.ReadUInt32(),
                    UnknownUInt32_7 = reader.ReadUInt32(),
                    UnknownFloat_3 = reader.ReadSingle(),
                    UnknownFloat_4 = reader.ReadSingle(),
                    TextureIndex = reader.ReadInt32(),
                    UnknownFloat_5 = reader.ReadSingle(),
                    UnknownFloat_6 = reader.ReadSingle(),
                    UnknownFloat_7 = reader.ReadSingle(),
                    UnknownFloat_8 = reader.ReadSingle(),
                    UnknownFloat_9 = reader.ReadSingle(),
                    UnknownFloat_10 = reader.ReadSingle()
                };

                // Add this material to our list.
                Materials.Add(material);
            }

            // Return the list of materials read from the file.
            return Materials;
        }
    }
}
