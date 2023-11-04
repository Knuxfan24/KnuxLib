namespace KnuxLib.Engines.ProjectM
{
    // TODO: What's with all the backslashes in the messages?
    public class MessageTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable() { }
        public MessageTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.projectm.messagetable.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            // The Japanese messages in this table.
            public string[] Japanese { get; set; } = Array.Empty<string>();

            // The English messages in this table.
            public string[] English { get; set; } = Array.Empty<string>();

            // The German messages in this table.
            public string[] German { get; set; } = Array.Empty<string>();

            // The French messages in this table.
            public string[] French { get; set; } = Array.Empty<string>();

            // The Spanish messages in this table.
            public string[] Spanish { get; set; } = Array.Empty<string>();

            // The Italian messages in this table.
            public string[] Italian { get; set; } = Array.Empty<string>();

            // The American French messages in this table.
            public string[] AmericanFrench { get; set; } = Array.Empty<string>();

            // The American Spanish messages in this table.
            public string[] AmericanSpanish { get; set; } = Array.Empty<string>();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), System.Text.Encoding.UTF8, true);

            // Read this file's signature.
            reader.ReadSignature(0x06, "tdpack");
            reader.FixPadding(0x04);

            // Ignore two unknown values.
            reader.JumpAhead(0x04); // Value of FF 00 01 00
            reader.JumpAhead(0x04); // Value of 0x30, might be where the data starts, assuming all this is a header.

            // Read the rest of the header(?)
            uint fileSize = reader.ReadUInt32();
            uint languageCount = reader.ReadUInt32();
            reader.JumpAhead(0x04); // Duplicate of LanguageCount.
            reader.JumpAhead(0x04); // Value of 0, likely padding of some sort.
            uint languageOffsetTableOffset = reader.ReadUInt32();
            uint languageSizeTableOffset = reader.ReadUInt32();
            reader.JumpAhead(0x08); // Value of 0, likely padding of some sort.

            // Jump to the Offset Table, should already be at this location but just to be safe.
            reader.JumpTo(languageOffsetTableOffset);

            // Read all the offsets for each language.
            uint jpnOffset = reader.ReadUInt32();
            uint enOffset = reader.ReadUInt32();
            uint deOffset = reader.ReadUInt32();
            uint frOffset = reader.ReadUInt32();
            uint esOffset = reader.ReadUInt32();
            uint itOffset = reader.ReadUInt32();
            uint usfrOffset = reader.ReadUInt32();
            uint usesOffset = reader.ReadUInt32();

            // Jump to the Size Table, should already be at this location but just to be safe.
            reader.JumpTo(languageSizeTableOffset);

            // Read all the length values for each language.
            uint jpnLength = reader.ReadUInt32();
            uint enLength = reader.ReadUInt32();
            uint deLength = reader.ReadUInt32();
            uint frLength = reader.ReadUInt32();
            uint esLength = reader.ReadUInt32();
            uint itLength = reader.ReadUInt32();
            uint usfrLength = reader.ReadUInt32();
            uint usesLength = reader.ReadUInt32();

            reader.JumpAhead(0x10); // Skip 0x10 bytes of nulls that are likely padding.

            // Read the Japanese messages.
            reader.JumpTo(jpnOffset);
            Data.Japanese = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the English messages.
            reader.JumpTo(enOffset);
            Data.English = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the Dutch messages.
            reader.JumpTo(deOffset);
            Data.German = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the French messages.
            reader.JumpTo(frOffset);
            Data.French = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the Spanish messages.
            reader.JumpTo(esOffset);
            Data.Spanish = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the Italian messages.
            reader.JumpTo(itOffset);
            Data.Italian = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the American French messages.
            reader.JumpTo(usfrOffset);
            Data.AmericanFrench = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Read the American Spanish messages.
            reader.JumpTo(usesOffset);
            Data.AmericanSpanish = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

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
            BinaryWriterEx writer = new(File.Create(filepath), System.Text.Encoding.UTF8, true);

            // Write this file's signature.
            writer.Write("tdpack");
            writer.FixPadding(0x04);

            // Write two unknown hardcoded values.
            writer.Write(0xFF000100);
            writer.Write(0x30);

            // Set up the location of and write a placeholder size entry to fill in later.
            long sizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write the language counts, hardcoded to 0x08.
            writer.Write(0x08);
            writer.Write(0x08);
            writer.WriteNulls(0x04);

            // Write the table offsets, just hardcode to 0x30 and 0x50.
            writer.Write(0x30);
            writer.Write(0x50);
            writer.WriteNulls(0x08);

            // Add the offsets for each language.
            writer.AddOffset("jpnOffset");
            writer.AddOffset("enOffset");
            writer.AddOffset("deOffset");
            writer.AddOffset("frOffset");
            writer.AddOffset("esOffset");
            writer.AddOffset("itOffset");
            writer.AddOffset("usfrOffset");
            writer.AddOffset("usesOffset");

            // Set up the locations and write language size entries to fill in later.
            long jpnSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long enSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long deSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long frSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long esSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long itSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long usfrSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            long usesSizePosition = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write the 0x10 bytes of padding following the size table.
            writer.WriteNulls(0x10);

            // Write the string tables.
            WriteLanguage(writer, "jpnOffset", Data.Japanese, jpnSizePosition);
            WriteLanguage(writer, "enOffset", Data.English, enSizePosition);
            WriteLanguage(writer, "deOffset", Data.German, deSizePosition);
            WriteLanguage(writer, "frOffset", Data.French, frSizePosition);
            WriteLanguage(writer, "esOffset", Data.Spanish, esSizePosition);
            WriteLanguage(writer, "itOffset", Data.Italian, itSizePosition);
            WriteLanguage(writer, "usfrOffset", Data.AmericanFrench, usfrSizePosition);
            WriteLanguage(writer, "usesOffset", Data.AmericanSpanish, usesSizePosition);

            // Fill in the file size.
            writer.BaseStream.Position = sizePosition;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Writes the specified language string table and fills in the approriate data.
        /// </summary>
        /// <param name="writer">The BinaryWriterEx we're using.</param>
        /// <param name="offset">The offset to fill in.</param>
        /// <param name="messages">The array of messages to write.</param>
        /// <param name="sizePosition">The size value to fill in.</param>
        private static void WriteLanguage(BinaryWriterEx writer, string offset, string[] messages, long sizePosition)
        {
            // Save the start position of this string table for later maths.
            long currentPosition = writer.BaseStream.Position;

            // Fill in the offset to this string table.
            writer.FillOffset(offset);

            // Write every entry (minus the last one as a bodge in my reading code results in an empty entry) with a carriage return.
            for (int messageIndex = 0; messageIndex < messages.Length - 1; messageIndex++)
            {
                writer.Write(messages[messageIndex]);
                writer.Write((byte)0x0D);
                writer.Write((byte)0x0A);
            }

            // Calculate this string table's size.
            uint size = (uint)(writer.BaseStream.Position - currentPosition);

            // Do the alignment padding.
            writer.FixPadding(0x20);

            // Update currentPos so we can jump back after filling in the size.
            currentPosition = writer.BaseStream.Position;

            // Fill in the size.
            writer.BaseStream.Position = sizePosition;
            writer.Write(size);
            writer.BaseStream.Position = currentPosition;
        }
    }
}
