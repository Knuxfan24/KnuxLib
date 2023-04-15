namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Are the Unleashed and Generations terrain-instanceinfo files the same? As the SCHG page (https://info.sonicretro.org/SCHG:Sonic_Generations#.instanceinfo) marks the root node type as "likely 5"?
    public class InstanceInfo : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public InstanceInfo() { }
        public InstanceInfo(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.instanceinfo.json", Data);
        }

        public class FormatData
        {
            /// <summary>
            /// The name of the model used by this instance.
            /// </summary>
            public string ModelName { get; set; } = "";

            /// <summary>
            /// The name of this instance.
            /// </summary>
            public string InstanceName { get; set; } = "";

            /// <summary>
            /// The matrix that makes up this instance's transform data.
            /// </summary>
            public Matrix4x4 Matrix { get; set; }

            /// <summary>
            /// A copy of this instance's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();

            public override string ToString() => InstanceName;
        }

        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Skip the root node type, as it's always 0.
            reader.JumpAhead(0x04);

            // Read the root node size.
            uint rootNodeSize = reader.ReadUInt32();

            // Read the offset to the root node.
            uint rootNodeOffset = reader.ReadUInt32();

            // Read the offset to the footer.
            uint footerOffset = reader.ReadUInt32();

            // Skip the file end offset, as it's always 0.
            reader.JumpAhead(0x04);

            // Set the reader's offset to the value in rootNodeOffset.
            reader.Offset = rootNodeOffset;

            // Jump to rootNodeOffset.
            reader.JumpTo(rootNodeOffset);

            // Read this instance's model name.
            Data.ModelName = Helpers.ReadNullTerminatedStringTableEntry(reader, true);

            // Read the offset to this instance's matrix.
            uint matrixOffset = reader.ReadUInt32();

            // Read this instance's name.
            Data.InstanceName = Helpers.ReadNullTerminatedStringTableEntry(reader, true);

            // Jump to this instance's matrix offset.
            reader.JumpTo(matrixOffset, true);

            // Read this instance's transposed matrix.
            Data.Matrix = Matrix4x4.Transpose(reader.ReadMatrix());

            // Decompose this instance's matrix into a human readable format.
            Data.Transform.Process(Data.Matrix);

            // We skip the file's footer, as it's always the same sixteen bytes.

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), true) { Offset = 0x18 };

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write the root node type constant.
            writer.Write(0x00);

            // Write a placeholder for the root node's size.
            writer.Write("SIZE");

            // Add an offset for the root node.
            writer.AddOffset("RootNodeOffset");

            // Add an offset for the footer.
            writer.AddOffset("FooterOffset");

            // Write the file end offset constant.
            writer.Write(0x00);

            // Fill in the root node offset.
            writer.FillOffset("RootNodeOffset");

            // Add an offset for the model name.
            writer.AddOffset("ModelNameOffset");

            // Add an offset for the matrix.
            writer.AddOffset("MatrixOffset");

            // Add an offset for the instance name.
            writer.AddOffset("InstanceNameOffset");

            // Fill in the model name's offset.
            writer.FillOffset("ModelNameOffset", true);

            // Write the model name.
            writer.WriteNullTerminatedString(Data.ModelName);

            // Fill in the matrix's offset.
            writer.FillOffset("MatrixOffset", true);

            // Write the transposed matrix.
            writer.Write(Matrix4x4.Transpose(Data.Matrix));

            // Fill in the instance name's offset.
            writer.FillOffset("InstanceNameOffset", true);

            // Write the instance name.
            writer.WriteNullTerminatedString(Data.ModelName);

            // Calculate the root node size value.
            uint rootNodeSize = (uint)(writer.BaseStream.Length - 0x18);

            // Fill in the offset for the footer.
            writer.FillOffset("FooterOffset");

            // Write the constant values for the footer.
            writer.Write(0x03);
            writer.Write(0x00);
            writer.Write(0x04);
            writer.Write(0x08);

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Write the root node size.
            writer.BaseStream.Position = 0x08;
            writer.Write(rootNodeSize);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
