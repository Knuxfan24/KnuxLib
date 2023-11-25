namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out the unknown values.
    public class DensityPointCloud : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DensityPointCloud() { }
        public DensityPointCloud(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.densitypointcloud.json", Data);
        }

        // Classes for this format.
        public class Instance
        {
            /// <summary>
            /// This instance's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This instance's roation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This instance's scale in 3D space.
            /// </summary>
            public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

            /// <summary>
            /// Some sort of colour value.
            /// TODO: How does this interact with the instances themselves?
            /// </summary>
            public VertexColour Colour { get; set; } = new();

            /// <summary>
            /// The type this instance should be.
            /// TODO: Is this pulled from the .densitysetting format?
            /// </summary>
            public uint TypeIndex;

            /// <summary>
            /// An unknown integer value that can be four values, being 0x00000000, 0x01000000, 0x02000000 and 0x03000000
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1;
        }

        // Actual data presented to the end user.
        public List<Instance> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "EIYD";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip two unknown values of 4 and 2.
            reader.JumpAhead(0x0C);

            // Get the 64 bit offset to the Instance Table and the count of the instances in it.
            long instanceTableOffset = reader.ReadInt64();
            ulong instanceCount = reader.ReadUInt64();

            // Jump to the instance table (should already be here but lets play it safe).
            reader.JumpTo(instanceTableOffset, false);

            // Loop through each instance.
            for (ulong instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
            {
                // Set up a new instance.
                Instance instance = new();

                // Read this instance's position.
                instance.Position = Helpers.ReadHedgeLibVector3(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this instance's scale.
                instance.Scale = Helpers.ReadHedgeLibVector3(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this instance's rotation.
                instance.Rotation = Helpers.ReadHedgeLibQuaternion(reader);

                // Skip four unknown values of 0.
                reader.JumpAhead(0x10);

                // Read this instance's unknown colour data.
                instance.Colour.Alpha = reader.ReadByte();
                instance.Colour.Red = reader.ReadByte();
                instance.Colour.Green = reader.ReadByte();
                instance.Colour.Blue = reader.ReadByte();

                // Read this instance's type index.
                instance.TypeIndex = reader.ReadUInt32();

                // Read this instance's unknown integer value.
                instance.UnknownUInt32_1 = reader.ReadUInt32();

                // Skip two unknown values of 0.
                reader.JumpAhead(0x08);

                // Save this instance.
                Data.Add(instance);
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

            // Write this file's signature.
            writer.WriteSignature(Signature);

            // Write the unknown header values.
            writer.Write(0x04);
            writer.Write(0x02L);

            // Set up the BINA Offset Table.
            writer.AddOffset("instanceOffset", 0x08);

            // Write how many instances are in this file.
            writer.Write((ulong)Data.Count);

            // Fill in the offset position.
            writer.FillInOffset("instanceOffset", false, false);

            // Loop through each instance.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this instance's position.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Position);

                // Write this instance's first empty value.
                writer.Write(0x00);

                // Write this instance's scale.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Scale);

                // Write this instance's second empty value.
                writer.Write(0x00);

                // Write this instance's rotation.
                Helpers.WriteHedgeLibQuaternion(writer, Data[dataIndex].Rotation);

                // Write this instance's sequence of empty values.
                writer.WriteNulls(0x10);

                // Write this instance's unknown colour data.
                writer.Write((byte)Data[dataIndex].Colour.Alpha);
                writer.Write(Data[dataIndex].Colour.Red);
                writer.Write(Data[dataIndex].Colour.Green);
                writer.Write(Data[dataIndex].Colour.Blue);

                // Write this instance's type index.
                writer.Write(Data[dataIndex].TypeIndex);

                // Write this instance's unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_1);

                // Write this instance's second sequence of empty values.
                writer.WriteNulls(0x08);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
