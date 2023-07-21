using System.IO.Compression;

namespace KnuxLib.Engines.Black
{
    // Based on https://github.com/meh2481/MSFHDEx
    // TODO: Finish format saving.
    // TODO: Only tested on Shantae and the Pirate's Curse. The QuickBMS script linked above has multiple versions, so this will definitely fail on other Engine Black games.
    // TODO: Does this archive have the concept of folders? The QuickBMS script replaces any _ with a / to make directories, but that feels very wrong to me.
    // TODO: What is the sequence of 0x204 bytes that I'm not reading for? Nulling them crashes the game.
    // TODO: What is the unknown value in the file data for? Nulling them crashes the game. Checksum?
    public class DataArchive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public DataArchive() { }
        public DataArchive(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Classes for this format.
        public class DataEntry
        {
            /// <summary>
            /// The name of this file.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Could this be a checksum value of some sort? Tried putting a file's data (compressed and uncompressed) through a CRC32 calculator and got no matches.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// Whether or not this file is compressed.
            /// </summary>
            public bool IsCompressed { get; set; }

            /// <summary>
            /// How large this file is when compressed.
            /// </summary>
            public ulong CompressedSize { get; set; }

            /// <summary>
            /// How large this file is when decompressed.
            /// </summary>
            public ulong UncompressedSize { get; set; }

            /// <summary>
            /// The offset in the archive to this file's data.
            /// </summary>
            public long DataOffset { get; set; }

            /// <summary>
            /// The data that makes up this file.
            /// </summary>
            public byte[]? Data { get; set; }

            public override string ToString() => Name;
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

            // Read this file's signature.
            reader.ReadSignature(4, new byte[4] { 0x12, 0x2F, 0xF3, 0x18 });

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x02);

            // Skip an unknown value of 0x0606
            reader.JumpAhead(0x02);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x02);

            // Skip an unknown value of 0x07.
            reader.JumpAhead(0x02);

            // Read this file's first unknown offset, consisting of 0x204 bytes that I don't know about.
            // These are important in someway, as nulling them made the game crash on launch, failing to find Boot.vol.
            uint UnknownOffset_1 = reader.ReadUInt32();

            // Read this file's file count.
            uint FileCount = reader.ReadUInt32();

            // Read the offset to this file's name table.
            uint FileNameTableOffset = reader.ReadUInt32();

            // Read this file's second unknown offset, consisting of a value for each file in it.
            // These are important in someway, as nulling them made the game crash on launch, failing to find Boot.vol.
            uint UnknownOffset_2 = reader.ReadUInt32();

            // Read this file's compression table offset.
            uint CompressionTableOffset = reader.ReadUInt32();

            // Read this file's compressed size table offset.
            uint CompressedSizeTableOffset = reader.ReadUInt32();

            // Read this file's uncompressed size table offset.
            uint UncompressedSizeTableOffset = reader.ReadUInt32();

            // Read this file's data offset table offset.
            uint DataOffsetTableOffset = reader.ReadUInt32();

            // Read this file's data table offset.
            uint DataTableOffset = reader.ReadUInt32();

            // Create an array of data entries based on how many files this archive has.
            DataEntry[] entries = new DataEntry[FileCount];

            // Jump to the file name table.
            reader.JumpTo(FileNameTableOffset);

            // Loop through and read each file's name.
            for (int i = 0; i < FileCount; i++)
            {
                // Initalise the file at this index.
                entries[i] = new();

                // Read this file's name.
                entries[i].Name = Helpers.ReadNullTerminatedStringTableEntry(reader);
            }

            // Jump to the second unknown offset.
            reader.JumpTo(UnknownOffset_2);

            // Loop through and read each unknown value here.
            for (int i = 0; i < FileCount; i++)
                entries[i].UnknownUInt32_1 = reader.ReadUInt32();

            // Jump to this file's compression table.
            reader.JumpTo(CompressionTableOffset);

