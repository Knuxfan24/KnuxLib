namespace KnuxLib.Engines.Playstation2
{
    // TODO: Figure out what the unknown bytes in the name table are.
    // TODO: Writing support, how are the files that have the name table at 0x7FFF8 gonna work with this?
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
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            Data.Add(new() { Data = Encoding.ASCII.GetBytes("playstation2"), Name = "knuxtools_archivetype.txt" });
            Helpers.ExtractArchive(Data, directory);
        }
    }
}
