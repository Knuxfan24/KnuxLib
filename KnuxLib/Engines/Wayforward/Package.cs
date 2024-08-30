using System.Text.RegularExpressions;

namespace KnuxLib.Engines.Wayforward
{
    // Based on https://github.com/artlavrov/paktools
    public class Package : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Package() { }
        public Package(string filepath, bool bigEndian = false, bool extract = false)
        {
            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // Import the files in the directory.
                Data = Helpers.ImportArchive(filepath, false);

                // If the extract flag is set, then save this archive.
                if (extract)
                    Save($"{filepath}.pak", bigEndian);
            }

            // Check if the input path is a file.
            else
            {
                // Load this file.
                Load(filepath, bigEndian);

                // If the extract flag is set, then extract this archive.
                if (extract)
                    Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
            }
        }

        // Actual data presented to the end user.
        public FileNode[] Data = [];

        // Internal values used for extraction.
        bool isBigEndian = false;

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="bigEndian">Whether this file should be read in big endian.</param>
        public void Load(string filepath, bool bigEndian)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), bigEndian);

            // Set the internal big endian flag.
            isBigEndian = bigEndian;

            // Set the offset in the reader to the position of this package's file data table.
            reader.Offset = reader.ReadUInt32();

            // Read this package's file count.
            Data = new FileNode[reader.ReadUInt32()];

            // Loop through and read each file.
            for (int fileIndex = 0; fileIndex < Data.Length; fileIndex++)
            {
                // Read the FILELINK_____END signature for this file's entry.
                reader.ReadSignature(0x10, "FILELINK_____END");

                // Fead the offset to this file's data.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's size.
                int fileSize = reader.ReadInt32();

                // Read this file's name, replacing the first colon with a backslash.
                string fileName = reader.ReadNullTerminatedString().Replace(':', '\\');

                // Dirty hack to skip past the many padding ? bytes, as the amount of padding seems inconsistent?
                while (reader.ReadByte() == 0x3F) { }
                reader.JumpBehind(0x01);

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data.
                reader.JumpTo(fileOffset, false);

                // Read the MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END signature for this file's data.
                reader.ReadSignature(0x40, "MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END");

                // Read this file's data and add a generic file node for it.
                Data[fileIndex] = new()
                {
                    Name = fileName,
                    Data = reader.ReadBytes(fileSize)
                };

                // Jump back for the next file.
                reader.JumpTo(position);
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="bigEndian">Whether this file should be saved in big endian.</param>
        public void Save(string filepath, bool bigEndian)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath), bigEndian);

            // If a file has no data in it, then just hardcode it.
            // TODO: This feels dumb.
            if (Data.Length == 0)
            {
                writer.Write(0x10);
                writer.Write(0);
                writer.Write("????????????????????????????????????????????????????????");
                writer.Close();
                return;
            }

            // Add an offset for this package's file data table.
            writer.AddOffset("FileTable");

            // Write the count of files in this package.
            writer.Write(Data.Length);

            // Set up a value to track how much padding needs to be done.
            long paddingCount;

            // Loop through and write each file entry.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Write the FILELINK_____END signature.
                writer.Write("FILELINK_____END");

                // Add an offset for this file's data.
                writer.AddOffset($"File{dataIndex}Offset");

                // Write the size of this file.
                writer.Write(Data[dataIndex].Data.Length);

                // Write this file's name, replacing the first sub directory divider with a colon.
                writer.WriteNullTerminatedString(new Regex(Regex.Escape("\\")).Replace(Data[dataIndex].Name, ":", 1).Replace('/', '\\'));

                // Store our current position to figure out how many bytes we need.
                paddingCount = writer.BaseStream.Position;

                // Realign the writer.
                writer.FixPadding(0x08);

                // Calculate padding.
                paddingCount = writer.BaseStream.Position - paddingCount;

                // Jump back by the amount of padding bytes.
                writer.BaseStream.Position -= paddingCount;

                // Replace the padding bytes with question marks.
                for (int paddingIndex = 0; paddingIndex < paddingCount; paddingIndex++)
                    writer.Write((byte)0x3F);
            }

            // Store our current position to figure out how many bytes we need.
            paddingCount = writer.BaseStream.Position;

            // Realign the writer.
            writer.FixPadding(0x10);

            // Calculate padding.
            paddingCount = writer.BaseStream.Position - paddingCount;

            // Jump back by the amount of padding bytes.
            writer.BaseStream.Position -= paddingCount;

            // Replace the padding bytes with question marks.
            for (int paddingIndex = 0; paddingIndex < paddingCount; paddingIndex++)
                writer.Write((byte)0x3F);

            // Fill in the offset for the file data table.
            writer.FillInOffset("FileTable");

            // Set the writer's offset so the file offsets get writen correctly.
            writer.Offset = (uint)writer.BaseStream.Position;

            // Loop through and write each file's data.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Fill in the offset for this file's data.
                writer.FillInOffset($"File{dataIndex}Offset", false);

                // Write the MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END signature.
                writer.Write("MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END");

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);

                // Store our current position to figure out how many bytes we need to pad by.
                paddingCount = writer.BaseStream.Position;

                // Realign the writer.
                writer.FixPadding(0x10);

                // Calculate padding.
                paddingCount = writer.BaseStream.Position - paddingCount;

                // Jump back by the amount of padding bytes.
                writer.BaseStream.Position -= paddingCount;

                // Replace the padding bytes with question marks.
                for (int paddingIndex = 0; paddingIndex < paddingCount; paddingIndex++)
                    writer.Write((byte)0x3F);
            }

            // Store our current position to figure out how many bytes we need to pad by.
            paddingCount = writer.BaseStream.Position;

            // Realign the writer.
            writer.FixPadding(0x40);

            // Calculate padding.
            paddingCount = writer.BaseStream.Position - paddingCount;

            // Jump back by the amount of padding bytes.
            writer.BaseStream.Position -= paddingCount;

            // Replace the padding bytes with question marks.
            for (int paddingIndex = 0; paddingIndex < paddingCount; paddingIndex++)
                writer.Write((byte)0x3F);

            // Close our BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Set up a string to store the version flag in. Default it to wayforward.
            string versionFlag = "wayforward";

            // If the big endian flag is set, then change the version tag.
            if (isBigEndian)
                versionFlag = "wayforward_bigendian";

            // Extract the archive.
            Helpers.ExtractArchive(Data, directory, versionFlag);
        }
    }
}