            // Loop through and read each file's compressed flag.
            for (int i = 0; i < FileCount; i++)
                entries[i].IsCompressed = reader.ReadBoolean();

            // Jump to this file's compressed size table.
            reader.JumpTo(CompressedSizeTableOffset);

            // Loop through and read each file's compressed size.
            for (int i = 0; i < FileCount; i++)
                entries[i].CompressedSize = reader.ReadUInt64();

            // Jump to this file's uncompressed size table.
            reader.JumpTo(UncompressedSizeTableOffset);

            // Loop through and read each file's uncompressed size.
            for (int i = 0; i < FileCount; i++)
                entries[i].UncompressedSize = reader.ReadUInt64();

            // Jump to this file's data offset table.
            reader.JumpTo(DataOffsetTableOffset);

            // Loop through and read each file's data offset.
            for (int i = 0; i < FileCount; i++)
                entries[i].DataOffset = reader.ReadInt64();

            // Loop through, read and decompress each file's data.
            for (int i = 0; i < FileCount; i++)
            {
                // Print the name of the file we're deccompressing.
                Console.WriteLine($"Decompressing {entries[i].Name}.");

                // Jump to this file's data offset.
                reader.JumpTo(entries[i].DataOffset);

                // Set up an output stream for the decompressed data.
                using MemoryStream outputStream = new();

                // Set up a deflate stream to decompress this file's data.
                using DeflateStream deflateStream = new(new MemoryStream(reader.ReadBytes((int)entries[i].CompressedSize)), CompressionMode.Decompress);

                // Copy the output of the deflate stream to the output stream.
                deflateStream.CopyTo(outputStream);

                // Read the output stream as an array to get this file's data.
                entries[i].Data = outputStream.ToArray();

                // Add this file to a standard FileNode list.
                Data.Add(new()
                {
                    Name = entries[i].Name,
                    Data = entries[i].Data
                });
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
            // Set up a list of compressed data.
            List<byte[]> CompressedData = new();

            // Loop through and compress each file.
            for (int i = 0; i < Data.Count; i++)
            {
                // Print the name of the file we're compressing.
                Console.WriteLine($"Compressing {Data[i].Name}.");

                // Use a memory stream and deflate stream to compress the data.
                using MemoryStream outputStream = new();
                using (DeflateStream deflateStream = new(outputStream, CompressionMode.Compress))
                {
                    deflateStream.Write(Data[i].Data, 0, Data[i].Data.Length);
                }

                // Add this decompressed data to our list.
                CompressedData.Add(outputStream.ToArray());
            }

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write this file's signature.
            writer.Write(0x18F32F12);

            // Write an unknown value of 0x01.
            writer.Write((ushort)0x01);

            // Write an unknown value of 0x0606.
            writer.Write((ushort)0x0606);

            // Write an unknown value of 0.
            writer.Write((ushort)0);

            // Write an unknown value of 0x07.
            writer.Write((ushort)0x07);

            // Add the offset to this file's first unknown values.
            writer.AddOffset("UnknownOffset_1");

            // Write the amount of files in this archive.
            writer.Write(Data.Count);

            // Add the offset to this archive's file name table.
            writer.AddOffset("FileNameTableOffset");

            // Add the offset to this file's second unknown values.
            writer.AddOffset("UnknownOffset_2");

            // Add the offset to this archive's compression table.
            writer.AddOffset("CompressionTableOffset");

            // Add the offset to this archive's compressed file size table.
            writer.AddOffset("CompressedSizeTableOffset");

            // Add the offset to this archive's uncompressed file size table.
            writer.AddOffset("UncompressedSizeTableOffset");

            // Add the offset to this archive's data offset table.
            writer.AddOffset("DataOffsetTableOffset");

            // Add the offset to this archive's data table.
            writer.AddOffset("DataTableOffset");

            // Fill in the offset for the first unknown values.
            writer.FillOffset("UnknownOffset_1");

            //Awful hardcoded hack for the unknown chunk of data.
            writer.Write(new byte[0x204] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x1D, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x26, 0x00, 0x00, 0x00, 0x2E, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x3D, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x45, 0x00, 0x00, 0x00, 0x49, 0x00, 0x00, 0x00, 0x4D, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00, 0x56, 0x00, 0x00, 0x00, 0x5E, 0x00, 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x66, 0x00, 0x00, 0x00, 0x6A, 0x00, 0x00, 0x00, 0x6E, 0x00, 0x00, 0x00, 0x71, 0x00, 0x00, 0x00, 0x76, 0x00, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00, 0x7F, 0x00, 0x00, 0x00, 0x83, 0x00, 0x00, 0x00, 0x88, 0x00, 0x00, 0x00, 0x8E, 0x00, 0x00, 0x00, 0x95, 0x00, 0x00, 0x00, 0x97, 0x00, 0x00, 0x00, 0x9A, 0x00, 0x00, 0x00, 0xA0, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x00, 0xAD, 0x00, 0x00, 0x00, 0xB3, 0x00, 0x00, 0x00, 0xB8, 0x00, 0x00, 0x00, 0xC2, 0x00, 0x00, 0x00, 0xC7, 0x00, 0x00, 0x00, 0xCD, 0x00, 0x00, 0x00, 0xD5, 0x00, 0x00, 0x00, 0xD9, 0x00, 0x00, 0x00, 0xDC, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0xEC, 0x00, 0x00, 0x00, 0xED, 0x00, 0x00, 0x00, 0xF1, 0x00, 0x00, 0x00, 0xF6, 0x00, 0x00, 0x00, 0xF7, 0x00, 0x00, 0x00, 0xFB, 0x00, 0x00, 0x00, 0xFD, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x08, 0x01, 0x00, 0x00, 0x0E, 0x01, 0x00, 0x00, 0x11, 0x01, 0x00, 0x00, 0x15, 0x01, 0x00, 0x00, 0x19, 0x01, 0x00, 0x00, 0x1F, 0x01, 0x00, 0x00, 0x25, 0x01, 0x00, 0x00, 0x27, 0x01, 0x00, 0x00, 0x2B, 0x01, 0x00, 0x00, 0x2E, 0x01, 0x00, 0x00, 0x30, 0x01, 0x00, 0x00, 0x33, 0x01, 0x00, 0x00, 0x36, 0x01, 0x00, 0x00, 0x39, 0x01, 0x00, 0x00, 0x3A, 0x01, 0x00, 0x00, 0x3E, 0x01, 0x00, 0x00, 0x41, 0x01, 0x00, 0x00, 0x45, 0x01, 0x00, 0x00, 0x48, 0x01, 0x00, 0x00, 0x4F, 0x01, 0x00, 0x00, 0x53, 0x01, 0x00, 0x00, 0x57, 0x01, 0x00, 0x00, 0x5C, 0x01, 0x00, 0x00, 0x5F, 0x01, 0x00, 0x00, 0x62, 0x01, 0x00, 0x00, 0x66, 0x01, 0x00, 0x00, 0x6B, 0x01, 0x00, 0x00, 0x6F, 0x01, 0x00, 0x00, 0x76, 0x01, 0x00, 0x00, 0x7A, 0x01, 0x00, 0x00, 0x7D, 0x01, 0x00, 0x00, 0x84, 0x01, 0x00, 0x00, 0x87, 0x01, 0x00, 0x00, 0x8A, 0x01, 0x00, 0x00, 0x8F, 0x01, 0x00, 0x00, 0x95, 0x01, 0x00, 0x00, 0x9B, 0x01, 0x00, 0x00, 0xA0, 0x01, 0x00, 0x00, 0xA4, 0x01, 0x00, 0x00, 0xA7, 0x01, 0x00, 0x00, 0xAA, 0x01, 0x00, 0x00, 0xB0, 0x01, 0x00, 0x00, 0xB3, 0x01, 0x00, 0x00, 0xB7, 0x01, 0x00, 0x00, 0xBE, 0x01, 0x00, 0x00, 0xC4, 0x01, 0x00, 0x00, 0xCB, 0x01, 0x00, 0x00, 0xCC, 0x01, 0x00, 0x00, 0xD0, 0x01, 0x00, 0x00, 0xD6, 0x01, 0x00, 0x00, 0xDA, 0x01, 0x00, 0x00, 0xDF, 0x01, 0x00, 0x00, 0xDF, 0x01, 0x00, 0x00, 0xEB, 0x01, 0x00, 0x00, 0xEF, 0x01, 0x00, 0x00, 0xF5, 0x01, 0x00, 0x00, 0xFA, 0x01, 0x00, 0x00, 0xFF, 0x01, 0x00, 0x00, 0x03, 0x02, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x10, 0x02, 0x00, 0x00, 0x18, 0x02, 0x00, 0x00, 0x1E, 0x02, 0x00, 0x00, 0x1E, 0x02, 0x00, 0x00, 0x22, 0x02, 0x00, 0x00, 0x25, 0x02, 0x00, 0x00, 0x28, 0x02, 0x00, 0x00, 0x2B, 0x02, 0x00, 0x00, 0x2C, 0x02, 0x00, 0x00, 0x2F, 0x02, 0x00, 0x00, 0x34, 0x02, 0x00, 0x00, 0x39, 0x02, 0x00, 0x00, 0x3D, 0x02, 0x00, 0x00 });

