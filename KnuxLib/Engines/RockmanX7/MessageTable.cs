namespace KnuxLib.Engines.RockmanX7
{
    // TODO: Everything, don't commit until there's SOMETHING functional.
    public class MessageTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MessageTable() { }
        public MessageTable(string filepath, bool export = false)
        {
            Load(filepath);

            //if (export)
            //    JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
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
            
            // Message binary lengths.
            for (int i = 0 ; i < messageCount; i++)
            {
                reader.JumpAhead(0x04);
            }

            for (int i = 0 ; i < messageCount; i++)
            {
                // First byte broke the text box entirely?
                // Second seemed to shove the name to the side?
                // Third and fourth breaks the text box but also throws a random symbol there? Are these a pair?
                reader.JumpAhead(0x0C); // Control stuff.
            }
        }
    }
}
