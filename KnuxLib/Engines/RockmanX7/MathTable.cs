using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7
{
    public class MathTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MathTable() { }
        public MathTable(string filepath, bool export = false)
        {
            Load(filepath);

            //if (export)
            //    JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.rockmanx7.mathtable.json", Data);
        }

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

                // TODO: Make sure this is correct and, if so, figure out what chunk type is what.
                ushort chunkType = reader.ReadUInt16();

                // Realign to 0x10 bytes.
                reader.FixPadding(0x10);

                // Save our current position so we can jump back for the next chunk.
                long position = reader.BaseStream.Position;

                // Jump to the current headerEnd value.
                reader.JumpTo(headerEnd);

                // TODO: Handle the 0xFFFF (texture?) chunk type.
                if (chunkType != 0xFFFF)
                {
                    // Read the count of offsets in this chunk.
                    uint offsetCount = reader.ReadUInt32();

                    // Loop through and read each offset.
                    for (int offsetIndex = 0; offsetIndex < offsetCount; offsetIndex++)
                    {
                        // Read this offset, relative to the chunk's start.
                        uint offset = reader.ReadUInt32(); // first offset always points to 0x10 nulls,
                                                           // second offset is always 0,
                                                           // third offset is always 0x10 higher than the first.
                                                           // TODO: Check this outside of a 0x17E type.

                        // Save our current position so we can jump back for the next offset.
                        long offsetPosition = reader.BaseStream.Position;

                        // Only read this offset's data if it isn't 0.
                        if (offset != 0)
                        {
                            // Jump to this offset.
                            reader.JumpTo(offset + headerEnd);

                            // TODO: Figure out the actual data.
                        }

                        // Jump back for the next offset.
                        reader.JumpTo(offsetPosition);
                    }

                    // Jump back for the temporary chunk split.
                    reader.JumpTo(headerEnd);
                }

                // Temporarily read this chunk's raw bytes and dump them to files.
                File.WriteAllBytes($@"{filepath}.{chunkType}", reader.ReadBytes(chunkSize));

                // Realign to 0x800, don't need to do this, just making sure for now.
                reader.FixPadding(0x800);

                // Update the headerEnd position as a hacky way to make jumping to the next chunk doable.
                headerEnd = (uint)reader.BaseStream.Position;

                // Jump back for the next chunk.
                reader.JumpTo(position);
            }
        }
    }
}
