using System;
using System.Diagnostics;

namespace KnuxLib.Engines.Hedgehog
{
    public class DensitySettingOld : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DensitySettingOld() { }
        public DensitySettingOld(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.densitysetting.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public Vector2 UnknownVector2_1 { get; set; }

            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint[] UnknownUInt32Array_1 { get; set; } = new uint[31];

            public float[] UnknownFloatArray_1 { get; set; } = new float[32];

            public string UnknownString_1 { get; set; } = "";

            public string UnknownString_2 { get; set; } = "";

            public string UnknownString_3 { get; set; } = "";

            public string UnknownString_4 { get; set; } = "";

            public Struct1[] Struct1s { get; set; } = Array.Empty<Struct1>();

            public Struct2[] Struct2s { get; set; } = Array.Empty<Struct2>();

            public Struct3[] Struct3s { get; set; } = Array.Empty<Struct3>();

            public Struct4[] Struct4s { get; set; } = Array.Empty<Struct4>();

            public Struct5[] Struct5s { get; set; } = Array.Empty<Struct5>();

            public Struct6[] Struct6s { get; set; } = Array.Empty<Struct6>();

            public Struct7[] Struct7s { get; set; } = Array.Empty<Struct7>();

            public string[] Collisions { get; set; } = Array.Empty<string>();

            public uint[] Struct8s { get; set; } = Array.Empty<uint>();

            public uint[][] Struct9s { get; set; } = new uint[5][];

            public string[] Struct10s { get; set; } = Array.Empty<string>();
        }

        public class Struct1
        {
            public string Name { get; set; } = "";

            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public override string ToString() => Name;
        }

        public class Struct2
        {
            public uint UnknownUInt32_1 { get; set; }

            public ushort UnknownUInt16_1 { get; set; }

            public ushort UnknownUInt16_2 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public uint UnknownUInt32_5 { get; set; }

            public uint UnknownUInt32_6 { get; set; }

            public uint UnknownUInt32_7 { get; set; }

            public float UnknownFloat_1 { get; set; }

            public uint UnknownUInt32_8 { get; set; }
        }

        public class Struct3
        {
            public uint UnknownUInt32_1 { get; set; }

