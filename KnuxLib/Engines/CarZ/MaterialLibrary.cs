using Assimp.Configs;
using Assimp;

namespace KnuxLib.Engines.CarZ
{
    public class MaterialLibrary : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MaterialLibrary() { }
        public MaterialLibrary(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                ExportMTL($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.mtl");
        }

        // Classes for this format.
        public class Material
        {
            /// <summary>
            /// The name of this material.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The flags this material has.
            /// TODO: Get a list of these?
            /// </summary>
            public string Flags { get; set; } = "";

            /// <summary>
            /// The opacity of this material.
            /// </summary>
            public byte Opacity { get; set; } = 255;

            /// <summary>
            /// The diffuse texture this material should use, if any.
            /// </summary>
            public string? Diffuse { get; set; }

            /// <summary>
            /// The alpha mask texture this material should use, if any.
            /// </summary>
            public string? AlphaMask { get; set; }

            /// <summary>
            /// The normal map texture this material should use, if any.
            /// </summary>
            public string? NormalMap { get; set; }

            /// <summary>
            /// The environment map texture this material should use, if any.
            /// </summary>
            public string? EnvironmentMap { get; set; }

            /// <summary>
            /// The strength of the reflection on this material?
            /// </summary>
            public byte? EnvironmentMapPower { get; set; }

            /// <summary>
            /// The RGB colour values on this material.
            /// </summary>
            public byte[] Colours { get; set; } = new byte[3] { 255, 255, 255 };

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<Material> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Load the MAT into a string array.
            string[] mat = File.ReadAllLines(filepath);

            // Check that the start of the MAT is what we're expecting.
            if (mat[0] != "[MaterialBegin]")
                throw new Exception($"'{filepath}' does not appear to be a CarZ engine MAT file.");

            // Set up an empty material.
            Material material = new();

            // Loop through all the lines in the MAT file.
            for (int lineIndex = 0; lineIndex < mat.Length; lineIndex++)
            {
                // If this line is the [MaterialBegin] tag, then set up a new material.
                if (mat[lineIndex] == "[MaterialBegin]")
                    material = new();

                // If this line is the [MaterialEnd] tag, then save this material.
                else if (mat[lineIndex] == "[MaterialEnd]")
                    Data.Add(material);

                // If this line is neither of those and isn't empty, then assume it's a material parameter.
                else if (mat[lineIndex] != "")
                {
                    switch (mat[lineIndex])
                    {
                        case string s when s.StartsWith("Name"): material.Name = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("Flags"): material.Flags = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("Opacity"): material.Opacity = byte.Parse(mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]); break;

                        case string s when s.StartsWith("Texture"): material.Diffuse = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("AlphaMask"): material.AlphaMask = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("NormalMap"): material.NormalMap = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("EnvMap"): material.EnvironmentMap = mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("EnvPower"): material.EnvironmentMapPower = byte.Parse(mat[lineIndex][(mat[lineIndex].IndexOf(' ') + 1)..]); break;

                        case string s when s.StartsWith("Color24"):
                            string[] colours = mat[lineIndex].Split(' ');
                            material.Colours = new byte[3];
                            material.Colours[0] = byte.Parse(colours[1]);
                            material.Colours[1] = byte.Parse(colours[2]);
                            material.Colours[2] = byte.Parse(colours[3]);
                            break;

                        default: throw new NotImplementedException($"Unknown material setting '{mat[lineIndex].Remove(mat[lineIndex].IndexOf('='))}'.");
                    }
                }
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter mat = new(filepath);

            // Write each material.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the [MaterialBegin] header.
                mat.WriteLine("[MaterialBegin]");

                // Write this material's name.
                mat.WriteLine($"Name= {Data[dataIndex].Name}");

                // Write this material's flags.
                mat.WriteLine($"Flags= {Data[dataIndex].Flags}");

                // Write this material's opacity value.
                mat.WriteLine($"Opacity= {Data[dataIndex].Opacity}");

                // Write this material's diffuse texture, if it has one.
                if (Data[dataIndex].Diffuse != null) { mat.WriteLine($"Texture= {Data[dataIndex].Diffuse}"); }

                // Write this material's alpha mask texture, if it has one.
                if (Data[dataIndex].AlphaMask != null) { mat.WriteLine($"AlphaMask= {Data[dataIndex].AlphaMask}"); }

                // Write this material's normal map texture, if it has one.
                if (Data[dataIndex].NormalMap != null) { mat.WriteLine($"NormalMap= {Data[dataIndex].NormalMap}"); }

                // Write this material's environment map texture, if it has one.
                if (Data[dataIndex].EnvironmentMap != null) { mat.WriteLine($"EnvMap= {Data[dataIndex].EnvironmentMap}"); }

                // Write this material's reflection strength value, if it has one.
                if (Data[dataIndex].EnvironmentMapPower != null) { mat.WriteLine($"EnvPower= {Data[dataIndex].EnvironmentMapPower}"); }

                // Write this material's colours.
                mat.WriteLine($"Color24= {Data[dataIndex].Colours[0]} {Data[dataIndex].Colours[1]} {Data[dataIndex].Colours[2]}");

                // Write the [MaterialEnd] footer and a spare empty line.
                mat.WriteLine("[MaterialEnd]\n");
            }

            // Close this StreamWriter.
            mat.Close();
        }

