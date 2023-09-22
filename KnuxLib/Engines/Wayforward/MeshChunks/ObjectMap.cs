namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class ObjectMap
    {
        /// <summary>
        /// An unknown hash.
        /// TODO: What is this for?
        /// </summary>
        public ulong UnknownHash { get; set; }

        /// <summary>
        /// The hash identifier of this object map.
        /// </summary>
        public ulong ObjectHash { get; set; }

        /// <summary>
        /// The hash of the vertex table to use for this object map, either in this file or a .gpu file.
        /// </summary>
        public ulong VertexHash { get; set; }

        /// <summary>
        /// The hash of the face table to use for this object map, either in this file or a .gpu file.
        /// </summary>
        public ulong FaceHash { get; set; }

        /// <summary>
        /// This object map's Axis-Aligned Bounding Box.
        /// </summary>
        public Vector3[] AABB { get; set; } = new Vector3[2];

        /// <summary>
        /// Read the data of this group from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static ObjectMap Read(BinaryReaderEx reader)
        {
            // Initialise the object map.
            ObjectMap objectMap = new();

            // Read this object map's unknown hash.
            objectMap.UnknownHash = reader.ReadUInt64();

            // Read this object map's target object hash.
            objectMap.ObjectHash = reader.ReadUInt64();

            // Read this object map's target vertex hash.
            objectMap.VertexHash = reader.ReadUInt64();

            // Read this object map's target face hash.
            objectMap.FaceHash = reader.ReadUInt64();

            // Read the two Vector3s for this object map's AABB.
            objectMap.AABB[0] = reader.ReadVector3();
            objectMap.AABB[1] = reader.ReadVector3();

            // Return our read object map.
            return objectMap;
        }
    }
}
