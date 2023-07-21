namespace KnuxLib.Engines.Gods
{
    // TODO: Get samples of the WAD files from other Data Design Interactive games that run on their GODS engine. Add new FormatVersions as approriate.
    // TODO: The folder structure of this is ugly, but I don't see how else to do it.
    // TODO: Format saving.
    // TODO: Format importing.
    // TODO: Convert this node setup to the FileNode in Helpers.cs
    public class WAD : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public WAD() { }
        public WAD(string filepath, FormatVersion version = FormatVersion.NinjabreadMan_PCPS2, bool extract = false)
        {
            Load(filepath, version);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Classes for this format.
        public enum FormatVersion
        {
            NinjabreadMan_PCPS2 = 0,
            NinjabreadMan_Wii = 1
        }

        public class Node
        {
            /// <summary>
            /// The name of this node.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown value only found in Wii WADs.
            /// TODO: What is this?
            /// </summary>
            public int? UnknownWiiInt32_1 { get; set; } = null;

            /// <summary>
            /// The bytes that make up this node; directories don't have any.
            /// </summary>
            public byte[]? Data { get; set; }

            /// <summary>
            /// An unknown value only found in Wii WADs.
            /// TODO: What is this?
            /// </summary>
            public int? UnknownWiiInt32_2 { get; set; } = null;

            /// <summary>
            /// An unknown boolean value.
            /// TODO: What is this?
            /// </summary>
            public bool UnknownBoolean_1 { get; set; }

            /// <summary>
            /// The index of the last node in this node's root.
            /// </summary>
            public int LastRootNodeIndex { get; set; } = -1;

            /// <summary>
            /// This node's sibling index.
            /// </summary>
            public int SiblingIndex { get; set; } = -1;

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<Node> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The WAD version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.NinjabreadMan_PCPS2)
        {
            // Set up the size of an entry in the file table for later maths.
            uint entrySize = 0x18;

            // Update the entrySize based on the Format Version.
            switch (version)
            {
                case FormatVersion.NinjabreadMan_Wii: entrySize = 0x20; break;
            }

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "WADH");

            // Read the offset to the data table.
            uint DataTableOffset = reader.ReadUInt32();

            // Read the count of the nodes in this file.
            uint NodeCount = reader.ReadUInt32();

            // Read the length of this file's string table.
            uint StringTableLength = reader.ReadUInt32();

            // Calculate where the string table will be.
            uint StringTableOffset = (NodeCount * entrySize) + 0x10;

            // Loop through and read the nodes.
            for (int i = 0; i < NodeCount; i++)
            {
                // Skip the dummy root directory.
                if (i == 0)
                {
                    reader.JumpAhead(entrySize);
                    continue;
                }

                // Set up a node.
                Node node = new();

                // Read this node's name.
                node.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false, StringTableOffset);

                // If this file is a Ninjabread Man Wii Version WAD, then read this node's first unknown value.
                if (version == FormatVersion.NinjabreadMan_Wii) node.UnknownWiiInt32_1 = reader.ReadInt32();

                // Read the offset and size to this node's data.
                uint DataOffset = reader.ReadUInt32();
                int DataSize = reader.ReadInt32();

                // If this file is a Ninjabread Man Wii Version WAD, then read this node's second unknown value.
                if (version == FormatVersion.NinjabreadMan_Wii) node.UnknownWiiInt32_2 = reader.ReadInt32();

                // Read this node's unknown boolean value.
                node.UnknownBoolean_1 = reader.ReadBoolean(0x04);
                
                // Read this node's last root node index.
                node.LastRootNodeIndex = reader.ReadInt32();

                // Read this node's sibling index.
                node.SiblingIndex = reader.ReadInt32();

                // Save our current position so we can jump back for the next node.
                long pos = reader.BaseStream.Position;

                // If this entry has a data size, then jump to and read it.
                if (DataSize != 0)
                {
                    reader.JumpTo(DataTableOffset + DataOffset);
                    node.Data = reader.ReadBytes(DataSize);
                }

                // Save this node.
                Data.Add(node);

                // Jump back for the next node.
                reader.JumpTo(pos);
            }

            // Close Marathon's BinaryReader.
            reader.Close();

            // Awful system to handle directories, but I can't think of a different way to really do this...
            // Create a list of filepaths with an empty entry in it to line up the Sibling Indices.
            List<string> paths = new() { "" };

            // Set up a string for the file's full path.
            string fullPath = "";

            // Loop through all our entries.
            for (int i = 0; i < Data.Count; i++)
            {
                // If this entry does not have a sibling, then simply add its name onto the full path.
                if (Data[i].SiblingIndex == -1)
                    fullPath += $@"\{Data[i].Name}";

                // If it does, then set the fullPath to the sibling's path, but remove the name of the sibling entry and add this entry's in its place.
                else
                    fullPath = paths[Data[i].SiblingIndex].Replace(Data[Data[i].SiblingIndex - 1].Name, Data[i].Name);

                // Add this entry's path for future entries to reference.
                paths.Add(fullPath);
            }

            // Remove the empty root path.
            paths.RemoveAt(0);

            // Set the node names to their paths.
            for (int i = 0; i < Data.Count; i++)
                Data[i].Name = paths[i];
        }

        /// <summary>
        /// Saves this format's file.
        /// TODO: Implement.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The WAD version to save with.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.NinjabreadMan_PCPS2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// TODO: Get around to converting this to use the FileNode system so this can be replaced with the generic extractor.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Loop through each node to extract.
            foreach (Node node in Data)
            {
                // Skip this node if it's only a directory entry.
                if (node.Data == null)
                    continue;

                // Print the name of the file we're extracting.
                Console.WriteLine($"Extracting {node.Name}.");

                // The GODS Engine can use sub directories in its archives. Create the directory if needed.
                if (!Directory.Exists($@"{directory}\{Path.GetDirectoryName(node.Name)}"))
                    Directory.CreateDirectory($@"{directory}\{Path.GetDirectoryName(node.Name)}");

                // Extract the file.
                File.WriteAllBytes($@"{directory}\{node.Name}", node.Data);
            }
        }

        /// <summary>
        /// Imports a directory as a Gods Engine WAD.
        /// TODO: Implement.
        /// </summary>
        /// <param name="directory">The directory to import, including sub directories.</param>
        public void Import(string directory)
        {
            throw new NotImplementedException();
        }
    }
}
