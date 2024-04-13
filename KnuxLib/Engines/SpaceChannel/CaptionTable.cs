namespace KnuxLib.Engines.SpaceChannel
{
    // TODO: warn_cap.bin doesn't save quite right, but every other file is correct.
    public class CaptionTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public CaptionTable() { }
        public CaptionTable(string filepath, bool isJapanese = false, bool export = false)
        {
            Load(filepath, isJapanese);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.spacechannel.caption.json", Data);
        }

        // Actual data presented to the end user.
        public List<string> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="isJapanese">Whether this is a Japanese encoded file.</param>
        public void Load(string filepath, bool isJapanese = false)
        {
            // Set up the correct encoding.
            Encoding encoding = Encoding.GetEncoding("cp1252");

            // If this is a Japanese file, then switch to shift-jis.
            if (isJapanese)
                encoding = Encoding.GetEncoding(932);

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "DGCP");

            // Read the count of messages in this caption table.
            uint messageCount = reader.ReadUInt32();

            // Skip two unknown values of 0 (likely padding?).
            reader.JumpAhead(0x08);

            // Loop through each message entry in this caption table.
            for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
            {
                // Read the amount of split strings that make up this message.
                uint stringCount = reader.ReadUInt32();

                // Read the offset to this message's string table.
                uint stringTableOffset = reader.ReadUInt32();

                // Save our current position.
                long position = reader.BaseStream.Position;

                // Jump to the string table offset.
                reader.JumpTo(stringTableOffset);

                // Set up a message placeholder.
                string message = "";

                // Loop through each string in this message.
                for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
                {
                    // Read this message's string offset.
                    uint stringOffset = reader.ReadUInt32();

                    // Save our current position so we can jump back for the next message.
                    long stringPosition = reader.BaseStream.Position;

                    // Jump to this message's string offset.
                    reader.JumpTo(stringOffset);

                    // Set up a length value.
                    int length = 0;

                    // Read bytes until one is 0, adding to length each time.
                    while (reader.ReadByte() != 0)
                        length++;

                    // Jump back to the start of the string.
                    reader.JumpTo(stringOffset);

                    // Encode the read bytes and add it to the message.
                    message += encoding.GetString(reader.ReadBytes(length));

                    // Jump back for the next message.
                    reader.JumpTo(stringPosition);

                    // If this is NOT the last string in the message, then add a line break.
                    if (stringIndex != stringCount - 1)
                        message += "\r\n";
                }

                // Save this message.
                Data.Add(message);

                // Jump back for the next message.
                reader.JumpTo(position);
            }

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
            // Set up the correct encoding.
            Encoding encoding = Encoding.GetEncoding("cp1252");

            // If this is a Japanese file, then switch to shift-jis.
            if (isJapanese)
                encoding = Encoding.GetEncoding(932);

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write this file's signature.
            writer.Write("DGCP");

            // Write the count of messages in this caption table.
            writer.Write(Data.Count);

            // Write two unknown values of 0 (likely padding?).
            writer.WriteNulls(0x08);

            // Loop through each message in this caption table.
            for (int messageIndex = 0; messageIndex < Data.Count; messageIndex++)
            {
                // Check if this message isn't empty.
                if (Data[messageIndex] != "")
                {
                    // Write the count of line breaks in this message, plus one.
                    writer.Write(Data[messageIndex].Select((c, i) => Data[messageIndex][i..]).Count(sub => sub.StartsWith("\r\n")) + 1);

                    // Add an offset for this message's string table.
                    writer.AddOffset($"Message{messageIndex}StringTableOffset");
                }
                else
                {
                    // Add some dummy values for empty messages.
                    writer.WriteNulls(0x08);
                }
            }

            // Loop through each message in this caption table.
            for (int messageIndex = 0; messageIndex < Data.Count; messageIndex++)
            {
                // Check if this message isn't empty.
                if (Data[messageIndex] != "")
                {
                    // Fill in the offset for this message.
                    writer.FillOffset($"Message{messageIndex}StringTableOffset");

                    // Add an offset for each line break in this message.
                    for (int splitIndex = 0; splitIndex < Data[messageIndex].Split("\r\n").Length; splitIndex++)
                        writer.AddOffset($"Message{messageIndex}String{splitIndex}Offset");
                }
            }

            // Loop through each message in this caption table.
            for (int messageIndex = 0; messageIndex < Data.Count; messageIndex++)
            {
                // Check if this message isn't empty.
                if (Data[messageIndex] != "")
                {
                    // Split this message on the line breaks.
                    string[] splits = Data[messageIndex].Split("\r\n");

                    // Loop through each split in this message.
                    for (int splitIndex = 0; splitIndex < splits.Length; splitIndex++)
                    {
                        // Fill in the offset for this split.
                        writer.FillOffset($"Message{messageIndex}String{splitIndex}Offset");

                        // Write this split.
                        writer.Write(encoding.GetBytes(splits[splitIndex]));

                        // Write a null terminator for the string.
                        writer.WriteNull();
                    }
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
