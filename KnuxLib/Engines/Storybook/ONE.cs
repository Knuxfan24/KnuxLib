using FraGag.Compression;
using System.Text;

namespace KnuxLib.Engines.Storybook
{
    public class ONE : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ONE() { }
        public ONE(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Actual data presented to the end user.
        public List<FileNode> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read this archive's file count.
            uint fileCount = reader.ReadUInt32();

            // Skip 0x0C bytes that are the same between all files.
            reader.JumpAhead(0x0C); // Always 0x10 then an offset to the first file's binary data then either four nulls or four FF bytes.

            // Read each file.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Set up a new node.
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
                node.Data = Prs.Decompress(reader.ReadBytes(fileLength));

                // Jump back for the next file.
                reader.JumpTo(position);

                // Save this node.
                Data.Add(node);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a list of compressed data.
            List<byte[]> CompressedData = new();

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), true);

            // Write this archive's file count.
            writer.Write(Data.Count);

            // Write this file's unknown consistent 0x10 value.
            writer.Write(0x10);

            // Add an offset for this file's data position.
            writer.AddOffset("BinaryStart");

            // Write four null bytes (some files have this as 0xFFFFFFFF instead).
            writer.WriteNulls(0x04);

            // Loop through each file's information.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Print the name of the file we're compressing.
                Console.WriteLine($"Compressing {Data[dataIndex].Name}.");

                // Compress this file's data.
                CompressedData.Add(Prs.Compress(Data[dataIndex].Data));

                // Write this file's name.
                writer.WriteNullPaddedString(Data[dataIndex].Name, 0x20);

                // Write this file's index.
                writer.Write(dataIndex);

                // Add an offset for this file's compressed data.
                writer.AddOffset($"File{dataIndex}Data");

                // Write this file's compressed length.
                writer.Write(CompressedData[dataIndex].Length);

                // Write this file's decompressed length.
                writer.Write(Data[dataIndex].Data.Length);
            }

            // Fill in the BinaryStart offset.
            writer.FillOffset("BinaryStart");

            // Loop through each file's data.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill this file's data offset
                writer.FillOffset($"File{dataIndex}Data");

                // Write this file's compressed data.
                writer.Write(CompressedData[dataIndex]);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            Data.Add(new() { Data = Encoding.ASCII.GetBytes("storybook"), Name = "knuxtools_archivetype.txt" });
            Helpers.ExtractArchive(Data, directory);
        }

        /// <summary>
        /// Imports files from a directory into a ONE node.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
