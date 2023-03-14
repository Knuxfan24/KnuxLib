using KnuxLib.Engines.Nu2.ObjectChunks;

namespace KnuxLib.Engines.Nu2
{
    // TODO: Finish reverse engineering the differences in the PS2 and Xbox versions.
    // TODO: Write a way to save this format.
    // TODO: Write a way to import models to this format.
    // TODO: Write a way to export models from this format.
    // TODO: Implement a way to decompress/recompress the PS2 version's RnC compression (https://segaretro.org/Rob_Northen_compression).
    public class Scene : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Scene() { }
        public Scene(string filepath, FormatVersion version = FormatVersion.GameCube, bool extract = false)
        {
            Load(filepath, version);

            if (extract)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            Xbox = 1,
            PlayStation2 = 2
        }

        public class FormatData
        {
            /// <summary>
            /// This scene's name table.
            /// </summary>
            public List<string>? NameTable { get; set; }

            /// <summary>
            /// This scene's texture data.
            /// </summary>
            public List<TextureSet.TextureData>? Textures { get; set; }

            /// <summary>
            /// This scene's materials.
            /// </summary>
            public List<MaterialSet.MaterialData>? Materials { get; set; }

            /// <summary>
            /// This scene's geometry data.
            /// </summary>
            public List<GeometrySet.GeometryData>? Geometry { get; set; }

            /// <summary>
            /// This scene's instances.
            /// </summary>
            public InstanceSet.InstanceData? Instances { get; set; }

            /// <summary>
            /// This scene's SPEC data.
            /// TODO: What does SPEC stand for?
            /// </summary>
            public List<SPECSet.SPECData>? SPEC { get; set; }

            /// <summary>
            /// This scene's splines.
            /// </summary>
            public List<SplineSet.SplineData>? Splines { get; set; }

            /// <summary>
            /// This scene's LDIR data.
            /// TODO: What does LDIR stand for?
            /// TODO: Why is this always just a large chunk of empty data?
            /// </summary>
            public byte[]? LDIR { get; set; }

            /// <summary>
            /// This scene's texture animations.
            /// </summary>
            public TextureAnimationSet.TextureAnimationData? TextureAnimations { get; set; }

            /// <summary>
            /// The value of this scene's SPHE chunk, if it has one.
            /// TODO: What does SPHE stand for?
            /// TODO: What does this chunk do, if anything?
            /// </summary>
            public uint? SPHEValue { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature and some other initial data based on the format version.
            switch (version)
            {
                case FormatVersion.GameCube:
                    // Switch the reader to Big Endian if this is a GameCube file.
                    reader.IsBigEndian = true;
                    
                    // Check for the GSC0 signature (stored as 0CSG due to endianness).
                    reader.ReadSignature(0x04, "0CSG");

                    // Read this file's filesize.
                    uint fileSize = reader.ReadUInt32();
                    break;

                case FormatVersion.Xbox:
                    // Check for the NUX0 signature.
                    reader.ReadSignature(0x04, "NUX0");

                    // Read an unknown value.
                    uint unknownXboxValue = reader.ReadUInt32();
                    break;

                case FormatVersion.PlayStation2:
                    // Check for the NU20 signature.
                    reader.ReadSignature(0x04, "NU20");

                    // Read an unknown value.
                    uint unknownPS2Value = reader.ReadUInt32();

                    // Skip two unknown values of 0x06 and 0x00.
                    reader.JumpAhead(0x08);
                    break;
            }

            // Loop through chunks until we hit the end of the file.
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Store this chunk's position.
                long chunkStartPosition = reader.BaseStream.Position;

                // Read this chunk's type.
                string chunkType = reader.ReadNullPaddedString(0x04);

                // If this is a GameCube file, then flip chunkType.
                if (version == FormatVersion.GameCube)
                    chunkType = new string(chunkType.Reverse().ToArray());

                // Read this chunk's size.
                uint chunkSize = reader.ReadUInt32();

                // Process this chunk depending on the type.
                switch (chunkType)
                {
                    case "NTBL": Data.NameTable = NameTable.Read(reader, version); break;
                    //case "TST0": Data.Textures = TextureSet.Read(reader, version); break;
                    case "MS00": Data.Materials = MaterialSet.Read(reader, version); break;
                    case "GST0": Data.Geometry = GeometrySet.Read(reader, version); break;
                    case "INST": Data.Instances = InstanceSet.Read(reader); break;
                    case "SPEC": Data.SPEC = SPECSet.Read(reader, version); break;
                    case "SST0": Data.Splines = SplineSet.Read(reader, version); break;
                    case "LDIR": Data.LDIR = reader.ReadBytes((int)chunkSize - 0x08); break;
                    case "TAS0": Data.TextureAnimations = TextureAnimationSet.Read(reader, version); break;
                    //case "ALIB": ALIBSet.Read(reader, version); break;
                    case "SPHE": reader.JumpBehind(0x04); Data.SPHEValue = reader.ReadUInt32(); break;

                    // Skip this chunk if we don't yet support it.
                    default:
                        Console.WriteLine($"'{chunkType}' at '0x{(reader.BaseStream.Position - 0x08).ToString("X").PadLeft(8, '0')}' with a size of '0x{chunkSize.ToString("X").PadLeft(8, '0')}' not yet handled.");
                        break;
                }

                // Jump to the end of this chunk by caluclating where it started and its size.
                reader.JumpTo(chunkStartPosition + chunkSize);
            
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }
    }
}
