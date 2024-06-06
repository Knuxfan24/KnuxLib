namespace KnuxLib.Engines.Flipnic
{
    // Based on information from: https://github.com/MarkusMaal/FlipnicBinExtractor/tree/master.
    // TODO: Format saving.
    // TODO: Format importing.
    public class BinaryArchive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public BinaryArchive() { }
        public BinaryArchive(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Classes for this format.
        public class DataEntry
        {
            /// <summary>
            /// The name of this file.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The offset to this file's data.
            /// </summary>
            public uint Offset { get; set; }

            /// <summary>
            /// The binary data of this file.
            /// </summary>
            public byte[]? Data { get; set; }

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<FileNode> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Create a list of placeholder entries for this file.
            List<DataEntry> cdTOC = new();

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the "*Top Of CD Data" string that starts the Table of Contents for this file.
            string cdDataSignature = reader.ReadNullPaddedString(0x3C);

            // Read the offset to this file's data.
            uint dataStreamOffset = reader.ReadUInt32() * 0x800;

            // Loop through the table of contents, reading an entry if it isn't the "*End Of CD Data" string.
            while (reader.ReadNullPaddedString(0x3C) != "*End Of CD Data")
            {
                // Jump back to read this entry's name.
                reader.JumpBehind(0x3C);

                // Read this entry's name.
                string entryName = reader.ReadNullPaddedString(0x3C);

                // Read the offset to this entry's name.
                uint entryOffset = reader.ReadUInt32() * 0x800;

                // Save our position for the next entry.
                long position = reader.BaseStream.Position;

                // Jump to this entry's offset.
                reader.JumpTo(entryOffset);

                // Check if this is a folder or not.
                if (entryName.EndsWith('\\'))
                {
                    // Create a list of placeholder entries for this folder.
                    List<DataEntry> folderTOC = new();

                    // If this is a folder, then start reading file entries from it.
                    while (reader.ReadNullPaddedString(0x3C) != "*End Of Mem Data")
                    {
                        // Jump back to read this file's name.
                        reader.JumpBehind(0x3C);

                        // Read this file's name and append it to the folder name.
                        string fileName = entryName + reader.ReadNullPaddedString(0x3C);

                        // Read the offset to this file, adding entryOffset to it.
                        uint fileOffset = reader.ReadUInt32() + entryOffset;

                        // Save a placeholder entry for this file into the list.
                        folderTOC.Add(new()
                        {
                            Name = fileName,
                            Offset = fileOffset
                        });
                    }

                    // Read the offset to this folder's end.
                    uint folderDataEnd = reader.ReadUInt32() + entryOffset;

                    // Loop through all the placeholder entries.
                    for (int folderTOCIndex = 0; folderTOCIndex < folderTOC.Count; folderTOCIndex++)
                    {
                        // Jump to this entry's offset.
                        reader.JumpTo(folderTOC[folderTOCIndex].Offset);

                        // If this isn't the last file, then read the next file's offset, subtract this file's offset and read that amount of bytes.
                        if (folderTOCIndex != folderTOC.Count - 1)
                            folderTOC[folderTOCIndex].Data = reader.ReadBytes((int)(folderTOC[folderTOCIndex + 1].Offset - folderTOC[folderTOCIndex].Offset));

                        // If this is the last file, then read the folder data end, subtract this file's offset and read that amount of bytes.
                        else
                            folderTOC[folderTOCIndex].Data = reader.ReadBytes((int)(folderDataEnd - folderTOC[folderTOCIndex].Offset));

                        // Save this file.
                        Data.Add(new()
                        {
                            Name = folderTOC[folderTOCIndex].Name,
                            Data = folderTOC[folderTOCIndex].Data
                        });
                    }
                }
                else
                {
                    // Save a placeholder entry for this file into the list.
                    cdTOC.Add(new()
                    {
                        Name = entryName,
                        Offset = entryOffset
                    });
                }

                // Jump back for the next entry.
                reader.JumpTo(position);
            }

            // Read the file size, still following the 0x800 rule.
            uint fileSize = reader.ReadUInt32() * 0x800;

            // Loop through all the placeholder entries.
            for (int cdTOCIndex = 0; cdTOCIndex < cdTOC.Count; cdTOCIndex++)
            {
                // Jump to this entry's offset.
                reader.JumpTo(cdTOC[cdTOCIndex].Offset);

                // If this isn't the last file, then read the next file's offset, subtract this file's offset and read that amount of bytes.
                if (cdTOCIndex != cdTOC.Count - 1)
                    cdTOC[cdTOCIndex].Data = reader.ReadBytes((int)(cdTOC[cdTOCIndex + 1].Offset - cdTOC[cdTOCIndex].Offset));

                // If this is the last file, then read the file size end, subtract this file's offset and read that amount of bytes.
                else
                    cdTOC[cdTOCIndex].Data = reader.ReadBytes((int)(fileSize - cdTOC[cdTOCIndex].Offset));

                // Save this file.
                Data.Add(new()
                {
                    Name = cdTOC[cdTOCIndex].Name,
                    Data = cdTOC[cdTOCIndex].Data
                });
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "flipnic");

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
