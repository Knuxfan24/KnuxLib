using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnuxLib.Engines.Hedgehog
{
    public class DensitySetting : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DensitySetting() { }
        public DensitySetting(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.densitysetting.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public Vector2 Size { get; set; }

            public uint[] UnknownUInt32Array_1 { get; set; } = new uint[31];

            public float[] UnknownFloatArray_1 { get; set; } = new float[32];

            public string Area { get; set; } = "";

            public string? Layer { get; set; }

            public string? Colour { get; set; }

            public string? Scale { get; set; }

            public ModelName[] ModelNames { get; set; } = Array.Empty<ModelName>();
        }

        public class ModelName
        {
            public string Name { get; set; } = "";

            public uint UnknownUInt32_1 { get; set; }
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

            // Skip an unknown value that is always 0x0B.
            reader.JumpAhead(0x04);

            // Read this density setting's map size(?).
            Data.Size = new(reader.ReadSingle(), reader.ReadSingle());

            // Read three unknown values.
            uint UnknownUInt32_1 = reader.ReadUInt32();
            uint UnknownUInt32_2 = reader.ReadUInt32();
            uint UnknownUInt32_3 = reader.ReadUInt32();

            // Read 31 unknown integers.
            for (int unknownIndex = 0; unknownIndex < 31; unknownIndex++)
                Data.UnknownUInt32Array_1[unknownIndex] = reader.ReadUInt32();

            // Read 32 unknown floats.
            for (int unknownIndex = 0; unknownIndex < 32; unknownIndex++)
                Data.UnknownFloatArray_1[unknownIndex] = reader.ReadSingle();

            // Read this density setting's area name.
            Data.Area = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Read this density setting's layer name, setting it to null if it comes back as an empty string.
            Data.Layer = Helpers.ReadNullTerminatedStringTableEntry(reader);
            if (Data.Layer == "") Data.Layer = null;

            // Read this density setting's colour name, setting it to null if it comes back as an empty string.
            Data.Colour = Helpers.ReadNullTerminatedStringTableEntry(reader);
            if (Data.Colour == "") Data.Colour = null;

            // Read this density setting's scale name, setting it to null if it comes back as an empty string.
            Data.Scale = Helpers.ReadNullTerminatedStringTableEntry(reader);
            if (Data.Scale == "") Data.Scale = null;

            // Read the offset to this density setting's model name table.
            long modelNamesOffset = reader.ReadInt64();

            // Read the count of enteries in this density setting's model name table.
            ulong modelNamesCount = reader.ReadUInt64();

            // Save our position so we can jump back for the next data chunk.
            long position = reader.BaseStream.Position;

            // Jump to this density setting's model name table.
            reader.JumpTo(modelNamesOffset, false);

            // Initialise this density setting's model name array.
            Data.ModelNames = new ModelName[modelNamesCount];

            // Loop through each model name entry.
            for (ulong modelNameIndex = 0; modelNameIndex < modelNamesCount; modelNameIndex++)
            {
                // Read this model name and its unknown integer value.
                Data.ModelNames[modelNameIndex] = new()
                {
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader),
                    UnknownUInt32_1 = reader.ReadUInt32()
                };
                
                // Realign to 0x08 bytes.
                reader.FixPadding(0x08);
            }

            // Jump back for the next data chunk.
            reader.JumpTo(position);

            long u2Offset = reader.ReadInt64();
            ulong u2Count = reader.ReadUInt64();

            // Save our position so we can jump back for the next data chunk.
            position = reader.BaseStream.Position;

            reader.JumpTo(u2Offset, false);
            for (ulong u2Index = 0; u2Index < u2Count; u2Index++)
            {
                if (reader.ReadUInt32() != 0) Debugger.Break();
                reader.JumpAhead(60);
            }
        }
    }
}
