namespace KnuxLib.Engines.Alchemy
{
    // Based on https://wiki.xentax.com/index.php/Vicarious_Visions_GOB_GFC#GOB.2C_GFC
    // TODO: Format saving.
    // TODO: Format importing.
    public class AssetsContainer : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AssetsContainer() { }
        public AssetsContainer(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Classes for this format.
        private class BlockEntry
        {
            /// <summary>
            /// The file size of this block's data in its ZLib compressed form.
            /// </summary>
            public uint CompressedFileSize { get; set; }

            /// <summary>
            /// The offset in the GOB file for this block's data.
            /// </summary>
            public uint DataOffset { get; set; }

            /// <summary>
            /// The block that should follow this one, as decompressed blocks can only be 0x00010000 bytes long.
            /// </summary>
            public uint NextBlock { get; set; } = 0x00007FFF;

            /// <summary>
            /// The hash of this block.
            /// TODO: What CRC algorithm is this, and does it include the data header and footer/is this hashing the decompressed data?
            /// </summary>
            public uint CRCHash { get; set; }

            /// <summary>
            /// The binary data in this block.
            /// </summary>
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

        private class FileEntry
        {
            /// <summary>
            /// The hash of this file.
            /// TODO: What CRC algorithm is this, and is it hashing the decompressed data?
            /// </summary>
            public uint CRCHash { get; set; }

            /// <summary>
            /// The size of this file when all of its blocks are decompressed.
            /// </summary>
            public uint DecompressedFileSize { get; set; }

            /// <summary>
            /// The index for the block holding the initial data for this file.
            /// </summary>
            public uint BlockIndex { get; set; }

            /// <summary>
            /// The name of this file.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Unknown bytes to fill out the rest of the 0x48 byte space reserved for the name.
            /// TODO: What are these? Clearly not just padding as they vary a lot.
            /// </summary>
            public byte[] UnknownBytes { get; set; } = Array.Empty<byte>();

            public override string ToString() => Name;
        }

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
        public override void Load(string filepath)
        {
            List<BlockEntry> Blocks = new();
            List<FileEntry> Files = new();

            // Set up Marathon's BinaryReader for the GFC file.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read this file's signature.
            reader.ReadSignature(0x00008008);

            // Read the size of this GFC's accompanying GOB file.
            uint gobSize = reader.ReadUInt32();

            // Read the block count in this GFC.
            uint blockCount = reader.ReadUInt32();

            // Read the file count in this GFC.
            uint fileCount = reader.ReadUInt32();
            
            // Loop through the block table and store each entry.
            for (int i = 0; i < blockCount; i++)
            {
                BlockEntry block = new()
                {
                    CompressedFileSize = reader.ReadUInt32(),
                    DataOffset = reader.ReadUInt32(),
                    NextBlock = reader.ReadUInt32(),
                };
                Blocks.Add(block);
            }

            // Read the CRC Hashes for each block.
            for (int i = 0; i < blockCount; i++)
            {
                Blocks[i].CRCHash = reader.ReadUInt32();
            }

            // Loop through the file table and store each entry.
            for (int i = 0; i < fileCount; i++)
            {
                FileEntry file = new()
                {
                    CRCHash = reader.ReadUInt32(),
                    DecompressedFileSize = reader.ReadUInt32(),
                    BlockIndex = reader.ReadUInt32()
                };
                Files.Add(file);
            }

            // Read the file name and unknown bytes for each file.
            for (int i = 0; i < fileCount; i++)
            {
                Files[i].Name = reader.ReadNullTerminatedString();
                Files[i].UnknownBytes = reader.ReadBytes(0x48 - (Files[i].Name.Length + 1));
            }

            // Close Marathon's BinaryReader for the GFC file.
            reader.Close();

            // Set up Marathon's BinaryReader for the GOB file.
            reader = new(File.OpenRead($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.gob"), true);

            // Loop through each block defined in the GFC file.
            for (int i = 0; i < Blocks.Count; i++)
            {
                //Jump to this block's data offset.
                reader.JumpTo(Blocks[i].DataOffset);

                // Read this block's STBL header.
                reader.ReadSignature(4, "STBL");

                // Read this block's data compression byte.
                byte Compression = reader.ReadByte();

                // If the byte is 0x7A (a lowercase z), then decompress the ZLib data, ignoring the STBL header and ENBL footer.
                if (Compression == 0x7A)
                    Blocks[i].Data = ZlibStream.Decompress(reader.ReadBytes((int)Blocks[i].CompressedFileSize - 0x9));

                // If the byte is not 0x7A, then simply read the bytes as they are.
                else
                    Blocks[i].Data = reader.ReadBytes((int)Blocks[i].CompressedFileSize - 0x9);
            }

            // Loop through each file defined in the GFC file.
            foreach (FileEntry file in Files)
            {
                // Setup an array of arrays to store each block's data.
                byte[][] fileData = new byte[1][];

                // Set the first entry in the array to this file's initial block index.
                fileData[0] = Blocks[(int)file.BlockIndex].Data;

                // Calculate the next block index.
                uint nextBlockIndex = Blocks[(int)file.BlockIndex].NextBlock;

                // Loop as long as the next block index isn't 0x00007FFF.
                while (nextBlockIndex != 0x00007FFF)
                {
                    // Add a new slot to the array.
                    Array.Resize(ref fileData, fileData.Length + 1);

                    // Add the next block's data to the new slot in the array.
                    fileData[^1] = Blocks[(int)nextBlockIndex].Data;

                    // Caluclate the next block index.
                    nextBlockIndex = Blocks[(int)nextBlockIndex].NextBlock;
                }

                // Create a Node based on this data.
                Node node = new()
                {
                    Name = file.Name,
                    Data = Helpers.CombineByteArrays(fileData)
                };

                // Save this node.
                Data.Add(node);
            }

            // Close Marathon's BinaryReader for the GOB file.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// TODO: Implement.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Loop through each node to extract.
            foreach (Node node in Data)
            {
                // Print the name of the file we're extracting.
                Console.WriteLine($"Extracting {node.Name}.");

                // The Alchemy Engine can use sub directories in its archives. Create the directory if needed.
                if (!Directory.Exists($@"{directory}\{Path.GetDirectoryName(node.Name)}"))
                    Directory.CreateDirectory($@"{directory}\{Path.GetDirectoryName(node.Name)}");

                // Extract the file.
                File.WriteAllBytes($@"{directory}\{node.Name}", node.Data);
            }
        }

        /// <summary>
        /// Imports files from a directory into an Alchemy node.
        /// </summary>
        /// <param name="directory">The directory to import, including sub directories.</param>
        public void Import(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                Node node = new()
                {
                    Name = file.Replace(directory, ".\\"),
                    Data = File.ReadAllBytes(file)
                };
                Data.Add(node);
            }
        }
    }
}
