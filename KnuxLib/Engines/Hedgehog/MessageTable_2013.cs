using System.Reflection.Metadata.Ecma335;

namespace KnuxLib.Engines.Hedgehog
{
    // Partially based on: https://gist.github.com/Radfordhound/9c7695a0f6b1bcdfaeb4ad4c5462a6e8
    // TODO: Figure out the unknown values.
    // TODO: Convert saving over to new standard.
    // TODO: Importing a JSON creates an inaccurate file, but it seems to work?
    public class MessageTable_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable_2013() { }
        public MessageTable_2013(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.messagetable_2013.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Sheet[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.xtb2");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
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
            public Cell[] Cells { get; set; } = [];

            /// <summary>
            /// Displays this sheet's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this sheet with default data.
            /// </summary>
            public Sheet() { }

            /// <summary>
            /// Initialises this sheet with the provided data.
            /// </summary>
            public Sheet(string name, Cell[] cells)
            {
                Name = name;
                Cells = cells;
            }

            /// <summary>
            /// Initialises this sheet by reading its data from a BINAReader.
            /// </summary>
            public Sheet(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this sheet.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read the offset to this sheet's data.
                uint sheetDataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this sheet is read.
                long position = reader.BaseStream.Position;

                // Jump to this sheet's data.
                reader.JumpTo(sheetDataOffset, false);

                // Read this sheet's name.
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

                // Read this sheet's cell count.
                uint cellCount = reader.ReadUInt32();

                // Read the offset to this sheet's cell table.
                uint cellTableOffset = reader.ReadUInt32();

                // Skip two unknown values that are both always the same as this sheet's cell count.
                reader.JumpAhead(0x08);

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x04);

                // Initialise this sheet's cell array.
                Cells = new Cell[cellCount];

                // Jump to this sheet's cell table.
                reader.JumpTo(cellTableOffset, false);

                for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
                    Cells[cellIndex] = new(reader);

                // Jump back for the next sheet.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the data for this sheet.
            /// </summary>
            public void Write(BINAWriter writer, int index)
            {
                // Fill in this sheet's offset.
                writer.FillInOffset($"Sheet{index}DataOffset", false);

                // Add a string for this sheet's name.
                writer.AddString($"Sheet{index}Name", Name);

                // Write the count of cells in this sheet.
                writer.Write(Cells.Length);

                // Add an offset for this sheet's cell table.
                writer.AddOffset($"Sheet{index}CellTableOffset");

                // Write copies of the cell count.
                writer.Write(Cells.Length);
                writer.Write(Cells.Length);

                // Write an unknown value that is always 0.
                writer.Write(0);
            }
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
            public int UnknownInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's only ever 0x14 or 0x16.
            /// </summary>
            public int UnknownInt32_2 { get; set; }

            /// <summary>
            /// The remap entries this cell has, if any.
            /// </summary>
            public Remap[]? Remaps { get; set; }

            /// <summary>
            /// Displays this cell's name in the debugger if it has one.
            /// Otherwise, displayes the cell's message instead.
            /// </summary>
            public override string ToString()
            {
                if (Name == null) return Message;
                else return Name;
            }

            /// <summary>
            /// Initialises this cell with default data.
            /// </summary>
            public Cell() { }

            /// <summary>
            /// Initialises this cell with the provided data.
            /// </summary>
            public Cell(string? name, string message, int unknownInt32_1, int unknownInt32_2, Remap[]? remaps)
            {
                Name = name;
                Message = message;
                UnknownInt32_1 = unknownInt32_1;
                UnknownInt32_2 = unknownInt32_2;
                Remaps = remaps;
            }

            /// <summary>
            /// Initialises this cell by reading its data from a BINAReader.
            /// </summary>
            public Cell(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this cell.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read the offset to this cell's data.
                uint dataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this cell is read.
                long position = reader.BaseStream.Position;

                // Jump to this cell's data.
                reader.JumpTo(dataOffset, false);

                // Read this cell's name.
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

                // Read this cell's message, encoded in UTF-16.
                Message = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04, false, true);

                // Read the offset to this cell's remap data.
                uint remapTableOffset = reader.ReadUInt32();

                // Read this cell's remap count. This is either a 0 or a 1.
                uint remapCount = reader.ReadUInt32();

                // Skip an unknown value that is always the same as the cell's remap count.
                reader.JumpAhead(0x04);

                // Skip 0x16 bytes that are always 0.
                reader.JumpAhead(0x16);

                // Skip the first instance of this cell's message's last character index.
                reader.JumpAhead(0x02);

                // Skip an unknown value that is always 0x02.
                reader.JumpAhead(0x04);

                // Read this cell's first unknown integer value.
                UnknownInt32_1 = reader.ReadInt32();

                // Skip an unknown value that is always 0x00.
                reader.JumpAhead(0x02);

                // Skip the second instance of this cell's message's last character index.
                reader.JumpAhead(0x02);

                // Skip an unknown value that is always 0x01.
                reader.JumpAhead(0x04);

                // Read this cell's second unknown integer value.
                UnknownInt32_2 = reader.ReadInt32();

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
                if (remapTableOffset != 0)
                {
                    // Initialise this cell's remap array.
                    Remaps = new Remap[remapCount];

                    // Jump to this cell's remap table.
                    reader.JumpTo(remapTableOffset, false);

                    for (int remapIndex = 0; remapIndex < remapCount; remapIndex++)
                        Remaps[remapIndex] = new Remap(reader);
                }

                // Jump back for the next cell.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the data for this cell.
            /// </summary>
            public void Write(BINAWriter writer, int sheetIndex, int cellIndex)
            {
                // Fill in the offset for this cell.
                writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}Offset", false);

                // If this cell has a name, then add a string for it, if not, just write a 0.
                if (Name != null)
                    writer.AddString($"Sheet{sheetIndex}Cell{cellIndex}Name", Name);
                else
                    writer.Write(0);

                // Add an offset to this cell's message.
                writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}MessageOffset");

                // Write data for this cell's remap offset and length.
                if (Remaps != null)
                {
                    // Add an offset for this cell's remap table.
                    writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}RemapTableOffset");

                    // Write the count of remaps in this cell.
                    writer.Write(Remaps.Length);

                    // Write a copy of this cell's remap count.
                    writer.Write(Remaps.Length);
                }
                else
                {
                    // If this cell doesn't have any remaps, then fill in this space with 0x0C null values.
                    writer.WriteNulls(0x0C);
                }

