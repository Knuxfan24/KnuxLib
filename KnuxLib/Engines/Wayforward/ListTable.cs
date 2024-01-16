namespace KnuxLib.Engines.Wayforward
{
    // TODO: If the game accepts it, maybe refactor this to remove the seperate tag and string name entries? As maintaining that in a JSON will be ugly.
    // TODO: Tidy up? Not the biggest fan of how some of this works.
    public class ListTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public ListTable() { }
        public ListTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.listtable.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The entries in this file.
            /// </summary>
            public EntryData[] Entries { get; set; } = Array.Empty<EntryData>();

            /// <summary>
            /// The names of the entries in this file.
            /// </summary>
            public List<StringName>[] Names { get; set; } = Array.Empty<List<StringName>>();

            /// <summary>
            /// The tags for the data in the enteries in this file.
            /// </summary>
            public List<Tag>[] Tags { get; set; } = Array.Empty<List<Tag>>();
        }

        public class EntryData
        {
            /// <summary>
            /// The strings that make up this entry, consisting of a tag name and the value.
            /// </summary>
            public Dictionary<string, string?> Keys { get; set; } = new();

            /// <summary>
            /// The name of this entry. Not actually stored with this data, but copied here for ease of reading in a JSON.
            /// </summary>
            public string StringName { get; set; } = "";

            public override string ToString() => StringName;
        }

        public class StringName
        {
            /// <summary>
            /// The index of the entry this name is for.
            /// </summary>
            public ulong Index { get; set; }

            /// <summary>
            /// The name of this... Name.
            /// </summary>
            public string Name { get; set; } = "";

            public override string ToString() => Name;
        }

        public class Tag
        {
            /// <summary>
            /// The index of this tag.
            /// </summary>
            public ulong Index { get; set; }

            /// <summary>
            /// The name of this tag.
            /// </summary>
            public string Name { get; set; } = "";

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), Encoding.UTF8);

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the count of string datas in this file. 
            uint stringDataCount = reader.ReadUInt32();

            // Read the amount of string keys in this file.
            ulong keysCount = reader.ReadUInt64();

            // Skip an unknown value of 0x40 (likely an offset that's always in the same place).
            reader.JumpAhead(0x08);

            // Read the string name count in this file.
            ulong stringNamesCount = reader.ReadUInt64();

            // Read the offset for the string name table.
            long stringNamesOffset = reader.ReadInt64();

            // Read this file's tag count.
            ulong tagCount = reader.ReadUInt64();

            // Read the offset to this file's tags.
            long tagOffset = reader.ReadInt64();

            // Jump to the offset for this file's tags.
            reader.JumpTo(tagOffset);

            // Define the array of tags in this file.
            Data.Tags = new List<Tag>[tagCount];

            // Define the array of string names used in this file.
            Data.Names = new List<StringName>[stringNamesCount];

            // Set up a list of the tags for later usage.
            Dictionary<ulong, string> tags = new();

            // Set up a value to hold the largest tag's index.
            ulong largestTagIndex = 0;

            // Loop through and read all the tags.
            for (ulong tagIndex = 0; tagIndex < tagCount; tagIndex++)
            {
                // Read this tag's data offset.
                long tagIndexOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next tag.
                long position = reader.BaseStream.Position;

                // Jump to this tag's data.
                reader.JumpTo(tagIndexOffset);

                // Read this tag collection's tag count.
                ulong tagIndexCount = reader.ReadUInt64();

                // Skip an unknown value of 0x10.
                reader.JumpAhead(0x08);

                // Set up a new tag collection for this tag's linear index.
                Data.Tags[tagIndex] = new();

                // Read the tags if there actually is any.
                if (tagIndexCount != 0)
                {
                    // Skip a table of values that point to each tag's strings in an additive form.
                    reader.JumpAhead(0x08 * (long)tagIndexCount);

                    // Loop through and read this collection's tags.
                    for (ulong tagIndexIndex = 0; tagIndexIndex < tagIndexCount; tagIndexIndex++)
                    {
                        // Define a new tag entry.
                        Tag tag = new();

                        // Read this tag's index.
                        tag.Index = reader.ReadUInt64();

                        // Skip an unknown value of 0x10.
                        reader.JumpAhead(0x08);

                        // Read this tag's name.
                        tag.Name = reader.ReadNullTerminatedString();

                        // Save this tag.
                        Data.Tags[tagIndex].Add(tag);

                        // Add this tag to the dictonary.
                        tags.Add(tag.Index, tag.Name);

                        // Update the largest tag index if we need to.
                        if (tag.Index > largestTagIndex)
                            largestTagIndex = tag.Index;
                    }

                }

                // Jump back for the next tag.
                reader.JumpTo(position);
            }

            // Jump to the offset of data near the start of the file.
            reader.JumpTo(0x40);

            // Define the string data in this file.
            Data.Entries = new EntryData[stringDataCount];

            // Definie all the entries in the string data array.
            for (int stringDataIndex = 0; stringDataIndex < stringDataCount; stringDataIndex++)
                Data.Entries[stringDataIndex] = new();

            // Loop through each tag.
            for (ulong tagIndex = 0; tagIndex <= largestTagIndex; tagIndex++)
            {
                // Loop through each string data entry.
                for (int stringDataIndex = 0; stringDataIndex < stringDataCount; stringDataIndex++)
                {
                    // Read the null terminated string at the listed offset.
                    string? data = Helpers.ReadNullTerminatedStringTableEntry(reader, false, 0, true);

                    // If the returned string is empty, then null it out.
                    if (data == "")
                        data = null;

                    // Add the read string to the string data alongside its tag.
                    Data.Entries[stringDataIndex].Keys.Add(tags[tagIndex], data);
                }
            }

            // Jump to the offset of the string names table.
            reader.JumpTo(stringNamesOffset);

            // Loop through each string name.
            for (ulong stringNamesIndex = 0; stringNamesIndex < stringNamesCount; stringNamesIndex++)
            {
                // Read this name's data offset.
                long nameOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next name.
                long position = reader.BaseStream.Position;

                // Jump to this name's data.
                reader.JumpTo(nameOffset);

                // Read the count of entries in this name.
                ulong entryCount = reader.ReadUInt64();

                // Skip an unknown value of 0x10.
                reader.JumpAhead(0x08);

                // Set up a new name collection for this name's linear index.
                Data.Names[stringNamesIndex] = new();

                // Read the names if there actually are any.
                if (entryCount != 0)
                {
                    // Skip a table of values that point to each name's strings in an additive form.
                    reader.JumpAhead(0x08 * (long)entryCount);

                    // Loop through and read this collection's names.
                    for (ulong entryIndex = 0; entryIndex < entryCount; entryIndex++)
                    {
                        // Define a new name entry.
                        StringName name = new();

                        // Read the index of the entry this name is for.
                        name.Index = reader.ReadUInt64();

                        // Skip an unknown value of 0x10.
                        reader.JumpAhead(0x08);

                        // Read this name's... Name.
                        name.Name = reader.ReadNullTerminatedString();

                        // Set the string name in the corrosponding entry to this name's index for ease of viewing.
                        Data.Entries[name.Index].StringName = name.Name;

                        // Save this name.
                        Data.Names[stringNamesIndex].Add(name);
                    }
                }

                // Jump back for the next tag.
                reader.JumpTo(position);
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
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), Encoding.UTF8);

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write the amount of entries in this file.
            writer.Write(Data.Entries.Length);

            // Calculate the length of the entry keys. If there isn't any, then write a 0 instead.
            if (Data.Entries.Length > 0)
                writer.Write((long)(Data.Entries.Length * Data.Entries[0].Keys.Count));
            else
                writer.Write(0L);

            // Write a value that is always 0x40. This is likely an offset, but as there's no table like a BINA Format, I don't need to worry about it.
            writer.Write(0x40L);

            // Write the count of name entries.
            writer.Write((long)Data.Names.Length);

            // Add an offset to the string name table.
            writer.AddOffset("StringNames", 0x08);

            // Write the count of tags.
            writer.Write((long)Data.Tags.Length);

            // Add an offset to the tag table.
            writer.AddOffset("Tags", 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // If we have any data entries, then write them.
            if (Data.Entries.Length > 0)
            {
                // Loop through each tag and each entry and add an offset for each one.
                for (int keyIndex = 0; keyIndex < Data.Entries[0].Keys.Count; keyIndex++)
                    for (int entryIndex = 0; entryIndex < Data.Entries.Length; entryIndex++)
                        writer.AddOffset($"String{entryIndex}Key{keyIndex}", 0x08);

                // Realign to 0x40 bytes.
                writer.FixPadding(0x40);

                // Loop through each tag and each entry to write their data.
                for (int keyIndex = 0; keyIndex < Data.Entries[0].Keys.Count; keyIndex++)
                {
                    for (int entryIndex = 0; entryIndex < Data.Entries.Length; entryIndex++)
                    {
                        // Fill in the offset for this string and this tag.
                        writer.FillOffset($"String{entryIndex}Key{keyIndex}");

                        // Get the value of the tag.
                        string? keyValue = Data.Entries[entryIndex].Keys.ElementAt(keyIndex).Value;

                        // If the value is null, then replace it with an empty string.
                        keyValue ??= "";

                        // Write the value of the tag.
                        writer.WriteNullTerminatedString(keyValue);

                        // Realign to 0x08 bytes.
                        writer.FixPadding(0x08);
                    }
                }

                // Realign to 0x40 bytes.
                writer.FixPadding(0x40);
            }

            // Fill in the offset for the string name table.
            writer.FillOffset("StringNames");

            // Loop through and add an offset for each name entry.
            for (int nameIndex = 0; nameIndex < Data.Names.Length; nameIndex++)
                writer.AddOffset($"StringName{nameIndex}", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write each name entry.
            for (int nameIndex = 0; nameIndex < Data.Names.Length; nameIndex++)
            {
                // Fill in this name entry's offset.
                writer.FillOffset($"StringName{nameIndex}");

                // Write the count of names in this entry.
                writer.Write((long)Data.Names[nameIndex].Count);

                // Write an unknown value that is always 0x10.
                writer.Write(0x10L);

                // Set up a dictonary of string offsets.
                Dictionary<long, int> stringOffsets = new();

                // Loop through each name in this entry for the string offset table.
                for (int nameIndexIndex = 0; nameIndexIndex < Data.Names[nameIndex].Count; nameIndexIndex++)
                {
                    // Add an entry to the dictonary consisting of our current position and the name's linear index.
                    stringOffsets.Add(writer.BaseStream.Position, nameIndexIndex);

                    // Write a placeholder to fill in later.
                    writer.Write("==TEMP==");
                }

                // Loop through each name in this entry for writing.
                for (int nameIndexIndex = 0; nameIndexIndex < Data.Names[nameIndex].Count; nameIndexIndex++)
                {
                    // Write this name's index.
                    writer.Write((long)Data.Names[nameIndex][nameIndexIndex].Index);

                    // Write an unknown value that is always 0x10.
                    writer.Write(0x10L);

                    // Save our position so we can jump back after calculating the string offset.
                    long position = writer.BaseStream.Position;

                    // Jump to the stored position for this name.
                    writer.BaseStream.Position = stringOffsets.ElementAt(nameIndexIndex).Key;

                    // Fill in the placeholder, calculating the gap between the start of the table and where this name's string should be.
                    writer.Write(position - (writer.BaseStream.Position - (0x08 * nameIndexIndex)));

                    // Jump back to our saved position.
                    writer.BaseStream.Position = position;

                    // Write this entry's name.
                    writer.WriteNullTerminatedString(Data.Names[nameIndex][nameIndexIndex].Name);
                }

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Fill in the offset for this file's tags.
            writer.FillOffset("Tags");

            // Loop through and add an offset for each tag entry.
            for (int tagIndex = 0; tagIndex < Data.Tags.Length; tagIndex++)
                writer.AddOffset($"Tag{tagIndex}", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write each tag entry.
            for (int tagIndex = 0; tagIndex < Data.Tags.Length; tagIndex++)
            {
                // Fill in this tag entry's offset.
                writer.FillOffset($"Tag{tagIndex}");

                // Write the count of tags in this entry.
                writer.Write((long)Data.Tags[tagIndex].Count);

                // Write an unknown value that is always 0x10.
                writer.Write(0x10L);

                // Set up a dictonary of string offsets.
                Dictionary<long, int> stringOffsets = new();

                // Loop through each tag in this entry for the string offset table.
                for (int tagIndexIndex = 0; tagIndexIndex < Data.Tags[tagIndex].Count; tagIndexIndex++)
                {
                    // Add an entry to the dictonary consisting of our current position and the tag's linear index.
                    stringOffsets.Add(writer.BaseStream.Position, tagIndexIndex);

                    // Write a placeholder to fill in later.
                    writer.Write("==TEMP==");
                }

                // Loop through each tag in this entry for writing.
                for (int tagIndexIndex = 0; tagIndexIndex < Data.Tags[tagIndex].Count; tagIndexIndex++)
                {
                    // Write this tag's index.
                    writer.Write((long)Data.Tags[tagIndex][tagIndexIndex].Index);

                    // Write an unknown value that is always 0x10.
                    writer.Write(0x10L);

                    // Save our position so we can jump back after calculating the string offset.
                    long position = writer.BaseStream.Position;

                    // Jump to the stored position for this tag.
                    writer.BaseStream.Position = stringOffsets.ElementAt(tagIndexIndex).Key;

                    // Fill in the placeholder, calculating the gap between the start of the table and where this name's string should be.
                    writer.Write(position - (writer.BaseStream.Position - (0x08 * tagIndexIndex)));

                    // Jump back to our saved position.
                    writer.BaseStream.Position = position;

                    // Write this tag's name.
                    writer.WriteNullTerminatedString(Data.Tags[tagIndex][tagIndexIndex].Name);
                }

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
