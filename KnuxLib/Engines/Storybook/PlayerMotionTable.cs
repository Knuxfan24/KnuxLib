namespace KnuxLib.Engines.Storybook
{
    public class PlayerMotion : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PlayerMotion() { }
        public PlayerMotion(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.playermotion.json", Data);
        }

        // Classes for this format.
        public class MotionEntry
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// This stage's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_4 { get; set; }

            public override string ToString() => Name;
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
            for (int i = 0; i < motionCount; i++)
            {
                // Set up a new motion entry.
                MotionEntry motion = new();

                // Read this motion's first unknown integer value.
                motion.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this motion's name.
                motion.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, false, stringTableOffset);

                // Skip an unknown value of 0xFFFFFFFF.
                reader.JumpAhead(0x04);

                // Read this motion's second unknown integer value.
                motion.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this motion's first unknown floating point value.
                motion.UnknownFloat_1 = reader.ReadSingle();

                // Read this motion's second unknown floating point value.
                motion.UnknownFloat_2 = reader.ReadSingle();

                // Read this motion's third unknown integer value.
                motion.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this motion's third unknown floating point value.
                motion.UnknownFloat_3 = reader.ReadSingle();

                // Read this motion's fourth unknown floating point value.
                motion.UnknownFloat_4 = reader.ReadSingle();

                // Save this motion entry.
                Data.Add(motion);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
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
            for (int i = 0; i < Data.Count; i++)
            {
                // Write this motion's first unknown integer value.
                writer.Write(Data[i].UnknownUInt32_1);

                // Write the current value of totalStringLength.
                writer.Write(totalStringLength);

                // Add the length of this motion's name (including the null terminator) to the totalStringLength.
                totalStringLength += (Data[i].Name.Length + 1);

                // Write an unknown value of 0xFFFFFFFF.
                writer.Write(0xFFFFFFFF);

                // Write this motion's second unknown integer value.
                writer.Write(Data[i].UnknownUInt32_2);

                // Write this motion's first unknown floating point value.
                writer.Write(Data[i].UnknownFloat_1);

                // Write this motion's second unknown floating point value.
                writer.Write(Data[i].UnknownFloat_2);

                // Write this motion's third unknown integer value.
                writer.Write(Data[i].UnknownUInt32_3);

                // Write this motion's third unknown floating point value.
                writer.Write(Data[i].UnknownFloat_3);

                // Write this motion's fourth unknown floating point value.
                writer.Write(Data[i].UnknownFloat_4);
            }

            // Fill in the offset for this file's string table.
            writer.FillOffset("StringTable");

            // Loop through and write each motion entry's name.
            for (int i = 0; i < Data.Count; i++)
                writer.WriteNullTerminatedString(Data[i].Name);

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
