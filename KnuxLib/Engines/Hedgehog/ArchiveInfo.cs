namespace KnuxLib.Engines.Hedgehog
{
    public class ArchiveInfo : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ArchiveInfo() { }
        public ArchiveInfo(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.archiveinfo.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<ArchiveEntry[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.arcinfo");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
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
            public byte UnknownByte_1 { get; set; } = 0x03;

            /// <summary>
            /// Displays this entry's archive in the debugger.
            /// </summary>
            public override string ToString() => Archive;

            /// <summary>
            /// Initialises this entry with default data.
            /// </summary>
            public ArchiveEntry() { }

            /// <summary>
            /// Initialises this entry with the provided data.
            /// </summary>
            public ArchiveEntry(string archive, byte unknownByte_1)
            {
                Archive = archive;
                UnknownByte_1 = unknownByte_1;
            }

            /// <summary>
            /// Initialises this entry by reading its data from a BinaryReader.
            /// </summary>
            public ArchiveEntry(ExtendedBinaryReader reader, uint archiveByteOffset, int archiveEntryIndex) => Read(reader, archiveByteOffset, archiveEntryIndex);

            /// <summary>
            /// Reads the data for this entry.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, uint archiveByteOffset, int archiveEntryIndex)
            {
                Archive = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                long position = reader.BaseStream.Position;
                reader.JumpTo(archiveByteOffset + archiveEntryIndex, false);
                UnknownByte_1 = reader.ReadByte();
                reader.JumpTo(position);
            }
        }

        // Actual data presented to the end user.
        public ArchiveEntry[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), true);

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Skip an unknown value of 0x01.
            reader.CheckValue(0x01);

            // Read the offset to the end of the string table.
            uint stringTableEnd = reader.ReadUInt32();

            // Read the file's offset size.
            uint offsetSize = reader.ReadUInt32();

            // Set the reader's offset to the value of offsetSize.
            reader.Offset = offsetSize;

            // Skip an unknown value that seems to be the same as stringTableEnd but with offsetSize added to it.
            reader.CheckValue(stringTableEnd + offsetSize);

            // Skip an unknown (potentially padding) value of 0x00.
            reader.CheckValue(0x00);

            // Read the amount of archives in this file.
            Data = new ArchiveEntry[reader.ReadInt32()];

            // Skip an unknown value of 0x0C.
            reader.CheckValue(0x0C);

            // Read the offset to the table of unknown bytes.
            uint archiveByteOffset = reader.ReadUInt32();

            // Loop through and read each entry in this file.
            for (int archiveEntryIndex = 0; archiveEntryIndex < Data.Length; archiveEntryIndex++)
                Data[archiveEntryIndex] = new(reader, archiveByteOffset, archiveEntryIndex);

            // While there is data beyond the string table, it simply consists of a count that is two higher than archiveCount and a table of uints in multiples of 4.

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Create this file through a BinaryWriter, setting the file offset to 0x18.
            ExtendedBinaryWriter writer = new(File.Create(filepath), true) { Offset = 0x18 };

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
            writer.Write(Data.Length);

            // Write an unknown value of 0x0C.
            writer.Write(0x0C);

            // Add an offset for this file's byte table.
            writer.AddOffset("ByteTable");

            // Add the offsets for the string table.
            for (int archiveEntryIndex = 0; archiveEntryIndex < Data.Length; archiveEntryIndex++)
                writer.AddOffset($"Entry{archiveEntryIndex}Archive");

            // Fill in the offset for this file's byte table.
            writer.FillInOffset("ByteTable", false);

            // Fill in the byte table.
            for (int archiveEntryIndex = 0; archiveEntryIndex < Data.Length; archiveEntryIndex++)
                writer.Write(Data[archiveEntryIndex].UnknownByte_1);

            // Align to 0x04.
            writer.FixPadding(0x04);

            // Fill in the archive names.
            for (int archiveEntryIndex = 0; archiveEntryIndex < Data.Length; archiveEntryIndex++)
            {
                writer.FillInOffset($"Entry{archiveEntryIndex}Archive", false);
                writer.WriteNullTerminatedString(Data[archiveEntryIndex].Archive);
            }

            // Align to 0x04.
            writer.FixPadding(0x04);

            // Save the position to the end offset table.
            uint offsetTablePosition = (uint)writer.BaseStream.Position;

            // Write this weird offset table's count.
            writer.Write(Data.Length + 0x02);

            // Write this weird offset table.
            for (int archiveEntryIndex = 0; archiveEntryIndex < Data.Length + 0x02; archiveEntryIndex++)
                writer.Write(0x04 * (archiveEntryIndex + 1));

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Write the offset table position values.
            writer.BaseStream.Position = 0x08;
            writer.Write(offsetTablePosition - 0x18);
            writer.BaseStream.Position = 0x10;
            writer.Write(offsetTablePosition);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
