using System.Diagnostics;
using System.Drawing;

namespace KnuxLib.Engines.RockmanX7
{
    public class MathTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MathTable() { }
        public MathTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.rockmanx7.mathtable.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public List<MathTableChunks.Environment> Environments { get; set; } = new();

            public List<Bitmap> Textures { get; set; } = new();
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

            // Read the count of chunks in this file.
            uint chunkCount = reader.ReadUInt32();

            // Read the length of this header, should always be 0x800?
            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != 0x800) Debugger.Break();

            // Read this file's MATH_TBL signature.
            reader.ReadSignature(0x08, "MATH_TBL");

            // Loop through each chunk in this file.
            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                // Read the size of this chunk.
                int chunkSize = reader.ReadInt32();

                // TODO: What is this for? Called BLOCKCOUNT in a QuickBMS script.
                uint chunkBlockCount = reader.ReadUInt16();

                // TODO: What is this for?
                ushort chunkUnknownUShort_1 = reader.ReadUInt16();

                // TODO: What is this for?
                ushort chunkUnknownUShort_2 = reader.ReadUInt16();

                // TODO: Make sure this is correct and, if so, figure out what the different chunk types do.
                ushort chunkType = reader.ReadUInt16();

                // Realign to 0x10 bytes.
                reader.FixPadding(0x10);

                // Save our current position so we can jump back for the next chunk.
                long position = reader.BaseStream.Position;

                // Jump to the current headerEnd value.
                reader.JumpTo(headerEnd);

                // Check the chunk type, if it's not a texture one, then read it as an environment chunk.
                if (chunkType != 0xFFFF)
                    Data.Environments.Add(MathTableChunks.Environment.Read(reader));
                else
                    Data.Textures = MathTableChunks.Texture.Read(reader);

                reader.JumpTo(headerEnd);

                // Jump ahead by the size of this chunk so we can reach the next one.
                reader.JumpAhead(chunkSize);

                // Realign to 0x800 so that we're in the right place.
                reader.FixPadding(0x800);

                // Update the headerEnd position as a hacky way to make jumping to the next chunk doable.
                headerEnd = (uint)reader.BaseStream.Position;

                // Jump back for the next chunk.
                reader.JumpTo(position);
            }
        }
    }
}
