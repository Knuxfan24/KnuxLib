namespace KnuxLib.Engines.Hedgehog
{
    //03-06-2024, ricky-daniel13: Fixed missing pointer to Sheet list. Swapped names to those in Rad's specs.

    // TODO: Figure out all of the unknown values.

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
            /// Pointer to the data in the list
            /// </summary>
            public uint ListPointer { get; set; }

            /// <summary>
            /// Count of data in the list
            /// </summary>
            public uint ListCount { get; set; }

            /// <summary>
            /// The categories this file has in it.
            /// </summary>
            public List<Sheet> Sheets { get; set; } = new();
        }

        public class Sheet
        {
            /// <summary>
            /// The name of this category.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// Count of cells in the cell list
            /// </summary>
            public uint CellsCount { get; set; }

            /// <summary>
            /// The cells this sheet has within it.
            /// </summary>
            public List<Cell> Cells { get; set; } = new();

            public override string ToString() => Name;
        }

        public class Cell
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
            public uint remapCapacity { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint Unknown5_UInt32 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint Unknown8_UInt32 { get; set; }

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
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(200);



        /// Rad's spec available on: https://gist.github.com/Radfordhound/9c7695a0f6b1bcdfaeb4ad4c5462a6e8
        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Version
            reader.JumpAhead(0x02);

            // Read the amount of sheets in this file.
            ushort sheetCount = reader.ReadUInt16();

            // Pointer to the data in the sheet list
            Data.ListPointer = reader.ReadUInt32();

            // List Count
            Data.ListCount = reader.ReadUInt32();

            // List Capacity, 32 uint, same as ListCount
            reader.JumpAhead(0x04);

            // Allocator interface pointer, 3D bit address address, Null in file.
            reader.JumpAhead(0x04);

            // Jump to the category's data offset.
            reader.JumpTo(Data.ListPointer, false);

            // Loop through and read each category.
            for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
            {
                // Set up a new category.
                Sheet sheet = new();

                // Read the offset to this sheet data.
                uint sheetDataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this category is read.
                long position = reader.BaseStream.Position;

                // Jump to the category's data offset.
                reader.JumpTo(sheetDataOffset, false);

                // Read this category's name.
                sheet.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                // Read this category's cell count
                sheet.CellsCount = reader.ReadUInt32();

                // Read the data offset of the Cells* list
                uint cellListDataLoc = reader.ReadUInt32();

                // Read the count of the Cells* list
                uint cellListCount = reader.ReadUInt32();

                // Skip the Cells* list capacity (Same as count)
                reader.JumpAhead(0x04);

                // Skip the Cells* list pointer to the allocator (Null in files)
                reader.JumpAhead(0x04);

                // Jump to this category's message data.
                reader.JumpTo(cellListDataLoc, false);

                // Loop through and read each message in this category.
                for (int cellIndex = 0; cellIndex < cellListCount; cellIndex++)
                {
                    // Set up a new message.
                    Cell cell = new();

                    // Read the offset to this message's data.
                    uint dataOffset = reader.ReadUInt32();

                    // Save our current position so we can jump back for the next message.
                    long currentPosition = reader.BaseStream.Position;

                    // Jump to this message's data.
                    reader.JumpTo(dataOffset, false);

                    // Read this message's name, if it has one.
                    cell.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                    // Read this message's UTF16 encoded text.
                    cell.Message = Helpers.ReadNullTerminatedStringTableEntry(reader, false, true, 0, true);

                    // Read the pointer to this remap list's data.
                    uint remapPointer = reader.ReadUInt32();

                    // Read this remap list count, this is always 0 or 1, but the game might support multiple (assuming this is a count).
                    uint remapCount = reader.ReadUInt32();

                    // Read this remap list capacity
                    cell.remapCapacity = reader.ReadUInt32();

                    // Skip this remap list allocator pointer
                    reader.JumpAhead(0x04);

                    // Skip 0x12 null bytes. (ulong + ulong + short)
                    reader.JumpAhead(0x12);

                    // Skip Last Character Index
                    reader.JumpAhead(0x02);

                    // Skip Unknown 4, which is always 0x02.
                    reader.JumpAhead(0x04);

                    // Read Unknown 5
                    cell.Unknown5_UInt32 = reader.ReadUInt32();

                    // Skip Unknown 6
                    reader.JumpAhead(0x02);

                    // Skip last character index 2
                    reader.JumpAhead(0x02);

                    // Skip Unkown 7 0x01.
                    reader.JumpAhead(0x04);

                    // Read Unknown 8
                    cell.Unknown8_UInt32 = reader.ReadUInt32();

                    // Skip Unknown 9
                    reader.JumpAhead(0x02);

                    // Sip last character index 3
                    reader.JumpAhead(0x02);

                    //  Skip Unknown 10, 0x00
                    reader.JumpAhead(0x04);

                    //  Skip Unknown 11 of 0x01.
                    reader.JumpAhead(0x04);

                    //  Skip Unknown 12 of 0.
                    reader.JumpAhead(0x02);

                    // Skip last character index 4
                    reader.JumpAhead(0x02);

                    //  Skip Unknown 13 of 0x03.
                    reader.JumpAhead(0x04);

                    // Skip Unknown 14 of 0.
                    reader.JumpAhead(0x04);

                    // If this cell has any remaps, then read them as well.
                    if (remapPointer != 0)
                    {
                        // Define the remap list.
                        cell.Remaps = new();

                        // Jump to the previously read remap offset.
                        reader.JumpTo(remapPointer, false);

                        // Loop through each remap in this message.
                        for (int remapIndex = 0; remapIndex < remapCount; remapIndex++)
                        {
                            // Read the offset to this remap's data.
                            uint remapDataOffset = reader.ReadUInt32();

                            // Save our current position to jump back for the next remap.
                            long remapPosition = reader.BaseStream.Position;

                            // Jump to the remap offset.
                            reader.JumpTo(remapDataOffset, false);

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
                            cell.Remaps.Add(remap);

                            // Jump back for the next remap entry.
                            reader.JumpTo(remapPosition);
                        }
                    }

                    // Save this message.
                    sheet.Cells.Add(cell);

                    // Jump back for the next message.
                    reader.JumpTo(currentPosition);
                }

                // Save this category.
                Data.Sheets.Add(sheet);

                // Jump back for the next category.
                reader.JumpTo(position);
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

            // Write versionm
            writer.Write((ushort)0x02);

            // Write the amount of categories in this file.
            writer.Write((ushort)Data.Sheets.Count);

            // Add the sheet list pointer
            writer.AddOffset("CategoryListPointer");

            // Write this sheet list count
            writer.Write(Data.ListCount);

            // Write this sheet list capacity
            writer.Write(Data.ListCount);

            // Write this sheet list allocator
            writer.WriteNulls(0x04);

            //write the sheet list poioter
            writer.FillInOffset("CategoryListPointer", false, false);

            // Add an offset table for the categories.
            writer.AddOffsetTable($"Categories", (uint)Data.Sheets.Count);

            // Loop through and write each category entry.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                // Fill in this category's offset.
                writer.FillInOffset($"Categories_{categoryIndex}", false, false);

                // Add this category's name.
                writer.AddString($"Category{categoryIndex}Name", Data.Sheets[categoryIndex].Name);

                // Write this category's unknown integer value.
                writer.Write(Data.Sheets[categoryIndex].CellsCount);

                // Add an offset to this category's message table.
                writer.AddOffset($"Category{categoryIndex}Messages");

                // Write this category's message count.
                writer.Write(Data.Sheets[categoryIndex].Cells.Count);

                // Write another copy of this category's message count.
                writer.Write(Data.Sheets[categoryIndex].Cells.Count);

                // Write an unknown value of 0.
                writer.Write(0x00);
            }

            // Loop through and write each category's messages.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                // Fill in the offset for this category's message table.
                writer.FillInOffset($"Category{categoryIndex}Messages", false, false);

                // Add an offset table for this category's actual messages.
                writer.AddOffsetTable($"Category{categoryIndex}Messages", (uint)Data.Sheets[categoryIndex].Cells.Count);

                // Loop through and write each message in this category.
                for (int messageIndex = 0; messageIndex < Data.Sheets[categoryIndex].Cells.Count; messageIndex++)
                {
                    // Fill in this message's offset.
                    writer.FillInOffset($"Category{categoryIndex}Messages_{messageIndex}", false, false);

                    // Add this message's name.
                    writer.AddString($"Category{categoryIndex}Message{messageIndex}Name", Data.Sheets[categoryIndex].Cells[messageIndex].Name);

                    // Add an offset for this message's UTF16 encoded text.
                    writer.AddOffset($"Category{categoryIndex}Message{messageIndex}Message");

                    // If this message has remaps, then add an offset and write the count of them.
                    if (Data.Sheets[categoryIndex].Cells[messageIndex].Remaps != null)
                    {
                        writer.AddOffset($"Category{categoryIndex}Message{messageIndex}Remaps");
                        writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Remaps.Count);
                    }

                    // If not, then just write eight nulls.
                    else
                    {
                        writer.WriteNulls(0x08);
                    }

                    // Write this message's first unknown integer value.
                    writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].remapCapacity);

                    // Write 0x16 null bytes.
                    writer.WriteNulls(0x16);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write an unknown value of 0x02.
                    writer.Write(0x02);

                    // Write this message's second unknown integer value.
                    writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Unknown5_UInt32);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write an unknown value of 0x01.
                    writer.Write(0x01);

                    // Write this message's third unknown integer value.
                    writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Unknown8_UInt32);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write four null bytes.
                    writer.WriteNulls(0x04);

                    // Write an unknown value of 0x01.
                    writer.Write(0x01);

                    // Write two null bytes.
                    writer.WriteNulls(0x02);

                    // Write a value that is the length of the message minus 1.
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write an unknown value of 0x03.
                    writer.Write(0x03);

                    // Write four null bytes.
                    writer.WriteNulls(0x04);
                }
            }

            // Loop through and write each remap entry.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                for (int messageIndex = 0; messageIndex < Data.Sheets[categoryIndex].Cells.Count; messageIndex++)
                {
                    // Only do this if this message actually has a remap entry.
                    if (Data.Sheets[categoryIndex].Cells[messageIndex].Remaps != null)
                    {
                        // Fill in this message's remap offset.
                        writer.FillInOffset($"Category{categoryIndex}Message{messageIndex}Remaps", false, false);

                        // Add an offset table for this message's remap entries.
                        writer.AddOffsetTable($"Category{categoryIndex}Message{messageIndex}RemapEntries", (uint)Data.Sheets[categoryIndex].Cells[messageIndex].Remaps.Count);

                        // Loop through each of this message's remaps.
                        for (int remapIndex = 0; remapIndex < Data.Sheets[categoryIndex].Cells[messageIndex].Remaps.Count; remapIndex++)
                        {
                            // Fill in this remap entry's offset.
                            writer.FillInOffset($"Category{categoryIndex}Message{messageIndex}RemapEntries_{remapIndex}", false, false);

                            // Write this remap's character index.
                            writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].CharacterIndex);

                            // Write this remap's unknown short value.
                            writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].UnknownUShort_1);

                            //Write the type index and data for this remap.
                            if (Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].RemapData.GetType() == typeof(byte[]))
                            {
                                writer.Write(0x04);
                                writer.Write((byte[])Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].RemapData);
                            }
                            if (Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].RemapData.GetType() == typeof(uint))
                            {
                                writer.Write(0x05);
                                writer.Write((uint)Data.Sheets[categoryIndex].Cells[messageIndex].Remaps[remapIndex].RemapData);
                            }
                        }
                    }
                }
            }

            // Loop through and write each UTF16 encoded message.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                for (int messageIndex = 0; messageIndex < Data.Sheets[categoryIndex].Cells.Count; messageIndex++)
                {
                    // Fill in this message's offset.
                    writer.FillInOffset($"Category{categoryIndex}Message{messageIndex}Message", false, false);

                    // Write the UTF16 encoded text for this message.
                    writer.WriteNullTerminatedStringUTF16(Data.Sheets[categoryIndex].Cells[messageIndex].Message);
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }
    }
}
