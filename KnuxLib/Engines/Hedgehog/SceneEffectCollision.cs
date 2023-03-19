namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Tidy all of this up and document how it works.
    // TODO: Figure out how the values are actually structured, as there's a lot of temporary solutions here that aren't correct.
    // TOOD: Figure out what the values are for.
    public class SceneEffectCollision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SceneEffectCollision() { }
        public SceneEffectCollision(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".fxcol", "")}.hedgehog.sceneeffectcollision.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public List<EffectVisiblityShape> Data1 { get; set; } = new();

            public List<UnknownStruct_1> Data2 { get; set; } = new();

            public List<ulong> Data3 { get; set; } = new();
        }

        public class EffectVisiblityShape
        {
            public string Name { get; set; } = "";

            public uint UnknownUInt32_1 { get; set; }

            public Vector3 UnknownVector3_1 { get; set; }

            public Vector3 UnknownVector3_2 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public Vector3 UnknownVector3_3 { get; set; }

            public Quaternion Rotation { get; set; }

            public override string ToString() => Name;
        }

        public class UnknownStruct_1
        {
            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public Vector3 UnknownVector3_1 { get; set; } // AABB pt1?

            public Vector3 UnknownVector3_2 { get; set; } // AABB pt2?
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "OCXF";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            ulong count1 = reader.ReadUInt64();
            long offset1 = reader.ReadInt64();

            ulong count2 = reader.ReadUInt64();
            long offset2 = reader.ReadInt64();

            ulong count3 = reader.ReadUInt64();
            long offset3 = reader.ReadInt64();

            reader.JumpTo(offset1, false);
            for (ulong i = 0; i < count1; i++)
            {
                EffectVisiblityShape shape = new();
                shape.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);
                shape.UnknownUInt32_1 = reader.ReadUInt32();
                shape.UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);
                shape.UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);
                shape.UnknownUInt32_2 = reader.ReadUInt32();
                shape.UnknownUInt32_3 = reader.ReadUInt32();
                reader.JumpAhead(0x1C);
                reader.JumpAhead(0x08); // none string.
                shape.UnknownVector3_3 = Helpers.ReadHedgeLibVector3(reader);
                shape.Rotation = Helpers.ReadHedgeLibQuaternion(reader);
                reader.JumpAhead(0x04); // 0, doesn't exist in last one.
                Data.Data1.Add(shape);
            }

            // Definitely not a ulong.
            reader.JumpTo(offset3, false);
            for (ulong i = 0; i < count3; i++)
                Data.Data3.Add(reader.ReadUInt64());

            reader.JumpTo(offset2, false);
            for (ulong i = 0; i < count2; i++)
            {
                UnknownStruct_1 unknown = new();
                unknown.UnknownUInt32_1 = reader.ReadUInt32();
                unknown.UnknownUInt32_2 = reader.ReadUInt32();
                unknown.UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);
                unknown.UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);
                Data.Data2.Add(unknown);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this file's signature.
            writer.WriteSignature(Signature);

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            writer.Write((ulong)Data.Data1.Count);

            writer.AddOffset("data1Offset", 0x08);

            writer.Write((ulong)Data.Data2.Count);

            writer.AddOffset("data2Offset", 0x08);

            writer.Write((ulong)Data.Data3.Count);

            writer.AddOffset("data3Offset", 0x08);

            writer.FillInOffset("data1Offset", false, false);

            for (int i = 0; i < Data.Data1.Count; i++)
            {
                writer.AddString($"shape{i}name", Data.Data1[i].Name, 0x08);

                writer.Write(Data.Data1[i].UnknownUInt32_1);

                Helpers.WriteHedgeLibVector3(writer, Data.Data1[i].UnknownVector3_1);

                Helpers.WriteHedgeLibVector3(writer, Data.Data1[i].UnknownVector3_2);

                writer.Write(Data.Data1[i].UnknownUInt32_2);

                writer.Write(Data.Data1[i].UnknownUInt32_3);

                writer.WriteNulls(0x1C);

                writer.AddString($"shape{i}none", "none", 0x08);

                Helpers.WriteHedgeLibVector3(writer, Data.Data1[i].UnknownVector3_3);

                Helpers.WriteHedgeLibQuaternion(writer, Data.Data1[i].Rotation);

                if (i < Data.Data1.Count - 1)
                    writer.Write(0x00);
            }

            writer.FillInOffset("data3Offset", false, false);

            for (int i = 0; i < Data.Data3.Count; i++)
                writer.Write(Data.Data3[i]);

            writer.FillInOffset("data2Offset", false, false);

            for (int i = 0; i < Data.Data2.Count; i++)
            {
                writer.Write(Data.Data2[i].UnknownUInt32_1);

                writer.Write(Data.Data2[i].UnknownUInt32_2);

                Helpers.WriteHedgeLibVector3(writer, Data.Data2[i].UnknownVector3_1);

                Helpers.WriteHedgeLibVector3(writer, Data.Data2[i].UnknownVector3_2);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
