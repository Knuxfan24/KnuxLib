namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class ObjectData
    {
        /// <summary>
        /// The hash that identifies this object data.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// The type of this object data.
        /// </summary>
        public string Type { get; set; } = "";

        public override string ToString() => $"Object Data {Type}: 0x{Hash.ToString("X").PadLeft(0x08, '0')}";

        /// <summary>
        /// Read the data of this object data from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static ObjectData Read(BinaryReaderEx reader)
        {
            // Initialise the object data.
            ObjectData objectData = new();

            // Read this object data's hash.
            objectData.Hash = reader.ReadUInt64();

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

            // Read the offset to this object data's type string.
            uint TypeOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the object data's type string.
            uint TypeOffset_Size = reader.ReadUInt32();

            // Save our position to jump back to after reading the object data.
            long position = reader.BaseStream.Position;

            // Jump to this object table's type offset.
            reader.JumpTo(TypeOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{TypeOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Read this object data's type.
            objectData.Type = reader.ReadNullTerminatedString();

            // TODO: Figure out what we need to do based on the type?
            switch (objectData.Type)
            {
                default: Console.WriteLine($"Object Data type '{objectData.Type}' not yet handled."); break;
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return our read object data.
            return objectData;
        }
    }
}
