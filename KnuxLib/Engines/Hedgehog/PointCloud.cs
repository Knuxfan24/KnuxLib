namespace KnuxLib.Engines.Hedgehog
{
    public class PointCloud : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PointCloud() { }
        public PointCloud(string filepath, bool export = false, string saveExtension = ".pcmodel")
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.pointcloud.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Instance[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}{saveExtension}");
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

            /// <summary>
            /// Displays this instance's name in the debugger.
            /// </summary>
            public override string ToString() => InstanceName;

            /// <summary>
            /// Initialises this instance with default data.
            /// </summary>
            public Instance() { }

            /// <summary>
            /// Initialises this instance with the provided data.
            /// </summary>
            public Instance(string instanceName, string assetName, Vector3 position, Vector3 rotation, uint unknownUInt32_1, Vector3 scale)
            {
                InstanceName = instanceName;
                AssetName = assetName;
                Position = position;
                Rotation = rotation;
                UnknownUInt32_1 = unknownUInt32_1;
                Scale = scale;
            }

            /// <summary>
            /// Initialises this instance by reading its data from a BINAReader.
            /// </summary>
            public Instance(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this instance.
            /// </summary>
            public void Read(BINAReader reader)
            {
                InstanceName = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                AssetName = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                Position = reader.ReadVector3();
                Rotation = reader.ReadVector3();
                UnknownUInt32_1 = reader.ReadUInt32();
                Scale = reader.ReadVector3();
                reader.CheckValue(0x00);
                reader.FixPadding(0x08);
            }
        
            /// <summary>
            /// Writes the data for this instance.
            /// </summary>
            public void Write(BINAWriter writer, int index)
            {
                writer.AddString($"Instane{index}InstanceName", AssetName, 0x08);
                writer.AddString($"Instane{index}AssetName", AssetName, 0x08);
                writer.Write(Position);
                writer.Write(Rotation);
                writer.Write(UnknownUInt32_1);
                writer.Write(Scale);
                writer.WriteNulls(0x04);
            }
        }

        // Actual data presented to the end user.
        public Instance[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "CPIC");

            // Read an unknown value that is usually 2, but can be 1.
            int UnknownInt32_1 = reader.ReadInt32();

            // Read the offset to this file's instance table.
            long instanceTableOffset = reader.ReadInt64();

            // Initialise the data array.
            Data = new Instance[reader.ReadInt64()];

            // Jump to the instance table (should already be here but lets play it safe).
            reader.JumpTo(instanceTableOffset, false);

            // Loop through and read each instance.
            for (int instanceIndex = 0; instanceIndex < Data.Length; instanceIndex++)
                Data[instanceIndex] = new(reader);

            // Close our BINAReader.
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
            writer.Write("CPIC");

            // Write an unknown value that is usually 2.
            writer.Write(0x02);

            // Add an offset for the instance table.
            writer.AddOffset("instanceTableOffset", 0x08);

            // Write how many instances are in this file.
            writer.Write((ulong)Data.Length);

            // Fill in the offset for the instance table.
            writer.FillInOffset("instanceTableOffset");

            // Loop through each instance in this file.
            for (int instanceIndex = 0; instanceIndex < Data.Length; instanceIndex++)
            {
                // Write this instance.
                Data[instanceIndex].Write(writer, instanceIndex);

                // All instances but the last one appear to be aligned, so if this isn't the last instance, align it.
                if (instanceIndex != Data.Length - 1)
                    writer.FixPadding(0x08);
            }

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
