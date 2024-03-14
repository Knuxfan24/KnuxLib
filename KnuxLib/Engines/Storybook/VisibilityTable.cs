namespace KnuxLib.Engines.Storybook
{
    public class VisibilityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public VisibilityTable() { }
        public VisibilityTable(string filepath, FormatVersion version = FormatVersion.SecretRings, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.visibilitytable.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            SecretRings = 0,
            BlackKnight = 1
        }

        public class VisibilityBlock
        {
            /// <summary>
            /// This block's index.
            /// TODO: Does this do anything? Setting them all to 24 didn't seem to change anything?
            /// </summary>
            public uint Index { get; set; }

            /// <summary>
            /// The position of this visibility block.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This visibility block's rotation, only used in Black Knight's STG221_BLK file.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown sequence of floating point values that affect the block's area in some way?
            /// </summary>
            public float[] UnknownFloatArray_1 { get; set; } = new float[5];

            /// <summary>
            /// An unknown integer value that only exists in the Black Knight format version.
            /// TODO: What is this?
            /// </summary>
            public uint? UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The sectors that should be visible when within this block.
            /// </summary>
            public List<byte> Sectors { get; set; } = new(16);
        }

        // Actual data presented to the end user.
        public VisibilityBlock[] Data = Array.Empty<VisibilityBlock>();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read the LDBK signature in this file.
            reader.ReadSignature(4, "LDBK");

            // Initalise the visibility block array.
            Data = new VisibilityBlock[reader.ReadUInt32()];

            // Skip an unknown value that is either 0xCCCCCCCCCCCCCCCC or 0x0000000000000000 depending on the format version.
            reader.JumpAhead(0x08);

            // Loop through each visibility block in this file.
            for (int blockIndex = 0; blockIndex < Data.Length; blockIndex++)
            {
                // Set up a new visibility block entry.
                VisibilityBlock block = new();

                // Read this visibility block's entry.
                block.Index = reader.ReadUInt32();

                // Read this visibility block's position.
                block.Position = reader.ReadVector3();

                // Read and convert the three integer values for this visibility block's rotation from BAMS to Euler.
                block.Rotation = new(Helpers.CalculateBAMsValue(reader.ReadInt32()), Helpers.CalculateBAMsValue(reader.ReadInt32()), Helpers.CalculateBAMsValue(reader.ReadInt32()));
                
                // Read the five unknown floating point values for this visibility block.
                for (int floatIndex = 0; floatIndex < 5; floatIndex++)
                    block.UnknownFloatArray_1[floatIndex] = reader.ReadSingle();

                // If this is a Black Knight visibility table, then read this visibility block's unknown integer value.
                if (version == FormatVersion.BlackKnight)
                    block.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this visibility block's sectors.
                for (int sectorIndex = 0; sectorIndex < 16; sectorIndex++)
                    block.Sectors.Add(reader.ReadByte());
                
                // Save this visibility block.
                Data[blockIndex] = block;
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), true);

            // Write the LDBK signature in this file.
            writer.Write("LDBK");

            // Write the count of visibility blocks in this file.
            writer.Write(Data.Length);

            // Write an unknown value depending on the format version.
            if (version == FormatVersion.SecretRings) writer.Write(0xCCCCCCCCCCCCCCCC);
            if (version == FormatVersion.BlackKnight) writer.WriteNulls(0x08);

            // Loop through each visibility block.
            for (int blockIndex = 0; blockIndex < Data.Length; blockIndex++)
            {
                // Write this visibility block's index.
                writer.Write(Data[blockIndex].Index);

                // Write this visibility block's position.
                writer.Write(Data[blockIndex].Position);

                // Write this visibility block's rotation, converted from Euler Angles to the Binary Angle Measurement System.
                writer.Write(Helpers.CalculateBAMsValue(Data[blockIndex].Rotation.X));
                writer.Write(Helpers.CalculateBAMsValue(Data[blockIndex].Rotation.Y));
                writer.Write(Helpers.CalculateBAMsValue(Data[blockIndex].Rotation.Z));

                // Loop through and write each of this visibility block's unknown floating point values.
                for (int floatIndex = 0; floatIndex < 5; floatIndex++)
                    writer.Write(Data[blockIndex].UnknownFloatArray_1[floatIndex]);

                // Check if this is a Black Knight file.
                if (version == FormatVersion.BlackKnight)
                {
                    // If so and it has an unknown integer value, then write it. If not, then write a 1.
                    if (Data[blockIndex].UnknownUInt32_1 != null)
                        writer.Write((uint)Data[blockIndex].UnknownUInt32_1);
                    else
                        writer.Write(0x01);
                }

                // Loop through and write each of this visibility block's sectors.
                for (int sectorIndex = 0; sectorIndex < 16; sectorIndex++)
                    writer.Write(Data[blockIndex].Sectors[sectorIndex]);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
