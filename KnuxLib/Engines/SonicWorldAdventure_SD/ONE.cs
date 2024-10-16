﻿using AuroraLib.Compression.Algorithms;

namespace KnuxLib.Engines.SonicWorldAdventure_SD
{
    // TODO: Test the saved ONZ files, as the Storybook ONEs were broken for ages before I noticed.
    public class ONE : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ONE() { }
        public ONE(string filepath, bool compress = false, bool extract = false)
        {
            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // Import the files in the directory.
                Import(filepath);

                // If the extract flag is set, then save this archive.
                if (extract)
                    if (!compress)
                        Save($"{filepath}.one");
                    else
                        Save($"{filepath}.onz", true);
            }

            // Check if the input path is a file.
            else
            {
                // Load this file.
                Load(filepath);

                // If the extract flag is set, then extract this archive.
                if (extract)
                    Extract(StringHelpers.GetExtension(filepath, true));
            }
        }

        // Actual data presented to the end user.
        public FileNode[] Data = [];

        // Internal value used for extraction.
        public bool wasCompressed = false;

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Check if this file is LZ11 compressed.
            if (new LZ11().IsMatch(File.OpenRead(filepath)))
            {
                // Flag that this file was compressed for the archive identifier.
                wasCompressed = true;

                // Set up a memory stream to hold the decompressed file.
                MemoryStream decompressed = new();

                // Decompress the file.
                new LZ11().Decompress(File.OpenRead(filepath), decompressed);

                // Reinitialise the reader with the decompressed stream.
                reader = new ExtendedBinaryReader(decompressed);

                // Set the reader to the start of the stream.
                reader.BaseStream.Position = 0;
            }

            // Read this file's signature.
            reader.ReadSignature(0x04, "one.");

            // Read the amount of files in this archive.
            Data = new FileNode[reader.ReadUInt32()];

            // Loop through each file.
            for (int fileIndex = 0; fileIndex < Data.Length; fileIndex++)
            {
                // Set up a new node.
                FileNode node = new();

                // Read this file's name.
                node.Name = reader.ReadNullPaddedString(0x38);

                // Read the offset to this file's compressed data.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's length.
                int fileLength = reader.ReadInt32();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data location.
                reader.JumpTo(fileOffset);

                // Read and decompress this file's data.
                node.Data = reader.ReadBytes(fileLength);

                // Jump back for the next file.
                reader.JumpTo(position);

                // Save this node.
                Data[fileIndex] = node;
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="compress">Whether to compress the resulting file.</param>
        public void Save(string filepath, bool compress = false)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // Write this archive's signature.
            writer.WriteNullPaddedString("one.", 0x04);

            // Write the amount of files in this archive.
            writer.Write(Data.Length);

            // Loop through each file entry.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Write this file's name, padded to 0x38 bytes.
                writer.WriteNullPaddedString(Data[dataIndex].Name, 0x38);

                // Add an offset for this file's data.
                writer.AddOffset($"File{dataIndex}Data");

                // Write the size of this file.
                writer.Write(Data[dataIndex].Data.Length);
            }

            // Weird padding fix. Not sure about this one.
            writer.FixPadding(0x10);
            writer.WriteNulls(0x10);

            // Loop through each file.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Fill in the offset for this file.
                writer.FillInOffset($"File{dataIndex}Data");

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);

                // Even weirder padding fix. Even less sure about this one.
                if (writer.BaseStream.Position % 0x10 == 0)
                    writer.WriteNulls(0x10);
                writer.FixPadding(0x20);
            }

            // Close our BinaryReader.
            writer.Close();

            // Check if we need to compress the saved file.
            if (compress)
            {
                // Read the newly saved, uncompressed file into a file stream.
                using FileStream uncompressed = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Set up a memory stream to hold the compressed data.
                using MemoryStream compressed = new();

                // Compress the data.
                new LZ11().Compress(uncompressed, compressed);

                // Close the uncompressed file.
                uncompressed.Close();

                // Overwrite the file with the compressed data.
                File.WriteAllBytes(filepath, compressed.ToArray());
            }
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// TODO: Add fileordering to the FileNode extractor so this can use it instead.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Set up our file order log.
            StreamWriter log = new(File.Open($@"{directory}\fileorder.log", FileMode.Create));

            // Loop through each node to extract.
            foreach (FileNode node in Data)
            {
                // Write this file's name to the order log.
                log.WriteLine(node.Name);

                // If this isn't a space.bin padding file, then write it to disk.
                if (node.Name != "space.bin")
                {
                    // Print the name of the file we're extracting.
                    Console.WriteLine($"Extracting {node.Name}.");

                    // Extract the file.
                    File.WriteAllBytes($@"{directory}\{node.Name}", node.Data);
                }
            }

            // Write the archive type identifier based on the compression flag.
            if (!wasCompressed)
                File.WriteAllText($@"{directory}\knuxtools_archivetype.txt", "swa_sd");
            else
                File.WriteAllText($@"{directory}\knuxtools_archivetype.txt", "swa_sd_compressed");

            // Finish the order log.
            log.Close();
        }

        /// <summary>
        /// Imports files from a directory into this format, respecting a fileorder.log file if one exists.
        /// TODO: Add fileordering to the FileNode importer so this can use it instead.
        /// </summary>
        /// <param name="directory">The directory to import, excluding sub directories.</param>
        public void Import(string directory)
        {
            // Set up a list of nodes.
            List<FileNode> nodes = [];

            // Get the files in our input directory's root.
            string[] files = Directory.GetFiles(directory, "*.*");

            // Check if we have an order to reference, if not, then just do them sequentially.
            if (!File.Exists(@$"{directory}\fileorder.log"))
            {
                foreach (string file in files)
                {
                    // If this file is the KnuxTools archive type identifier shortcut, then skip over it.
                    if (Path.GetFileName(file) == "knuxtools_archivetype.txt")
                        continue;

                    FileNode node = new()
                    {
                        Name = Path.GetFileName(file),
                        Data = File.ReadAllBytes(file)
                    };
                    nodes.Add(node);
                }
            }

            else
            {
                // Read our order list.
                string[] fileOrder = File.ReadAllLines(@$"{directory}\fileorder.log");

                // Loop through each file in the order list.
                foreach (string file in fileOrder)
                {
                    // If this file exists, then read it like normal.
                    if (File.Exists(@$"{directory}\{file}"))
                    {
                        FileNode node = new()
                        {
                            Name = Path.GetFileName(@$"{directory}\{file}"),
                            Data = File.ReadAllBytes(@$"{directory}\{file}")
                        };
                        nodes.Add(node);
                    }

                    // If this file doesn't exist, check if it's the space.bin padding and create it if so.
                    else if (file == "space.bin")
                    {
                        FileNode node = new()
                        {
                            Name = "space.bin",
                            Data = [0x73, 0x70, 0x61, 0x63, 0x65, 0x20, 0x20, 0x20]
                        };
                        nodes.Add(node);
                    }

                    // If both the previous checks fail, then thrown an exception.
                    else
                    {
                        throw new Exception($"Couldn't find file '{file}' referenced by fileorder.log.");
                    }
                }
            }

            // Set the data to the nodes list, converted to an array.
            Data = [.. nodes];
        }
    }
}
