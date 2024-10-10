namespace KnuxLib.Engines.SonicStorybook
{
    // TODO: Figure out the unknown values.
    public class MotionTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MotionTable() { }
        public MotionTable(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".sonicstorybook.motion.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Motion[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.bin");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class Motion
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
            /// The index of the motion to use as a follow up when this one ends.
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

            /// <summary>
            /// Displays this motion's index and ninja file in the debugger.
            /// </summary>
            public override string ToString() => $"Index {AnimationIndex}: {AssociatedNinjaMotion}";

            /// <summary>
            /// Initialises this motion with default data.
            /// </summary>
            public Motion() { }

            /// <summary>
            /// Initialises this motion with the provided data.
            /// </summary>
            public Motion(uint animationIndex, string associatedNinjaMotion, uint followUpAnimationIndex, float unknownFloat_1, float unknownFloat_2, ushort unknownUShort_1, ushort unknownUShort_2, float framerateMultiplier, float unknownFloat_3)
            {
                AnimationIndex = animationIndex;
                AssociatedNinjaMotion = associatedNinjaMotion;
                FollowUpAnimationIndex = followUpAnimationIndex;
                UnknownFloat_1 = unknownFloat_1;
                UnknownFloat_2 = unknownFloat_2;
                UnknownUShort_1 = unknownUShort_1;
                UnknownUShort_2 = unknownUShort_2;
                FramerateMultiplier = framerateMultiplier;
                UnknownFloat_3 = unknownFloat_3;
            }

            /// <summary>
            /// Initialises this motion by reading its data from a BinaryReader.
            /// </summary>
            public Motion(ExtendedBinaryReader reader, uint stringTableOffset) => Read(reader, stringTableOffset);

            /// <summary>
            /// Reads the data for this motion.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, uint stringTableOffset)
            {
                // Read this motion's internal index.
                AnimationIndex = reader.ReadUInt32();

                // Read the offset into the name table for this motion.
                uint nameOffset = reader.ReadUInt32();

                // Save our position so we can jump back after reading the motion name.
                long position = reader.BaseStream.Position;

                // Jump to this motion's entry in the string table.
                reader.JumpTo(stringTableOffset + nameOffset);

                // Read this motion's Ninja file reference.
                AssociatedNinjaMotion = reader.ReadNullTerminatedString();

                // Jump back for the rest of the motion.
                reader.JumpTo(position);

                // Skip an unknown value of 0xFFFFFFFF.
                reader.CheckValue(-1);

                // Read this motion's follow up animation index.
                FollowUpAnimationIndex = reader.ReadUInt32();

                // Read this motion's first unknown floating point value.
                UnknownFloat_1 = reader.ReadSingle();

                // Read this motion's second unknown floating point value.
                UnknownFloat_2 = reader.ReadSingle();

                // Read this motion's first unknown short value.
                UnknownUShort_1 = reader.ReadUInt16();

                // Read this motion's second unknown short value.
                UnknownUShort_2 = reader.ReadUInt16();

                // Read this motion's framerate multiplier.
                FramerateMultiplier = reader.ReadSingle();

                // Read this motion's third unknown floating point value.
                UnknownFloat_3 = reader.ReadSingle();
            }

            /// <summary>
            /// Writes the data for this motion.
            /// </summary>
            public int Write(ExtendedBinaryWriter writer, int totalStringLength)
            {
                // Write this motion's internal animation index.
                writer.Write(AnimationIndex);

                // Write the current value of totalStringLength.
                writer.Write(totalStringLength);

                // Add the length of this motion's Ninja file name (including the null terminator) to the totalStringLength.
                totalStringLength += AssociatedNinjaMotion.Length + 1;

                // Write an unknown value of 0xFFFFFFFF.
                writer.Write(0xFFFFFFFF);

                // Write this motion's follow up animation index.
                writer.Write(FollowUpAnimationIndex);

                // Write this motion's first unknown floating point value.
                writer.Write(UnknownFloat_1);

                // Write this motion's second unknown floating point value.
                writer.Write(UnknownFloat_2);

                // Write this motion's first unknown short value.
                writer.Write(UnknownUShort_1);

                // Write this motion's second unknown short value.
                writer.Write(UnknownUShort_2);

                // Write this motion's framerate modifier.
                writer.Write(FramerateMultiplier);

                // Write this motion's third unknown floating point value.
                writer.Write(UnknownFloat_3);

                // Return the updated string length.
                return totalStringLength;
            }
        }

        // Actual data presented to the end user.
        public Motion[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Read this file's size.
            uint fileSize = reader.ReadUInt32();

            // Read the motion count for this file.
            Data = new Motion[reader.ReadUInt32()];

            // Read the offset to this file's motion table.
            uint motionTableOffset = reader.ReadUInt32();

            // Read the offset to this file's string table.
            uint stringTableOffset = reader.ReadUInt32();

            // Jump to the motion table.
            reader.JumpTo(motionTableOffset);

            // Loop through and read each motion.
            for (int motionIndex = 0; motionIndex < Data.Length; motionIndex++)
                Data[motionIndex] = new(reader, stringTableOffset);

            // Close our BinaryReader.
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

            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write this file's motion count.
            writer.Write(Data.Length);

            // Add an offset for this file's motion table.
            writer.AddOffset("MotionTable");

            // Add an offset for this file's string table.
            writer.AddOffset("StringTable");

            // Fill in the offset for this file's motion table.
            writer.FillInOffset("MotionTable");

            // Loop through and write each motion entry.
            for (int motionIndex = 0; motionIndex < Data.Length; motionIndex++)
                totalStringLength = Data[motionIndex].Write(writer, totalStringLength);

            // Fill in the offset for this file's string table.
            writer.FillInOffset("StringTable");

            // Loop through and write each motion entry's Ninja file name.
            for (int motionIndex = 0; motionIndex < Data.Length; motionIndex++)
                writer.WriteNullTerminatedString(Data[motionIndex].AssociatedNinjaMotion);

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
