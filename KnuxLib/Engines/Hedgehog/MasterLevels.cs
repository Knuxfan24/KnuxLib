namespace KnuxLib.Engines.Hedgehog
{
    public class MasterLevels : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MasterLevels() { }
        public MasterLevels(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.masterlevels.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Level[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.mlevel");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class Level
        {
            /// <summary>
            /// The name of this level entry.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An array of file names used by this level entry.
            /// </summary>
            public string[]? Files { get; set; }

            /// <summary>
            /// Entries that this level entry depends on.
            /// </summary>
            public string[]? Dependencies { get; set; }

            /// <summary>
            /// An unknown boolean value.
            /// TODO: What is this?
            /// </summary>
            public bool UnknownBoolean_1 { get; set; }

            /// <summary>
            /// Displays this level's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this level with default data.
            /// </summary>
            public Level() { }

            /// <summary>
            /// Initialises this level with the provided data.
            /// </summary>
            public Level(string name, string[]? files, string[]? dependencies, bool unknownBoolean_1)
            {
                Name = name;
                Files = files;
                Dependencies = dependencies;
                UnknownBoolean_1 = unknownBoolean_1;
            }

            /// <summary>
            /// Initialises this level by reading its data from a BINAReader.
            /// </summary>
            public Level(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this level.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read the offset to this level's data.
                long levelOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next level.
                long position = reader.BaseStream.Position;

                // Jump to this level's data.
                reader.JumpTo(levelOffset, false);

                // Read this level's name.
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                // Read this level's file count.
                uint fileCount = reader.ReadUInt32();

                // Read this level's dependency count.
                uint dependencyCount = reader.ReadUInt32();

                // Read this level's file offset.
                ulong filesOffset = reader.ReadUInt64();

                // Read this level's dependency offset.
                ulong dependenciesOffset = reader.ReadUInt64();

                // Read this level's unknown boolean.
                UnknownBoolean_1 = reader.ReadBoolean();

                // Realign to 0x08 bytes.
                reader.FixPadding(0x08);

                // Read the boolean that says whether this level has files.
                bool hasFiles = reader.ReadBoolean();

                // If this level has dependencies, then handle them.
                if (dependencyCount != 0)
                {
                    // Define the array for this level's dependencies.
                    Dependencies = new string[dependencyCount];

                    // Jump to this level's other offset table.
                    reader.JumpTo(dependenciesOffset, false);

                    // Loop through this level's dependencies.
                    for (int dependencyIndex = 0; dependencyIndex < dependencyCount; dependencyIndex++)
                    {
                        // Read this dependency's offset.
                        ulong dependencyOffset = reader.ReadUInt64();

                        // Save our current position so we can jump back for the next dependency.
                        long dependencyPosition = reader.BaseStream.Position;

                        // Jump to this dependency's offset.
                        reader.JumpTo(dependencyOffset, false);

                        // Read this dependency.
                        Dependencies[dependencyIndex] = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                        // Skip an unknown value of 0.
                        reader.JumpAhead(0x08);

                        // Jump back for the next dependency.
                        reader.JumpTo(dependencyPosition);
                    }
                }

                // If this level has files, then handle them.
                if (fileCount != 0)
                {
                    // Define the array for this level's files.
                    Files = new string[fileCount];

                    // Jump to this level's file offset table.
                    reader.JumpTo(filesOffset, false);

                    // Loop through and read each file.
                    for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
                    {
                        // Read this file's offset.
                        ulong fileOffset = reader.ReadUInt64();

                        // Save our current position so we can jump back for the next file.
                        long filePosition = reader.BaseStream.Position;

                        // Jump to this file's offset.
                        reader.JumpTo(fileOffset, false);

                        // Read the name of this file.
                        Files[fileIndex] = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                        // Skip an unknown offset that always points to a single null character in the string table.
                        reader.JumpAhead(0x08);

                        // Skip an unknown value of 0.
                        reader.JumpAhead(0x08);

                        // Jump back for the next file.
                        reader.JumpTo(filePosition);
                    }
                }

                // Jump back for the next level.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the base data for this level.
            /// </summary>
            public void WriteBase(BINAWriter writer, int index)
            {
                // Write eight null bytes.
                writer.WriteNulls(0x08);

                // Fill in the offset for this level index.
                writer.FillInOffset($"level_{index}", false);

                // Add a string for this level's name.
                writer.AddString($"level_{index}_name", Name, 0x08);

                // Write the amount of files this level has.
                if (Files != null)
                    writer.Write(Files.Length);
                else
                    writer.WriteNulls(0x04);

                // Write the amount of dependencies this level has.
                if (Dependencies != null)
                    writer.Write(Dependencies.Length);
                else
                    writer.WriteNulls(0x04);

                // Add an offset for this level's files.
                writer.AddOffset($"level_{index}_files", 0x08);

                // Add an offset for this level's dependencies.
                writer.AddOffset($"level_{index}_dependencies", 0x08);

                // Write this level's unknown boolean.
                writer.Write(UnknownBoolean_1);

                // Write the HasFiles boolean.
                if (Files != null)
                    writer.Write(true);
                else
                    writer.Write(false);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            /// <summary>
            /// Writes the dependency data for this level.
            /// </summary>
            public void WriteDependencies(BINAWriter writer, int index)
            {
                // Fill in the offset for this level's dependencies.
                writer.FillInOffset($"level_{index}_dependencies", false);

                // Write this level's dependencies if it has any.
                if (Dependencies != null)
                {
                    // Add an offset table for this level's dependencies.
                    writer.AddOffsetTable($"level_{index}_dependencies", (uint)Dependencies.Length, 0x08);

                    // Loop through each dependency in this level.
                    for (int dependencyIndex = 0; dependencyIndex < Dependencies.Length; dependencyIndex++)
                    {
                        // Fill in this dependency's offset.
                        writer.FillInOffset($"level_{index}_dependencies_{dependencyIndex}", false);

                        // Write this dependency's name.
                        writer.AddString($"level_{index}_dependencies_{dependencyIndex}_value", Dependencies[dependencyIndex], 0x08);

                        // Write an unknown value of 0.
                        writer.WriteNulls(0x08);
                    }
                }

                // Fill in the offset for this level's files.
                writer.FillInOffset($"level_{index}_files", false);

                // Write this level's files if it has any.
                if (Files != null)
                {
                    // Add an offset table for this level's files.
                    writer.AddOffsetTable($"level_{index}_files", (uint)Files.Length, 0x08);

                    // Loop through each file in this level.
                    for (int fileIndex = 0; fileIndex < Files.Length; fileIndex++)
                    {
                        // Fill in this file's offset.
                        writer.FillInOffset($"level_{index}_files_{fileIndex}", false);

                        // Write this file's name.
                        writer.AddString($"level_{index}_files_{fileIndex}_value", Files[fileIndex], 0x08);

                        // Write an empty string.
                        writer.AddString($"level_{index}_files_{fileIndex}_pad", "", 0x08, true);

                        // Write an unknown value of 0.
                        writer.WriteNulls(0x08);
                    }
                }
            }
        }

        // Actual data presented to the end user.
        public Level[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "LMEH");

            // Realign to 0x08 bytes.
            reader.FixPadding(0x08);
            
            // Initialise the data array.
            Data = new Level[reader.ReadInt64()];

            // Read the offset to this file's level table table.
            ulong levelOffsetTable = reader.ReadUInt64();

            // Skip an unknown value that is always 0. Likely padding?
            reader.JumpAhead(0x08);

            // Jump to the main offset table (should already be here but lets play it safe).
            reader.JumpTo(levelOffsetTable, false);

            // Loop through and read each level entry.
            for (int levelIndex = 0; levelIndex < Data.Length; levelIndex++)
                Data[levelIndex] = new(reader);

            // Close our BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(210);

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Write this file's signature.
            writer.Write("LMEH");

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);

            // Write how many levels are in this file.
            writer.Write((ulong)Data.Length);

            // Add an offset for the level table.
            writer.AddOffset("levelTableOffset", 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Fill in the offset for the level table.
            writer.FillInOffset("levelTableOffset");

            // Add an offset table for each level entry.
            writer.AddOffsetTable("level", (uint)Data.Length, 0x08);

            // Loop through and write each level's base entry.
            for (int levelIndex = 0; levelIndex < Data.Length; levelIndex++)
                Data[levelIndex].WriteBase(writer, levelIndex);

            // Write eight null bytes.
            writer.WriteNulls(0x08);

            // Loop through and write each level's dependencies.
            for (int levelIndex = 0; levelIndex < Data.Length; levelIndex++)
                Data[levelIndex].WriteDependencies(writer, levelIndex);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
