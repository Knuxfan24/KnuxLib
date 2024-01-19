namespace KnuxLib.Engines.YachtClub
{
    // TODO: The Japanese files don't seem to write correctly? Do they have broken reading too?
    public class StringTranslationList : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StringTranslationList() { }
        public StringTranslationList(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.yachtclub.stringtranslationlist.json", Data);
        }

        // Actual data presented to the end user.
        public List<string> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), Encoding.UTF8);

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x08);
            
            // Read the count of strings in this file.
            uint stringCount = reader.ReadUInt32();

            // Read this file's encoding type.
            // TODO: Is that what this is? Its 1 in every file except the Japanese ones, which use 2.
            uint encodingType = reader.ReadUInt32();

            // Skip an unknown value of 0x18 (likely an offset that's always in the same place).
            reader.JumpAhead(0x08);

            // Loop through and read the string at each offset in the table.
            for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
                Data.Add(Helpers.ReadNullTerminatedStringTableEntry(reader, false, 0, true));

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="isJapanese">Whether this is a Japanese encoded file.</param>
        public void Save(string filepath, bool isJapanese = false)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), Encoding.UTF8);

            // Write an unknown value that is always 0.
            writer.Write(0L);

            // Write the count of strings in this file.
            writer.Write(Data.Count);

            // Write the encoding value based on if this is a Japanese file or not.
            if (isJapanese)
                writer.Write(0x02);
            else
                writer.Write(0x01);

            // Write the offset value to the string table.
            writer.Write(0x18L);

            // Loop through and add an offset for each string in this file.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.AddOffset($"String{dataIndex}Offset", 0x08);

            // Loop through each string in this file.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Only write any data if this string actually has any.
                if (Data[dataIndex] != null)
                {
                    // Fill in this string's offset.
                    writer.FillOffset($"String{dataIndex}Offset");

                    // Write this string's value.
                    writer.WriteNullTerminatedString(Data[dataIndex]);

                    // Realign to 0x08 bytes.
                    writer.FixPadding(0x08);
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
