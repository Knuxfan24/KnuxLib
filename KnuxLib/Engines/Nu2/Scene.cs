using KnuxLib.Engines.Nu2.ObjectChunks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace KnuxLib.Engines.Nu2
{
    // TODO: Finish reverse engineering the differences in the PS2 and Xbox versions.
    // TODO: Write a way to save this format.
    // TODO: Write a way to import models to this format.
    // TODO: Implement a way to decompress/recompress the PS2 version's RnC compression (https://segaretro.org/Rob_Northen_compression).
    public class Scene : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Scene() { }
        public Scene(string filepath, FormatVersion version = FormatVersion.GameCube, bool export = false)
        {
            Load(filepath, version);

            if (export)
            {
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.nu2.scene.json", Data);
                Export($@"{Path.GetDirectoryName(filepath)}\export");
            }
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            Xbox = 1,
            PlayStation2 = 2
        }

        public class FormatData
        {
            /// <summary>
            /// This scene's name table.
            /// </summary>
            public List<string>? NameTable { get; set; }

            /// <summary>
            /// This scene's texture data.
            /// </summary>
            public List<TextureSet.TextureData>? Textures { get; set; }

            /// <summary>
            /// This scene's materials.
            /// </summary>
            public List<MaterialSet.MaterialData>? Materials { get; set; }

            /// <summary>
            /// This scene's geometry data.
            /// </summary>
            public List<GeometrySet.GeometryData>? Geometry { get; set; }

            /// <summary>
            /// This scene's instances.
            /// </summary>
            public InstanceSet.InstanceData? Instances { get; set; }

            /// <summary>
            /// This scene's SPEC data.
            /// TODO: What does SPEC stand for?
            /// </summary>
            public List<SPECSet.SPECData>? SPEC { get; set; }

            /// <summary>
            /// This scene's splines.
            /// </summary>
            public List<SplineSet.SplineData>? Splines { get; set; }

            /// <summary>
            /// This scene's LDIR data.
            /// TODO: What does LDIR stand for?
            /// TODO: Why is this always just a large chunk of empty data?
            /// </summary>
            public byte[]? LDIR { get; set; }

            /// <summary>
            /// This scene's texture animations.
            /// </summary>
            public TextureAnimationSet.TextureAnimationData? TextureAnimations { get; set; }

            /// <summary>
            /// The value of this scene's SPHE chunk, if it has one.
            /// TODO: What does SPHE stand for?
            /// TODO: What does this chunk do, if anything?
            /// </summary>
            public uint? SPHEValue { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature and some other initial data based on the format version.
            switch (version)
            {
                case FormatVersion.GameCube:
                    // Switch the reader to Big Endian if this is a GameCube file.
                    reader.IsBigEndian = true;
                    
                    // Check for the GSC0 signature (stored as 0CSG due to endianness).
                    reader.ReadSignature(0x04, "0CSG");

                    // Read this file's filesize.
                    uint fileSize = reader.ReadUInt32();
                    break;

                case FormatVersion.Xbox:
                    // Check for the NUX0 signature.
                    reader.ReadSignature(0x04, "NUX0");

                    // Read an unknown value.
                    uint unknownXboxValue = reader.ReadUInt32();
                    break;

                case FormatVersion.PlayStation2:
                    // Check for the NU20 signature.
                    reader.ReadSignature(0x04, "NU20");

                    // Read an unknown value.
                    uint unknownPS2Value = reader.ReadUInt32();

                    // Skip two unknown values of 0x06 and 0x00.
                    reader.JumpAhead(0x08);
                    break;
            }

            // Loop through chunks until we hit the end of the file.
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Store this chunk's position.
                long chunkStartPosition = reader.BaseStream.Position;

                // Read this chunk's type.
                string chunkType = reader.ReadNullPaddedString(0x04);

                // If this is a GameCube file, then flip chunkType.
                if (version == FormatVersion.GameCube)
                    chunkType = new string(chunkType.Reverse().ToArray());

                // Read this chunk's size.
                uint chunkSize = reader.ReadUInt32();

                // Process this chunk depending on the type.
                switch (chunkType)
                {
                    case "NTBL": Data.NameTable = NameTable.Read(reader, version); break;
                    case "TST0": Data.Textures = TextureSet.Read(reader, version); break;
                    case "MS00": Data.Materials = MaterialSet.Read(reader, version); break;
                    case "GST0": Data.Geometry = GeometrySet.Read(reader, version); break;
                    case "INST": Data.Instances = InstanceSet.Read(reader); break;
                    case "SPEC": Data.SPEC = SPECSet.Read(reader, version); break;
                    case "SST0": Data.Splines = SplineSet.Read(reader, version); break;
                    case "LDIR": Data.LDIR = reader.ReadBytes((int)chunkSize - 0x08); break;
                    case "TAS0": Data.TextureAnimations = TextureAnimationSet.Read(reader, version); break;
                    case "SPHE": reader.JumpBehind(0x04); Data.SPHEValue = reader.ReadUInt32(); break;

                    // Skip this chunk if we don't yet support it.
                    default:
                        Console.WriteLine($"'{chunkType}' at '0x{(reader.BaseStream.Position - 0x08).ToString("X").PadLeft(8, '0')}' with a size of '0x{chunkSize.ToString("X").PadLeft(8, '0')}' not yet handled.");
                        break;
                }

                // Jump to the end of this chunk by caluclating where it started and its size.
                reader.JumpTo(chunkStartPosition + chunkSize);
            
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Exports data from this scene file using the functions below.
        /// </summary>
        /// <param name="directory">The directory to extract data to.</param>
        public void Export(string directory)
        {
            // Create the target directory.
            Directory.CreateDirectory(directory);

            // Export the scene's geometry.
            ExportSceneOBJ(directory);

            // Export the scene's materials.
            ExportSceneMTL(directory);

            // Write a MaxScript (sorry Blender users I'm a hypocrite) to do all the instancing.
            ExportMaxScript(directory);

            // Export the textures, using code from LibTWOC.
            ExportTextures(directory);

            // Export the splines.
            ExportSplines(directory);
        }
    
        private void ExportSceneOBJ(string directory)
        {
            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 0;

            // Create a StreamWriter for this scene.
            StreamWriter obj = new($@"{directory}\scene.obj");

            // Write a reference to the obj material file.
            obj.WriteLine($"mtllib scene.mtl\r\n");

            // Loop through each geometry node in this scene.
            for (int geometryIndex = 0; geometryIndex < Data.Geometry.Count; geometryIndex++)
            {
                // Ignore this mesh if it's a plane.
                // TODO: Handle these in SOME way.
                if (Data.Geometry[geometryIndex].Type == GeometrySet.GeometryType.Plane)
                    continue;

                // Loop through each mesh in this geometry node.
                for (int meshIndex = 0; meshIndex < Data.Geometry[geometryIndex].Meshes.Count; meshIndex++)
                {
                    // Write this mesh's vertices.
                    foreach (var vertex in Data.Geometry[geometryIndex].Meshes[meshIndex].Vertices)
                        obj.WriteLine($"v {vertex.Position.X:F8} {vertex.Position.Y:F8} {vertex.Position.Z:F8}");

                    // Write this mesh's texture corrdinates.
                    foreach (var vertex in Data.Geometry[geometryIndex].Meshes[meshIndex].Vertices)
                        obj.WriteLine($"vt {-vertex.TextureCoordinates.X:F8} {-vertex.TextureCoordinates.Y:F8}");

                    // Write this mesh's normals.
                    foreach (var vertex in Data.Geometry[geometryIndex].Meshes[meshIndex].Vertices)
                        obj.WriteLine($"vn {vertex.Normals.Value.X:F8} {vertex.Normals.Value.Y:F8} {vertex.Normals.Value.Z:F8}");

                    // Write this mesh's vertex colours.
                    // TODO: There's no official support for this, though the VColorOBJ MaxScript in HeroesPowerPlant can use them.
                    foreach (var vertex in Data.Geometry[geometryIndex].Meshes[meshIndex].Vertices)
                        obj.WriteLine($"vc {vertex.Colour.Red} {vertex.Colour.Green} {vertex.Colour.Blue} {vertex.Colour.Alpha}");         
                }

                // Write this object's name (just geometry followed by its index).
                obj.WriteLine($"o geometry{geometryIndex}");
                obj.WriteLine($"g geometry{geometryIndex}");

                // Loop through each mesh in this geometry set.
                for (int meshIndex = 0; meshIndex < Data.Geometry[geometryIndex].Meshes.Count; meshIndex++)
                {
                    // Write this mesh's material reference.
                    obj.WriteLine($"usemtl Material{Data.Geometry[geometryIndex].Meshes[meshIndex].MaterialIndex}");

                    // Determine whether to write as a triangle list or a triangle strip.
                    if (Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleList != null)
                    {
                        // Loop through the triangle list for this mesh.
                        for (int triListIndex = 0; triListIndex < Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleList.Count - 2; triListIndex += 3)
                        {
                            // Calculate the index of each vertex so we don't repeat these three awful lines three times each.
                            int face1 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleList[triListIndex + 0] + 1 + vertexCount;
                            int face2 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleList[triListIndex + 1] + 1 + vertexCount;
                            int face3 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleList[triListIndex + 2] + 1 + vertexCount;

                            // Write this face.
                            obj.WriteLine($"f {face1}/{face1}/{face1} {face2}/{face2}/{face2} {face3}/{face3}/{face3}");
                        }
                    }
                    else
                    {
                        // Loop through each triangle strip in this mesh.
                        for (int triStripIndex = 0; triStripIndex < Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleStrips.Count; triStripIndex++)
                        {
                            // Loop through this triangle strip.
                            for (int triStripVertIndex = 0; triStripVertIndex < Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleStrips[triStripIndex].Count - 2; triStripVertIndex++)
                            {
                                // Calculate the index of each vertex so we don't repeat these three awful lines three times each.
                                int faceAdd0 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleStrips[triStripIndex][triStripVertIndex + 0] + 1 + vertexCount;
                                int faceAdd1 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleStrips[triStripIndex][triStripVertIndex + 1] + 1 + vertexCount;
                                int faceAdd2 = Data.Geometry[geometryIndex].Meshes[meshIndex].PrimitiveTriangleStrips[triStripIndex][triStripVertIndex + 2] + 1 + vertexCount;

                                // Write this face depending on where in the triangle strip we are.
                                if ((triStripVertIndex & 1) == 0)
                                    obj.WriteLine($"f {faceAdd0}/{faceAdd0}/{faceAdd0} {faceAdd1}/{faceAdd1}/{faceAdd1} {faceAdd2}/{faceAdd2}/{faceAdd2}");
                                else
                                    obj.WriteLine($"f {faceAdd1}/{faceAdd1}/{faceAdd1} {faceAdd0}/{faceAdd0}/{faceAdd0} {faceAdd2}/{faceAdd2}/{faceAdd2}");
                            }
                        }
                    }

                    // Increment vertexCount.
                    vertexCount += Data.Geometry[geometryIndex].Meshes[meshIndex].Vertices.Count;

                    // Add a line break for neatness.
                    obj.WriteLine();
                }
            }

            // Close the StreamWriter.
            obj.Close();
        }
    
        private void ExportSceneMTL(string directory)
        {
            // Create a StreamWriter for this material file.
            StreamWriter mtl = new($@"{directory}\scene.mtl");

            // Loop through each material in this scene.
            for (int materialIndex = 0; materialIndex < Data.Materials.Count; materialIndex++)
            {
                // Write a material entry for this material.
                mtl.WriteLine($"newmtl Material{materialIndex}");

                // Write this material's diffuse colours.
                mtl.WriteLine($"\tKd {Data.Materials[materialIndex].Colour.X} {Data.Materials[materialIndex].Colour.Y} {Data.Materials[materialIndex].Colour.Z}");

                // If this material has a texture, then write a reference to the bitmap with the approriate index.
                if (Data.Materials[materialIndex].TextureIndex != -1)
                    mtl.WriteLine($"\tmap_Kd bitmap{Data.Materials[materialIndex].TextureIndex}.png");

                // Add a line break for neatness.
                mtl.WriteLine();
            }

            // Close the StreamWriter.
            mtl.Close();
        }

        private void ExportMaxScript(string directory)
        {
            // Create a StreamWriter for the instancer MaxScript.
            StreamWriter maxscript = new($@"{directory}\instance_fix.ms");

            // Loop through each instance.
            for (int instanceIndex = 0; instanceIndex < Data.Instances.Instances.Count; instanceIndex++)
            {
                // Set up a spec set.
                SPECSet.SPECData? spec = null;

                // Loop through each spec set in this scene to try and find one that references this instance index.
                if (Data.SPEC != null)
                    foreach (SPECSet.SPECData specSet in Data.SPEC)
                        if (specSet.InstanceIndex == instanceIndex)
                            spec = specSet;

                // Decompose this instance's matrix so we can get the position, rotation and scale.
                Matrix4x4.Decompose(Data.Instances.Instances[instanceIndex].Matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

                // Write the start of a try catch block, as some geometry entries end up with no data due to me not currently writing planes, so they don't end up existing.
                maxscript.WriteLine("try (");

                // Get a refernce to the geometry this instance uses.
                maxscript.WriteLine($"\ttargetGeometry = getNodeByName \"geometry{Data.Instances.Instances[instanceIndex].GeometryIndex}\"");

                // Select the geometry this instance uses.
                maxscript.WriteLine($"\tselect targetGeometry");

                // Clone the geometry this instance uses.
                maxscript.WriteLine($"\tmaxOps.cloneNodes (selection as array) cloneType:#instance newNodes:&nnl #nodialog");

                // If there isn't a spec set for this instance, then rename the clone to "instance", folloewd by its index.
                if (spec == null)
                    maxscript.WriteLine($"\tnnl[1].name = \"instance{instanceIndex}\"");
                // Else, rename the clone to the name given in the spec set.
                else
                    maxscript.WriteLine($"\tnnl[1].name = \"{spec.Name}\"");

                // Set this node's rotation, factoring in Z-Up.
                maxscript.WriteLine($"\tnnl[1].rotation = quat {rotation.X} {-rotation.Z} {rotation.Y} {rotation.W}");

                // Set this node's position, factoring in Z-Up.
                maxscript.WriteLine($"\tnnl[1].position = point3 {translation.X} {-translation.Z} {translation.Y}");

                // Set this node's scale.
                maxscript.WriteLine($"\tnnl[1].scale = point3 {scale.X} {scale.Z} {scale.Y}");

                // Write the end of the try catch block.
                maxscript.WriteLine(") catch (\r\n)\r\n");
            }

            // Loop through the original geometry so we can delete it.
            for (int geometryIndex = 0; geometryIndex < Data.Geometry.Count; geometryIndex++)
            {
                // Write the start of a try catch block, as some geometry entries end up with no data due to me not currently writing planes, so they don't end up existing.
                maxscript.WriteLine("try (");

                // Get a refernce to this geometry index.
                maxscript.WriteLine($"\ttargetGeometry = getNodeByName \"geometry{geometryIndex}\"");

                // Delete this geometry index.
                maxscript.WriteLine($"\tdelete targetGeometry");

                // Write the end of the try catch block.
                maxscript.WriteLine(") catch (\r\n)\r\n");
            }

            // Close the StreamWriter.
            maxscript.Close();
        }

        #region Texture stuff taken from LibTWOC
        private void ExportTextures(string directory)
        {
            for (int i = 0; i < Data.Textures.Count; i++)
            {
                var image = new Image<Byte4>((int)Data.Textures[i].Width, (int)Data.Textures[i].Height);
                switch (Data.Textures[i].Type)
                {
                    case 0x80:
                        int index = 0;
                        for (int y = 0; y < Data.Textures[i].Height; y += 8)
                        {
                            for (var x = 0; x < Data.Textures[i].Width; x += 8)
                            {
                                DecodeDXTBlock(ref image, Data.Textures[i].Data, index, x, y);
                                index += 8;
                                DecodeDXTBlock(ref image, Data.Textures[i].Data, index, x + 4, y);
                                index += 8;
                                DecodeDXTBlock(ref image, Data.Textures[i].Data, index, x, y + 4);
                                index += 8;
                                DecodeDXTBlock(ref image, Data.Textures[i].Data, index, x + 4, y + 4);
                                index += 8;
                            }
                        }
                        break;

                    case 0x81:
                        index = 0;
                        for (var y = 0; y < Data.Textures[i].Height; y += 4)
                        {
                            for (var x = 0; x < Data.Textures[i].Width; x += 4)
                            {
                                for (var by = 0; by < 4; by++)
                                {
                                    for (var bx = 0; bx < 4; bx++)
                                    {
                                        DecodeRGB5A3Block(ref image, Data.Textures[i].Data, index, x + bx, y + by);
                                        index += 2;
                                    }
                                }
                            }
                        }
                        break;

                    case 0x82: // This is incorrect but I just wanted something to render
                        index = 0;
                        for (var y = 0; y < Data.Textures[i].Height; y += 4)
                        {
                            for (var x = 0; x < Data.Textures[i].Width; x += 4)
                            {
                                for (var by = 0; by < 4; by++)
                                {
                                    for (var bx = 0; bx < 4; bx++)
                                    {
                                        DecodeRGB5A3Block(ref image, Data.Textures[i].Data, index, x + bx, y + by);
                                        index += 2;
                                    }
                                }
                            }
                        }
                        break;
                }
                image.SaveAsPng($@"{directory}\bitmap{i}.png");
            }
        }

        private static ushort ArrayReadU16(byte[] array, long offset, bool bigEndian = true)
        {
            if (bigEndian)
                return (ushort)(array[offset + 1] | (array[offset] << 8));
            return (ushort)(array[offset] | (array[offset + 1] << 8));
        }

        private static byte Convert3To8(byte v) => (byte)((v << 5) | (v << 2) | (v >> 1));

        private static byte Convert4To8(byte v) => (byte)((v << 4) | v);

        private static byte Convert5To8(byte v) => (byte)((v << 3) | (v >> 2));

        private static byte Convert6To8(byte v) => (byte)((v << 2) | (v >> 4));

        private static int DXTBlend(int v1, int v2) => ((v1 * 3 + v2 * 5) >> 3);

        private static void DecodeDXTBlock(ref Image<Byte4> dst, byte[] src, int srcOffset, int blockX, int blockY)
        {
            if (srcOffset >= src.Length) return;
            var c1 = ArrayReadU16(src, srcOffset, true);
            var c2 = ArrayReadU16(src, srcOffset + 2, true);
            var lines = new byte[4] { src[srcOffset + 4], src[srcOffset + 5], src[srcOffset + 6], src[srcOffset + 7] };

            byte blue1 = Convert5To8((byte)(c1 & 0x1F));
            byte blue2 = Convert5To8((byte)(c2 & 0x1F));
            byte green1 = Convert6To8((byte)((c1 >> 5) & 0x3F));
            byte green2 = Convert6To8((byte)((c2 >> 5) & 0x3F));
            byte red1 = Convert5To8((byte)((c1 >> 11) & 0x1F));
            byte red2 = Convert5To8((byte)((c2 >> 11) & 0x1F));

            Byte4[] colors = new Byte4[4];
            colors[0] = new Byte4(red1, green1, blue1, 255);
            colors[1] = new Byte4(red2, green2, blue2, 255);
            if (c1 > c2)
            {
                colors[2] = new Byte4((byte)DXTBlend(red2, red1), (byte)DXTBlend(green2, green1), (byte)DXTBlend(blue2, blue1), 255);
                colors[3] = new Byte4((byte)DXTBlend(red1, red2), (byte)DXTBlend(green1, green2), (byte)DXTBlend(blue1, blue2), 255);
            }
            else
            {
                // color[3] is the same as color[2] (average of both colors), but transparent.
                // This differs from DXT1 where color[3] is transparent black.
                colors[2] = new Byte4((byte)((red1 + red2) / 2), (byte)((green1 + green2) / 2), (byte)((blue1 + blue2) / 2), 255);
                colors[3] = new Byte4((byte)((red1 + red2) / 2), (byte)((green1 + green2) / 2), (byte)((blue1 + blue2) / 2), 0);
            }

            for (int y = 0; y < 4; y++)
            {
                int val = lines[y];
                for (int x = 0; x < 4; x++)
                {
                    dst[x + blockX, y + blockY] = colors[(val >> 6) & 3];
                    val <<= 2;
                }
            }
        }

        private static void DecodeRGB5A3Block(ref Image<Byte4> dst, byte[] src, int srcOffset, int x, int y)
        {
            if (srcOffset >= src.Length) return;
            var c = ArrayReadU16(src, srcOffset, true);

            byte r, g, b, a;
            if ((c & 0x8000) != 0)
            {
                r = Convert5To8((byte)((c >> 10) & 0x1F));
                g = Convert5To8((byte)((c >> 5) & 0x1F));
                b = Convert5To8((byte)((c) & 0x1F));
                a = 0xFF;
            }
            else
            {
                a = Convert3To8((byte)((c >> 12) & 0x7));
                b = Convert4To8((byte)((c >> 8) & 0xF));
                g = Convert4To8((byte)((c >> 4) & 0xF));
                r = Convert4To8((byte)((c) & 0xF));
            }

            dst[x, y] = new Byte4(r, g, b, a);
        }
        #endregion
    
        private void ExportSplines(string directory)
        {
            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 0;

            // Create a StreamWriter for this scene's splines.
            StreamWriter splines = new($@"{directory}\splines.obj");

            // Loop through each spline in this scene.
            for (int splineIndex = 0; splineIndex < Data.Splines.Count; splineIndex++)
            {
                // Write this splines's vertices.
                foreach (Vector3 vertex in Data.Splines[splineIndex].Vertices)
                    splines.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write this spline's name.
                splines.WriteLine($"o {Data.Splines[splineIndex].Name}");
                splines.WriteLine($"g {Data.Splines[splineIndex].Name}");

                // Write this path's object.
                splines.Write("l ");
                for (int pointIndex = 0; pointIndex < Data.Splines[splineIndex].Vertices.Count; pointIndex++)
                    splines.Write($"{pointIndex + 1 + vertexCount} ");

                // Write two line breaks, one to end the object and another for neatness.
                splines.WriteLine("\r\n");

                // Increment vertexCount.
                vertexCount += Data.Splines[splineIndex].Vertices.Count;
            }

            // Close the StreamWriter.
            splines.Close();
        }
    }
}
