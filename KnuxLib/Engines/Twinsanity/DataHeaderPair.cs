namespace KnuxLib.Engines.Twinsanity
{
    // TODO: Does this format care about file order?
    public class DataHeaderPair
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DataHeaderPair() { }
        public DataHeaderPair(string filepath, bool extract = false)
        {
            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // Import the files in the directory.
                Data = Helpers.ImportArchive(filepath);

                // If the extract flag is set, then save this archive.
                if (extract)
                    Save($"{filepath}.arc");
            }

            // Check if the input path is a file.
            else
            {
                // Load this file.
                Load(filepath);

                // If the extract flag is set, then extract this archive.
                if (extract)
                    Helpers.ExtractArchive(Data, Helpers.GetExtension(filepath, true), "twinsanity");
            }
        }

        // Actual data presented to the end user.
        public FileNode[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Set up a list of files.
            List<FileNode> files = [];

            // Load the header and data files into two seperate BinaryReaders.
            ExtendedBinaryReader headerReader = new(File.OpenRead(Path.ChangeExtension(filepath, ".BH")));
            ExtendedBinaryReader dataReader = new(File.OpenRead(Path.ChangeExtension(filepath, ".BD")));

            // TODO: What is this first value? Is it always 01 05 00 00?
            headerReader.JumpAhead(0x04);

            // Loop through until we hit the end of the header file.
            while (headerReader.BaseStream.Position < headerReader.BaseStream.Length)
            {
                // Set up a new file entry.
                FileNode file = new();

                // Read this file's name, prefixed with its length.
                file.Name = headerReader.ReadNullPaddedString(headerReader.ReadInt32());

                // Read the offset to this file's data.
                uint DataOffset = headerReader.ReadUInt32();

                // Read the size of this file.
                int DataSize = headerReader.ReadInt32();

                // Jump the data reader to our read offset.
                dataReader.BaseStream.Position = DataOffset;

                // Read this file's data.
                file.Data = dataReader.ReadBytes(DataSize);

                // Save this file.
                files.Add(file);
            }

            // Set the data to the files list, converted to an array.
            Data = [.. files];

            // Close our BinaryReaders.
            headerReader.Close();
            dataReader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Create the header and data files through two seperate BinaryWriters.
            ExtendedBinaryWriter headerWriter = new(File.Create(Path.ChangeExtension(filepath, ".BH")));
            ExtendedBinaryWriter dataWriter = new(File.Create(Path.ChangeExtension(filepath, ".BD")));

            // Write the unknown value to the header.
            headerWriter.Write(0x501);

            // Loop through each file in this data pair.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Write the length of this file's name to the header.
                headerWriter.Write(Data[dataIndex].Name.Length);

                // Write this file's name to the header.
                headerWriter.WriteNullPaddedString(Data[dataIndex].Name, Data[dataIndex].Name.Length);

                // Write the current position of the data writer to the header.
                headerWriter.Write((uint)dataWriter.BaseStream.Position);

                // Write the size of this file to the header.
                headerWriter.Write(Data[dataIndex].Data.Length);

                // Write this file's data to the data writer.
                dataWriter.Write(Data[dataIndex].Data);
            }

            // Close our BinaryWriters.
            headerWriter.Close();
            dataWriter.Close();
        }
    }
}
