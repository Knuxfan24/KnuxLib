using System.Xml.Linq;
using static HedgeLib.Misc.ForcesText;
using static KnuxLib.Engines.Hedgehog.MessageTable_2013;

namespace KnuxLib.Engines.Hedgehog
{
    // First Commit -> senorDane
    // TODO: Figure out the unknown values.
    public class Staffroll_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Staffroll_2013() { }
        public Staffroll_2013(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.staffroll_2013.json", Data);
        }

        // Classes for this format.
        public enum TextAlignment { Left=0, Center=1, Right=2, ScreenCenter=3 };
        public enum CommandType { Text = 0, Image = 1, Video = 2 };


        public class Command
        {
            /// <summary>
            /// The type of element. UInt32
            /// </summary>
            public CommandType Type { get; set; } = CommandType.Text;

            /// <summary>
            /// The resource name/text for this element. ASCII for resource, WideChar for text. Pointer
            /// </summary>
            public string Resource { get; set; } = "";

            /// <summary>
            /// Time before this elements plays. Seems to only be used by the video element. float32
            /// </summary>
            public float DelayTime = 0.0f;

            /// <summary>
            /// Position of this element along the credits. float32
            /// </summary>
            public float VerticalPosition = 0.0f;

            /// <summary>
            /// Aligment of this element along the columns. UInt32
            /// </summary>
            public TextAlignment TextAlignment = TextAlignment.Center;

            /// <summary>
            /// Size of the text element. Usually 0-2. UInt32
            /// </summary>
            public UInt32 headerSize = 0;

            public override string ToString() => Resource;
        }

        public class StaffRoll
        {
            public const String Signature = "CREDITSCOMMANDS"; //Saved as a pointer to the string table
            public const UInt32 HarcodedCount = 720; //Calculated by counting every pointer to a resource
            public UInt32 CommandCount = 715;
            public UInt32 UnkownUint32_1 = 4;
            public UInt32 UnkownUint32_2 = 1;
            public Command[] CreditCommands { get; set; } = Array.Empty<Command>();

            public override string ToString()
            {
                return "Staffroll_2013";
            }
        }

        // Actual data presented to the end user.
        public StaffRoll? Data = null;

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

            // Read the "Signature"
            string signature = Helpers.ReadNullTerminatedStringTableEntry(reader, false);
            Console.WriteLine(signature);

            Data = new StaffRoll();
            Data.CommandCount = reader.ReadUInt32();
            Data.UnkownUint32_1 = reader.ReadUInt32();
            Data.UnkownUint32_2 = reader.ReadUInt32();

            // Read the offset to the command list
            uint commandListOffset = reader.ReadUInt32();

            Data.CreditCommands = new Command[StaffRoll.HarcodedCount];


            // Jump to the command list
            reader.JumpTo(commandListOffset, false);

            // Loop through each sheet in this message table.
            for (int commandIndex = 0; commandIndex < StaffRoll.HarcodedCount; commandIndex++)
            {
                // Set up a new sheet entry.
                Command command = new();

                // Read the offset to this sheet's data.
                command.Type = (CommandType)reader.ReadUInt32();

                switch (command.Type)
                {
                    case CommandType.Image:
                    case CommandType.Video:
                        //Read this command's resource
                        command.Resource = Helpers.ReadNullTerminatedStringTableEntry(reader, false);
                        break;
                    case CommandType.Text:
                        // Read this command' s text, encoded in UTF-16.
                        command.Resource = Helpers.ReadNullTerminatedStringTableEntry(reader, false, true, 0, true);
                        break;
                }

                command.DelayTime = reader.ReadSingle();
                command.VerticalPosition = reader.ReadSingle();
                command.TextAlignment = (TextAlignment)reader.ReadUInt32();
                command.headerSize = reader.ReadUInt32();

                Data.CreditCommands[commandIndex] = command;
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {/*
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
            writer.Close();*/
        }
    }
}
