namespace KnuxLib.Engines.Storybook
{
    public class StageEntityTableItems : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTableItems() { }
        public StageEntityTableItems(string filepath, FormatVersion version = FormatVersion.SecretRings, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.stageentitytableitems.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            SecretRings = 0,
            BlackKnight = 1
        }

        public class FormatData
        {
            /// <summary>
            /// A list of stages and their indices.
            /// </summary>
            public List<StageEntry> Stages { get; set; } = new();

            /// <summary>
            /// A list of objects defined in this file.
            /// </summary>
            public List<ObjectEntry> Objects { get; set; } = new();
        }

        public class StageEntry
        {
            /// <summary>
            /// This stage's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// This stage's index, always 0xCCCCCCCC in Secret Rings, varies in Black Knight.
            /// </summary>
            public uint Index { get; set; } = 0xCCCCCCCC;

            public override string ToString() => Name;
        }

        public class ObjectEntry
        {
            /// <summary>
            /// This object's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// This object's ID.
            /// </summary>
            public byte ObjectID { get; set; }

            /// <summary>
            /// The ID of the table this object belongs to.
            /// </summary>
            public byte TableID { get; set; }

            /// <summary>
            /// A list of stage indices this object can load in.
            /// </summary>
            public List<bool> AllowedStages { get; set; } = new();

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's size.
            uint fileSize = reader.ReadUInt32();

            // Read the stage count for this file.
            uint stageCount = reader.ReadUInt32();

            // If this is a Black Knight file, then read the extra sub count.
            uint? stageSubCount = null;
            if (version == FormatVersion.BlackKnight)
                stageSubCount = reader.ReadUInt32();

            // Read the object count for this file.
            uint objectCount = reader.ReadUInt32();

            // Read an unknown value in this file.
            // TODO: What is this value?
            uint UnknownUInt32_1 = reader.ReadUInt32();

            // Read an unknown value in this file.
            // TODO: What is this value?
            uint UnknownUInt32_2 = reader.ReadUInt32();
            
            // Read the offset to this file's object table.
            uint objectTableOffset = reader.ReadUInt32();

            // Loop through each stage entry.
            for (int i = 0; i < stageCount; i++)
            {
                // Set up a stage entry.
                StageEntry stage;

                // Check the format version.
                switch (version)
                {
                    // Read a single stage entry if this is a Secret Rings file.
                    case FormatVersion.SecretRings:
                        stage = new()
                        {
                            Name = reader.ReadNullPaddedString(0x10),
                            Index = reader.ReadUInt32()
                        };
                        Data.Stages.Add(stage);
                        break;

                    // Read multiple stage entries based on stageSubCount if this is a Black Knight file.
                    case FormatVersion.BlackKnight:
                        for (int s = 0; s < stageSubCount; s++)
                        {
                            stage = new()
                            {
                                Name = reader.ReadNullPaddedString(0x10),
                                Index = reader.ReadUInt32()
                            };
                            Data.Stages.Add(stage);
                        }
                        break;
                }
            }

            // Jump to this file's object table.
            reader.JumpTo(objectTableOffset);

            // Loop through each object in this file.
            for (int i = 0; i < objectCount; i++)
            {
                // Set up a new object entry.
                ObjectEntry obj = new();

                // Read this object's entry.
                obj.Name = reader.ReadNullPaddedString(0x20);

                // Read this object's ID.
                obj.ObjectID = reader.ReadByte();

                // Read the ID of the table this object is a part of.
                obj.TableID = reader.ReadByte();

                // Skip over two bytes that are always 0xCD.
                reader.JumpAhead(0x02);

                // Set up a list of uints for parsing.
                List<uint> allowedStages = new();

                // Read three values into the allowedStages list.
                allowedStages.Add(reader.ReadUInt32());
                allowedStages.Add(reader.ReadUInt32());
                allowedStages.Add(reader.ReadUInt32());

                // If this is a Black Knight file, then read an additional six values.
                if (version == FormatVersion.BlackKnight)
                {
                    allowedStages.Add(reader.ReadUInt32());
                    allowedStages.Add(reader.ReadUInt32());
                    allowedStages.Add(reader.ReadUInt32());
                    allowedStages.Add(reader.ReadUInt32());
                    allowedStages.Add(reader.ReadUInt32());
                    allowedStages.Add(reader.ReadUInt32());
                }

                // Convert each allowed stage value to binary and check if it's allowed to load.
                foreach (var value in allowedStages)
                {
                    // Convert this value to a binary string.
                    string binary = Helpers.ToBinaryString(value);

                    // Loop through each character of the binary string backwards and add a true or false depending on the character.
                    for (int boolean = binary.Length - 1; boolean >= 0; boolean--)
                    {
                        if (binary[boolean] == '1')
                            obj.AllowedStages.Add(true);
                        else
                            obj.AllowedStages.Add(false);
                    }
                }

                // Save this object.
                Data.Objects.Add(obj);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Set up the location of and write a placeholder size entry to fill in later.
            long sizePos = writer.BaseStream.Position;
            writer.Write("SIZE");

            // Check the file version.
            switch (version)
            {
                // If this is a Secret Rings file, then just write the stage count.
                case FormatVersion.SecretRings:
                    writer.Write(Data.Stages.Count);
                    break;

                // If this is a Black Knight file, then write the stage count divided by six, followed by a six.
                case FormatVersion.BlackKnight:
                    writer.Write(Data.Stages.Count / 0x06);
                    writer.Write(0x06);
                    break;
            }

            // Write this file's object count.
            writer.Write(Data.Objects.Count);

            // Check the file version.
            switch (version)
            {
                // If this is a Secret Rings file, then write 0x03 and 0x18.
                case FormatVersion.SecretRings:
                    writer.Write(0x03);
                    writer.Write(0x18);
                    break;

                // If this is a Black Knight file, then write 0x09 and 0x1C.
                case FormatVersion.BlackKnight:
                    writer.Write(0x09);
                    writer.Write(0x1C);
                    break;
            }

            // Add an offset to the object table.
            writer.AddOffset("ObjectTable");

            // Loop through each stage entry.
            for (int i = 0; i < Data.Stages.Count; i++)
            {
                // Write this stage's name.
                writer.WriteNullPaddedString(Data.Stages[i].Name, 0x10);

                // Write this stage's index.
                writer.Write(Data.Stages[i].Index);
            }

            // Fill in the offset to the object table.
            writer.FillOffset("ObjectTable");

            // Loop through each object.
            for (int i = 0; i < Data.Objects.Count; i++)
            {
                // Write this object's name.
                writer.WriteNullPaddedString(Data.Objects[i].Name, 0x20);

                // Write this object's ID.
                writer.Write(Data.Objects[i].ObjectID);

                // Write this object's Table ID.
                writer.Write(Data.Objects[i].TableID);

                // Write the two bytes that are always 0xCD.
                writer.Write((ushort)0xCDCD);

                // Write the stage table for this object.
                for (int stages = 0; stages < Data.Objects[i].AllowedStages.Count; stages += 32)
                {
                    // Set up a temporary string.
                    string bits = "";

                    // Get the 32 values in these bits and flip them.
                    List<bool> range = Data.Objects[i].AllowedStages.GetRange(stages, 32);
                    range.Reverse();

                    // Add this value to the string.
                    foreach (bool stage in range)
                        bits += Convert.ToInt32(stage);

                    // Convert the string to a uint and write it.
                    writer.Write(Convert.ToUInt32(bits, 2));
                }
            }

            // Go back and fill in the file size.
            writer.BaseStream.Position = sizePos;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
