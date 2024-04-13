namespace KnuxLib.Engines.OpenSpace
{
    // TODO: Saving support.
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
            // If the user has passed the description file in, then swap the extension out for the big file one instead.
            if (Path.GetExtension(filepath) == ".DSC")
                filepath = Path.ChangeExtension(filepath, ".BF");

            // Check that this big file archive has a corrosponding description file.
            if (!File.Exists(Path.ChangeExtension(filepath, ".DSC")))
                throw new Exception($"'{filepath}' doesn't appear to have a description file?");

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the description file, splitting it on the specified characters.
            string[] splitDescriptionFile = File.ReadAllText(Path.ChangeExtension(filepath, ".DSC")).Split(new char[4] { '[', ']', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            // Set the first entry in the split to be the base directory.
            string directory = splitDescriptionFile[0];

            // Loop through each line in the description file (skipping the first).
            for (int lineIndex = 1; lineIndex < splitDescriptionFile.Length; lineIndex++)
            {
                // If this split does not contain a comma, then set the directory to it (plus the first value).
                // TODO: Can directories have sub directories? If so, this'll break.
                if (!splitDescriptionFile[lineIndex].Contains(','))
                {
                    directory = $"{splitDescriptionFile[0]}\\{splitDescriptionFile[lineIndex]}";
                }

                // If this split does have a comma, then read the file.
                else
                {
                    // Split this split further.
                    string[] lineSplit = splitDescriptionFile[lineIndex].Split(',');

                    // Jump to the position given in the second value in the split.
                    reader.JumpTo(int.Parse(lineSplit[1]));

                    // Read and save this file.
                    Data.Add(new()
                    {
                        // Combine the value in directory with the first value in the split.
                        Name = $"{directory}\\{lineSplit[0]}",

                        // Read the amount of bytes specified in the third value in the split.
                        Data = reader.ReadBytes(int.Parse(lineSplit[2]))
                    });
                }
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "openspace_big");

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
