using System.Diagnostics;

namespace KnuxLib.Engines.Hedgehog
{
    public class Gismo_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Gismo_2013() { }
        public Gismo_2013(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.gismo_2013.json", Data);
        }

        // Classes for this format.
        public class Gismo
        {
            public string Name { get; set; } = "";

            public string Model { get; set; } = "";

            public string? Havok { get; set; }

            public float[] UnknownFloatArray_1 { get; set; } = Array.Empty<float>();

            public KeyValuePair<uint, string>? UnknownUIntXString_1 { get; set; }
        }

        // Actual data presented to the end user.
        public List<Gismo> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(200);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            if (Helpers.ReadNullTerminatedStringTableEntry(reader, false) != "GISM") Debugger.Break();

            if (reader.ReadUInt32() != 0x01) Debugger.Break();

            uint gismoCount = reader.ReadUInt32();

            uint gismoTableOffset = reader.ReadUInt32();

            reader.JumpTo(gismoTableOffset, false);

            for (int gismoIndex = 0; gismoIndex < gismoCount; gismoIndex++)
            {
                Gismo gismo = new();

                gismo.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                gismo.Model = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                string havok = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                if (havok != "")
                    gismo.Havok = havok;

                uint unknownFloatArray_1_Count = reader.ReadUInt32();

                switch (unknownFloatArray_1_Count)
                {
                    case 0:
                        if (reader.ReadUInt32() != 0) Debugger.Break();
                        if (reader.ReadUInt32() != 0) Debugger.Break();
                        break;

                    case 1:
                        gismo.UnknownFloatArray_1 = new float[1];
                        gismo.UnknownFloatArray_1[0] = reader.ReadSingle();
                        if (reader.ReadUInt32() != 0) Debugger.Break();
                        break;

                    case 2:
                        gismo.UnknownFloatArray_1 = new float[2];
                        gismo.UnknownFloatArray_1[0] = reader.ReadSingle();
                        gismo.UnknownFloatArray_1[1] = reader.ReadSingle();
                        break;
                }

                bool bool_1 = reader.ReadUInt32() == 0x01;
                uint int_2 = reader.ReadUInt32();
                bool bool_2 = reader.ReadUInt32() == 0x01;
                uint int_3 = reader.ReadUInt32();

                long position = reader.BaseStream.Position;

                if (bool_1)
                {
                    reader.JumpTo(int_2, false);
                    gismo.UnknownUIntXString_1 = new(reader.ReadUInt32(), Helpers.ReadNullTerminatedStringTableEntry(reader, false));
                }

                if (bool_2)
                {
                    reader.JumpTo(int_3, false);
                    if (reader.ReadUInt32() != 0x01) Debugger.Break();
                    uint int_4 = reader.ReadUInt32();
                    Vector3 test = Helpers.ReadHedgeLibVector3(reader);
                    Vector3 test2 = Helpers.ReadHedgeLibVector3(reader);
                }

                reader.JumpTo(position);
            }
        }
    }
}
