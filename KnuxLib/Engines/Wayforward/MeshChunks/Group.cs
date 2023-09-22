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

        public override string ToString() => Name;

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
            uint UnknownCount_1 = reader.ReadUInt32();

            // Read the count of the data in UnknownOffset_2.
            uint UnknownCount_2 = reader.ReadUInt32();

            // Read this group's unknown hash.
            group.UnknownHash = reader.ReadUInt64();

            // Read and transpose this group's matrix.
            group.Matrix = Matrix4x4.Transpose(reader.ReadMatrix());

            // Decompose this group's matrix into a human readable format.
            group.Transform.Process(group.Matrix);

            // Read an unknown offset.
            // TODO: Understand the data at this offset.
            uint UnknownOffset_1 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_1.
            uint UnknownOffset_1_Size = reader.ReadUInt32();

            // Read an unknown offset.
            // TODO: Read the data at this offset. Is a set of strings, but the count doesn't match up?
            uint UnknownOffset_2 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_2.
            uint UnknownOffset_2_Size = reader.ReadUInt32();

            // Read the offset to this group's name.
            uint NameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the name offset.
            uint NameOffset_Size = reader.ReadUInt32();

            // Save our position to jump back to after reading the group data.
            long position = reader.BaseStream.Position;

            // Jump to this group's name offset.
            reader.JumpTo(NameOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{NameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this group's name.
            group.Name = reader.ReadNullTerminatedString();

            // If UnknownOffset_1 has a value, then read its data.
            if (UnknownOffset_1 != 0)
            {
                // Initialise the array of unknown data.
                group.UnknownUInt32_Array = new uint[UnknownCount_1];

                // Jump to the unknown offset.
                reader.JumpTo(UnknownOffset_1);

                // Read and check this data's magic value.
                dataMagic = reader.ReadUInt32();
                if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{NameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

                // Read this data's size.
                dataSize = reader.ReadUInt32();

                // Loop through and read each value into the array.
                for (int i = 0; i < UnknownCount_1; i++)
                    group.UnknownUInt32_Array[i] = reader.ReadUInt32();
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return this group.
            return group;
        }
    }
}
