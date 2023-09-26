using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class VertexTable
    {
        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Flags : uint
        {
            UV = 0x00,
            Tangent = 0x01,
            Unknown = 0x08
        }

        public class Vertex
        {
            /// <summary>
            /// The position of this vertex in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The weights of this vertex.
            /// TODO: How do these affect things?
            /// </summary>
            public byte[] Weights { get; set; } = new byte[4];

            /// <summary>
            /// The indices of this vertex.
            /// TODO: How do these affect things?
            /// </summary>
            public byte[] Indices { get; set; } = new byte[4];

            /// <summary>
            /// The tangent space normals for this vertex.
            /// </summary>
            public uint[]? Tangent { get; set; }

            /// <summary>
            /// The UV Coordinates for this vertex, stored as halfs but converted to floats on read.
            /// </summary>
            public float[] UVCoordinates { get; set; } = new float[2];

            /// <summary>
            /// Four unknown bytes used when the flag value is set to 8.
            /// TODO: What are these?
            /// </summary>
            public byte[]? Flag8_UnknownBytes { get; set; }
        }

        /// <summary>
        /// The hash that identifies this vertex table.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// The list of the vertices in this table.
        /// </summary>
        public Vertex[] Vertices { get; set; } = Array.Empty<Vertex>();

        public override string ToString() => $"Vertex Table: 0x{Hash.ToString("X").PadLeft(0x08, '0')}";

        /// <summary>
        /// The size of the vertices in this table, calculated during and used for writing.
        /// </summary>
        private int VertexSize = 0x1C;

        /// <summary>
        /// Read the data of this vertex table from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static VertexTable Read(BinaryReaderEx reader)
        {
            // Initialise the vertex table.
            VertexTable vertexTable = new();

            // Read the count of vertices in this vertex table.
            uint vertexCount = reader.ReadUInt32();

            // Initialise the vertex table's vertices array.
            vertexTable.Vertices = new Vertex[vertexCount];

            // Read the flags for this vertex table.
            Flags vertexFlags = (Flags)reader.ReadUInt32();

            // Read this vertex table's hash.
            vertexTable.Hash = reader.ReadUInt64();

            // Read the offset to this vertex table's data.
            uint vertexTableDataOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the vertex table offset.
            uint vertexTableDataSize = reader.ReadUInt32();

            // Save our position to jump back to after reading the vertex table.
            long position = reader.BaseStream.Position;

            // Jump to this vertex table's data offset.
            reader.JumpTo(vertexTableDataOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{vertexTableDataOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Loop through and read each vertex.
            for (int i = 0; i < vertexCount; i++)
            {
                // Define a new vertex entry.
                Vertex vertex = new();

                // Read this vertex's position.
                vertex.Position = reader.ReadVector3();

                // Read this vertex's weights.
                // TODO: Is this correct?
                vertex.Weights = reader.ReadBytes(0x04);

                // Read this vertex's weight indices.
                // TODO: Is this correct?
                vertex.Indices = reader.ReadBytes(0x04);

                // If this vertex table is using tangent values, then read them too.
                if (vertexFlags == Flags.Tangent)
                {
                    vertex.Tangent = new uint[2];
                    vertex.Tangent[0] = reader.ReadUInt32();
                    vertex.Tangent[1] = reader.ReadUInt32();
                }

                // Read this vertex's UV coordinates.
                vertex.UVCoordinates[0] = (float)reader.ReadHalf();
                vertex.UVCoordinates[1] = (float)reader.ReadHalf();

                // Read the unknown bytes added by flag 8.
                if (vertexFlags == Flags.Unknown)
                    vertex.Flag8_UnknownBytes = reader.ReadBytes(0x04);

                // Save this vertex.
                vertexTable.Vertices[i] = vertex;
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return our read vertex table.
            return vertexTable;
        }

        /// <summary>
        /// Writes the data of this vertex table to the writer's current position.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void Write(BinaryWriterEx writer, int nodeIndex)
        {
            // Write the Node Type.
            writer.Write(0x02);

            // Write empty values for the sub node count and offset, as vertex tables don't have them.
            writer.Write(0);
            writer.Write(0L);

            // Write the amount of vertices in this vertex table.
            writer.Write(Vertices.Length);

            // Write the flag value depending on if this vertex table has any tangent or unknown bytes.
            // Also calculate the vertex size.
            if (Vertices[0].Tangent != null)
            {
                writer.Write(0x01);
                VertexSize = 0x24;
            }
            else if (Vertices[0].Flag8_UnknownBytes != null)
            {
                writer.Write(0x08);
                VertexSize = 0x20;
            }
            else
            {
                writer.Write(0x00);
            }

            // Write this vertex table's hash.
            writer.Write(Hash);

            // Add an offset for this face table's data.
            writer.AddOffset($"VertexTable{nodeIndex}Data");

            // Write the length of this vertex table's data, including the data magic value and size.
            writer.Write((VertexSize * Vertices.Length) + 0x08);
        }

        /// <summary>
        /// Write this vertex table's vertex data.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="nodeIndex">The index of this node.</param>
        public void WriteData(BinaryWriterEx writer, int nodeIndex)
        {
            // Fill in the offset for this texture's image.
            writer.FillOffset($"VertexTable{nodeIndex}Data");

            // Write the data magic value.
            writer.Write(0xFFFFFF);

            // Write the length of this vertex table's data.
            writer.Write(VertexSize * Vertices.Length);

            // Loop through and write each vertex.
            for (int i = 0; i < Vertices.Length; i++)
            {
                // Write this vertex's position.
                writer.Write(Vertices[i].Position);

                // Write this vertex's weights.
                writer.Write(Vertices[i].Weights);

                // Write this vertex's indices.
                writer.Write(Vertices[i].Indices);

                // If this vertex has tangent values, then write them.
                if (Vertices[i].Tangent != null)
                {
                    writer.Write(Vertices[i].Tangent[0]);
                    writer.Write(Vertices[i].Tangent[1]);
                }

                // Write this vertex's UV Coordinates.
                writer.Write((Half)Vertices[i].UVCoordinates[0]);
                writer.Write((Half)Vertices[i].UVCoordinates[1]);

                // If this vertex has the Flag 8 bytes, then write them.
                if (Vertices[i].Flag8_UnknownBytes != null)
                    writer.Write(Vertices[i].Flag8_UnknownBytes);
            }

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);
        }
    }
}
