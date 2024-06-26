﻿namespace KnuxLib.Engines.OpenSpace
{
    // TODO: Does this format support more than a single deep sub directory? If so, then this'll need to be modified to account for that.
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
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Create a StreamWriter for this archive's descriptor.
            StreamWriter descriptor = new(Path.ChangeExtension(filepath, ".DSC"));

            // Calculate the root directory for this archive.
            string root = Data[0].Name.Remove(Data[0].Name.IndexOf('\\'));

            // Set up a string to store a file's sub directory.
            string subDirectory = "";

            // Set up a value to store the archive size.
            int size = 0;

            // Write the name of the root directory.
            descriptor.Write($"({root}");

            // Loop through each file in this archive.
            for (int fileIndex = 0; fileIndex < Data.Count; fileIndex++)
            {
                // Check if this file has a backslash beyond its root.
                if (Data[fileIndex].Name[(root.Length + 1)..].Contains('\\'))
                {
                    // Check if this file's sub directory isn't our currently stored one.
                    if (subDirectory != Data[fileIndex].Name[(root.Length + 1)..].Remove(Data[fileIndex].Name[(root.Length + 1)..].IndexOf('\\')))
                    {
                        // Check if the current sub directory isn't blank so we don't write a terminator for the root.
                        if (subDirectory != "")
                            descriptor.Write(")");

                        // Set the sub directory to this file's.
                        subDirectory = Data[fileIndex].Name[(root.Length + 1)..].Remove(Data[fileIndex].Name[(root.Length + 1)..].IndexOf('\\'));

                        // Write the opener for the sub directory.
                        descriptor.Write($"({subDirectory}");
                    }
                }
                // If this file's name doesn't have a backslash and thus is in the root.
                else
                {
                    // Check if the current sub directory isn't the root.
                    if (subDirectory != "")
                    {
                        // Close the current sub directory.
                        descriptor.Write(")");

                        // Set the sub directory back to the root.
                        subDirectory = "";
                    }
                }

                // Write an entry for this file, with its name, starting position, length and an unknown value.
                descriptor.Write($"[{Path.GetFileName(Data[fileIndex].Name)},{size},{Data[fileIndex].Data.Length},0]");

                // Add this file's size to the length.
                size += Data[fileIndex].Data.Length;

                // Set up a value to calculate file padding.
                int padding = 0;

                // Add 0x800 to the padding value until it's larger than or equal to size.
                while (padding < size)
                    padding += 0x800;

                // Subtract size from padding.
                padding -= size;

                // Add the calculated padding value to the size.
                size += padding;
            }

            // Close the sub directory if the last file had one.
            if (subDirectory != "")
                descriptor.Write(")");

            // Close the root directory.
            descriptor.Write(")");

            // Close the StreamWriter.
            descriptor.Close();

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(Path.ChangeExtension(filepath, ".BF")));

            // Loop through each file in this archive.
            for (int fileIndex = 0; fileIndex < Data.Count; fileIndex++)
            {
                // Write this file's data.
                writer.Write(Data[fileIndex].Data);

                // Realign to 0x800.
                writer.FixPadding(0x800);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
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
