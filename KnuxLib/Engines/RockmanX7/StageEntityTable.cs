namespace KnuxLib.Engines.RockmanX7
{
    // TODO: This is basically all unknown, mess around and find out what the values are.
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
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
            /// TODO: Find out what each number corrosponds to.
            /// </summary>
            public uint ObjectType { get; set; }

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
            /// TODO: What is this?
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
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_13 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_14 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_15 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_6 { get; set; }

            /// <summary>
            /// The object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_16 { get; set; }
        }

        // Actual data presented to the end user.
        public List<SetObject> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's object count.
            uint objectCount = reader.ReadUInt32();

            // Read each object except for the last one, which has every value set to 0xFFFFFFFF.
            for (int i = 0; i < objectCount - 1; i++)
            {
                // Create a new object and read its data.
                SetObject obj = new()
                {
                    UnknownUInt32_1 = reader.ReadUInt32(),
                    ObjectType = reader.ReadUInt32(),
                    Behaviour = reader.ReadUInt32(),
                    UnknownUInt32_2 = reader.ReadUInt32(),
                    UnknownFloat_1 = reader.ReadSingle(),
                    UnknownFloat_2 = reader.ReadSingle(),
                    UnknownFloat_3 = reader.ReadSingle(),
                    UnknownUInt32_3 = reader.ReadUInt32(),
                    UnknownUInt32_4 = reader.ReadUInt32(),
                    UnknownFloat_4 = reader.ReadSingle(),
                    UnknownUInt32_5 = reader.ReadUInt32(),
                    UnknownFloat_5 = reader.ReadSingle(),
                    UnknownFloat_6 = reader.ReadSingle(),
                    UnknownFloat_7 = reader.ReadSingle(),
                    UnknownFloat_8 = reader.ReadSingle(),
                    UnknownFloat_9 = reader.ReadSingle(),
                    UnknownFloat_10 = reader.ReadSingle(),
                    UnknownFloat_11 = reader.ReadSingle(),
                    UnknownFloat_12 = reader.ReadSingle(),
                    UnknownFloat_13 = reader.ReadSingle(),
                    UnknownFloat_14 = reader.ReadSingle(),
                    UnknownFloat_15 = reader.ReadSingle(),
                    UnknownUInt32_6 = reader.ReadUInt32(),
                    Position = reader.ReadVector3(),
                    UnknownFloat_16 = reader.ReadSingle()
                };

                // Save this object.
                Data.Add(obj);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write this file's object count (including the terminator(?) object)
            writer.Write(Data.Count + 1);
            
            // Write each object's data.
            foreach (SetObject obj in Data)
            {
                writer.Write(obj.UnknownUInt32_1);
                writer.Write(obj.ObjectType);
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
                writer.Write(obj.UnknownFloat_8);
                writer.Write(obj.UnknownFloat_9);
                writer.Write(obj.UnknownFloat_10);
                writer.Write(obj.UnknownFloat_11);
                writer.Write(obj.UnknownFloat_12);
                writer.Write(obj.UnknownFloat_13);
                writer.Write(obj.UnknownFloat_14);
                writer.Write(obj.UnknownFloat_15);
                writer.Write(obj.UnknownUInt32_6);
                writer.Write(obj.Position);
                writer.Write(obj.UnknownFloat_16);
            }

            // Write the terminator(?) object.
            for (int i = 0; i < 0x6C; i++)
                writer.Write((byte)0xFF);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
