namespace KnuxLib.Engines.YachtClub
{
    // TODO: Figure out how to fill in the two unknown values, as they seem to break everything.
    public class Package : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Package() { }
        public Package(string filepath, bool extract = false)
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

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read this package's file count.
            uint fileCount = reader.ReadUInt32();

            // Skip an unknown value of 0x18 (likely an offset that's always in the same place).
            reader.JumpAhead(0x08);

            // Read the offset to this file's string table.
            long stringTableOffset = reader.ReadInt64();

            // Loop through and read each file.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Set up a new FileNode.
                FileNode node = new();

                // Read the offset to this file's data.
                long fileOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to the entry for this file in the string table.
                reader.JumpTo(stringTableOffset + (fileIndex * 0x08));

                // Read and jump to the string for this file's name.
                reader.JumpTo(reader.ReadInt64());

                // Read this file's name.
                node.Name = reader.ReadNullTerminatedString();

                // Jump to this file's data.
                reader.JumpTo(fileOffset);

                // Read the length of this file.
                ulong fileLength = reader.ReadUInt64();

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x08);

                // Read an unknown value.
                // TODO: What is this? Seems to be a 32-bit hash value, followed by padding. What is the hashing algorithm if so?
                ulong UnknownULong_1 = reader.ReadUInt64();

                // Read an unknown value.
                // TODO: What is this? Seems to be a 32-bit value then four nulls, so I'm just lumping them together as one.
                ulong UnknownULong_2 = reader.ReadUInt64();

                // Read this file's data.
                node.Data = reader.ReadBytes((int)fileLength);

                // Save this file.
                Data.Add(node);

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
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write this package's file count.
            writer.Write(Data.Count);

            // Write the file table offset that is always 0x18.
            writer.Write(0x18L);

            // Add an offset for the string table.
            writer.AddOffset("StringTableOffset", 0x08);

            // Loop through and write each file's data offset.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.AddOffset($"File{dataIndex}Offset", 0x08);
            
            // Loop through and write each file's data.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in the offset for this file's data.
                writer.FillOffset($"File{dataIndex}Offset");

                // Write this file's length.
                writer.Write((ulong)Data[dataIndex].Data.Length);

                // Write an unknown value that is always 0.
                writer.Write(0L);

                // Write two placeholders for the unknown values.
                // TODO: How do we determine what goes here? Game rejects them if they're wrong.
                writer.Write(0L);
                writer.Write(0L);

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Fill in the offset for this file's string table.
            writer.FillOffset("StringTableOffset");

            // Loop through and write each file's string offset offset.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.AddOffset($"File{dataIndex}StringOffset", 0x08);

            // Write each file's name.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in the offset for this string.
                writer.FillOffset($"File{dataIndex}StringOffset");

                // Write this file's name.
                writer.WriteNullTerminatedString(Data[dataIndex].Name);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            Data.Add(new() { Data = Encoding.ASCII.GetBytes("yachtclub"), Name = "knuxtools_archivetype.txt" });
            Helpers.ExtractArchive(Data, directory);
        }

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
