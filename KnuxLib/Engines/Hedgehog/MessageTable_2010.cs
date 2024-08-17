using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Change out the reader and write for the actual data to the new setup.
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
            public List<Style> Styles { get; set; } = [];

            /// <summary>
            /// The categories of messages this file has.
            /// </summary>
            public List<Category> Categories { get; set; } = [];
        }

        public class Style
        {
            /// <summary>
            /// The name of this style sheet.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The size of text using this style.
            /// </summary>
            public uint FontSize { get; set; }

            /// <summary>
            /// The red colour for this style.
            /// </summary>
            public byte RedColour { get; set; } = 255;

            /// <summary>
            /// The green colour for this style.
            /// </summary>
            public byte GreenColour { get; set; } = 255;

            /// <summary>
            /// The blue colour for this style.
            /// </summary>
            public byte BlueColour { get; set; } = 255;

            /// <summary>
            /// How text using this style is aligned.
            /// </summary>
            public HorizontalAlignment HorizontalAlignment { get; set; }

            /// <summary>
            /// An unknown byte value that is always 0 in the sonic_2010 versions.
            /// TODO: What is this?
            /// TODO: Is this actually a boolean?
            /// </summary>
            public byte UnknownByte_1 { get; set; }

            public override string ToString() => Name;
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
            public List<MessageEntry> Messages { get; set; } = [];

            public override string ToString() => Name;
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

            public override string ToString() => Name;
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

            // Loop through and read each style sheet.
            for (int styleIndex = 0; styleIndex < styleCount; styleIndex++)
            {
                // Set up a new style sheet entry.
                Style style = new();

                // Read this style sheet's name.
                style.Name = reader.ReadNullPaddedString(reader.ReadByte());

                // Skip an unknown value that is always 1.
                reader.JumpAhead(0x01);

                // Read this style's font size.
                style.FontSize = reader.ReadUInt32();

                // Read this style's RGB colour.
                style.RedColour = reader.ReadByte();
                style.GreenColour = reader.ReadByte();
                style.BlueColour = reader.ReadByte();

                // Read this style's horizontal alignment.
                style.HorizontalAlignment = (HorizontalAlignment)reader.ReadByte();

                // Read this style's unknown byte.
                style.UnknownByte_1 = reader.ReadByte();

                // Save this style sheet.
                Data.Styles.Add(style);
            }

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

            // Loop through and read each category in this file.
            for (int categoryIndex = 0; categoryIndex < categoryCount; categoryIndex++)
            {
                // Set up a new category entry.
                Category category = new();

                // Read this category's name.
                category.Name = reader.ReadNullPaddedString(reader.ReadByte());

                // Read the amount of messages in this category.
                uint messageCount = reader.ReadByte();

                // If this is a Mario and Sonic at the London 2012 Olympic Games file, then read the MessageCount as a uint instead.
                if (version == FormatVersion.william)
                {
                    reader.JumpBehind(0x01);
                    messageCount = reader.ReadUInt32();
                }

                // Loop through and read each message in this category.
                for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
                {
                    // Set up a new message entry.
                    MessageEntry message = new();

                    // Read this message's name.
                    message.Name = reader.ReadNullPaddedString(reader.ReadByte());

                    // Read this message's style sheet reference.
                    message.StyleSheet = reader.ReadNullPaddedString(reader.ReadByte());

                    // Read the amount of bytes that make up this message's UTF16 encoded data.
                    int messageByteCount = reader.ReadInt32();

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
                    utf16Decoder.GetChars(messageBytes, 0, messageByteCount, characters, 0, true);

                    // Convert the char array to a string.
                    message.Message = new string(characters);

                    // Save this message.
                    category.Messages.Add(message);
                }

                // Save this category.
                Data.Categories.Add(category);
            }

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
                    writer.Write((ushort)Data.Styles.Count);

                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    break;
                case FormatVersion.blueblur:
                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    // Write the amount of style sheets in this file.
                    writer.Write((ushort)Data.Styles.Count);

                    break;
            }

            // Loop through and write each style sheet.
            for (int styleIndex = 0; styleIndex < Data.Styles.Count; styleIndex++)
            {
                // Write the size of this style sheet's name.
                writer.Write((byte)Data.Styles[styleIndex].Name.Length);

                // Write this style sheet's name.
                writer.Write(Data.Styles[styleIndex].Name);

                // Write an unknown value that is always 0x01.
                writer.Write((byte)0x01);

                // Write this style sheet's font size.
                writer.Write(Data.Styles[styleIndex].FontSize);

                // Write this style sheet's RGB colour.
                writer.Write(Data.Styles[styleIndex].RedColour);
                writer.Write(Data.Styles[styleIndex].GreenColour);
                writer.Write(Data.Styles[styleIndex].BlueColour);

                // Write this style sheet's horizontal alignment.
                writer.Write((byte)Data.Styles[styleIndex].HorizontalAlignment);

                // Write this style sheet's unknown byte.
                writer.Write(Data.Styles[styleIndex].UnknownByte_1);
            }

            // Write the category count depending on the version.
            switch (version)
            {
                case FormatVersion.sonic_2010:
                case FormatVersion.william:
                    // Write the amount of message categories in this file.
                    writer.Write((ushort)Data.Categories.Count);

                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    break;

                case FormatVersion.blueblur:
                    // Write six null bytes.
                    writer.WriteNulls(0x06);

                    // Write the amount of message categories in this file.
                    writer.Write((ushort)Data.Categories.Count);

                    break;
            }

            // Loop through and write this file's categories.
            for (int categoryIndex = 0; categoryIndex < Data.Categories.Count; categoryIndex++)
            {
                // Write the size of this category's name.
                writer.Write((byte)Data.Categories[categoryIndex].Name.Length);

                // Write this category's name.
                writer.Write(Data.Categories[categoryIndex].Name);

                // Write the amount of messages in this category.
                if (version != FormatVersion.william)
                    writer.Write((byte)Data.Categories[categoryIndex].Messages.Count);
                else
                    writer.Write(Data.Categories[categoryIndex].Messages.Count);

                for (int messageIndex = 0; messageIndex < Data.Categories[categoryIndex].Messages.Count; messageIndex++)
                {
                    // Write the size of this message's name.
                    writer.Write((byte)Data.Categories[categoryIndex].Messages[messageIndex].Name.Length);

                    // Write this message's name.
                    writer.Write(Data.Categories[categoryIndex].Messages[messageIndex].Name);

                    // Write the size of this message's style sheet.
                    writer.Write((byte)Data.Categories[categoryIndex].Messages[messageIndex].StyleSheet.Length);

                    // Write this message's style sheet.
                    writer.Write(Data.Categories[categoryIndex].Messages[messageIndex].StyleSheet);

                    // Write the amount of bytes that make the up the UTF16 encoded string.
                    // TODO: Is it safe to just assume that it will always be twice the size of the decoded string?
                    writer.Write(Data.Categories[categoryIndex].Messages[messageIndex].Message.Length * 2);

                    // Write the length of the decoded string.
                    writer.Write(Data.Categories[categoryIndex].Messages[messageIndex].Message.Length);

                    // Set up a UTF16 Encoder.
                    Encoder utf16Encoder = Encoding.Unicode.GetEncoder();

                    // Set up a byte array to hold the UTF16 encoded bytes.
                    byte[] messageBytes = new byte[Data.Categories[categoryIndex].Messages[messageIndex].Message.Length * 2];

                    // Encode this message into a UTF16 byte array.
                    utf16Encoder.GetBytes(Data.Categories[categoryIndex].Messages[messageIndex].Message.ToCharArray(), messageBytes, true);

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

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
