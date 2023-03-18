global using HedgeLib.Headers;
global using Marathon.IO;
global using System.Numerics;

namespace KnuxLib
{
    // Generic classes for other formats.
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

    public class VertexColour
    {
        /// <summary>
        /// The Red value for this Vertex Colour.
        /// </summary>
        public byte Red { get; set; }

        /// <summary>
        /// The Green value for this Vertex Colour.
        /// </summary>
        public byte Green { get; set; }

        /// <summary>
        /// The Blue value for this Vertex Colour.
        /// </summary>
        public byte Blue { get; set; }

        /// <summary>
        /// The (optional) Alpha value for this Vertex Colour.
        /// </summary>
        public byte? Alpha { get; set; }

        public void Read(BinaryReaderEx reader, bool hasAlpha = true)
        {
            Red = reader.ReadByte();
            Blue = reader.ReadByte();
            Green = reader.ReadByte();

            if (hasAlpha)
                Alpha = reader.ReadByte();
        }
    }

    public class DecomposedMatrix
    {
        /// <summary>
        /// This matrix's position in 3D space.
        /// </summary>
        public Vector3 Translation { get; set; }

        /// <summary>
        /// This matrix's rotation, converted from a quaternion to euler angles.
        /// </summary>
        public Vector3 EulerRotation { get; set; }

        /// <summary>
        /// This matrix's scale factor.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Process a matrix into a decomposed version.
        /// </summary>
        /// <param name="matrix">The matrix to decompose.</param>
        public void Process(Matrix4x4 matrix)
        {
            // Set up the values for the Matrix's decomposition.
            Vector3 scale = new();
            Quaternion rotation = new();
            Vector3 translation = new();

            // Decompose the matrix.
            Matrix4x4.Decompose(matrix, out scale, out rotation, out translation);

            // Convert the quaternion to euler angles using HedgeLib#.
            HedgeLib.Vector3 hedgeLibV3 = new HedgeLib.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W).ToEulerAngles();

            // Set the values for this decomposed matrix.
            Translation = translation;
            EulerRotation = new(hedgeLibV3.X, hedgeLibV3.Y, hedgeLibV3.Z);
            Scale = scale;
        }
    }

    public class Helpers
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

        /// <summary>
        /// Checks a value in a binary file against an expected value.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="expectedValue">The value we expect this data to be.</param>
        public static void TestValueUInt(BinaryReaderEx reader, uint expectedValue)
        {
            // Read the value.
            uint actualValue = reader.ReadUInt32();

            // Check it against our expected value and throw an exception if it's wrong.
            if (actualValue != expectedValue)
                throw new Exception($"Value at '0x{(reader.BaseStream.Position - 0x04).ToString("X").PadLeft(8, '0')}' is '0x{actualValue.ToString("X").PadLeft(8, '0')}', expected '0x{expectedValue.ToString("X").PadLeft(8, '0')}'");
        }

        /// <summary>
        /// Checks a value in a binary file against an expected value.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="expectedValue">The value we expect this data to be.</param>
        public static void TestValueUShort(BinaryReaderEx reader, ushort expectedValue)
        {
            // Read the value.
            ushort actualValue = reader.ReadUInt16();

            // Check it against our expected value and throw an exception if it's wrong.
            if (actualValue != expectedValue)
                throw new Exception($"Value at '0x{(reader.BaseStream.Position - 0x04).ToString("X").PadLeft(8, '0')}' is '0x{actualValue.ToString("X").PadLeft(8, '0')}', expected '0x{expectedValue.ToString("X").PadLeft(8, '0')}'");
        }

        /// <summary>
        /// Finds a string in an Nu2 Engine scene file based on an offset.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="nameTableOffset">The offset within the Name Table.</param>
        /// <param name="version">The system version the file is being read as.</param>
        public static string FindNu2SceneName(BinaryReaderEx reader, uint nameTableOffset, Engines.Nu2.Scene.FormatVersion version)
        {
            // Set the chunk type for the node name table.
            string expectedChunkType = "NTBL";

            // If this is a GameCube file, then flip expectedChunkType.
            if (version == Engines.Nu2.Scene.FormatVersion.GameCube)
                expectedChunkType = new string(expectedChunkType.Reverse().ToArray());

            // Store our current position in the file.
            long pos = reader.BaseStream.Position;

            // Jump to the first chunk in the Nu2 Scene.
            switch (version)
            {
                case Engines.Nu2.Scene.FormatVersion.GameCube:
                case Engines.Nu2.Scene.FormatVersion.Xbox:
                    reader.JumpTo(0x08);
                    break;
                case Engines.Nu2.Scene.FormatVersion.PlayStation2:
                    reader.JumpTo(0x10);
                    break;
            }

            // If the chunk we've jumped to isn't the Name Table, then jump forward by the chunk's size minus the size of the chunk header.
            while (reader.ReadNullPaddedString(0x04) != expectedChunkType)
                reader.JumpAhead(reader.ReadUInt32() - 0x8);

            // Jump ahead past the chunk size and name table size to reach the actual table.
            reader.JumpAhead(0x08);

            // Jump forward in the table by the amount specified.
            reader.JumpAhead(nameTableOffset);

            // Set up a string value.
            // Read the name from the table at the current position.
            string name = reader.ReadNullTerminatedString();

            // Jump back to the saved location.
            reader.JumpTo(pos);

            // Return the name we read.
            return name;
        }

        /// <summary>
        /// Gets a file's full extension, rather than just the last extension.
        /// Taken from http://zuga.net/articles/cs-get-extension-of-a-file-that-has-multiple-periods/
        /// </summary>
        /// <param name="path">The file path to get the extension of.</param>
        /// <param name="returnWithout">Return the path without the extension instead of the extension itself.</param>
        public static string GetExtension(string path, bool returnWithout = false)
        {
            var ret = "";
            for (; ; )
            {
                var ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext))
                    break;
                path = path[..^ext.Length];
                ret = ext + ret;
            }

            if (returnWithout)
                return path.Replace(ret, "");

            return ret;
        }

        /// <summary>
        /// Converts a signed integer stored in the Binary Angle Measurement System to a floating point value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static float CalculateBAMsValue(int value) => value * 360f / 65536f;

        /// <summary>
        /// Converts a floating point value to a signed integer stored in the Binary Angle Measurement System.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static float CalculateBAMsValue(float value) => value * 65536f / 360f;
    }
}