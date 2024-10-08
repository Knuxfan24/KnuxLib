using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Check Sonic Colours Ultimate and see if they did anything to this format in that.
    public class MessageTable_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable_2010() { }
        public MessageTable_2010(string filepath, FormatVersion version = FormatVersion.sonic_2010, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.messagetable_2010.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<FormatData>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.xtb", version);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public enum FormatVersion
        {
            /// <summary>
            /// Sonic Colours.
            /// </summary>
            sonic_2010 = 0,

            /// <summary>
            /// Sonic Generations.
            /// </summary>
            blueblur = 1,

            /// <summary>
            /// Mario and Sonic at the London 2012 Olympic Games.
            /// </summary>
            william = 2
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The style sheets for this file.
            /// </summary>
            public StyleSheet[] StyleSheets { get; set; } = [];

            /// <summary>
            /// The categories of messages this file has.
            /// </summary>
            public Category[] Categories { get; set; } = [];
        }

        public class StyleSheet
        {
            /// <summary>
            /// The name of this style sheet.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The size of text using this style sheet.
            /// </summary>
            public uint FontSize { get; set; }

            /// <summary>
            /// The colour of this style sheet.
            /// </summary>
            public VertexColour Colour { get; set; } = new();

            /// <summary>
            /// How text using this style sheet is aligned.
            /// </summary>
            public HorizontalAlignment HorizontalAlignment { get; set; }

            /// <summary>
            /// An unknown byte value that is always 0 in the sonic_2010 versions.
            /// TODO: What is this?
            /// TODO: Is this actually a boolean?
            /// TODO: Check if this always 0 in the william versions too.
            /// </summary>
            public byte UnknownByte_1 { get; set; }

            /// <summary>
            /// Displays this style sheet's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this style sheet with default data.
            /// </summary>
            public StyleSheet() { }

            /// <summary>
            /// Initialises this style sheet with the provided data.
            /// </summary>
            public StyleSheet(string name, uint fontSize, VertexColour colour, HorizontalAlignment horizontalAlignment, byte unknownByte_1)
            {
                Name = name;
                FontSize = fontSize;
                Colour = colour;
                HorizontalAlignment = horizontalAlignment;
                UnknownByte_1 = unknownByte_1;
            }

            /// <summary>
            /// Initialises this style sheet by reading its data from a BinaryReader.
            /// </summary>
            public StyleSheet(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this style sheet.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Name = reader.ReadNullPaddedString(reader.ReadByte());
                reader.CheckValue(0x01);
                FontSize = reader.ReadUInt32();
                Colour = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), null);
                HorizontalAlignment = (HorizontalAlignment)reader.ReadByte();
                UnknownByte_1 = reader.ReadByte();
            }

            /// <summary>
            /// Writes the data for this style sheet.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.Write((byte)Name.Length);
                writer.Write(Name);
                writer.Write((byte)0x01);
                writer.Write(FontSize);
                Colour.Write(writer);
                writer.Write((byte)HorizontalAlignment);
                writer.Write(UnknownByte_1);
            }
        }

        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum HorizontalAlignment : byte
        {
            Left = 0,
            Centre = 1,
            Right = 2,
            Distributed = 3
        }

        public class Category
        {
            /// <summary>
            /// The name of this category.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// A list of the messages within this category.
            /// </summary>
            public MessageEntry[] Messages { get; set; } = [];

            /// <summary>
            /// Displays this category's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this category with default data.
            /// </summary>
            public Category() { }

            /// <summary>
            /// Initialises this category with the provided data.
            /// </summary>
            public Category(string name, MessageEntry[] messages)
            {
                Name = name;
                Messages = messages;
            }

            /// <summary>
            /// Initialises this category by reading its data from a BinaryReader.
            /// </summary>
            public Category(ExtendedBinaryReader reader, FormatVersion version = FormatVersion.sonic_2010) => Read(reader, version);

            /// <summary>
            /// Reads the data for this category.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, FormatVersion version = FormatVersion.sonic_2010)
            {
                // Read this category's name.
                Name = reader.ReadNullPaddedString(reader.ReadByte());

                // Read the amount of messages in this category.
                uint messageCount = reader.ReadByte();

                // If this is a Mario and Sonic at the London 2012 Olympic Games file, then read the MessageCount as an integer instead.
                if (version == FormatVersion.william)
                {
                    reader.JumpBehind(0x01);
                    messageCount = reader.ReadUInt32();
                }

                // Initialise the messages array.
                Messages = new MessageEntry[messageCount];

                // Loop through and read each message in this category.
                for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
                    Messages[messageIndex] = new(reader, version);
            }

            /// <summary>
            /// Writes the data for this category.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer, FormatVersion version = FormatVersion.sonic_2010)
            {
                // Write the size of this category's name.
                writer.Write((byte)Name.Length);

                // Write this category's name.
                writer.Write(Name);

                // Write the amount of messages in this category.
                if (version != FormatVersion.william)
                    writer.Write((byte)Messages.Length);
                else
                    writer.Write(Messages.Length);

                // Loop through and write each of the messages in this category.
                for (int messageIndex = 0; messageIndex < Messages.Length; messageIndex++)
                    Messages[messageIndex].Write(writer, version);
            }
        }

        public class MessageEntry
        {
            /// <summary>
            /// The name of this message.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The style sheet this message uses.
            /// </summary>
            public string StyleSheet { get; set; } = "";

            /// <summary>
            /// The contents of this message.
            /// </summary>
            public string Message { get; set; } = "";

            /// <summary>
            /// Displays this message's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this message with default data.
            /// </summary>
            public MessageEntry() { }

            /// <summary>
            /// Initialises this message with the provided data.
            /// </summary>
            public MessageEntry(string name, string styleSheet, string message)
            {
                Name = name;
                StyleSheet = styleSheet;
                Message = message;
            }

            /// <summary>
            /// Initialises this message by reading its data from a BinaryReader.
            /// </summary>
            public MessageEntry(ExtendedBinaryReader reader, FormatVersion version = FormatVersion.sonic_2010) => Read(reader, version);

            /// <summary>
            /// Reads the data for this message.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, FormatVersion version = FormatVersion.sonic_2010)
            {
                // Read this message's name.
                Name = reader.ReadNullPaddedString(reader.ReadByte());

                // Read this message's style sheet reference.
                StyleSheet = reader.ReadNullPaddedString(reader.ReadByte());

                // Read the amount of bytes that make up this message's UTF16 encoded data.
                uint messageByteCount = reader.ReadUInt32();

                // Read the amount of characters this message when decoded.
                uint messageCharacterCount = reader.ReadUInt32();

                // Set up a byte array to read the UTF16 encoded data.
                byte[] messageBytes = new byte[messageByteCount];

                // Read each UTF-16 encoded byte for this message.
                for (int characterIndex = 0; characterIndex < messageByteCount; characterIndex++)
                    messageBytes[characterIndex] = reader.ReadByte();

                // Set up a UTF16 Decoder.
                Decoder utf16Decoder = Encoding.Unicode.GetDecoder();

                // Set up a char array to hold the decoded characters.
                char[] characters = new char[messageCharacterCount];

                // If this is a Sonic Generations XTB file, then we need to endian swap the UTF16 encoded text's bytes.
                if (version == FormatVersion.blueblur)
                {
                    // Loop through every other byte.
                    for (int characterByte = 0; characterByte < messageBytes.Length; characterByte += 2)
                    {
                        // Read this byte and the next one.
                        byte val0 = messageBytes[characterByte];
                        byte val1 = messageBytes[characterByte + 1];

                        // Switch the values around.
                        messageBytes[characterByte] = val1;
                        messageBytes[characterByte + 1] = val0;
                    }
                }

                // Decode the bytes to the char array.
                utf16Decoder.GetChars(messageBytes, 0, (int)messageByteCount, characters, 0, true);

                // Convert the char array to a string.
                Message = new string(characters);
            }

            /// <summary>
            /// Writes the data for this message.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer, FormatVersion version = FormatVersion.sonic_2010)
            {
                // Write the size of this message's name.
                writer.Write((byte)Name.Length);

                // Write this message's name.
                writer.Write(Name);

                // Write the size of this message's style sheet.
                writer.Write((byte)StyleSheet.Length);

                // Write this message's style sheet.
                writer.Write(StyleSheet);

                // Write the amount of bytes that make the up the UTF16 encoded string.
                // TODO: Is it safe to just assume that it will always be twice the size of the decoded string?
                writer.Write(Message.Length * 2);

                // Write the length of the decoded string.
                writer.Write(Message.Length);

                // Set up a UTF16 Encoder.
                Encoder utf16Encoder = Encoding.Unicode.GetEncoder();

                // Set up a byte array to hold the UTF16 encoded bytes.
                byte[] messageBytes = new byte[Message.Length * 2];

                // Encode this message into a UTF16 byte array.
                utf16Encoder.GetBytes(Message.ToCharArray(), messageBytes, true);

                // If this is a Sonic Generations XTB file, then we need to endian swap the UTF16 encoded text's bytes.
                if (version == FormatVersion.blueblur)
                {
                    // Loop through every other byte.
                    for (int characterIndex = 0; characterIndex < messageBytes.Length; characterIndex += 2)
                    {
                        // Read this byte and the next one.
                        byte val0 = messageBytes[characterIndex];
                        byte val1 = messageBytes[characterIndex + 1];

                        // Switch the values around.
                        messageBytes[characterIndex] = val1;
                        messageBytes[characterIndex + 1] = val0;
                    }
                }

                // Write the encoded array.
                writer.Write(messageBytes);
            }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.sonic_2010)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Set up counts for the styles and categories.
            ushort styleCount = 0;
            ushort categoryCount = 0;

            // Skip the very start of the file, as it's always eight 0xFF bytes followed by 0x0200.
            reader.JumpAhead(0x0A);

            // Read the style count depending on the version.
            switch (version)
            {
                case FormatVersion.sonic_2010:
                case FormatVersion.william:
                    // Read the amount of style sheets in this file.
                    styleCount = reader.ReadUInt16();

                    // Skip six null bytes.
                    reader.JumpAhead(0x06);

                    break;

                case FormatVersion.blueblur:
                    // Switch the endianness to Big Endian.
                    reader.IsBigEndian = true;

                    // Skip six null bytes.
                    reader.JumpAhead(0x06);

                    // Read the amount of style sheets in this file.
                    styleCount = reader.ReadUInt16();

                    break;
            }

            // Initialise the style sheets array.
            Data.StyleSheets = new StyleSheet[styleCount];

            // Loop through and read each style sheet.
            for (int styleSheetIndex = 0; styleSheetIndex < styleCount; styleSheetIndex++)
                Data.StyleSheets[styleSheetIndex] = new(reader);

            // Read the category count depending on the version.
            switch (version)
            {
                case FormatVersion.sonic_2010:
                case FormatVersion.william:
                    // Read the amount of categories in this file.
                    categoryCount = reader.ReadUInt16();

                    // Skip six null bytes.
                    reader.JumpAhead(0x06);

                    break;

                case FormatVersion.blueblur:
                    // Skip six null bytes.
                    reader.JumpAhead(0x06);

                    // Read the amount of categories in this file.
                    categoryCount = reader.ReadUInt16();

                    break;
            }

            // Initialise the categories array.
            Data.Categories = new Category[categoryCount];

            // Loop through and read each category in this file.
            for (int categoryIndex = 0; categoryIndex < categoryCount; categoryIndex++)
                Data.Categories[categoryIndex] = new(reader, version);

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.sonic_2010)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // If we're writing a Sonic Generations XTB, then swap to big endian.
            if (version == FormatVersion.blueblur)
                writer.IsBigEndian = true;

            // Write the first eight bytes that are always 0xFF.
            writer.Write(0xFFFFFFFFFFFFFFFFL);

            // Write the next two bytes that are always 0x0200.
            writer.Write((ushort)0x0002);

            // Write the style count depending on the version.
            switch (version)
            {
                case FormatVersion.sonic_2010:
                case FormatVersion.william:
                    // Write the amount of style sheets in this file.
                    writer.Write((ushort)Data.StyleSheets.Length);

                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    break;
                case FormatVersion.blueblur:
                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    // Write the amount of style sheets in this file.
                    writer.Write((ushort)Data.StyleSheets.Length);

                    break;
            }

            // Loop through and write each style sheet.
            for (int styleSheetIndex = 0; styleSheetIndex < Data.StyleSheets.Length; styleSheetIndex++)
                Data.StyleSheets[styleSheetIndex].Write(writer);

            // Write the category count depending on the version.
            switch (version)
            {
                case FormatVersion.sonic_2010:
                case FormatVersion.william:
                    // Write the amount of message categories in this file.
                    writer.Write((ushort)Data.Categories.Length);

                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    break;

                case FormatVersion.blueblur:
                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    // Write the amount of message categories in this file.
                    writer.Write((ushort)Data.Categories.Length);

                    break;
            }

            // Loop through and write this file's categories.
            for (int categoryIndex = 0; categoryIndex < Data.Categories.Length; categoryIndex++)
                Data.Categories[categoryIndex].Write(writer, version);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
