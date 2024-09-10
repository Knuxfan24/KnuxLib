using System.Xml.Linq;
using static HedgeLib.Misc.ForcesText;
using static KnuxLib.Engines.Hedgehog.MessageTable_2013;

namespace KnuxLib.Engines.Hedgehog
{
    // First Commit -> senorDane
    // TODO: Figure out the unknown values.
    // TODO: Figure out why the output isn't perfect (Some values seem to have padding or double null terminator? Maybe its padding for aligment? Currently
    // the output file is slightly smaller, and if we an extra null terminator to every string, it becomes slightly larger.).
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
            /// The resource name/text for this element. ASCII for resource (movie file with extension, or texture name), WideChar for text. Pointer
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
            public UInt32 HeaderSize = 0;

            public override string ToString() => Resource;
        }

        public class StaffRoll
        {
            public const String Signature = "CREDITSCOMMANDS"; //Saved as a pointer to the string table
            public const UInt32 HarcodedCount = 720; //Calculated by counting every pointer to a resource/message
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
                command.HeaderSize = reader.ReadUInt32();

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
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write the signature
            writer.AddString($"Signature", StaffRoll.Signature);

            // Write the (stale?) count, 715
            writer.Write(Data.CommandCount);

            // Write the first unknown, 4
            writer.Write(Data.UnkownUint32_1);

            // Write the second unknown, 1
            writer.Write(Data.UnkownUint32_2);

            // Add an offset to this file's command list.
            writer.AddOffset("CreditsCommands");

            // Fill in command list offset.
            writer.FillInOffset("CreditsCommands", false);
                    
            // Loop through each command in this file.
            for (int commandIndex = 0; commandIndex < StaffRoll.HarcodedCount; commandIndex++)
            {
                writer.Write((UInt32)Data.CreditCommands[commandIndex].Type);
                // Add Offset to the resource
                writer.AddOffset($"Command{commandIndex}Resource");

                // Write the count of cells in this sheet.
                writer.Write(Data.CreditCommands[commandIndex].DelayTime);
                writer.Write(Data.CreditCommands[commandIndex].VerticalPosition);
                writer.Write((UInt32)Data.CreditCommands[commandIndex].TextAlignment);
                writer.Write(Data.CreditCommands[commandIndex].HeaderSize);
            }

            // Loop through each command in this file.
            for (int commandIndex = 0; commandIndex < StaffRoll.HarcodedCount; commandIndex++)
            {
                writer.FillInOffset($"Command{commandIndex}Resource", false);
                //I do this instead of AddString because the resource/messages appear in order at the end, together.
                switch (Data.CreditCommands[commandIndex].Type)
                {
                    case CommandType.Image:
                    case CommandType.Video:
                        writer.WriteNullTerminatedString($"{Data.CreditCommands[commandIndex].Resource}");
                        break;
                    case CommandType.Text:
                        writer.WriteNullTerminatedStringUTF16($"{Data.CreditCommands[commandIndex].Resource}");
                        break;
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
