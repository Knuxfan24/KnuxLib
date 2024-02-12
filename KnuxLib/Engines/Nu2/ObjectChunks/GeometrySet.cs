using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: Work out the data that makes up an Xbox geometry entry.
    // TODO: The PlayStation 2 version uses a completely different chunk (OBJ0) for geometry data. Reverse engineer that version.
    public class GeometrySet
    {
        // Classes for this NuObject chunk.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GeometryType
        {
            Mesh = 0,
            Plane = 1
        }

        public enum PrimitiveType
        {
            TriangleList = 5,
            TriangleStrip = 6
        }

        public class GeometryData
        {
            /// <summary>
            /// Whether this Geometry is a 3D Mesh or a 2D Plane.
            /// </summary>
            public GeometryType Type { get; set; }

            /// <summary>
            /// The list of Meshes that make up this Geometry entry.
            /// </summary>
            public List<MeshSet> Meshes { get; set; } = new();
        }

        public class MeshSet
        {
            /// <summary>
            /// The index of the material in the file's material set that this mesh should use.
            /// </summary>
            public uint MaterialIndex { get; set; }

            /// <summary>
            /// An unknown integer value only found in plane meshes.
            /// TODO: What is this?
            /// </summary>
            public uint? UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The list of vertices that make up the points in this mesh.
            /// </summary>
            public List<MeshVertex> Vertices { get; set; } = new();

            /// <summary>
            /// A list of triangle strips, if this mesh uses them.
            /// </summary>
            public List<List<ushort>>? PrimitiveTriangleStrips { get; set; }

            /// <summary>
            /// A list of face indices, if this mesh DOESN'T use triangle strips.
            /// </summary>
            public List<ushort>? PrimitiveTriangleList { get; set; }
        }

        public class MeshVertex
        {
            /// <summary>
            /// The position of this vertex in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The normals for this vertex.
            /// </summary>
            public Vector3? Normals { get; set; }

            /// <summary>
            /// The RGBA colour value of this vertex.
            /// </summary>
            public VertexColour Colour { get; set; } = new();

            /// <summary>
            /// The coordinates for the texture applied to this vertex.
            /// Used for scale instead if this vertex belongs to a plane mesh.
            /// </summary>
            public Vector2 TextureCoordinates { get; set; }
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static List<GeometryData> Read(BinaryReaderEx reader, FormatVersion version)
        {
            // Set up our list of geometry entires.
            List<GeometryData> Geometry = new();

            // Read the amount of geometry entries in this file.
            uint geometryCount = reader.ReadUInt32();

            // Loop through and read each geometry entry.
            for (int geometryIndex = 0; geometryIndex < geometryCount; geometryIndex++)
            {
                // Set up a geometry entry.
                GeometryData geometry = new();

                // Skip an unknown value of 0x01.
                reader.JumpAhead(0x04);

                // Read this geometry entry's type.
                geometry.Type = (GeometryType)reader.ReadUInt32();

                // Set up a value for this geometry's mesh count.
                uint geometryMeshCount = 0;

                // Read this geometry differently depending on if it's a mesh or plane.
                switch (geometry.Type)
                {
                    case GeometryType.Mesh:
                        // Skip three unknown values of 0x00.
                        reader.JumpAhead(0xC);

                        // Read how many meshes are in this geometry entry.
                        geometryMeshCount = reader.ReadUInt32();

                        // Loop through each mesh.
                        for (int geometryMeshIndex = 0; geometryMeshIndex < geometryMeshCount; geometryMeshIndex++)
                        {
                            // Set up a new mesh entry.
                            MeshSet mesh = new();

                            // Read this mesh's material index.
                            mesh.MaterialIndex = reader.ReadUInt32();

                            // Read the vertex count for this mesh.
                            uint meshVertexCount = reader.ReadUInt32();

                            // Loop through each of this mesh's vertices.
                            for (int meshVertexIndex = 0; meshVertexIndex < meshVertexCount; meshVertexIndex++)
                            {
                                // Set up a vertex entry.
                                MeshVertex vertex = new();

                                // Read this vertex's position.
                                vertex.Position = reader.ReadVector3();

                                // Read this vertex's normals.
                                vertex.Normals = reader.ReadVector3();

                                // Read this vertex's colour.
                                vertex.Colour.Alpha = reader.ReadByte();
                                vertex.Colour.Red = reader.ReadByte();
                                vertex.Colour.Green = reader.ReadByte();
                                vertex.Colour.Blue = reader.ReadByte();

                                // Read this vertex's texture coordinates.
                                vertex.TextureCoordinates = reader.ReadVector2();

                                // Save this vertex.
                                mesh.Vertices.Add(vertex);
                            }

                            // Skip two unknown values of 0x00 and 0x01
                            reader.JumpAhead(0x8);

                            // Read this mesh's primitive type.
                            PrimitiveType primitiveType = (PrimitiveType)reader.ReadUInt32();

                            // Read the face count for this mesh's primitive.
                            uint primitiveFaceCount = reader.ReadUInt32();

                            // Read this mesh's primitive differently depending on if it uses a triangle list or a triangle strip.
                            switch (primitiveType)
                            {
                                case PrimitiveType.TriangleList:
                                    // Initialise the mesh's triangle list.
                                    mesh.PrimitiveTriangleList = new();

                                    // Read each face entry and save it to the triangle list.
                                    for (int primitiveFaceIndex = 0; primitiveFaceIndex < primitiveFaceCount; primitiveFaceIndex++)
                                        mesh.PrimitiveTriangleList.Add(reader.ReadUInt16());

                                    break;

                                case PrimitiveType.TriangleStrip:
                                    // Initialise the mesh's triangle strips.
                                    mesh.PrimitiveTriangleStrips = new();

                                    // Track how many values we've read.
                                    uint readValues = 0;

                                    // Keep going until we've read the right amount of values.
                                    while (readValues < primitiveFaceCount)
                                    {
                                        // Set up a triangle strip.
                                        List<ushort> triangleStrip = new();

                                        // Find out how many values are in this triangle strip.
                                        ushort triangleStripFaceCount = reader.ReadUInt16();

                                        // Increment readShorts as these count numbers are treated as part of the face count.
                                        readValues++;

                                        // Loop through based on the amount of faces in this strip.
                                        for (int traingleStripFaceIndex = 0; traingleStripFaceIndex < triangleStripFaceCount; traingleStripFaceIndex++)
                                        {
                                            // Read a face index into the triangle strip.
                                            triangleStrip.Add(reader.ReadUInt16());

                                            // Increment readShorts.
                                            readValues++;
                                        }

                                        // Add this triangle strip to the list of triangle strips in this primitive.
                                        mesh.PrimitiveTriangleStrips.Add(triangleStrip);
                                    }
                                    break;
                            }

                            // Skip two unknown values of 0x00.
                            reader.JumpAhead(0x8);

                            // Add this mesh to our geometry entry.
                            geometry.Meshes.Add(mesh);
                        }
                        break;

                    case GeometryType.Plane:
                        // Read how many meshes are in this geometry entry.
                        geometryMeshCount = reader.ReadUInt32();

                        // Loop through each mesh.
                        for (int geometryMeshIndex = 0; geometryMeshIndex < geometryMeshCount; geometryMeshIndex++)
                        {
                            // Set up a new mesh entry.
                            MeshSet mesh = new();

                            // Read this mesh's material index.
                            mesh.MaterialIndex = reader.ReadUInt32();

                            // Read the vertex count for this mesh.
                            uint meshVertexCount = reader.ReadUInt32();

                            // Read the unknown integer value only found in plane meshes.
                            mesh.UnknownUInt32_1 = reader.ReadUInt32();

                            // Skip an unknown value of 0x00.
                            reader.JumpAhead(0x04);

                            // Loop through each of this mesh's vertices.
                            for (int meshVertexIndex = 0; meshVertexIndex < meshVertexCount; meshVertexIndex++)
                            {
                                // Set up a vertex entry.
                                MeshVertex vertex = new();

                                // Read this vertex's position.
                                vertex.Position = reader.ReadVector3();

                                // Read this vertex's texture coordinates.
                                vertex.TextureCoordinates = reader.ReadVector2();

                                // Read this vertex's colour.
                                vertex.Colour.Read(reader);

                                // Save this vertex.
                                mesh.Vertices.Add(vertex);
                            }

                            // Add this mesh to our geometry entry.
                            geometry.Meshes.Add(mesh);
                        }
                        break;
                }

                // Add this geometry entry to our list.
                Geometry.Add(geometry);
            }

            // Return the list of geometry entries read from the file.
            return Geometry;
        }
    }
}