            // Fill in the offset for this archive's file name table.
            writer.FillOffset("FileNameTableOffset");

            // Loop through and add an offset for each file's name.
            for (int i = 0; i < Data.Count; i++)
                writer.AddOffset($"File{i}Name");

            // Loop through and write each file's name.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in the offset for this file's name.
                writer.FillOffset($"File{i}Name");

                // Write this file's name.
                writer.WriteNullTerminatedString(Data[i].Name);
            }

            // Realign to 0x04 bytes.
            // TODO: Should this be 0x08 instead?
            writer.FixPadding(0x04);

            // Fill in the offset for the second unknown values.
            writer.FillOffset("UnknownOffset_2");

            // Loop through and write a placeholder for each file.
            // TODO: Fill in.
            for (int i = 0; i < Data.Count; i++)
                writer.Write("UKWN");

            // Fill in the offset for the compressison table.
            writer.FillOffset("CompressionTableOffset");

            // Loop through and write a 0x01 for each file.
            // TODO: Unhardcode this.
            for (int i = 0; i < Data.Count; i++)
                writer.Write(true);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Fill in the offset for the compressed size table.
            writer.FillOffset("CompressedSizeTableOffset");

            // Loop through and write a placeholder for each file.
            for (int i = 0; i < Data.Count; i++)
                writer.Write((ulong)CompressedData[i].Length);

            // Fill in the offset for the uncompressed size table.
            writer.FillOffset("UncompressedSizeTableOffset");

            // Loop through and write each file's uncompressed size.
            for (int i = 0; i < Data.Count; i++)
                writer.Write((ulong)Data[i].Data.Length);

            // Fill in the offset for the data offset table.
            writer.FillOffset("DataOffsetTableOffset");

            // Loop through and add an offset for each file's data.
            for (int i = 0; i < Data.Count; i++)
                writer.AddOffset($"File{i}Data", 0x08);

            // Fill in the offset for the data offset table.
            writer.FillOffset("DataTableOffset");

            // Loop through and write each file's compressed name.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in the offset for this file's data.
                writer.FillOffset($"File{i}Data");

                // Write this file's data.
                writer.Write(CompressedData[i]);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory);

        /// <summary>
        /// Imports files from a directory into an archive node.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
