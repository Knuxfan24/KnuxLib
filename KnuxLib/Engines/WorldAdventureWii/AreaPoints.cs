namespace KnuxLib.Engines.WorldAdventureWii
{
    public class AreaPoints : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AreaPoints() { }
        public AreaPoints(string filepath, FormatVersion version = FormatVersion.Wii, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.worldadventurewii.areapoints.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            PlayStation2 = 0,
            Wii = 1
        }

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
            /// A copy of this instance's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();
        }

        // Actual data presented to the end user.
        public List<Area> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.Wii)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a Wii wap file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.Wii)
                reader.IsBigEndian = true;

            // Read the ARP. signature in this file.
            reader.ReadSignature(4, "ARP.");

            // Skip an unknown value of 0x20071102.
            reader.JumpAhead(0x04);

            // Read this file's area count.
            uint areaCount = reader.ReadUInt32();

            // Skip an unknown floating point value of 1.
            reader.JumpAhead(0x04);

            // Check this file's version string against our version.
            switch (version)
            {
                case FormatVersion.PlayStation2: reader.ReadSignature(4, "PS2 "); break;
                case FormatVersion.Wii: reader.ReadSignature(4, "Wii "); break;
            }

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Loop through each area.
            for (int i = 0; i < areaCount; i++)
            {
                // Set up a new area entry.
                Area area = new();

                // Read this area's matrix, transposing the Wii versions.
                switch (version)
                {
                    case FormatVersion.PlayStation2: area.Matrix = reader.ReadMatrix(); break;
                    case FormatVersion.Wii: area.Matrix = Matrix4x4.Transpose(reader.ReadMatrix()); break;
                }

                // Decompose this area's matrix into a human readable format.
                area.Transform.Process(area.Matrix);

                // Save this area entry.
                Data.Add(area);
            }

            // Loop through each area.
            for (int i = 0; i < areaCount; i++)
            {
                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this area's unknown floating point value.
                Data[i].UnknownFloat_1 = reader.ReadSingle();

                // Skip two unknown values of 0.
                reader.JumpAhead(0x08);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.Wii)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If this is a Wii wap file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.Wii)
                writer.IsBigEndian = true;

            // Write this file's signature.
            writer.WriteSignature("ARP.");

            // Write an unknown value of 0x20071102.
            writer.Write(0x20071102);

            // Write the number of areas in this file.
            writer.Write(Data.Count);

            // Write an unknown floating point value of 1.
            writer.Write(1f);

            // Write the correct platform identifier.
            switch (version)
            {
                case FormatVersion.PlayStation2: writer.WriteSignature("PS2 "); break;
                case FormatVersion.Wii: writer.WriteSignature("Wii "); break;
            }

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Loop through and write each area's matrix, transposing it if it's a Wii version matrix.
            for (int i = 0; i < Data.Count; i++)
            {
                switch (version)
                {
                    case FormatVersion.PlayStation2: writer.Write(Data[i].Matrix); break;
                    case FormatVersion.Wii: writer.Write(Matrix4x4.Transpose(Data[i].Matrix)); ; break;
                }
            }

            // Loop through each area again.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write an unknown value of 0.
                writer.WriteNulls(0x4);

                // Write this area's unknown floating point value.
                writer.Write(Data[i].UnknownFloat_1);

                // Write two unknown values of 0.
                writer.WriteNulls(0x8);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
