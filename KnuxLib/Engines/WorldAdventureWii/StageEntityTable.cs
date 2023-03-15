using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.WorldAdventureWii
{    
    // TODO: Figure out object parameters.
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, FormatVersion version = FormatVersion.Wii, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.worldadventurewii.stageentitytable.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            PlayStaion2 = 0,
            Wii = 1
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ObjectType
        {
            GIM_RING = 0x10000001,
            GIM_RING2 = 0x10000005,
            GIM_RING_GENERATOR = 0x10000002,
            GIM_RING_GENERATOR2 = 0x10000003,
            GIM_RING_GENERATOR3 = 0x10000004,
            GIM_RING_GENERATOR4 = 0x10000006,
            GIM_RESTART_OBJECT = 0x00000009,
            GIM_SCENE_PLAY = 0x0000000F,
            GIM_TRIGGER_PLANE_DEAD = 0x10050001,
            GIM_TRIGGER_PLANE_BOBSLEIGH = 0x10050003,
            GIM_TRIGGER_PLANE_CHANGE_CAMERA = 0x10050004,
            GIM_TRIGGER_PLANE_CONTROL_SONIC = 0x10050005,
            GIM_TRIGGER_PLANE_LAP_TIMER = 0x10050008,
            GIM_TRIGGER_SOLID_SHARE_FLAG = 0x10058002,
            GIM_TRIGGER_SOLID_QTE = 0x10058003,
            GIM_COMMON_DECORATION = 0x10010001,
            GIM_COMMON_ITEM_BOX = 0x10010002,
            GIM_COMMON_SWITCH = 0x10010003,
            GIM_COMMON_DOOR = 0x10010004,
            GIM_COMMON_GOAL_RING = 0x10010006,
            GIM_COMMON_FLOOR = 0x10010007,
            GIM_COMMON_FLOOR2 = 0x10010008,
            GIM_COMMON_FLOOR3 = 0x10010009,
            GIM_COMMON_FLOOR4 = 0x1001000A,
            GIM_COMMON_FLOOR5 = 0x1001000B,
            GIM_COMMON_FLOOR6 = 0x1001000C,
            GIM_COMMON_BELT_CONVEYER = 0x10010005,
            GIM_COMMON_PENDULUM = 0x10030007,
            GIM_COMMON_TALK = 0x1001000D,
            GIM_COMMON_TALK2 = 0x10010014,
            GIM_COMMON_FIELD_WALL = 0x1001000E,
            GIM_COMMON_RESTART_POINT = 0x10010012,
            GIM_COMMON_RESTART_COLLISION = 0x10010013,
            GIM_COMMON_COLLISION = 0x10010015,
            GIM_SONIC_JUMP_POLE = 0x10020001,
            GIM_SONIC_JUMP_STAND = 0x10020002,
            GIM_SONIC_BOBSLEIGH = 0x10020003,
            GIM_SONIC_ROCKET_SIDE_F = 0x10020005,
            GIM_SONIC_ROCKET_WALL = 0x10020007,
            GIM_SONIC_ST2_FLOOR = 0x10020008,
            GIM_SONIC_ST2_WALL = 0x10020009,
            GIM_SONIC_DASH_PANEL = 0x1002000A,
            GIM_SONIC_DASH_CIRCLE = 0x1002000B,
            GIM_SONIC_JUMP_SELECTOR = 0x1002000C,
            GIM_SONIC_SPRING = 0x1002000D,
            GIM_SONIC_SPRING2 = 0x1002000E,
            GIM_SONIC_REACTION_PLATE = 0x1002000F,
            GIM_SONIC_REEL = 0x10020010,
            GIM_SONIC_REEL2 = 0x10020018,
            GIM_SONIC_PULLEY = 0x10020011,
            GIM_SONIC_TRICK_STAND = 0x10020012,
            GIM_SONIC_CANNON = 0x10020013,
            GIM_SONIC_IVY = 0x10020014,
            GIM_SONIC_FERRIS_WHEEL = 0x10020015,
            GIM_SONIC_BREAKABLE = 0x10020016,
            GIM_SONIC_BOX = 0x10020017,
            GIM_SONIC_DAMAGABLE = 0x1002001A,
            GIM_SONIC_DAMAGABLE2 = 0x1002001C,
            GIM_SONIC_DAMAGABLE3 = 0x1002001F,
            GIM_SONIC_SHRINE_ROBO = 0x1002001B,
            GIM_SONIC_BAR = 0x1002001D,
            GIM_SONIC_DGFEELER = 0x1002001E,
            GIM_SONIC_COLLISION = 0x10020020,
            GIM_SONIC_COLLISION2 = 0x10020021,
            GIM_EVIL_GROUND_BOX = 0x10030001,
            GIM_EVIL_COLUMN = 0x10030002,
            GIM_EVIL_BAR = 0x10030003,
            GIM_EVIL_HANDLE = 0x10030004,
            GIM_EVIL_SPEW = 0x10030005,
            GIM_EVIL_FLAME = 0x10030006,
            GIM_EVIL_SHUTTER = 0x10030009,
            GIM_EVIL_FIELD_WALL = 0x1003000A,
            GIM_EVIL_LEVER = 0x1003000B,
            GIM_EVIL_COLUMN2 = 0x1003000C,
            GIM_EVIL_COLUMN2_LINE = 0x1003000D,
            GIM_EVIL_CRANK = 0x1003000E,
            GIM_EVIL_CRANK2 = 0x1003001D,
            GIM_EVIL_BREAKABLE = 0x1003000F,
            GIM_EVIL_BREAKABLE2 = 0x10030016,
            GIM_EVIL_NEEDLE = 0x10030010,
            GIM_EVIL_NEEDLE2 = 0x10030011,
            GIM_EVIL_NEEDLE3 = 0x10030012,
            GIM_EVIL_NEEDLE4 = 0x10030013,
            GIM_EVIL_CIRCLE_EDGE = 0x10030014,
            GIM_EVIL_DRAW_BRIDGE = 0x10030017,
            GIM_EVIL_ELEVATOR = 0x10030018,
            GIM_EVIL_BIG_ELEVATOR = 0x10030019,
            GIM_EVIL_DAMAGABLE = 0x10030015,
            GIM_EVIL_DAMAGABLE2 = 0x1003001C,
            GIM_EVIL_IFPLAY_EMBLEM = 0x1001000F,
            GIM_EVIL_IFPLAY_ITEM = 0x10010010,
            GIM_EVIL_IFPLAY_COLLISION = 0x10010011,
            GIM_EVIL_BOX = 0x1003001A,
            GIM_EVIL_BOX_GENERATOR = 0x1003001B,
            GIM_ENTR_EXIT = 0x10040001,
            GIM_ENTR_LIGHT = 0x10040002,
            GIM_ENTR_LIGHT2 = 0x10040005,
            GIM_ENTR_DOOR = 0x10040003,
            GIM_ENTR_WARP_GATE = 0x10040004,
            GIM_MISS_TREASURE_ITEM = 0x10060001,
            GIM_MISS_POINT_MARKER = 0x10060002,
            ENM_SPINNER = 0x20000001,
            ENM_SPANNER = 0x20000002,
            ENM_THUNDER_BALL = 0x20000003,
            ENM_MOLE_CANNON = 0x20000004,
            ENM_EGG_FIGHTER_NORMAL = 0x20000005,
            ENM_EGG_FIGHTER_SHIELD = 0x20000006,
            ENM_EGG_FIGHTER_SPRING = 0x20000007,
            ENM_EGG_FIGHTER_SWORD = 0x20000008,
            ENM_EGG_FIGHTER_MISSILE = 0x20000009,
            ENM_AIR_CANNON = 0x2000000D,
            ENM_AIR_CHASER = 0x2000000F,
            ENM_EGG_TYPHOON = 0x2000000A,
            ENM_EGG_FLAME = 0x2000000C,
            ENM_EGG_BLIZZARD = 0x2000000B,
            ENM_EGG_SHACKLE = 0x20000011,
            ENM_NIGHTMARE = 0x20000012,
            ENM_BRAVEHEART = 0x20000013,
            ENM_KILLERBEE = 0x20000014,
            ENM_DARKFLOAT = 0x20000015,
            ENM_DARKFLOAT_CANNON = 0x20000016,
            ENM_THUNDER_FLOAT = 0x20000017,
            ENM_BIGMOTHER = 0x20000018,
            ENM_TITAN = 0x20000019,
            ENM_CUREMASTER = 0x2000001A,
            ENM_FIREMASTER = 0x2000001B,
            ENM_SPOOKYMASTER = 0x2000001C,
            ENM_THUNDERMASTER = 0x2000001D,
            ENM_RECKLESS = 0x2000001E,
            ENM_SPOOKY = 0x2000001F,
            ENM_BIG_CHASER = 0x20000010,
            ENM_DARKGAIA_FLYINGHAND = 0x20000020,
            ENM_EGG_TYPHOON_NIGHT = 0x20000021,
            ENM_EGG_FLAME_NIGHT = 0x20000022,
            ENM_EGG_BLIZZARD_NIGHT = 0x20000023,
            ENM_DARKGAIA_EYE = 0x20000024,
            BOSS_EGGBEETLE = 0x30000001,
            BOSS_PHOENIX = 0x30000002,
            BOSS_EGG_DRAGOON = 0x30000003,
            BOSS_MORAY = 0x30000004,
            BOSS_DARKGAIA_THONIC = 0x30000005,
            BOSS_DARKGAIA_FIRST = 0x30000006,
            BOSS_EGG_RAYBIRD = 0x30000007,
            BOSS_DARKGAIA_SECOND = 0x30000008,
            BOSS_EGG_LANCER = 0x30000009,
            NOT_IN_RSO = 0x10050002
        }

        public class SetObject
        {
            /// <summary>
            /// This object's type.
            /// TODO: I don't like this massive enum, but I'm not sure of a better way to do this.
            /// </summary>
            public ObjectType Type { get; set; }

            /// <summary>
            /// This object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This object's rotation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This object's parameters.
            /// TODO: Find a way to read the parameters as the correct types.
            /// </summary>
            public List<SetParameter> Parameters { get; set; } = new();
        }

        public class SetParameter
        {
            /// <summary>
            /// This parameter's name.
            /// TODO: Currently not used as these parameters just boil down to reading the table each byte at a time.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The data for this parameter.
            /// </summary>
            public object Data { get; set; } = (byte)0;

            /// <summary>
            /// The type of this parameter.
            /// </summary>
            public Type DataType { get; set; } = typeof(byte);
        }

        // Actual data presented to the end user.
        public List<SetObject> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.Wii)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Check the version.
            if (reader.ReadByte() != (byte)version)
                throw new Exception("Incorrect format version set!");
            
            // Read the number of objects in this SET.
            uint objectCount = reader.ReadUInt32();

            // Read this SET's file size.
            uint fileSize = reader.ReadUInt32();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Switch the reader to Big Endian if this is a Wii file.
            if (version == FormatVersion.Wii)
                reader.IsBigEndian = true;

            // Loop through this SET's object table.
            for (int i = 0; i < objectCount; i++)
            {
                // Read the offset to this object's data.
                uint objectOffset = reader.ReadUInt32();
                
                // Read the length of the data that makes up this object.
                uint objectLength = reader.ReadUInt32();

                // Save our position in the table.
                long pos = reader.BaseStream.Position;

                // Jump to our object.
                reader.JumpTo(objectOffset);

                // Skip an unknown value that is always a copy of objectLength.
                reader.JumpAhead(0x04);

                // Create a new object.
                SetObject obj = new();

                // Read this object's type.
                obj.Type = (ObjectType)reader.ReadUInt32();

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's rotation.
                obj.Rotation = reader.ReadQuaternion();

                // Calculate the size of this object's parameter table.
                uint parameterDataSize = objectLength - 0x24;

                // If this object has parameters, then read each byte as a parameter.
                for (int p = 0; p < parameterDataSize; p++)
                {
                    SetParameter param = new()
                    {
                        Data = reader.ReadByte(),
                        DataType = typeof(byte)
                    };
                    obj.Parameters.Add(param);
                }

                // Save this object.
                Data.Add(obj);

                // Jump back to our saved position for the next object.
                reader.JumpTo(pos);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.Wii)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the version byte.
            writer.Write((byte)version);

            // Write the amount of objects in this SET.
            writer.Write(Data.Count);

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Switch the writer to Big Endian if this is a Wii file.
            if (version == FormatVersion.Wii)
                writer.IsBigEndian = true;

            // Loop through each object to write the offset table.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add an offset for this object.
                writer.AddOffset($"object{i}Offset");

                // Write the size of this object.
                // TODO: Unhardcode this when parameter types are figured out.
                writer.Write(Data[i].Parameters.Count + 0x24);
            }

            // Loop through and write each object.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in the offset for this object.
                writer.FillOffset($"object{i}Offset");

                // Write the size of this object.
                // TODO: Unhardcode this when parameter types are figured out.
                writer.Write(Data[i].Parameters.Count + 0x24);

                // Write this object's type.
                writer.Write((uint)Data[i].Type);

                // Write this object's position.
                writer.Write(Data[i].Position);

                // Write this object's rotation.
                writer.Write(Data[i].Rotation);

                // Write each of this object's parameters.
                // TODO: Unhardcode this when parameter types are figured out.
                for (int p = 0; p < Data[i].Parameters.Count; p++)
                    writer.Write((byte)Data[i].Parameters[p].Data);
            }

            // Write the file size.
            writer.IsBigEndian = false;
            writer.BaseStream.Position = 0x05;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
