namespace KnuxLib.Engines.Criware
{
    // TODO: Figure out what the unknown bytes in the name table are.
    // TODO: Which name table position is the standard one?
    // TODO: Is the first chunk normally padded to 0x800 or is 0x80000 the standard?
    public class ArchiveFileSystem : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ArchiveFileSystem() { }
        public ArchiveFileSystem(string filepath, bool extract = false)
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
            reader.ReadSignature(0x03, "AFS");

            // Realign to 0x04 bytes.
            reader.FixPadding(0x04);

            // Read the amount of files in this archive.
            uint fileCount = reader.ReadUInt32();

            // Jump to the end of the file table to get the data for the name table.
            reader.JumpAhead(0x08 * fileCount);

            // Read the offset to the name table.
            uint nameTableOffset = reader.ReadUInt32();

            // Read the length of the name table.
            uint nameTableLength = reader.ReadUInt32();

            // If the name table values are 0, then jump to the very end of the table and read again to see if they're there (Monster Hunter does this).
            if (nameTableLength == 0 && nameTableOffset == 0)
            {
                // Jump to 0x7FFF8;
                reader.JumpTo(0x7FFF8);

                // Read the offset to the name table.
                nameTableOffset = reader.ReadUInt32();

                // Read the length of the name table.
                nameTableLength = reader.ReadUInt32();
            }

            // Jump back to 0x08.
            reader.JumpTo(0x08);

            // Loop through each file in this archive.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Set up a new file node.
                FileNode file = new();

                // Read this file's offset.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's length.
                int fileLength = reader.ReadInt32();

                // Save our position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data.
                reader.JumpTo(fileOffset);

                // Read this file's offset.
                file.Data = reader.ReadBytes(fileLength);

                // Jump to this file's index in the name table.
                reader.JumpTo(nameTableOffset + (fileIndex * 0x30));

                // Read this file's name.
                file.Name = reader.ReadNullPaddedString(0x20);

                // TODO: Read and figure out the purpose for the next 0x10 bytes.

                // Save this file.
                Data.Add(file);

                // Jump back for the next file.
                reader.JumpTo(position);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="firstSectionLength">How long the first chunk should be.</param>
        /// <param name="nameTableAtEndOfChunk">Whether or not the name table's offset is placed at the end of the first chunk.</param>
        public void Save(string filepath, uint firstSectionLength = 0x800, bool nameTableAtEndOfChunk = false)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the AFS signature.
            writer.WriteNullPaddedString("AFS", 0x04);

            // Write the amount of files in this archive.
            writer.Write(Data.Count);

            // Loop through each file to add their offsets.
            for (int fileIndex = 0; fileIndex < Data.Count; fileIndex++)
            {
                // Add an offset for this file.
                writer.AddOffset($"File{fileIndex}Offset");

                // Write this file's size.
                writer.Write(Data[fileIndex].Data.Length);
            }

            // Handle padding and the name table's offset depending on its position.
            if (!nameTableAtEndOfChunk)
            {
                // Add an offset for the name table.
                writer.AddOffset("NameTableOffset");

                // Write the size for the name table.
                writer.Write(Data.Count * 0x30);

                // Pad our position to the desired length.
                writer.FixPadding(firstSectionLength);
            }
            else
            {
                // Pad our position to the desired length, minus 0x08.
                writer.FixPadding(firstSectionLength - 0x08);

                // Add an offset for the name table.
                writer.AddOffset("NameTableOffset");

                // Write the size for the name table.
                writer.Write(Data.Count * 0x30);
            }

            // Loop through and write each file.
            for (int fileIndex = 0; fileIndex < Data.Count; fileIndex++)
            {
                // Fill in the offset for this file.
                writer.FillOffset($"File{fileIndex}Offset");

                // Write the binary data for this file.
                writer.Write(Data[fileIndex].Data);

                // Pad our position to a multiple of 0x800.
                writer.FixPadding(0x800);
            }

            // Fill in the offset for the name table.
            writer.FillOffset("NameTableOffset");

            // Loop through and write each's file name table data.
            for (int fileIndex = 0; fileIndex < Data.Count; fileIndex++)
            {
                // Write this file's name, padded to 0x20.
                writer.WriteNullPaddedString(Data[fileIndex].Name, 0x20);

                // Write some placeholders for the unknown data.
                // TODO: Figure these out.
                writer.WriteNulls(0x10);
            }

            // Pad this archive to a multiple of 0x800.
            writer.FixPadding(0x800);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "playstation2");
    }
}
