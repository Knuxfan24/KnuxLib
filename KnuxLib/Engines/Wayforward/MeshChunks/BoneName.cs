namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class BoneName
    {
        /// <summary>
        /// The hash that identifies this bone name.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// This bone name's IDX values.
        /// TODO: How do these affect things?
        /// </summary>
        public uint[] IDX { get; set; } = new uint[4];

        /// <summary>
        /// The name of this bone.
        /// </summary>
        public string Name { get; set; } = "";

        public override string ToString() => $"Bone Name {Name}: 0x{Hash.ToString("X").PadLeft(0x08, '0')}";

        /// <summary>
        /// Read the data of this bone name from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static BoneName Read(BinaryReaderEx reader)
        {
            // Initialise the bone name.
            BoneName boneName = new();

            // Read this bone name's hash.
            boneName.Hash = reader.ReadUInt64();

            // Read this bone's IDX values.
            boneName.IDX[0] = reader.ReadUInt32();
            boneName.IDX[1] = reader.ReadUInt32();
            boneName.IDX[2] = reader.ReadUInt32();
            boneName.IDX[3] = reader.ReadUInt32();

            // Read an unknown offset.
            // TODO: Read the data at this offset.
            uint UnknownOffset_1 = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at UnknownOffset_1.
            uint UnknownOffset_1_Size = reader.ReadUInt32();

            // Read the offset to this bone's name.
            uint NameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the name offset.
            uint NameOffset_Size = reader.ReadUInt32();

            // Save our position to jump back to after reading the bone name.
            long position = reader.BaseStream.Position;

            // Jump to this bone's name data offset.
            reader.JumpTo(NameOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{NameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this bone's name.
            boneName.Name = reader.ReadNullTerminatedString();

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return this bone name.
            return boneName;
        }
    }
}
