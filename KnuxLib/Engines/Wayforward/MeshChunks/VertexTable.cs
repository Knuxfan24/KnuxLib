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
            public byte[] Indices { get; set; } = new byte[2];

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
    }
}
