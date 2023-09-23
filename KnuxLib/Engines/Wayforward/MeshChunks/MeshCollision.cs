namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class MeshCollision
    {
        /// <summary>
        /// The hash that identifies this collision.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// The name of this collision.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Two unknown ulong values.
        /// TODO: What are these?
        /// </summary>
        public ulong[] UnknownULong_Array { get; set; } = new ulong[2];

        /// <summary>
        /// The sub nodes stored within this collision.
        /// </summary>
        public List<object> SubNodes { get; set; } = new();

        public override string ToString() => $"Mesh Collision {Name}: 0x{Hash.ToString("X").PadLeft(0x08, '0')}";

        /// <summary>
        /// Read the data of this collision from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static MeshCollision Read(BinaryReaderEx reader)
        {
            // Initialise the collision.
            MeshCollision collision = new();

            // Read this collision's hash.
            collision.Hash = reader.ReadUInt64();

            // Read the offset to this collision's name.
            uint NameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the name offset.
            uint NameOffset_Size = reader.ReadUInt32();

            // Read the two unknown ulong values for this collision.
            collision.UnknownULong_Array[0] = reader.ReadUInt64();
            collision.UnknownULong_Array[1] = reader.ReadUInt64();

            // Save our position to jump back to after reading the collision.
            long position = reader.BaseStream.Position;

            // Jump to this collision's name offset.
            reader.JumpTo(NameOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{NameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this collision's name.
            collision.Name = reader.ReadNullTerminatedString();

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return this collision.
            return collision;
        }
    }
}
