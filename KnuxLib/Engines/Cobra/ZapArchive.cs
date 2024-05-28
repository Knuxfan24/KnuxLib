namespace KnuxLib.Engines.Cobra
{
    // TODO: Format saving.
    public class ZapArchive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ZapArchive() { }
        public ZapArchive(string filepath, bool extract = false)
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
            reader.ReadSignature(0x03, "ZAP");

            // Realign to 0x04 bytes.
            reader.FixPadding(0x04);

            // Read the length of this file's padding (either 0x800 or 0x8000).
            uint paddingLength = reader.ReadUInt32();

            // Read the count of files in this archive.
            uint fileCount = reader.ReadUInt32();

            // Read an unknown integer value.
            // TODO: What is this?
            uint UnknownUInt32_1 = reader.ReadUInt32();

            Console.WriteLine(UnknownUInt32_1.ToString("X"));

            // Skip an unknown value that is always 0D 0A
            reader.JumpAhead(0x02);

            // Set up a string to figure out the full directory path for each file.
            string directory = "";

            // Set up a dictionary of file names and their sizes.
            Dictionary<string, int> fileNamesAndSizes = new();

            // Loop through each file reference in this archive.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Read this file's size.
                int fileSize = reader.ReadInt32();

                // Read this file's name.
                string fileName = reader.ReadNullPaddedString(reader.ReadUInt16());

                // Skip an unknown value that is always 0D 0A
                reader.JumpAhead(0x02);

                // If this file name ends with a / character, then add it to the directory string.
                if (fileName.EndsWith("/"))
                    directory += fileName;

                // If this file's name is just .., then remove the last directory entry in the directory string.
                else if (fileName == "..")
                    directory = directory.Remove(directory[..directory.LastIndexOf("/")].LastIndexOf("/") + 1);

                // If it's neither of these, then add the directory plus file name to the dictionary, along with its size.
                else
                    fileNamesAndSizes.Add($"{directory}{fileName}", fileSize);
            }

            // Realign to the padding size.
            reader.FixPadding(paddingLength);

            // Loop through each file reference we've saved.
            for (int fileIndex = 0; fileIndex < fileNamesAndSizes.Count; fileIndex++)
            {
                // Save a file node with this index's key and the data at our current position of the size stored in the dictionary.
                Data.Add(new()
                {
                    Name = fileNamesAndSizes.Keys.ElementAt(fileIndex),
                    Data = reader.ReadBytes(fileNamesAndSizes.Values.ElementAt(fileIndex))
                });

                // Realign to the padding size.
                reader.FixPadding(paddingLength);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }


        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "wererabbit");

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
