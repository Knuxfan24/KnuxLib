namespace KnuxLib.Engines.Storybook
{
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, StageEntityTableItems? items = null, bool export = false)
        {
            Load(filepath, items);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.stageentitytable.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// This SET's Signature, can vary (usually changing the number at the end to match with the file's).
            /// </summary>
            public string Signature { get; set; } = "STP0";

            /// <summary>
            /// A list of the Objects in this SET.
            /// </summary>
            public List<SetObject> Objects { get; set; } = new();

            public override string ToString() => Signature;
        }

        public class SetObject
        {
            /// <summary>
            /// This object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this, does this work with UnknownUInt32_2 and UnknownUInt32_3 to form a BAMS rotation?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this, does this work with UnknownUInt32_1 and UnknownUInt32_3 to form a BAMS rotation?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this, does this work with UnknownUInt32_1 and UnknownUInt32_2 to form a BAMS rotation?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_1 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_2 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_3 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_4 { get; set; }

            /// <summary>
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_5 { get; set; }
                               
            /// <summary>
            /// How far away this object can be before it is loaded and displayed.
            /// </summary>
            public byte DrawDistance { get; set; }

            /// <summary>
            /// The ID of this object in the StageEntityTableItems object list.
            /// </summary>
            public byte ObjectID { get; set; }

            /// <summary>
            /// The table of this object in the StageEntityTableItems object list.
            /// </summary>
            public byte TableID { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_4 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_5 { get; set; }

            /// <summary>
            /// A list of this object's parameters.
            /// </summary>
            public List<SetParameter>? Parameters { get; set; }

            /// <summary>
            /// This object's type, as determined from the ID and Table.
            /// This is NOT actually a thing the SET file has, and is only set by the Load function having a StageEntityTableItems object passed to it.
            /// </summary>
            public string? Type { get; set; }

            public override string ToString() => Type;
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
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, StageEntityTableItems? items = null)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature, as it can vary depending on part, we store it rather than thrown an exception if it's different.
            Data.Signature = reader.ReadNullPaddedString(0x04);

            // Read the number of objects in this SET.
            uint objectCount = reader.ReadUInt32();

            // Read the number of parameters in this SET.
            uint parameterCount = reader.ReadUInt32();

            // Read the length of this SET's parameter data in bytes.
            uint parameterDataTableLength = reader.ReadUInt32();

            // Calculate the offset to this SET's parameter table.
            uint parameterTableOffset = (objectCount * 0x30) + 0x10;

            // Calculate the offset to this SET's parameter data table.
            uint parameterDataTableOffset = parameterTableOffset + (parameterCount * 0x8);

            // Loop through this SET's object table.
            for (int i = 0; i < objectCount; i++)
            {
                // Create a new object.
                SetObject obj = new();

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's first unknown integer value.
                obj.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this object's second unknown integer value.
                obj.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this object's third unknown integer value.
                obj.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this object's first unknown byte value.
                obj.UnknownByte_1 = reader.ReadByte();

                // Read this object's second unknown byte value.
                obj.UnknownByte_2 = reader.ReadByte();

                // Read this object's third unknown byte value.
                obj.UnknownByte_3 = reader.ReadByte();

                // Read this object's fourth unknown byte value.
                obj.UnknownByte_4 = reader.ReadByte();

                // Read this object's fifth unknown byte value.
                obj.UnknownByte_5 = reader.ReadByte();

                // Read this object's draw distance.
                obj.DrawDistance = reader.ReadByte();

                // Read this object's ID in the item table.
                obj.ObjectID = reader.ReadByte();

                // Read this object's table in the item table.
                obj.TableID = reader.ReadByte();

                // Read this object's fourth unknown integer value.
                obj.UnknownUInt32_4 = reader.ReadUInt32();

                // Skip an unknown value of 0x00.
                reader.JumpAhead(0x04);

                // Read this object's fifth unknown integer value.
                obj.UnknownUInt32_5 = reader.ReadUInt32();

                // Read the index of this object's parameters.
                uint parameterIndex = reader.ReadUInt32();

                // If we've loaded a StageEntityTableItems object, then find this object's name from it.
                if (items != null)
                    foreach (var item in items.Data.Objects)
                        if ((item.ObjectID == obj.ObjectID) && (item.TableID == obj.TableID))
                            obj.Type = item.Name;

                // If this object's first unknown byte value is NOT 0x01, then read this object's parameters.
                if (obj.UnknownByte_1 != 0x01)
                {
                    // Initialise this object's parameter table.
                    obj.Parameters = new();

                    // Save our position in the object table.
                    long pos = reader.BaseStream.Position;

                    // Jump to the parameter table.
                    reader.JumpTo(parameterTableOffset);

                    // Set up a value to calculate how far into the data table this object's parameters begin.
                    uint parameterOffset = 0;

                    // Loop through based on this object's parameter index.
                    for (int index = 0; index < parameterIndex; index++)
                    {
                        // Skip two bytes that are always 01 00.
                        // TODO: Verify.
                        reader.JumpAhead(0x02);

                        // Add the length of a previous object's parameter data to the offset value.
                        parameterOffset += reader.ReadByte();

                        // Skip five bytes that are always 00 00 00 00 00.
                        // TODO: Verify.
                        reader.JumpAhead(0x05);
                    }

                    // Skip two bytes that are always 01 00.
                    // TODO: Verify.
                    reader.JumpAhead(0x02);

                    // Read the length of this object's parameter data in bytes.
                    byte objectParameterLength = reader.ReadByte();

                    // Jump to the parameter data table, adding our offset value.
                    reader.JumpTo(parameterDataTableOffset + parameterOffset);

                    // Read each of this object's parameters.
                    // TODO: Unhardcode this once a template solution is figured out.
                    for (byte parameter = 0; parameter < objectParameterLength; parameter++)
                    {
                        SetParameter param = new()
                        {
                            Data = reader.ReadByte(),
                            DataType = typeof(byte)
                        };
                        obj.Parameters.Add(param);
                    }

                    // Jump back to our saved position for the next object.
                    reader.JumpTo(pos);
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
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write this SET's signature.
            writer.Write(Data.Signature);

            // Write the amount of objects in this SET.
            writer.Write(Data.Objects.Count);

            // Calculate how many objects have parameters in this SET.
            uint parametersCount = 0;
            foreach (SetObject obj in Data.Objects)
                if (obj.Parameters != null)
                        parametersCount++;

            // Write this SET's parameter count.
            writer.Write(parametersCount);

            // Write a placeholder for this file's parameter data table length.
            writer.Write("SIZE");

            // Set up the parameter index value.
            uint parameterIndex = 0;

            // Loop through each object.
            for (int i = 0; i < Data.Objects.Count; i++)
            {
                // Write this object's position.
                writer.Write(Data.Objects[i].Position);

                // Write this object's first unknown integer value.
                writer.Write(Data.Objects[i].UnknownUInt32_1);

                // Write this object's second unknown integer value.
                writer.Write(Data.Objects[i].UnknownUInt32_2);

                // Write this object's third unknown integer value.
                writer.Write(Data.Objects[i].UnknownUInt32_3);

                // Write this object's first unknown byte value.
                writer.Write(Data.Objects[i].UnknownByte_1);

                // Write this object's second unknown byte value.
                writer.Write(Data.Objects[i].UnknownByte_2);

                // Write this object's third unknown byte value.
                writer.Write(Data.Objects[i].UnknownByte_3);

                // Write this object's fourth unknown byte value.
                writer.Write(Data.Objects[i].UnknownByte_4);

                // Write this object's fifth unknown byte value.
                writer.Write(Data.Objects[i].UnknownByte_5);

                // Write this object's draw distance.
                writer.Write(Data.Objects[i].DrawDistance);

                // Write this object's ID in the item table.
                writer.Write(Data.Objects[i].ObjectID);

                // Write this object's table in the item table.
                writer.Write(Data.Objects[i].TableID);

                // Write this object's fourth unknown integer value.
                writer.Write(Data.Objects[i].UnknownUInt32_4);

                // Write an unknown value of 0x00.
                writer.Write(0x00);

                // Write this object's fifth unknown integer value.
                writer.Write(Data.Objects[i].UnknownUInt32_5);

                // If this object has parameters, then write the value of parameterIndex and increment it.
                if (Data.Objects[i].Parameters != null)
                {
                    writer.Write(parameterIndex);
                    parameterIndex++;
                }
                
                // If not, then just write 0x00.
                else
                {
                    writer.Write(0x00);
                }
            }

            // Fill in the parameter table.
            for (int i = 0; i < Data.Objects.Count; i++)
            {
                // Only write stuff here if this object actually has parameters.
                if (Data.Objects[i].Parameters != null)
                {
                    // Write an unknown value of 0x01.
                    writer.Write((byte)0x01);

                    // Write an unknown value of 0x00.
                    writer.Write((byte)0x00);

                    // Write this object's parameter count.
                    // TODO: Unhardcode this when parameter types are figured out.
                    writer.Write((byte)Data.Objects[i].Parameters.Count);

                    // Write five null bytes.
                    writer.WriteNulls(0x05);
                }
            }

            // Set up the calculation for the parameter data table's length.
            long parameterDataTableLength = writer.BaseStream.Position;

            // Write each object's parameters, assuming it has any.
            // TODO: Unhardcode this when parameter types are figured out.
            for (int i = 0; i < Data.Objects.Count; i++)
                if (Data.Objects[i].Parameters != null)
                    for (int p = 0; p < Data.Objects[i].Parameters.Count; p++)
                        writer.Write((byte)Data.Objects[i].Parameters[p].Data);

            // Calculate the parameter data table's length.
            parameterDataTableLength = writer.BaseStream.Length - parameterDataTableLength;

            // Pad the file to 0x20.
            writer.FixPadding(0x20);

            // Write the parameter data table's length.
            writer.BaseStream.Position = 0x0C;
            writer.Write((uint)parameterDataTableLength);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
