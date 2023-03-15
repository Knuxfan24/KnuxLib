namespace KnuxLib.Engines.Westwood
{
    // TODO: The message indices sometimes skip values. Is this a problem?
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

        // Actual data presented to the end user.
        public List<string> Data = new();

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

            // Skip the Message Indices.
            for (int i = 0; i < entryCount; i++)
                reader.JumpAhead(0x02);

            // Read the actual Message Text.
            for (int i = 0; i < entryCount; i++)
            {
                // Read the offset to this message's text.
                ushort offset = reader.ReadUInt16();

                // Save our current position so we can jump back afterwards.
                long pos = reader.BaseStream.Position;

                // Jump to the offset of this message's text.
                reader.JumpTo(offset);

                // Read this message's text.
                Data.Add(reader.ReadNullTerminatedString());

                // Jump back to where we were.
                reader.JumpTo(pos);
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
            for (int i = 0; i < Data.Count; i++)
                writer.Write((ushort)i);

            // Write the string offset table.
            for (int i = 0; i < Data.Count; i++)
                writer.AddOffset($"Message{i}", 0x02);

            // Get a list of the offsets.
            List<uint> offsets = writer.GetOffsets();

            // Write the message strings.
            for (int i = 0; i < Data.Count; i++)
            {
                // Save our current position.
                long pos = writer.BaseStream.Position;

                // Jump back to this message's offset position.
                writer.BaseStream.Position = offsets[i];

                // Fill in the offset.
                writer.Write((ushort)pos);

                // Return to the message position.
                writer.BaseStream.Position = pos;

                // Write the message.
                writer.WriteNullTerminatedString(Data[i]);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
