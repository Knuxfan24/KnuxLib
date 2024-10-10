namespace KnuxLib.Helpers
{
    public class StringHelpers
    {
        /// <summary>
        /// Reads an entry from a null terminated string table.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="offsetLength"></param>
        public static string? ReadNullTerminatedStringTableEntry(ExtendedBinaryReader reader, int offsetLength, bool absolute = false, bool isUTF16 = false, bool allowZeroOffset = false)
        {
            // Set up a value to store our offset.
            long offset;

            // Read the bytes that make up our offset.
            byte[] offsetBytes = reader.ReadBytes(offsetLength);

            // If this is a big endian offset, then reverse the bytes.
            if (reader.IsBigEndian)
                Array.Reverse(offsetBytes);

            // If our offset is at least eight bytes long, then parse it as a 64-bit integer.
            if (offsetBytes.Length >= 8)
                offset = BitConverter.ToInt64(offsetBytes, 0);

            // If not, parse it as a 32-bit integer instead.
            else
                offset = BitConverter.ToInt32(offsetBytes, 0);

            // If this offset is just 0, then abort and return a null string.
            if (offset == 0 && !allowZeroOffset)
                return null;

            // Save our current position so we can jump back after reading the string.
            long position = reader.BaseStream.Position;

            // Jump to the offset we read.
            reader.JumpTo(offset, absolute);

            // Set up a string value.
            string value;

            // Read the string at the offset based on whether it needs to be UTF16 or not.
            if (!isUTF16)
                value = reader.ReadNullTerminatedString();
            else
                value = reader.ReadNullTerminatedStringUTF16();

            // Jump back to where we were.
            reader.JumpTo(position);

            // Return the string we read.
            return value;
        }

        /// <summary>
        /// Reads a length prefixed string from a table, using the structure found in the Wayforward engine.
        /// </summary>
        /// <param name="reader">The ExtendedBinaryReader to use.</param>
        /// <param name="stringTableOffset">The offset to the file's string table.</param>
        /// <param name="stringIndex">The index of the string we want.</param>
        public static string ReadWayforwardLengthPrefixedString(ExtendedBinaryReader reader, long stringTableOffset, uint stringIndex)
        {
            // Save the reader's position so we can jump back for the rest of whatever is being read.
            long position = reader.BaseStream.Position;

            // Jump to the offset of the string table, plus the index multiplied by eight to get the offset we want.
            reader.JumpTo(stringTableOffset + (stringIndex * 0x08));

            // Jump to the value at the offset we calculated.
            reader.JumpTo(reader.ReadInt64());

            // Read the string, with the length being determined by the value right before it.
            string value = reader.ReadNullPaddedString(reader.ReadInt32());

            // Jump back for the rest of the data.
            reader.JumpTo(position);

            // Return our string.
            return value;
        }

        /// <summary>
        /// Gets a file's full extension, rather than just the last extension.
        /// Taken from http://zuga.net/articles/cs-get-extension-of-a-file-that-has-multiple-periods/
        /// </summary>
        /// <param name="path">The file path to get the extension of.</param>
        /// <param name="returnWithout">Return the path without the extension instead of the extension itself.</param>
        public static string GetExtension(string path, bool returnWithout = false)
        {
            string ret = "";
            for (; ; )
            {
                string ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext))
                    break;
                path = path[..^ext.Length];
                ret = ext + ret;
            }

            if (returnWithout)
                return path.Replace(ret, "");

            return ret;
        }
    }
}
