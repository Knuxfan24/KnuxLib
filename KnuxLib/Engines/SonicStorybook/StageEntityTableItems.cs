using System.Collections;

namespace KnuxLib.Engines.SonicStorybook
{
    public class StageEntityTableItems : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTableItems() { }
        public StageEntityTableItems(string filepath, FormatVersion version = FormatVersion.SecretRings, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".sonicstorybook.setitems.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<FormatData>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.bin", version);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
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
            /// The stages defined in this items table.
            /// </summary>
            public Stage[] Stages { get; set; } = [];

            /// <summary>
            /// The objects defined in this items table.
            /// </summary>
            public Object[] Objects { get; set; } = [];
        }

        public class Stage
        {
            /// <summary>
            /// This stage's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// This stage's index, always 0xCCCCCCCC in Secret Rings, varies in Black Knight.
            /// </summary>
            public uint Index { get; set; } = 0xCCCCCCCC;

            /// <summary>
            /// The names of the objects this stage will load.
            /// </summary>
            public List<string> Objects { get; set; } = [];

            /// <summary>
            /// Displays this stage's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this stage with default data.
            /// </summary>
            public Stage() { }

            /// <summary>
            /// Initialises this stage with the provided data.
            /// </summary>
            public Stage(string name, uint index, List<string> objects)
            {
                Name = name;
                Index = index;
                Objects = objects;
            }

            /// <summary>
            /// Initialises this stage by reading its data from a BinaryReader.
            /// </summary>
            public Stage(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this stage.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Name = reader.ReadNullPaddedString(0x10);
                Index = reader.ReadUInt32();
            }

            /// <summary>
            /// Writes the data for this stage.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.WriteNullPaddedString(Name, 0x10);
                writer.Write(Index);
            }
        }

        public class Object
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
            /// Displays this object's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this object with default data.
            /// </summary>
            public Object() { }

            /// <summary>
            /// Initialises this object with the provided data.
            /// </summary>
            public Object(string name, byte objectID, byte tableID)
            {
                Name = name;
                ObjectID = objectID;
                TableID = tableID;
            }

            /// <summary>
            /// Initialises this object by reading its data from a BinaryReader.
            /// </summary>
            public Object(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this object.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Name = reader.ReadNullPaddedString(0x20);
                ObjectID = reader.ReadByte();
                TableID = reader.ReadByte();
                reader.JumpAhead(0x02); // Always both 0xCD.
            }

            /// <summary>
            /// Writes the data for this object.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.WriteNullPaddedString(Name, 0x20);
                writer.Write(ObjectID);
                writer.Write(TableID);
                writer.Write(new byte[2] { 0xCD, 0xCD });
            }
        }

        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Read this file's size.
            uint fileSize = reader.ReadUInt32();

            // Read the stage count for this file.
            uint stageCount = reader.ReadUInt32();

            // If this is a Black Knight file, then read the extra sub count.
            uint stageSubCount = 1;
            if (version == FormatVersion.BlackKnight)
                stageSubCount = reader.ReadUInt32();

            // Initialise the data arrays.
            Data.Stages = new Stage[stageCount * stageSubCount];
            Data.Objects = new Object[reader.ReadUInt32()];

            // Read an unknown value in this file.
            // TODO: What is this value? It's 0x03 in Secret Rings and 0x09 in Black Knight.
            uint unknownUInt32_1 = reader.ReadUInt32();

            // Read the offset to this file's stage table.
            uint stageTableOffset = reader.ReadUInt32();

            // Read the offset to this file's object table.
            uint objectTableOffset = reader.ReadUInt32();

            // Jump to this file's stage table (should already be here but lets play it safe).
            reader.JumpTo(stageTableOffset);

            // Loop through and read each stage.
            for (int stageIndex = 0; stageIndex < Data.Stages.Length; stageIndex++)
                Data.Stages[stageIndex] = new(reader);

            // Jump to this file's object table (should already be here but lets play it safe).
            reader.JumpTo(objectTableOffset);

            // Loop through each object in this file.
            for (int objectIndex = 0; objectIndex < Data.Objects.Length; objectIndex++)
            {
                // Read this object.
                Data.Objects[objectIndex] = new(reader);

                // Determine how many bytes are needed to read this object's allowed stage count.
                int allowedStageByteCount = 0x0C;
                if (version == FormatVersion.BlackKnight)
                    allowedStageByteCount = 0x24;

                // Loop through each allowed stage byte.
                for (int allowedStageByteIndex = 0; allowedStageByteIndex < allowedStageByteCount; allowedStageByteIndex++)
                {
                    // Read this byte and split it to its individual bits.
                    BitArray bits = new(reader.ReadBytes(0x01));

                    // Loop through each bit.
                    for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                    {
                        // Calculate the stage this bit is for.
                        int stageIndex = bitIndex + (allowedStageByteIndex * 8);

                        // If this stage index is valid and this bit is true, then add this object to the stage's object list.
                        if (stageIndex < Data.Stages.Length)
                            if (bits[bitIndex] == true)
                                Data.Stages[stageIndex].Objects.Add(Data.Objects[objectIndex].Name);
                    }
                }
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Check the file version.
            switch (version)
            {
                // If this is a Secret Rings file, then just write the stage count.
                case FormatVersion.SecretRings:
                    writer.Write(Data.Stages.Length);
                    break;

                // If this is a Black Knight file, then write the stage count divided by six, followed by a six.
                case FormatVersion.BlackKnight:
                    writer.Write(Data.Stages.Length / 0x06);
                    writer.Write(0x06);
                    break;
            }

            // Write this file's object count.
            writer.Write(Data.Objects.Length);

            // Write the unknown integer value.
            if (version == FormatVersion.SecretRings) writer.Write(0x03);
            if (version == FormatVersion.BlackKnight) writer.Write(0x09);

            // Add an offset for the stage table.
            writer.AddOffset("StageTable");

            // Add an offset for the object table.
            writer.AddOffset("ObjectTable");

            // Fill in the offset for the stage table.
            writer.FillInOffset("StageTable");

            // Loop through and write each stage.
            for (int stageIndex = 0; stageIndex < Data.Stages.Length; stageIndex++)
                Data.Stages[stageIndex].Write(writer);

            // Fill in the offset for the object table.
            writer.FillInOffset("ObjectTable");

            // Loop through each object.
            for (int objectIndex = 0; objectIndex < Data.Objects.Length; objectIndex++)
            {
                // Write this object.
                Data.Objects[objectIndex].Write(writer);

                // Set up an array to store the bytes for this object's allowed stages.
                byte[] allowedStagesBytes = new byte[0x0C];
                if (version == FormatVersion.BlackKnight)
                    allowedStagesBytes = new byte[0x24];

                // Set up a BitArray to store this object's allowed stages.
                BitArray allowedStagesBits = new(Data.Stages.Length);

                // Loop through each stage and check for this object in its list, if found, flip the bit for it in our array.
                for (int i = 0; i < Data.Stages.Length; i++)
                    if (Data.Stages[i].Objects.Contains(Data.Objects[objectIndex].Name))
                        allowedStagesBits[i] = true;

                // Convert our BitArray to a byte array.
                allowedStagesBits.CopyTo(allowedStagesBytes, 0);

                // Write the contents of our byte array.
                writer.Write(allowedStagesBytes);
            }

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Close our BinaryWriter.
            writer.Close();
        }
    }
}
