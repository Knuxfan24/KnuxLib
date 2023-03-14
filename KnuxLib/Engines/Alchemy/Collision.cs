using System.Diagnostics;

namespace KnuxLib.Engines.Alchemy
{
    // TODO: What does HKE mean?
    // TODO: Tidy all of this up and document how it works.
    // TODO: Figure out all of the unknown values.
    // TODO: Check the values in the Subspace chunk we're skipping on the PS2 and Xbox.
    // TODO: Figure out how the Subspace Data pieces the models together, one arena looked right with the value that I assume is position, another didn't.
    public class Collision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision() { }
        public Collision(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        public class FormatData
        {
            public float UnknownFloat_1 { get; set; }

            public List<Model> Models { get; set; } = new();

            public Subspace Subspace { get; set; } = new();
        }

        public class Model
        {
            public string Name { get; set; } = "";

            public List<Vector3> Vertices { get; set; } = new();

            public List<ModelFace> Faces { get; set; } = new();

            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public override string ToString() => Name;
        }

        public class ModelFace
        {
            public uint IndexA { get; set; }

            public uint IndexB { get; set; }

            public uint IndexC { get; set; }
        }

        public class Subspace
        {
            public float UnknownFloat_1 { get; set; }

            public float UnknownFloat_2 { get; set; }

            public uint UnknownUInt32_1 { get; set; }

            public string CollectionName { get; set; } = "";

            public bool HasDragChunk { get; set; } = false;

            public List<Instance> Instances { get; set; } = new();

            public override string ToString() => CollectionName;
        }

        public class Instance
        {
            public string Name { get; set; } = "";

            public float UnknownFloat_1 { get; set; }

            public float UnknownFloat_2 { get; set; }

            public float UnknownFloat_3 { get; set; }

            public float UnknownFloat_4 { get; set; }

            public Vector3 UnknownVector3_1 { get; set; }

            public Vector3 Position { get; set; }

            public Vector3 UnknownVector3_2 { get; set; }

            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public uint UnknownUInt32_5 { get; set; }

            public uint UnknownUInt32_6 { get; set; }

            public Vector3 UnknownVector3_3 { get; set; }

            public uint UnknownUInt32_7 { get; set; }

            public uint UnknownUInt32_8 { get; set; }

            public uint? UnknownUInt32_9 { get; set; }

            public uint? UnknownUInt32_10 { get; set; }

            public override string ToString() => Name;
        }

        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Skip this file's header(?).
            reader.JumpAhead(0x24); // This header(?) is the same between all files.
            Data.UnknownFloat_1 = reader.ReadSingle(); // Always a 1, except in two files, where it's 1.000002, rounding error?
            reader.JumpAhead(0x08); // These bytes are the same between all files.

            // TODO: Is there a better way to count these?
            // Read a model entry if the next byte isn't a null.
            while (reader.ReadByte() != 0)
            {
                // Jump back a byte to get the one we skipped.
                reader.JumpBehind(0x01);

                // Set up a new model.
                Model model = new();

                // Read this model's name.
                model.Name = reader.ReadNullTerminatedString();

                // Read this model's vertex count.
                uint vertexCount = reader.ReadUInt32();

                // Read each of this model's vertices.
                for (int i = 0; i < vertexCount; i++)
                    model.Vertices.Add(reader.ReadVector3());

                // Read this model's face count.
                uint faceCount = reader.ReadUInt32();

                // Read each of this model's faces.
                for (int i = 0; i < faceCount; i++)
                {
                    ModelFace face = new()
                    {
                        IndexA = reader.ReadUInt32(),
                        IndexB = reader.ReadUInt32(),
                        IndexC = reader.ReadUInt32()
                    };
                    model.Faces.Add(face);
                }

                // Read unknown data.
                model.UnknownUInt32_1 = reader.ReadUInt32();
                model.UnknownUInt32_2 = reader.ReadUInt32();
                model.UnknownUInt32_3 = reader.ReadUInt32();
                model.UnknownUInt32_4 = reader.ReadUInt32();

                // Save this model.
                Data.Models.Add(model);
            }

            // Instance data?
            string subspaceName = reader.ReadNullTerminatedString(); // Always default_subspace.
            reader.JumpAhead(0x04); // Always D9 AE 66 0C.
            reader.JumpAhead(0x08); // Always 0x00.
            Data.Subspace.UnknownFloat_1 = reader.ReadSingle();
            reader.JumpAhead(0x04); // Always 35 1B A6 00.
            reader.JumpAhead(0x04); // Always CD CC CC 3D.
            reader.JumpAhead(0x04); // Always A2 B2 D2 01.
            reader.JumpAhead(0x04); // Always 01 A9 5C C3.
            reader.JumpAhead(0x04); // Always 00 00 00 A0.
            reader.JumpAhead(0x04); // Always 40 D9 B3 AB.
            Data.Subspace.UnknownFloat_2 = reader.ReadSingle();
            Data.Subspace.UnknownUInt32_1 = reader.ReadUInt32();
            reader.JumpAhead(0x04); // Always 0A EE 2A 88.
            reader.JumpAhead(0x04); // Always 0C FE A7 DF.
            reader.JumpAhead(0x04); // Always 0E 6E 0A DC.
            reader.JumpAhead(0x01); // Always 0x07.
            Data.Subspace.CollectionName = reader.ReadNullTerminatedString();
            reader.JumpAhead(0x04); // Always 12 22 87 04.
            reader.JumpAhead(0x04); // Always C2 C2 4C 00.
            reader.JumpAhead(0x04); // Always 25 D0 CD 02.
            reader.JumpAhead(0x04); // Always CD CC CC 3D.
            reader.JumpAhead(0x04); // Always 99 77 E3 03.

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Instance instance = new();
                instance.Name = reader.ReadNullTerminatedString();
                Helpers.TestValueUInt(reader, 0x08C76EF9); // Always F9 6E C7 08.
                instance.UnknownFloat_1 = reader.ReadSingle();
                Helpers.TestValueUInt(reader, 0x0055670E); // Always 0E 67 55 00.
                instance.UnknownFloat_2 = reader.ReadSingle();
                Helpers.TestValueUInt(reader, 0x00C1A1AE); // Always AE A1 C1 00.
                instance.UnknownFloat_3 = reader.ReadSingle();
                Helpers.TestValueUInt(reader, 0x0486894E); // Always 4E 89 86 04.
                instance.UnknownFloat_4 = reader.ReadSingle();
                instance.UnknownVector3_1 = reader.ReadVector3();
                Helpers.TestValueUInt(reader, 0x085FEC0E); // Always 0E EC 5F 08.
                instance.Position = reader.ReadVector3();
                Helpers.TestValueUInt(reader, 0x0CA54AD9); // Always D9 4A A5 0C.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x041158F9); // Always F9 58 11 04.
                instance.UnknownVector3_2 = reader.ReadVector3();
                Helpers.TestValueUInt(reader, 0x08F80D24); // Always 24 0D F8 08.
                Helpers.TestValueUInt(reader, 0x588EA500); // Always 00 A5 8E 58.
                instance.UnknownUInt32_1 = reader.ReadUInt32(); // Always 04 00 34 9D in final.
                instance.UnknownUInt32_2 = reader.ReadUInt32(); // Always F8 01 00 00 in final.
                instance.UnknownUInt32_3 = reader.ReadUInt32(); // Always 0x00 in final.
                instance.UnknownUInt32_4 = reader.ReadUInt32(); // Always 0x00 in final.
                instance.UnknownUInt32_5 = reader.ReadUInt32(); // Always 00 00 55 8D in final.
                Helpers.TestValueUInt(reader, 0x548507FA); // Always FA 07 85 54.
                Helpers.TestValueUShort(reader, 0x0173); // Always 73 01
                string instanceName2 = reader.ReadNullTerminatedString(); // Always the same as instanceName.
                if (instanceName2 != instance.Name)
                    Debugger.Break();
                Helpers.TestValueUInt(reader, 0x00051683); // Always 83 16 05 00.
                instance.UnknownUInt32_6 = reader.ReadUInt32(); // Always 0x00 in final.
                Helpers.TestValueUInt(reader, 0x0486894E); // Always 4E 89 86 04.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x085FEC0E); // Always 0E EC 5F 08.
                instance.UnknownVector3_3 = reader.ReadVector3();
                Helpers.TestValueUInt(reader, 0x0834F51B); // Always 1B F5 34 08.
                Helpers.TestValueUInt(reader, 0x00000000); // Always 0x00.
                Helpers.TestValueUInt(reader, 0x0A41ADC9); // Always C9 AD 41 0A.
                string instanceName3 = reader.ReadNullTerminatedString(); // Always the same as instanceName.
                if (instanceName3 != instance.Name)
                    Debugger.Break();
                Helpers.TestValueUInt(reader, 0x04843AA8); // Always A8 3A 84 04.
                instance.UnknownUInt32_7 = reader.ReadUInt32();
                Helpers.TestValueUInt(reader, 0xCDEF0008); // Always 08 00 EF CD.
                Helpers.TestValueUInt(reader, 0xCB8512AB); // Always AB 12 85 CB.
                Helpers.TestValueUInt(reader, 0x1A890834); // Always 34 08 89 1A.
                instance.UnknownUInt32_8 = reader.ReadUInt32();
                if (instance.UnknownUInt32_8 == 0x77990F47)
                {
                    Helpers.TestValueUShort(reader, 0x03E3); // Always E3 03
                }
                else
                {
                    Helpers.TestValueUShort(reader, 0x12AB); // Always AB 12
                    Helpers.TestValueUInt(reader, 0x023B068E); // Always 8E 06 3B 02.
                    instance.UnknownUInt32_9 = reader.ReadUInt32();
                    instance.UnknownUInt32_10 = reader.ReadUInt32();

                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        reader.JumpAhead(0x27);
                        Data.Subspace.HasDragChunk = true;
                    }
                }
                Data.Subspace.Instances.Add(instance);
            }
        }

        public void ExportOBJ(string directory)
        {
            Directory.CreateDirectory(directory);

            foreach (var model in Data.Models)
            {
                int? instanceIndex = null;
                for (int i = 0; i < Data.Subspace.Instances.Count; i++)
                {
                    if (Data.Subspace.Instances[i].Name == model.Name)
                        instanceIndex = i;
                }

                if (instanceIndex == null)
                {
                    Debugger.Break();
                }

                StreamWriter mat = new($@"{directory}\{model.Name}.obj");

                foreach (var vertex in model.Vertices)
                    mat.WriteLine($"v {vertex.X + Data.Subspace.Instances[(int)instanceIndex].Position.X} {vertex.Y + Data.Subspace.Instances[(int)instanceIndex].Position.Y} {vertex.Z + Data.Subspace.Instances[(int)instanceIndex].Position.Z}");

                mat.WriteLine($"g {model.Name}");
                mat.WriteLine($"o {model.Name}");

                foreach (var face in model.Faces)
                    mat.WriteLine($"f {face.IndexA + 1} {face.IndexB + 1} {face.IndexC + 1}");

                mat.Close();
            }
        }
    }
}