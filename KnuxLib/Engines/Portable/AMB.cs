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
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
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
                    reader.JumpTo(stringTableOffset + (0x20 * fileIndex));
                    node.Name = reader.ReadNullPaddedString(0x20);
                }

                // If this archive doesn't have a string table (which only seems to happen in Episode 1's World Map), then just name this file sequentially.
                else
                {
                    node.Name = $"file{fileIndex}";
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
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add an offset for this file's data.
                writer.AddOffset($"File{dataIndex}Data");

                // Write the length of this file's data.
                writer.Write(Data[dataIndex].Data.Length);

                // Write two consistent unknown values.
                writer.Write(0xFFFFFFFF);
                writer.Write(0x00000000);
            }

            // Fill in the BinaryStart offset.
            writer.FillOffset("BinaryStart");

            // Loop through each file node.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in this file's data offset.
                writer.FillOffset($"File{dataIndex}Data");

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);
            }

            // Fill in the StringTable offset.
            writer.FillOffset("StringTable");

            // Write each file's name.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.WriteNullPaddedString(Data[dataIndex].Name, 0x20);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "portable");

        /// <summary>
        /// Imports files from a directory into an AMB node.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
