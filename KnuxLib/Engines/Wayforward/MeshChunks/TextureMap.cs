namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class TextureMap
    {
        /// <summary>
        /// The hash of the object map to apply this texture map tp.
        /// </summary>
        public ulong ObjectHash { get; set; }

        /// <summary>
        /// The hash of the texture to use for this texture map, either in this file or a .gpu file.
        /// </summary>
        public ulong TextureHash { get; set; }

        /// <summary>
        /// Three unknown ulong values.
        /// TODO: What are these? They seem to maybe be hashes to something?
        /// </summary>
        public ulong[] UnknownULong_Array { get; set; } = new ulong[3];

        /// <summary>
        /// Read the data of this texture map from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static TextureMap Read(BinaryReaderEx reader)
        {
            // Initialise the texture map.
            TextureMap textureMap = new();

            // Read the object hash for this texture map.
            textureMap.ObjectHash = reader.ReadUInt64();

            // Read the texture hash for this texture map.
            textureMap.TextureHash = reader.ReadUInt64();

            // Read the three unknown ulong values for this texture map.
            textureMap.UnknownULong_Array[0] = reader.ReadUInt64();
            textureMap.UnknownULong_Array[1] = reader.ReadUInt64();
            textureMap.UnknownULong_Array[2] = reader.ReadUInt64();

            // Return our read texture map.
            return textureMap;
        }

        /// <summary>
        /// Writes the data of this texture map to the writer's current position.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        public void Write(BinaryWriterEx writer)
        {
            // Write the Node Type.
            writer.Write(0x04);

            // Write empty values for the sub node count and offset, as texture maps don't have them.
            writer.Write(0);
            writer.Write(0L);

            // Write this texture map's object hash.
            writer.Write(ObjectHash);

            // Write this texture map's texture hash.
            writer.Write(TextureHash);

            // Write the three unknown ulong values for this texture map.
            writer.Write(UnknownULong_Array[0]);
            writer.Write(UnknownULong_Array[1]);
            writer.Write(UnknownULong_Array[2]);
        }
    }
}
