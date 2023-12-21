using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7
{
    // TODO: Figure out how the objects use the various values and make a HSON solution.
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, bool previewSet = false, bool export = false)
        {
            Load(filepath, previewSet);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.rockmanx7.stageentitytable.json", Data);
        }

        // Classes for this format.
        // TODO: Identify the unknown objects.
        // TODO: Confirm all the assumptions.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ObjectType : uint
        {
            Surveillance_Radar_Purple = 0x00,
            Surveillance_Radar_Green = 0x01,
            Radarroid = 0x02,
            Guard_Fan = 0x03,
            Ball_De_Voux_Normal_Type = 0x04,
            Ball_De_Voux_Shot_Type = 0x05,
            Kyuun_B = 0x06,
            Guard_Door = 0x07,
            Bee_Blader = 0x08,
            Wall_Blaster = 0x09,
            Background_Bee_Blader = 0x0A,
            Wall_Jump_Crusher = 0x0C,
            Red_Guardian = 0x0D, // Unconfirmed, assumed based on placement.
            Yellow_Guardian = 0x0E, // Unconfirmed, assumed based on placement.
            Dragon_Blaster = 0x0F, // Unconfirmed, assumed based on placement.
            Flyer = 0x10,
            Runnerbomb = 0x11,
            Skelebat = 0x12,
            Antenna_Shield = 0x13, // Unconfirmed, assumed based on placement.
            Guillotine = 0x14, // Unconfirmed, assumed based on placement.
            Large_Mine = 0x15,
            Mega_Tortoise = 0x16,
            Unknown_1 = 0x17, // Only in Crimson Palace, which I don't have a save in.
            Rolling_Stone = 0x18,
            Pastegunner = 0x19,
            Ape_Stone = 0x1A,
            Ape_Sting = 0x1B, // Unconfirmed, assumed based on placement.
            Yoku_Block = 0x1C,
            Aperoid = 0x1D, // Unconfirmed, assumed based on placement.
            Bounding = 0x1E,
            Blockape = 0x1F,
            Ruinsman = 0x20,
            Ape_Stone_Miniboss = 0x26, // Unconfirmed, assumed based on placement.
            Bomb = 0x29, // Crashed when changing every object to this, but the bomb counter did show up with a much higher value.
            Birdy = 0x2A, // Only actually placed in a Preview SET, spawned by other means in the final.
            Plane = 0x2B,
            Flame_Hyenard = 0x2E, // Unconfirmed, assumed from Preview SET.
            Cyber_Stone = 0x2F, // Unconfirmed, assumed based on placement.
            Mushadroyd = 0x30,
            Big_Ray = 0x31,
            Unknown_Preview_1 = 0x32,
            Air_Force_Spawner_Door = 0x33,
            Air_Force_Door = 0x34, // Only exists in the Preview SET, works in final.
            Air_Force_Destructable_Door = 0x35, // Only exists in the Preview SET, works in final.
            Soldier_Stonekong = 0x38, // Unconfirmed, assumed from Preview SET.
            Metall_S = 0x39,
            Cone_Metall = 0x3A, // Unconfirmed, assumed based on placement.
            Air_Force_End_Door = 0x3C,
            Hostage = 0x3E,
            Unknown_Preview_2 = 0x3F,
            Unknown_Preview_3 = 0x40,
            Tornado_Tonion = 0x41, // Unconfirmed, assumed from Preview SET.
            Crash_Roadster = 0x42, // Unconfirmed, assumed based on placement.
            Proto_Ride = 0x43,
            Crimson_Palace_Platform = 0x45, // Unconfirmed, assumed based on placement.
            Unknown_Preview_4 = 0x47,
            Unknown_Preview_5 = 0x49,
            Unknown_Preview_6 = 0x4A,
            Unknown_Preview_7 = 0x4D,
            Splash_Warfly = 0x4B,
            Explosive_Container = 0x4C,
            Wind_Crowrang = 0x50, // Unconfirmed, assumed from Preview SET.
            Ride_Boarski = 0x52, // Unconfirmed, assumed from Preview SET.
            Hellguarder = 0x53, // Unconfirmed, assumed based on placement.
            Vanishing_Gungaroo = 0x54, // Unconfirmed, assumed from Preview SET.
            Unknown_2 = 0x55, // Central Highway Enemy Wave Spawner? Only used once in O010000 and never again.
            Spotlight = 0x56,
            Scrap_Metall = 0x57,
            Unknown_Preview_8 = 0x58,
            Gun_Volt = 0x59,
            Teleporter = 0x5A, // Unconfirmed, assumed based on placement.
            Boss_Rush_Warp = 0x5C, // Unconfirmed, assumed based on placement.
            Unknown_3 = 0x5E, // Only found in Soul Asylum, which I don't have a save in.
            Dr_Light_Capsule = 0x5F,
            Tunnel_Base_Gate = 0x60,
            Tunnel_Base_Door = 0x61,
            Crimson_Palace_Teleporter = 0x63, // Unconfirmed, assumed based on placement.
            Unknown_4 = 0x64, // Only in whatever Stage 14 is, Sigma?
            Unknown_5 = 0x65, // Only in whatever Stage 14 is, Sigma?
            Item = 0x66,
            Ride_Armour = 0xFFFFFFFF
        }

        // TODO: Figure out how each of these affect how the object spawns.
        public enum SpawnBehaviour : byte
        {
            Unknown_1 = 0x01, // Seems to differ based on the object type?
            Unknown_2 = 0x02,
            Unknown_3 = 0x03,
            Unknown_4 = 0x04,
            Unknown_5 = 0x05,
            Unknown_6 = 0x06,
            Unknown_7 = 0x07,
            Unknown_8 = 0x08
        }

        // Classes for this format.
        public class SetObject
        {
            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Used values are 0, 1, 2 or 5. Fiddling with this can crash the game.
            /// </summary>
            public byte UnknownByte_1 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Only ever either 0 or 0xFF. The next two bytes match this value.
            /// </summary>
            public byte UnknownByte_2 { get; set; }

            /// <summary>
            /// The type of this object.
            /// </summary>
            public ObjectType ObjectType { get; set; }

            /// <summary>
            /// This object's behavioural type.
            /// TODO: Is this the correct way to describe this?
            /// </summary>
            public uint Behaviour { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Only ever either 0 or 0xFF.
            /// </summary>
            public byte UnknownByte_3 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Only ever either 0 or 0xFF. The next byte matches this value.
            /// </summary>
            public byte UnknownByte_4 { get; set; }

            /// <summary>
            /// Controls spawn distance in some way.
            /// TODO: How does SpawnRadius impact this?
            /// </summary>
            public SpawnBehaviour SpawnBehaviour { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Only ever either 0 or 0xCC.
            /// </summary>
            public byte UnknownByte_5 { get; set; }

            /// <summary>
            /// The radius from this object that the player must be in for it to spawn.
            /// TODO: Is this ACTUALLY a radius?
            /// TODO: How does SpawnBehaviour impact this?
            /// </summary>
            public float SpawnRadius { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Usually set to 10, but a few objects (listed below) can have it set to various different values instead.
                /// Ball_De_Voux_Normal_Type
                /// Ball_De_Voux_Shot_Type
                /// Blockape
                /// Cone_Metall
                /// Crimson_Palace_Teleporter
                /// Cyber_Stone
                /// Dragon_Blaster
                /// Explosive_Container
                /// Guard_Door
                /// Guard_Fan
                /// Hellguarder
                /// Hostage
                /// Item
                /// Large_Mine
                /// Mega_Tortoise
                /// Metall_S
                /// Pastegunner
                /// Plane
                /// Proto_Ride
                /// Red_Guardian
                /// Ride_Armour
                /// Rolling_Stone
                /// Ruinsman
                /// Runnerbomb
                /// Scrap_Metall
                /// Tunnel_Base_Door
                /// Tunnel_Base_Gate
                /// Unknown_1
                /// Unknown_5
                /// Wall_Blaster
                /// Yellow_Guardian
            /// </summary>
            public float UnknownFloat_1 { get; set; } = 10f;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Usually set to 1200, but a few objects (listed below) can have it set to various different values instead.
                /// Boss_Rush_Warp
                /// Mushadroyd
                /// Runnerbomb
                /// Unknown_3
                /// Unknown_4
                /// Wall_Blaster
            /// </summary>
            public uint UnknownUInt32_2 { get; set; } = 1200;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Usually set to 0, but a few objects (listed below) can have it set to various different values instead.
                /// Antenna_Shield
                /// Ape_Stone
                /// Ball_De_Voux_Shot_Type
                /// Bee_Blader
                /// Bomb
                /// Bounding
                /// Crimson_Palace_Teleporter
                /// Dr_Light_Capsule
                /// Gun_Volt
                /// Hostage
                /// Item
                /// Pastegunner
                /// Runnerbomb
                /// Unknown_1
                /// Yellow_Guardian
            /// </summary>
            public uint UnknownUInt32_3 { get; set; } = 0;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Usually set to 0, but a few objects (listed below) can have it set to various different values instead.
                /// Bee_Blader
                /// Bomb
                /// Crimson_Palace_Teleporter
                /// Dr_Light_Capsule
                /// Hostage
                /// Runnerbomb
                /// Tunnel_Base_Gate
                /// Unknown_1
                /// Unknown_2
            /// </summary>
            public uint UnknownUInt32_4 { get; set; } = 0;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Usually set to 0, but a few Ball_De_Voux_Normal_Types use 1, Proto_Rides can also use 1, 3 and 4, and Wall_Blasters can use 1, 2, 3 and 99(?!).
            /// </summary>
            public uint UnknownUInt32_5 { get; set; } = 0;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Various objects set this to a variety of values.
            /// </summary>
            public float UnknownFloat_3 { get; set; } = 0f;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Usually set to 0, but a few objects (listed below) have different values instead.
                /// Crimson_Palace_Teleporter (can use 0.12)
                /// Large_Mine (can use 90)
                /// Pastegunner (can use -90, 90 and 180)
                /// Spotlight (can use a variety of different values)
            /// </summary>
            public float UnknownFloat_4 { get; set; } = 0f;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? It's always set to 0, except for a few Pastegunners (which use -80 or -90) and the Spotlights (which use a range of values up to and including 0.5).
            /// </summary>
            public float UnknownFloat_5 { get; set; } = 0f;

            /// <summary>
            /// This object's rotation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? Usually set to 0, but a few objects (listed below) have it set to 1 instead.
                /// Air_Force_Door (may or may not be always?)
                /// Air_Force_End_Door
                /// Air_Force_Spawner_Door
                /// Big_Ray
                /// Bomb
                /// Dr_Light_Capsule (not always)
                /// Plane
                /// Runnerbomb (not always)
                /// Spotlight
                /// Tunnel_Base_Door
                /// Tunnel_Base_Gate
                /// Unknown_Preview_5 (may or may not be always?)
            /// </summary>
            public float UnknownFloat_6 { get; set; } = 0f;

            /// <summary>
            /// An unknown Vector3.
            /// TODO: Do the three floats here all work together for every object or just some? If not, then this should be split again.
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? It's always set to 0, except for a single Bee Blader in Central Highway and some Planes in Air Force Act 1 using 1 instead.
            /// </summary>
            public float UnknownFloat_7 { get; set; } = 0f;

            /// <summary>
            /// The object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this? It's always set to 1, except for a single Big Ray in Air Force Act 2 using 180 instead.
            /// </summary>
            public float UnknownFloat_8 { get; set; } = 1f;

            public override string ToString() => ObjectType.ToString();
        }

        // Actual data presented to the end user.
        public List<SetObject> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="previewSet">Whether this file is from the Rockman X7 Preview</param>
        public void Load(string filepath, bool previewSet = false)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's object count.
            uint objectCount = reader.ReadUInt32();

            // Read each object except for the last one, which has every value set to 0xFFFFFFFF.
            for (int objectIndex = 0; objectIndex < objectCount - 1; objectIndex++)
            {
                // Create a new object.
                SetObject obj = new();

                // Read this object's first unknown byte.
                obj.UnknownByte_1 = reader.ReadByte();

                // Read this object's second unknown byte.
                obj.UnknownByte_2 = reader.ReadByte();

                // Check my assumption that the next two bytes match the second.
                if (reader.ReadByte() != obj.UnknownByte_2) Debugger.Break();
                if (reader.ReadByte() != obj.UnknownByte_2) Debugger.Break();

                // Read this object's type.
                obj.ObjectType = (ObjectType)reader.ReadUInt32();

                // Read this object's behaviour value.
                obj.Behaviour = reader.ReadUInt32();

                // Read this object's third unknown byte.
                obj.UnknownByte_3 = reader.ReadByte();
                
                // Check the next byte to make sure it matches my assumption that it's always 0.
                if (reader.ReadByte() != 0) Debugger.Break();

                // Read this object's fourth unknown byte.
                obj.UnknownByte_4 = reader.ReadByte();

                // Check the next byte to make sure it matches my assumption that it matches UnknownByte_4.
                if (reader.ReadByte() != obj.UnknownByte_4) Debugger.Break();

                // Check this object's index to make sure it matches my assumption (that this value is i + 7).
                if (reader.ReadByte() != objectIndex + 7) Debugger.Break();

                // Read this object's spwaning behaviour.
                obj.SpawnBehaviour = (SpawnBehaviour)reader.ReadByte();

                // Read this object's fifth unknown byte.
                obj.UnknownByte_5 = reader.ReadByte();

                // Check this object's extra unknown byte to make sure it matches my assumption (that this value always matches UnknownByte_1).
                if (reader.ReadByte() != obj.UnknownByte_5) Debugger.Break();

                // Read this object's spawn radius.
                obj.SpawnRadius = reader.ReadSingle();

                // Read this object's first unknown floating point value.
                obj.UnknownFloat_1 = reader.ReadSingle();

                // Read this object's second unknown integer value.
                obj.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this object's third unknown integer value.
                obj.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this object's fourth unknown integer value.
                obj.UnknownUInt32_4 = reader.ReadUInt32();

                // Read this object's fifth unknown integer value.
                obj.UnknownUInt32_5 = reader.ReadUInt32();

                // Read this object's third unknown floating point value.
                obj.UnknownFloat_3 = reader.ReadSingle();

                // Read this object's fourth unknown floating point value.
                obj.UnknownFloat_4 = reader.ReadSingle();

                // Read this object's fifth unknown floating point value.
                obj.UnknownFloat_5 = reader.ReadSingle();

                // Read this object's rotation.
                obj.Rotation = reader.ReadVector3();

                // Read this object's sixth unknown floating point value.
                obj.UnknownFloat_6 = reader.ReadSingle();

                // Read this object's unknown Vector3.
                obj.UnknownVector3_1 = reader.ReadVector3();

                // Read this object's seventh unknown floating point value.
                obj.UnknownFloat_7 = reader.ReadSingle();

                // If this SET isn't from the Rockman X7 Preview Trial, then check the extra bytes here to make sure it matches my assumption (that these bytes always match UnknownByte_3 and UnknownByte_4 and the two other values around it).
                if (!previewSet)
                {
                    if (reader.ReadByte() != obj.UnknownByte_3) Debugger.Break();
                    if (reader.ReadByte() != 0) Debugger.Break();
                    if (reader.ReadByte() != obj.UnknownByte_4) Debugger.Break();
                    if (reader.ReadByte() != obj.UnknownByte_4) Debugger.Break();
                }

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's eighth unknown floating point value.
                obj.UnknownFloat_8 = reader.ReadSingle();

                // Save this object.
                Data.Add(obj);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="previewSet">Whether this file is from the Rockman X7 Preview</param>
        public void Save(string filepath, bool previewSet = false)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write this file's object count (including the terminator(?) object)
            writer.Write(Data.Count + 1);

            // Write each object's data.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this object's first unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_1);

                // Write three copies of this object's second unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_2);
                writer.Write(Data[dataIndex].UnknownByte_2);
                writer.Write(Data[dataIndex].UnknownByte_2);

                // Write this object's type.
                writer.Write((uint)Data[dataIndex].ObjectType);

                // Write this object's behaviour value.
                writer.Write(Data[dataIndex].Behaviour);

                // Write this object's third unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_3);

                // Write a 0 byte.
                writer.Write((byte)0);

                // Write two copies of this object's fourth unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_4);
                writer.Write(Data[dataIndex].UnknownByte_4);

                // Write this object's index.
                writer.Write((byte)(dataIndex + 7));

                // Write this object's spwaning behaviour.
                writer.Write((byte)Data[dataIndex].SpawnBehaviour);

                // Write two copies of this object's fifth unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_5);
                writer.Write(Data[dataIndex].UnknownByte_5);

                // Write this object's spawn radius.
                writer.Write(Data[dataIndex].SpawnRadius);

                // Write this object's first unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_1);

                // Write this object's second unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_2);

                // Write this object's third unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_3);

                // Write this object's fourth unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_4);

                // Write this object's fifth unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_5);

                // Write this object's third unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_3);

                // Write this object's fourth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_4);

                // Write this object's fifth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_5);

                // Write this object's rotation.
                writer.Write(Data[dataIndex].Rotation);

                // Write this object's sixth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_6);

                // Write this object's unknown Vector3.
                writer.Write(Data[dataIndex].UnknownVector3_1);

                // Write this object's seventh unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_7);

                // If this SET isn't from the Rockman X7 Preview Trial, then write another copy of UnknownByte_3, a 0, and two copies of UnknownByte_4.
                if (!previewSet)
                {
                    writer.Write(Data[dataIndex].UnknownByte_3);
                    writer.Write((byte)0);
                    writer.Write(Data[dataIndex].UnknownByte_4);
                    writer.Write(Data[dataIndex].UnknownByte_4);
                }

                // Write this object's position.
                writer.Write(Data[dataIndex].Position);

                // Write this object's eighth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_8);
            }

            // Write the terminator(?) object.
            if (!previewSet)
                for (int i = 0; i < 0x6C; i++)
                    writer.Write((byte)0xFF);
            else
                for (int i = 0; i < 0x68; i++)
                    writer.Write((byte)0xFF);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
