using Newtonsoft.Json;
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
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.nu2.cratetable.json", Data);
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
            public Crate[] Crates { get; set; } = Array.Empty<Crate>();
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
        }

        // Actual data presented to the end user.
        public List<Group> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a GameCube ai file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Skip an unknown value that is always 0x04.
            reader.JumpAhead(0x04);

            // Read the count of groups in this crate table.
            ushort groupCount = reader.ReadUInt16();

            // Loop through each group in this crate table.
            for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
            {
                // Create a new group.
                Group group = new Group();

                // Read this group's position.
                group.Position = reader.ReadVector3();

                // Read this group's index.
                group.Index = reader.ReadUInt16();

                // Read the count of crates in this group and create the array accordingly.
                group.Crates = new Crate[reader.ReadUInt16()];

                // Read and convert this group's rotation from the Binary Angle Measurement System.
                group.Rotation = Helpers.CalculateBAMsValue(reader.ReadUInt16());

                // Loop through each crate in this group.
                for (int crateIndex = 0; crateIndex < group.Crates.Length; crateIndex++)
                {
                    // Create a new crate.
                    Crate crate = new();

                    // Read this crate's postion.
                    crate.Position = reader.ReadVector3();

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x04);

                    // Read this crate's first unknown short value.
                    crate.UnknownUShort_1 = reader.ReadUInt16();

                    // Read this crate's second unknown short value.
                    crate.UnknownUShort_2 = reader.ReadUInt16();

                    // Read this crate's third unknown short value.
                    crate.UnknownUShort_3 = reader.ReadUInt16();

                    // Read this crate's types.
                    crate.StandardType = (Type)reader.ReadByte();
                    crate.TimeTrialType = (Type)reader.ReadByte();
                    crate.OutlineType = (Type)reader.ReadByte();
                    crate.RouletteType = (Type)reader.ReadByte();

                    // Read this crate's fourth unknown short value.
                    crate.UnknownUShort_4 = reader.ReadUInt16();

                    // Read this crate's fifth unknown short value.
                    crate.UnknownUShort_5 = reader.ReadUInt16();

                    // Read this crate's sixth unknown short value.
                    crate.UnknownUShort_6 = reader.ReadUInt16();

                    // Read this crate's seventh unknown short value.
                    crate.UnknownUShort_7 = reader.ReadUInt16();

                    // Read this crate's eighth unknown short value.
                    crate.UnknownUShort_8 = reader.ReadUInt16();

                    // Read this crate's ninth unknown short value.
                    crate.UnknownUShort_9 = reader.ReadUInt16();

                    // Read this crate's tenth unknown short value.
                    crate.UnknownUShort_10 = reader.ReadUInt16();

                    // Save this crate.
                    group.Crates[crateIndex] = crate;
                }

                // Save this group.
                Data.Add(group);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If this is a GameCube ai file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write an unknown value of 0x04.
            writer.Write(0x04);

            // Write the count of groups in this crate table.
            writer.Write((ushort)Data.Count);

            // Loop through each group in this crate table.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this group's position.
                writer.Write(Data[dataIndex].Position);

                // Write this group's index.
                writer.Write(Data[dataIndex].Index);

                // Write the count of crates in this group.
                writer.Write((ushort)Data[dataIndex].Crates.Length);

                // Write this group's rotation, converted to the Binary Angle Measurement System.
                writer.Write((ushort)Helpers.CalculateBAMsValue(Data[dataIndex].Rotation));

                // Loop through each crate in this group.
                for (int crateIndex = 0; crateIndex < Data[dataIndex].Crates.Length; crateIndex++)
                {
                    // Write this crate's postion.
                    writer.Write(Data[dataIndex].Crates[crateIndex].Position);

                    // Write an unknown value that is always 0.
                    writer.WriteNulls(0x04);

                    // Write this crate's first unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_1);

                    // Write this crate's second unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_2);

                    // Write this crate's third unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_3);
                    
                    // Write this crate's types.
                    writer.Write((byte)Data[dataIndex].Crates[crateIndex].StandardType);
                    writer.Write((byte)Data[dataIndex].Crates[crateIndex].TimeTrialType);
                    writer.Write((byte)Data[dataIndex].Crates[crateIndex].OutlineType);
                    writer.Write((byte)Data[dataIndex].Crates[crateIndex].RouletteType);

                    // Write this crate's fourth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_4);

                    // Write this crate's fifth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_5);

                    // Write this crate's sixth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_6);

                    // Write this crate's seventh unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_7);

                    // Write this crate's eighth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_8);

                    // Write this crate's ninth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_9);

                    // Write this crate's tenth unknown short value.
                    writer.Write(Data[dataIndex].Crates[crateIndex].UnknownUShort_10);
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
