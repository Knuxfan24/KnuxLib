using System.IO.Compression;

namespace KnuxLib.Engines.CapcomMT
{
    public class Archive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Archive() { }
        public Archive(string filepath, bool extract = false, ushort version = 0x07, bool compressed = true)
        {
            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // Import the files in the directory.
                Data = Helpers.ImportArchive(filepath);

                // If the extract flag is set, then save this archive.
                if (extract)
                    Save($"{filepath}.arc", version, compressed);
            }

            // Check if the input path is a file.
            else
            {
                // Load this file.
                Load(filepath);

                // If the extract flag is set, then extract this archive.
                if (extract)
                    Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
            }
        }

        // Classes for this format.
        readonly Dictionary<uint, string> FileExtensions = new()
        {
            { 0x02358E1A, ".spk" },  // Unconfirmed, assumed from file header signature.
            { 0x0589CBA3, ".mcb" },  // Sourced from old PC version of Megaman X8.
            { 0x05C5AC17, ".pat" },  // Unconfirmed, assumed from file header signature.
            { 0x064A3AD8, ".xml" },  // Unconfirmed, assumed from formatting.
            { 0x06C3DBAA, ".bin" },  // Sourced from old PC version of Megaman X7.
            { 0x06DEC69C, ".fuv" },  // Unconfirmed, assumed from file header signature.
            { 0x07F768AF, ".gii" },  // Unconfirmed, assumed from file header signature.
            { 0x11320E86, ".omp" },  // Unconfirmed, assumed from file header signature.
            { 0x14B5C8E6, ".sndb" }, // Unconfirmed, assumed from file header signature.
            { 0x15D782FB, ".sbk" },  // Unconfirmed, assumed from file header signature.
            { 0x15E8853F, ".emi" },  // Sourced from old PC version of Megaman X7.
            { 0x167DBBFF, ".stgr" }, // Unconfirmed, assumed from file header signature.
            { 0x185365EA, ".emp" },  // Unconfirmed, assumed from file header signature.
            { 0x1BCC4966, ".srq" },  // Unconfirmed, assumed from file header signature.
            { 0x1C7858A2, ".lpk" },  // Sourced from old PC version of Megaman X8.
            { 0x1C9AA587, ".emb" },  // Unconfirmed, assumed from file header signature, contains a # before the signature itself.
            { 0x1E3EE6FB, ".wsx" },  // Sourced from old PC version of Megaman X8.
            { 0x1EC24743, ".ctex" }, // Unconfirmed, assumed from file header signature.
            { 0x1EF7D5BD, ".fst" },  // Unconfirmed, assumed from file header signature.
            { 0x219D76EB, ".gdsd" }, // Unconfirmed, assumed from file header signature.
            { 0x22948394, ".gui" },  // Unconfirmed, assumed from file header signature.
            { 0x232E228C, ".revr" }, // Unconfirmed, assumed from file header signature.
            { 0x23D4C76D, ".tam" },  // Sourced from old PC version of Megaman X7.
            { 0x241F5DEB, ".tex" },  // Unconfirmed, assumed from file header signature.
            { 0x242BB29A, ".gmd" },  // Unconfirmed, assumed from file header signature.
            { 0x255D51CD, ".sngw" }, // Unconfirmed, assumed from file header signature.
            { 0x25AF5760, ".bvs" },  // Unconfirmed, assumed from file header signature.
            { 0x2609378F, ".cof" },  // Unconfirmed, assumed from file header signature.
            { 0x2618DE3F, ".sreq" }, // Unconfirmed, assumed from file header signature.
            { 0x2749C8A8, ".mrl" },  // Unconfirmed, assumed from file header signature.
            { 0x28B3BA4D, ".bcm" },  // Unconfirmed, assumed from file header signature, contains a # before the signature itself.
            { 0x2D462600, ".gfd" },  // Unconfirmed, assumed from file header signature.
            { 0x311E683F, ".rl" },   // Sourced from old PC version of Megaman X8.
            { 0x31BF570E, ".set" },  // Sourced from old PC version of Megaman X8.
            { 0x328F438B, ".osd" },  // Sourced from old PC version of Megaman X7.
            { 0x39C52040, ".lcm" },  // Unconfirmed, assumed from file header signature.
            { 0x3AE9C67F, ".red" },  // Sourced from old PC version of Megaman X7.
            { 0x3C89A0B1, ".tlkd" }, // Unconfirmed, assumed from file header signature.
            { 0x3F481835, ".rlst" }, // Unconfirmed, assumed from file header signature.
            { 0x3FD1FBD2, ".mnl" },  // Unconfirmed, assumed from file header signature.
            { 0x40D2ABF8, ".weak" }, // Unconfirmed, assumed from file header signature.
            { 0x4323D83A, ".xfs" },  // Unconfirmed, assumed from file header signature.
            { 0x451EAA25, ".oprm" }, // Unconfirmed, assumed from file header signature.
            { 0x4A0DE37E, ".dat4" }, // Unconfirmed, assumed from file header signature.
            { 0x4B575FD0, ".datk" }, // Unconfirmed, assumed from file header signature.
            { 0x4C0B6650, ".comb" }, // Unconfirmed, assumed from file header signature.
            { 0x4C0DB839, ".sdl" },  // Unconfirmed, assumed from file header signature.
            { 0x4CCD72C3, ".gdat" }, // Unconfirmed, assumed from file header signature.
            { 0x4E397417, ".ean" },  // Unconfirmed, assumed from file header signature.
            { 0x4FA96078, ".cl2" },  // Sourced from old PC version of Megaman X7.
            { 0x5344F0C7, ".mpl" },  // Unconfirmed, assumed from file header signature.
            { 0x541C83B1, ".dmg" },  // Unconfirmed, assumed from file header signature.
            { 0x55B2ABCA, ".tlkl" }, // Unconfirmed, assumed from file header signature.
            { 0x589E9D20, ".stff" }, // Unconfirmed, assumed from file header signature.
            { 0x58A15856, ".mod" },  // Unconfirmed, assumed from file header signature.
            { 0x59633D17, ".xbgm" }, // Unconfirmed, assumed from file header signature.
            { 0x5B9EA119, ".dem" },  // Sourced from old PC version of Megaman X7.
            { 0x619CF7E7, ".col" },  // Unconfirmed, assumed from file header signature.
            { 0x67195A2E, ".madp" }, // Unconfirmed, assumed from file header signature.
            { 0x68C6C984, ".bac" },  // Unconfirmed, assumed from file header signature, contains a # before the signature itself.
            { 0x6AE91701, ".sld" },  // Sourced from old PC version of Megaman X7.
            { 0x6B685B4E, ".dfc" },  // Unconfirmed, assumed from file header signature.
            { 0x6BC1BAC7, ".ocl" },  // Unconfirmed, assumed from file header signature.
            { 0x6D5AE854, ".efl" },  // Unconfirmed, assumed from file header signature.
            { 0x710B8A11, ".scrl" }, // Unconfirmed, assumed from file header signature.
            { 0x724DF879, ".wav" },  // Unconfirmed, assumed from formatting.
            { 0x74D08E45, ".gdsi" }, // Unconfirmed, assumed from file header signature.
            { 0x734B8585, ".ecm" },  // Unconfirmed, assumed from file header signature, contains a # before the signature itself.
            { 0x76820D81, ".lmt" },  // Unconfirmed, assumed from file header signature.
            { 0x7772787E, ".evt" },  // Sourced from old PC version of Megaman X8.
            { 0x7808EA10, ".rtx" },  // Unconfirmed, assumed from file header signature.
            { 0x7C322EDC, ".medl" }, // Unconfirmed, assumed from file header signature.
            { 0x7E8B2835, ".wpl" }   // Unconfirmed, assumed from file header signature.
        };

        // Actual data presented to the end user.
        public FileNode[] Data = [];

        // Internal values used for extraction.
        ushort version;
        bool isNotCompressed = false;

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x03, "ARC");

            // Realign to 0x04 bytes.
            reader.FixPadding(0x04);

            // Check this file's version.
            // TODO: Handle versions other than V7 and V9.
            version = reader.ReadUInt16();
            if (version != 0x07 && version != 0x09)
                throw new NotImplementedException($"Capcom MT Framework Archive with version identifier of 0x{version.ToString("X").PadLeft(4, '0')} not supported.");

            // Read the amount of files in this archive.
            Data = new FileNode[reader.ReadUInt16()];

            // If this is a version 9 archive, then read the not compressed flag.
            if (version == 0x09)
                isNotCompressed = reader.ReadBoolean(0x04);

            // Loop through each file.
            for (int fileIndex = 0; fileIndex < Data.Length; fileIndex++)
            {
                // Set up a new file node.
                FileNode node = new();

                // Read this file's name.
                // TODO: Is this length correct?
                node.Name = reader.ReadNullPaddedString(0x40);

                // Read this file's type. Used to determine the extension.
                uint fileType = reader.ReadUInt32();

                // Read this file's compressed size.
                int compressedSize = reader.ReadInt32();

                // Read this file's decompressed size, subtracting 0x40000000 to deal with the random extra nibble.
                uint decompressedSize = reader.ReadUInt32() - 0x40000000;

                // Read the offset to this file's data.
                uint dataOffset = reader.ReadUInt32();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this file's data offset.
                reader.JumpTo(dataOffset);

                // Check if this file is compressed.
                if (!isNotCompressed)
                {
                    // Set up a ZLibStream with the compressed data.
                    ZLibStream zLibData = new(new MemoryStream(reader.ReadBytes(compressedSize)), CompressionMode.Decompress);

                    // Set up a MemoryStream to hold the decompressed data.
                    MemoryStream decompressedData = new();

                    // Copy the decompressed data to the memory stream.
                    zLibData.CopyTo(decompressedData);

                    // Save the decompressed data to this file node.
                    node.Data = decompressedData.ToArray();
                }

                // If this file isn't compressed, then just read its data.
                else
                    node.Data = reader.ReadBytes(compressedSize);

                // Jump back for the next file.
                reader.JumpTo(position);

                // Determine this file's extension using its type, setting it to the type if its not handled.
                if (FileExtensions.TryGetValue(fileType, out string? value))
                    node.Name += value;
                else
                {
                    Console.WriteLine($"Unknown file type 0x{fileType.ToString("X").PadLeft(8, '0')} for file {node.Name} in {filepath}.");
                    node.Name += $".{fileType.ToString("X").PadLeft(8, '0')}";
                }

                // Save this file.
                Data[fileIndex] = node;
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// TODO: Properly test V9 saving.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The version to save as.</param>
        /// <param name="compressed">Whether the files in this archive need to be compressed (if applicable).</param>
        public void Save(string filepath, ushort version = 0x07, bool compressed = true)
        {
            // Print that we're saving this archive format.
            Console.WriteLine($"Saving Capcom MT Framework Engine Archive (Version {version}).");

            // Set up an array of compressed data.
            byte[][] CompressedData = new byte[Data.Length][];

            // Loop through each file to compress it.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Print the name of the file we're compressing.
                Console.WriteLine($"Compressing {Data[dataIndex].Name}.");

                // Set up a MemoryStream to hold the compressed data.
                MemoryStream compressedStream = new();

                // Set up a ZLibStream to compress the data.
                ZLibStream zlibStream = new(compressedStream, CompressionMode.Compress);

                // Use the ZLibStream to compress the data.
                zlibStream.Write(Data[dataIndex].Data, 0, Data[dataIndex].Data.Length);

                // Close the ZLibStream.
                zlibStream.Close();

                // Save this file's compressed data.
                CompressedData[dataIndex] = compressedStream.ToArray();
            }

            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // Write this file's signature.
            writer.WriteNullPaddedString("ARC", 0x04);

            // Write this file's version.
            writer.Write(version);

            // Write the amount of files in this archive.
            writer.Write((ushort)Data.Length);

            // Write version 9's compression flag.
            if (version == 0x09)
                if (compressed)
                    writer.Write(0);
                else
                    writer.Write(1);

            // Loop through each file's information.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Write this file's name, padded to 0x40 bytes with the extension stripped.
                writer.WriteNullPaddedString(Path.ChangeExtension(Data[dataIndex].Name, null), 0x40);

                // Set up a value to track the file type.
                uint? fileType = null;

                // Determine the file type value based on the extension.
                foreach (var value in FileExtensions)
                    if (value.Value == Path.GetExtension(Data[dataIndex].Name))
                        fileType = value.Key;

                // Write the file's type, converting the extension to a hex integer if we haven't found it in the list.
                if (fileType == null)
                    writer.Write(Convert.ToInt32(Path.GetExtension(Data[dataIndex].Name)[1..], 16));
                else
                    writer.Write((uint)fileType);

                // Determine how to write the file sizes based on the version.
                switch (version)
                {
                    case 0x07:
                        // Write the compressed size of this file.
                        writer.Write(CompressedData[dataIndex].Length);
                        break;

                    case 0x09:
                        // Write the compressed size of this file.
                        if (compressed)
                            writer.Write(CompressedData[dataIndex].Length);

                        // Write the uncompressed size of this file.
                        else
                            writer.Write(Data[dataIndex].Data.Length);

                        break;
                }

                // Write the uncompressed size of this file, adding 0x40000000 to handle the extra nibble.
                writer.Write(Data[dataIndex].Data.Length + 0x40000000);

                // Add an offset to this file's data.
                writer.AddOffset($"File{dataIndex}Data");
            }

            // Realign to 0x8000.
            // TODO: Check that all archives do this.
            writer.FixPadding(0x8000);

            // Loop through each file.
            for (int dataIndex = 0; dataIndex < Data.Length; dataIndex++)
            {
                // Fill in the offset for this file's data.
                writer.FillInOffset($"File{dataIndex}Data");

                // Determine how to write the file data based on the version.
                switch (version)
                {
                    case 0x07:
                        // Write this file's compressed data.
                        writer.Write(CompressedData[dataIndex]);
                        break;

                    case 0x09:
                        // Write this file's compressed data.
                        if (compressed)
                            writer.Write(CompressedData[dataIndex]);

                        // Write this file's uncompressed data.
                        else
                            writer.Write(Data[dataIndex].Data);

                        break;
                }
            }

            // Close our BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Set up a string to store the version flag in. Default it to capcomv7.
            string versionFlag = "capcomv7";

            // Switch based on the read version and compression flag (where appropriate).
            switch (version)
            {
                case 9:
                    versionFlag = "capcomv9";
                    if (isNotCompressed)
                        versionFlag = "capcomv9_uncompressed";
                    break;
            }

            // Extract the archive.
            Helpers.ExtractArchive(Data, directory, versionFlag);
        }
    }
}
