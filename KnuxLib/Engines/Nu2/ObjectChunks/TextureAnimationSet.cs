using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: All of this data is unknown, figure it out.
    public class TextureAnimationSet
    {
        public class TextureAnimationData
        {
            /// <summary>
            /// A list of the animations in this file.
            /// </summary>
            public List<Animation> TextureAnimations { get; set; } = new();

            /// <summary>
            /// A list of UInt16s.
            /// TODO: What are these?
            /// </summary>
            public List<ushort> UnknownUShortList_1 { get; set; } = new();
        }

        public class Animation
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_4 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_6 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_7 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_8 { get; set; }
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static TextureAnimationData Read(BinaryReaderEx reader, FormatVersion version)
        {
            // Set up our texture animation data.
            TextureAnimationData TextureAnimation = new();

            // Read the amount of animations in this file.
            uint animationCount = reader.ReadUInt32();

            // Skip an unknown value of 0x00.
            reader.JumpAhead(0x04);

            // Loop through and read each of this file's animations.
            for (int animationIndex = 0; animationIndex < animationCount; animationIndex++)
            {
                // Set up an animation entry.
                Animation animation = new();

                // Read this animation's first unknown value.
                animation.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this animation's second unknown value.
                animation.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this animation's third unknown value.
                animation.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this animation's fourth unknown value.
                animation.UnknownUInt32_4 = reader.ReadUInt32();

                // Read this animation's fifth unknown value.
                animation.UnknownUInt32_5 = reader.ReadUInt32();

                // Read this animation's sixth unknown value.
                animation.UnknownUInt32_6 = reader.ReadUInt32();

                // Read this animation's seventh unknown value.
                animation.UnknownUInt32_7 = reader.ReadUInt32();

                // Read this animation's eighth unknown value.
                animation.UnknownUInt32_8 = reader.ReadUInt32();

                // Save this texture animation.
                TextureAnimation.TextureAnimations.Add(animation);
            }

            // Read the size of the ushort table for this chunk.
            uint ushortCount = reader.ReadUInt32();

            // Loop through and read each ushort.
            for (int ushortIndex = 0; ushortIndex < ushortCount; ushortIndex++)
                TextureAnimation.UnknownUShortList_1.Add(reader.ReadUInt16());

            // Return the texture animation data.
            return TextureAnimation;
        }
    }
}
