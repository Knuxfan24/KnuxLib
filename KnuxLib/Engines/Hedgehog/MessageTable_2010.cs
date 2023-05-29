using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace KnuxLib.Engines.Hedgehog
{
    public class MessageTable_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable_2010() { }
        public MessageTable_2010(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.messagetable_2010.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The style sheets for this file.
            /// </summary>
            public List<Style> Styles { get; set; } = new();

            /// <summary>
            /// The categories of messages this file has.
            /// </summary>
            public List<Category> Categories { get; set; } = new();
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
            public List<MessageEntry> Messages { get; set; } = new();

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
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Skip the very start of the file, as it's always eight 0xFF bytes followed by 0x0200.
            reader.JumpAhead(0x0A);

            // Read the amount of style sheets in this file.
            ushort StyleCount = reader.ReadUInt16();

            // Skip six null bytes.
            reader.JumpAhead(0x06);

            // Loop through and read each style sheet.
            for (int i = 0; i < StyleCount; i++)
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

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x01);

                // Save this style sheet.
                Data.Styles.Add(style);
            }

            // Read the amount of categories in this file.
            ushort CategoryCount = reader.ReadUInt16();

            // Skip six null bytes.
            reader.JumpAhead(0x06);

            // Loop through and read each category in this file.
            for (int cat = 0; cat < CategoryCount; cat++)
            {
                // Set up a new category entry.
                Category category = new();

                // Read this category's name.
                category.Name = reader.ReadNullPaddedString(reader.ReadByte());

                // Read the amount of messages in this category.
                byte MessageCount = reader.ReadByte();

                // Loop through and read each message in this category.
                for (int i = 0; i < MessageCount; i++)
                {
                    // Set up a new message entry.
                    MessageEntry message = new();

                    // Read this message's name.
                    message.Name = reader.ReadNullPaddedString(reader.ReadByte());

                    // Read this message's style sheet reference.
                    message.StyleSheet = reader.ReadNullPaddedString(reader.ReadByte());

                    // Read the amount of bytes that make up this message's UTF16 encoded data.
                    int MessageByteCount = reader.ReadInt32();

                    // Read the amount of characters this message when decoded.
                    uint MessageCharacterCount = reader.ReadUInt32();

                    // Set up a byte array to read the UTF16 encoded data.
                    byte[] MessageBytes = new byte[MessageByteCount];

                    // Read each UTF-16 encoded byte for this message.
                    for (int chara = 0; chara < MessageByteCount; chara++)
                        MessageBytes[chara] = reader.ReadByte();

                    // Set up a UTF16 Decoder.
                    Decoder utf16Decoder = Encoding.Unicode.GetDecoder();

                    // Set up a char array to hold the decoded characters.
                    char[] characters = new char[MessageCharacterCount];

                    // Decode the bytes to the char array.
                    utf16Decoder.GetChars(MessageBytes, 0, MessageByteCount, characters, 0, true);

                    // Convert the char array to a string.
                    message.Message = new string(characters);

                    // Save this message.
                    category.Messages.Add(message);
                }
                
                // Save this category.
                Data.Categories.Add(category);
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
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the first eight bytes that are always 0xFF.
            writer.Write(0xFFFFFFFFFFFFFFFFL);

            // Write the next two bytes that are always 0x0200.
            writer.Write((ushort)0x0002);

            // Write the amount of style sheets in this file.
            writer.Write((ushort)Data.Styles.Count);

            // Write six null bytes.
            writer.WriteNulls(0x06);

            // Loop through and write each style sheet.
            for (int i = 0; i < Data.Styles.Count; i++)
            {
                // Write the size of this style sheet's name.
                writer.Write((byte)Data.Styles[i].Name.Length);

                // Write this style sheet's name.
                writer.Write(Data.Styles[i].Name);

                // Write an unknown value that is always 0x01.
                writer.Write((byte)0x01);

                // Write this style sheet's font size.
                writer.Write(Data.Styles[i].FontSize);

                // Write this style sheet's RGB colour.
                writer.Write(Data.Styles[i].RedColour);
                writer.Write(Data.Styles[i].GreenColour);
                writer.Write(Data.Styles[i].BlueColour);

                // Write this style sheet's horizontal alignment.
                writer.Write((byte)Data.Styles[i].HorizontalAlignment);

                // Write an unknown value that is always 0.
                writer.Write((byte)0x00);
            }

            // Write the amount of message categories in this file.
            writer.Write((ushort)Data.Categories.Count);

            // Write six null bytes.
            writer.WriteNulls(0x06);

            // Loop through and write this file's categories.
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                // Write the size of this category's name.
                writer.Write((byte)Data.Categories[i].Name.Length);

                // Write this category's name.
                writer.Write(Data.Categories[i].Name);

                // Write the amount of messages in this category.
                writer.Write((byte)Data.Categories[i].Messages.Count);

                for (int m = 0; m < Data.Categories[i].Messages.Count; m++)
                {
                    // Write the size of this message's name.
                    writer.Write((byte)Data.Categories[i].Messages[m].Name.Length);

                    // Write this message's name.
                    writer.Write(Data.Categories[i].Messages[m].Name);

                    // Write the size of this message's style sheet.
                    writer.Write((byte)Data.Categories[i].Messages[m].StyleSheet.Length);

                    // Write this message's style sheet.
                    writer.Write(Data.Categories[i].Messages[m].StyleSheet);

                    // Write the amount of bytes that make the up the UTF16 encoded string.
                    // TODO: Is it safe to just assume that it will always be twice the size of the decoded string?
                    writer.Write(Data.Categories[i].Messages[m].Message.Length * 2);

                    // Write the length of the decoded string.
                    writer.Write(Data.Categories[i].Messages[m].Message.Length);

                    // Set up a UTF16 Encoder.
                    Encoder utf16Encoder = Encoding.Unicode.GetEncoder();

                    // Set up a byte array to hold the UTF16 encoded bytes.
                    byte[] messageBytes = new byte[Data.Categories[i].Messages[m].Message.Length * 2];

                    // Encode this message into a UTF16 byte array.
                    utf16Encoder.GetBytes(Data.Categories[i].Messages[m].Message.ToCharArray(), messageBytes, true);

                    // Write the encoded array.
                    writer.Write(messageBytes);
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
