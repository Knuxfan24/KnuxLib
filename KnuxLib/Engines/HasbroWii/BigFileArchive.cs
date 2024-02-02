namespace KnuxLib.Engines.HasbroWii
{
    public class BigFileArchive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public BigFileArchive() { }
        public BigFileArchive(string filepath, bool extract = false)
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

            // Read this file's signature.
            reader.ReadSignature(0x04, "BIGF");

            // Read this archive's size.
            uint archiveSize = reader.ReadUInt32();

            // Change the reader to big endian for the rest of the file.
            reader.IsBigEndian = true;

            // Read the amount of files in this archive.
            uint fileCount = reader.ReadUInt32();

            // Read an unknown value.
            // TODO: Seems to be the size of the header and file table, including the L234 text and four null bytes afterwards?
            uint UnknownUInt32_1 = reader.ReadUInt32();

            // Loop through and read each file.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Set up a new FileNode.
                FileNode node = new();

                // Read the offset to this file's data.
                uint fileOffset = reader.ReadUInt32();

                // Read the length of this file.
                int fileLength = reader.ReadInt32();

                // Read this file's name.
                node.Name = reader.ReadNullTerminatedString();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data.
                reader.JumpTo(fileOffset);

                // Read this file's data.
                node.Data = reader.ReadBytes(fileLength);

                // Save this file.
                Data.Add(node);

                // Jump back for the next file.
                reader.JumpTo(position);
            }

            // At the end of the file table is the text "L234", is this always there? All the files in Family Game Night 1 and 2 had it.

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

            // Write this file's BIGF signature.
            writer.Write("BIGF");

            // Write a placeholder for the archive size to fill in later.
            writer.Write("TEMP");

            // Set the writer to big endian for the rest of the file.
            writer.IsBigEndian = true;

            // Write the amount of files in this archive.
            writer.Write(Data.Count);

            // Write a placeholder for the header size to fill in later.
            writer.Write("TEMP");

            // Loop through each file.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add an offset for this file.
                writer.AddOffset($"File{dataIndex}Offset");

                // Write the size of this file.
                writer.Write(Data[dataIndex].Data.Length);

                // Write this file's name.
                writer.WriteNullTerminatedString(Data[dataIndex].Name);
            }

            // Write the L234 string.
            writer.Write("L234");

            // Write four null bytes that are included in the header size calculation.
            writer.WriteNulls(0x04);

            // Jump back to 0x0C.
            writer.BaseStream.Position = 0x0C;

            // Write the current file size (only containing the header and file table).
            writer.Write((uint)writer.BaseStream.Length);

            // Jump back to the end of the current file.
            writer.BaseStream.Position = writer.BaseStream.Length;

            // Loop through each file.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Realign the reader to 0x8000.
                writer.FixPadding(0x8000);

                // Fill in this file's offset.
                writer.FillOffset($"File{dataIndex}Offset");

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);
            }

            // Jump back to 0x04.
            writer.BaseStream.Position = 0x04;

            // Switch the writer back to little endian.
            writer.IsBigEndian = false;

            // Write the archive's total size.
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "hasbro_big");

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
