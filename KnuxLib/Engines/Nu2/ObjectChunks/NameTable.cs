namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: A few PlayStation 2 and Xbox files have extra data after the string table. Figure out what it is and if it's important.
    internal class NameTable
    {
        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static List<string> Read(BinaryReaderEx reader, Scene.FormatVersion version)
        {
            // Set up our list of Names.
            List<string> Names = new();

            // Calculate where the string table ends.
            long tableEnd = reader.ReadUInt32() + reader.BaseStream.Position;

            // As long as we haven't reached the end of the table, add an entry to the list.
            while (reader.BaseStream.Position < tableEnd)
                Names.Add(reader.ReadNullTerminatedString());

            // Return the list of names read from the object.
            return Names;
        }
    }
}
