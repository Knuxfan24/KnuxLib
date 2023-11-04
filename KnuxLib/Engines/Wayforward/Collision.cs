using Assimp.Configs;
using Assimp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward
{
    public class Collision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision() { }
        public Collision(string filepath, FormatVersion version = FormatVersion.hero, bool export = false)
        {
            Load(filepath, version);

            if (export)
                ExportOBJ(Path.ChangeExtension(filepath, ".obj"));
        }

        // Classes for this format.
        public enum FormatVersion
        {
            hero = 0,
            sevensirens = 1
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Behaviour : ulong
        {
            // TODO: Figure out if I can split 0x400 depending on format version.
            // TODO: Figure out the unknown tags.
            // TODO: Check the game specific ones in the other (just in case some HGH stuff works in SS and vice versa)
            Solid = 0x1,
            TopSolid = 0x2,
            Unknown_1 = 0x4, // Half-Genie Hero only.
            Unknown_2 = 0x5,
            Spikes = 0x8,
            NoMonkey = 0x10,
            BottomlessPit = 0x20,
            DamageZone = 0x40, // Has to be paired with Water to use. Seven Sirens only.
            Unknown_3 = 0x100, // Half-Genie Hero only.
            WoodSound = 0x400, // Half-Genie Hero only.
            //HealingZone = 0x400, // Has to be paired with Water to use. Seven Sirens only.
            Unknown_4 = 0x4000,  // Half-Genie Hero only.
            DrillZone = 0x8000, // How does this one actually work? Seven Sirens only.
            Water = 0x200000, //Seven Sirens only.
            Slide = 0x400000, // Half-Genie Hero only.
            Unknown_5 = 0x800000, // Half-Genie Hero only.
            Unknown_6 = 0x1000000, // Half-Genie Hero only.
            Unknown_7 = 0x2000000, // Half-Genie Hero only.
            Unknown_8 = 0x10000000, // What does this one do? Seems to be next to water a lot?
            Unknown_9 = 0x20000000, // What does this one do? Seven Sirens only.
            Unknown_10 = 0x80000000, // What does this one do? Half-Genie Hero only.
            UseBoundingBox = 0x200000000 // Seven Sirens only.
        }

        public class FormatData
        {
            /// <summary>
            /// The models that make up this collision.
            /// </summary>
            public List<Model> Models = new();

            /// <summary>
            /// An unknown chunk of data that is only present in Seven Sirens and still needs reverse engineering.
            /// </summary>
            public UnknownData? UnknownData = new();
        }

        public class Model
        {
            /// <summary>
            /// This model's Axis-Aligned Bounding Box.
            /// </summary>
            public Vector3[] AABB { get; set; } = new Vector3[2];

            /// <summary>
            /// How this model behaves.
            /// </summary>
            public Behaviour Behaviour { get; set; } = Behaviour.Solid;

            /// <summary>
            /// An unknown 64 bit integer value that is only present in Seven Sirens.
            /// TODO: What is this?
            /// </summary>
            public ulong? UnknownULong_1 { get; set; }

            /// <summary>
            /// The coordinates for the various vertices that make up this model.
            /// </summary>
            public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// The faces that make up this model.
            /// </summary>
            public Face[] Faces { get; set; } = Array.Empty<Face>();
        }

        /// <summary>
        /// TODO: This is temporary and needs replacing with a proper class for whatever this data is.
        /// </summary>
        public class UnknownData
        {
            public ushort UnknownUShort_1 { get; set; }
            public ushort UnknownUShort_2 { get; set; }
            public ushort UnknownUShort_3 { get; set; }
            public ushort UnknownUShort_4 { get; set; }
            public ushort UnknownUShort_5 { get; set; }
            public ushort UnknownUShort_6 { get; set; }
            public ushort UnknownUShort_7 { get; set; }
            public List<ulong> Data = new();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.hero)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the count of collision models in this file.
            uint modelCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0x40. Likely an offset to the collision model table.
            reader.JumpAhead(0x08);

            // Skip an unknown value that is always 1 in Seven Sirens but 0 in Half-Genie Hero. Potentially a count of some sorts for UnknownOffset_1?
            reader.JumpAhead(0x08);

            // Read an offset to data that I currently don't know. Seems to (in some way) control screen transitions.
            // Only exists in Seven Sirens.
            long unknownOffset_1 = reader.ReadInt64();

            // Realign to 0x40 bytes.
            reader.FixPadding(0x40);

            // Loop through and read each collision model in this file.
            for (int modelIndex = 0; modelIndex < modelCount; modelIndex++)
            {
                // Define a new model entry.
                Model model = new();

                // Loop through and read the two values of this model's Axis-Aligned Bounding Box.
                for (int aabb = 0; aabb < 2; aabb++)
                    model.AABB[aabb] = reader.ReadVector3();

                // Read an unknown 64 bit integer.
                model.Behaviour = (Behaviour)reader.ReadUInt64();

                // Read an unknown 64 bit integer that only exists in Seven Sirens.
                if (version == FormatVersion.sevensirens)
                    model.UnknownULong_1 = reader.ReadUInt64();

                // Read the offset to this model's vertex and face data.
                long modelDataOffset = reader.ReadInt64();

                // Save our position to jump back for the next model.
                long position = reader.BaseStream.Position;

                // Jump to this model's data.
                reader.JumpTo(modelDataOffset);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this model's vertex count.
                uint vertexCount = reader.ReadUInt32();

                // Skip a value that is always 0x20. Likely an additive offset to the vertex table.
                reader.JumpAhead(0x08);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this model's face count. The vanilla models have this value always match Vertex Count, but lets be safe.
                uint faceCount = reader.ReadUInt32();

                // Skip a value that appears to be an additive offset to the face table.
                reader.JumpAhead(0x08);

                // Define the sizes of the vertex and face arrays.
                model.Vertices = new Vector3[vertexCount];
                model.Faces = new Face[faceCount];

                // Read each vertex in this model.
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                    model.Vertices[vertexIndex] = reader.ReadVector3();

                // Read each face in this model.
                for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                {
                    model.Faces[faceIndex] = new()
                    {
                        IndexA = reader.ReadUInt32(),
                        IndexB = reader.ReadUInt32(),
                        IndexC = reader.ReadUInt32()
                    };
                }

                // Save this model.
                Data.Models.Add(model);

                // Jump back for the next model.
                reader.JumpTo(position);
            }

            // TODO: Reverse engineer this massive chunk of data.
            if (version == FormatVersion.sevensirens)
            {
                // Jump to the offset of unknown data exclusive to Seven Sirens.
                reader.JumpTo(unknownOffset_1);

                // Define the UnknownData.
                Data.UnknownData = new();

                // Temporarily read this set of values.
                Data.UnknownData.UnknownUShort_1 = reader.ReadUInt16(); // only not 0 on title screen
                Data.UnknownData.UnknownUShort_2 = reader.ReadUInt16(); // only not 0 on title screen
                Data.UnknownData.UnknownUShort_3 = reader.ReadUInt16();
                Data.UnknownData.UnknownUShort_4 = reader.ReadUInt16(); // only not 0 on title screen
                Data.UnknownData.UnknownUShort_5 = reader.ReadUInt16(); // only not 0 on title screen
                Data.UnknownData.UnknownUShort_6 = reader.ReadUInt16(); // only not 0 on title screen
                Data.UnknownData.UnknownUShort_7 = reader.ReadUInt16();

                // Realign to 0x10 bytes.
                reader.FixPadding(0x10);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x08);

                // Skip an offset that is always the position of this + 0x28.
                reader.JumpAhead(0x08);

                // Realign to 0x40 bytes.
                reader.FixPadding(0x40);

                // Temporarily read the rest of the file's data.
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    Data.UnknownData.Data.Add(reader.ReadUInt64());
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.hero)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write the amount of models in this file.
            writer.Write(Data.Models.Count);

            // Write a value that is always 0x40. This is likely an offset, but as there's no table like a BINA Format, I don't need to worry about it.
            writer.Write(0x40L);

            if (version == FormatVersion.sevensirens)
            {
                // Write a value that is always 1. This is likely a count of something, but it never changes.
                writer.Write(0x01L);

                // Add an offset to the unknown data.
                writer.AddOffset("UnknownOffset_1", 0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write the model table.
            for (int modelIndex = 0; modelIndex < Data.Models.Count; modelIndex++)
            {
                // Write this model's AABB values.
                writer.Write(Data.Models[modelIndex].AABB[0]);
                writer.Write(Data.Models[modelIndex].AABB[1]);

                // Write this model's behaviour tag.
                writer.Write((ulong)Data.Models[modelIndex].Behaviour);

                // Write this model's unknown integer value.
                if (version == FormatVersion.sevensirens)
                    writer.Write((ulong)Data.Models[modelIndex].UnknownULong_1);

                // Add an offset for this model's data.
                writer.AddOffset($"Model{modelIndex}Data", 0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write the table of model data.
            for (int modelIndex = 0; modelIndex < Data.Models.Count; modelIndex++)
            {
                // Fill in the offset for this model's data.
                writer.FillOffset($"Model{modelIndex}Data");

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write this model's vertex count.
                writer.Write(Data.Models[modelIndex].Vertices.Length);

                // Write an additive offset value that is always 0x20.
                writer.Write(0x20L);

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write this model's face count.
                writer.Write(Data.Models[modelIndex].Faces.Length);

                // Calculate and write an additive offset for this model's face table.
                writer.Write((long)(Data.Models[modelIndex].Vertices.Length * 0x0C + 0x20));

                // Write each vertex's coordinates.
                foreach (Vector3 vertex in Data.Models[modelIndex].Vertices)
                    writer.Write(vertex);

                // Write each face's data.
                foreach (Face face in Data.Models[modelIndex].Faces)
                {
                    writer.Write(face.IndexA);
                    writer.Write(face.IndexB);
                    writer.Write(face.IndexC);
                }

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            if (version == FormatVersion.sevensirens)
            {
                // Fill in the offset to the unknown data.
                writer.FillOffset("UnknownOffset_1");

                // Write the temporary data for the unknown chunk.
                writer.Write(Data.UnknownData.UnknownUShort_1);
                writer.Write(Data.UnknownData.UnknownUShort_2);
                writer.Write(Data.UnknownData.UnknownUShort_3);
                writer.Write(Data.UnknownData.UnknownUShort_4);
                writer.Write(Data.UnknownData.UnknownUShort_5);
                writer.Write(Data.UnknownData.UnknownUShort_6);
                writer.Write(Data.UnknownData.UnknownUShort_7);
                writer.FixPadding(0x10);
                writer.Write(0L);
                writer.Write(writer.BaseStream.Position + 0x28);
                writer.FixPadding(0x40);

                foreach (ulong value in Data.UnknownData.Data)
                    writer.Write(value);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Exports this collision's model data to an OBJ file.
        /// TODO: Figure out what I want to do with the Unknown Data in Seven Sirens when it comes to this.
        /// TODO: Figure out how I want to present the tags, the @ system is what I want but 3DS Max's OBJ importer is stupid and changes most special characters to an underscore.
        /// </summary>
        /// <param name="filepath">The directory to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 0;

            // Create the StreamWriter.
            StreamWriter obj = new(filepath);

            // Loop through each model.
            for (int modelIndex = 0; modelIndex < Data.Models.Count; modelIndex++)
            {
                // Write the Vertex Comment for this model.
                obj.WriteLine($"# Model {modelIndex} Vertices\r\n");

                // Write each vertex.
                foreach (Vector3 vertex in Data.Models[modelIndex].Vertices)
                    obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write the Name/Behaviour Tags Comment for this model.
                obj.WriteLine($"\r\n# Model {modelIndex} Name and Behaviour Tags\r\n");

                // Set up a list of the behaviour tags.
                string tags = "";

                // Populate the list with the tags from this model.
                foreach (string tag in Data.Models[modelIndex].Behaviour.ToString().Split(", "))
                    tags += $"@{tag}";

                // Write the object name for this model.
                if (Data.Models[modelIndex].UnknownULong_1 != null)
                {
                    ulong value = (ulong)Data.Models[modelIndex].UnknownULong_1;
                    obj.WriteLine($"g model{modelIndex}{tags}_unk1[0x{value.ToString("X").PadLeft(16, '0')}]");
                    obj.WriteLine($"o model{modelIndex}{tags}_unk1[0x{value.ToString("X").PadLeft(16, '0')}]");
                }
                else
                {
                    obj.WriteLine($"g model{modelIndex}{tags}");
                    obj.WriteLine($"o model{modelIndex}{tags}");
                }

                // Write the Faces Comment for this model.
                obj.WriteLine($"\r\n# Model {modelIndex} Faces\r\n");

                // Write each face for this model, with the indices incremented by 1 (and the current value of vertexCount) due to OBJ counting from 1 not 0.
                foreach (Face face in Data.Models[modelIndex].Faces)
                    obj.WriteLine($"f {face.IndexA + 1 + vertexCount} {face.IndexB + 1 + vertexCount} {face.IndexC + 1 + vertexCount}");

                // Add the amount of vertices in this model to the count.
                vertexCount += Data.Models[modelIndex].Vertices.Length;

                // Write an empty line for neatness.
                obj.WriteLine();
            }

            // Close the StreamWriter.
            obj.Close();
        }

        /// <summary>
        /// Imports a Assimp compatible model and converts it to a Wayforward Engine collision model.
        /// TODO: Handle the differences in the Seven Sirens format when that's finally reverse engineered.
        /// </summary>
        /// <param name="filepath">The filepath of the model to import.</param>
        public void ImportAssimp(string filepath)
        {
            // Setup AssimpNet Scene.
            AssimpContext assimpImporter = new();
            KeepSceneHierarchyConfig config = new(true);
            assimpImporter.SetConfig(config);
            Scene assimpModel = assimpImporter.ImportFile(filepath, PostProcessSteps.PreTransformVertices | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.GenerateBoundingBoxes);

            // Loop through all meshes in the imported file.
            for (int meshIndex = 0; meshIndex < assimpModel.Meshes.Count; meshIndex++)
            {
                // Set up the model.
                Model model = new();

                // Create the vertex and face lists.
                model.Vertices = new Vector3[assimpModel.Meshes[meshIndex].Vertices.Count];
                model.Faces = new Face[assimpModel.Meshes[meshIndex].Faces.Count];

                // Fill in the AABB.
                model.AABB[0] = new(assimpModel.Meshes[meshIndex].BoundingBox.Min.X, assimpModel.Meshes[meshIndex].BoundingBox.Min.Y, assimpModel.Meshes[meshIndex].BoundingBox.Min.Z);
                model.AABB[1] = new(assimpModel.Meshes[meshIndex].BoundingBox.Max.X, assimpModel.Meshes[meshIndex].BoundingBox.Max.Y, assimpModel.Meshes[meshIndex].BoundingBox.Max.Z);

                // Add all the vertices for this mesh.
                for (int vertexIndex = 0; vertexIndex < assimpModel.Meshes[meshIndex].Vertices.Count; vertexIndex++)
                    model.Vertices[vertexIndex] = new(assimpModel.Meshes[meshIndex].Vertices[vertexIndex].X, assimpModel.Meshes[meshIndex].Vertices[vertexIndex].Y, assimpModel.Meshes[meshIndex].Vertices[vertexIndex].Z);

                // Add all the faces for this mesh.
                for (int faceIndex = 0; faceIndex < assimpModel.Meshes[meshIndex].Faces.Count; faceIndex++)
                    model.Faces[faceIndex] = new() { IndexA = (uint)assimpModel.Meshes[meshIndex].Faces[faceIndex].Indices[0], IndexB = (uint)assimpModel.Meshes[meshIndex].Faces[faceIndex].Indices[1], IndexC = (uint)assimpModel.Meshes[meshIndex].Faces[faceIndex].Indices[2] };

                // Add the behaviour tags for this mesh.
                if (assimpModel.Meshes[meshIndex].Name.Contains('@'))
                {
                    // Split the mesh name based on the @ character (hold over from old Sonic stuff).
                    string[] nameSplit = assimpModel.Meshes[meshIndex].Name.Split('@');

                    // Loop through each split (ignoring the first) and apply the approriate tag.
                    for (int splitIndex = 1; splitIndex < nameSplit.Length; splitIndex++)
                    {
                        switch (nameSplit[splitIndex].ToLower())
                        {
                            case "solid":                         model.Behaviour |= Behaviour.Solid;          break;
                            case "topsolid":                      model.Behaviour |= Behaviour.TopSolid;       break;
                            case "unknown_1":                     model.Behaviour |= Behaviour.Unknown_1;      break;
                            case "unknown_2":                     model.Behaviour |= Behaviour.Unknown_2;      break;
                            case "spikes":                        model.Behaviour |= Behaviour.Spikes;         break;
                            case "nomonkey":                      model.Behaviour |= Behaviour.NoMonkey;       break;
                            case "bottomlesspit":                 model.Behaviour |= Behaviour.BottomlessPit;  break;
                            case "damagezone":                    model.Behaviour |= Behaviour.DamageZone;     break;
                            case "unknown_3":                     model.Behaviour |= Behaviour.Unknown_3;      break;
                            case "woodsound": case "healingzone": model.Behaviour |= Behaviour.WoodSound;      break;
                            case "unknown_4":                     model.Behaviour |= Behaviour.Unknown_4;      break;
                            case "drillzone":                     model.Behaviour |= Behaviour.DrillZone;      break;
                            case "water":                         model.Behaviour |= Behaviour.Water;          break;
                            case "slide":                         model.Behaviour |= Behaviour.Slide;          break;
                            case "unknown_5":                     model.Behaviour |= Behaviour.Unknown_5;      break;
                            case "unknown_6":                     model.Behaviour |= Behaviour.Unknown_6;      break;
                            case "unknown_7":                     model.Behaviour |= Behaviour.Unknown_7;      break;
                            case "unknown_8":                     model.Behaviour |= Behaviour.Unknown_8;      break;
                            case "unknown_9":                     model.Behaviour |= Behaviour.Unknown_9;      break;
                            case "unknown_10":                    model.Behaviour |= Behaviour.Unknown_10;     break;
                            case "useboundingbox":                model.Behaviour |= Behaviour.UseBoundingBox; break;
                        }
                    }
                }

                // Save this mesh.
                Data.Models.Add(model);
            }
        }
    }
}
