namespace KnuxLib.Engines.Hedgehog
{
    public class MasterLevels : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MasterLevels() { }
        public MasterLevels(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.masterlevels.json", Data);
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

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<Level> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "LMEH";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Realign to 0x08 bytes.
            reader.FixPadding(0x08);

            // Read the level count in this file.
            ulong levelCount = reader.ReadUInt64();

            // Read this master level's main offset table (should always be 0x20).
            long levelOffsetTable = reader.ReadInt64();

            // Jump to the main offset table (should already be here but lets play it safe).
            reader.JumpTo(levelOffsetTable, false);

            // Loop through each level entry.
            for (ulong levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                // Set up a new level entry.
                Level level = new();

                // Read the offset to this level's data.
                long levelOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next level.
                long position = reader.BaseStream.Position;

                // Jump to this level's data.
                reader.JumpTo(levelOffset, false);

                // Read this level's name.
                level.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this level's file count.
                uint fileCount = reader.ReadUInt32();

                // Read this level's dependency count.
                uint dependencyCount = reader.ReadUInt32();

                // Read this level's file offset.
                long filesOffset = reader.ReadInt64();

                // Read this level's dependency offset.
                long dependenciesOffset = reader.ReadInt64();

                // Read this level's unknown boolean.
                level.UnknownBoolean_1 = reader.ReadBoolean();

                // Realign to 0x08 bytes.
                reader.FixPadding(0x08);

                // Read the boolean that says whether this level has files.
                bool hasFiles = reader.ReadBoolean();

                // If this level has dependencies, then handle them.
                if (dependencyCount != 0)
                {
                    // Define the array for this level's dependencies.
                    level.Dependencies = new string[dependencyCount];

                    // Jump to this level's other offset table.
                    reader.JumpTo(dependenciesOffset, false);

                    // Loop through this level's dependencies.
                    for (int dependencyIndex = 0; dependencyIndex < dependencyCount; dependencyIndex++)
                    {
                        // Read this dependency's offset.
                        long dependencyOffset = reader.ReadInt64();

                        // Save our current position so we can jump back for the next dependency.
                        long dependencyPosition = reader.BaseStream.Position;

                        // Jump to this dependency's offset.
                        reader.JumpTo(dependencyOffset, false);

                        // Read this dependency.
                        level.Dependencies[dependencyIndex] = Helpers.ReadNullTerminatedStringTableEntry(reader);

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
                    level.Files = new string[fileCount];

                    // Jump to this level's file offset table.
                    reader.JumpTo(filesOffset, false);

                    // Loop through and read each file.
                    for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
                    {
                        // Read this file's offset.
                        long fileOffset = reader.ReadInt64();

                        // Save our current position so we can jump back for the next file.
                        long filePosition = reader.BaseStream.Position;

                        // Jump to this file's offset.
                        reader.JumpTo(fileOffset, false);

                        // Read the name of this file.
                        level.Files[fileIndex] = Helpers.ReadNullTerminatedStringTableEntry(reader);

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

                // Save this level.
                Data.Add(level);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this file's signature.
            writer.WriteSignature(Signature);

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);

            // Write this file's level count.
            writer.Write((ulong)Data.Count);

            // Add an offset to the level table.
            writer.AddOffset("LevelOffsetTable", 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Fill in the offset for the level table.
            writer.FillInOffset("LevelOffsetTable", false);

            // Add an offset table for each level entry.
            writer.AddOffsetTable("level", (uint)Data.Count, 0x08);

            // Loop through each level entry.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write eight null bytes.
                writer.WriteNulls(0x08);

                // Fill in the offset for this level index.
                writer.FillInOffset($"level_{dataIndex}", false);

                // Add a string for this level's name.
                writer.AddString($"level_{dataIndex}_name", Data[dataIndex].Name, 0x08);

                // Write the amount of files this level has.
                if (Data[dataIndex].Files != null)
                    writer.Write(Data[dataIndex].Files.Length);
                else
                    writer.WriteNulls(0x04);

                // Write the amount of dependencies this level has.
                if (Data[dataIndex].Dependencies != null)
                    writer.Write(Data[dataIndex].Dependencies.Length);
                else
                    writer.WriteNulls(0x04);

                // Add an offset for this level's files.
                writer.AddOffset($"level_{dataIndex}_files", 0x08);

                // Add an offset for this level's dependencies.
                writer.AddOffset($"level_{dataIndex}_dependencies", 0x08);

                // Write this level's unknown boolean.
                writer.Write(Data[dataIndex].UnknownBoolean_1);

                // Write the HasFiles boolean.
                if (Data[dataIndex].Files != null)
                    writer.Write(true);
                else
                    writer.Write(false);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Write eight null bytes.
            writer.WriteNulls(0x08);

            // Loop through each level.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in the offset for this level's dependencies.
                writer.FillInOffset($"level_{dataIndex}_dependencies", false);

                // Write this level's dependencies if it has any.
                if (Data[dataIndex].Dependencies != null)
                {
                    // Add an offset table for this level's dependencies.
                    writer.AddOffsetTable($"level_{dataIndex}_dependencies", (uint)Data[dataIndex].Dependencies.Length, 0x08);

                    // Loop through each dependency in this level.
                    for (int dependencyIndex = 0; dependencyIndex < Data[dataIndex].Dependencies.Length; dependencyIndex++)
                    {
                        // Fill in this dependency's offset.
                        writer.FillInOffset($"level_{dataIndex}_dependencies_{dependencyIndex}", false);

                        // Write this dependency's name.
                        writer.AddString($"level_{dataIndex}_dependencies_{dependencyIndex}_value", Data[dataIndex].Dependencies[dependencyIndex], 0x08);

                        // Write an unknown value of 0.
                        writer.WriteNulls(0x08);
                    }
                }

                // Fill in the offset for this level's files.
                writer.FillInOffset($"level_{dataIndex}_files", false);

                // Write this level's files if it has any.
                if (Data[dataIndex].Files != null)
                {
                    // Add an offset table for this level's files.
                    writer.AddOffsetTable($"level_{dataIndex}_files", (uint)Data[dataIndex].Files.Length, 0x08);

                    // Loop through each file in this level.
                    for (int fileIndex = 0; fileIndex < Data[dataIndex].Files.Length; fileIndex++)
                    {
                        // Fill in this file's offset.
                        writer.FillInOffset($"level_{dataIndex}_files_{fileIndex}", false);

                        // Write this file's name.
                        writer.AddString($"level_{dataIndex}_files_{fileIndex}_value", Data[dataIndex].Files[fileIndex], 0x08);

                        // Write an empty string. This doesn't work right with HedgeLib# in its normal state, although the game doesn't seem to care.
                        // Lines 220-224 in BINA.cs need commenting out to make this accurate.
                        writer.AddString($"level_{dataIndex}_files_{fileIndex}_pad", "", 0x08);

                        // Write an unknown value of 0.
                        writer.WriteNulls(0x08);
                    }
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
