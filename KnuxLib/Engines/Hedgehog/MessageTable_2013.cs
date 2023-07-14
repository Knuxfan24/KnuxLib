﻿namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Fix saving so that both instances are binary identical to the source files.
    // TODO: Figure out all of the unknown values.
    // TODO: Properly test stuff when HedgeArcPack gets Lost World pac writing fixed.
    public class MessageTable_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable_2013() { }
        public MessageTable_2013(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.messagetable_2013.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// The categories this file has in it.
            /// </summary>
            public List<Category> Categories { get; set; } = new();
        }

        public class Category
        {
            /// <summary>
            /// The name of this category.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The messages this category has within it.
            /// </summary>
            public List<MessageEntry> Messages { get; set; } = new();

            public override string ToString() => Name;
        }

        public class MessageEntry
        {
            /// <summary>
            /// The name of this message.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// The text this message displays.
            /// </summary>
            public string Message { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// The remap entries this message has.
            /// </summary>
            public List<RemapEntry>? Remaps { get; set; }

            public override string ToString() => Name;
        }

        public class RemapEntry
        {
            /// <summary>
            /// The index of the character this remap entry replaces.
            /// </summary>
            public ushort CharacterIndex { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_1 { get; set; }

            /// <summary>
            /// The data for this remap.
            /// </summary>
            public object RemapData { get; set; }
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
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip an unknown value of 0x02.
            reader.JumpAhead(0x02);

            // Read the amount of categories in this file.
            ushort CategoryCount = reader.ReadUInt16();

            // Read the first unknown integer value.
            Data.UnknownUInt32_1 = reader.ReadUInt32();

            // Read the second unknown integer value.
            Data.UnknownUInt32_2 = reader.ReadUInt32();

            // Skip an unknown value that is the same as the second unknown integer value.
            reader.JumpAhead(0x04);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x04);

            // Loop through and read each category.
            for (int i = 0; i < CategoryCount; i++)
            {
                // Set up a new category.
                Category category = new();

                // Read the offset to this category's data.
                uint CategoryDataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this category is read.
                long pos = reader.BaseStream.Position;

                // Jump to the category's data offset.
                reader.JumpTo(CategoryDataOffset, false);

                // Read this category's name.
                category.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                // Read this category's unknown integer value.
                category.UnknownUInt32_1 = reader.ReadUInt32();

                // Read the offset to this category's messages.
                uint messagesOffset = reader.ReadUInt32();

                // Read the count of messages in this category.
                uint messageCount = reader.ReadUInt32();

                // Skip an unknown value that matches the message count.
                reader.JumpAhead(0x04);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Jump to this category's message data.
                reader.JumpTo(messagesOffset, false);

                // Loop through and read each message in this category.
                for (int m = 0; m < messageCount; m++)
                {
                    // Set up a new message.
                    MessageEntry message = new();

                    // Read the offset to this message's data.
                    uint dataOffset = reader.ReadUInt32();

                    // Save our current position so we can jump back for the next message.
                    long currentPos = reader.BaseStream.Position;

                    // Jump to this message's data.
                    reader.JumpTo(dataOffset, false);

                    // Read this message's name, if it has one.
                    message.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                    // Read this message's UTF16 encoded text.
                    message.Message = Helpers.ReadNullTerminatedStringTableEntry(reader, false, true, 0, true);

                    // Read the offset to this message's remap data.
                    uint RemapOffset = reader.ReadUInt32();

                    // Read this message's remap count, this is always 0 or 1, but the game might support multiple (assuming this is a count).
                    uint RemapCount = reader.ReadUInt32();

                    // Read this message's first unknown integer value.
                    message.UnknownUInt32_1 = reader.ReadUInt32();

                    // Skip 0x16 null bytes.
                    reader.JumpAhead(0x16);

                    // Skip a value that appears to be the length of the message string minus 1.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value 0x02.
                    reader.JumpAhead(0x04);

                    // Read this message's second unknown integer value.
                    message.UnknownUInt32_2 = reader.ReadUInt32();

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x02);

                    // Skip a value that appears to be the length of the message string minus 1.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value 0x01.
                    reader.JumpAhead(0x04);

                    // Read this message's third unknown integer value.
                    message.UnknownUInt32_3 = reader.ReadUInt32();

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x02);

                    // Skip a value that appears to be the length of the message string minus 1.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value of 0x01.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x02);

                    // Skip a value that appears to be the length of the message string minus 1.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value of 0x03.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x04);

                    // If this message has any remaps, then read them as well.
                    if (RemapOffset != 0)
                    {
                        // Define the remap list.
                        message.Remaps = new();

                        // Jump to the previously read remap offset.
                        reader.JumpTo(RemapOffset, false);

                        // Loop through each remap in this message.
                        for (int r = 0; r < RemapCount; r++)
                        {
                            // Read the offset to this remap's data.
                            uint RemapDataOffset = reader.ReadUInt32();

                            // Save our current position to jump back for the next remap.
                            long remapPos = reader.BaseStream.Position;

                            // Jump to the remap offset.
                            reader.JumpTo(RemapDataOffset, false);

                            // Create a new remap.
                            RemapEntry remap = new();

                            // Read this remap's character index.
                            remap.CharacterIndex = reader.ReadUInt16();

                            // Read this remap's unknown short value.
                            remap.UnknownUShort_1 = reader.ReadUInt16();

                            // Read this remap's data type.
                            uint remapDataType = reader.ReadUInt32();

                            // Read the next four bytes as either a byte array or one value depending on the remap type.
                            switch (remapDataType)
                            {
                                case 4: remap.RemapData = reader.ReadBytes(0x04); break;
                                case 5: remap.RemapData = reader.ReadUInt32(); break;
                            }

                            // Save this remap entry.
                            message.Remaps.Add(remap);

                            // Jump back for the next remap entry.
                            reader.JumpTo(remapPos);
                        }
                    }

                    // Save this message.
                    category.Messages.Add(message);

                    // Jump back for the next message.
                    reader.JumpTo(currentPos);
                }

                // Save this category.
                Data.Categories.Add(category);

                // Jump back for the next category.
                reader.JumpTo(pos);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// TODO: Fix the BINA Footer being incorrect. Tested the English versions of text_common_text.xtb2 and text_ev_msg_text.xtb2, common was identical but ev wasn't.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter for the gismod file and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write an unknown value of 0x02.
            writer.Write((ushort)0x02);

            // Write the amount of categories in this file.
            writer.Write((ushort)Data.Categories.Count);

            // Write this file's first unknown integer value.
            writer.Write(Data.UnknownUInt32_1);

            // Write this file's second unknown integer value.
            writer.Write(Data.UnknownUInt32_2);

            // Write another copy of this file's second unknown integer value.
            writer.Write(Data.UnknownUInt32_2);

            // Write an unknown value of 0.
            writer.WriteNulls(0x04);

            // Add an offset table for the categories.
            writer.AddOffsetTable($"Categories", (uint)Data.Categories.Count);

            // Loop through and write each category entry.
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                // Fill in this category's offset.
                writer.FillInOffset($"Categories_{i}", false, false);

                // Add this category's name.
                writer.AddString($"Category{i}Name", Data.Categories[i].Name);

                // Write this category's unknown integer value.
                writer.Write(Data.Categories[i].UnknownUInt32_1);

                // Add an offset to this category's message table.
                writer.AddOffset($"Category{i}Messages");

                // Write this category's message count.
                writer.Write(Data.Categories[i].Messages.Count);

                // Write another copy of this category's message count.
                writer.Write(Data.Categories[i].Messages.Count);

                // Write an unknown value of 0.
                writer.Write(0x00);
            }

            // Loop through and write each category's messages.
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                // Fill in the offset for this category's message table.
                writer.FillInOffset($"Category{i}Messages", false, false);

                // Add an offset table for this category's actual messages.
                writer.AddOffsetTable($"Category{i}Messages", (uint)Data.Categories[i].Messages.Count);

                // Loop through and write each message in this category.
                for (int m = 0; m < Data.Categories[i].Messages.Count; m++)
                {
                    // Fill in this message's offset.
                    writer.FillInOffset($"Category{i}Messages_{m}", false, false);

                    // Add this message's name.
                    writer.AddString($"Category{i}Message{m}Name", Data.Categories[i].Messages[m].Name);

                    // Add an offset for this message's UTF16 encoded text.
                    writer.AddOffset($"Category{i}Message{m}Message");

                    // If this message has remaps, then add an offset and write the count of them.
                    if (Data.Categories[i].Messages[m].Remaps != null)
                    {
                        writer.AddOffset($"Category{i}Message{m}Remaps");
                        writer.Write(Data.Categories[i].Messages[m].Remaps.Count);
                    }

                    // If not, then just write eight nulls.
                    else
                    {
                        writer.WriteNulls(0x08);
                    }

                    // Write this message's first unknown integer value.
                    writer.Write(Data.Categories[i].Messages[m].UnknownUInt32_1);

                    // Write 0x16 null bytes.
                    writer.WriteNulls(0x16);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Categories[i].Messages[m].Message.Length - 1));

                    // Write an unknown value of 0x02.
                    writer.Write(0x02);

                    // Write this message's second unknown integer value.
                    writer.Write(Data.Categories[i].Messages[m].UnknownUInt32_2);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Categories[i].Messages[m].Message.Length - 1));

                    // Write an unknown value of 0x01.
                    writer.Write(0x01);

                    // Write this message's third unknown integer value.
                    writer.Write(Data.Categories[i].Messages[m].UnknownUInt32_3);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Categories[i].Messages[m].Message.Length - 1));

                    // Write four null bytes.
                    writer.WriteNulls(0x04);

                    // Write an unknown value of 0x01.
                    writer.Write(0x01);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Categories[i].Messages[m].Message.Length - 1));

                    // Write an unknown value of 0x03.
                    writer.Write(0x03);

                    // Write four null bytes.
                    writer.WriteNulls(0x04);
                }
            }

            // Loop through and write each remap entry.
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                for (int m = 0; m < Data.Categories[i].Messages.Count; m++)
                {
                    // Only do this if this message actually has a remap entry.
                    if (Data.Categories[i].Messages[m].Remaps != null)
                    {
                        // Fill in this message's remap offset.
                        writer.FillInOffset($"Category{i}Message{m}Remaps", false, false);

                        // Add an offset table for this message's remap entries.
                        writer.AddOffsetTable($"Category{i}Message{m}RemapEntries", (uint)Data.Categories[i].Messages[m].Remaps.Count);

                        // Loop through each of this message's remaps.
                        for (int r = 0; r < Data.Categories[i].Messages[m].Remaps.Count; r++)
                        {
                            // Fill in this remap entry's offset.
                            writer.FillInOffset($"Category{i}Message{m}RemapEntries_{r}", false, false);

                            // Write this remap's character index.
                            writer.Write(Data.Categories[i].Messages[m].Remaps[r].CharacterIndex);

                            // Write this remap's unknown short value.
                            writer.Write(Data.Categories[i].Messages[m].Remaps[r].UnknownUShort_1);

                            //Write the type index and data for this remap.
                            if (Data.Categories[i].Messages[m].Remaps[r].RemapData.GetType() == typeof(byte[]))
                            {
                                writer.Write(0x04);
                                writer.Write((byte[])Data.Categories[i].Messages[m].Remaps[r].RemapData);
                            }
                            if (Data.Categories[i].Messages[m].Remaps[r].RemapData.GetType() == typeof(uint))
                            {
                                writer.Write(0x05);
                                writer.Write((uint)Data.Categories[i].Messages[m].Remaps[r].RemapData);
                            }
                        }
                    }
                }
            }

            // Loop through and write each UTF16 encoded message.
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                for (int m = 0; m < Data.Categories[i].Messages.Count; m++)
                {
                    // Fill in this message's offset.
                    writer.FillInOffset($"Category{i}Message{m}Message", false, false);

                    // Write the UTF16 encoded text for this message.
                    writer.WriteNullTerminatedStringUTF16(Data.Categories[i].Messages[m].Message);
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }
    }
}