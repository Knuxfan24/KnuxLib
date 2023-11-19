namespace KnuxLib.Engines.Westwood
{
    public class MessageTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable() { }
        public MessageTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.westwood.messagetable.json", Data);
        }

        // Classes for this format.
        public class Message
        {
            /// <summary>
            /// The index of this message, stored because some files skip values.
            /// </summary>
            public ushort MessageIndex { get; set; }

            /// <summary>
            /// The actual string of this message.
            /// </summary>
            public string MessageValue { get; set; } = "";
        }

        // Actual data presented to the end user.
        public List<Message> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), System.Text.Encoding.Latin1);

            // Read the amount of messages this file has.
            ushort entryCount = reader.ReadUInt16();

            // Read the Message Indices.
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                // Set up a new message.
                Message message = new();

                // Read this message's index.
                message.MessageIndex = reader.ReadUInt16();

                // Save this message.
                Data.Add(message);
            }

            // Read the actual Message Text.
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                // Read the offset to this message's text.
                ushort offset = reader.ReadUInt16();

                // Save our current position so we can jump back afterwards.
                long position = reader.BaseStream.Position;

                // Jump to the offset of this message's text.
                reader.JumpTo(offset);

                // Read this message's text.
                Data[entryIndex].MessageValue = reader.ReadNullTerminatedString();

                // Jump back to where we were.
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
            BinaryWriterEx writer = new(File.Create(filepath), System.Text.Encoding.Latin1);

            // Write the amount of messages this file has.
            writer.Write((ushort)Data.Count);

            // Write the message index table.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.Write(Data[dataIndex].MessageIndex);

            // Write the string offset table.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.AddOffset($"Message{dataIndex}", 0x02);

            // Get a list of the offsets.
            List<uint> offsets = writer.GetOffsets();

            // Write the message strings.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Save our current position.
                long position = writer.BaseStream.Position;

                // Jump back to this message's offset position.
                writer.BaseStream.Position = offsets[dataIndex];

                // Fill in the offset.
                writer.Write((ushort)position);

                // Return to the message position.
                writer.BaseStream.Position = position;

                // Write the message.
                writer.WriteNullTerminatedString(Data[dataIndex].MessageValue);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
