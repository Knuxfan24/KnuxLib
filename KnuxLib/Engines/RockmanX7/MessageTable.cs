using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7
{
    public class MessageTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable() { }
        public MessageTable(string filepath, bool export = false)
        {
            Load(filepath);

            //if (export)
            //   JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.rockmanx7.mathtable.json", Data);
        }

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            uint messageCount = reader.ReadUInt32();

            uint messageStartPosition = 0x04 + (messageCount * 0x04);

            for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
            {
                long position = reader.BaseStream.Position;
                
                reader.JumpTo(messageStartPosition + reader.ReadUInt32());

                if (reader.ReadUInt32() != 0x80178011) Debugger.Break();

                reader.JumpTo(position + 0x04);
            }
        }
    }
}
