using System.Text;

namespace KnuxLib.Engines.Storybook
{
    // TODO: What is the extra set of offsets even for?
    // TODO: The event message tables save incorrectly, why?
    public class MessageTable_SecretRings : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable_SecretRings() { }
        public MessageTable_SecretRings(string filepath, bool isJapanese = false, bool export = false)
        {
            Load(filepath, isJapanese);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.messagetable_secretrings.json", Data);
        }

        // Classes for this format.
        public class MessageEntry
        {
            /// <summary>
            /// The text in this message.
            /// </summary>
            public string Message { get; set; } = "";

            /// <summary>
            /// Whether or not this message has an extra offset pointing to its position.
            /// TODO: What is this offset for?
            /// </summary>
            public bool HasExtraOffset { get; set; } = false;

            public override string ToString() => Message;
        }

        // Actual data presented to the end user.
        public List<MessageEntry> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, bool isJapanese = false)
        {
            // Set up the correct encoding.
            Encoding encoding = Encoding.GetEncoding("cp1252");

            // If this is a Japanese file, then switch to shift-jis.
            if (isJapanese)
                encoding = Encoding.GetEncoding(932);

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read this file's size.
            uint fileSize = reader.ReadUInt32();

            // Skip an unknown value of 0x08.
            reader.JumpAhead(0x04);

            // Read the offset to this file's message table.
            uint messageTableOffset = reader.ReadUInt32();

            // Set up a list of the unknown offsets.
            List<uint> unknownOffsets = new();

            // Read each value until we reach the message table offset and add it to unknown offsets.
            while (reader.BaseStream.Position < messageTableOffset)
                unknownOffsets.Add(reader.ReadUInt32());

            // Jump to the message table offset. Should already be at this position but just to be safe.
            reader.JumpTo(messageTableOffset);

            // Read the first entry so we know where the string table is.
            uint stringTableOffset = reader.ReadUInt32();

            // Jump back so we don't skip the first entry.
            reader.JumpBehind(0x04);

            // Loop until we hit the string table's position.
            while (reader.BaseStream.Position < stringTableOffset)
            {
                // Set up a new message entry.
                MessageEntry message = new();

                // Flip this message's extra offset boolean if this position is in our unknown offsets list.
                if (unknownOffsets.Contains((uint)reader.BaseStream.Position))
                    message.HasExtraOffset = true;

                // Read this message's string offset.
                uint stringOffset = reader.ReadUInt32();

                // Save our current position so we can jump back for the next message.
                long position = reader.BaseStream.Position;

                // Jump to this message's string offset.
                reader.JumpTo(stringOffset);

                // Set up a length value.
                int length = 0;

                // Read bytes until one is 0, adding to length each time.
                while (reader.ReadByte() != 0)
                    length++;

                // Jump back to the start of the string.
                reader.JumpTo(stringOffset);

                // Use the encoder to read the amount of bytes in length as a shift-jis encoded string.
                message.Message = encoding.GetString(reader.ReadBytes(length));

                // Jump back for the next message.
                reader.JumpTo(position);

                // Save this message.
                Data.Add(message);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath, bool isJapanese = false)
        {
            // Set up the correct encoding.
            Encoding encoding = Encoding.GetEncoding("cp1252");

            // If this is a Japanese file, then switch to shift-jis.
            if (isJapanese)
                encoding = Encoding.GetEncoding(932);

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), true);

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write an unknown value of 0x08.
            writer.Write(0x08);

            // Add an offset for the message table.
            writer.AddOffset("MessageTableOffset");

            // Loop through each message and add an offset for the extra offset table if the extra offset value is set.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                if (Data[dataIndex].HasExtraOffset)
                    writer.AddOffset($"Message{dataIndex}ExtraOffset");

            // Fill in the offset for the message table.
            writer.FillOffset("MessageTableOffset");

            // Loop through each message.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // If this message has the extra offset value set, then fill the previously made offset in.
                if (Data[dataIndex].HasExtraOffset)
                    writer.FillOffset($"Message{dataIndex}ExtraOffset");

                // Add an offset for this message's string.
                writer.AddOffset($"Message{dataIndex}String");
            }

            // Loop through each message to actually write the message strings.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in the offset for this message's string.
                writer.FillOffset($"Message{dataIndex}String");         
                
                // Write this message's string, encoded into shift-jis bytes.
                writer.Write(encoding.GetBytes(Data[dataIndex].Message));

                // Write a null terminator for this string.
                writer.WriteNull();
            }

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
