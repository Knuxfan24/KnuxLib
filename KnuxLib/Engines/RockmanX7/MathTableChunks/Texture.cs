using System.Drawing;
using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7.MathTableChunks
{
    public class Texture
    {
        public class Palette
        {
            /// <summary>
            /// The red value of this colour.
            /// </summary>
            public byte Red { get; set; } = 0xFF;

            /// <summary>
            /// The green value of this colour.
            /// </summary>
            public byte Green { get; set; } = 0xFF;

            /// <summary>
            /// The blue value of this colour.
            /// </summary>
            public byte Blue { get; set; } = 0xFF;

            /// <summary>
            /// The alpha value of this colour.
            /// TODO: This is often set to 0x80 instead of 0xFF, why?
            /// </summary>
            public byte Alpha { get; set; } = 0x80;
        }

        public static List<Bitmap> Read(BinaryReaderEx reader)
        {
            // Initialise the texture list.
            List<Bitmap> textures = new();

            // Set the reader's offset to the start of this texture chunk.
            reader.Offset = (uint)reader.BaseStream.Position;

            // Check that the next byte is always set to 0.
            if (reader.ReadByte() != 0) Debugger.Break();

            // Read the count of textures (including their palettes) in this chunk.
            byte textureCount = reader.ReadByte();

            // Check that the next two bytes are always set to 0.
            if (reader.ReadByte() != 0) Debugger.Break();
            if (reader.ReadByte() != 0) Debugger.Break();

            // Check that the next three values are always set to 0.
            if (reader.ReadUInt32() != 0) Debugger.Break();
            if (reader.ReadUInt32() != 0) Debugger.Break();
            if (reader.ReadUInt32() != 0) Debugger.Break();

            // Loop through each texture and its palette.
            for (byte textureIndex = 0; textureIndex < textureCount; textureIndex += 2)
            {
                // Check that the next byte is always set to 1. (Likely an identifier to indicate this is a texture).
                if (reader.ReadByte() != 1) Debugger.Break();

                // Read this texture's format.
                byte formatType = reader.ReadByte();

                // Check that the next value is always set to 0.
                if (reader.ReadUInt16() != 0) Debugger.Break();

                // Read this texture's dimensions.
                ushort textureWidth = reader.ReadUInt16();
                ushort textureHeight = reader.ReadUInt16();

                // Read the offset to this texture's data.
                uint textureOffset = reader.ReadUInt32();

                // Check that the next value is always set to 0.
                if (reader.ReadUInt32() != 0) Debugger.Break();

                // Check that the next byte is always set to 2. (Likely an identifier to indicate this is a palette).
                if (reader.ReadByte() != 2) Debugger.Break();

                // Check that the next byte is always set to 0.
                if (reader.ReadByte() != 0) Debugger.Break();

                // Check that the next value is always set to 0.
                if (reader.ReadUInt16() != 0) Debugger.Break();

                // Read this palette's dimensions (used to determine how many bytes make up the data).
                ushort paletteWidth = reader.ReadUInt16();
                ushort paletteHeight = reader.ReadUInt16();

                // Read the offset to this palette's data.
                uint paletteOffset = reader.ReadUInt32();

                // Check that the next value is always set to 0.
                if (reader.ReadUInt32() != 0) Debugger.Break();

                // Save our current position so we can jump back for the next texture.
                long position = reader.BaseStream.Position;
                
                // Jump to the texture's palette data.
                reader.JumpTo(paletteOffset, true);

                // Set up a new palette array, using the width and height to determine the size.
                Palette[] palette = new Palette[paletteWidth * paletteHeight];

                // Loop through and read each colour in this texture's palette.
                for (int paletteIndex = 0; paletteIndex < palette.Length; paletteIndex++)
                {
                    palette[paletteIndex] = new()
                    {
                        Red = reader.ReadByte(),
                        Green = reader.ReadByte(),
                        Blue = reader.ReadByte(),
                        Alpha = reader.ReadByte()
                    };
                }

                // Jump to this texture's data.
                reader.JumpTo(textureOffset, true);

                // Set up a new bitmap with the width and height of this texture.
                Bitmap bmp = new(textureWidth, textureHeight);

                // TODO: Read the two texture types, not sure on 0x14 at all, but 0x13 seems to be a list of palette values, but I cannot figure out how they're pieced together.
                
                // Save this texture.                
                textures.Add(bmp);

                // Jump back for the next texture.
                reader.JumpTo(position);
            }
                        
            // Return our list of textures.
            return textures;
        }
    }
}
