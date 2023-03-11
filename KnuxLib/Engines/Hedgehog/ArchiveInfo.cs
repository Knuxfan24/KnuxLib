namespace KnuxLib.Engines.Hedgehog
{
    // TODO: See if the unknown values, especially the large table at the end of the file, do anything.
    public class ArchiveInfo : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ArchiveInfo() { }
        public ArchiveInfo(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        public class ArchiveEntry
        {
            /// <summary>
            /// The name of the archive referenced in this entry.
            /// </summary>
            public string Archive { get; set; } = "";

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_1 { get; set; }

            public override string ToString() => Archive;
        }

        // Actual data presented to the end user.
        public List<ArchiveEntry> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read the offset to the end of the string table.
            uint stringTableEnd = reader.ReadUInt32();

            // Read the file's offset size.
            uint offsetSize = reader.ReadUInt32();

            // Set the reader's offset to the value of offsetSize.
            reader.Offset = offsetSize;

            // Skip an unknown value that seems to be the same as stringTableEnd but with offsetSize added to it.
            reader.JumpAhead(0x04);

            // Skip an unknown (potentially padding) value of 0x0.
            reader.JumpAhead(0x04);

            // Read the amount of archives in this file.
            uint archiveCount = reader.ReadUInt32();

            // Skip an unknown value of 0x0C.
            reader.JumpAhead(0x04);

            // Read the offset to the table of unknown bytes.
            uint archiveByteOffset = reader.ReadUInt32();

            // Read each archive in this file.
            for (int i = 0; i < archiveCount; i++)
            {
                // Set up a new archive entry.
                ArchiveEntry entry = new();

                // Read this archive's name.
                entry.Archive = Helpers.ReadNullTerminatedStringTableEntry(reader, true);

                // Save our current position.
                long pos = reader.BaseStream.Position;

                // Jump to the archive byte table, plus the index of whatever archive we're currently on.
                reader.JumpTo(archiveByteOffset + i, true);

                // Read this archive's unknown byte.
                entry.UnknownByte_1 = reader.ReadByte();

                // Jump back to the saved position.
                reader.JumpTo(pos);

                // Save this archive entry.
                Data.Add(entry);
            }

            // While there is data beyond the string table, it simply consists of a count that is two higher than archiveCount and a table of uints in multiples of 4.

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
            BinaryWriterEx writer = new(File.Create(filepath), true) { Offset = 0x18 };

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            // Write a placeholder for this file's stringTableEnd value.
            writer.Write("SEND");

            // Write this file's offset size.
            writer.Write(0x18);

            // Write a placeholder for this file's stringTableEnd value with the offset size applied..
            writer.Write("SEND");

            // Write an unknown (potentially padding) value of 0x00.
            writer.Write(0x00);

            // Write this file's archive count.
            writer.Write(Data.Count);

            // Write an unknown value of 0x0C.
            writer.Write(0x0C);

            // Add an offset for this file's byte table.
            writer.AddOffset("ByteTable");

            // Add the offsets for the string table.
            for (int i = 0; i < Data.Count; i++)
                writer.AddOffset($"Entry{i}Archive");

            // Fill in the offset for this file's byte table.
            writer.FillOffset("ByteTable", true);

            // Fill in the byte table.
            for (int i = 0; i < Data.Count; i++)
                writer.Write(Data[i].UnknownByte_1);

            // Align to 0x04.
            writer.FixPadding(0x04);

            // Fill in the archive names.
            for (int i = 0; i < Data.Count; i++)
            {
                writer.FillOffset($"Entry{i}Archive", true);
                writer.WriteNullTerminatedString(Data[i].Archive);
            }

            // Align to 0x04.
            writer.FixPadding(0x04);

            // Save the position to the end offset table.
            uint offsetTablePosition = (uint)writer.BaseStream.Position;

            // Write this weird offset table's count.
            writer.Write(Data.Count + 0x02);

            // Write this weird offset table.
            for (int i = 0; i < Data.Count + 0x02; i++)
                writer.Write(0x04 * (i + 1));

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Write the offset table position values.
            writer.BaseStream.Position = 0x08;
            writer.Write(offsetTablePosition - 0x18);
            writer.BaseStream.Position = 0x10;
            writer.Write(offsetTablePosition);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
