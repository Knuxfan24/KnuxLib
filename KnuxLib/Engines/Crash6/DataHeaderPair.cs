namespace KnuxLib.Engines.Crash6
{
    // TODO: Does this format care about file order?
    public class DataHeaderPair : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DataHeaderPair() { }
        public DataHeaderPair(string filepath, bool extract = false)
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
            // Load the header and data files.
            BinaryReaderEx headerReader = new(File.OpenRead(Path.ChangeExtension(filepath, ".BH")));
            BinaryReaderEx dataReader = new(File.OpenRead(Path.ChangeExtension(filepath, ".BD")));

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
                Data.Add(file);
            }

            // Close Marathon's BinaryReader.
            headerReader.Close();
            dataReader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx headerWriter = new(File.Create(Path.ChangeExtension(filepath, ".BH")));
            BinaryWriterEx dataWriter = new(File.Create(Path.ChangeExtension(filepath, ".BD")));

            // Write the unknown value to the header.
            headerWriter.Write(0x501);

            // Loop through each file in this data pair.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
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

            // Close Marathon's BinaryWriter.
            headerWriter.Close();
            dataWriter.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "crash6_datapair");

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
