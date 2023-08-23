using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward
{
    public class Collision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision() { }
        public Collision(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
            {
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.collision.json", Data);
                ExportOBJTemp($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
            }
        }

        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Behaviour : ulong
        {
            Solid = 0x1,
            TopSolid = 0x2,
            Unknown_1 = 0x5, // What does this one do? Often paired with NoNewt?
            Spikes = 0x8,
            NoNewt = 0x10,
            BottomlessPit = 0x20,
            DamageZone = 0x40, // Has to be paired with Water to use.
            HealingZone = 0x400, // Has to be paired with Water to use.
            Drill_1 = 0x800, // How does this one actually work?
            Drill_2 = 0x8000, // How does this one actually work?
            Water = 0x200000,
            Unknown_2 = 0x10000000, // What does this one do?
            Unknown_3 = 0x20000000, // What does this one do?
            UseBoundingBox = 0x200000000
        }

        public class FormatData
        {
            /// <summary>
            /// The models that make up this collision.
            /// </summary>
            public List<Model> Models = new();

            /// <summary>
            /// An unknown chunk of data that still needs reverse engineering.
            /// </summary>
            public UnknownData UnknownData = new();
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
            public Behaviour Behaviour { get; set; }

            /// <summary>
            /// An unknown 64 bit integer value.
            /// TODO: What is this?
            /// </summary>
            public ulong UnknownULong_1 { get; set; }

            /// <summary>
            /// The coordinates for the various vertices that make up this model.
            /// </summary>
            public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// The faces that make up this model.
            /// </summary>
            public Face[] Faces { get; set; } = Array.Empty<Face>();
        }

        public class Face
        {
            /// <summary>
            /// The index of this face's first vertex.
            /// </summary>
            public uint IndexA { get; set; }

            /// <summary>
            /// The index of this face's second vertex.
            /// </summary>
            public uint IndexB { get; set; }

            /// <summary>
            /// The index of this face's third vertex.
            /// </summary>
            public uint IndexC { get; set; }
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
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the count of collision models in this file.
            uint ModelCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0x40. Likely an offset to the collision model table.
            reader.JumpAhead(0x08);

            // Skip an unknown value that is always 1. Count for UO1?
            reader.JumpAhead(0x08);

            // Read an offset to data that I currently don't know. Seems to (in some way) control screen transitions.
            long UnknownOffset_1 = reader.ReadInt64();

            // Realign to 0x40 bytes.
            reader.FixPadding(0x40);

            // Loop through and read each collision model in this file.
            for (int i = 0; i < ModelCount; i++)
            {
                // Define a new model entry.
                Model Model = new();

                // Loop through and read the two values of this model's Axis-Aligned Bounding Box.
                for (int aabb = 0; aabb < 2; aabb++)
                    Model.AABB[aabb] = reader.ReadVector3();

                // Read an unknown 64 bit integer.
                Model.Behaviour = (Behaviour)reader.ReadUInt64();

                // Read an unknown 64 bit integer.
                Model.UnknownULong_1 = reader.ReadUInt64();

                // Read the offset to this model's vertex and face data.
                long ModelDataOffset = reader.ReadInt64();

                // Save our position to jump back for the next model.
                long position = reader.BaseStream.Position;

                // Jump to this model's data.
                reader.JumpTo(ModelDataOffset);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this model's vertex count.
                uint VertexCount = reader.ReadUInt32();

                // Skip a value that is always 0x20. Likely an additive offset to the vertex table.
                reader.JumpAhead(0x08);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this model's face count. The vanilla models have this value always match Vertex Count, but lets be safe.
                uint FaceCount = reader.ReadUInt32();

                // Skip a value that appears to be an additive offset to the face table.
                reader.JumpAhead(0x08);

                // Define the sizes of the vertex and face arrays.
                Model.Vertices = new Vector3[VertexCount];
                Model.Faces = new Face[FaceCount];

                // Read each vertex in this model.
                for (int v = 0; v < VertexCount; v++)
                    Model.Vertices[v] = reader.ReadVector3();

                // Read each face in this model.
                for (int v = 0; v < FaceCount; v++)
                {
                    Model.Faces[v] = new()
                    {
                        IndexA = reader.ReadUInt32(),
                        IndexB = reader.ReadUInt32(),
                        IndexC = reader.ReadUInt32()
                    };
                }

                // Save this model.
                Data.Models.Add(Model);

                // Jump back for the next model.
                reader.JumpTo(position);
            }

            // TODO: Reverse engineer this massive chunk of data.
            reader.JumpTo(UnknownOffset_1);

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

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write the amount of models in this file.
            writer.Write(Data.Models.Count);

            // Write a value that is always 0x40. This is likely an offset, but as there's no table like a BINA Format, I don't need to worry about it.
            writer.Write(0x40L);

            // Write a value that is always 1. This is likely a count of something, but it never changes.
            writer.Write(0x01L);

            // Add an offset to the unknown data.
            writer.AddOffset("UnknownOffset_1", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write the model table.
            for (int i = 0; i < Data.Models.Count; i++)
            {
                // Write this model's AABB values.
                writer.Write(Data.Models[i].AABB[0]);
                writer.Write(Data.Models[i].AABB[1]);

                // Write this model's behaviour tag.
                writer.Write((ulong)Data.Models[i].Behaviour);

                // Write this model's unknown integer value.
                writer.Write(Data.Models[i].UnknownULong_1);

                // Add an offset for this model's data.
                writer.AddOffset($"Model{i}Data", 0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write the table of model data.
            for (int i = 0; i < Data.Models.Count; i++)
            {
                // Fill in the offset for this model's data.
                writer.FillOffset($"Model{i}Data");

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write this model's vertex count.
                writer.Write(Data.Models[i].Vertices.Length);

                // Write an additive offset value that is always 0x20.
                writer.Write(0x20L);

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write this model's face count.
                writer.Write(Data.Models[i].Faces.Length);

                // Calculate and write an additive offset for this model's face table.
                writer.Write((long)(Data.Models[i].Vertices.Length * 0x0C + 0x20));

                // Write each vertex's coordinates.
                foreach (Vector3 vertex in Data.Models[i].Vertices)
                    writer.Write(vertex);

                // Write each face's data.
                foreach (Face face in Data.Models[i].Faces)
                {
                    writer.Write(face.IndexA);
                    writer.Write(face.IndexB);
                    writer.Write(face.IndexC);
                }
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

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

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Temporary solution to export the model data to an OBJ.
        /// </summary>
        /// <param name="directory">The directory to export to.</param>
        public void ExportOBJTemp(string directory)
        {
            // Create the directory.
            Directory.CreateDirectory(directory);

            // Loop through each model.
            for (int i = 0; i < Data.Models.Count; i++)
            {
                // Create a StreamWriter for this model.
                StreamWriter obj = new($@"{directory}\model{i}.obj");

                // Print the model we're exporting.
                Console.WriteLine($@"Exporting {directory}\model{i}.obj");

                // Write each vertex.
                foreach (var vertex in Data.Models[i].Vertices)
                    obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write the object name for this model.
                obj.WriteLine($"g model{i}_{Data.Models[i].Behaviour}_unk2[0x{Data.Models[i].UnknownULong_1.ToString("X").PadLeft(16, '0')}]");
                obj.WriteLine($"o model{i}_{Data.Models[i].Behaviour}_unk2[0x{Data.Models[i].UnknownULong_1.ToString("X").PadLeft(16, '0')}]");

                // Write each face for this model, with the indices incremented by 1 due to OBJ counting from 1 not 0.
                foreach (var face in Data.Models[i].Faces)
                    obj.WriteLine($"f {face.IndexA + 1} {face.IndexB + 1} {face.IndexC + 1}");

                // Close the StreamWriter.
                obj.Close();
            }
        }
    }
}
