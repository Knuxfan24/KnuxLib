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

        /// <summary>
        /// Writes the data of this face table to the writer's current position.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void Write(BinaryWriterEx writer, int nodeIndex)
        {
            // Write the Node Type.
            writer.Write(0x03);

            // Write empty values for the sub node count and offset, as face tables don't have them.
            writer.Write(0);
            writer.Write(0L);

            // Write the face indices count.
            writer.Write(Faces.Length * 3);

            // Write an unknown value of 0x02.
            writer.Write(0x02);

            // Write this face table's hash.
            writer.Write(Hash);

            // Add an offset for this face table's data.
            writer.AddOffset($"FaceTable{nodeIndex}Data");

            // Write the length of this face table's data, including the data magic value and size.
            writer.Write((Faces.Length * 0x0C) + 0x08);
        }

        /// <summary>
        /// Write this face table's data.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void WriteData(BinaryWriterEx writer, int nodeIndex)
        {
            // Fill in the offset for this face table's data.
            writer.FillOffset($"FaceTable{nodeIndex}Data");

            // Write the data magic value.
            writer.Write(0xFFFFFF);

            // Write the length of this texture's data.
            writer.Write(Faces.Length * 0x0C);

            // Loop through and write each face's indices.
            for (int i = 0; i < Faces.Length; i++)
            {
                writer.Write(Faces[i].IndexA);
                writer.Write(Faces[i].IndexB);
                writer.Write(Faces[i].IndexC);
            }

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);
        }
    }
}