            public float UnknownFloat_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }
        }

        public class Struct4
        {
            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }
        }

        public class Struct5
        {
            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public float UnknownFloat_1 { get; set; }

            public float UnknownFloat_2 { get; set; }

            public uint UnknownUInt32_5 { get; set; }

            public uint UnknownUInt32_6 { get; set; }

            public uint UnknownUInt32_7 { get; set; }

            public uint UnknownUInt32_8 { get; set; }

            public uint UnknownUInt32_9 { get; set; }

            public uint UnknownUInt32_10 { get; set; }
        }

        public class Struct6
        {
            public string UnknownString_1 { get; set; } = "";

            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public override string ToString() => UnknownString_1;
        }

        public class Struct7
        {
            public uint UnknownUInt32_1 { get; set; }

            public uint UnknownUInt32_2 { get; set; }

            public uint UnknownUInt32_3 { get; set; }

            public uint UnknownUInt32_4 { get; set; }

            public uint UnknownUInt32_5 { get; set; }

            public uint UnknownUInt32_6 { get; set; }

            public float UnknownFloat_1 { get; set; }

            public float UnknownFloat_2 { get; set; }

            public uint UnknownUInt32_7 { get; set; }

            public uint UnknownUInt32_8 { get; set; }

            public uint UnknownUInt32_9 { get; set; }

            public uint UnknownUInt32_10 { get; set; }

            public uint UnknownUInt32_11 { get; set; }

            public uint UnknownUInt32_12 { get; set; }

            public uint UnknownUInt32_13 { get; set; }

            public uint UnknownUInt32_14 { get; set; }

            public int UnknownInt32_1 { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

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

            if (reader.ReadUInt32() != 0x0B) Debugger.Break();
            Data.UnknownVector2_1 = new(reader.ReadSingle(), reader.ReadSingle());

            Data.UnknownUInt32_1 = reader.ReadUInt32();
            Data.UnknownUInt32_2 = reader.ReadUInt32();
            Data.UnknownUInt32_3 = reader.ReadUInt32();

            for (int unknownIndex = 0; unknownIndex < 31; unknownIndex++)
                Data.UnknownUInt32Array_1[unknownIndex] = reader.ReadUInt32();

            for (int unknownIndex = 0; unknownIndex < 32; unknownIndex++)
                Data.UnknownFloatArray_1[unknownIndex] = reader.ReadSingle();

            Data.UnknownString_1 = Helpers.ReadNullTerminatedStringTableEntry(reader);
            Data.UnknownString_2 = Helpers.ReadNullTerminatedStringTableEntry(reader);
            Data.UnknownString_3 = Helpers.ReadNullTerminatedStringTableEntry(reader);
            Data.UnknownString_4 = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Names (List5 u1)
            long namesOffset = reader.ReadInt64();
            ulong namesCount = reader.ReadUInt64();

            Data.Struct1s = new Struct1[namesCount];

            // Save our current position.
            long position = reader.BaseStream.Position;

            reader.JumpTo(namesOffset, false);

            for (ulong nameIndex = 0; nameIndex < namesCount; nameIndex++)
            {
                Data.Struct1s[nameIndex] = new()
                {
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader),
                    UnknownUInt32_1 = reader.ReadUInt32(),
                    UnknownUInt32_2 = reader.ReadUInt32()
                };
            }

            reader.JumpTo(position);

            // List5 u2
            long u2Offset = reader.ReadInt64();
            ulong u2Count = reader.ReadUInt64();

            Data.Struct2s = new Struct2[u2Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u2Offset, false);

            for (ulong u2Index = 0; u2Index < u2Count; u2Index++)
            {
                Struct2 u2 = new();

                u2.UnknownUInt32_1 = reader.ReadUInt32();
                u2.UnknownUInt16_1 = reader.ReadUInt16();
                u2.UnknownUInt16_2 = reader.ReadUInt16();
                u2.UnknownUInt32_2 = reader.ReadUInt32();
                u2.UnknownUInt32_3 = reader.ReadUInt32();
                u2.UnknownUInt32_4 = reader.ReadUInt32();
                u2.UnknownUInt32_5 = reader.ReadUInt32();
                u2.UnknownUInt32_6 = reader.ReadUInt32();
                u2.UnknownUInt32_7 = reader.ReadUInt32();
                u2.UnknownFloat_1 = reader.ReadSingle();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadSingle() != 1) Debugger.Break();
                if (reader.ReadSingle() != 1) Debugger.Break();
                if (reader.ReadSingle() != 1) Debugger.Break();
                u2.UnknownUInt32_8 = reader.ReadUInt32();

                Data.Struct2s[u2Index] = u2;
            }

            reader.JumpTo(position);

            // List5 u3
            long u3Offset = reader.ReadInt64();
            ulong u3Count = reader.ReadUInt64();

            Data.Struct3s = new Struct3[u3Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u3Offset, false);

            for (ulong u3Index = 0; u3Index < u3Count; u3Index++)
            {
                Data.Struct3s[u3Index] = new()
                {
                    UnknownUInt32_1 = reader.ReadUInt32(),
                    UnknownFloat_1 = reader.ReadSingle(),
                    UnknownUInt32_2 = reader.ReadUInt32()
                };
                if (reader.ReadUInt32() != 0) Debugger.Break();
            }

            reader.JumpTo(position);

            // List5 u4
            long u4Offset = reader.ReadInt64();
            ulong u4Count = reader.ReadUInt64();

            Data.Struct4s = new Struct4[u4Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u4Offset, false);

            for (ulong u4Index = 0; u4Index < u4Count; u4Index++)
            {
                Struct4 u4 = new();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                u4.UnknownUInt32_1 = reader.ReadUInt32();
                u4.UnknownUInt32_2 = reader.ReadUInt32();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                Data.Struct4s[u4Index] = u4;
            }

            reader.JumpTo(position);

            // List5 u5
            long u5Offset = reader.ReadInt64();
            ulong u5Count = reader.ReadUInt64();

            Data.Struct5s = new Struct5[u5Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u5Offset, false);

            for (ulong u5Index = 0; u5Index < u5Count; u5Index++)
            {
                Struct5 u5 = new();
                u5.UnknownUInt32_1 = reader.ReadUInt32();
                u5.UnknownUInt32_2 = reader.ReadUInt32();
                u5.UnknownUInt32_3 = reader.ReadUInt32();
                u5.UnknownUInt32_4 = reader.ReadUInt32();
                u5.UnknownFloat_1 = reader.ReadSingle();
                u5.UnknownFloat_2 = reader.ReadSingle();
                u5.UnknownUInt32_5 = reader.ReadUInt32();
                u5.UnknownUInt32_6 = reader.ReadUInt32();
                u5.UnknownUInt32_7 = reader.ReadUInt32();
                u5.UnknownUInt32_8 = reader.ReadUInt32();
                u5.UnknownUInt32_9 = reader.ReadUInt32();
                u5.UnknownUInt32_10 = reader.ReadUInt32();
                Data.Struct5s[u5Index] = u5;
            }

            reader.JumpTo(position);

            // List5 u6
            long u6Offset = reader.ReadInt64();
            ulong u6Count = reader.ReadUInt64();

            Data.Struct6s = new Struct6[u6Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u6Offset, false);

            for (ulong u6Index = 0; u6Index < u6Count; u6Index++)
            {
                Struct6 u6 = new();
                u6.UnknownString_1 = Helpers.ReadNullTerminatedStringTableEntry(reader);
                u6.UnknownUInt32_1 = reader.ReadUInt32();
                u6.UnknownUInt32_2 = reader.ReadUInt32();
                u6.UnknownUInt32_3 = reader.ReadUInt32();
                u6.UnknownUInt32_4 = reader.ReadUInt32();
                Data.Struct6s[u6Index] = u6;
            }

            reader.JumpTo(position);

            // List5 u7
            long u7Offset = reader.ReadInt64();
            ulong u7Count = reader.ReadUInt64();

            Data.Struct7s = new Struct7[u7Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u7Offset, false);

            for (ulong u7Index = 0; u7Index < u7Count; u7Index++)
            {
                Struct7 u7 = new();
                u7.UnknownUInt32_1 = reader.ReadUInt32();
                u7.UnknownUInt32_2 = reader.ReadUInt32();
                u7.UnknownUInt32_3 = reader.ReadUInt32();
                u7.UnknownUInt32_4 = reader.ReadUInt32();
                u7.UnknownUInt32_5 = reader.ReadUInt32();
                u7.UnknownUInt32_6 = reader.ReadUInt32();
                u7.UnknownFloat_1 = reader.ReadSingle();
                u7.UnknownFloat_2 = reader.ReadSingle();
                u7.UnknownUInt32_7 = reader.ReadUInt32();
                u7.UnknownUInt32_8 = reader.ReadUInt32();
                u7.UnknownUInt32_9 = reader.ReadUInt32();
                u7.UnknownUInt32_10 = reader.ReadUInt32();
                u7.UnknownUInt32_11 = reader.ReadUInt32();
                u7.UnknownUInt32_12 = reader.ReadUInt32();
                u7.UnknownUInt32_13 = reader.ReadUInt32();
                u7.UnknownUInt32_14 = reader.ReadUInt32();
                u7.UnknownInt32_1 = reader.ReadInt32();
                Data.Struct7s[u7Index] = u7;
            }

            reader.JumpTo(position);

            // collisions
            long collisionsOffset = reader.ReadInt64();
            ulong collisionsCount = reader.ReadUInt64();

            Data.Collisions = new string[collisionsCount];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(collisionsOffset, false);

            for (ulong collisionIndex = 0; collisionIndex < collisionsCount; collisionIndex++)
            {
                Data.Collisions[collisionIndex] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            }

            reader.JumpTo(position);

            // u8
            long u8Offset = reader.ReadInt64();
            ulong u8Count = reader.ReadUInt64();

            Data.Struct8s = new uint[u8Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u8Offset, false);

            for (ulong u8Index = 0; u8Index < u8Count; u8Index++)
            {
                Data.Struct8s[u8Index] = reader.ReadUInt32();
            }

            reader.JumpTo(position);

            // u9s
            for (int i = 0; i < 5; i++)
            {
                long u9Offset = reader.ReadInt64();
                ulong u9Count = reader.ReadUInt64();

                Data.Struct9s[i] = new uint[u9Count];

                // Save our current position.
                position = reader.BaseStream.Position;

                reader.JumpTo(u9Offset, false);

                for (ulong u9Index = 0; u9Index < u9Count; u9Index++)
                {
                    Data.Struct9s[i][u9Index] = reader.ReadUInt32();
                }

                reader.JumpTo(position);
            }

            // u10
            long u10Offset = reader.ReadInt64();
            ulong u10Count = reader.ReadUInt64();

            Data.Struct10s = new string[u10Count];

            // Save our current position.
            position = reader.BaseStream.Position;

            reader.JumpTo(u10Offset, false);

            for (ulong u10Index = 0; u10Index < u10Count; u10Index++)
            {
                Data.Struct10s[u10Index] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            }

            reader.JumpTo(position);
        }
    }
}
