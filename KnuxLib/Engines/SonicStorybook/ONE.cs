using AuroraLib.Compression.Algorithms;
using System;

namespace KnuxLib.Engines.SonicStorybook
{
    public class ONE : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ONE() { }
        public ONE(string filepath, bool extract = false)
        {
            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // Import the files in the directory.
                Data = Helpers.ImportArchive(filepath, true);

                // If the extract flag is set, then save this archive.
                if (extract)
                    Save($"{filepath}.one");
            }

            // Check if the input path is a file.
            else
            {
                // Load this file.
                Load(filepath);

                // If the extract flag is set, then extract this archive.
                if (extract)
                    Helpers.ExtractArchive(Data, Helpers.GetExtension(filepath, true), "storybook");
            }
        }

        // Actual data presented to the end user.
        public FileNode[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), true);

            // Initialise the data array.
            Data = new FileNode[reader.ReadInt32()];

            // Skip 0x0C bytes that are the same between all files.
            reader.JumpAhead(0x0C); // Always 0x10 then an offset to the first file's binary data then either four nulls or four FF bytes.

            // Loop through each file in this archive.
            for (int fileIndex = 0; fileIndex < Data.Length; fileIndex++)
            {
                // Set up a new file node.
                FileNode node = new();

                // Read this file's name.
                node.Name = reader.ReadNullPaddedString(0x20);

                // Read this file's index.
                uint nodeIndex = reader.ReadUInt32();

                // Read the offset to this file's compressed data.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's length.
                int fileLength = reader.ReadInt32();

                // Read this file's decompressed size.
                uint decompressedSize = reader.ReadUInt32();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data location.
                reader.JumpTo(fileOffset);

                // Print the name of the file we're deccompressing.
                Console.WriteLine($"Decompressing {node.Name}.");

                // Read and decompress this file's data.
                node.Data = new PRS().Decompress(reader.ReadBytes(fileLength));

                // Jump back for the next file.
                reader.JumpTo(position);

                // Save this file node.
                Data[fileIndex] = node;
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Print that we're saving this archive format.
            Console.WriteLine($"Saving Sonic Storybook Engine ONE Archive.");

            // Set up an array of compressed data.
            byte[][] CompressedData = new byte[Data.Length][];

            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath), true);

            // Write this archive's file count.
            writer.Write(Data.Length);

            // Write this file's unknown consistent 0x10 value.
            writer.Write(0x10);

            // Add an offset for this file's data position.
            writer.AddOffset("BinaryStart");

            // Write four null bytes (some files have this as 0xFFFFFFFF instead).
            writer.WriteNulls(0x04);

            // Loop through each file's information.
            for (int fileIndex = 0; fileIndex < Data.Length; fileIndex++)
            {
                // Print the name of the file we're compressing.
                Console.WriteLine($"Compressing {Data[fileIndex].Name}.");

                // Compress this file's data.
                CompressedData[fileIndex] = new PRS().Compress(Data[fileIndex].Data).ToArray();

                // Write this file's name.
                writer.WriteNullPaddedString(Data[fileIndex].Name, 0x20);

                // Write this file's index.
                writer.Write(fileIndex);

                // Add an offset for this file's compressed data.
                writer.AddOffset($"File{fileIndex}Data");

                // Write this file's compressed length.
                writer.Write(CompressedData[fileIndex].Length);

                // Write this file's decompressed length.
                writer.Write(Data[fileIndex].Data.Length);
            }

            // Fill in the BinaryStart offset.
            writer.FillInOffset("BinaryStart");

            // Loop through each file's data.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Fill this file's data offset
                writer.FillInOffset($"File{dataIndex}Data");

                // Write this file's compressed data.
                writer.Write(CompressedData[dataIndex]);
            }

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
