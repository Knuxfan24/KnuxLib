namespace KnuxLib.Engines.HasbroWii
{
    // TODO: See if file names exist somewhere.
    // TODO: What is that unknown value?
    // TODO: Saving support, tried hacking a solution together with Audio.xpac but the game just went silent.
    public class XPackage : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public XPackage() { }
        public XPackage(string filepath, bool extract = false)
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

            // Skip 0x08 bytes that are always 0.
            reader.JumpAhead(0x08);

            // Read the location that this archive's file data starts at.
            uint dataStartLocation = reader.ReadUInt32();

            // Read the amount of files in this archive.
            uint fileCount = reader.ReadUInt32();

            // Skip 0x08 bytes that are always 0.
            reader.JumpAhead(0x08);

            // Loop through and read each file.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Set up a new FileNode, named after the file with the current fileIndex value, as this format seems to lose file names.
                FileNode node = new() { Name = $"{Path.GetFileNameWithoutExtension(filepath)}_file{fileIndex}" };

                // Read an unknown value.
                // TODO: Is this important?
                uint UnknownUInt32_1 = reader.ReadUInt32();

                // Read the offset to this file's data.
                uint fileOffset = reader.ReadUInt32();

                // Read the length of this file.
                int fileLength = reader.ReadInt32();

                // Skip two unknown values that are always a copy of the file's length, followed by four nulls.
                reader.JumpAhead(0x08);

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

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "hasbro_xpac");
    }
}
