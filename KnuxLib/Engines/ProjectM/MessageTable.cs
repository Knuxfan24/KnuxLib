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
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public string[] Japanese { get; set; } = Array.Empty<string>();

            public string[] English { get; set; } = Array.Empty<string>();

            public string[] German { get; set; } = Array.Empty<string>();

            public string[] French { get; set; } = Array.Empty<string>();

            public string[] Spanish { get; set; } = Array.Empty<string>();

            public string[] Italian { get; set; } = Array.Empty<string>();

            public string[] AmericanFrench { get; set; } = Array.Empty<string>();

            public string[] AmericanSpanish { get; set; } = Array.Empty<string>();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(Stream filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(filepath, System.Text.Encoding.UTF8, true);

            // Read this file's signature.
            reader.ReadSignature(6, "tdpack");
            reader.FixPadding(0x4);

            // Ignore two unknown values.
            reader.JumpAhead(0x4); // Value of FF 00 01 00
            reader.JumpAhead(0x4); // Value of 0x30, might be where the data starts, assuming all this is a header.

            // Read the rest of the header(?)
            uint FileSize = reader.ReadUInt32();
            uint LanguageCount = reader.ReadUInt32();
            reader.JumpAhead(0x4); // Duplicate of LanguageCount.
            reader.JumpAhead(0x4); // Value of 0, likely padding of some sort.
            uint LanguageOffsetTableOffset = reader.ReadUInt32();
            uint LanguageSizeTableOffset = reader.ReadUInt32();
            reader.JumpAhead(0x8); // Value of 0, likely padding of some sort.

            // Jump to the Offset Table, should already be at this location but just to be safe.
            reader.JumpTo(LanguageOffsetTableOffset);

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
            reader.JumpTo(LanguageSizeTableOffset);

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

            // Read all the languages.
            // Japanese
            reader.JumpTo(jpnOffset);
            Data.Japanese = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // English
            reader.JumpTo(enOffset);
            Data.English = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Dutch
            reader.JumpTo(deOffset);
            Data.German = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // French
            reader.JumpTo(frOffset);
            Data.French = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Spanish
            reader.JumpTo(esOffset);
            Data.Spanish = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // Italian
            reader.JumpTo(itOffset);
            Data.Italian = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // American French
            reader.JumpTo(usfrOffset);
            Data.AmericanFrench = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            // American Spanish
            reader.JumpTo(usesOffset);
            Data.AmericanSpanish = reader.ReadNullTerminatedString().Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public override void Save(Stream filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(filepath, System.Text.Encoding.UTF8, true);

            // Write this file's signature.
            writer.Write("tdpack");
            writer.FixPadding(0x4);

            // Write two unknown hardcoded values.
            writer.Write(0xFF000100);
            writer.Write(0x30);

            // Set up the location of and write a placeholder size entry to fill in later.
            long sizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write the language counts, hardcoded to 0x8.
            writer.Write(0x8);
            writer.Write(0x8);
            writer.WriteNulls(0x4);

            // Write the table offsets, just hardcode to 0x30 and 0x50.
            writer.Write(0x30);
            writer.Write(0x50);
            writer.WriteNulls(0x8);

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
            long jpnSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long enSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long deSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long frSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long esSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long itSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long usfrSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            long usesSizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Write the 0x10 bytes of padding following the size table.
            writer.WriteNulls(0x10);

            // Write the string tables.
            WriteLanguage(writer, "jpnOffset", Data.Japanese, jpnSizePos);
            WriteLanguage(writer, "enOffset", Data.English, enSizePos);
            WriteLanguage(writer, "deOffset", Data.German, deSizePos);
            WriteLanguage(writer, "frOffset", Data.French, frSizePos);
            WriteLanguage(writer, "esOffset", Data.Spanish, esSizePos);
            WriteLanguage(writer, "itOffset", Data.Italian, itSizePos);
            WriteLanguage(writer, "usfrOffset", Data.AmericanFrench, usfrSizePos);
            WriteLanguage(writer, "usesOffset", Data.AmericanSpanish, usesSizePos);

            // Fill in the file size.
            writer.BaseStream.Position = sizePos;
            writer.Write((uint)writer.BaseStream.Length);
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
            long currentPos = writer.BaseStream.Position;

            // Fill in the offset to this string table.
            writer.FillOffset(offset);

            // Write every entry (minus the last one as a bodge in my reading code results in an empty entry) with a carriage return.
            for (int i = 0; i < messages.Length - 1; i++)
            {
                writer.Write(messages[i]);
                writer.Write((byte)0x0D);
                writer.Write((byte)0x0A);
            }

            // Calculate this string table's size.
            uint size = (uint)(writer.BaseStream.Position - currentPos);

            // Do the alignment padding.
            writer.FixPadding(0x20);

            // Update currentPos so we can jump back after filling in the size.
            currentPos = writer.BaseStream.Position;

            // Fill in the size.
            writer.BaseStream.Position = sizePosition;
            writer.Write(size);
            writer.BaseStream.Position = currentPos;
        }
    }
}
