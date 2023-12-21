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
            /// An unknown integer value.
            /// TODO: What is this? Only ever 0x000000FF or 0xFFFF00FF. Four seperate bytes maybe?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// Controls spawn distance in some way.
            /// TODO: Figure out how the values affect spawning.
            /// </summary>
            public SpawnBehaviour SpawnBehaviour { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this? Only ever either 0 or 0xCC.
            /// </summary>
            public byte UnknownByte_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_4 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_4 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_5 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_6 { get; set; }

            /// <summary>
            /// This object's rotation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_7 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_8 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_9 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_10 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_11 { get; set; }

            /// <summary>
            /// The object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_12 { get; set; }

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

                // Read this object's second unknown integer value.
                obj.UnknownUInt32_2 = reader.ReadUInt32();

                // Check this object's index to make sure it matches my assumption (that this value is i + 7).
                if (reader.ReadByte() != objectIndex + 7) Debugger.Break();

                // Read this object's spwaning behaviour.
                obj.SpawnBehaviour = (SpawnBehaviour)reader.ReadByte();

                // Read this object's third unknown byte.
                obj.UnknownByte_3 = reader.ReadByte();

                // Check this object's extra unknown byte to make sure it matches my assumption (that this value always matches UnknownByte_1).
                if (reader.ReadByte() != obj.UnknownByte_3) Debugger.Break();

                // Read this object's first unknown floating point value.
                obj.UnknownFloat_1 = reader.ReadSingle();

                // Read this object's second unknown floating point value.
                obj.UnknownFloat_2 = reader.ReadSingle();

                // Read this object's third unknown integer value.
                obj.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this object's fourth unknown integer value.
                obj.UnknownUInt32_4 = reader.ReadUInt32();

                // Read this object's third unknown floating point value.
                obj.UnknownFloat_3 = reader.ReadSingle();

                // Read this object's fifth unknown integer value.
                obj.UnknownUInt32_5 = reader.ReadUInt32();

                // Read this object's fourth unknown floating point value.
                obj.UnknownFloat_4 = reader.ReadSingle();

                // Read this object's fifth unknown floating point value.
                obj.UnknownFloat_5 = reader.ReadSingle();

                // Read this object's sixth unknown floating point value.
                obj.UnknownFloat_6 = reader.ReadSingle();

                // Read this object's rotation.
                obj.Rotation = reader.ReadVector3();

                // Read this object's seventh unknown floating point value.
                obj.UnknownFloat_7 = reader.ReadSingle();

                // Read this object's eighth unknown floating point value.
                obj.UnknownFloat_8 = reader.ReadSingle();

                // Read this object's ninth unknown floating point value.
                obj.UnknownFloat_9 = reader.ReadSingle();

                // Read this object's tenth unknown floating point value.
                obj.UnknownFloat_10 = reader.ReadSingle();

                // Read this object's eleventh unknown floating point value.
                obj.UnknownFloat_11 = reader.ReadSingle();

                // If this SET isn't from the Rockman X7 Preview Trial, then check the extra value here to make sure it matches my assumption (that this value always matches UnknownUInt32_2).
                if (!previewSet)
                    if (reader.ReadUInt32() != obj.UnknownUInt32_2)
                        Debugger.Break();

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's twelveth unknown floating point value.
                obj.UnknownFloat_12 = reader.ReadSingle();

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

                // Write three copies of this object's first unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_2);
                writer.Write(Data[dataIndex].UnknownByte_2);
                writer.Write(Data[dataIndex].UnknownByte_2);

                // Write this object's type.
                writer.Write((uint)Data[dataIndex].ObjectType);

                // Write this object's behaviour value.
                writer.Write(Data[dataIndex].Behaviour);

                // Write this object's second unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_2);

                // Write this object's index.
                writer.Write((byte)(dataIndex + 7));

                // Write this object's spwaning behaviour.
                writer.Write((byte)Data[dataIndex].SpawnBehaviour);

                // Write two copies of this object's third unknown byte.
                writer.Write(Data[dataIndex].UnknownByte_3);
                writer.Write(Data[dataIndex].UnknownByte_3);

                // Write this object's first unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_1);

                // Write this object's second unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_2);

                // Write this object's third unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_3);

                // Write this object's fourth unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_4);

                // Write this object's third unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_3);

                // Write this object's fifth unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_5);

                // Write this object's fourth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_4);

                // Write this object's fifth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_5);

                // Write this object's sixth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_6);

                // Write this object's rotation.
                writer.Write(Data[dataIndex].Rotation);

                // Write this object's seventh unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_7);

                // Write this object's eighth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_8);

                // Write this object's ninth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_9);

                // Write this object's tenth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_10);

                // Write this object's eleventh unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_11);

                // If this SET isn't from the Rockman X7 Preview Trial, then write another copy of UnknownUInt32_2.
                if (!previewSet) writer.Write(Data[dataIndex].UnknownUInt32_2);

                // Write this object's position.
                writer.Write(Data[dataIndex].Position);

                // Write this object's twelveth unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_12);
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
