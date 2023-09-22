namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class FaceTable
    {
        public class Face
        {
            /// <summary>
            /// The index of this face's first vertex.
            /// </summary>
            public uint IndexA { get; set; }

            /// <summary>
            /// The index of this face's second vertex.
            /// </summary>
            public uint IndexB { get; set; }

            /// <summary>
            /// The index of this face's third vertex.
            /// </summary>
            public uint IndexC { get; set; }
        }

        /// <summary>
        /// The hash that identifies this face table.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// The list of faces in this table.
        /// </summary>
        public Face[] Faces { get; set; } = Array.Empty<Face>();

        /// <summary>
        /// Read the data of this face table from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static FaceTable Read(BinaryReaderEx reader)
        {
            // Initialise the face table.
            FaceTable faceTable = new();

            // Read the value for the face indices and divide it by 3 to get the actual face count.
            uint faceCount = reader.ReadUInt32() / 3;

            // Skip an unknown value of 0x02.
            reader.JumpAhead(0x04);

            // Read this face table's hash.
            faceTable.Hash = reader.ReadUInt64();

            // Read the offset to this face table's data.
            uint faceTableDataOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the face table offset.
            uint faceTableDataSize = reader.ReadUInt32();

            // Save our position to jump back to after reading the face table.
            long position = reader.BaseStream.Position;

            // Jump to this face table's data offset.
            reader.JumpTo(faceTableDataOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{faceTableDataOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Initialise the face table's face array.
            faceTable.Faces = new Face[faceCount];

            // Loop through and read each face.
            for (int i = 0; i < faceCount; i++)
            {
                faceTable.Faces[i] = new Face
                {
                    IndexA = reader.ReadUInt32(),
                    IndexB = reader.ReadUInt32(),
                    IndexC = reader.ReadUInt32()
                };
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return our read face table.
            return faceTable;
        }
    }
}
