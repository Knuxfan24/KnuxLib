namespace KnuxLib.Engines.Hedgehog
{
    // Partially based on: https://gist.github.com/Radfordhound/9c7695a0f6b1bcdfaeb4ad4c5462a6e8
    // TODO: Figure out the unknown values.
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
        public class Sheet
        {
            /// <summary>
            /// The name of this category.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The cells this sheet has within it.
            /// </summary>
            public Cell[] Cells { get; set; } = Array.Empty<Cell>();

            public override string ToString() => Name;
        }

        public class Cell
        {
            /// <summary>
            /// The name of this cell, if it has one.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// The message this cell displays.
            /// </summary>
            public string Message { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's only ever 0x14 or 0x16.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// The remap entries this cell has, if any.
            /// </summary>
            public Remap[]? Remaps { get; set; }

            public override string ToString()
            {
                if (Name == null) return Message;
                else return Name;
            }
        }

        public class Remap
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
            public object RemapData { get; set; } = new();
        }

        // Actual data presented to the end user.
        public Sheet[] Data = Array.Empty<Sheet>();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(200);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip an unknown value that is always 0x02, likely a version identifier.
            reader.JumpAhead(0x02);

            // Read this message table's sheet count.
            ushort sheetCount = reader.ReadUInt16();

            // Read the offset to this message table's sheet table.
            uint sheetTableOffset = reader.ReadUInt32();

            // Skip two unknown values that are both always the same as sheet count (but as an integer rather than a short).
            reader.JumpAhead(0x08);

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Initialise the sheet array.
            Data = new Sheet[sheetCount];

            // Jump to this message table's sheet table.
            reader.JumpTo(sheetTableOffset, false);

            // Loop through each sheet in this message table.
            for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
            {
                // Set up a new sheet entry.
                Sheet sheet = new();

                // Read the offset to this sheet's data.
                uint sheetDataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this sheet is read.
                long position = reader.BaseStream.Position;

                // Jump to this sheet's data.
                reader.JumpTo(sheetDataOffset, false);

                // Read this sheet's name.
                sheet.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                // Read this sheet's cell count.
                uint sheetCellCount = reader.ReadUInt32();

                // Read the offset to this sheet's cell table.
                uint sheetCellTableOffset = reader.ReadUInt32();

                // Skip two unknown values that are both always the same as this sheet's cell count.
                reader.JumpAhead(0x08);

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x04);

                // Initialise this sheet's cell array.
                sheet.Cells = new Cell[sheetCellCount];

                // Jump to this sheet's cell table.
                reader.JumpTo(sheetCellTableOffset, false);

                // Loop through each cell in this sheet.
                for (int sheetCellIndex = 0; sheetCellIndex < sheetCellCount; sheetCellIndex++)
                {
                    // Set up a new cell entry.
                    Cell cell = new();

                    // Read the offset to this cell's data.
                    uint cellDataOffset = reader.ReadUInt32();

                    // Save our current position to jump back to once this cell is read.
                    long cellPosition = reader.BaseStream.Position;

                    // Jump to this cell's data.
                    reader.JumpTo(cellDataOffset, false);

                    // Read this cell's name.
                    cell.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                    // Read this cell's message, encoded in UTF-16.
                    cell.Message = Helpers.ReadNullTerminatedStringTableEntry(reader, false, true, 0, true);

                    // Read the offset to this cell's remap data.
                    uint cellRemapTableOffset = reader.ReadUInt32();

                    // Read this cell's remap count. This is either a 0 or a 1.
                    uint cellRemapCount = reader.ReadUInt32();

                    // Skip an unknown value that is always the same as the cell's remap count.
                    reader.JumpAhead(0x04);

                    // Skip 0x16 bytes that are always 0.
                    reader.JumpAhead(0x16);

                    // Skip the first instance of this cell's message's last character index.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value that is always 0x02.
                    reader.JumpAhead(0x04);

                    // Read this cell's first unknown integer value.
                    cell.UnknownUInt32_1 = reader.ReadUInt32();

                    // Skip an unknown value that is always 0x00.
                    reader.JumpAhead(0x02);

                    // Skip the second instance of this cell's message's last character index.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value that is always 0x01.
                    reader.JumpAhead(0x04);

                    // Read this cell's second unknown integer value.
                    cell.UnknownUInt32_2 = reader.ReadUInt32();

                    // Skip an unknown value that is always 0x00.
                    reader.JumpAhead(0x02);

                    // Skip the third instance of this cell's message's last character index.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value that is always 0x01.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x02);

                    // Skip the fourth instance of this cell's message's last character index.
                    reader.JumpAhead(0x02);

                    // Skip an unknown value that is always 0x03.
                    reader.JumpAhead(0x04);

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x04);

                    // If this cell has remap data, then read it as well.
                    if (cellRemapTableOffset != 0)
                    {
                        // Initialise this cell's remap array.
                        cell.Remaps = new Remap[cellRemapCount];

                        // Jump to this cell's remap table.
                        reader.JumpTo(cellRemapTableOffset, false);

                        // Loop through each remap in this cell.
                        for (int cellRemapIndex = 0; cellRemapIndex < cellRemapCount; cellRemapIndex++)
                        {
                            // Set up a new remap entry.
                            Remap remap = new();

                            // Read the offset to this remap's data.
                            uint remapDataOffset = reader.ReadUInt32();

                            // Save our current position to jump back to once this remap entry is read.
                            long remapPosition = reader.BaseStream.Position;

                            // Jump to this remap's data.
                            reader.JumpTo(remapDataOffset, false);

                            // Read the index of the character to replace.
                            remap.CharacterIndex = reader.ReadUInt16();

                            // Read this remap's unknown short value.
                            remap.UnknownUShort_1 = reader.ReadUInt16();

                            // Read this remap's data type.
                            uint cellRemapDataType = reader.ReadUInt32();

                            // Read the next four bytes as either a byte array or one value depending on the remap type.
                            switch (cellRemapDataType)
                            {
                                case 4: remap.RemapData = reader.ReadBytes(0x04); break;
                                case 5: remap.RemapData = reader.ReadUInt32(); break;
                            }

                            // Save this remap entry.
                            cell.Remaps[cellRemapIndex] = remap;

                            // Jump back for the next remap entry.
                            reader.JumpTo(remapPosition);
                        }
                    }

                    // Save this cell.
                    sheet.Cells[sheetCellIndex] = cell;

                    // Jump back for the next cell.
                    reader.JumpTo(cellPosition);
                }

                // Save this sheet.
                Data[sheetIndex] = sheet;

                // Jump back for the next sheet.
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
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write an unknown value that is always 0x02, likely a version identifier.
            writer.Write((ushort)0x02);

            // Write the count of sheets in this file.
            writer.Write((ushort)Data.Length);

            // Add an offset to this file's sheet table.
            writer.AddOffset("SheetTableOffset");

            // Write copies of the sheet count.
            writer.Write(Data.Length);
            writer.Write(Data.Length);

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Fill in the sheet table offset.
            writer.FillInOffset("SheetTableOffset", false);

            // Loop through and add an offset for each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
                writer.AddOffset($"Sheet{sheetIndex}DataOffset");
                    
            // Loop through each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
            {
                // Fill in this sheet's offset.
                writer.FillInOffset($"Sheet{sheetIndex}DataOffset", false);

                // Add a string for this sheet's name.
                writer.AddString($"Sheet{sheetIndex}Name", Data[sheetIndex].Name);

                // Write the count of cells in this sheet.
                writer.Write(Data[sheetIndex].Cells.Length);

                // Add an offset for this sheet's cell table.
                writer.AddOffset($"Sheet{sheetIndex}CellTableOffset");

                // Write copies of the cell count.
                writer.Write(Data[sheetIndex].Cells.Length);
                writer.Write(Data[sheetIndex].Cells.Length);

                // Write an unknown value that is always 0.
                writer.Write(0);
            }

            // Loop through each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
            {
                // Fill in the offset for this sheet's cell table.
                writer.FillInOffset($"Sheet{sheetIndex}CellTableOffset", false);

                // Loop through and add an offset for each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                    writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}Offset");

                // Loop through each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                {
                    // Fill in the offset for this cell.
                    writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}Offset", false);

                    // If this cell has a name, then add a string for it, if not, just write a 0.
                    if (Data[sheetIndex].Cells[cellIndex].Name != null)
                        writer.AddString($"Sheet{sheetIndex}Cell{cellIndex}Name", Data[sheetIndex].Cells[cellIndex].Name);
                    else
                        writer.Write(0);

                    // Add an offset to this cell's message.
                    writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}MessageOffset");

                    // Write data for this cell's remap offset and length.
                    if (Data[sheetIndex].Cells[cellIndex].Remaps != null)
                    {
                        // Add an offset for this cell's remap table.
                        writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}RemapTableOffset");

                        // Write the count of remaps in this cell.
                        writer.Write(Data[sheetIndex].Cells[cellIndex].Remaps.Length);

                        // Write a copy of this cell's remap count.
                        writer.Write(Data[sheetIndex].Cells[cellIndex].Remaps.Length);
                    }
                    else
                    {
                        // If this cell doesn't have any remaps, then fill in this space with 0x0C null values.
                        writer.WriteNulls(0x0C);
                    }

                    // Write 0x16 null bytes.
                    writer.WriteNulls(0x16);

                    // Write the first instance of this cell's message's last character index.
                    writer.Write((ushort)(Data[sheetIndex].Cells[cellIndex].Message.Length - 1));

                    // Write an unknown value that is always 0x02.
                    writer.Write(0x02);

                    // Write this cell's first unknown integer value.
                    writer.Write(Data[sheetIndex].Cells[cellIndex].UnknownUInt32_1);

                    // Write an unknown value that is always 0.
                    writer.Write((ushort)0);

                    // Write the second instance of this cell's message's last character index.
                    writer.Write((ushort)(Data[sheetIndex].Cells[cellIndex].Message.Length - 1));

                    // Write an unknown value that is always 0x01.
                    writer.Write(0x01);

                    // Write this cell's second unknown integer value.
                    writer.Write(Data[sheetIndex].Cells[cellIndex].UnknownUInt32_2);

                    // Write an unknown value that is always 0.
                    writer.Write((ushort)0);

                    // Write the third instance of this cell's message's last character index.
                    writer.Write((ushort)(Data[sheetIndex].Cells[cellIndex].Message.Length - 1));

                    // Write an unknown value that is always 0.
                    writer.Write(0);

                    // Write an unknown value that is always 0x01.
                    writer.Write(0x01);

                    // Write an unknown value that is always 0.
                    writer.Write((ushort)0);

                    // Write the fourth instance of this cell's message's last character index.
                    writer.Write((ushort)(Data[sheetIndex].Cells[cellIndex].Message.Length - 1));

                    // Write an unknown value that is always 0x03.
                    writer.Write(0x03);

                    // Write an unknown value that is always 0.
                    writer.Write(0);
                }
            }

            // Loop through each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
            {
                // Loop through each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                {
                    // Only write any data here if this cell has remap values.
                    if (Data[sheetIndex].Cells[cellIndex].Remaps != null)
                    {
                        // Fill in the offset for this cell's remap table.
                        writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}RemapTableOffset", false);

                        // Loop through and add an offset for each remap entry in this cell.
                        for (int remapIndex = 0; remapIndex < Data[sheetIndex].Cells[cellIndex].Remaps.Length; remapIndex++)
                            writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}Remap{remapIndex}Offset");

                        // Loop through each remap entry in this cell.
                        for (int remapIndex = 0; remapIndex < Data[sheetIndex].Cells[cellIndex].Remaps.Length; remapIndex++)
                        {
                            // Fill in the offset for this remap entry.
                            writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}Remap{remapIndex}Offset", false);

                            // Write this remap entry's character index.
                            writer.Write(Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].CharacterIndex);

                            // Write this remap entry's unknown short value.
                            writer.Write(Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].UnknownUShort_1);

                            // Determine and write the type index and data for this remap.
                            if (Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].RemapData.GetType() == typeof(byte[]))
                            {
                                writer.Write(0x04);
                                writer.Write((byte[])Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].RemapData);
                            }
                            if (Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].RemapData.GetType() == typeof(uint))
                            {
                                writer.Write(0x05);
                                writer.Write((uint)Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].RemapData);
                            }
                        }
                    }
                }
            }

            // Loop through each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
            {
                // Loop through each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                {
                    // Fill in this cell's message offset.
                    writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}MessageOffset", false);

                    // Write the UTF-16 encoded text for this message.
                    writer.WriteNullTerminatedStringUTF16(Data[sheetIndex].Cells[cellIndex].Message);
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
