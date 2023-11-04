namespace KnuxLib.Engines.Hedgehog
{
    public class Collision_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision_2010() { }
        public Collision_2010(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
            {
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.collision_2010.json", Data);
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
            }
        }

        // Classes for this format.
        public class FormatData
        {
            // An unknown set of values that are always eight bytes long.
            // TODO: What are these?
            public List<ulong> UnknownValues { get; set; } = new();

            // The set of three (by default) meshes that make up this collision file.
            public Mesh[] Meshes { get; set; } = new Mesh[3];
        }

        public class Mesh
        {
            // This mesh's vertices.
            public List<Vector3> Vertices { get; set; } = new();

            // This mesh's faces.
            public List<Face> Faces { get; set; } = new();
        }

        public class Face
        {
            // The three vertex indices that make up this face.
            public ushort[] VertexIndices { get; set; } = new ushort[3];

            // This face's tag(?).
            public ushort Tag { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Skip an unknown value of 0x000368E0.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_1 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_2 = reader.ReadUInt32();

            // Read the amount of entries in the unknown data chunk.
            uint unknownDataCount = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_4 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_5 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_6 = reader.ReadUInt32();

            // Read the offset to the unknown data chunk.
            uint unknownDataOffset = reader.ReadUInt32();

            // Read the offset to the first mesh.
            uint mesh1Offset = reader.ReadUInt32();

            // Read the offset to the second mesh.
            uint mesh2Offset = reader.ReadUInt32();

            // Read the offset to the third mesh.
            uint mesh3Offset = reader.ReadUInt32();

            // Jump to the unknown data chunk.
            reader.JumpTo(unknownDataOffset, true);

            // Loop through and read the values in the unknown data chunk.
            for (int unknownDataIndex = 0; unknownDataIndex < unknownDataCount; unknownDataIndex++)
            {
                // TODO: What is this data? Each entry is eight bytes, first four are always 0.
                Data.UnknownValues.Add(reader.ReadUInt64());
            }

            // Jump to and read the various meshes in this file.
            reader.JumpTo(mesh1Offset, true);
            ReadMesh(reader, 0);
            reader.JumpTo(mesh2Offset, true);
            ReadMesh(reader, 1);
            reader.JumpTo(mesh3Offset, true);
            ReadMesh(reader, 2);

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Reads the NXS.MESH data.
        /// </summary>
        /// <param name="reader">The Marathon BINAReader to use.</param>
        /// <param name="meshIndex">The index of the mesh we're reading.</param>
        private void ReadMesh(BINAReader reader, int meshIndex)
        {
            // Switch to a little endian system for reading the mesh.
            // TODO: Will I actually need to switch back to big endian at some point?
            reader.IsBigEndian = false;

            // Not every file has the second and third mesh, so check for them.
            if (reader.ReadByte() == 0)
            {
                // I've seen at least one mesh be one byte off the offset, so check for that.
                if (reader.ReadChar() != 'N')
                    return;
                else
                    reader.JumpBehind(0x01);
            }

            // If we didn't read a 0 here, then jump back.
            else
            {
                reader.JumpBehind(0x01);
            }

            // Create a new mesh entry in the specified slot.
            Data.Meshes[meshIndex] = new();

            // Read the NXS.MESH string.
            reader.ReadSignature(0x08, "NXS\u0001MESH");

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read an unknown value.
            // TODO: What is this?
            uint unknownUInt32_1 = reader.ReadUInt32();

            // Skip an unknown value of 0.001.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 255.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x04);

            //Read this mesh's vertex count.
            uint vertexCount = reader.ReadUInt32();

            // Read this mesh's face count.
            uint faceCount = reader.ReadUInt32();

            // Loop through and read this mesh's vertices.
            for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                Data.Meshes[meshIndex].Vertices.Add(reader.ReadVector3());

            // Loop through and read each face of this mesh.
            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
            {
                // Set up a new face.
                Face face = new();

                // Read the three vertex indices for this face.
                // TODO: How is the data size actually decided? Some files only use a byte, most use a ushort.
                // TODO: Are there files that use ushorts but have less than 256 verts?
                if (Data.Meshes[meshIndex].Vertices.Count < 256)
                {
                    face.VertexIndices[0] = reader.ReadByte();
                    face.VertexIndices[1] = reader.ReadByte();
                    face.VertexIndices[2] = reader.ReadByte();
                }
                else
                {
                    face.VertexIndices[0] = reader.ReadUInt16();
                    face.VertexIndices[1] = reader.ReadUInt16();
                    face.VertexIndices[2] = reader.ReadUInt16();
                }

                // Save this face.
                Data.Meshes[meshIndex].Faces.Add(face);
            }

            // Read each face's tag.
            // TODO: Is this right?
            for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                Data.Meshes[meshIndex].Faces[faceIndex].Tag = reader.ReadUInt16();

            // TODO: Is this count always one lower than it should be? Every file I've checked seems to end up slightly off.
            // TODO: Does the data this is counting use the same byte vs ushort setup as the faces?
            uint unknownCount_1 = reader.ReadUInt32();

            // TODO: Read the rest of this data.

            // Switch back to the Big Endian setup.
            reader.IsBigEndian = true;
        }

        /// <summary>
        /// Exports this collision model to a standard Wavefront OBJ.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Set up a variable to track vertices.
            int vertexCount = 0;

            // Loop through each mesh in the model.
            for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
            {
                // Check that this mesh actually exists.
                if (Data.Meshes[meshIndex] != null)
                {
                    // Write this mesh's vertices.
                    foreach (Vector3 vertex in Data.Meshes[meshIndex].Vertices)
                        obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                    // Write this mesh's name.
                    obj.WriteLine($"o collisionmesh_{meshIndex}");
                    obj.WriteLine($"g collisionmesh_{meshIndex}");

                    // Write this mesh's faces.
                    foreach (Face face in Data.Meshes[meshIndex].Faces)
                        obj.WriteLine($"f {face.VertexIndices[0] + 1 + vertexCount} {face.VertexIndices[1] + 1 + vertexCount} {face.VertexIndices[2] + 1 + vertexCount}");

                    // Increment vertexCount.
                    vertexCount += Data.Meshes[meshIndex].Vertices.Count;
                }
            }

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
