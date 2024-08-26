namespace KnuxLib.Engines.Hedgehog
{
    public class DensityPointCloud : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DensityPointCloud() { }
        public DensityPointCloud(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.densitypointcloud.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Instance[]>(filepath);

                //If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.densitypointcloud");
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
        public class Instance
        {
            /// <summary>
            /// This instance's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This instance's scale in 3D space.
            /// </summary>
            public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

            /// <summary>
            /// This instance's roation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

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

            /// <summary>
            /// Initialises this instance with default data.
            /// </summary>
            public Instance() { }

            /// <summary>
            /// Initialises this instance with the provided data.
            /// </summary>
            public Instance(Vector3 position, Vector3 scale, Quaternion rotation, VertexColour colour, uint typeIndex, uint unknownUInt32_1)
            {
                Position = position;
                Scale = scale;
                Rotation = rotation;
                Colour = colour;
                TypeIndex = typeIndex;
                UnknownUInt32_1 = unknownUInt32_1;
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
                Position = reader.ReadVector3();
                reader.CheckValue(0x00);
                Scale = reader.ReadVector3();
                reader.CheckValue(0x00);
                Rotation = reader.ReadQuaternion();
                reader.CheckValue(0x00, 0x04);
                Colour.Read(reader, true, true);
                TypeIndex = reader.ReadUInt32();
                UnknownUInt32_1 = reader.ReadUInt32();
                reader.CheckValue(0x00, 0x02);
            }

            /// <summary>
            /// Writes the data for this instance.
            /// </summary>
            public void Write(BINAWriter writer)
            {
                writer.Write(Position);
                writer.WriteNulls(0x04);
                writer.Write(Scale);
                writer.WriteNulls(0x04);
                writer.Write(Rotation);
                writer.WriteNulls(0x10);
                Colour.Write(writer, true);
                writer.Write(TypeIndex);
                writer.Write(UnknownUInt32_1);
                writer.WriteNulls(0x08);
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
            reader.ReadSignature(0x04, "EIYD");

            // Skip three unknown values of 0x04, 0x02 and 0x00.
            reader.CheckValue(0x04);
            reader.CheckValue(0x02);
            reader.CheckValue(0x00);

            // Read the offset to this file's instance table.
            ulong instanceTableOffset = reader.ReadUInt64();

            // Initialise the data array.
            Data = new Instance[reader.ReadUInt64()];

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
            writer.Write("EIYD");

            // Write the unknown header values.
            writer.Write(0x04);
            writer.Write(0x02);
            writer.Write(0x00);

            // Add an offset for the instance table.
            writer.AddOffset("instanceTableOffset", 0x08);

            // Write how many instances are in this file.
            writer.Write((ulong)Data.Length);

            // Fill in the offset for the instance table.
            writer.FillInOffset("instanceTableOffset");

            // Loop through and write each instance in this file.
            for (int instanceIndex = 0; instanceIndex < Data.Length; instanceIndex++)
                Data[instanceIndex].Write(writer);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
