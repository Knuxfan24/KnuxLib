namespace KnuxLib.Engines.Wayforward
{
    // Based on https://github.com/artlavrov/paktools
    // TODO: Rethink the Colon stuff, as some files in Half-Genie Hero imply sub directory support.
        // i.e. global:dialog/chapter0/magnet_delivery/intro_mimic.lua
        // Not every file is just prefixed with the archive name, so that won't work as a solution.
    public class Package : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Package() { }
        public Package(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Actual data presented to the end user.
        public List<FileNode> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Set the offset in the reader to the position of this package's file data table.
            reader.Offset = reader.ReadUInt32();

            // Read this package's file count.
            uint fileCount = reader.ReadUInt32();

            // Loop through and read each file.
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                // Read the FILELINK_____END signature for this file's entry.
                reader.ReadSignature(0x10, "FILELINK_____END");

                // Fead the offset to this file's data.
                uint fileOffset = reader.ReadUInt32();

                // Read this file's size.
                int fileSize = reader.ReadInt32();

                // Read this file's name.
                // TODO: Properly handle file names.
                string fileName = reader.ReadNullTerminatedString().Replace(':', '\\');

                // Dirty hack to skip past the many padding ? bytes, as the amount of padding seems inconsistent?
                while (reader.ReadByte() == 0x3F) { }
                reader.JumpBehind(0x01);

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data.
                reader.JumpTo(fileOffset, true);

                // Read the MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END signature for this file's data.
                reader.ReadSignature(0x40, "MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END");

                // Read this file's data and add a generic file node for it.
                Data.Add
                (
                    new()
                    {
                        Name = fileName,
                        Data = reader.ReadBytes(fileSize)
                    }
                );

                // Jump back for the next file.
                reader.JumpTo(position);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If a file has no data in it, then just hardcode it.
            // TODO: This feels dumb.
            if (Data.Count == 0)
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
            writer.Write(Data.Count);

            // Loop through and write each file entry.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the FILELINK_____END signature.
                writer.Write("FILELINK_____END");

                // Add an offset for this file's data.
                writer.AddOffset($"File{dataIndex}Offset");

                // Write the size of this file.
                writer.Write(Data[dataIndex].Data.Length);

                // Write this file's name, replacing the divider of sub directories with a colon.
                // TODO: Figure out how this should actually be done.
                writer.WriteNullTerminatedString(Data[dataIndex].Name.Replace('\\', ':'));

                // Store our current position to figure out how many bytes we need.
                long paddingCount = writer.BaseStream.Position;

                // Realign the writer.
                writer.FixPadding(0x8);

                // Calculate padding.
                paddingCount = writer.BaseStream.Position - paddingCount;

                // Jump back by the amount of padding bytes.
                writer.BaseStream.Position -= paddingCount;

                // Replace the padding bytes with question marks.
                for (int paddingIndex = 0; paddingIndex < paddingCount; paddingIndex++)
                    writer.Write((byte)0x3F);
            }

            // Realign the writer. The actual files use question marks for this, but null bytes seem to work fine.
            writer.FixPadding(0x10);

            // Fill in the offset for the file data table.
            writer.FillOffset("FileTable");

            // Set the writer's offset so the file offsets get writen correctly.
            writer.Offset = (uint)writer.BaseStream.Position;

            // Loop through and write each file's data.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in the offset for this file's data.
                writer.FillOffset($"File{dataIndex}Offset", true);

                // Write the MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END signature.
                writer.Write("MANAGEDFILE_DATABLOCK_USED_IN_ENGINE_________________________END");

                // Write this file's data.
                writer.Write(Data[dataIndex].Data);

                // Store our current position to figure out how many bytes we need to pad by.
                long paddingCount = writer.BaseStream.Position;

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
            long finalPaddingCount = writer.BaseStream.Position;

            // Realign the writer.
            writer.FixPadding(0x40);

            // Calculate padding.
            finalPaddingCount = writer.BaseStream.Position - finalPaddingCount;

            // Jump back by the amount of padding bytes.
            writer.BaseStream.Position -= finalPaddingCount;

            // Replace the padding bytes with question marks.
            for (int paddingIndex = 0; paddingIndex < finalPaddingCount; paddingIndex++)
                writer.Write((byte)0x3F);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            Data.Add(new() { Data = Encoding.ASCII.GetBytes("wayforward"), Name = "knuxtools_archivetype.txt" });
            Helpers.ExtractArchive(Data, directory);
        }

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
