using System.Diagnostics;

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
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.collision_2010.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public List<ulong> UnknownValues { get; set; } = new();

            public Mesh[] Meshes { get; set; } = new Mesh[3];
        }

        public class Mesh
        {
            public List<Vector3> Vertices { get; set; } = new();

            public List<Face> Faces { get; set; } = new();
        }

        public class Face
        {
            public ushort[] VertexIndices { get; set; } = new ushort[3];

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
            uint UnknownUInt32_1 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint UnknownUInt32_2 = reader.ReadUInt32();

            // Read the amount of entries in the unknown data chunk.
            uint UnknownDataCount = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint UnknownUInt32_4 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint UnknownUInt32_5 = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: What is this?
            uint UnknownUInt32_6 = reader.ReadUInt32();

            // Read the offset to the unknown data chunk.
            uint UnknownDataOffset = reader.ReadUInt32();

            // Read the offset to the first mesh.
            uint Mesh1Offset = reader.ReadUInt32();

            // Read the offset to the second mesh.
            uint Mesh2Offset = reader.ReadUInt32();

            // Read the offset to the third mesh.
            uint Mesh3Offset = reader.ReadUInt32();

            // Jump to the unknown data chunk.
            reader.JumpTo(UnknownDataOffset, true);

            // Loop through and read the values in the unknown data chunk.
            for (int i = 0; i < UnknownDataCount; i++)
            {
                // TODO: What is this data? Each entry is eight bytes, first four are always 0.
                Data.UnknownValues.Add(reader.ReadUInt64());
            }

            // Jump to and read the various meshes in this file.
            reader.JumpTo(Mesh1Offset, true);
            ReadMesh(reader, 0);
            reader.JumpTo(Mesh2Offset, true);
            ReadMesh(reader, 1);
            reader.JumpTo(Mesh3Offset, true);
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
            // TODO: Maybe check if we read an N?
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
            uint UnknownUInt32_1 = reader.ReadUInt32();

            // Skip an unknown value of 0.001.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 255.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x04);

            //Read this mesh's vertex count.
            uint VertexCount = reader.ReadUInt32();

            // Read this mesh's face count.
            uint FaceCount = reader.ReadUInt32();

            // Loop through and read this mesh's vertices.
            for (int i = 0; i < VertexCount; i++)
                Data.Meshes[meshIndex].Vertices.Add(reader.ReadVector3());

            // Loop through and read each face of this mesh.
            for (int i = 0; i < FaceCount; i++)
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
            for (int i = 0; i < FaceCount; i++)
                Data.Meshes[meshIndex].Faces[i].Tag = reader.ReadUInt16();

            // TODO: Is this count always one lower than it should be? Every file I've checked seems to end up slightly off.
            // TODO: Does the data this is counting use the same byte vs ushort setup as the faces?
            uint UnknownCount_1 = reader.ReadUInt32();

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
            for (int i = 0; i < Data.Meshes.Length; i++)
            {
                // Check that this mesh actually exists.
                if (Data.Meshes[i] != null)
                {
                    // Write this mesh's vertices.
                    foreach (Vector3 vertex in Data.Meshes[i].Vertices)
                        obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                    // Write this mesh's name.
                    obj.WriteLine($"o collisionmesh_{i}");
                    obj.WriteLine($"g collisionmesh_{i}");

                    // Write this mesh's faces.
                    foreach (Face face in Data.Meshes[i].Faces)
                        obj.WriteLine($"f {face.VertexIndices[0] + 1 + vertexCount} {face.VertexIndices[1] + 1 + vertexCount} {face.VertexIndices[2] + 1 + vertexCount}");

                    // Increment vertexCount.
                    vertexCount += Data.Meshes[i].Vertices.Count;
                }
            }

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
