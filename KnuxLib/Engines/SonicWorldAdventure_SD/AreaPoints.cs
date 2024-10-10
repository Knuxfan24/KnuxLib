namespace KnuxLib.Engines.SonicWorldAdventure_SD
{
    public class AreaPoints : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AreaPoints() { }
        public AreaPoints(string filepath, bool export = false, bool bigEndian = true)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".sonicworldadventure_sd.areapoints.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Area[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.wap", bigEndian);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class Area
        {
            /// <summary>
            /// The matrix that makes up this area's transform data.
            /// </summary>
            public Matrix4x4 Matrix { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// A copy of this area's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();

            /// <summary>
            /// Initialises this area with default data.
            /// </summary>
            public Area() { }

            /// <summary>
            /// Initialises this area with the provided data.
            /// </summary>
            public Area(Matrix4x4 matrix, float unknownFloat_1)
            {
                Matrix = matrix;
                UnknownFloat_1 = unknownFloat_1;
                Transform = new(matrix);
            }

            /// <summary>
            /// Initialises this instance by reading its data from a BinaryReader.
            /// </summary>
            public Area(ExtendedBinaryReader reader) => ReadMatrix(reader);

            /// <summary>
            /// Reads this area's matrix.
            /// </summary>
            public void ReadMatrix(ExtendedBinaryReader reader)
            {
                Matrix = reader.ReadMatrix();
                if (reader.IsBigEndian) Matrix = Matrix4x4.Transpose(Matrix);
                Transform = new(Matrix);
            }

            /// <summary>
            /// Reads this area's unknown float.
            /// </summary>
            public void ReadUnknown(ExtendedBinaryReader reader)
            {
                reader.CheckValue(0x00);
                UnknownFloat_1 = reader.ReadSingle();
                reader.CheckValue(0x00, 0x02);
            }

            /// <summary>
            /// Writes this area's matrix.
            /// </summary>
            public void WriteMatrix(ExtendedBinaryWriter writer)
            {
                Matrix4x4 matrix = Matrix;
                if (writer.IsBigEndian) matrix = Matrix4x4.Transpose(matrix);
                writer.Write(matrix);
            }

            /// <summary>
            /// Writes this area's unknown float.
            /// </summary>
            public void WriteUnknown(ExtendedBinaryWriter writer)
            {
                writer.Write(0x00);
                writer.Write(UnknownFloat_1);
                writer.Write(0x00L);
            }
        }

        // Actual data presented to the end user.
        public Area[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "ARP.");

            // Reads an unknown value that is always consistent.
            uint unknownValue = reader.ReadUInt32();

            // If we read 0x02110720 instead of 0x20071102, then flip the reader to big endian.
            if (unknownValue == 0x02110720)
                reader.IsBigEndian = true;

            // If we didn't read either expected value, then throw an exception.
            else if (unknownValue != 0x20071102)
                throw new Exception($"Expected value of 0x20071102, got 0x{unknownValue.ToString("X").PadLeft(8, '0')}.\r\nFile: {(reader.BaseStream as FileStream).Name}\r\nPosition: 0x{(reader.BaseStream.Position - 0x04).ToString("X").PadLeft(16, '0')}");

            // Read this file's area count.
            Data = new Area[reader.ReadUInt32()];

            // Skip an unknown floating point value of 1.
            reader.CheckValue(1f);

            // Read the right platform signature depending on the unknown value.
            if (unknownValue == 0x02110720)
                reader.ReadSignature(0x04, "Wii ");
            else
                reader.ReadSignature(0x04, "PS2 ");

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Loop through and read each area's matrix.
            for (int areaIndex = 0; areaIndex < Data.Length; areaIndex++)
                Data[areaIndex] = new(reader);

            // Loop through and read each area's unknown floating point value.
            for (int areaIndex = 0; areaIndex < Data.Length; areaIndex++)
                Data[areaIndex].ReadUnknown(reader);

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath, bool bigEndian = true)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath), bigEndian);

            // Write this file's signature.
            writer.Write("ARP.");

            // Write an unknown value of 0x20071102.
            writer.Write(0x20071102);

            // Write the count of areas in this file.
            writer.Write(Data.Length);

            // Write an unknown value of 1.
            writer.Write(1f);

            // Write this file's platform signature depending on the reader endianness.
            if (writer.IsBigEndian)
                writer.Write("Wii ");
            else
                writer.Write("PS2 ");

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Loop through and write each area's matrix.
            for (int areaIndex = 0; areaIndex < Data.Length; areaIndex++)
                Data[areaIndex].WriteMatrix(writer);

            // Loop through and write each area's unknown floating point value.
            for (int areaIndex = 0; areaIndex < Data.Length; areaIndex++)
                Data[areaIndex].WriteUnknown(writer);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
