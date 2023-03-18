namespace KnuxLib.Engines.Alchemy
{
    // TODO: What does HKE mean?
    // TODO: Figure out all of the unknown values.
    // TODO: Figure out how the Subspace Data pieces the models together, one arena looked right with the value that I assume is position, another didn't.
    // TODO: Make a proper exporter for this.
    // TODO: Make an importer for this.
    // TODO: Make a save function for this.
    public class Collision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision() { }
        public Collision(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
            {
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.alchemy.collision.json", Data);
                ExportOBJTemp($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
            }
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// In all files besides barin2 and teknee2 (and a prototype's CTR tracks) this is a 1, these files have 1.000002 for some reason.
            /// </summary>
            public float UnknownFloat_1 { get; set; } = 1f;

            /// <summary>
            /// A list of the model data for this collision file.
            /// </summary>
            public List<Model> Models { get; set; } = new();

            /// <summary>
            /// The subspace chunk that makes up the back half of an Alchemy Engine collision file.
            /// </summary>
            public Subspace Subspace { get; set; } = new();
        }

        public class Model
        {
            /// <summary>
            /// The name of this model.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The coordinates for the various vertices that make up this model.
            /// </summary>
            public List<Vector3> Vertices { get; set; } = new();

            /// <summary>
            /// The faces that make up this model.
            /// </summary>
            public List<Face> Faces { get; set; } = new();

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0x019FEEA9 or 0x0310E1B5.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0x05053045 or 0xC6059501.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            public override string ToString() => Name;
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

        public class Subspace
        {
            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Seems to only have four values that are all versions of 9.8 (9.8, -9.8, -9.80002, -9.800026).
            /// </summary>
            public float UnknownFloat_1 { get; set; } = 9.8f;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0xCCCCCD07 or 0xD1B71707.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0x841FC238 or 0x841FC23D.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// The name of this subspace collection, usually RBCollection01 but not always.
            /// </summary>
            public string CollectionName { get; set; } = "RBCollection01";

            /// <summary>
            /// Whether this subspace has the Drag_1 chunk at the end that is always the same sequence of bytes (44 72 61 67 5F 31 00 C7 74 33 06 CD CC CC 3D 57 5C 21 02 CD CC CC 3D EF CD AB 12 9E 30 5C 03 F5 E1 DA 0D 24 0D 4C 0A).
            /// </summary>
            public bool HasDragChunk { get; set; } = false;

            /// <summary>
            /// A list of the instance data in this subspace chunk.
            /// TODO: Instance feels like the wrong name for this data?
            /// </summary>
            public List<Instance> Instances { get; set; } = new();

            public override string ToString() => CollectionName;
        }

        public class Instance
        {
            /// <summary>
            /// The name of this instance.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Seems to only ever be 0 or 0.3
            /// </summary>
            public float UnknownFloat_1 { get; set; } = 0f;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Seems to only ever be 0 or 0.3, but not specifically the same as UnknownFloat_1.
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// The position of this instance in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0x9D340004 in the final.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0x000001F8 in the final.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0 in the final.
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0 in the final.
            /// </summary>
            public uint UnknownUInt32_4 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0x8D550000 in the final.
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0 in the final.
            /// </summary>
            public uint UnknownUInt32_6 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0xF80D2400 or 0xF80D2401.
            /// </summary>
            public uint UnknownUInt32_7 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?  Seems to only ever be 0x77990F47 or 0xCDEF0F47, with the second option being the one that leads to UnknownUInt32_9 and UnknownUInt32_10 being filled in.
            /// </summary>
            public uint UnknownUInt32_8 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only appear in instances where UnknownUInt32_8 is NOT 0x77990F47 and can only be 0x0731347E or 0x0DDAE1F5.
            /// </summary>
            public uint? UnknownUInt32_9 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only appear in instances where UnknownUInt32_8 is NOT 0x77990F47 and can only be 0x0004B877 or 0x0A4C0D24.
            /// </summary>
            public uint? UnknownUInt32_10 { get; set; }

            public override string ToString() => Name;
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

            // Skip the first 0x24 bytes of this file, as they're always the same sequence of bytes (42 E4 DE 0E 04 4D 41 58 5F 65 78 70 6F 72 74 65 64 5F 77 6F 72 6C 64 00 6E 7E A7 0A 50 05 00 00 05 33 1B 0A).
            reader.JumpAhead(0x24);

            // Read the main data's unknown floating point value.
            Data.UnknownFloat_1 = reader.ReadSingle();

            // Skip eight bytes that are always the same (A9 EE 9F 01 45 30 05 05).
            reader.JumpAhead(0x08);

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
                    Face face = new()
                    {
                        IndexA = reader.ReadUInt32(),
                        IndexB = reader.ReadUInt32(),
                        IndexC = reader.ReadUInt32()
                    };
                    model.Faces.Add(face);
                }

                // Skip an unknown value of 0x12ABCDEF.
                reader.JumpAhead(0x04);

                // Skip an unknown value of 0x0EA30AC9.
                reader.JumpAhead(0x04);

                // Read this model's first unknown value.
                model.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this model's second unknown value.
                model.UnknownUInt32_2 = reader.ReadUInt32();

                // Save this model.
                Data.Models.Add(model);
            }

            // Read the default_subspace string.
            string subspaceName = reader.ReadNullTerminatedString();

            // Skip 0x0C bytes that are always the same (D9 AE 66 0C 00 00 00 00 00 00 00 00).
            reader.JumpAhead(0x0C);

            // Read this subspace's first unknown floating point value.
            Data.Subspace.UnknownFloat_1 = reader.ReadSingle();

            // Skip 0x18 bytes that are always the same (35 1B A6 00 CD CC CC 3D A2 B2 D2 01 01 A9 5C C3 00 00 00 A0 40 D9 B3 AB).
            reader.JumpAhead(0x18);

            // Read this subspace's first unknown integer value.
            Data.Subspace.UnknownUInt32_1 = reader.ReadUInt32();

            // Read this subspace's second unknown integer value.
            Data.Subspace.UnknownUInt32_2 = reader.ReadUInt32();

            // Skip 0x0D bytes that are always the same (0A EE 2A 88 0C FE A7 DF 0E 6E 0A DC 07).
            reader.JumpAhead(0x0D);

            // Read this subspace's collection name.
            Data.Subspace.CollectionName = reader.ReadNullTerminatedString();

            // Skip 0x14 bytes that are always the same (12 22 87 04 C2 C2 4C 00 25 D0 CD 02 CD CC CC 3D 99 77 E3 03).
            reader.JumpAhead(0x14);

            // Loop until the end of the file to find the instance data.
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Set up a new instance.
                Instance instance = new();

                // Read this instance's name.
                instance.Name = reader.ReadNullTerminatedString();

                // Skip an unknown value of 0x08C76EF9.
                reader.JumpAhead(0x4);

                // Read this instance's first unknown floating point value.
                instance.UnknownFloat_1 = reader.ReadSingle();

                // Skip an unknown value of 0x0055670E.
                reader.JumpAhead(0x4);

                // Read this instance's second unknown floating point value.
                instance.UnknownFloat_2 = reader.ReadSingle();

                // Skip an unknown value of 0x00C1A1AE.
                reader.JumpAhead(0x4);

                // Skip an unknown value that is always the same as UnknownFloat_2.
                reader.JumpAhead(0x4);

                // Skip an unknown value of 0x0486894E.
                reader.JumpAhead(0x4);

                // Read this instance's third unknown floating point value.
                instance.UnknownFloat_3 = reader.ReadSingle();

                // Read this instance's first unknown Vector3.
                instance.UnknownVector3_1 = reader.ReadVector3();

                // Skip an unknown value of 0x085FEC0E.
                reader.JumpAhead(0x4);

                // Read this instance's position value.
                instance.Position = reader.ReadVector3();

                // Skip 0x14 bytes that are always the same (D9 4A A5 0C 00 00 00 00 00 00 00 00 00 00 00 00 F9 58 11 04).
                reader.JumpAhead(0x14);

                // Read this instance's second unknown Vector3.
                instance.UnknownVector3_2 = reader.ReadVector3();

                // Skip eight bytes that are always the same (24 0D F8 08 00 A5 8E 58).
                reader.JumpAhead(0x08);

                // Read this instance's first unknown integer value.
                instance.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this instance's second unknown integer value.
                instance.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this instance's third unknown integer value.
                instance.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this instance's fourth unknown integer value.
                instance.UnknownUInt32_4 = reader.ReadUInt32();

                // Read this instance's fifth unknown integer value.
                instance.UnknownUInt32_5 = reader.ReadUInt32();

                // Skip six bytes that are always the same (FA 07 85 54 73 01).
                reader.JumpAhead(0x06);

                // Read the second instance name, which is always the same as the first.
                string instanceName2 = reader.ReadNullTerminatedString();

                // Skip an unknown value of 0x00051683
                reader.JumpAhead(0x04);

                // Read this instance's sixth unknown integer value.
                instance.UnknownUInt32_6 = reader.ReadUInt32();

                // Skip 0x18 bytes that are always the same (4E 89 86 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0E EC 5F 08)
                reader.JumpAhead(0x18);

                // Read this instance's third unknown Vector3.
                instance.UnknownVector3_3 = reader.ReadVector3();

                // Skip 0x0C bytes that are always the same (1B F5 34 08 00 00 00 00 C9 AD 41 0A).
                reader.JumpAhead(0x0C);

                // Read the third instance name, which is always the same as the first.
                string instanceName3 = reader.ReadNullTerminatedString();

                // Skip an unknown value of 0x04843AA8.
                reader.JumpAhead(0x04);

                // Read this instance's seventh unknown integer value.
                instance.UnknownUInt32_7 = reader.ReadUInt32();

                // Skip 0x0C bytes that are always the same (08 00 EF CD AB 12 85 CB 34 08 89 1A).
                reader.JumpAhead(0x0C);

                // Read this instance's eighth unknown integer value.
                instance.UnknownUInt32_8 = reader.ReadUInt32();

                // Check the value of the eighth unknown integer, if it's 0x77990F47, then just skip ahead by two bytes (which are always 0x03E3).
                if (instance.UnknownUInt32_8 == 0x77990F47)
                    reader.JumpAhead(0x02);

                // If the value isn't 0x77990F47, then continue to read some extra data after it.
                // TODO: Is this data technically part of the subspace? As this might always be on the last instance?
                else
                {
                    // Skip six bytes that are always the same (AB 12 8E 06 3B 02).
                    reader.JumpAhead(0x06);

                    // Read this instance's ninth unknown integer value.
                    instance.UnknownUInt32_9 = reader.ReadUInt32();

                    // Read this instance's tenth unknown integer value.
                    instance.UnknownUInt32_10 = reader.ReadUInt32();

                    // If we're still not at the end of the file, then jump ahead by 0x27 bytes and mark this subspace as having the Drag_1 chunk.
                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        reader.JumpAhead(0x27);
                        Data.Subspace.HasDragChunk = true;
                    }
                }

                // Save this instance.
                Data.Subspace.Instances.Add(instance);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the first 0x24 bytes of this file that are always the same sequence of bytes.
            writer.Write(new byte[] { 0x42, 0xE4, 0xDE, 0x0E, 0x04, 0x4D, 0x41, 0x58, 0x5F, 0x65, 0x78, 0x70, 0x6F, 0x72, 0x74, 0x65, 0x64, 0x5F, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x00, 0x6E, 0x7E, 0xA7, 0x0A, 0x50, 0x05, 0x00, 0x00, 0x05, 0x33, 0x1B, 0x0A });

            // Write the main data's unknown floating point value.
            writer.Write(Data.UnknownFloat_1);

            // Write eight bytes that are always the same.
            writer.Write(new byte[] { 0xA9, 0xEE, 0x9F, 0x01, 0x45, 0x30, 0x05, 0x05 });

            // Loop through and write each model entry's data.
            for (int i = 0; i < Data.Models.Count; i++)
            {
                // Write this model's name.
                writer.WriteNullTerminatedString(Data.Models[i].Name);

                // Write this model's vertex count.
                writer.Write(Data.Models[i].Vertices.Count);

                // Write each vertex in this model.
                foreach (Vector3 vertex in Data.Models[i].Vertices)
                    writer.Write(vertex);

                // Write this model's face count.
                writer.Write(Data.Models[i].Faces.Count);

                // Write each of this model's face indices.
                foreach (Face face in Data.Models[i].Faces)
                {
                    writer.Write(face.IndexA);
                    writer.Write(face.IndexB);
                    writer.Write(face.IndexC);
                }

                // Write an unknown value of 0x12ABCDEF.
                writer.Write(0x12ABCDEF);

                // Write an unknown value of 0x0EA30AC9.
                writer.Write(0x0EA30AC9);

                // Write this model's first unknown value.
                writer.Write(Data.Models[i].UnknownUInt32_1);

                // Write this model's second unknown value.
                writer.Write(Data.Models[i].UnknownUInt32_2);
            }
            
            // Write the single null byte that marks the end of the model table.
            writer.WriteNull();

            // Write the default_subspace string.
            writer.WriteNullTerminatedString("default_subspace");

            // Write 0x0C bytes that are always the same.
            writer.Write(new byte[] { 0xD9, 0xAE, 0x66, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            // Write this subspace's first unknown floating point value.
            writer.Write(Data.Subspace.UnknownFloat_1);

            // Write 0x18 bytes that are always the same.
            writer.Write(new byte[] { 0x35, 0x1B, 0xA6, 0x00, 0xCD, 0xCC, 0xCC, 0x3D, 0xA2, 0xB2, 0xD2, 0x01, 0x01, 0xA9, 0x5C, 0xC3, 0x00, 0x00, 0x00, 0xA0, 0x40, 0xD9, 0xB3, 0xAB });

            // Write this subspace's first unknown integer value.
            writer.Write(Data.Subspace.UnknownUInt32_1);

            // Write this subspace's second unknown integer value.
            writer.Write(Data.Subspace.UnknownUInt32_2);

            // Write 0x0D bytes that are always the same.
            writer.Write(new byte[] { 0x0A, 0xEE, 0x2A, 0x88, 0x0C, 0xFE, 0xA7, 0xDF, 0x0E, 0x6E, 0x0A, 0xDC, 0x07 });

            // Write this subspace's collection name.
            writer.WriteNullTerminatedString(Data.Subspace.CollectionName);

            // Write 0x14 bytes that are always the same.
            writer.Write(new byte[] { 0x12, 0x22, 0x87, 0x04, 0xC2, 0xC2, 0x4C, 0x00, 0x25, 0xD0, 0xCD, 0x02, 0xCD, 0xCC, 0xCC, 0x3D, 0x99, 0x77, 0xE3, 0x03 });

            // Loop through and write the data for each instance in the subspace chunk.
            for (int i = 0; i < Data.Subspace.Instances.Count; i++)
            {
                // Write the first occurance of this instance's name.
                writer.WriteNullTerminatedString(Data.Subspace.Instances[i].Name);

                // Write an unknown value of 0x08C76EF9.
                writer.Write(0x08C76EF9);

                // Write this instance's first unknown floating point value.
                writer.Write(Data.Subspace.Instances[i].UnknownFloat_1);

                // Write an unknown value of 0x0055670E.
                writer.Write(0x0055670E);

                // Write this instance's second unknown floating point value.
                writer.Write(Data.Subspace.Instances[i].UnknownFloat_2);

                // Write an unknown value of 0x00C1A1AE.
                writer.Write(0x00C1A1AE);

                // Write this instance's second unknown floating point value a second time.
                writer.Write(Data.Subspace.Instances[i].UnknownFloat_2);

                // Write an unknown value of 0x0486894E.
                writer.Write(0x0486894E);

                // Write this instance's third unknown floating point value.
                writer.Write(Data.Subspace.Instances[i].UnknownFloat_3);

                // Write this instance's first unknown Vector3.
                writer.Write(Data.Subspace.Instances[i].UnknownVector3_1);

                // Write an unknown value of 0x085FEC0E.
                writer.Write(0x085FEC0E);

                // Write this instance's position value.
                writer.Write(Data.Subspace.Instances[i].Position);

                // Write 0x14 bytes that are always the same.
                writer.Write(new byte[] { 0xD9, 0x4A, 0xA5, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF9, 0x58, 0x11, 0x04 });

                // Write this instance's second unknown Vector3.
                writer.Write(Data.Subspace.Instances[i].UnknownVector3_2);

                // Write eight bytes that are always the same.
                writer.Write(new byte[] { 0x24, 0x0D, 0xF8, 0x08, 0x00, 0xA5, 0x8E, 0x58 });

                // Write this instance's first unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_1);

                // Write this instance's second unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_2);

                // Write this instance's third unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_3);

                // Write this instance's fourth unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_4);

                // Write this instance's fifth unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_5);

                // Write six bytes that are always the same.
                writer.Write(new byte[] { 0xFA, 0x07, 0x85, 0x54, 0x73, 0x01 });

                // Write the second occurance of this instance's name.
                writer.WriteNullTerminatedString(Data.Subspace.Instances[i].Name);

                // Write an unknown value of 0x00051683.
                writer.Write(0x00051683);

                // Write this instance's sixth unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_6);

                // Write 0x18 bytes that are always the same.
                writer.Write(new byte[] { 0x4E, 0x89, 0x86, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0xEC, 0x5F, 0x08 });

                // Write this instance's third unknown Vector3.
                writer.Write(Data.Subspace.Instances[i].UnknownVector3_3);

                // Write 0x0C bytes that are always the same.
                writer.Write(new byte[] { 0x1B, 0xF5, 0x34, 0x08, 0x00, 0x00, 0x00, 0x00, 0xC9, 0xAD, 0x41, 0x0A });

                // Write the third occurance of this instance's name.
                writer.WriteNullTerminatedString(Data.Subspace.Instances[i].Name);

                // Write an unknown value of 0x04843AA8.
                writer.Write(0x04843AA8);

                // Write this instance's seventh unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_7);

                // Write 0x0C bytes that are always the same.
                writer.Write(new byte[] { 0x08, 0x00, 0xEF, 0xCD, 0xAB, 0x12, 0x85, 0xCB, 0x34, 0x08, 0x89, 0x1A });

                // Write this instance's eighth unknown integer value.
                writer.Write(Data.Subspace.Instances[i].UnknownUInt32_8);

                // If this instance's eighth unknown integer value is 0x77990F47, then write two bytes that are always the same.
                if (Data.Subspace.Instances[i].UnknownUInt32_8 == 0x77990F47)
                    writer.Write((ushort)0x03E3);
                
                // If not, then continue to write some extra data.
                else
                {
                    // Write six bytes that are always the same.
                    writer.Write(new byte[] { 0xAB, 0x12, 0x8E, 0x06, 0x3B, 0x02 });

                    // Write this instance's ninth unknown integer value.
                    writer.Write((uint)Data.Subspace.Instances[i].UnknownUInt32_9);

                    // Write this instance's tenth unknown integer value.
                    writer.Write((uint)Data.Subspace.Instances[i].UnknownUInt32_10);
                }
            }

            // If this subspace chunk is marked as having the Drag01 chunk, then write the 0x27 bytes that form it.
            if (Data.Subspace.HasDragChunk)
                writer.Write(new byte[] { 0x44, 0x72, 0x61, 0x67, 0x5F, 0x31, 0x00, 0xC7, 0x74, 0x33, 0x06, 0xCD, 0xCC, 0xCC, 0x3D, 0x57, 0x5C, 0x21, 0x02, 0xCD, 0xCC, 0xCC, 0x3D, 0xEF, 0xCD, 0xAB, 0x12, 0x9E, 0x30, 0x5C, 0x03, 0xF5, 0xE1, 0xDA, 0x0D, 0x24, 0x0D, 0x4C, 0x0A });

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
            foreach (var model in Data.Models)
            {
                // Set up the instance index value for this model.
                int instanceIndex = 0;

                // Loop through each instance to find the right one for this model.
                for (int i = 0; i < Data.Subspace.Instances.Count; i++)
                    if (Data.Subspace.Instances[i].Name == model.Name)
                        instanceIndex = i;

                // Create a StreamWrite for this model.
                StreamWriter obj = new($@"{directory}\{model.Name}.obj");

                // Print the model we're exporting.
                Console.WriteLine($@"Exporting {directory}\{model.Name}.obj");

                // Write each vertex with the position value from the instance added to it.
                foreach (var vertex in model.Vertices)
                    obj.WriteLine($"v {vertex.X + Data.Subspace.Instances[(int)instanceIndex].Position.X} {vertex.Y + Data.Subspace.Instances[(int)instanceIndex].Position.Y} {vertex.Z + Data.Subspace.Instances[(int)instanceIndex].Position.Z}");

                // Write the object name for this model.
                obj.WriteLine($"g {model.Name}");
                obj.WriteLine($"o {model.Name}");

                // Write each face for this model, with the indices incremented by 1 due to OBJ counting from 1 not 0.
                foreach (var face in model.Faces)
                    obj.WriteLine($"f {face.IndexA + 1} {face.IndexB + 1} {face.IndexC + 1}");

                // Close the StreamWriter.
                obj.Close();
            }
        }
    }
}