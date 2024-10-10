namespace KnuxLib.Helpers
{
    public class ArchiveHelpers
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
    }
}
