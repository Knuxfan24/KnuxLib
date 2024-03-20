namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out the unknown values.
    public class DensitySetting : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DensitySetting() { }
        public DensitySetting(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.densitysetting.json", this);
        }

        // Classes for this format.
        public class DensityModel
        {
            public string modelName { get; set; } = "";
            public UInt32 UnknownUInt_0 { get; set; }
            public UInt32 UnknownUInt_1 { get; set; }
        }

        public class UnknownStruct_0
        {
            public short UnknownShort_0 { get; set; }
            public short UnknownShort_1 { get; set; }
            public short UnknownShort_2 { get; set; }
            public short UnknownShort_3 { get; set; }
            public UInt32 UnknownUInt_0 { get; set; }
            public UInt32 UnknownUInt_1 { get; set; }
            public UInt32 UnknownUInt_2 { get; set; }
            public UInt32 UnknownUInt_3 { get; set; }
            public UInt32 UnknownUInt_4 { get; set; }
            public UInt32 UnknownUInt_5 { get; set; }
            public float UnknownFloat_0 { get; set; }
            public float UnknownFloat_1 { get; set; }
            public float UnknownFloat_2 { get; set; }
            public float UnknownFloat_3 { get; set; }
            public float UnknownFloat_4 { get; set; }
            public float UnknownFloat_5 { get; set; }
            public float UnknownFloat_6 { get; set; }
            public byte UnknownByte_0 { get; set; }
            public byte UnknownByte_1 { get; set; }
            public byte UnknownByte_2 { get; set; }
            public byte UnknownByte_3 { get; set; }
        }

        public class UnknownStruct_1
        {
            public UInt32 UnknownUInt_0 { get; set; }
            public float UnknownFloat_0 { get; set; }
            public UInt32 UnknownUInt_1 { get; set; }
            public UInt32 UnknownUInt_2 { get; set; }
        }
        
        public class UnknownStruct_2
        {
            public UInt32 UnknownUInt_0 { get; set; }
            public UInt32 UnknownUInt_1 { get; set; }
            public UInt32 UnknownUInt_2 { get; set; }
            public UInt32 UnknownUInt_3 { get; set; }
        }

        public class UnknownStruct_3
        {
            public UInt32 UnknownUInt_0 { get; set; }
            public UInt32 UnknownUInt_1 { get; set; }
            public UInt32 UnknownUInt_2 { get; set; }
            public UInt32 UnknownUInt_3 { get; set; }
            public float UnknownFloat_0 { get; set; }
            public float UnknownFloat_1 { get; set; }
            public short UnknownShort_0 { get; set; }
            public short UnknownShort_1 { get; set; }
            public byte UnknownByte_0 { get; set; }
            public byte UnknownByte_1 { get; set; }
            public byte UnknownByte_2 { get; set; }
            public byte UnknownByte_3 { get; set; }
            public UInt32 UnknownUInt_4 { get; set; }
            public UInt32 UnknownUInt_5 { get; set; }
            public UInt32 UnknownUInt_6 { get; set; }
            public UInt32 UnknownUInt_7 { get; set; }
        }

        public class IDItem
        {
            public string Name { get; set; } = "";
            public UInt32 ID { get; set; }
            public UInt32 UnknownUInt_0 { get; set; }
            public float UnknownFloat_0 { get; set; }
            public float UnknownFloat_1 { get; set; }
        }

        public class CollisionItem
        {
            public string Name { get; set; } = "";
        }

        // Actual data presented to the end user.
        public float WorldSizeX;
        public float WorldSizeY;
        public UInt32 Count; // Still not sure on this one
        public string AreaMap;
        public string LayerMap;
        public string ColorMap;
        public string ScaleMap;
        public List<DensityModel> DensityModels = new();
        public List<UnknownStruct_0> UnknownStructs_0 = new();
        public List<UnknownStruct_1> UnknownStructs_1 = new();
        public List<UnknownStruct_2> UnknownStructs_2 = new();
        public List<UnknownStruct_3> UnknownStructs_3 = new();
        public List<IDItem> IDList = new();
        /*public List<UnknownStruct_3> UnknownStructs_3 = new();*/ // This is another list of unknown structs, more info can be found in this template https://github.com/Ashrindy/AshDump/blob/main/010Templates/Sonic/Sonic%20Frontiers/ResDensitySetting.bt
        public List<CollisionItem> CollisionData = new();


        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "GSDC";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            reader.JumpAhead(4);

            // Get the world size (I would use vectors, but JSON doesn't support them)
            WorldSizeX = reader.ReadSingle();
            WorldSizeY = reader.ReadSingle();

            // Jump ahead an unknown value
            reader.JumpAhead(4);

            // Not 100% sure what this is, but it's mostly similar to the count of the models
            Count = reader.ReadUInt32();

            // Jump ahead 32 UInt32's and 32 floats
            reader.JumpAhead(256);

            // Read all of the maps that the density uses
            AreaMap = Helpers.ReadNullTerminatedStringTableEntry(reader);
            LayerMap = Helpers.ReadNullTerminatedStringTableEntry(reader);
            ColorMap = Helpers.ReadNullTerminatedStringTableEntry(reader);
            ScaleMap = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Read the density model names? I'm not sure what they really are.
            long StartOffset = reader.ReadInt64();
            long Amount = reader.ReadInt64();

            long PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for(int i = 0; i < Amount; i++)
            {
                DensityModel densityModel = new()
                {
                    modelName = Helpers.ReadNullTerminatedStringTableEntry(reader),
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownUInt_1 = reader.ReadUInt32(),
                };

                DensityModels.Add(densityModel);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Read an Unknown Struct 0
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                UnknownStruct_0 unkStr_0 = new()
                {
                    UnknownShort_0 = reader.ReadInt16(),
                    UnknownShort_1 = reader.ReadInt16(),
                    UnknownShort_2 = reader.ReadInt16(),
                    UnknownShort_3 = reader.ReadInt16(),
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownUInt_1 = reader.ReadUInt32(),
                    UnknownUInt_2 = reader.ReadUInt32(),
                    UnknownUInt_3 = reader.ReadUInt32(),
                    UnknownUInt_4 = reader.ReadUInt32(),
                    UnknownUInt_5 = reader.ReadUInt32(),
                    UnknownFloat_0 = reader.ReadSingle(),
                    UnknownFloat_1 = reader.ReadSingle(),
                    UnknownFloat_2 = reader.ReadSingle(),
                    UnknownFloat_3 = reader.ReadSingle(),
                    UnknownFloat_4 = reader.ReadSingle(),
                    UnknownFloat_5 = reader.ReadSingle(),
                    UnknownFloat_6 = reader.ReadSingle(),
                    UnknownByte_0 = reader.ReadByte(),
                    UnknownByte_1 = reader.ReadByte(),
                    UnknownByte_2 = reader.ReadByte(),
                    UnknownByte_3 = reader.ReadByte(),
                };

                UnknownStructs_0.Add(unkStr_0);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Read an Unknown Struct 1
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                UnknownStruct_1 unkStr_1 = new()
                {
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownFloat_0 = reader.ReadSingle(),
                    UnknownUInt_1 = reader.ReadUInt32(),
                    UnknownUInt_2 = reader.ReadUInt32(),
                };

                UnknownStructs_1.Add(unkStr_1);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Read an Unknown Struct 2
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                UnknownStruct_2 unkStr_2 = new()
                {
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownUInt_1 = reader.ReadUInt32(),
                    UnknownUInt_2 = reader.ReadUInt32(),
                    UnknownUInt_3 = reader.ReadUInt32(),
                };

                UnknownStructs_2.Add(unkStr_2);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Read an Unknown Struct 3
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                UnknownStruct_3 unkStr_3 = new()
                {
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownUInt_1 = reader.ReadUInt32(),
                    UnknownUInt_2 = reader.ReadUInt32(),
                    UnknownUInt_3 = reader.ReadUInt32(),
                    UnknownFloat_0 = reader.ReadSingle(),
                    UnknownFloat_1 = reader.ReadSingle(),
                    UnknownShort_0 = reader.ReadInt16(),
                    UnknownShort_1 = reader.ReadInt16(),
                    UnknownByte_0 = reader.ReadByte(),
                    UnknownByte_1 = reader.ReadByte(),
                    UnknownByte_2 = reader.ReadByte(),
                    UnknownByte_3 = reader.ReadByte(),
                    UnknownUInt_4 = reader.ReadUInt32(),
                    UnknownUInt_5 = reader.ReadUInt32(),
                    UnknownUInt_6 = reader.ReadUInt32(),
                    UnknownUInt_7 = reader.ReadUInt32(),
                };

                UnknownStructs_3.Add(unkStr_3);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Read the ID List
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                IDItem idItem = new()
                {
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader),
                    ID = reader.ReadUInt32(),
                    UnknownUInt_0 = reader.ReadUInt32(),
                    UnknownFloat_0 = reader.ReadSingle(),
                    UnknownFloat_1 = reader.ReadSingle(),
                };

                IDList.Add(idItem);
            }

            // Jump back to the offset table
            reader.JumpTo(PrePos, true);

            // Skip the Unknown Struct Pointer and amount that's so far unnecessary
            reader.JumpAhead(16);

            // Read the Collision Data
            StartOffset = reader.ReadInt64();
            Amount = reader.ReadInt64();

            PrePos = reader.BaseStream.Position;

            reader.JumpTo(StartOffset, false);

            for (int i = 0; i < Amount; i++)
            {
                CollisionItem collisionItem = new()
                {
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader),
                };

                CollisionData.Add(collisionItem);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
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


            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
