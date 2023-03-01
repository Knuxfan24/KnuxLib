global using HedgeLib.Headers;
global using Marathon.IO;
global using System.Numerics;

namespace KnuxLib
{
    public class FileNode
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The bytes that make up this node.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public override string ToString() => Name;
    }

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

        /// <summary>
        /// Reads and jumps to an offset to get a string from later in a file.
        /// </summary>
        /// <param name="reader">The HedgeLib# BINAReader to use.</param>
        /// <param name="is64bit">Whether or not the offset value we need to read is 64-bit (8 bytes) or not.</param>
        /// <param name="hasReaderOffset">Whether or not the jump has to respect the reader's offset.</param>
        /// <param name="extraOffset">A custom value to add to the read offset before jumping.</param>
        /// <returns>The string read from the location.</returns>
        public static string? ReadNullTerminatedStringTableEntry(HedgeLib.IO.BINAReader reader, bool is64bit = true, bool hasReaderOffset = true, uint extraOffset = 0)
        {
            // Set up a dummy position value.
            long position;

            // Check if the file is 32-bit or 64-bit.
            if (is64bit)
            {
                // Read our offset's value.
                long offset = reader.ReadInt64();

                // If this offset is empty, then don't jump and just return a null string instead.
                if (offset == 0)
                    return null;

                // Store our current position in the file.
                position = reader.BaseStream.Position;

                // Jump to the offset, using the reader's offset if needed.
                reader.JumpTo(offset + extraOffset, !hasReaderOffset);
            }
            else
            {
                // Read our offset's value.
                uint offset = reader.ReadUInt32();

                // If this offset is empty, then don't jump and just return a null string instead.
                if (offset == 0)
                    return null;

                // Store our current position in the file.
                position = reader.BaseStream.Position;

                // Jump to the offset, using the reader's offset if needed.
                reader.JumpTo(offset + extraOffset, !hasReaderOffset);
            }

            // Read the string at the offset.
            string entry = reader.ReadNullTerminatedString();

            // Jump back.
            reader.JumpTo(position);

            // Return the read string.
            return entry;
        }

        /// <summary>
        /// Reads a Vector3 from a file using HedgeLib#'s BINAReader, as it uses a custom Vector3 class.
        /// </summary>
        /// <param name="reader">The HedgeLib# BINAReader to use.</param>
        public static Vector3 ReadHedgeLibVector3(HedgeLib.IO.BINAReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Writes a Vector3 to a file using HedgeLib#'s BINAWriter, as it uses a custom Vector3 class.
        /// </summary>
        /// <param name="writer">The HedgeLib# BINAWriter to use.</param>
        /// <param name="value">The Vector3 value to write.</param>
        public static void WriteHedgeLibVector3(HedgeLib.IO.BINAWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        /// <summary>
        /// Combines multiple byte arrays into one.
        /// Taken from https://www.techiedelight.com/concatenate-byte-arrays-csharp/
        /// </summary>
        /// <param name="arrays">The array of arrays to combine.</param>
        public static byte[] CombineByteArrays(byte[][] arrays)
        {
            byte[] bytes = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, bytes, offset, array.Length);
                offset += array.Length;
            }

            return bytes;
        }

        /// <summary>
        /// Converts a uint to a string representing each bit in binary.
        /// Taken from https://stackoverflow.com/a/47918790
        /// </summary>
        /// <param name="num">The uint to convert.</param>
        public static string ToBinaryString(uint num) => Convert.ToString(num, 2).PadLeft(32, '0');
    }
}