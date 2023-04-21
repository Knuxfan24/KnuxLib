using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: What does ALIB stand for?
    // TODO: What does the data in this chunk actually do?
    // TODO: Read ANY of this data, haven't had much luck just looking at it in a hex editor.
    public class ALIBSet
    {
        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static void Read(BinaryReaderEx reader, FormatVersion version)
        {
        }
    }
}
