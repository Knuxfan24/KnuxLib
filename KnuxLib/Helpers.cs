using libHSON;

namespace KnuxLib
{
    public class Helpers
    {
        /// <summary>
        /// Reads and jumps to an offset to get a string from later in a file.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="hasReaderOffset">Whether or not the jump has to respect the reader's offset.</param>
        /// <param name="extraOffset">A custom value to add to the read offset before jumping.</param>
        /// <returns>The string read from the location.</returns>
        public static string? ReadNullTerminatedStringTableEntry(BinaryReaderEx reader, bool hasReaderOffset = false, uint extraOffset = 0, bool is64bit = false)
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
                reader.JumpTo(offset + extraOffset, hasReaderOffset);
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
                reader.JumpTo(offset + extraOffset, hasReaderOffset);
            }

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
        /// <param name="isUTF16">Whether or not to read the text with UTF16 encoding.</param>
        /// <returns>The string read from the location.</returns>
        public static string? ReadNullTerminatedStringTableEntry(HedgeLib.IO.BINAReader reader, bool is64bit = true, bool hasReaderOffset = true, uint extraOffset = 0, bool isUTF16 = false)
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

            // Set up a string entry.
            string entry;

            // Read the string at the offset based on whether it needs to be UTF16 or not.
            if (!isUTF16)
                entry = reader.ReadNullTerminatedString();
            else
                entry = reader.ReadNullTerminatedStringUTF16();

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
        /// Reads a Quaternion from a file using HedgeLib#'s BINAReader, as it uses a custom Quaternion class.
        /// </summary>
        /// <param name="reader">The HedgeLib# BINAReader to use.</param>
        public static Quaternion ReadHedgeLibQuaternion(HedgeLib.IO.BINAReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Writes a Quaternion to a file using HedgeLib#'s BINAWriter, as it uses a custom Quaternion class.
        /// </summary>
        /// <param name="writer">The HedgeLib# BINAWriter to use.</param>
        /// <param name="value">The Quaternion value to write.</param>
        public static void WriteHedgeLibQuaternion(HedgeLib.IO.BINAWriter writer, Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
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
            long position = reader.BaseStream.Position;

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
            reader.JumpTo(position);

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

        /// <summary>
        /// Converts a signed integer stored in the Binary Angle Measurement System to a floating point value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static float CalculateBAMsValue(int value) => value * 360f / 65536f;

        /// <summary>
        /// Converts a floating point value to a signed integer stored in the Binary Angle Measurement System.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static int CalculateBAMsValue(float value) => (int)(value * 65536f / 360f);
    
        /// <summary>
        /// Creates a project for the HSON Specification in libHSON-csharp.
        /// </summary>
        /// <param name="name">The name for this project's metadata.</param>
        /// <param name="author">The author for this project's metadata</param>
        /// <param name="description">The description for this project's metadata.</param>
        public static Project CreateHSONProject(string name, string author = "Knuxfan24", string? description = null)
        {
            // Create and return the project.
            return new Project
            {
                Metadata = new ProjectMetadata
                {
                    Name = name,
                    Author = author,
                    Date = DateTime.UtcNow,
                    Description = description
                }
            };
        }

        /// <summary>
        /// Creates an empty object for the HSON Specification in libHSON-csharp.
        /// </summary>
        /// <param name="type">The type of object to add.</param>
        /// <param name="name">The name of the the object to add.</param>
        /// <param name="position">The position in 3D space of this object.</param>
        /// <param name="rotation">The rotation in 3D space of this object.</param>
        /// <param name="addRangeTags">Whether to add the RangeIn RangeOut tags used by the GEdit standard.</param>
        /// <param name="rangeIn">What to set the RangeIn tag to.</param>
        /// <param name="rangeOut">What to set the RangeOut tag to.</param>
        public static libHSON.Object CreateHSONObject(string type, string name, Vector3 position, Quaternion rotation, bool addRangeTags = true, float rangeIn = 100f, float rangeOut = 20f)
        {
            // Create the object.
            libHSON.Object hsonObj = new
            (
                type: type,
                name: name,
                position: position,
                rotation: rotation
            );

            // Add the range tags if we need to.
            if (addRangeTags)
            {
                hsonObj.LocalParameters.Add("tags/RangeSpawning/rangeIn", new Parameter(rangeIn));
                hsonObj.LocalParameters.Add("tags/RangeSpawning/rangeOut", new Parameter(rangeOut));
            }

            // Return the object.
            return hsonObj;
        }

        /// <summary>
        /// Converts a rotation in Euler Angles to a Quaternion.
        /// </summary>
        /// <param name="angles">The Euler Angles to convert.</param>
        public static Quaternion EulerToQuat(Vector3 angles)
        {
            // Create a HedgeLib# quaternion from the input angles.
            HedgeLib.Quaternion hedgelibQuaternion = new(new HedgeLib.Vector3(angles.X, angles.Y, angles.Z));

            // Return the conversion as a System.Numerics quaternion.
            return new(hedgelibQuaternion.X, hedgelibQuaternion.Y, hedgelibQuaternion.Z, hedgelibQuaternion.W);
        }

        /// <summary>
        /// Creates a Matrix4x4 from a position, scale and rotation.
        /// </summary>
        /// <param name="translation">The position values for this matrix.</param>
        /// <param name="scale">The scale values for this matrix.</param>
        /// <param name="rotation">The rotation values for this matrix.</param>
        public static Matrix4x4 CreateMatrix(Vector3 translation, Vector3 scale, Quaternion rotation)
        {
            // Create the initial matrix from the rotation.
            Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(rotation);

            // Set the matrix's translation.
            matrix.Translation = translation;

            // Apply the scale values to the matrix.
            matrix.M11 *= scale.X;
            matrix.M12 *= scale.X;
            matrix.M13 *= scale.X;
            matrix.M21 *= scale.Y;
            matrix.M22 *= scale.Y;
            matrix.M23 *= scale.Y;
            matrix.M31 *= scale.Z;
            matrix.M32 *= scale.Z;
            matrix.M33 *= scale.Z;

            // Return the generated matrix.
            return matrix;
        }

        /// <summary>
        /// Returns a uint as a string, formatted as a 32-bit hex value.
        /// </summary>
        /// <param name="value">The value to return.</param>
        public static string ReturnUIntAsHex(uint value) => $"0x{value.ToString("X").PadLeft(8, '0')}";

        /// <summary>
        /// Extracts a list of generic filenodes.
        /// </summary>
        /// <param name="files">The list of files to extract.</param>
        /// <param name="directory">The path to extract to.</param>
        /// <param name="extension">The extension to change all the files to.</param>
        public static void ExtractArchive(List<FileNode> files, string directory, string? extension = null)
        {
            // Set up a dictionary so we can handle duplicated names.
            Dictionary<string, int> usedNames = new();

            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Loop through each node to extract.
            foreach (FileNode node in files)
            {
                // Get this file's name.
                string fileName = node.Name;

                // If this file name isn't in the dictonary, then add it.
                if (!usedNames.ContainsKey(fileName))
                    usedNames.Add(fileName, 0);

                // If this file name is already in the dictonary, then increment its value and append the number to the name.
                else
                {
                    usedNames[fileName]++;
                    fileName += $"_{usedNames[fileName]}";
                }

                // If we need to, change the extension.
                if (extension != null)
                    fileName = Path.ChangeExtension(fileName, extension);

                // Print the name of the file we're extracting.
                Console.WriteLine($"Extracting {fileName}.");

                // Some files in Journey of Dreams have a full drive path in them, trying to extract them fails, so replace the colon with an indicator as a workaround.
                if (fileName.Contains(':'))
                    fileName = fileName.Replace(":", "[COLON]");

                // Create directory paths if needed.
                if (!Directory.Exists($@"{directory}\{Path.GetDirectoryName(fileName)}"))
                    Directory.CreateDirectory($@"{directory}\{Path.GetDirectoryName(fileName)}");

                // Extract the file.
                File.WriteAllBytes($@"{directory}\{fileName}", node.Data);
            }
        }

        /// <summary>
        /// Imports a directory of files to a list of generic filenodes.
        /// </summary>
        /// <param name="directory">The path to import.</param>
        /// <param name="removeExtensions">Whether or not to remove the extensions.</param>
        /// <returns>The list of imported files.</returns>
        public static List<FileNode> ImportArchive(string directory, bool removeExtensions = false)
        {
            // Set up a new list of files.
            List<FileNode> files = new();

            // Loop through each file in the directory.
            // TODO: Is it worth making a toggle for current directory only for formats that don't use subdirectories?
            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                // If this file is the KnuxTools archive type identifier shortcut, then skip over it.
                if (Path.GetFileName(file) == "knuxtools_archivetype.txt")
                    continue;

                // Read this file's name (stripping out the directory name in the search) and binary data.
                FileNode node = new()
                {
                    Name = file.Replace($@"{directory}\", ""),
                    Data = File.ReadAllBytes(file)
                };

                // Remove the extension if we need to.
                if (removeExtensions)
                    node.Name = Path.ChangeExtension(node.Name, null);

                // Some files in Journey of Dreams have a full drive path in them, extracting them replaces the colon with an indicator as a workaround, so undo that.
                if (node.Name.Contains("[COLON]"))
                    node.Name = node.Name.Replace("[COLON]", ":");

                // Save this file.
                files.Add(node);
            }

            // Return the list of files.
            return files;
        }

        /// <summary>
        /// Reads a length prefixed string from a table, using the structure found in the Wayforward engine.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="stringTableOffset">The offset to the file's string table.</param>
        /// <param name="stringIndex">The index of the string we want.</param>
        public static string ReadWayforwardLengthPrefixedString(BinaryReaderEx reader, long stringTableOffset, uint stringIndex)
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
    }
}