                // Write 0x16 null bytes.
                writer.WriteNulls(0x16);

                // Write the first instance of this cell's message's last character index.
                writer.Write((ushort)(Message.Length - 1));

                // Write an unknown value that is always 0x02.
                writer.Write(0x02);

                // Write this cell's first unknown integer value.
                writer.Write(UnknownInt32_1);

                // Write an unknown value that is always 0.
                writer.Write((ushort)0);

                // Write the second instance of this cell's message's last character index.
                writer.Write((ushort)(Message.Length - 1));

                // Write an unknown value that is always 0x01.
                writer.Write(0x01);

                // Write this cell's second unknown integer value.
                writer.Write(UnknownInt32_2);

                // Write an unknown value that is always 0.
                writer.Write((ushort)0);

                // Write the third instance of this cell's message's last character index.
                writer.Write((ushort)(Message.Length - 1));

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write an unknown value that is always 0x01.
                writer.Write(0x01);

                // Write an unknown value that is always 0.
                writer.Write((ushort)0);

                // Write the fourth instance of this cell's message's last character index.
                writer.Write((ushort)(Message.Length - 1));

                // Write an unknown value that is always 0x03.
                writer.Write(0x03);

                // Write an unknown value that is always 0.
                writer.Write(0);
            }
        }

        public class Remap
        {
            /// <summary>
            /// The index of the character this remap entry replaces.
            /// </summary>
            public short CharacterIndex { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public short UnknownShort_1 { get; set; }

            /// <summary>
            /// The data for this remap.
            /// </summary>
            public object RemapData { get; set; } = new();

            /// <summary>
            /// Initialises this remap with default data.
            /// </summary>
            public Remap() { }

            /// <summary>
            /// Initialises this remap with the provided data.
            /// </summary>
            public Remap(short characterIndex, short unknownShort_1, object remapData)
            {
                CharacterIndex = characterIndex;
                UnknownShort_1 = unknownShort_1;
                RemapData = remapData;
            }

            /// <summary>
            /// Initialises this remap by reading its data from a BINAReader.
            /// </summary>
            public Remap(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this remap.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read the offset to this remap's data.
                uint remapDataOffset = reader.ReadUInt32();

                // Save our current position to jump back to once this remap entry is read.
                long remapPosition = reader.BaseStream.Position;

                // Jump to this remap's data.
                reader.JumpTo(remapDataOffset, false);

                // Read the index of the character to replace.
                CharacterIndex = reader.ReadInt16();

                // Read this remap's unknown short value.
                UnknownShort_1 = reader.ReadInt16();

                // Read this remap's data type.
                uint cellRemapDataType = reader.ReadUInt32();

                // Read the next four bytes as either a byte array or one value depending on the remap type.
                switch (cellRemapDataType)
                {
                    case 4: RemapData = reader.ReadBytes(0x04); break;
                    case 5: RemapData = reader.ReadUInt32(); break;
                }

                // Jump back for the next remap entry.
                reader.JumpTo(remapPosition);
            }

            /// <summary>
            /// Writes the data for this remap.
            /// </summary>
            public void Write(BINAWriter writer, int sheetIndex, int cellIndex, int remapIndex)
            {
                // Fill in the offset for this remap entry.
                writer.FillInOffset($"Sheet{sheetIndex}Cell{cellIndex}Remap{remapIndex}Offset", false);

                // Write this remap entry's character index.
                writer.Write(CharacterIndex);

                // Write this remap entry's unknown short value.
                writer.Write(UnknownShort_1);

                // Determine and write the type index and data for this remap.
                if (RemapData.GetType() == typeof(byte[]))
                {
                    writer.Write(0x04);
                    writer.Write((byte[])RemapData);
                }
                if (RemapData.GetType() == typeof(uint))
                {
                    writer.Write(0x05);
                    writer.Write((uint)RemapData);
                }
            }
        }

        // Actual data presented to the end user.
        public Sheet[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            BINAReader reader = new(File.OpenRead(filepath));

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
                Data[sheetIndex] = new(reader);

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(200);

            // Set up our BINAWriter and write the BINAV2 header.
            BINAWriter writer = new(File.Create(filepath), header);

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

            // Loop through and write each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
                Data[sheetIndex].Write(writer, sheetIndex);

            // Loop through each sheet in this file.
            for (int sheetIndex = 0; sheetIndex < Data.Length; sheetIndex++)
            {
                // Fill in the offset for this sheet's cell table.
                writer.FillInOffset($"Sheet{sheetIndex}CellTableOffset", false);

                // Loop through and add an offset for each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                    writer.AddOffset($"Sheet{sheetIndex}Cell{cellIndex}Offset");

                // Loop through and write each cell in this sheet.
                for (int cellIndex = 0; cellIndex < Data[sheetIndex].Cells.Length; cellIndex++)
                    Data[sheetIndex].Cells[cellIndex].Write(writer, sheetIndex, cellIndex);
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

                        // Loop through and write each remap entry in this cell.
                        for (int remapIndex = 0; remapIndex < Data[sheetIndex].Cells[cellIndex].Remaps.Length; remapIndex++)
                            Data[sheetIndex].Cells[cellIndex].Remaps[remapIndex].Write(writer, sheetIndex, cellIndex, remapIndex);
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

            // Close HedgeLib#'s BINAWriter.
            writer.Close(header);
        }
    }
}
