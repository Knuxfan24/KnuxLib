namespace KnuxLib.Engines.Hedgehog
{
    //03-06-2024, ricky-daniel13: Fixed missing pointer to Sheet list. Swapped names to those in Rad's specs.
    //03-07-2024, ricky-daniel13: Deleted redundant (And probably invalid after editing) data like counts and pointers.

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
            public uint Unknown5_UInt32 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint Unknown8_UInt32 { get; set; }

            /// <summary>
            /// The remap entries this cell has.
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

            // Pointer to the data in the Sheet* list
            uint sheetListPointer = reader.ReadUInt32();

            // Skip List Count (Same as sheetCount)
            reader.JumpAhead(0x04);

            // Skip List Capacity, (same as ListCount)
            reader.JumpAhead(0x04);

            // Skip Allocator interface pointer, 32 bit address, Null in file.
            reader.JumpAhead(0x04);

            // Jump to the Sheet* list's data pointer.
            reader.JumpTo(sheetListPointer, false);

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

                // Read this sheet's name.
                sheet.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                // Read this sheet's cell count
                uint cellListCount = reader.ReadUInt32();

                // Pointer to the data in the Cell* list
                uint cellListDataLoc = reader.ReadUInt32();

                // Skip the count of the Cell* list (Same as cellListCount)
                reader.JumpAhead(0x04);

                // Skip the Cell* list capacity (Same as count)
                reader.JumpAhead(0x04);

                // Skip Allocator interface pointer, 32 bit address, Null in file.
                reader.JumpAhead(0x04);

                // Jump to the Cell* list's data pointer.
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

                    // Read the pointer to this Remap* list's data.
                    uint remapPointer = reader.ReadUInt32();

                    // Read this Remap* list count, this is always 0 or 1, but the game might support multiple (assuming this is a count).
                    uint remapCount = reader.ReadUInt32();

                    // Skip this Remap* list capacity 
                    reader.JumpAhead(0x04);

                    // Skip this Remap* list allocator pointer
                    reader.JumpAhead(0x04);

                    // Skip 0x12 null bytes. (ulong + ulong + short, Unknowns 1, 2 and 3)
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

                    // Skip last character index 3
                    reader.JumpAhead(0x02);

                    // Skip Unknown 10, 0x00
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
                            // Read the pointer to this remap's data.
                            uint remapDataOffset = reader.ReadUInt32();

                            // Save our current position to jump back for the next remap.
                            long remapPosition = reader.BaseStream.Position;

                            // Jump to the remap pointer.
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
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter for the gismod file and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write versionm
            writer.Write((ushort)0x02);

            // Write the amount of sheets in this file.
            writer.Write((ushort)Data.Sheets.Count);


            #region Sheet list data
            // Add the sheet list pointer
            writer.AddOffset("CategoryListPointer");

            // Write this sheet list count
            writer.Write((uint)Data.Sheets.Count);

            // Write this sheet list capacity
            writer.Write((uint)Data.Sheets.Count);

            // Write this sheet list allocator
            writer.WriteNulls(0x04);
            #endregion

            // Write the sheet list pointer
            writer.FillInOffset("CategoryListPointer", false, false);

            // Add the pointer list for the sheets.
            writer.AddOffsetTable($"Categories", (uint)Data.Sheets.Count);

            // Loop through and write each sheet entry.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                // Fill in this sheet's pointer.
                writer.FillInOffset($"Categories_{categoryIndex}", false, false);

                // Add this sheet's name.
                writer.AddString($"Category{categoryIndex}Name", Data.Sheets[categoryIndex].Name);

                // Write this sheet's cell count.
                writer.Write(Data.Sheets[categoryIndex].Cells.Count);

                #region Cell list data
                    // Add an pointer to the cell list data
                    writer.AddOffset($"Category{categoryIndex}Messages");

                    // Write the cell list count
                    writer.Write(Data.Sheets[categoryIndex].Cells.Count);

                    // Write the cell list capacity
                    writer.Write(Data.Sheets[categoryIndex].Cells.Count);

                    // Write the cell list pointer to the allocator
                    writer.Write(0x00);
                #endregion

            }

            // Loop through and write each sheet's cells.
            for (int categoryIndex = 0; categoryIndex < Data.Sheets.Count; categoryIndex++)
            {
                // Fill in the pointer list for this sheet's cell list.
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

                    // If this message has remaps, write the remap list data
                    #region Remap list data
                    if (Data.Sheets[categoryIndex].Cells[messageIndex].Remaps != null)
                    {
                        //Pointer to remap list data
                        writer.AddOffset($"Category{categoryIndex}Message{messageIndex}Remaps");
                        //Remap list count
                        writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Remaps.Count);
                        //Remap list capacity
                        writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Remaps.Count);
                        //Remap list pointer to allocator
                        writer.WriteNulls(0x04);
                    }

                    // If not, then just write sixteen nulls. (The size of the data of the list class)
                    else
                    {
                        writer.WriteNulls(0x10);
                    }
                    #endregion

                    // Write 0x16 null bytes. (ulong + ulong + short, Unknowns 1, 2 and 3)
                    writer.WriteNulls(0x16);

                    // Write the last char index 1
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write Unknown 4 value of 0x02.
                    writer.Write(0x02);

                    // Write Unknown 5.
                    writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Unknown5_UInt32);

                    // Write Unknown 6 (ushort)
                    writer.WriteNulls(0x02);

                    // Write the last character index 2
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write Unknown 7
                    writer.Write(0x01);

                    // Write Unknown 8
                    writer.Write(Data.Sheets[categoryIndex].Cells[messageIndex].Unknown8_UInt32);

                    // Write Unkown 9 (ushort)
                    writer.WriteNulls(0x02);

                    // Write the last character index 3
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write Unknown 10 (uint)
                    writer.WriteNulls(0x04);

                    // Write Unknown 11 value of 1
                    writer.Write(0x01);

                    // Write Unknown 12 (ushort)
                    writer.WriteNulls(0x02);

                    // Write Last character Index 4
                    writer.Write((ushort)(Data.Sheets[categoryIndex].Cells[messageIndex].Message.Length - 1));

                    // Write Unknown 13 value of 0x03.
                    writer.Write(0x03);

                    // Write Unknown 14 (uint)
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
