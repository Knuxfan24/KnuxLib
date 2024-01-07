using System.Drawing;
using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7.MathTableChunks
{
    // TODO: Read the other texture type (0x14).
    // TODO: Fix the palettes.
    // TODO: Figure out if the alpha value in the palette actually IS an alpha value or something else.
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

                if (formatType == 0x13)
                {
                    // Set up the list of 16x16 blocks.
                    List<Bitmap> _16x16blocks = new();

                    // Loop through each pixel and create the 16x16 blocks from them.
                    for (int blockIndex = 0; blockIndex < ((textureWidth / 16) * (textureHeight / 16)); blockIndex++)
                    {
                        // Set up a new 16x16 bitmap for this block.
                        Bitmap block = new(16, 16);

                        // Loop through 16x16 times.
                        for (int y = 0; y < 16; y++)
                        {
                            for (int x = 0; x < 16; x++)
                            {
                                // Read this pixel's palette index and look it up from the palette array.
                                Palette paletteValue = palette[reader.ReadByte()];

                                // Set the current pixel to the colour that we read.
                                block.SetPixel(x, y, Color.FromArgb(255, paletteValue.Red, paletteValue.Green, paletteValue.Blue));
                            }
                        }

                        // Save this 16x16 block into the list.
                        _16x16blocks.Add(block);
                    }

                    // Run the MergeBlocks function twice to piece together the 64x64 blocks.
                    List<Bitmap> _64x64blocks = MergeBlocks(MergeBlocks(_16x16blocks, 32), 64);

                    // Set up a graphics device to create the final image.
                    Graphics final = Graphics.FromImage(bmp);

                    // Set up a count to track the block index.
                    int finalBlockIndex = 0;

                    // Loop through based on the texture height and width, divided by 64.
                    for (int height = 0; height < textureHeight / 64; height++)
                    {
                        for (int width = 0; width < textureWidth / 64; width++)
                        {
                            // Attach the current block index at the right position in the final image.
                            final.DrawImage(_64x64blocks[finalBlockIndex], 0 + (width * 64), (height * 64));

                            // Increment finalBlockIndex so we read the correct 64x64 block.
                            finalBlockIndex++;
                        }
                    }
                }
                
                // Save this texture.                
                textures.Add(bmp);

                // Jump back for the next texture.
                reader.JumpTo(position);
            }
                        
            // Return our list of textures.
            return textures;
        }

        /// <summary>
        /// Used to merge the 16x16 blocks into 32x32 chunks, and then again to merge those into 64x64 chunks.
        /// </summary>
        /// <param name="blocks">The block list to process.</param>
        /// <param name="outBlockSize">The size we're processing to (either 32 or 64).</param>
        /// <returns>The new block list.</returns>
        private static List<Bitmap> MergeBlocks(List<Bitmap> blocks, int outBlockSize)
        {
            // Set up the block list to return.
            List<Bitmap> outBlocks = new();

            // Loop through each block in the list, four at a time.
            for (int blockIndex = 0; blockIndex < blocks.Count; blockIndex += 4)
            {
                // Set up a bitmap for the merge.
                Bitmap mergedBlock = new(outBlockSize, outBlockSize);

                // Use a graphics device to combine the four blocks.
                using (Graphics g = Graphics.FromImage(mergedBlock))
                {
                    g.DrawImage(blocks[blockIndex], 0, 0);
                    g.DrawImage(blocks[blockIndex + 1], outBlockSize / 2, 0);
                    g.DrawImage(blocks[blockIndex + 2], 0, outBlockSize / 2);
                    g.DrawImage(blocks[blockIndex + 3], outBlockSize / 2, outBlockSize / 2);
                }

                // Save the new block.
                outBlocks.Add(mergedBlock);
            }

            // Return the new block list.
            return outBlocks;
        }
    }
}
