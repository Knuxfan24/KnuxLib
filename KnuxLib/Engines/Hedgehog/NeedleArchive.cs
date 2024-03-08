using System.Diagnostics;

namespace KnuxLib.Engines.Hedgehog
{
    public class NeedleArchive : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public NeedleArchive() { }
        public NeedleArchive(string filepath, bool extract = false)
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
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            reader.ReadSignature(0x08, "NEDARCV1");

            uint fileLength = reader.ReadUInt32(); // not including the NEDARCV1 sig.

            if (fileLength != reader.BaseStream.Length - 0x08) Debugger.Break();

            reader.ReadSignature(0x03, "arc");

            if (reader.ReadByte() != 0) Debugger.Break(); //padding?

            int lodCount = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                FileNode file = new();

                string needleFileType = reader.ReadNullPaddedString(0x10);

                if (needleFileType == "NEDLDIV1lodinfo")
                    file.Name = $"{Path.GetFileNameWithoutExtension(filepath)}.lodinfo";
                if (needleFileType == "NEDMDLV5model")
                {
                    file.Name = $"{Path.GetFileNameWithoutExtension(filepath)}.{lodCount}{Path.GetExtension(filepath)}";
                    lodCount++;
                }

                file.Data = reader.ReadBytes(reader.ReadInt32());

                Data.Add(file);
            }
        }

        /// <summary>
        /// Extracts the files in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory) => Helpers.ExtractArchive(Data, directory, "hedgehog_needle");
    }
}
