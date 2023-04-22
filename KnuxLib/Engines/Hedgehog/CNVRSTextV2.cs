using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Finish saving this offset hell. Won't be accurate due to how I've handled the fonts and layout, is that a problem or will I need to rejig it?
    // TODO: Is V2 the right name for this? Assuming Forces is V1.
    public class CNVRSTextV2 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public CNVRSTextV2() { }
        public CNVRSTextV2(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.cnvrs-textV2.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The index for this message table's language.
            /// </summary>
            public byte LanguageIndex { get; set; }

            /// <summary>
            /// The code for this message table's language.
            /// </summary>
            public string LanguageCode { get; set; } = "en";

            /// <summary>
            /// The messages within this table.
            /// </summary>
            public List<CNVRSMessage> Messages { get; set; } = new();
        }

        public class CNVRSMessage
        {
            /// <summary>
            /// The unique index number for this message.
            /// </summary>
            public ulong Index { get; set; }

            /// <summary>
            /// This message's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The actual text of this message.
            /// </summary>
            public string Text { get; set; } = "";

            /// <summary>
            /// The font entry this message should use.
            /// </summary>
            public CNVRSFont? Font { get; set; }

            /// <summary>
            /// The layout entry this message should use.
            /// </summary>
            public CNVRSLayout? Layout { get; set; }

            /// <summary>
            /// The type of the first tag for this message.
            /// </summary>
            public string? TagType { get; set; }

            /// <summary>
            /// The data of the first tag for this message.
            /// </summary>
            public string? TagData { get; set; }

            /// <summary>
            /// The type of the second tag for this message.
            /// </summary>
            public string? TagType2 { get; set; }

            /// <summary>
            /// The data of the second tag for this message.
            /// </summary>
            public string? TagData2 { get; set; }
        }

        public class CNVRSFont
        {
            /// <summary>
            /// The name of this font.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The typeface of this font.
            /// </summary>
            public string Typeface { get; set; } = "";

            /// <summary>
            /// The size of this font.
            /// </summary>
            public float Size { get; set; }

            /// <summary>
            /// The space between lines in this font.
            /// </summary>
            public float LineSpacing { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// The colour of this font, in ARGB format.
            /// </summary>
            public VertexColour? Colour { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// Whether this font has a specific offset to a 0 value, or if the offset itself is set to 0.
            /// </summary>
            public bool HasUnknownNullValue { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CNRVSLayoutHorizontalAlignment
        {
            Left = 0,
            Centre = 1,
            Right = 2,
            CentreSingleLeftMulti = 3
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CNRVSLayoutVerticalAlignment
        {
            Top = 0,
            Middle = 1,
            Bottom = 2
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CNRVSLayoutFitBehaviour
        {
            Normal = 0,
            ScaleDown = 1,
            Condense = 2
        }

        public class CNVRSLayout
        {
            /// <summary>
            /// The name of this layout.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// How text in this layout is horizontally aligned.
            /// </summary>
            public CNRVSLayoutHorizontalAlignment HorizontalAlignment { get; set; }

            /// <summary>
            /// How text in this layout is vertically aligned.
            /// </summary>
            public CNRVSLayoutVerticalAlignment VerticalAlignment { get; set; }

            /// <summary>
            /// Whether text in this layout should line wrap or not.
            /// </summary>
            public bool WordWrap { get; set; }

            /// <summary>
            /// How text in this layout behaves when it overflows its text box.
            /// </summary>
            public CNRVSLayoutFitBehaviour FitBehaviour { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip an unknown value of 0x06.
            reader.JumpAhead(0x01);

            // Read the language index for this file.
            Data.LanguageIndex = reader.ReadByte();

            // Read the message count for this file.
            ushort messageCount = reader.ReadUInt16();

            // Skip an unknown value of 0.
            reader.JumpAhead(0x04);

            // Read the offset to this file's data table.
            long dataTableOffset = reader.ReadInt64();

            // Read this file's language code.
            Data.LanguageCode = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x08);

            // Jump to the light field table.
            reader.JumpTo(dataTableOffset, false);

            // Loop through each message in this file.
            for (ulong i = 0; i < messageCount; i++)
            {
                // Set up a new message entry.
                CNVRSMessage message = new();

                // Read this message's index.
                message.Index = reader.ReadUInt64();

                // Read this message' name.
                message.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read the offset for this message's font data.
                long fontDataOffset = reader.ReadInt64();

                // Read this message's UTF16 encoded text.
                message.Text = Helpers.ReadNullTerminatedStringTableEntry(reader, true, true, 0, true);

                // Read the length of this message's text.
                ulong textLength = reader.ReadUInt64();

                // Read the offset for this message's tag data.
                long tagOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next message.
                long pos = reader.BaseStream.Position;

                // Jump to the offset for this message's font data.
                reader.JumpTo(fontDataOffset, false);

                // Skip an unknown offset that always matches the message's name.
                reader.JumpAhead(0x08);

                // Read this message's font offset.
                long fontOffset = reader.ReadInt64();

                // Read this message's layout offset.
                long layoutOffset = reader.ReadInt64();

                // Skip an unknown value of 0.
                reader.JumpAhead(0x08);

                // Only do this is the font offset actually has a value (some files don't).
                if (fontOffset != 0)
                {
                    // Jump to the font offset.
                    reader.JumpTo(fontOffset, false);

                    // Set up a new font entry.
                    CNVRSFont font = new();

                    // Read this font's name.
                    font.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                    // Read this font's typeface.
                    font.Typeface = Helpers.ReadNullTerminatedStringTableEntry(reader);

                    // Read the offset to this font's size value.
                    long fontSizeOffset = reader.ReadInt64();

                    // Read the offset to this font's line spacing value.
                    long fontLineSpacingOffset = reader.ReadInt64();

                    // Read the offset to this font's unknown floating point value.
                    long fontUnknownOffset1 = reader.ReadInt64();

                    // Read the offset to this font's colour value.
                    long fontColourOffset = reader.ReadInt64();

                    // Read the offset to this font's first unknown integer value.
                    long fontUnknownOffset2 = reader.ReadInt64();

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x08);

                    // Skip an offset to an unknown value that is always 0.
                    reader.JumpAhead(0x08);

                    // Skip 0x18 bytes that are all 0.
                    reader.JumpAhead(0x18);

                    // Check if this font has an offset to a value that is always 0, or if the offset itself is 0.
                    if (reader.ReadInt64() != 0)
                        font.HasUnknownNullValue = true;

                    // Read the offset to this font's second unknown integer value.
                    long fontUnknownOffset3 = reader.ReadInt64();

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x08);

                    // Jump to this font's size offset.
                    reader.JumpTo(fontSizeOffset, false);

                    // Read this font's size value.
                    font.Size = reader.ReadSingle();

                    // Jump to this font's line spacing offset.
                    reader.JumpTo(fontLineSpacingOffset, false);

                    // Read this font's line spacing value.
                    font.LineSpacing = reader.ReadSingle();

                    // Jump to this font's unknown floating point offset.
                    reader.JumpTo(fontUnknownOffset1, false);

                    // Read this font's unknown floating point value.
                    font.UnknownFloat_1 = reader.ReadSingle();

                    // Only do this if this font has a colour offset.
                    if (fontColourOffset != 0)
                    {
                        // Jump to this font's colour offset.
                        reader.JumpTo(fontColourOffset, false);

                        // Initialise this font's colour value.
                        font.Colour = new();

                        // Read this font's colour value as an ARGB setup.
                        font.Colour.Alpha = reader.ReadByte();
                        font.Colour.Red = reader.ReadByte();
                        font.Colour.Green = reader.ReadByte();
                        font.Colour.Blue = reader.ReadByte();
                    }

                    // Jump to this font's first unknown integer offset.
                    reader.JumpTo(fontUnknownOffset2, false);

                    // Read this font's first unknown integer value.
                    font.UnknownUInt32_1 = reader.ReadUInt32();

                    // Jump to this font's first unknown integer offset.
                    reader.JumpTo(fontUnknownOffset3, false);

                    // Read this font's second unknown integer value.
                    font.UnknownUInt32_2 = reader.ReadUInt32();

                    // Save this font into the message.
                    message.Font = font;
                }

                // Only do this is the layout offset actually has a value (some files don't).
                if (layoutOffset != 0)
                {
                    // Jump to the layout offset.
                    reader.JumpTo(layoutOffset, false);

                    // Set up a new layout entry.
                    CNVRSLayout layout = new();

                    // Read this layout's name.
                    layout.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x08);

                    // Read the offset to this layout's first unknown floating point value.
                    long layoutUnknownOffset1 = reader.ReadInt64();

                    // Read the offset to this layout's second unknown floating point value.
                    long layoutUnknownOffset2 = reader.ReadInt64();

                    // Read the offset to this layout's horizontal alignment value.
                    long layoutHorizontalAlignmentOffset = reader.ReadInt64();

                    // Read the offset to this layout's vertical alignment value.
                    long layoutVerticalAlignmentOffset = reader.ReadInt64();

                    // Read the offset to this layout's word wrap value.
                    long layoutWordWrapOffset = reader.ReadInt64();

                    // Read the offset to this layout's fit behaviour value.
                    long layoutFitBehaviourOffset = reader.ReadInt64();

                    // Jump to this layout's first unknown floating point offset.
                    reader.JumpTo(layoutUnknownOffset1, false);

                    // Read this layout's first unknown floating point value.
                    layout.UnknownFloat_1 = reader.ReadSingle();

                    // Jump to this layout's second unknown floating point offset.
                    reader.JumpTo(layoutUnknownOffset2, false);

                    // Read this layout's second unknown floating point value.
                    layout.UnknownFloat_2 = reader.ReadSingle();

                    // Jump to this layout's horizontal alignment offset.
                    reader.JumpTo(layoutHorizontalAlignmentOffset, false);

                    // Read this layout's horizontal alignment value.
                    layout.HorizontalAlignment = (CNRVSLayoutHorizontalAlignment)reader.ReadUInt32();

                    // Jump to this layout's vertical alignment offset.
                    reader.JumpTo(layoutVerticalAlignmentOffset, false);

                    // Read this layout's vertical alignment value.
                    layout.VerticalAlignment = (CNRVSLayoutVerticalAlignment)reader.ReadUInt32();

                    // Only do this if this layout has a word wrap offset.
                    if (layoutWordWrapOffset != 0)
                    {
                        // Jump to this layout's word wrap offset.
                        reader.JumpTo(layoutWordWrapOffset, false);

                        // Read this layout's word wrap value.
                        layout.WordWrap = reader.ReadBoolean();
                    }

                    // Only do this if this layout has a fit behaviour offset.
                    if (layoutFitBehaviourOffset != 0)
                    {
                        // Jump to this layout's fit behaviour offset.
                        reader.JumpTo(layoutFitBehaviourOffset, false);

                        // Read this layout's fit behaviour value.
                        layout.FitBehaviour = (CNRVSLayoutFitBehaviour)reader.ReadUInt32();
                    }

                    message.Layout = layout;
                }

                // Only do this is the tag offset actually has a value (some files don't).
                if (tagOffset != 0)
                {
                    // Jump to the tag offset.
                    reader.JumpTo(tagOffset, false);

                    // Read the type for this message's tag.
                    ulong tagType = reader.ReadUInt64();

                    // Skip an unknown offset that just points to the next eight bytes.
                    reader.JumpAhead(0x08);

                    // Read the tag depending on the type.
                    switch (tagType)
                    {
                        case 1:
                            // Skip an unknown offset that just points to the next eight bytes.
                            reader.JumpAhead(0x08);

                            // Read this message's tag type.
                            message.TagType = Helpers.ReadNullTerminatedStringTableEntry(reader);

                            // Skip an unknown value of 3.
                            reader.JumpAhead(0x08);

                            // Read this message's tag data.
                            message.TagData = Helpers.ReadNullTerminatedStringTableEntry(reader);
                            break;
                        case 2:
                            // Skip an unknown offset that just points ahead by 0x10 bytes.
                            reader.JumpAhead(0x08);

                            // Skip an unknown offset that just points ahead by 0x18 bytes.
                            reader.JumpAhead(0x08);

                            // Read this message's tag type.
                            message.TagType = Helpers.ReadNullTerminatedStringTableEntry(reader);

                            // Skip an unknown value of 3.
                            reader.JumpAhead(0x08);

                            // Read this message's tag data.
                            message.TagData = Helpers.ReadNullTerminatedStringTableEntry(reader);

                            // Read this message's second tag type.
                            message.TagType2 = Helpers.ReadNullTerminatedStringTableEntry(reader);

                            // Skip an unknown value of 3.
                            reader.JumpAhead(0x08);

                            // Read this message's second tag data.
                            message.TagData2 = Helpers.ReadNullTerminatedStringTableEntry(reader);
                            break;
                    }

                }

                // Jump back for the next message.
                reader.JumpTo(pos);

                // Save this message entry.
                Data.Messages.Add(message);
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
            throw new NotImplementedException();

            // Set up our BINAWriter for the gismod file and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write an unknown value of 0x06.
            writer.Write((byte)6);

            writer.Write(Data.LanguageIndex);

            writer.Write((ushort)Data.Messages.Count);

            writer.Write(0x00);

            writer.AddOffset("DataTable", 0x08);

            writer.AddString("LanguageCode", Data.LanguageCode, 0x08);

            writer.Write(0x00ul);

            writer.FillInOffset("DataTable", false, false);

            for (int i = 0; i < Data.Messages.Count; i++)
            {
                writer.Write(Data.Messages[i].Index);

                writer.AddString($"message{i}name", Data.Messages[i].Name, 0x08);

                writer.AddOffset($"message{i}FontData", 0x08);

                writer.AddOffset($"message{i}Text", 0x08);

                writer.Write((ulong)Data.Messages[i].Text.Length);

                writer.AddOffset($"message{i}Tags", 0x08);
            }

            for (int i = 0; i < Data.Messages.Count; i++)
            {
                writer.FillInOffset($"message{i}Text", false, false);

                writer.WriteNullTerminatedStringUTF16(Data.Messages[i].Text);
                writer.FixPadding(0x08);
            }

            for (int i = 0; i < Data.Messages.Count; i++)
            {
                writer.FillInOffset($"message{i}FontData", false, false);

                writer.AddString($"message{i}name", Data.Messages[i].Name, 0x08);

                writer.AddOffset($"message{i}Font", 0x08);

                writer.AddOffset($"message{i}Layout", 0x08);

                writer.Write(0x00ul);
            }

            for (int i = 0; i < Data.Messages.Count; i++)
            {
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }
    }
}
