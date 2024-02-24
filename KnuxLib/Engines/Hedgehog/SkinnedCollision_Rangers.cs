using static KnuxLib.Engines.Hedgehog.Collision_Rangers;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Properly reverse engineer all the unknown data.
    // TODO: Importing.
    public class SkinnedCollision_Rangers : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SkinnedCollision_Rangers() { }
        public SkinnedCollision_Rangers(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The name of this collision.
            /// TODO: Is this used or is it just an identifier?
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The meshes that make up this collision file.
            /// </summary>
            public Mesh[] Meshes { get; set; } = Array.Empty<Mesh>();

            /// <summary>
            /// The bones that are referenced by this collision.
            /// TODO: Can I find what things in the mesh use indices into this list and just put the strings directly in them instead?
            /// </summary>
            public string[] Bones { get; set; } = Array.Empty<string>();
        }

        public class Mesh
        {
            /// <summary>
            /// The name of this mesh.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The vertices that make up this mesh.
            /// </summary>
            public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// An unknown list of short values that act as indices into the bone table. In some way, this controls how things are attached to the model's bones.
            /// TODO: How does the data here get referenced and used?
            /// </summary>
            public ushort[]? UnknownBoneIndicesTable { get; set; }

            /// <summary>
            /// An unknown list of data, that in some way seems to control climbing?
            /// TODO: What does this properly do?
            /// </summary>
            public UnknownData_1[]? UnknownDataArray_1 { get; set; }

            /// <summary>
            /// An unknown list of data, that in some way seems to control how meshes in this collision fit together?
            /// TODO: What does this properly do?
            /// </summary>
            public UnknownData_2[]? UnknownDataArray_2 { get; set; }

            /// <summary>
            /// An unknown list of data.
            /// TODO: What does this do?
            /// </summary>
            public UnknownData_3[]? UnknownDataArray_3 { get; set; }

            /// <summary>
            /// The faces that make up this mesh.
            /// </summary>
            public Face[] Faces { get; set; } = Array.Empty<Face>();

            public override string ToString() => Name;
        }

        // TODO: Make sure the data types across these are correct and figure out their purposes.
        public class UnknownData_1
        {
            public ushort UnknownUShort_1 { get; set; }

            public ushort UnknownUShort_2 { get; set; }

            public float UnknownFloat_1 { get; set; }
        }

        public class UnknownData_2
        {
            public ushort UnknownUShort_1 { get; set; }

            public ushort UnknownUShort_2 { get; set; }

            public ushort UnknownUShort_3 { get; set; }

            public ushort UnknownUShort_4 { get; set; }

            public Quaternion UnknownQuaternion_1 { get; set; }
        }

        public class UnknownData_3
        {
            public uint UnknownUInt32_1 { get; set; }

            public ushort UnknownUShort_1 { get; set; }

            public ushort UnknownUShort_2 { get; set; }

            public ushort UnknownUShort_3 { get; set; }

            public ushort UnknownUShort_4 { get; set; }

            public ushort UnknownUShort_5 { get; set; }

            public ushort UnknownUShort_6 { get; set; }

            public float UnknownFloat_1 { get; set; }

            public float UnknownFloat_2 { get; set; }

            public float UnknownFloat_3 { get; set; }

            public float UnknownFloat_4 { get; set; }

            public float UnknownFloat_5 { get; set; }

            public float UnknownFloat_6 { get; set; }

            public float UnknownFloat_7 { get; set; }

            public float UnknownFloat_8 { get; set; }
        }

        public class Face
        {
            /// <summary>
            /// The index of this face's first vertex.
            /// </summary>
            public ushort IndexA { get; set; }

            /// <summary>
            /// The index of this face's second vertex.
            /// </summary>
            public ushort IndexB { get; set; }

            /// <summary>
            /// The index of this face's third vertex.
            /// </summary>
            public ushort IndexC { get; set; }

            /// <summary>
            /// This face's layer.
            /// </summary>
            public LayerType Layer { get; set; }

            /// <summary>
            /// This face's material.
            /// </summary>
            public Material Material { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip an unknown value that is always 1.
            reader.JumpAhead(0x08);

            // Read this collision's name.
            Data.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Read the count of meshes in this file.
            uint meshCount = reader.ReadUInt32();

            // Read the count of bone names in this file.
            uint boneNameCount = reader.ReadUInt32();

            // Read the offset to this file's mesh table.
            long meshTableOffset = reader.ReadInt64();

            // Read the offset to this file's bone name table.
            long boneNameTableOffset = reader.ReadInt64();

            // Initialise the meshes and bones arrays with the apporiate sizes.
            Data.Meshes = new Mesh[meshCount];
            Data.Bones = new string[boneNameCount];

            // Jump to the mesh table.
            reader.JumpTo(meshTableOffset, false);

            // Loop through each mesh in this file.
            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                // Initialise a new mesh entry.
                Mesh mesh = new();

                // Read this mesh's name.
                mesh.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this mesh's vertex count.
                uint vertexCount = reader.ReadUInt32();

                // Read the count of entries in the unknown bone indices table.
                uint unknownBoneIndicesCount = reader.ReadUInt32();

                // Read the count of entries in the first unknown data table.
                uint unknownData1TableCount = reader.ReadUInt32();

                // Read the count of entries in the second unknown data table.
                uint unknownData2TableCount = reader.ReadUInt32();

                // Read the count of entries in the third unknown data table.
                uint unknownData3TableCount = reader.ReadUInt32();

                // Read this mesh's face count.
                uint faceCount = reader.ReadUInt32();

                // Read the offset to this mesh's vertex table.
                long vertexTableOffset = reader.ReadInt64();

                // Read the offset to this mesh's unknown bone indices table.
                long unknownBoneIndicesOffset = reader.ReadInt64();

                // Read the offset to this mesh's first unknown data table.
                long unknownData1TableOffset = reader.ReadInt64();

                // Read the offset to this mesh's second unknown data table.
                long unknownData2TableOffset = reader.ReadInt64();

                // Read the offset to this mesh's third unknown data table.
                long unknownData3TableOffset = reader.ReadInt64();

                // Read the offset to this mesh's face table.
                long faceTableOffset = reader.ReadInt64();

                // Read the offset to this mesh's surface type table.
                long surfaceTypeTableOffset = reader.ReadInt64();

                // Initialise this mesh's arrays to the approriate values (if they need to be initialised).
                mesh.Vertices = new Vector3[vertexCount];
                if (unknownBoneIndicesCount != 0) mesh.UnknownBoneIndicesTable = new ushort[unknownBoneIndicesCount];
                if (unknownData1TableCount != 0) mesh.UnknownDataArray_1 = new UnknownData_1[unknownData1TableCount];
                if (unknownData2TableCount != 0) mesh.UnknownDataArray_2 = new UnknownData_2[unknownData2TableCount];
                if (unknownData3TableCount != 0) mesh.UnknownDataArray_3 = new UnknownData_3[unknownData3TableCount];
                mesh.Faces = new Face[faceCount];

                // Save our current position so we can jump back for the next mesh.
                long position = reader.BaseStream.Position;

                // Jump to this mesh's vertex table.
                reader.JumpTo(vertexTableOffset, false);

                // Loop through each vertex.
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    // Read this vertex's position.
                    mesh.Vertices[vertexIndex] = Helpers.ReadHedgeLibVector3(reader);

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x04);
                }

                // Jump to this mesh's unknown bone indices table.
                reader.JumpTo(unknownBoneIndicesOffset, false);

                // Loop through and read each bone index.
                for (int boneIndex = 0; boneIndex < unknownBoneIndicesCount; boneIndex++)
                    mesh.UnknownBoneIndicesTable[boneIndex] = reader.ReadUInt16();

                // Jump to this mesh's first unknown data table.
                reader.JumpTo(unknownData1TableOffset, false);

                // Loop through and read each entry in this mesh's first unknown data table.
                for (int unknownIndex = 0; unknownIndex < unknownData1TableCount; unknownIndex++)
                {
                    mesh.UnknownDataArray_1[unknownIndex] = new()
                    {
                        UnknownUShort_1 = reader.ReadUInt16(),
                        UnknownUShort_2 = reader.ReadUInt16(),
                        UnknownFloat_1 = reader.ReadSingle()
                    };
                }

                // Jump to this mesh's second unknown data table.
                reader.JumpTo(unknownData2TableOffset, false);

                // Loop through and read each entry in this mesh's second unknown data table.
                for (int unknownIndex = 0; unknownIndex < unknownData2TableCount; unknownIndex++)
                {
                    mesh.UnknownDataArray_2[unknownIndex] = new()
                    {
                        UnknownUShort_1 = reader.ReadUInt16(),
                        UnknownUShort_2 = reader.ReadUInt16(),
                        UnknownUShort_3 = reader.ReadUInt16(),
                        UnknownUShort_4 = reader.ReadUInt16(),
                        UnknownQuaternion_1 = Helpers.ReadHedgeLibQuaternion(reader)
                    };
                }

                // Jump to this mesh's third unknown data table.
                reader.JumpTo(unknownData3TableOffset, false);

                // Loop through and read each entry in this mesh's third unknown data table.
                for (int unknownIndex = 0; unknownIndex < unknownData3TableCount; unknownIndex++)
                {
                    mesh.UnknownDataArray_3[unknownIndex] = new()
                    {
                        UnknownUInt32_1 = reader.ReadUInt32(),
                        UnknownUShort_1 = reader.ReadUInt16(),
                        UnknownUShort_2 = reader.ReadUInt16(),
                        UnknownUShort_3 = reader.ReadUInt16(),
                        UnknownUShort_4 = reader.ReadUInt16(),
                        UnknownUShort_5 = reader.ReadUInt16(),
                        UnknownUShort_6 = reader.ReadUInt16(),
                        UnknownFloat_1 = reader.ReadSingle(),
                        UnknownFloat_2 = reader.ReadSingle(),
                        UnknownFloat_3 = reader.ReadSingle(),
                        UnknownFloat_4 = reader.ReadSingle(),
                        UnknownFloat_5 = reader.ReadSingle(),
                        UnknownFloat_6 = reader.ReadSingle(),
                        UnknownFloat_7 = reader.ReadSingle(),
                        UnknownFloat_8 = reader.ReadSingle()
                    };
                }

                // Jump to this mesh's face table.
                reader.JumpTo(faceTableOffset, false);

                // Loop through and read each face of this mesh.
                for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                {
                    mesh.Faces[faceIndex] = new()
                    {
                        IndexA = reader.ReadUInt16(),
                        IndexB = reader.ReadUInt16(),
                        IndexC = reader.ReadUInt16()
                    };
                }

                // Jump to this mesh's surface type table.
                reader.JumpTo(surfaceTypeTableOffset, false);

                // Loop through each face of this mesh.
                for (int surfaceTypeOffset = 0; surfaceTypeOffset < faceCount; surfaceTypeOffset++)
                {
                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x01);

                    // Read this face's layer.
                    mesh.Faces[surfaceTypeOffset].Layer = (LayerType)reader.ReadByte();

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x01);

                    // Read this face's material.
                    mesh.Faces[surfaceTypeOffset].Material = (Material)reader.ReadByte();

                    // Skip an unknown value that is always 1.
                    reader.JumpAhead(0x04);
                }

                // Save this mesh.
                Data.Meshes[meshIndex] = mesh;

                // Jump back for the next mesh.
                reader.JumpTo(position);
            }

            // Jump to the bone name table
            reader.JumpTo(boneNameTableOffset, false);

            // Loop through and read each bone name.
            for (int i = 0; i < boneNameCount; i++)
                Data.Bones[i] = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write an unknown value that is always 1.
            writer.Write(0x01L);

            // Add a string for this collision's name.
            writer.AddString("Name", Data.Name, 0x08);

            // Write the count of meshes in this file.
            writer.Write(Data.Meshes.Length);

            // Write the count of bones referenced by this file.
            writer.Write(Data.Bones.Length);

            // Add an offset for this file's mesh table.
            writer.AddOffset("MeshOffsetTable", 0x08);

            // Add an offset for this file's bone table.
            writer.AddOffset("BoneOffsetTable", 0x08);

            // Fill in the offset for the mesh table.
            writer.FillInOffset("MeshOffsetTable", false);

            // Loop through each mesh in this file.
            for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
            {
                // Add a string for this mesh's name.
                writer.AddString($"Mesh{meshIndex}Name", Data.Meshes[meshIndex].Name, 0x08);

                // Write the count of vertices that make up this mesh.
                writer.Write(Data.Meshes[meshIndex].Vertices.Length);

                // If this mesh has the unknown bone indices table, then write its length. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownBoneIndicesTable != null)
                    writer.Write(Data.Meshes[meshIndex].UnknownBoneIndicesTable.Length);
                else
                    writer.Write(0);

                // If this mesh has the first unknown data table, then write its length. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_1 != null)
                    writer.Write(Data.Meshes[meshIndex].UnknownDataArray_1.Length);
                else
                    writer.Write(0);

                // If this mesh has the second unknown data table, then write its length. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_2 != null)
                    writer.Write(Data.Meshes[meshIndex].UnknownDataArray_2.Length);
                else
                    writer.Write(0);

                // If this mesh has the third unknown data table, then write its length. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_3 != null)
                    writer.Write(Data.Meshes[meshIndex].UnknownDataArray_3.Length);
                else
                    writer.Write(0);

                // Write the count of faces that make up this mesh.
                writer.Write(Data.Meshes[meshIndex].Faces.Length);

                // Add an offset for this mesh's vertex table.
                writer.AddOffset($"Mesh{meshIndex}VertexTableOffset", 0x08);

                // If this mesh has the unknown bone indices table, then add an offset for it. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownBoneIndicesTable != null)
                    writer.AddOffset($"Mesh{meshIndex}UnknownUShortArray_1Offset", 0x08);
                else
                    writer.Write(0L);

                // If this mesh has the first unknown data table, then add an offset for it. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_1 != null)
                    writer.AddOffset($"Mesh{meshIndex}UnknownDataArray_1Offset", 0x08);
                else
                    writer.Write(0L);

                // If this mesh has the second unknown data table, then add an offset for it. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_2 != null)
                    writer.AddOffset($"Mesh{meshIndex}UnknownDataArray_2Offset", 0x08);
                else
                    writer.Write(0L);

                // If this mesh has the third unknown data table, then add an offset for it. If not, then just write a 0.
                if (Data.Meshes[meshIndex].UnknownDataArray_3 != null)
                    writer.AddOffset($"Mesh{meshIndex}UnknownDataArray_3Offset", 0x08);
                else
                    writer.Write(0L);

                // Add an offset for this mesh's face table.
                writer.AddOffset($"Mesh{meshIndex}FaceTableOffset", 0x08);

                // Add an offset for this mesh's surface type table.
                writer.AddOffset($"Mesh{meshIndex}SurfaceTableOffset", 0x08);
            }

            // Fill in the offset for the bone table.
            writer.FillInOffset("BoneOffsetTable", false);

            // Loop through and add a string for each bone.
            for (int boneIndex = 0; boneIndex < Data.Bones.Length; boneIndex++)
                writer.AddString($"Bone{boneIndex}Name", Data.Bones[boneIndex], 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Loop through each mesh.
            for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
            {
                // Fill in the offset for this mesh's vertex table.
                writer.FillInOffset($"Mesh{meshIndex}VertexTableOffset", false);

                // Loop through each vertex in this mesh.
                foreach (Vector3 vertex in Data.Meshes[meshIndex].Vertices)
                {
                    // Write this vertex's position.
                    Helpers.WriteHedgeLibVector3(writer, vertex);

                    // Write an unknown value that is always 0.
                    writer.Write(0f);
                }

                // If this mesh has the unknown bone indices table, then write its data.
                if (Data.Meshes[meshIndex].UnknownBoneIndicesTable != null)
                {
                    // Fill in the offset for this mesh's unknown bone indices table.
                    writer.FillInOffset($"Mesh{meshIndex}UnknownUShortArray_1Offset", false);

                    // Loop through each bone index and write it.
                    foreach (ushort boneIndex in Data.Meshes[meshIndex].UnknownBoneIndicesTable)
                        writer.Write(boneIndex);
                }

                // If this mesh has the first unknown data table, then write its data.
                if (Data.Meshes[meshIndex].UnknownDataArray_1 != null)
                {
                    // Realign to 0x04 bytes.
                    // TODO: Should this be after writing the bone indices table instead?
                    writer.FixPadding(0x04);

                    // Fill in the offset for this mesh's first unknown data table.
                    writer.FillInOffset($"Mesh{meshIndex}UnknownDataArray_1Offset", false);

                    // Loop through and write each entry from the first unknown data table.
                    foreach (UnknownData_1 unknownIndex in Data.Meshes[meshIndex].UnknownDataArray_1)
                    {
                        writer.Write(unknownIndex.UnknownUShort_1);
                        writer.Write(unknownIndex.UnknownUShort_2);
                        writer.Write(unknownIndex.UnknownFloat_1);
                    }
                }

                // If this mesh has the second unknown data table, then write its data.
                if (Data.Meshes[meshIndex].UnknownDataArray_2 != null)
                {
                    // Fill in the offset for this mesh's second unknown data table.
                    writer.FillInOffset($"Mesh{meshIndex}UnknownDataArray_2Offset", false);

                    // Loop through and write each entry from the second unknown data table.
                    foreach (UnknownData_2 unknownIndex in Data.Meshes[meshIndex].UnknownDataArray_2)
                    {
                        writer.Write(unknownIndex.UnknownUShort_1);
                        writer.Write(unknownIndex.UnknownUShort_2);
                        writer.Write(unknownIndex.UnknownUShort_3);
                        writer.Write(unknownIndex.UnknownUShort_4);
                        Helpers.WriteHedgeLibQuaternion(writer, unknownIndex.UnknownQuaternion_1);
                    }
                }

                // If this mesh has the third unknown data table, then write its data.
                if (Data.Meshes[meshIndex].UnknownDataArray_3 != null)
                {
                    // Fill in the offset for this mesh's third unknown data table.
                    writer.FillInOffset($"Mesh{meshIndex}UnknownDataArray_3Offset", false);

                    // Loop through and write each entry from the third unknown data table.
                    foreach (var value in Data.Meshes[meshIndex].UnknownDataArray_3)
                    {
                        writer.Write(value.UnknownUInt32_1);
                        writer.Write(value.UnknownUShort_1);
                        writer.Write(value.UnknownUShort_2);
                        writer.Write(value.UnknownUShort_3);
                        writer.Write(value.UnknownUShort_4);
                        writer.Write(value.UnknownUShort_5);
                        writer.Write(value.UnknownUShort_6);
                        writer.Write(value.UnknownFloat_1);
                        writer.Write(value.UnknownFloat_2);
                        writer.Write(value.UnknownFloat_3);
                        writer.Write(value.UnknownFloat_4);
                        writer.Write(value.UnknownFloat_5);
                        writer.Write(value.UnknownFloat_6);
                        writer.Write(value.UnknownFloat_7);
                        writer.Write(value.UnknownFloat_8);
                    }
                }

                // Fill in the offset for this mesh's face table.
                writer.FillInOffset($"Mesh{meshIndex}FaceTableOffset", false);

                // Loop through and write the three indices for each face.
                foreach (Face faceIndex in Data.Meshes[meshIndex].Faces)
                {
                    writer.Write(faceIndex.IndexA);
                    writer.Write(faceIndex.IndexB);
                    writer.Write(faceIndex.IndexC);
                }
                
                // Realign to 0x04 bytes.
                writer.FixPadding(0x04);

                // Fill in the offset for this mesh's surface type table.
                writer.FillInOffset($"Mesh{meshIndex}SurfaceTableOffset", false);

                // Loop through each face.
                foreach (Face faceIndex in Data.Meshes[meshIndex].Faces)
                {
                    // Write an unknown value that is always 0.
                    writer.Write((byte)0);

                    // Write this face's layer value.
                    writer.Write((byte)faceIndex.Layer);

                    // Write an unknown value that is always 0.
                    writer.Write((byte)0);

                    // Write this face's material value.
                    writer.Write((byte)faceIndex.Material);

                    // Write an unknown value that is always 1.
                    writer.Write(0x01);
                }
                
                // If this isn't the last mesh, then realign to 0x10 bytes.
                if (meshIndex != Data.Meshes.Length - 1)
                    writer.FixPadding(0x10);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }

        /// <summary>
        /// Exports this collision model to a standard Wavefront OBJ.
        /// TODO: Export material tags in a less stupid way maybe?
        /// TODO: See if some of the unknown data should really be included in this export too.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Set up a variable to track vertices.
            int vertexCount = 0;

            // Loop through each mesh in the model to write its vertices.
            for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
            {
                // Write each vertex in this mesh.
                foreach (Vector3 vertex in Data.Meshes[meshIndex].Vertices)
                    obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write this mesh's name.
                obj.WriteLine($"o {Data.Meshes[meshIndex].Name}");
                obj.WriteLine($"g {Data.Meshes[meshIndex].Name}");

                // Set up default material type and flag.
                Material materialType = Material.None;
                LayerType materialLayer = LayerType.None;

                // Write this mesh's faces.
                foreach (var face in Data.Meshes[meshIndex].Faces)
                {
                    // Check if the type and flags have been changed since the last face.
                    bool changedValue = false;

                    // Check and set the face's material type.
                    if (face.Material != materialType)
                    {
                        materialType = face.Material;
                        changedValue = true;
                    }

                    // Check and set the face's material layer.
                    if (face.Layer != materialLayer)
                    {
                        materialLayer = face.Layer;
                        changedValue = true;
                    }

                    // If either the flags or type have changed, then write a material entry.
                    if (changedValue)
                        obj.WriteLine($"usemtl {materialType}_{materialLayer}");

                    // Write each face in this mesh.
                    obj.WriteLine($"f {face.IndexA + 1 + vertexCount} {face.IndexB + 1 + vertexCount} {face.IndexC + 1 + vertexCount}");
                }

                // Increment vertexCount.
                vertexCount += Data.Meshes[meshIndex].Vertices.Length;
            }

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
