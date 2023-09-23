namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class Unknown2
    {
        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_1 { get; set; }

        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_2 { get; set; }

        /// <summary>
        /// The name of this chunk.
        /// </summary>
        public string Name { get; set; } = "";

        public override string ToString() => $"Unknown 2 Chunk: {Name}";

        /// <summary>
        /// Read the data of this unknown chunk from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static Unknown2 Read(BinaryReaderEx reader)
        {
            // Initialise this unknown chunk.
            Unknown2 unknown2 = new();

            // Read the first unknown integer value in this chunk.
            unknown2.UnknownUInt32_1 = reader.ReadUInt32();

            // Read the second unknown integer value in this chunk.
            unknown2.UnknownUInt32_2 = reader.ReadUInt32();

            // Read an unknown offset.
            // TODO: Read the data at this offset.
            uint UnknownOffset_1 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_1.
            uint UnknownOffset_1_Size = reader.ReadUInt32();

            // Read an unknown offset.
            // TODO: Read the data at this offset.
            uint UnknownOffset_2 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_2.
            uint UnknownOffset_2_Size = reader.ReadUInt32();

            // Read the offset to this group's name.
            uint NameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the name offset.
            uint NameOffset_Size = reader.ReadUInt32();

            // Save our position to jump back to after reading the chunk name.
            long position = reader.BaseStream.Position;

            // Jump to this chunk's name offset.
            reader.JumpTo(NameOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{NameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this chunk's name.
            unknown2.Name = reader.ReadNullTerminatedString();

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return this unknown chunk.
            return unknown2;
        }
    }
}
