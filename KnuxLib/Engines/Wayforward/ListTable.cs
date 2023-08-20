using System.Text;

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
            uint StringDataCount = reader.ReadUInt32();

            // Read the amount of string keys in this file.
            ulong keysCount = reader.ReadUInt64();

            // Skip an unknown value of 0x40 (likely an offset that's always in the same place).
            reader.JumpAhead(0x08);

            // Read the string name count in this file.
            ulong StringNamesCount = reader.ReadUInt64();

            // Read the offset for the string name table.
            long StringNamesOffset = reader.ReadInt64();

            // Read this file's tag count.
            ulong TagCount = reader.ReadUInt64();

            // Read the offset to this file's tags.
            long TagOffset = reader.ReadInt64();

            // Jump to the offset for this file's tags.
            reader.JumpTo(TagOffset);

            // Define the array of tags in this file.
            Data.Tags = new List<Tag>[TagCount];

            // Define the array of string names used in this file.
            Data.Names = new List<StringName>[StringNamesCount];

            // Set up a list of the tags for later usage.
            Dictionary<ulong, string> tags = new();

            // Set up a value to hold the largest tag's index.
            ulong largestTagIndex = 0;

            // Loop through and read all the tags.
            for (ulong i = 0; i < TagCount; i++)
            {
                // Read this tag's data offset.
                long tagOffset = reader.ReadInt64();

                // Save our current position so we can jump back for the next tag.
                long position = reader.BaseStream.Position;

                // Jump to this tag's data.
                reader.JumpTo(tagOffset);

                // Read this tag collection's tag count.
                ulong tagCount = reader.ReadUInt64();

                // Skip an unknown value of 0x10.
                reader.JumpAhead(0x08);

                // Set up a new tag collection for this tag's linear index.
                Data.Tags[i] = new();

                // Read the tags if there actually is any.
                if (tagCount != 0)
                {
                    // Skip a table of values that point to each tag's strings in an additive form.
                    reader.JumpAhead(0x08 * (long)tagCount);

                    // Loop through and read this collection's tags.
                    for (ulong t = 0; t < tagCount; t++)
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
                        Data.Tags[i].Add(tag);

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
            Data.Entries = new EntryData[StringDataCount];

            // Definie all the entries in the string data array.
            for (int i = 0; i < StringDataCount; i++)
                Data.Entries[i] = new();

            // Loop through each tag.
            for (ulong i = 0; i <= largestTagIndex; i++)
            {
                // Loop through each string data entry.
                for (int t = 0; t < StringDataCount; t++)
                {
                    // Read the null terminated string at the listed offset.
                    string? data = Helpers.ReadNullTerminatedStringTableEntry(reader, false, 0, true);

                    // If the returned string is empty, then null it out.
                    if (data == "")
                        data = null;

                    // Add the read string to the string data alongside its tag.
                    Data.Entries[t].Keys.Add(tags[i], data);
                }
            }

            // Jump to the offset of the string names table.
            reader.JumpTo(StringNamesOffset);

            // Loop through each string name.
            for (ulong i = 0; i < StringNamesCount; i++)
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
                Data.Names[i] = new();

                // Read the names if there actually are any.
                if (entryCount != 0)
                {
                    // Skip a table of values that point to each name's strings in an additive form.
                    reader.JumpAhead(0x08 * (long)entryCount);

                    // Loop through and read this collection's names.
                    for (ulong t = 0; t < entryCount; t++)
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
                        Data.Names[i].Add(name);
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
                for (int i = 0; i < Data.Entries[0].Keys.Count; i++)
                    for (int t = 0; t < Data.Entries.Length; t++)
                        writer.AddOffset($"String{t}Key{i}", 0x08);

                // Realign to 0x40 bytes.
                writer.FixPadding(0x40);

                // Loop through each tag and each entry to write their data.
                for (int i = 0; i < Data.Entries[0].Keys.Count; i++)
                {
                    for (int t = 0; t < Data.Entries.Length; t++)
                    {
                        // Fill in the offset for this string and this tag.
                        writer.FillOffset($"String{t}Key{i}");

                        // Get the value of the tag.
                        string? keyValue = Data.Entries[t].Keys.ElementAt(i).Value;

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
            for (int i = 0; i < Data.Names.Length; i++)
                writer.AddOffset($"StringName{i}", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write each name entry.
            for (int i = 0; i < Data.Names.Length; i++)
            {
                // Fill in this name entry's offset.
                writer.FillOffset($"StringName{i}");

                // Write the count of names in this entry.
                writer.Write((long)Data.Names[i].Count);

                // Write an unknown value that is always 0x10.
                writer.Write(0x10L);

                // Set up a dictonary of string offsets.
                Dictionary<long, int> stringOffsets = new();

                // Loop through each name in this entry for the string offset table.
                for (int n = 0; n < Data.Names[i].Count; n++)
                {
                    // Add an entry to the dictonary consisting of our current position and the name's linear index.
                    stringOffsets.Add(writer.BaseStream.Position, n);

                    // Write a placeholder to fill in later.
                    writer.Write("==TEMP==");
                }

                // Loop through each name in this entry for writing.
                for (int n = 0; n < Data.Names[i].Count; n++)
                {
                    // Write this name's index.
                    writer.Write((long)Data.Names[i][n].Index);

                    // Write an unknown value that is always 0x10.
                    writer.Write(0x10L);

                    // Save our position so we can jump back after calculating the string offset.
                    long pos = writer.BaseStream.Position;

                    // Jump to the stored position for this name.
                    writer.BaseStream.Position = stringOffsets.ElementAt(n).Key;

                    // Fill in the placeholder, calculating the gap between the start of the table and where this name's string should be.
                    writer.Write(pos - (writer.BaseStream.Position - (0x08 * n)));

                    // Jump back to our saved position.
                    writer.BaseStream.Position = pos;

                    // Write this entry's name.
                    writer.WriteNullTerminatedString(Data.Names[i][n].Name);
                }

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Fill in the offset for this file's tags.
            writer.FillOffset("Tags");

            // Loop through and add an offset for each tag entry.
            for (int i = 0; i < Data.Tags.Length; i++)
                writer.AddOffset($"Tag{i}", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through and write each tag entry.
            for (int i = 0; i < Data.Tags.Length; i++)
            {
                // Fill in this tag entry's offset.
                writer.FillOffset($"Tag{i}");

                // Write the count of tags in this entry.
                writer.Write((long)Data.Tags[i].Count);

                // Write an unknown value that is always 0x10.
                writer.Write(0x10L);

                // Set up a dictonary of string offsets.
                Dictionary<long, int> stringOffsets = new();

                // Loop through each tag in this entry for the string offset table.
                for (int n = 0; n < Data.Tags[i].Count; n++)
                {
                    // Add an entry to the dictonary consisting of our current position and the tag's linear index.
                    stringOffsets.Add(writer.BaseStream.Position, n);

                    // Write a placeholder to fill in later.
                    writer.Write("==TEMP==");
                }

                // Loop through each tag in this entry for writing.
                for (int n = 0; n < Data.Tags[i].Count; n++)
                {
                    // Write this tag's index.
                    writer.Write((long)Data.Tags[i][n].Index);

                    // Write an unknown value that is always 0x10.
                    writer.Write(0x10L);

                    // Save our position so we can jump back after calculating the string offset.
                    long pos = writer.BaseStream.Position;

                    // Jump to the stored position for this tag.
                    writer.BaseStream.Position = stringOffsets.ElementAt(n).Key;

                    // Fill in the placeholder, calculating the gap between the start of the table and where this name's string should be.
                    writer.Write(pos - (writer.BaseStream.Position - (0x08 * n)));

                    // Jump back to our saved position.
                    writer.BaseStream.Position = pos;

                    // Write this tag's name.
                    writer.WriteNullTerminatedString(Data.Tags[i][n].Name);
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
