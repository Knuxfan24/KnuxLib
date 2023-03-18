namespace KnuxLib.Engines.Portable
{
    // TODO: Verify saving/importing works correctly with files that have .\ at the start of their names.
    public class AMB : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AMB() { }
        public AMB(string filepath, bool extract = false)
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
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            reader.ReadSignature(0x04, "#AMB");

            // Skip 0x0C bytes that are always 20 00 00 00 00 00 04 00 00 00 00 00
            reader.JumpAhead(0x0C);

            // Read this archive's file count.
            uint fileCount = reader.ReadUInt32();

            // Read the offset to this archive's file table.
            uint fileTableOffset = reader.ReadUInt32();

            // Read the offset to this archive's data table.
            uint dataTableOffset = reader.ReadUInt32();

            // Read the offset to this archive's string table.
            uint stringTableOffset = reader.ReadUInt32();

            // Jump to this archive's file table offset.
            reader.JumpTo(fileTableOffset);

            // Read each file.
            for (int i = 0; i < fileCount; i++)
            {
                // Set up a new node.
                FileNode node = new();

                // Read the offset to this file's data.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's length.
                int fileLength = reader.ReadInt32();

                // Skip eight bytes that are always FF FF FF FF 00 00 00 00
                reader.JumpAhead(0x08);

                // If this is an empty file, then skip it.
                if (fileOffset == 0)
                    continue;

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data location.
                reader.JumpTo(fileOffset);

                // Read this file's data
                node.Data = reader.ReadBytes(fileLength);

                // If this archive has a string table, then get this file's name from it.
                if (stringTableOffset != 0)
                {
                    reader.JumpTo(stringTableOffset + (0x20 * i));
                    node.Name = reader.ReadNullPaddedString(0x20);
                }

                // If this archive doesn't have a string table (which only seems to happen in Episode 1's World Map), then just name this file sequentially.
                else
                {
                    node.Name = $"file{i}";
                }

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

            // Write this file's signature.
            writer.WriteNullPaddedString("#AMB", 0x04);

            // Write this file's unknown consistent values.
            writer.Write(0x00000020);
            writer.Write(0x00040000);
            writer.Write(0x00000000);

            // Write this archive's file count.
            writer.Write(Data.Count);

            // Add an offset for this archives's file table.
            writer.AddOffset("FileTable");

            // Add an offset for this archives's data table.
            writer.AddOffset("BinaryStart");

            // Add an offset for this archives's string table.
            writer.AddOffset("StringTable");

            // Fill in the FileTable offset.
            writer.FillOffset("FileTable");

            // Loop through each file node.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add an offset for this file's data.
                writer.AddOffset($"File{i}Data");

                // Write the length of this file's data.
                writer.Write(Data[i].Data.Length);

                // Write two consistent unknown values.
                writer.Write(0xFFFFFFFF);
                writer.Write(0x00000000);
            }

            // Fill in the BinaryStart offset.
            writer.FillOffset("BinaryStart");

            // Loop through each file node.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in this file's data offset.
                writer.FillOffset($"File{i}Data");

                // Write this file's data.
                writer.Write(Data[i].Data);
            }

            // Fill in the StringTable offset.
            writer.FillOffset("StringTable");

            // Write each file's name.
            for (int i = 0; i < Data.Count; i++)
                writer.WriteNullPaddedString(Data[i].Name, 0x20);

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

            // Loop through each node to extract.
            foreach (FileNode node in Data)
            {
                // Print the name of the file we're extracting.
                Console.WriteLine($"Extracting {node.Name}.");

                // The Sonic The Portable Engine can use sub directories in its archives. Create the directory if needed.
                if (!Directory.Exists($@"{directory}\{Path.GetDirectoryName(node.Name)}"))
                    Directory.CreateDirectory($@"{directory}\{Path.GetDirectoryName(node.Name)}");

                // Extract the file.
                File.WriteAllBytes($@"{directory}\{node.Name}", node.Data);
            }
        }

        /// <summary>
        /// Imports files from a directory into an AMB node.
        /// </summary>
        /// <param name="directory">The directory to import, including sub directories.</param>
        public void Import(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                FileNode node = new()
                {
                    Name = Path.GetFileName(file),
                    Data = File.ReadAllBytes(file)
                };
                Data.Add(node);
            }
        }
    }
}
