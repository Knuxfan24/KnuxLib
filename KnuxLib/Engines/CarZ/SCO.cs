using Assimp.Configs;
using Assimp;

namespace KnuxLib.Engines.CarZ
{
    // TODO: What does SCO mean?
    public class SCO : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SCO() { }
        public SCO(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The name of this model, might always match the file name?
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The origin point of this model?
            /// </summary>
            public Vector3 CentralPoint { get; set; }

            /// <summary>
            /// The coordinates for the various vertices that make up this model.
            /// </summary>
            public List<Vector3> Vertices { get; set; } = new();

            /// <summary>
            /// The faces that make up this model.
            /// </summary>
            public List<Face> Faces { get; set; } = new();

            public override string ToString() => Name;
        }

        public class Face
        {
            /// <summary>
            /// The three vertex indices that make up this face.
            /// </summary>
            public short[] VertexIndices { get; set; } = new short[3];

            /// <summary>
            /// The name of the material in the approriate material library to assign to this face.
            /// </summary>
            public string MaterialName { get; set; } = "";

            /// <summary>
            /// The texture coordinates for this model.
            /// </summary>
            public double[] TextureCoordinates { get; set; } = new double[6];

            public override string ToString() => $"<{VertexIndices[0]}, {VertexIndices[1]}, {VertexIndices[2]}>";
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Load the SCO into a string array.
            string[] sco = File.ReadAllLines(filepath);

            // Check that the start of the SCO is what we're expecting.
            if (sco[0] != "[ObjectBegin]")
                throw new Exception($"'{filepath}' does not appear to be a CarZ engine SCO file.");

            // Read this SCO's model name.
            Data.Name = sco[1][(sco[1].IndexOf(' ') + 1)..];

            // Get this SCO's central point.
            Data.CentralPoint = new(float.Parse(sco[2].Split(' ')[1]), float.Parse(sco[2].Split(' ')[2]), float.Parse(sco[2].Split(' ')[3]));

            // Figure out which line the faces start on.
            int FacePosition = 4 + int.Parse(sco[3][(sco[3].IndexOf(' ') + 1)..]);

            // Read this SCO's vertex coordinates.
            for (int i = 4; i < FacePosition; i++)
                Data.Vertices.Add(new Vector3(float.Parse(sco[i].Split(' ')[0]), float.Parse(sco[i].Split(' ')[1]), float.Parse(sco[i].Split(' ')[2])));

            // Read this SCO's faces.
            for (int i = FacePosition + 1; i < FacePosition + 1 + int.Parse(sco[FacePosition][(sco[FacePosition].IndexOf(' ') + 1)..]); i++)
            {
                // Split this face's line based on tab and space characters.
                char[] delimiters = { '\t', ' ' };
                string[] line = sco[i].Split(delimiters).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                // Create the face.
                Face face = new();
                face.VertexIndices[0] = short.Parse(line[1]);
                face.VertexIndices[1] = short.Parse(line[2]);
                face.VertexIndices[2] = short.Parse(line[3]);
                face.MaterialName = line[4];
                face.TextureCoordinates[0] = double.Parse(line[5]);
                face.TextureCoordinates[1] = double.Parse(line[6]);
                face.TextureCoordinates[2] = double.Parse(line[7]);
                face.TextureCoordinates[3] = double.Parse(line[8]);
                face.TextureCoordinates[4] = double.Parse(line[9]);
                face.TextureCoordinates[5] = double.Parse(line[10]);

                // Save the face.
                Data.Faces.Add(face);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter sco = new(filepath);

            // Write the [ObjectBegin] header.
            sco.WriteLine("[ObjectBegin]");

            // Write this model's name.
            sco.WriteLine($"Name= {Data.Name}");

            // Write this model's central point.
            sco.WriteLine($"CentralPoint= {Data.CentralPoint.X:n4} {Data.CentralPoint.Y:n4} {Data.CentralPoint.Z:n4}");

            // Write this model's vertex count.
            sco.WriteLine($"Verts= {Data.Vertices.Count}");

            // Write this model's vertices.
            foreach (Vector3 vertex in Data.Vertices)
                sco.WriteLine($"{vertex.X:n4} {vertex.Y:n4} {vertex.Z:n4}");

            // Write this model's face count.
            sco.WriteLine($"Faces= {Data.Faces.Count}");

            // Write this model's faces, preserving the original tab and padding spacing.
            foreach (Face face in Data.Faces)
                sco.WriteLine($"3\t" +
                              $"{face.VertexIndices[0],4} " +
                              $"{face.VertexIndices[1],4} " +
                              $"{face.VertexIndices[2],4}\t" +
                              $"{face.MaterialName,-20}\t" +
                              $"{face.TextureCoordinates[0]:n12} " +
                              $"{face.TextureCoordinates[1]:n12} " +
                              $"{face.TextureCoordinates[2]:n12} " +
                              $"{face.TextureCoordinates[3]:n12} " +
                              $"{face.TextureCoordinates[4]:n12} " +
                              $"{face.TextureCoordinates[5]:n12}");

            // Write the [ObjectEnd] footer and a spare empty line.
            sco.WriteLine("[ObjectEnd]\n");

            // Close this StreamWriter.
            sco.Close();
        }

        /// <summary>
        /// Exports this SCO model to a standard Wavefront OBJ.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        /// <param name="matFileName">The name of the mtl file this OBJ should use.</param>
        public void ExportOBJ(string filepath, string? matFileName = null)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Write the reference to the mtl file if one is specified.
            if (matFileName != null)
            {
                obj.WriteLine($"mtllib {Path.GetFileNameWithoutExtension(matFileName)}.mtl");
            }
            // If an mtl wasn't specified, then look in the current directory for a mat file.
            else
            {
                // Look for mat files in this directory.
                string[] matFiles = Directory.GetFiles(Path.GetDirectoryName(filepath), "*.mat");

                // If any were found, write a reference to the first.
                if (matFiles.Length != 0)
                    obj.WriteLine($"mtllib {Path.GetFileNameWithoutExtension(matFiles[0])}.mtl");
            }

