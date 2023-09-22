using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class Texture
    {
        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CompressionType : uint
        {
            DXT1 = 0x31,
            DXT5 = 0x64
        }

        /// <summary>
        /// The width of this texture.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// The height of this texture.
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_1 { get; set; }

        /// <summary>
        /// The compression type used for this texture.
        /// </summary>
        public CompressionType Type { get; set; }

        /// <summary>
        /// The hash that identifies this texture.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// The binary data that makes up this texture.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// The file name of this texture.
        /// </summary>
        public string Name { get; set; } = "";

        public override string ToString() => Name;

        /// <summary>
        /// Read the data of this texture from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static Texture Read(BinaryReaderEx reader)
        {
            // Create a new texture entry.
            Texture texture = new();

            // Read this texture's width.
            texture.Width = reader.ReadUInt32();

            // Read this texture's height.
            texture.Height = reader.ReadUInt32();

            // Read the first unknown integer value in this texture.
            texture.UnknownUInt32_1 = reader.ReadUInt32();

            // Read this texture's compression type.
            texture.Type = (CompressionType)reader.ReadUInt32();

            // Read this texture's hash.
            texture.Hash = reader.ReadUInt64();

            // Read the offset to this texture's image data.
            uint imageDataOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the image data offset.
            uint imageDataDataSize = reader.ReadUInt32();

            // Read the offset to this texture's file name.
            uint imageNameOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at the image name offset.
            uint imageNameDataSize = reader.ReadUInt32();

            // Save our position to jump back to after reading the texture image data and name.
            long position = reader.BaseStream.Position;

            // Jump to this texture's image data offset.
            reader.JumpTo(imageDataOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{imageNameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            int dataSize = reader.ReadInt32();

            // Read this texture's image data.
            texture.Data = reader.ReadBytes(dataSize);

            // Jump to this texture's name offset.
            reader.JumpTo(imageNameOffset);

            // Read and check this data's magic value.
            dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{imageNameOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            dataSize = reader.ReadInt32();

            // Read this texture's file name.
            texture.Name = reader.ReadNullTerminatedString();

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return our read texture.
            return texture;
        }
    }
}
