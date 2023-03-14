using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: Work out the data that makes up an Xbox texture.
    // TODO: Work out the data that makes up a PlayStation 2 texture.
    public class TextureSet
    {
        // Classes for this NuObject chunk.
        public class TextureData
        {
            /// <summary>
            /// The type of this texture.
            /// TODO: Get a list of these and turn this into an enum.
            /// </summary>
            public uint Type { get; set; }

            /// <summary>
            /// The width of this texture in pixels.
            /// </summary>
            public uint Width { get; set; }

            /// <summary>
            /// The height of this texture in pixels.
            /// </summary>
            public uint Height { get; set; }

            /// <summary>
            /// The bytes that make up this texture's data.
            /// </summary>
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static List<TextureData> Read(BinaryReaderEx reader, FormatVersion version)
        {
            // Set up our list of Textures.
            List<TextureData> Textures = new();

            // Read and check the TextureSet Counter's chunk type.
            string chunkType = reader.ReadNullPaddedString(0x04);

            // If this is a GameCube file, then flip chunkType.
            if (version == FormatVersion.GameCube)
                chunkType = new string(chunkType.Reverse().ToArray());

            if (chunkType != "TSH0")
                throw new Exception($"Expected 'TSH0', got '{chunkType}'.");

            // Read the size of the Texture Counter's chunk.
            uint textureCounterChunkSize = reader.ReadUInt32();

            // Read the count of textures in this file.
            uint textureCount = reader.ReadUInt32();

            // Loop through based on the amount of textures listed in the Counter and read them.
            for (int i = 0; i < textureCount; i++)
            {
                // Read and check the texture's chunk type.
                chunkType = reader.ReadNullPaddedString(0x04);

                // If this is a GameCube file, then flip chunkType.
                if (version == FormatVersion.GameCube)
                    chunkType = new string(chunkType.Reverse().ToArray());

                if (chunkType != "TXM0")
                    throw new Exception($"Expected 'TXM0', got '{chunkType}'.");

                // Read the size of this texture's chunk.
                uint textureChunkSize = reader.ReadUInt32();

                // Set up a texture entry.
                TextureData texture = new();

                // Read this texture's data depending on the format version.
                switch (version)
                {
                    case FormatVersion.GameCube:
                        texture = new()
                        {
                            Type = reader.ReadUInt32(),
                            Width = reader.ReadUInt32(),
                            Height = reader.ReadUInt32(),
                            Data = reader.ReadBytes(reader.ReadInt32())
                        };
                        break;

                    case FormatVersion.Xbox:
                        texture = new()
                        {
                            Type = reader.ReadUInt32(),
                            Width = reader.ReadUInt32(),
                            Height = reader.ReadUInt32()
                        };
                        break;
                }

                // Add this texture to our list.
                Textures.Add(texture);
            }

            // Return the list of textures read from the file.
            return Textures;
        }
    }
}
