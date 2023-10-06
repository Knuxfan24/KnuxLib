using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: What does IABL stand for?
    // TODO: What does the data in this chunk actually do?
    // TODO: Read ANY of this data.
    public class IABLSet
    {
        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static void Read(BinaryReaderEx reader, FormatVersion version)
        {
        }
    }
}
