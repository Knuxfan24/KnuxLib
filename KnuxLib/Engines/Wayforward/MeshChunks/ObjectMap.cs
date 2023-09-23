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

        /// <summary>
        /// Writes the data of this object map to the writer's current position.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        public void Write(BinaryWriterEx writer)
        {
            // Write the Node Type.
            writer.Write(0x06);

            // Write empty values for the sub node count and offset, as object maps don't have them.
            writer.Write(0);
            writer.Write(0L);

            // Write this object map's unknown hash.
            writer.Write(UnknownHash);

            // Write this object map's target object hash.
            writer.Write(ObjectHash);

            // Write this object map's vertex object hash.
            writer.Write(VertexHash);

            // Write this object map's face object hash.
            writer.Write(FaceHash);

            // Write the two Vector3s for this object map's AABB.
            writer.Write(AABB[0]);
            writer.Write(AABB[1]);
        }
    }
}
