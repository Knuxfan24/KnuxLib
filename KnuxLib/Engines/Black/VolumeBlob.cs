using System.IO.Compression;

namespace KnuxLib.Engines.Black
{
    // Based on https://github.com/meh2481/MSFHDEx
    // TODO: Handle the GOG version, as it updated the format in some way?
    // TODO: Format saving.
    // TODO: See if the unknown values in the header are important in some way.
    public class VolumeBlob : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public VolumeBlob() { }
        public VolumeBlob(string filepath, bool extract = false)
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

            // Check if this is a compressed file, ones extracted from the PC version's data archive won't be, but ones from the 3DS ROMFS will be.
            bool isCompressed = reader.ReadUInt64() == 0x101D177F050;

            // If this file is compressed, then decompress it.
            if (isCompressed)
            {
                // Read the compressed size of this file.
                int compressedSize = reader.ReadInt32();

                // Read the uncompressed size of this file.
                int uncompressedize = reader.ReadInt32();

                // Set up an output stream for the decompressed data.
                MemoryStream outputStream = new();

                // Set up a deflate stream to decompress this file's data.
                using DeflateStream deflateStream = new(new MemoryStream(reader.ReadBytes(compressedSize)), CompressionMode.Decompress);

                // Copy the output of the deflate stream to the output stream.
                deflateStream.CopyTo(outputStream);

                // Reinitalise the reader with the output stream.
                reader = new(outputStream);
            }

            // Jump back to the start of the file.
            reader.JumpTo(0);

            // Skip an unknown value that is always CB 32 3D B5.
            // TODO: This is different in the GOG version!
            reader.JumpAhead(0x04);

            // Read an unknown integer value.
            // TODO: What is this and is it important?
            uint unknownUInt32_1 = reader.ReadUInt32();

            // Skip an unknown value that is always 0x1C
            reader.JumpAhead(0x04);

            // Read the count of entries in this blob.
            uint entryCount = reader.ReadUInt32();

            // Read the size of this Volume Blob's header.
            uint headerSize = reader.ReadUInt32();

            // Set up a list of extra unknown values in the header.
            // TODO: What are these?
            List<uint> extraHeaderUnknowns = new();

            // Read the unknown header values.
            while (reader.BaseStream.Position < headerSize)
                extraHeaderUnknowns.Add(reader.ReadUInt32());

            // Loop through and read each entry.
            for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                // Set up a new node.
                FileNode node = new();

                // Read this node's unknown integer value.
                // TODO: What is this and is important?
                uint fileUnknownUInt32_1 = reader.ReadUInt32();

                // Read this node's file name.
                node.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read the offset to this node's data.
                uint dataOffset = reader.ReadUInt32();

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this node's length.
                int dataLength = reader.ReadInt32();

                // Save our current position so we can jump back for the next file.
                long position = reader.BaseStream.Position;

                // Jump to this node's data offset.
                reader.JumpTo(dataOffset);

                // Read this node's data.
                node.Data = reader.ReadBytes(dataLength);

                // Jump back for the next node.
                reader.JumpTo(position);

                // Save this node.
                Data.Add(node);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory);

        /// <summary>
        /// Imports files from a directory into this format.
        /// </summary>
        /// <param name="directory">The directory to import.</param>
        public void Import(string directory) => Data = Helpers.ImportArchive(directory);
    }
}
