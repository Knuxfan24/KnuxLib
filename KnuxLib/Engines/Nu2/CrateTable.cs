using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Nu2
{
    // TODO: Figure out all of the unknown values.
    // TODO: Is it worth hardcoding the group position and index values? Will simplifiy the JSON a bit but stop the files from writing binary identical.
    public class CrateTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public CrateTable() { }
        public CrateTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".nu2.cratetable.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Group[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.crt", version);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            PlayStation2Xbox = 1
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Type : byte
        {
            Outline = 0x00,
            Standard = 0x01,
            ExtraLife = 0x02,
            AkuAku = 0x03,
            Arrow = 0x04,
            QuestionMark = 0x05,
            Striped = 0x06,
            Checkpoint = 0x07,
            Roulette = 0x08,
            TNT = 0x09,
            TimeCrate1 = 0x0A,
            TimeCrate2 = 0x0B,
            TimeCrate3 = 0x0C,
            IronArrow = 0x0D,
            Switch = 0x0E,
            Iron = 0x0F,
            Nitro = 0x10,
            NitroSwitch = 0x11,
            Proximity = 0x12,
            Locked = 0x13,
            Invisibility = 0x14,
            None = 0xFF
        }

        public class Group
        {
            /// <summary>
            /// A position value for this group. Shared with the first crate in the group in all but once instance.
            /// TODO: Does this even do anything? Didn't seem to at first glance?
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// A unique index for this group.
            /// </summary>
            public ushort Index { get; set; }

            /// <summary>
            /// A rotation value for this group, converted from the Binary Angle Measurement System.
            /// </summary>
            public float Rotation { get; set; }

            /// <summary>
            /// The crates in this group.
            /// </summary>
            public Crate[] Crates { get; set; } = [];

            /// <summary>
            /// Initialises this group with default data.
            /// </summary>
            public Group() { }

            /// <summary>
            /// Initialises this group with the provided data.
            /// </summary>
            public Group(Vector3 position, ushort index, float rotation, Crate[] crates)
            {
                Position = position;
                Index = index;
                Rotation = rotation;
                Crates = crates;
            }

            /// <summary>
            /// Initialises this group by reading its data from a BinaryReader.
            /// </summary>
            public Group(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this group.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Position = reader.ReadVector3();
                Index = reader.ReadUInt16();
                Crates = new Crate[reader.ReadUInt16()];
                Rotation = Helpers.CalculateBAMsValue(reader.ReadUInt16());
                for (int crateIndex = 0; crateIndex < Crates.Length; crateIndex++)
                    Crates[crateIndex] = new(reader);
            }

            /// <summary>
            /// Writes the data for this group.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.Write(Position);
                writer.Write(Index);
                writer.Write((ushort)Crates.Length);
                writer.Write((ushort)Helpers.CalculateBAMsValue(Rotation));
                foreach (Crate crate in Crates)
                    crate.Write(writer);
            }
        }

        public class Crate
        {
            /// <summary>
            /// The position of this crate in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

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
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_3 { get; set; }

            /// <summary>
            /// The type of crate this normally is.
            /// </summary>
            public Type StandardType { get; set; }

            /// <summary>
            /// The type of crate this should be in a Time Trial.
            /// </summary>
            public Type TimeTrialType { get; set; }

            /// <summary>
            /// The type of crate this should become if activated from an outline, also used by the roulette crate.
            /// </summary>
            public Type OutlineType { get; set; }

            /// <summary>
            /// The second type of crate this should become in a roulette.
            /// </summary>
            public Type RouletteType { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_4 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_5 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_6 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_7 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_8 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_9 { get; set; } = 0xFF;

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_10 { get; set; } = 0xFF;

            /// <summary>
            /// Displays this crate's type and position in the debugger.
            /// </summary>
            public override string ToString() => $"{StandardType} at {Position}";

            /// <summary>
            /// Initialises this crate with default data.
            /// </summary>
            public Crate() { }

            /// <summary>
            /// Initialises this crate with the provided data.
            /// </summary>
            public Crate(Vector3 position, ushort unknownUShort_1, ushort unknownUShort_2, ushort unknownUShort_3, Type standardType, Type timeTrialType, Type outlineType, Type rouletteType, ushort unknownUShort_4, ushort unknownUShort_5, ushort unknownUShort_6, ushort unknownUShort_7, ushort unknownUShort_8, ushort unknownUShort_9, ushort unknownUShort_10)
            {
                Position = position;
                UnknownUShort_1 = unknownUShort_1;
                UnknownUShort_2 = unknownUShort_2;
                UnknownUShort_3 = unknownUShort_3;
                StandardType = standardType;
                TimeTrialType = timeTrialType;
                OutlineType = outlineType;
                RouletteType = rouletteType;
                UnknownUShort_4 = unknownUShort_4;
                UnknownUShort_5 = unknownUShort_5;
                UnknownUShort_6 = unknownUShort_6;
                UnknownUShort_7 = unknownUShort_7;
                UnknownUShort_8 = unknownUShort_8;
                UnknownUShort_9 = unknownUShort_9;
                UnknownUShort_10 = unknownUShort_10;
            }

            /// <summary>
            /// Initialises this crate by reading its data from a BinaryReader.
            /// </summary>
            public Crate(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this crate.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Position = reader.ReadVector3();
                reader.JumpAhead(0x04); // Always 0.
                UnknownUShort_1 = reader.ReadUInt16();
                UnknownUShort_2 = reader.ReadUInt16();
                UnknownUShort_3 = reader.ReadUInt16();
                StandardType = (Type)reader.ReadByte();
                TimeTrialType = (Type)reader.ReadByte();
                OutlineType = (Type)reader.ReadByte();
                RouletteType = (Type)reader.ReadByte();
                UnknownUShort_4 = reader.ReadUInt16();
                UnknownUShort_5 = reader.ReadUInt16();
                UnknownUShort_6 = reader.ReadUInt16();
                UnknownUShort_7 = reader.ReadUInt16();
                UnknownUShort_8 = reader.ReadUInt16();
                UnknownUShort_9 = reader.ReadUInt16();
                UnknownUShort_10 = reader.ReadUInt16();
            }

            /// <summary>
            /// Writes the data for this crate.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.Write(Position);
                writer.WriteNulls(0x04); // Always 0.
                writer.Write(UnknownUShort_1);
                writer.Write(UnknownUShort_2);
                writer.Write(UnknownUShort_3);
                writer.Write((byte)StandardType);
                writer.Write((byte)TimeTrialType);
                writer.Write((byte)OutlineType);
                writer.Write((byte)RouletteType);
                writer.Write(UnknownUShort_4);
                writer.Write(UnknownUShort_5);
                writer.Write(UnknownUShort_6);
                writer.Write(UnknownUShort_7);
                writer.Write(UnknownUShort_8);
                writer.Write(UnknownUShort_9);
                writer.Write(UnknownUShort_10);
            }
        }

        // Actual data presented to the end user.
        public Group[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // If this is a GameCube ai file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Skip an unknown value that is always 0x04.
            reader.JumpAhead(0x04);

            // Read the count of groups in this crate table.
            Data = new Group[reader.ReadUInt16()];

            // Loop through and read each group in this crate table.
            for (int groupIndex = 0; groupIndex < Data.Length; groupIndex++)
                Data[groupIndex] = new(reader);
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // If this is a GameCube ai file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write an unknown value of 0x04.
            writer.Write(0x04);

            // Write the count of groups in this crate table.
            writer.Write((ushort)Data.Length);

            // Loop through and write each group.
            for (int groupIndex = 0; groupIndex < Data.Length; groupIndex++)
                Data[groupIndex].Write(writer);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
