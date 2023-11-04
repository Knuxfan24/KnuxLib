namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class Group
    {
        /// <summary>
        /// The hash that identifies this group.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// An unknown hash.
        /// TODO: What is this for?
        /// </summary>
        public ulong UnknownHash { get; set; }

        /// <summary>
        /// A matrix with unknown purpose.
        /// TODO: What does this do and is it even a matrix?
        /// </summary>
        public Matrix4x4 Matrix { get; set; }

        /// <summary>
        /// A copy of this group's matrix decomposed into a human readable format.
        /// </summary>
        public DecomposedMatrix Transform { get; set; } = new();

        /// <summary>
        /// A group of unknown integer values.
        /// TODO: What are these?
        /// </summary>
        public uint[]? UnknownUInt32_Array { get; set; }

        /// <summary>
        /// The name of this group.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The sub nodes stored within this group.
        /// </summary>
        public List<object> SubNodes { get; set; } = new();

        public override string ToString() => $"Group {Name}: 0x{Hash.ToString("X").PadLeft(0x08, '0')}";

        // A list of offsets, used for writing.
        private readonly List<long> Offsets = new();

        /// <summary>
        /// Read the data of this group from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static Group Read(BinaryReaderEx reader)
        {
            // Initialise the group.
            Group group = new();

            // Read this group's hash.
            group.Hash = reader.ReadUInt64();

            // Read the count of the data in UnknownOffset_1.
            uint unknownCount_1 = reader.ReadUInt32();

            // Read the count of the data in UnknownOffset_2.
            uint unknownCount_2 = reader.ReadUInt32();

            // Read this group's unknown hash.
            group.UnknownHash = reader.ReadUInt64();

            // Read and transpose this group's matrix.
            group.Matrix = Matrix4x4.Transpose(reader.ReadMatrix());

            // Decompose this group's matrix into a human readable format.
            group.Transform.Process(group.Matrix);

            // Read an unknown offset.
            // TODO: Understand the data at this offset.
            uint unknownOffset_1 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_1.
            uint unknownOffset_1_Size = reader.ReadUInt32();

            // Read an unknown offset.
            // TODO: Read the data at this offset. Is a set of strings, but the count doesn't match up?
            uint unknownOffset_2 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_2.
            uint unknownOffset_2_Size = reader.ReadUInt32();

            // Read the offset to this group's name.
            uint nameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the name offset.
            uint nameOffset_Size = reader.ReadUInt32();

            // Save our position to jump back to after reading the group data.
            long position = reader.BaseStream.Position;

            // Jump to this group's name offset.
            reader.JumpTo(nameOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{nameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this group's name.
            group.Name = reader.ReadNullTerminatedString();

            // If UnknownOffset_1 has a value, then read its data.
            if (unknownOffset_1 != 0)
            {
                // Initialise the array of unknown data.
                group.UnknownUInt32_Array = new uint[unknownCount_1];

                // Jump to the unknown offset.
                reader.JumpTo(unknownOffset_1);

                // Read and check this data's magic value.
                dataMagic = reader.ReadUInt32();
                if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{nameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

                // Read this data's size.
                dataSize = reader.ReadUInt32();

                // Loop through and read each value into the array.
                for (int unknownIndex = 0; unknownIndex < unknownCount_1; unknownIndex++)
                    group.UnknownUInt32_Array[unknownIndex] = reader.ReadUInt32();
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return this group.
            return group;
        }

        /// <summary>
        /// Writes the data of this group to the writer's current position.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void Write(BinaryWriterEx writer, int nodeIndex)
        {
            // Write the Node Type.
            writer.Write(0x05);

            // Write the amount of sub nodes this group has.
            writer.Write(SubNodes.Count);

            // Add an offset for this group's sub nodes.
            writer.AddOffset($"Group{nodeIndex}SubNodes", 0x08);

            // Write this group's hash.
            writer.Write(Hash);

            // Write a placeholder value for UnknownOffset_1.
            // TODO: Add the count of the unknown data 1 once we figure it out.
            writer.Write(0);

            // Write a placeholder value for UnknownOffset_2.
            // TODO: Add the count of the unknown data 2 once we figure it out.
            writer.Write(0);

            // Write this group's unknown hash.
            writer.Write(UnknownHash);

            // Write this group's transposed matrix.
            writer.Write(Matrix4x4.Transpose(Matrix));

            // Add an offset for this group's first unknown chunk.
            writer.AddOffset($"Group{nodeIndex}Unknown1");

            // Write a placeholder size for UnknownOffset_1.
            // TODO: Fill this in once we figure that data out.
            writer.Write(0);

            // Add an offset for this group's second unknown chunk.
            writer.AddOffset($"Group{nodeIndex}Unknown2");

            // Write a placeholder size for UnknownOffset_2.
            // TODO: Fill this in once we figure that data out.
            writer.Write(0);

            // Add an offset for this group's name.
            writer.AddOffset($"Group{nodeIndex}Name");

            // Write the length of this group's name, including the null terminator, data magic value and size.
            writer.Write(Name.Length + 0x09);
        }

        /// <summary>
        /// Write this group's sub nodes.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void WriteSubNodes(BinaryWriterEx writer, int nodeIndex)
        {
            // Loop through each sub node.
            for (int subNodeIndex = 0; subNodeIndex < SubNodes.Count; subNodeIndex++)
            {
                // Add an offset of this node's position so we can fill in the sub node offsets.
                Offsets.Add(writer.BaseStream.Position);

                // Write the node based on its type.
                switch (SubNodes[subNodeIndex].GetType().Name)
                {
                    case "TextureMap": (SubNodes[subNodeIndex] as TextureMap).Write(writer); break;
                    case "ObjectMap":  (SubNodes[subNodeIndex] as ObjectMap).Write(writer);  break;

                    default: Console.WriteLine($"Writing of node type '{SubNodes[subNodeIndex].GetType().Name}' not yet implemented."); break;
                }
            }

            // Fill in the offset for the sub node table.
            writer.FillOffset($"Group{nodeIndex}SubNodes");

            // Write all the offsets for the sub nodes.
            foreach (long offset in Offsets)
                writer.Write(offset);
        }

        /// <summary>
        /// Write this group's name.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void WriteName(BinaryWriterEx writer, int nodeIndex)
        {
            // Fill in the offset for this group's name.
            writer.FillOffset($"Group{nodeIndex}Name");

            // Write the data magic value.
            writer.Write(0xFFFFFF);

            // Write the length of the name, including the null terminator.
            writer.Write(Name.Length + 0x01);

            // Write this group's name.
            writer.WriteNullTerminatedString(Name);

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);
        }
    }
}
