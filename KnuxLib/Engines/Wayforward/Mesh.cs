using Assimp.Configs;
using Assimp;
using KnuxLib.Engines.Wayforward.MeshChunks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward
{
    // Based on https://github.com/meh2481/wfLZEx/blob/master/wf3dEx.cpp
    // TODO: Reverse engineer the data at the unknown offsets in each of the chunks.
    // TODO: Figure out an export solution.
    // TODO: Figure out an import solution.
    // TODO: Get saving fully working once the unknown data in the chunks is sorted out.
    // TODO: Texture decompression/recompression.
    public class Mesh : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Mesh() { }
        public Mesh(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.mesh.json", Data);
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NodeType : uint
        {
            Root = 0x0,
            Texture = 0x1,
            VertexTable = 0x2,
            FaceTable = 0x3,
            TextureMap = 0x4,
            Group = 0x5,
            ObjectMap = 0x6,
            Unknown_1 = 0x7,
            BoneName = 0x8,
            Bone = 0x9,
            Collision = 0xA,
            ObjectData = 0xB,
            Unknown_2 = 0xC
        }

        // Actual data presented to the end user.
        public List<object> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the file signature.
            reader.ReadSignature(4, "WFSN");

            // There's a value here that's usually set to 0 in the .wf3d files, but the .gpu files seem to point to the first data offset.

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Check the type of the root node, which should always be 0.
            if (reader.ReadUInt32() != 0) throw new Exception($"First node in {filepath} is not a Root Node!");

            // Read the amount of nodes in this file.
            uint nodeCount = reader.ReadUInt32();

            // Read the position of the node offset table for this file.
            long nodeOffsetTable = reader.ReadInt64();

            // Jump to the node offset table.
            reader.JumpTo(nodeOffsetTable);

            // Loop through and read each node.
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
                ReadNode(reader, Data);

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Reads a node's type information and determines the functions to be used for reading.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="Data">The user end data to write the read data to.</param>
        private static void ReadNode(BinaryReaderEx reader, List<object> Data)
        {
            // Read the offset for this node's data from the offset table.
            long nodeOffset = reader.ReadInt64();

            // Save our current position so we can jump back for the next node.
            long position = reader.BaseStream.Position;

            // Jump to this node's data.
            reader.JumpTo(nodeOffset);

            // Read this node's type.
            NodeType nodeType = (NodeType)reader.ReadUInt32();

            // Read the count of sub entries in this node.
            uint nodeSubEntryCount = reader.ReadUInt32();

            // Read the offset to this node's sub entry table.
            long nodeSubEntryOffsetTable = reader.ReadInt64();

            // Read each node based on their type.
            switch (nodeType)
            {
                case NodeType.Texture:     Data.Add(Texture.Read(reader));         break;
                case NodeType.VertexTable: Data.Add(VertexTable.Read(reader));     break;
                case NodeType.FaceTable:   Data.Add(FaceTable.Read(reader));       break;
                case NodeType.TextureMap:  Data.Add(TextureMap.Read(reader));      break;
                case NodeType.Group:       Data.Add(Group.Read(reader));           break;
                case NodeType.ObjectMap:   Data.Add(ObjectMap.Read(reader));       break;
                case NodeType.Unknown_1:   Data.Add(Unknown1.Read(reader));        break;
                case NodeType.BoneName:    Data.Add(BoneName.Read(reader));        break;
                case NodeType.Bone:        Data.Add(MeshChunks.Bone.Read(reader)); break;
                case NodeType.Collision:   Data.Add(MeshCollision.Read(reader));   break;
                case NodeType.ObjectData:  Data.Add(ObjectData.Read(reader));      break;
                case NodeType.Unknown_2:   Data.Add(Unknown2.Read(reader));        break;
                default:                   throw new NotImplementedException();
            }

            // If this node has any sub entries, then read them as well.
            if (nodeSubEntryCount > 0)
            {
                // Jump to the sub entry offset table.
                reader.JumpTo(nodeSubEntryOffsetTable);

                // Loop through and read each sub node based on ther type of their parent.
                for (int nodeSubEntryIndex = 0; nodeSubEntryIndex < nodeSubEntryCount; nodeSubEntryIndex++)
                {
                    switch (nodeType)
                    {
                        case NodeType.Group:     ReadNode(reader, (Data.Last() as Group).SubNodes);         break;
                        case NodeType.Unknown_1: ReadNode(reader, (Data.Last() as Unknown1).SubNodes);      break;
                        case NodeType.Collision: ReadNode(reader, (Data.Last() as MeshCollision).SubNodes); break;
                        default:                 throw new NotImplementedException();
                    }
                }
            }

            // Jump back for the next node.
            reader.JumpTo(position);
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a list of offsets.
            List<long> Offsets = new();

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the WFSN signature.
            writer.Write("WFSN");

            // Add an offset to the first data chunk. The .wf3d files don't seem to need this, but the .gpu ones do.
            writer.AddOffset("FirstDataChunk");

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write the root node's type.
            writer.Write(0);

            // Write the amount of nodes in this file.
            writer.Write(Data.Count);

            // Add an offset for the root node's table.
            writer.AddOffset("RootNodeOffsetTable", 0x08);

            // Loop through each node for the main writing.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add an offset for this node.
                Offsets.Add(writer.BaseStream.Position);

                // Write the node based on its type.
                switch (Data[dataIndex].GetType().Name)
                {
                    case "Texture":     (Data[dataIndex] as Texture).Write(writer, dataIndex);     break;
                    case "VertexTable": (Data[dataIndex] as VertexTable).Write(writer, dataIndex); break;
                    case "FaceTable":   (Data[dataIndex] as FaceTable).Write(writer, dataIndex);   break;
                    case "TextureMap":  (Data[dataIndex] as TextureMap).Write(writer);     break;
                    case "Group":       (Data[dataIndex] as Group).Write(writer, dataIndex);       break;
                    case "ObjectMap":   (Data[dataIndex] as ObjectMap).Write(writer);      break;

                    default: Console.WriteLine($"Writing of node type '{Data[dataIndex].GetType().Name}' not yet implemented."); break;
                }
            }

            // Loop through each node for the sub nodes.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the sub nodes based on the parent node's type.
                switch (Data[dataIndex].GetType().Name)
                {
                    case "Group": (Data[dataIndex] as Group).WriteSubNodes(writer, dataIndex); break;
                }
            }

            // Fill in the offset for the root node's table.
            writer.FillOffset("RootNodeOffsetTable");

            // Write all the offsets for the nodes.
            foreach (long offset in Offsets)
                writer.Write(offset);

            // Fill in the offset for the first data chunk.
            writer.FillOffset("FirstDataChunk");

            // Loop through each node for their names.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the node name based on the type.
                switch (Data[dataIndex].GetType().Name)
                {
                    case "Texture": (Data[dataIndex] as Texture).WriteName(writer, dataIndex); break;
                    case "Group":   (Data[dataIndex] as Group).WriteName(writer, dataIndex);   break;
                }
            }

            // Loop through each node for their extra data chunks.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the extra data based on the node type.
                switch (Data[dataIndex].GetType().Name)
                {
                    case "Texture":     (Data[dataIndex] as Texture).WriteData(writer, dataIndex);     break;
                    case "VertexTable": (Data[dataIndex] as VertexTable).WriteData(writer, dataIndex); break;
                    case "FaceTable":   (Data[dataIndex] as FaceTable).WriteData(writer, dataIndex);   break;
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Imports a Assimp compatible model and converts it to a Wayforward Engine mesh.
        /// TODO: This is likely a temporary solution, considering the mesh format itself is yet to be fully reverse engineered.
        /// </summary>
        /// <param name="filepath">The file to import.</param>
        /// <param name="wf3dOut">The .wf3d file to save.</param>
        /// <param name="gpuInject">The .gpu file to inject the vertex and face tables to.</param>
        /// <param name="vertTableHash">The hash to identify the vertex table(s) by.</param>
        /// <param name="faceTableHash">The hash to identify the face table(s) by.</param>
        public void ImportAssimp(string filepath, string wf3dOut, string gpuInject, ulong vertTableHash = 0x2424242400, ulong faceTableHash = 0x242424240000)
        {
            // Store the original hash values so we can use them later.
            ulong origVertTableHash = vertTableHash;
            ulong origFaceTableHash = faceTableHash;

            // Setup AssimpNet Scene.
            AssimpContext assimpImporter = new();
            KeepSceneHierarchyConfig config = new(true);
            assimpImporter.SetConfig(config);
            Scene assimpModel = assimpImporter.ImportFile(filepath, PostProcessSteps.PreTransformVertices | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateBoundingBoxes);

            // Load the .gpu file to inject the custom vertex and face tables into.
            Load(gpuInject);

            // Loop through each mesh to write their vertex tables.
            for (int meshIndex = 0; meshIndex < assimpModel.Meshes.Count; meshIndex++)
            {
                // Create a vertex table entry.
                VertexTable vertTable = new()
                {
                    Hash = vertTableHash,
                    Vertices = new VertexTable.Vertex[assimpModel.Meshes[meshIndex].Vertices.Count]
                };

                // Loop through each vertex and add it to our table.
                for (int vertexIndex = 0; vertexIndex < assimpModel.Meshes[meshIndex].Vertices.Count; vertexIndex++)
                {
                    VertexTable.Vertex vert = new();
                    vert.Position = new(assimpModel.Meshes[meshIndex].Vertices[vertexIndex].X, assimpModel.Meshes[meshIndex].Vertices[vertexIndex].Y, -assimpModel.Meshes[meshIndex].Vertices[vertexIndex].Z);
                    vert.UVCoordinates[0] = assimpModel.Meshes[meshIndex].TextureCoordinateChannels[0][vertexIndex].X;
                    vert.UVCoordinates[1] = assimpModel.Meshes[meshIndex].TextureCoordinateChannels[0][vertexIndex].Y;
                    vertTable.Vertices[vertexIndex] = vert;
                }

                // Add our vertex table to the gpu file.
                Data.Add(vertTable);

                // Increment the vertex table hash for the next vertex table.
                vertTableHash++;
            }

            // Loop through each mesh to write their face tables.
            for (int meshIndex = 0; meshIndex < assimpModel.Meshes.Count; meshIndex++)
            {
                // Create a face table entry.
                FaceTable faceTable = new()
                {
                    Hash = faceTableHash,
                    Faces = new Face[assimpModel.Meshes[meshIndex].Faces.Count]
                };

                // Loop through each face and add it to our table.
                for (int faceInex = 0; faceInex < assimpModel.Meshes[meshIndex].Faces.Count; faceInex++)
                {
                    Face face = new()
                    {
                        IndexA = (uint)assimpModel.Meshes[meshIndex].Faces[faceInex].Indices[0],
                        IndexB = (uint)assimpModel.Meshes[meshIndex].Faces[faceInex].Indices[1],
                        IndexC = (uint)assimpModel.Meshes[meshIndex].Faces[faceInex].Indices[2]
                    };

                    faceTable.Faces[faceInex] = face;
                }

                // Add our face table to the gpu file.
                Data.Add(faceTable);

                // Increment the face table hash for the next face table.
                faceTableHash++;
            }

            // Save our updated GPU file.
            Save(gpuInject);

            // Clear the data list.
            Data = new();

            // Reset the hash values.
            vertTableHash = origVertTableHash;
            faceTableHash = origFaceTableHash;

            // Loop through each material in our model.
            for (int materialIndex = 0; materialIndex < assimpModel.Materials.Count; materialIndex++)
            {
                // Create a texture map entry for this material.
                // TODO: The object hash might end up wrong here.
                // TODO: This texture hash is hardcoded to level_1_1_burning_env.gpu's version of the blockout texture.
                TextureMap textureMap = new()
                {
                    ObjectHash = (ulong)(0x24242424000000 + materialIndex),
                    TextureHash = 0x545FB4B4E41CCA95
                };
                
                // Add this texture map to our file.
                Data.Add(textureMap);
            }

            // Create a group for this mesh.
            Group group = new()
            {
                Hash = 0x2424242400000000,
                UnknownHash = 0x166E6DE3,
                Matrix = new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1),
                Name = Path.GetFileNameWithoutExtension(wf3dOut)
            };

            // Loop through each mesh.
            for (int meshIndex = 0; meshIndex < assimpModel.Meshes.Count; meshIndex++)
            {
                // Create an object map for this mesh.
                ObjectMap objectMap = new()
                {
                    UnknownHash = 0x7608AF45F703132C,
                    Hash = (ulong)(0x24242424000000 + meshIndex),
                    VertexHash = vertTableHash,
                    FaceHash = faceTableHash
                };
                objectMap.AABB[0] = new(assimpModel.Meshes[meshIndex].BoundingBox.Min.X, assimpModel.Meshes[meshIndex].BoundingBox.Min.Y, assimpModel.Meshes[meshIndex].BoundingBox.Min.Z);
                objectMap.AABB[1] = new(assimpModel.Meshes[meshIndex].BoundingBox.Max.X, assimpModel.Meshes[meshIndex].BoundingBox.Max.Y, assimpModel.Meshes[meshIndex].BoundingBox.Max.Z);

                // Increment the vertex table and face table hashes for the next mesh.
                vertTableHash++;
                faceTableHash++;

                // Add this object map as a sub node to the group.
                group.SubNodes.Add(objectMap);
            }

            // Save the group.
            Data.Add(group);

            // Save our new mesh file.
            Save(wf3dOut);
        }

        /// <summary>
        /// Exports this mesh to an OBJ.
        /// TODO: OBJ won't be suitable for this long term.
        /// </summary>
        /// <param name="filepath">The directory to export to.</param>
        public void ExportOBJTemp(string filepath, string? gpuFile = null)
        {
            // Set up a GPU file.
            Mesh gpu = new();
            
            // If the user has passed in a GPU file, then load it.
            if (gpuFile != null)
                gpu = new(gpuFile);

            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 0;

            // Create the StreamWriters.
            StreamWriter obj = new(filepath);
            StreamWriter mtl = new(Path.ChangeExtension(filepath, ".mtl"));

            obj.WriteLine($"mtllib {Path.GetFileNameWithoutExtension(filepath)}.mtl");

            // Create a list of all the object maps in the .wf3d file we need to convert.
            List<ObjectMap> ObjectMaps = new();

            // Loop through each entry in the .wf3d file.
            foreach (object entry in Data)
            {
                // If this entry is an object map, then add it to the list.
                if (entry is ObjectMap map)
                    ObjectMaps.Add(map);

                // If this entry is a group, then check its sub nodes for any object maps and add them to the list.
                if (entry is Group group)
                    foreach (object subnode in group.SubNodes)
                        if (subnode is ObjectMap subnodeMap)
                            ObjectMaps.Add(subnodeMap);
            }

            // Loop through each object map we need to convert.
            foreach (ObjectMap objectMap in ObjectMaps)
            {
                // Set up references to the chunks we need.
                VertexTable? vertexTable = null;
                FaceTable? faceTable = null;
                TextureMap? textureMap = null;
                Texture? texture = null;

                // Loop through each entry in this .wf3d file.
                foreach (object entry in Data)
                {
                    // If this entry is a vertex table with the correct hash, then set it as the one to use.
                    if (entry is VertexTable vTable)
                        if (vTable.Hash == objectMap.VertexHash)
                            vertexTable = vTable;

                    // If this entry is a face table with the correct hash, then set it as the one to use.
                    if (entry is FaceTable fTable)
                        if (fTable.Hash == objectMap.FaceHash)
                            faceTable = fTable;

                    // If this entry is a texture map that targets this object's hash, then set it as the one to use.
                    if (entry is TextureMap tMap)
                        if (tMap.ObjectHash == objectMap.Hash)
                            textureMap = tMap;
                }

                // If a .gpu file has been provided, then loop through each entry in it too.
                if (gpuFile != null)
                {
                    foreach (object entry in gpu.Data)
                    {
                        // If this entry is a vertex table with the correct hash, then set it as the one to use.
                        if (entry is VertexTable vTable)
                            if (vTable.Hash == objectMap.VertexHash)
                                vertexTable = vTable;

                        // If this entry is a face table with the correct hash, then set it as the one to use.
                        if (entry is FaceTable fTable)
                            if (fTable.Hash == objectMap.FaceHash)
                                faceTable = fTable;
                    }
                }

                // Error out if we haven't found the vertex or face table.
                if (vertexTable == null)
                    throw new Exception("No Vertex Table was found for this Object Map. Try specifying a .gpu file to search in as well.");
                if (faceTable == null)
                    throw new Exception("No Face Table was found for this Object Map. Try specifying a .gpu file to search in as well.");

                // If a texture map chunk for this object map has been found, then search for a texture.
                if (textureMap != null)
                {
                    // Loop through each entry in this .wf3d file.
                    foreach (object entry in Data)
                        if (entry is Texture tex)
                            if (tex.Hash == textureMap.TextureHash)
                                texture = tex;

                    // Loop through each entry in the .gpu file if one was provided.
                    if (gpuFile != null)
                        foreach (object entry in gpu.Data)
                            if (entry is Texture tex)
                                if (tex.Hash == textureMap.TextureHash)
                                    texture = tex;

                    // If we found a texture with the right hash, then write a reference to it in the mtl.
                    if (texture != null)
                    {
                        mtl.WriteLine($"newmtl {Path.GetFileNameWithoutExtension(texture.Name)}");
                        mtl.WriteLine($"\tmap_Kd {texture.Name}");
                    }
                    // If not, then just write the hash with no diffuse map.
                    else
                    {
                        mtl.WriteLine($"newmtl 0x{textureMap.TextureHash.ToString("X").PadLeft(16, '0')}");
                    }
                }

                // Write the Vertex Comment for this model.
                obj.WriteLine($"# ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')} Vertices\r\n");

                // Write each vertex for this model, flipping the Z Axis coordinate.
                foreach (VertexTable.Vertex vertex in vertexTable.Vertices)
                    obj.WriteLine($"v {vertex.Position.X} {vertex.Position.Y} {-vertex.Position.Z}");

                // Write the UV Coordinates Comment for this model.
                obj.WriteLine($"# ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')} UV Coordinates\r\n");

                // Write each UV Coordinate for this model.
                foreach (VertexTable.Vertex vertex in vertexTable.Vertices)
                    obj.WriteLine($"vt {vertex.UVCoordinates[0]} {vertex.UVCoordinates[1]}");

                // Write the Object Name Comment for this model.
                obj.WriteLine($"\r\n# ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')} Name\r\n");

                // Write the name strings for this model.
                obj.WriteLine($"g {Path.GetFileNameWithoutExtension(filepath)} ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')}");
                obj.WriteLine($"o {Path.GetFileNameWithoutExtension(filepath)} ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')}");

                // If a texture map chunk for this object map has been found, then write the mtl reference based on if we found a texture.
                if (textureMap != null)
                {
                    if (texture != null)
                        obj.WriteLine($"usemtl {Path.GetFileNameWithoutExtension(texture.Name)}");
                    else
                        obj.WriteLine($"usemtl 0x{textureMap.TextureHash.ToString("X").PadLeft(16, '0')}");
                }

                // Write the Faces Comment for this model.
                obj.WriteLine($"\r\n# ObjectMap 0x{objectMap.Hash.ToString("X").PadLeft(16, '0')} Faces\r\n");

                // Write each face for this model, with the indices incremented by 1 (and the current value of vertexCount) due to OBJ counting from 1 not 0.
                foreach (Face face in faceTable.Faces)
                    obj.WriteLine($"f {face.IndexA + 1 + vertexCount}/{face.IndexA + 1 + vertexCount} {face.IndexB + 1 + vertexCount}/{face.IndexB + 1 + vertexCount} {face.IndexC + 1 + vertexCount}/{face.IndexC + 1 + vertexCount}");

                // Add the amount of vertices in this model to the count.
                vertexCount += vertexTable.Vertices.Length;

                // Write an empty line for neatness.
                obj.WriteLine();
            }

            // Close the StreamWriters.
            obj.Close();
            mtl.Close();
        }
    }
}
