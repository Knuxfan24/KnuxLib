namespace KnuxLib.Engines.WorldAdventureWii
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

        // Classes for this format.
        public class Node
        {
            /// <summary>
            /// The name of this node.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The bytes that make up this node.
            /// </summary>
            public byte[] Data { get; set; } = Array.Empty<byte>();

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<Node> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Check the first four bytes of this file.
            string compressedIdentifier = reader.ReadNullPaddedString(0x04);

            // If the value of the first four bytes is not "one.", then decompress it.
            if (compressedIdentifier != "one.")
            {
                // Close the old reader.
                reader.Close();

                // Set up a memory stream buffer.
                MemoryStream buffer = new();

                // Set up a PuyoTools LZ11 class.
                PuyoTools.Core.Compression.Lz11Compression lz11 = new();

                // Decompress the data from this file into the buffer.
                lz11.Decompress(File.OpenRead(filepath), buffer);

                //Reinitalise the reader with the decompressed LZ11 data.
                reader = new(buffer);

                // Jump back to the start of the reader's buffer.
                reader.JumpTo(0x00);

                // Reread the first four bytes.
                compressedIdentifier = reader.ReadNullPaddedString(0x04);
            }

            uint fileCount = reader.ReadUInt32();

            for (int i = 0; i < fileCount; i++)
            {
                // Set up a new node.
                Node node = new();

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
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            writer.WriteNullPaddedString("one.", 0x04);

            writer.Write(Data.Count);

            for (int i = 0; i < Data.Count; i++)
            {
                writer.WriteNullPaddedString(Data[i].Name, 0x38);
                writer.AddOffset($"File{i}Data");
                writer.Write(Data[i].Data.Length);
            }

            // Weird padding fix. Not sure about this one.
            writer.FixPadding(0x10);
            writer.WriteNulls(0x10);

            for (int i = 0; i < Data.Count; i++)
            {
                writer.FillOffset($"File{i}Data");
                writer.Write(Data[i].Data);

                // Even weirder padding fix. Even less sure about this one.
                if (writer.BaseStream.Position % 0x10 == 0)
                    writer.WriteNulls(0x10);
                writer.FixPadding(0x20);
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
            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Set up our file order log.
            StreamWriter log = new(File.Open($@"{directory}\fileorder.log", FileMode.Create));

            // Loop through each node to extract.
            foreach (Node node in Data)
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

            // Finish the order log.
            log.Close();
        }

        /// <summary>
        /// Imports files from a directory into a ONE node, respecting a fileorder.log file if one exists.
        /// </summary>
        /// <param name="directory">The directory to import, excluding sub directories.</param>
        public void Import(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.*");

            // Check if we have an order to reference, if not, then just do them sequentially.
            if (!File.Exists(@$"{directory}\fileorder.log"))
            {
                foreach (string file in files)
                {
                    Node node = new()
                    {
                        Name = Path.GetFileName(file),
                        Data = File.ReadAllBytes(file)
                    };
                    Data.Add(node);
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
                        Node node = new()
                        {
                            Name = Path.GetFileName(@$"{directory}\{file}"),
                            Data = File.ReadAllBytes(@$"{directory}\{file}")
                        };
                        Data.Add(node);
                    }

                    // If this file doesn't exist, check if it's the space.bin padding and create it if so.
                    else if (file == "space.bin")
                    {
                        Node node = new()
                        {
                            Name = "space.bin",
                            Data = new byte[] { 0x73, 0x70, 0x61, 0x63, 0x65, 0x20, 0x20, 0x20 }
                        };
                        Data.Add(node);
                    }

                    // If both the previous checks fail, then thrown an exception.
                    else
                    {
                        throw new Exception($"Couldn't find file '{file}' referenced by fileorder.log.");
                    }
                }
            }
        }
    }
}
