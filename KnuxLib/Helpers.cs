﻿namespace KnuxLib
{
    public class Helpers
    {
        /// <summary>
        /// Extracts a list of generic filenodes.
        /// </summary>
        /// <param name="files">The array of files to extract.</param>
        /// <param name="directory">The path to extract to.</param>
        /// <param name="archiveIdentifier">The identifier used by KnuxTools.</param>
        /// <param name="extension">The extension to change all the files to.</param>
        public static void ExtractArchive(FileNode[] files, string directory, string archiveIdentifier, string? extension = null)
        {
            // Set up a dictionary so we can handle duplicated names.
            Dictionary<string, int> usedNames = [];

            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Write the archive type identifier
            File.WriteAllText($@"{directory}\knuxtools_archivetype.txt", archiveIdentifier);

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

                // Some files in NiGHTS: Journey of Dreams have a full drive path in them.
                // Trying to extract them fails, so replace the colon with an indicator as a workaround.
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
        /// <param name="noSubDirectories">Whether to only include files from the root of the path.</param>
        /// <param name="removeExtensions">Whether or not to remove the extensions.</param>
        /// <returns>The list of imported files as an array.</returns>
        public static FileNode[] ImportArchive(string directory, bool noSubDirectories = false, bool removeExtensions = false)
        {
            // Set up a new list of files.
            List<FileNode> files = [];

            string[] fileList = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
            if (noSubDirectories)
                fileList = Directory.GetFiles(directory, "*.*");

            // Loop through each file in the directory.
            foreach (string file in fileList)
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

                // Some files in Journey of Dreams have a full drive path in them.
                // Extracting them replaces the colon with an indicator as a workaround, so undo that.
                if (node.Name.Contains("[COLON]"))
                    node.Name = node.Name.Replace("[COLON]", ":");

                // Save this file.
                files.Add(node);
            }

            // Return the list of files as an array.
            return [.. files];
        }

        /// <summary>
        /// Reads an entry from a null terminated string table.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="offsetLength"></param>
        public static string? ReadNullTerminatedStringTableEntry(ExtendedBinaryReader reader, int offsetLength, bool absolute = false)
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
            if (offset == 0)
                return null;

            // Save our current position so we can jump back after reading the string.
            long position = reader.BaseStream.Position;

            // Jump to the offset we read.
            reader.JumpTo(offset, absolute);

            // Get the string at this position.
            string value = reader.ReadNullTerminatedString();

            // Jump back to where we were.
            reader.JumpTo(position);

            // Return the string we read.
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

        /// <summary>
        /// Converts a Quaternion to a Vector3 storing the equivalent Euler angles.
        /// Taken from HedgeLib#, with a credit link for http://quat.zachbennett.com/ (which is now dead).
        /// </summary>
        /// <param name="quaternion">The quaternion to convert.</param>
        /// <param name="radians">Whether or not to return the results as Radians instead of Euler.</param>
        public static Vector3 ConvertQuaternionToEuler(Quaternion quaternion, bool radians = false)
        {
            float qx2 = quaternion.X * quaternion.X;
            float qy2 = quaternion.Y * quaternion.Y;
            float qz2 = quaternion.Z * quaternion.Z;
            float negativeChecker = quaternion.X * quaternion.Y + quaternion.Z * quaternion.W;

            if (negativeChecker > 0.499)
                return GetVect(0, 360 / Math.PI * Math.Atan2(quaternion.X, quaternion.W), 90);

            if (negativeChecker < -0.499)
                return GetVect(0, -360 / Math.PI * Math.Atan2(quaternion.X, quaternion.W), -90);

            double h = Math.Atan2(2 * quaternion.Y * quaternion.W - 2 * quaternion.X * quaternion.Z, 1 - 2 * qy2 - 2 * qz2);
            double a = Math.Asin(2 * quaternion.X * quaternion.Y + 2 * quaternion.Z * quaternion.W);
            double b = Math.Atan2(2 * quaternion.X * quaternion.W - 2 * quaternion.Y * quaternion.Z, 1 - 2 * qx2 - 2 * qz2);

            return GetVect(Math.Round(b * 180 / Math.PI), Math.Round(h * 180 / Math.PI), Math.Round(a * 180 / Math.PI));

            // Sub-Methods
            Vector3 GetVect(double x, double y, double z)
            {
                float multi = (radians) ? 0.0174533f : 1;
                return new Vector3((float)x * multi,
                    (float)y * multi, (float)z * multi);
            }
        }
    }
}