            // Write this model's vertices.
            foreach (Vector3 vertex in Data.Vertices)
                obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

            // Write the texture coordinates for this model, inverting both values.
            for (int i = 0; i < Data.Faces.Count; i++)
            {
                obj.WriteLine($"vt {-Data.Faces[i].TextureCoordinates[0]} {-Data.Faces[i].TextureCoordinates[1]}");
                obj.WriteLine($"vt {-Data.Faces[i].TextureCoordinates[2]} {-Data.Faces[i].TextureCoordinates[3]}");
                obj.WriteLine($"vt {-Data.Faces[i].TextureCoordinates[4]} {-Data.Faces[i].TextureCoordinates[5]}");
            }

            // Set up control stuff for materials and texture coordinates.
            int currentTextureCoordinate = 1;
            string currentMaterial = "";

            // Write this model's name.
            obj.WriteLine($"o {Data.Name}");
            obj.WriteLine($"g {Data.Name}");

            // Write this model's faces.
            for (int i = 0; i < Data.Faces.Count; i++)
            {
                // If this face's material is not the one stored in the currentMaterial string, then write the material reference and update the string.
                if (currentMaterial != Data.Faces[i].MaterialName)
                {
                    obj.WriteLine($"usemtl {Data.Faces[i].MaterialName}");
                    currentMaterial = Data.Faces[i].MaterialName;
                }

                // Write this face's vertex indices plus 1 (as OBJ counts from 1 not 0) and texture coordinates.
                obj.WriteLine($"f {Data.Faces[i].VertexIndices[0] + 1}/{currentTextureCoordinate} {Data.Faces[i].VertexIndices[1] + 1}/{currentTextureCoordinate + 1} {Data.Faces[i].VertexIndices[2] + 1}/{currentTextureCoordinate + 2}");
                
                // Add 3 to the currentTextureCoordinates integer.
                currentTextureCoordinate += 3;
            }

            // Close this StreamWriter.
            obj.Close();
        }

        /// <summary>
        /// Imports an Assimp compatible model and converts it to an SCO model.
        /// TODO: Seems to get texture coordinates wrong.
        /// </summary>
        /// <param name="filepath">The filepath of the model to import.</param>
        public void ImportAssimp(string filepath)
        {
            // Setup AssimpNet Scene.
            AssimpContext assimpImporter = new();
            KeepSceneHierarchyConfig config = new(true);
            assimpImporter.SetConfig(config);
            Scene assimpModel = assimpImporter.ImportFile(filepath, PostProcessSteps.PreTransformVertices);

            // Set the model name based on the file we're importing.
            Data.Name = Path.GetFileNameWithoutExtension(filepath);

            // Set up a count of the amount of vertices we've handled so that faces line up correctly.
            int vertCount = 0;

            // Loop through all meshes in the imported file.
            for (int i = 0; i < assimpModel.Meshes.Count; i++)
            {
                // Add all the vertices for this mesh.
                foreach (var vertex in assimpModel.Meshes[i].Vertices)
                    Data.Vertices.Add(new(vertex.X, vertex.Y, vertex.Z));

                // Set up a count of the texture coordinates so we can keep track of which to use.
                int textureCoordinate = 0;

                // Loop through each face in this mesh.
                foreach (var assimpFace in assimpModel.Meshes[i].Faces)
                {
                    // Create a new face.
                    Face face = new();

                    // Set material name for this face.
                    face.MaterialName = assimpModel.Materials[i].Name;

                    // Set the vertex indices on this face.
                    face.VertexIndices[0] = (short)(assimpFace.Indices[0] + vertCount);
                    face.VertexIndices[1] = (short)(assimpFace.Indices[1] + vertCount);
                    face.VertexIndices[2] = (short)(assimpFace.Indices[2] + vertCount);

                    // Set the texture coordinates for this face.
                    face.TextureCoordinates[0] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate].X;
                    face.TextureCoordinates[1] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate].Y;
                    face.TextureCoordinates[2] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate + 1].X;
                    face.TextureCoordinates[3] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate + 1].Y;
                    face.TextureCoordinates[4] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate + 2].X;
                    face.TextureCoordinates[5] = -assimpModel.Meshes[i].TextureCoordinateChannels[0][textureCoordinate + 2].Y;

                    // Save this face.
                    Data.Faces.Add(face);

                    // Increment texture coordinates by 3.
                    textureCoordinate += 3;
                }

                // Add this mesh's vertex count to the vertCount int.
                vertCount += assimpModel.Meshes[i].Vertices.Count;
            }
            
            // Set up list for the X, Y and Z coordinates for every vertex.
            List<float> xValues = new();
            List<float> yValues = new();
            List<float> zValues = new();

            // Get every vertex's X, Y and Z coordinate.
            foreach (var vertex in Data.Vertices)
            {
                xValues.Add(vertex.X);
                yValues.Add(vertex.Y);
                zValues.Add(vertex.Z);
            }

            // Sort the list of coordinates.
            xValues.Sort();
            yValues.Sort();
            zValues.Sort();

            // Calculate the model's central point.
            Data.CentralPoint = new((xValues[0] + xValues[^1]) / 2, (yValues[0] + yValues[^1]) / 2, (zValues[0] + zValues[^1]) / 2);
        }
    }
}
