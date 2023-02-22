global using Marathon.IO;
global using System.Numerics;

namespace KnuxLib
{
    internal class Helpers
    {
        /// <summary>
        /// Reads and jumps to an offset to get a string from later in a file.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="hasReaderOffset">Whether or not the jump has to respect the reader's offset.</param>
        /// <param name="extraOffset">A custom value to add to the read offset before jumping.</param>
        /// <returns>The string read from the location.</returns>
        public static string ReadNullTerminatedStringTableEntry(BinaryReaderEx reader, bool hasReaderOffset = false, uint extraOffset = 0)
        {
            // Read our offset's value.
            uint offset = reader.ReadUInt32();

            // Store our current position in the file.
            long position = reader.BaseStream.Position;

            // Jump to the offset, using the reader's offset if needed.
            reader.JumpTo(offset + extraOffset, hasReaderOffset);

            // Read the string at the offset.
            string entry = reader.ReadNullTerminatedString();

            // Jump back.
            reader.JumpTo(position);

            // Return the read string.
            return entry;
        }
    }
}