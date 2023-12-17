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
        // TODO: Find any objects from the Preview that work in the final (like the beta Wind Crowrang doors).
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
            Plane = 0x2B,
            Flame_Hyenard = 0x2E, // Unconfirmed, assumed from Preview SET.
            Cyber_Stone = 0x2F, // Unconfirmed, assumed based on placement.
            Mushadroyd = 0x30,
            Big_Ray = 0x31,
            Air_Force_Spawner_Door = 0x33,
            Soldier_Stonekong = 0x38, // Unconfirmed, assumed from Preview SET.
            Metall_S = 0x39,
            Cone_Metall = 0x3A, // Unconfirmed, assumed based on placement.
            Air_Force_End_Door = 0x3C,
            Hostage = 0x3E,
            Tornado_Tonion = 0x41, // Unconfirmed, assumed from Preview SET.
            Crash_Roadster = 0x42, // Unconfirmed, assumed based on placement.
            Proto_Ride = 0x43,
            Crimson_Palace_Platform = 0x45, // Unconfirmed, assumed based on placement.
            Splash_Warfly = 0x4B,
            Explosive_Container = 0x4C,
            Wind_Crowrang = 0x50, // Unconfirmed, assumed from Preview SET.
            Ride_Borski = 0x52, // Unconfirmed, assumed from Preview SET.
            Hellguarder = 0x53, // Unconfirmed, assumed based on placement.
            Vanishing_Gungaroo = 0x54, // Unconfirmed, assumed from Preview SET.
            Unknown_2 = 0x55, // Central Highway Enemy Wave Spawner? Only used once in O010000 and never again.
            Spotlight = 0x56,
            Scrap_Metall = 0x57,
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
            Item = 0x66, // Unconfirmed, assumed based on placement.
            Null = 0xFFFFFFFF // Assumed, as every SET ends with one of these, though some have one in the actual object table itself?
        }

        // Classes for this format.
        public class SetObject
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

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
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this, seems to have some impact on spawning?
            /// TODO: Determine if this is actually a float.
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

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
            public float UnknownFloat_4 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

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
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_7 { get; set; }

            /// <summary>
            /// This object's rotation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

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
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_12 { get; set; }

            /// <summary>
            /// The object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_13 { get; set; }
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
                // Create a new object and read its data.
                SetObject obj = new();
                obj.UnknownUInt32_1 = reader.ReadUInt32();
                obj.ObjectType = (ObjectType)reader.ReadUInt32();
                obj.Behaviour = reader.ReadUInt32();
                obj.UnknownUInt32_2 = reader.ReadUInt32();
                obj.UnknownFloat_1 = reader.ReadSingle();
                obj.UnknownFloat_2 = reader.ReadSingle();
                obj.UnknownFloat_3 = reader.ReadSingle();
                obj.UnknownUInt32_3 = reader.ReadUInt32();
                obj.UnknownUInt32_4 = reader.ReadUInt32();
                obj.UnknownFloat_4 = reader.ReadSingle();
                obj.UnknownUInt32_5 = reader.ReadUInt32();
                obj.UnknownFloat_5 = reader.ReadSingle();
                obj.UnknownFloat_6 = reader.ReadSingle();
                obj.UnknownFloat_7 = reader.ReadSingle();
                obj.Rotation = reader.ReadVector3();
                obj.UnknownFloat_8 = reader.ReadSingle();
                obj.UnknownFloat_9 = reader.ReadSingle();
                obj.UnknownFloat_10 = reader.ReadSingle();
                obj.UnknownFloat_11 = reader.ReadSingle();
                obj.UnknownFloat_12 = reader.ReadSingle();

                if (!previewSet)
                    if (reader.ReadUInt32() != obj.UnknownUInt32_2)
                        Debugger.Break();

                obj.Position = reader.ReadVector3();
                obj.UnknownFloat_13 = reader.ReadSingle();

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
            foreach (SetObject obj in Data)
            {
                writer.Write(obj.UnknownUInt32_1);
                writer.Write((uint)obj.ObjectType);
                writer.Write(obj.Behaviour);
                writer.Write(obj.UnknownUInt32_2);
                writer.Write(obj.UnknownFloat_1);
                writer.Write(obj.UnknownFloat_2);
                writer.Write(obj.UnknownFloat_3);
                writer.Write(obj.UnknownUInt32_3);
                writer.Write(obj.UnknownUInt32_4);
                writer.Write(obj.UnknownFloat_4);
                writer.Write(obj.UnknownUInt32_5);
                writer.Write(obj.UnknownFloat_5);
                writer.Write(obj.UnknownFloat_6);
                writer.Write(obj.UnknownFloat_7);
                writer.Write(obj.Rotation);
                writer.Write(obj.UnknownFloat_8);
                writer.Write(obj.UnknownFloat_9);
                writer.Write(obj.UnknownFloat_10);
                writer.Write(obj.UnknownFloat_11);
                writer.Write(obj.UnknownFloat_12);
                if (!previewSet) writer.Write(obj.UnknownUInt32_2);
                writer.Write(obj.Position);
                writer.Write(obj.UnknownFloat_13);
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