        /// <summary>
        /// Exports this material library to an OBJ compatible MTL file.
        /// </summary>
        /// <param name="filepath"></param>
        public void ExportMTL(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter mtl = new(filepath);

            // Write each material.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this material's name.
                mtl.WriteLine($"newmtl {Data[dataIndex].Name}");

                // Write this material's opacity.
                mtl.WriteLine($"d {(float)Data[dataIndex].Opacity / 255}");

                // Write this material's colours.
                mtl.WriteLine($"kd {(float)Data[dataIndex].Colours[0] / 255} {(float)Data[dataIndex].Colours[1] / 255} {(float)Data[dataIndex].Colours[2] / 255}");

                // Write this material's diffuse texture, if it has one.
                if (Data[dataIndex].Diffuse != null)
                    mtl.WriteLine($"map_Kd {Data[dataIndex].Diffuse}");

                // Write this material's alpha mask texture, if it has one.
                if (Data[dataIndex].AlphaMask != null)
                    mtl.WriteLine($"map_d {Data[dataIndex].AlphaMask}");

                // Write this material's normal map texture, if it has one.
                if (Data[dataIndex].NormalMap != null)
                    mtl.WriteLine($"map_bump {Data[dataIndex].NormalMap}");

                // Write the spare line break.
                mtl.WriteLine();
            }

            // Close this StreamWriter.
            mtl.Close();
        }


        /// <summary>
        /// Imports material data from an Assimp compatible model and converts it to a material library.
        /// </summary>
        /// <param name="filepath">The filepath of the model to import.</param>
        public void ImportAssimp(string filepath)
        {
            // Setup AssimpNet Scene.
            AssimpContext assimpImporter = new();
            KeepSceneHierarchyConfig config = new(true);
            assimpImporter.SetConfig(config);
            Scene assimpModel = assimpImporter.ImportFile(filepath, PostProcessSteps.PreTransformVertices);

            // Loop through each material in this model.
            foreach (Assimp.Material? assimpMaterial in assimpModel.Materials)
            {
                // Create a new material.
                Material material = new();

                // Set this material's name.
                material.Name = assimpMaterial.Name;

                // Set this material's flag.
                // TODO: Potentially unhardcode?
                material.Flags = "texture_gouraud_";

                // Set this material's RGB values.
                material.Colours[0] = (byte)(assimpMaterial.ColorDiffuse.R * 255);
                material.Colours[1] = (byte)(assimpMaterial.ColorDiffuse.G * 255);
                material.Colours[2] = (byte)(assimpMaterial.ColorDiffuse.B * 255);

                // Set this material's opactity.
                material.Opacity = (byte)(assimpMaterial.Opacity * 255);

                // Set this material's diffuse texture (with the extension replaced with .bmp), if it has one.
                if (assimpMaterial.TextureDiffuse.FilePath != null)
                    material.Diffuse = $"{Path.GetFileNameWithoutExtension(assimpMaterial.TextureDiffuse.FilePath)}.bmp";

                // Set this material's alpha mask texture (with the extension replaced with .bmp), if it has one.
                if (assimpMaterial.TextureOpacity.FilePath != null)
                    material.AlphaMask = $"{Path.GetFileNameWithoutExtension(assimpMaterial.TextureOpacity.FilePath)}.bmp";

                // Set this material's normal map texture (with the extension replaced with .bmp), if it has one.
                if (assimpMaterial.TextureNormal.FilePath != null)
                    material.NormalMap = $"{Path.GetFileNameWithoutExtension(assimpMaterial.TextureNormal.FilePath)}.bmp";

                // Set this material's environment map texture (with the extension replaced with .bmp), if it has one.
                if (assimpMaterial.TextureSpecular.FilePath != null)
                    material.EnvironmentMap = $"{Path.GetFileNameWithoutExtension(assimpMaterial.TextureSpecular.FilePath)}.bmp";

                // Save this material.
                Data.Add(material);
            }
        }
    }
}
