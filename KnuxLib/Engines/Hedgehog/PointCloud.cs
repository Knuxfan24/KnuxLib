namespace KnuxLib.Engines.Hedgehog
{
    public class PointCloud : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PointCloud() { }
        public PointCloud(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.pointcloud.json", Data);
        }

        // Classes for this format.
        public class Instance
        {
            /// <summary>
            /// The name of this instance.
            /// </summary>
            public string InstanceName { get; set; } = "";

            /// <summary>
            /// The name of the asset used by this instance.
            /// </summary>
            public string AssetName { get; set; } = "";

            /// <summary>
            /// This instance's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This instance's roation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 = 1;

            /// <summary>
            /// This instance's scale in 3D space.
            /// </summary>
            public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

            public override string ToString() => InstanceName;
        }

        // Actual data presented to the end user.
        public List<Instance> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "CPIC";

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

            // Check this file version. Usually 2, but can be 1?
            uint version = reader.ReadUInt32();

            // Get the 64 bit offset to the Instance Table and the count of the instances in it.
            long instanceTableOffset = reader.ReadInt64();
            ulong instanceCount = reader.ReadUInt64();

            // Jump to the instance table (should already be here but lets play it safe).
            reader.JumpTo(instanceTableOffset, false);

            // Loop through each instance.
            for (ulong instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
            {
                // Set up a new instance.
                Instance instance = new()
                {
                    // Read this instance's name.
                    InstanceName = Helpers.ReadNullTerminatedStringTableEntry(reader),

                    // Read the name of the asset used by this instance.
                    AssetName = Helpers.ReadNullTerminatedStringTableEntry(reader),

                    // Read this instance's position.
                    Position = Helpers.ReadHedgeLibVector3(reader),

                    // Read this instance's rotation.
                    Rotation = Helpers.ReadHedgeLibVector3(reader),

                    // Read this instance's unknown integer value.
                    UnknownUInt32_1 = reader.ReadUInt32(),

                    // Read this instance's scale.
                    Scale = Helpers.ReadHedgeLibVector3(reader),
                };

                // Skip four bytes that are always 0.
                reader.JumpAhead(0x04);

                // Realign for the next instance.
                reader.FixPadding(0x08);

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

            // Write the file version.
            writer.Write(0x02);

            // Set up the BINA Offset Table.
            writer.AddOffset("instanceOffset", 0x08);

            // Write how many instances are in this file.
            writer.Write((ulong)Data.Count);

            // Fill in the offset position.
            writer.FillInOffset("instanceOffset", false, false);

            // Loop through each instance.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add the string for this instance's name.
                writer.AddString($"instance{dataIndex}name", Data[dataIndex].InstanceName, 0x08);

                // Add the string for this instance's asset.
                writer.AddString($"instance{dataIndex}asset", Data[dataIndex].AssetName, 0x08);

                // Write this instance's position.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Position);

                // Write this instance's rotation.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Rotation);

                // Write this instance's unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_1);

                // Write this instance's scale.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Scale);

                // Write this instance's empty value.
                writer.Write(0x00);

                // All Data but the last one appear to be aligned, so if this isn't the last instance, align it.
                if (dataIndex != Data.Count - 1)
                    writer.FixPadding(0x08);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
