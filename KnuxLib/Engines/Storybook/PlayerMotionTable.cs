namespace KnuxLib.Engines.Storybook
{
    public class PlayerMotionTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PlayerMotionTable() { }
        public PlayerMotionTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.playermotion.json", Data);
        }

        // Classes for this format.
        public class MotionEntry
        {
            /// <summary>
            /// The index that other motion entries use to reference this one.
            /// </summary>
            public uint AnimationIndex { get; set; }

            /// <summary>
            /// The Ninja Animation file associated with this motion.
            /// </summary>
            public string AssociatedNinjaMotion { get; set; } = "";

            /// <summary>
            /// The index of the motion entry to use as a follow up when this one ends.
            /// </summary>
            public uint FollowUpAnimationIndex { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Something to do with frames? Loop start frame?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Something to do with frames? Loop end frame?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_1 { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_2 { get; set; }

            /// <summary>
            /// A value to multiple the framerate specified in this motion's GNM file by.
            /// </summary>
            public float FramerateMultiplier { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            public override string ToString() => AssociatedNinjaMotion;
        }

        // Actual data presented to the end user.
        public List<MotionEntry> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's size.
            uint fileSize = reader.ReadUInt32();

            // Read the motion count for this file.
            uint motionCount = reader.ReadUInt32();

            // Read the offset to this file's motion table.
            uint motionTableOffset = reader.ReadUInt32();

            // Read the offset to this file's string table.
            uint stringTableOffset = reader.ReadUInt32();

            // Jump to the motion table.
            reader.JumpTo(motionTableOffset);

            // Loop through each motion.
            for (int motionIndex = 0; motionIndex < motionCount; motionIndex++)
            {
                // Set up a new motion entry.
                MotionEntry motion = new();

                // Read this motion's internal index.
                motion.AnimationIndex = reader.ReadUInt32();

                // Read the offset into the name table for this motion.
                uint nameOffset = reader.ReadUInt32();

                // Save our position so we can jump back after reading the motion name.
                long position = reader.BaseStream.Position;

                // Jump to this motion's entry in the string table.
                reader.JumpTo(stringTableOffset + nameOffset);

                // Read this motion's Ninja file reference.
                motion.AssociatedNinjaMotion = reader.ReadNullTerminatedString();

                // Jump back for the rest of the motion.
                reader.JumpTo(position);

                // Skip an unknown value of 0xFFFFFFFF.
                reader.JumpAhead(0x04);

                // Read this motion's follow up animation index.
                motion.FollowUpAnimationIndex = reader.ReadUInt32();

                // Read this motion's first unknown floating point value.
                motion.UnknownFloat_1 = reader.ReadSingle();

                // Read this motion's second unknown floating point value.
                motion.UnknownFloat_2 = reader.ReadSingle();

                // Read this motion's first unknown short value.
                motion.UnknownUShort_1 = reader.ReadUInt16();

                // Read this motion's second unknown short value.
                motion.UnknownUShort_2 = reader.ReadUInt16();

                // Read this motion's framerate multiplier.
                motion.FramerateMultiplier = reader.ReadSingle();

                // Read this motion's third unknown floating point value.
                motion.UnknownFloat_3 = reader.ReadSingle();

                // Save this motion entry.
                Data.Add(motion);
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
            // Set up a value to store the string table's current length.
            int totalStringLength = 0;

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write this file's motion count.
            writer.Write(Data.Count);

            // Add an offset for this file's motion table.
            writer.AddOffset("MotionTable");

            // Add an offset for this file's string table.
            writer.AddOffset("StringTable");

            // Fill in the offset for this file's motion table.
            writer.FillOffset("MotionTable");

            // Loop through each motion entry.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this motion's internal animation index.
                writer.Write(Data[dataIndex].AnimationIndex);

                // Write the current value of totalStringLength.
                writer.Write(totalStringLength);

                // Add the length of this motion's Ninja file name (including the null terminator) to the totalStringLength.
                totalStringLength += Data[dataIndex].AssociatedNinjaMotion.Length + 1;

                // Write an unknown value of 0xFFFFFFFF.
                writer.Write(0xFFFFFFFF);

                // Write this motion's follow up animation index.
                writer.Write(Data[dataIndex].FollowUpAnimationIndex);

                // Write this motion's first unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_1);

                // Write this motion's second unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_2);

                // Write this motion's first unknown short value.
                writer.Write(Data[dataIndex].UnknownUShort_1);

                // Write this motion's second unknown short value.
                writer.Write(Data[dataIndex].UnknownUShort_2);

                // Write this motion's framerate modifier.
                writer.Write(Data[dataIndex].FramerateMultiplier);

                // Write this motion's third unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_3);
            }

            // Fill in the offset for this file's string table.
            writer.FillOffset("StringTable");

            // Loop through and write each motion entry's Ninja file name.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
                writer.WriteNullTerminatedString(Data[dataIndex].AssociatedNinjaMotion);

            // Jump back to the start of the file.
            writer.BaseStream.Position = 0x00;

            // Write this file's size.
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
