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
        /// Read the data of this face table from the reader's current position.
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
    }
}
