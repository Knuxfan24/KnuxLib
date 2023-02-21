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
            public byte Opacity { get; set; }

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
            public byte[] Colours { get; set; } = new byte[3];

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
            for (int i = 0; i < mat.Length; i++)
            {
                // If this line is the [MaterialBegin] tag, then set up a new material.
                if (mat[i] == "[MaterialBegin]")
                    material = new();

                // If this line is the [MaterialEnd] tag, then save this material.
                else if (mat[i] == "[MaterialEnd]")
                    Data.Add(material);

                // If this line is neither of those and isn't empty, then assume it's a material parameter.
                else if (mat[i] != "")
                {
                    switch (mat[i])
                    {
                        case string s when s.StartsWith("Name"): material.Name = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("Flags"): material.Flags = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("Opacity"): material.Opacity = byte.Parse(mat[i][(mat[i].IndexOf(' ') + 1)..]); break;

                        case string s when s.StartsWith("Texture"): material.Diffuse = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("AlphaMask"): material.AlphaMask = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("NormalMap"): material.NormalMap = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("EnvMap"): material.EnvironmentMap = mat[i][(mat[i].IndexOf(' ') + 1)..]; break;

                        case string s when s.StartsWith("EnvPower"): material.EnvironmentMapPower = byte.Parse(mat[i][(mat[i].IndexOf(' ') + 1)..]); break;

                        case string s when s.StartsWith("Color24"):
                            string[] colours = mat[i].Split(' ');
                            material.Colours = new byte[3];
                            material.Colours[0] = byte.Parse(colours[1]);
                            material.Colours[1] = byte.Parse(colours[2]);
                            material.Colours[2] = byte.Parse(colours[3]);
                            break;

                        default: throw new NotImplementedException($"Unknown material setting '{mat[i].Remove(mat[i].IndexOf('='))}'.");
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
            for (int i = 0; i < Data.Count; i++)
            {
                // Write the [MaterialBegin] header.
                mat.WriteLine("[MaterialBegin]");

                // Write this material's name.
                mat.WriteLine($"Name= {Data[i].Name}");

                // Write this material's flags.
                mat.WriteLine($"Flags= {Data[i].Flags}");

                // Write this material's opacity value.
                mat.WriteLine($"Opacity= {Data[i].Opacity}");

                // Write this material's diffuse texture, if it has one.
                if (Data[i].Diffuse != null) { mat.WriteLine($"Texture= {Data[i].Diffuse}"); }

                // Write this material's alpha mask texture, if it has one.
                if (Data[i].AlphaMask != null) { mat.WriteLine($"AlphaMask= {Data[i].AlphaMask}"); }

                // Write this material's normal map texture, if it has one.
                if (Data[i].NormalMap != null) { mat.WriteLine($"NormalMap= {Data[i].NormalMap}"); }

                // Write this material's environment map texture, if it has one.
                if (Data[i].EnvironmentMap != null) { mat.WriteLine($"EnvMap= {Data[i].EnvironmentMap}"); }

                // Write this material's reflection strength value, if it has one.
                if (Data[i].EnvironmentMapPower != null) { mat.WriteLine($"EnvPower= {Data[i].EnvironmentMapPower}"); }

                // Write this material's colours.
                mat.WriteLine($"Color24= {Data[i].Colours[0]} {Data[i].Colours[1]} {Data[i].Colours[2]}");

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
            for (int i = 0; i < Data.Count; i++)
            {
                // Write this material's name.
                mtl.WriteLine($"newmtl {Data[i].Name}");

                // Write this material's opacity.
                mtl.WriteLine($"d {(float)Data[i].Opacity / 255}");

                // Write this material's colours.
                mtl.WriteLine($"kd {(float)Data[i].Colours[0] / 255} {(float)Data[i].Colours[1] / 255} {(float)Data[i].Colours[2] / 255}");

                // Write this material's diffuse texture, if it has one.
                if (Data[i].Diffuse != null)
                    mtl.WriteLine($"map_Kd {Data[i].Diffuse}");

                // Write this material's alpha mask texture, if it has one.
                if (Data[i].AlphaMask != null)
                    mtl.WriteLine($"map_d {Data[i].AlphaMask}");

                // Write this material's normal map texture, if it has one.
                if (Data[i].NormalMap != null)
                    mtl.WriteLine($"map_bump {Data[i].NormalMap}");

                // Write the spare line break.
                mtl.WriteLine();
            }

            // Close this StreamWriter.
            mtl.Close();
        }


        /// <summary>
        /// Imports an OBJ compatible MTL file and converts it to a material library.
        /// TODO: Implement.
        /// </summary>
        /// <param name="filepath">The filepath of the MTL to import.</param>
        public void ImportMTL(string filepath)
        {
            throw new NotImplementedException();
        }
    }
}
