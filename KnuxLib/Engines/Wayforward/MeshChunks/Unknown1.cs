namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class Unknown1
    {
        /// <summary>
        /// The hash that identifies this unknown chunk.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// Two unknown Vector3 values.
        /// TODO: What are these for?
        /// </summary>
        public Vector3[] UnknownVector3_Array { get; set; } = new Vector3[2];

        /// <summary>
        /// The sub nodes stored within this unknown chunk.
        /// </summary>
        public List<object> SubNodes { get; set; } = new();

        /// <summary>
        /// Read the data of this unknown chunk from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static Unknown1 Read(BinaryReaderEx reader)
        {
            // Initialise this unknown chunk.
            Unknown1 unknown1 = new();

            // Read this unknown chunk's hash.
            unknown1.Hash = reader.ReadUInt64();

            // Read the two unknown Vector3 values.
            unknown1.UnknownVector3_Array[0] = reader.ReadVector3();
            unknown1.UnknownVector3_Array[1] = reader.ReadVector3();

            // Return this unknown chunk.
            return unknown1;
        }
    }
}
