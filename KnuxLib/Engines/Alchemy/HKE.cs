using System.Diagnostics;

namespace KnuxLib.Engines.Alchemy
{
    // TODO: What does HKE mean?
    // TODO: Finish reading, don't commit until there's SOMETHING functional.
    public class HKE : FileBase
    {
        public class Model
        {
            public string Name { get; set; } = "";

            public List<Vector3> Vertices { get; set; } = new();

            public List<ModelFace> Faces { get; set; } = new();
        }

        public class ModelFace
        {
            public uint IndexA { get; set; }

            public uint IndexB { get; set; }

            public uint IndexC { get; set; }
        }

        public List<Model> Data = new();

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
            float UnknownFloat_1 = reader.ReadSingle(); // Always a 1, except in two files, where it's 1.000002, rounding error?
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
                uint UnknownUInt32_1 = reader.ReadUInt32();
                uint UnknownUInt32_2 = reader.ReadUInt32();
                uint UnknownUInt32_3 = reader.ReadUInt32();
                uint UnknownUInt32_4 = reader.ReadUInt32();

                // Save this model.
                Data.Add(model);
            }

            // Instance data?
            string instanceName = reader.ReadNullTerminatedString(); // Always default_subspace.
            reader.JumpAhead(0x04); // Always D9 AE 66 0C.
            reader.JumpAhead(0x08); // Always 0x00.
            float UnknownFloat_2 = reader.ReadSingle();
            reader.JumpAhead(0x04); // Always 35 1B A6 00.
            reader.JumpAhead(0x04); // Always CD CC CC 3D.
            reader.JumpAhead(0x04); // Always A2 B2 D2 01.
            reader.JumpAhead(0x04); // Always 01 A9 5C C3.
            reader.JumpAhead(0x04); // Always 00 00 00 A0.
            reader.JumpAhead(0x04); // Always 40 D9 B3 AB.
            float UnknownFloat_3 = reader.ReadSingle();
            uint UnknownUInt32_5 = reader.ReadUInt32();
            reader.JumpAhead(0x04); // Always 0A EE 2A 88.
            reader.JumpAhead(0x04); // Always 0C FE A7 DF.
            reader.JumpAhead(0x04); // Always 0E 6E 0A DC.
            reader.JumpAhead(0x01); // Always 0x07.
            string collectionName = reader.ReadNullTerminatedString();
            reader.JumpAhead(0x04); // Always 12 22 87 04.
            reader.JumpAhead(0x04); // Always C2 C2 4C 00.
            reader.JumpAhead(0x04); // Always 25 D0 CD 02.
            reader.JumpAhead(0x04); // Always CD CC CC 3D.
            reader.JumpAhead(0x04); // Always 99 77 E3 03.
        }

        public void ExportOBJ(string directory)
        {
            foreach (var model in Data)
            {
                StreamWriter mat = new($@"{directory}\{model.Name}.obj");

                foreach (var vertex in model.Vertices)
                    mat.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                mat.WriteLine($"g {model.Name}");
                mat.WriteLine($"o {model.Name}");

                foreach (var face in model.Faces)
                    mat.WriteLine($"f {face.IndexA + 1} {face.IndexB + 1} {face.IndexC + 1}");

                mat.Close();
            }
        }
    }
}
