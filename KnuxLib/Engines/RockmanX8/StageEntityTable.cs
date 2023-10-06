namespace KnuxLib.Engines.RockmanX8
{
    // TODO: Figure out all the unknown data.
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, FormatVersion version = FormatVersion.LegacyCollection, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.rockmanx8.stageentitytable.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            Original = 0,
            LegacyCollection = 1
        }

        public class SetObject
        {
            /// <summary>
            /// This object's rotation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// This object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// This object's type, seemingly always Prm then four numbers.
            /// </summary>
            public string Type { get; set; } = "Prm????";

            /// <summary>
            /// This object's parameters.
            /// TODO: See if these actually are parameters of some type. Haven't messed with the values here yet.
            /// </summary>
            public List<SetParameter> Parameters { get; set; } = new();

            public override string ToString() => Type;
        }

        // Actual data presented to the end user.
        public List<SetObject> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.LegacyCollection)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a Legacy Collection SET, then read the extra twelve bytes it added.
            if (version == FormatVersion.LegacyCollection)
            {
                // Read the OSE signature.
                reader.ReadSignature(0x03, "OSE");
                reader.FixPadding(0x04);

                // Skip an unknown value of 0x01.
                reader.JumpAhead(0x04);

                // Read the size of this file (minus this header).
                uint dataSize = reader.ReadUInt32();
            }

            // Read this file's object count.
            uint objectCount = reader.ReadUInt32();

            // Skip 0x3C bytes that are always 0.
            reader.JumpAhead(0x3C);

            // Loop through each object.
            for (int i = 0; i < objectCount; i++)
            {
                // Set up a new object entry.
                SetObject obj = new();

                // Read this object's rotation.
                obj.Rotation = reader.ReadVector3();

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's first unknown value.
                obj.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this object's second unknown value.
                obj.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this object's third unknown value.
                obj.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this object's type string.
                obj.Type = reader.ReadNullPaddedString(0x08);

                // Read this object's parameters.
                for (int p = 0; p < 0x20; p++)
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
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.LegacyCollection)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If this is a Legacy Collection SET, then write the extra twelve bytes it added.
            if (version == FormatVersion.LegacyCollection)
            {
                // Write the OSE signature.
                writer.WriteNullPaddedString("OSE", 0x04);

                // Write an unknown value of 0.
                writer.Write(0x01);

                // Write a placeholder for this file's data size.
                writer.Write("SIZE");
            }

            // Write this file's object count.
            writer.Write(Data.Count);

            // Write 0x3C null bytes.
            writer.WriteNulls(0x3C);

            // Loop through each object.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write this object's rotation.
                writer.Write(Data[i].Rotation);

                // Write an unknown value of 0.
                writer.Write(0x00);

                // Write this object's position.
                writer.Write(Data[i].Position);

                // Write this object's first unknown value.
                writer.Write(Data[i].UnknownUInt32_1);

                // Write this object's second unknown value.
                writer.Write(Data[i].UnknownUInt32_2);

                // Write this object's third unknown value.
                writer.Write(Data[i].UnknownUInt32_3);

                // Write this object's type string.
                writer.WriteNullPaddedString(Data[i].Type, 0x08);

                // Write each of this object's parameters.
                for (int p = 0; p < Data[i].Parameters.Count; p++)
                    writer.Write((byte)Data[i].Parameters[p].Data);
            }

            // If this is a Legacy Collection SET, then calculate and write the data size.
            if (version == FormatVersion.LegacyCollection)
            {
                writer.BaseStream.Position = 0x08;
                writer.Write((uint)(writer.BaseStream.Length - 0x0C));
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
