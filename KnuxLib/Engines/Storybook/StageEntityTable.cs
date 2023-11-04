using KnuxLib.HSON;
using libHSON;
using System.Text.Json;

namespace KnuxLib.Engines.Storybook
{
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, string hsonPath, bool export = false, bool includePadding = false)
        {
            Load(filepath, hsonPath);

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
            /// This object's type, as determined from the ID and Table.
            /// </summary>
            public string Type { get; set; } = "";

            /// <summary>
            /// This object's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This object's rotation in 3D space, converted from the Binary Angle Measurement System.
            /// </summary>
            public Vector3 Rotation { get; set; }

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
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to only ever be 0x00, 0x02 or 0xC8.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// A list of this object's parameters.
            /// </summary>
            public List<SetParameter>? Parameters { get; set; }

            public override string ToString() => Type;
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="hsonPath">The path to the HSON Template Table to read this file with.</param>
        /// <param name="includePadding">Whether the always 0 values should be stored.</param>
        public void Load(string filepath, string hsonPath, bool includePadding = false)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

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
            for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
            {
                // Create a new object.
                SetObject obj = new();

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read and convert the three integer values for this object's rotation from BAMS to Euler.
                obj.Rotation = new(Helpers.CalculateBAMsValue(reader.ReadInt32()), Helpers.CalculateBAMsValue(reader.ReadInt32()), Helpers.CalculateBAMsValue(reader.ReadInt32()));

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

                // Read this object's first unknown integer value.
                obj.UnknownUInt32_1 = reader.ReadUInt32();

                // Skip an unknown value of 0x00.
                reader.JumpAhead(0x04);

                // Read this object's second unknown integer value.
                obj.UnknownUInt32_2 = reader.ReadUInt32();

                // Read the index of this object's parameters.
                uint parameterTableIndex = reader.ReadUInt32();

                // String together the object ID and table into a type.
                string objectType = $"0x{obj.ObjectID.ToString("X").PadLeft(2, '0')}{obj.TableID.ToString("X").PadLeft(2, '0')}";

                // Check for this object's type in the template sheet and set it if it exists.
                if (templates.Data.Objects.ContainsKey(objectType))
                    obj.Type = templates.Data.Objects[objectType].ObjectStruct;

                // If the object isn't found, then set the type to the object id and table id.
                else
                    obj.Type = objectType;

                // If this object's first unknown byte value is NOT 0x01, then read this object's parameters.
                if (obj.UnknownByte_1 != 0x01)
                {
                    // Initialise this object's parameter table.
                    obj.Parameters = new();

                    // Save our position in the object table.
                    long position = reader.BaseStream.Position;

                    // Jump to the parameter table.
                    reader.JumpTo(parameterTableOffset);

                    // Set up a value to calculate how far into the data table this object's parameters begin.
                    uint parameterOffset = 0;

                    // Loop through based on this object's parameter index.
                    for (int parameterIndex = 0; parameterIndex < parameterTableIndex; parameterIndex++)
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

                    // Check for this type in the template sheet.
                    if (templates.Data.Objects.ContainsKey(objectType))
                    {
                        // Make a reference to the object's struct. Just nicer than repeating this abomination of a line three times.
                        HSONTemplate.HSONStruct objStruct = templates.Data.Structs[templates.Data.Objects[objectType].ObjectStruct];

                        // Check that this object actually has parameters.
                        if (objStruct.Fields != null)
                        {
                            // Loop through each parameter for this object.
                            for (int hsonParameterIndex = 0; hsonParameterIndex < objStruct.Fields.Length; hsonParameterIndex++)
                            {
                                // Set up a parameter entry with the name of the parameter in the template.
                                SetParameter param = new()
                                {
                                    Name = objStruct.Fields[hsonParameterIndex].Name
                                };

                                // Read this parameter's data depending on its type.
                                switch (objStruct.Fields[hsonParameterIndex].Type)
                                {
                                    case "float32":
                                        param.DataType = typeof(float);
                                        param.Data = reader.ReadSingle();
                                        break;
                                    case "uint32":
                                        param.DataType = typeof(uint);
                                        param.Data = reader.ReadUInt32();
                                        break;
                                    default: throw new NotImplementedException();
                                }

                                // Don't save this parameter if it's a padding one and we've chosen to ignore them.
                                if (!includePadding && objStruct.Fields[hsonParameterIndex].Name.Contains("Padding"))
                                    continue;

                                // Save this parameter to the object.
                                obj.Parameters.Add(param);
                            }
                        }
                    }

                    // If this object is missing from the templates, then just read each byte individually.
                    else
                    {
                        // Loop through each byte in the parameter table and read them individually.
                        for (byte parameterIndex = 0; parameterIndex < objectParameterLength; parameterIndex++)
                        {
                            SetParameter param = new()
                            {
                                Data = reader.ReadByte(),
                                DataType = typeof(byte)
                            };
                            obj.Parameters.Add(param);
                        }
                    }

                    // Jump back to our saved position for the next object.
                    reader.JumpTo(position);
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
        /// <param name="hsonPath">The path to the HSON Template Table to write this file with.</param>
        public void Save(string filepath, string hsonPath)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

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

            // Set up the file parameter count value.
            uint fileParameterCount = 0;

            // Loop through each object.
            for (int objectIndex = 0; objectIndex < Data.Objects.Count; objectIndex++)
            {
                // Write this object's position.
                writer.Write(Data.Objects[objectIndex].Position);

                // Write this object's X rotation, converted from Euler Angles to the Binary Angle Measurement System.
                writer.Write(Helpers.CalculateBAMsValue(Data.Objects[objectIndex].Rotation.X));

                // Write this object's Y rotation, converted from Euler Angles to the Binary Angle Measurement System.
                writer.Write(Helpers.CalculateBAMsValue(Data.Objects[objectIndex].Rotation.Y));

                // Write this object's Z rotation, converted from Euler Angles to the Binary Angle Measurement System.
                writer.Write(Helpers.CalculateBAMsValue(Data.Objects[objectIndex].Rotation.Z));

                // Write this object's first unknown byte value.
                writer.Write(Data.Objects[objectIndex].UnknownByte_1);

                // Write this object's second unknown byte value.
                writer.Write(Data.Objects[objectIndex].UnknownByte_2);

                // Write this object's third unknown byte value.
                writer.Write(Data.Objects[objectIndex].UnknownByte_3);

                // Write this object's fourth unknown byte value.
                writer.Write(Data.Objects[objectIndex].UnknownByte_4);

                // Write this object's fifth unknown byte value.
                writer.Write(Data.Objects[objectIndex].UnknownByte_5);

                // Write this object's draw distance.
                writer.Write(Data.Objects[objectIndex].DrawDistance);

                // Write this object's ID in the item table.
                writer.Write(Data.Objects[objectIndex].ObjectID);

                // Write this object's table in the item table.
                writer.Write(Data.Objects[objectIndex].TableID);

                // Write this object's first unknown integer value.
                writer.Write(Data.Objects[objectIndex].UnknownUInt32_1);

                // Write an unknown value of 0x00.
                writer.Write(0x00);

                // Write this object's second unknown integer value.
                writer.Write(Data.Objects[objectIndex].UnknownUInt32_2);

                // If this object has parameters, then write the value of parameterIndex and increment it.
                if (Data.Objects[objectIndex].Parameters != null)
                {
                    writer.Write(fileParameterCount);
                    fileParameterCount++;
                }
                
                // If not, then just write 0x00.
                else
                {
                    writer.Write(0x00);
                }
            }

            // Fill in the parameter table.
            for (int objectIndex = 0; objectIndex < Data.Objects.Count; objectIndex++)
            {
                // Only write stuff here if this object actually has parameters.
                if (Data.Objects[objectIndex].Parameters != null)
                {
                    // Write an unknown value of 0x01.
                    writer.Write((byte)0x01);

                    // Write an unknown value of 0x00.
                    writer.Write((byte)0x00);

                    // Write this object's parameter count.
                    // String together the object ID and table into a type.
                    string objectType = $"0x{Data.Objects[objectIndex].ObjectID.ToString("X").PadLeft(2, '0')}{Data.Objects[objectIndex].TableID.ToString("X").PadLeft(2, '0')}";

                    // Check for this type in the template sheet.
                    if (templates.Data.Objects.ContainsKey(objectType))
                    {
                        // Make a reference to the object's struct. Just nicer than repeating this abomination of a line three times.
                        HSONTemplate.HSONStruct objStruct = templates.Data.Structs[templates.Data.Objects[objectType].ObjectStruct];

                        // Check that this object actually has parameters and write the amount of them multiplied by 4.
                        if (objStruct.Fields != null)
                            writer.Write((byte)(objStruct.Fields.Length * 0x04));
                        else
                            writer.Write((byte)0);
                    }

                    // If this object doesn't exist in the templates, then just write the amount of parameters.
                    else
                    {
                        writer.Write((byte)Data.Objects[objectIndex].Parameters.Count);
                    }

                    // Write five null bytes.
                    writer.WriteNulls(0x05);
                }
            }

            // Set up the calculation for the parameter data table's length.
            long parameterDataTableLength = writer.BaseStream.Position;

            // Write each object's parameters, assuming it has any.
            // TODO: Unhardcode this when parameter types are figured out.
            for (int objectIndex = 0; objectIndex < Data.Objects.Count; objectIndex++)
            {
                if (Data.Objects[objectIndex].Parameters != null)
                {
                    // String together the object ID and table into a type.
                    string objectType = $"0x{Data.Objects[objectIndex].ObjectID.ToString("X").PadLeft(2, '0')}{Data.Objects[objectIndex].TableID.ToString("X").PadLeft(2, '0')}";

                    // Check for this type in the template sheet.
                    if (templates.Data.Objects.ContainsKey(objectType))
                    {
                        // Make a reference to the object's struct. Just nicer than repeating this abomination of a line three times.
                        HSONTemplate.HSONStruct objStruct = templates.Data.Structs[templates.Data.Objects[objectType].ObjectStruct];

                        // Check that this object actually has parameters and write the amount of them multiplied by 4.
                        if (objStruct.Fields != null)
                        {
                            // Loop through each parameter defined in the template.
                            for (int hsonParameterIndex = 0; hsonParameterIndex < objStruct.Fields.Length; hsonParameterIndex++)
                            {
                                // Set up a check for if this parameter was found in the object.
                                bool foundParam = false;

                                // Loop through each parameter in the object.
                                foreach (SetParameter param in Data.Objects[objectIndex].Parameters)
                                {
                                    // Check the parameter's name against the one in the template.
                                    if (param.Name == objStruct.Fields[hsonParameterIndex].Name)
                                    {
                                        // Mark that this parameter has been found.
                                        foundParam = true;

                                        // Write this parameter's data depending on the type.
                                        switch (objStruct.Fields[hsonParameterIndex].Type)
                                        {
                                            case "float32": writer.Write(Convert.ToSingle(param.Data)); break;
                                            case "uint32": writer.Write(Convert.ToInt32(param.Data)); break;
                                            default: throw new NotImplementedException();
                                        }
                                    }
                                }

                                // If we didn't find this parameter, then write a 0 to fill it in.
                                if (!foundParam)
                                    writer.Write(0);
                            }
                        }
                    }
                    else
                    {
                        for (int parameterIndex = 0; parameterIndex < Data.Objects[objectIndex].Parameters.Count; parameterIndex++)
                            writer.Write((byte)Data.Objects[objectIndex].Parameters[parameterIndex].Data);
                    }
                }
            }

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

        /// <summary>
        /// Exports this format's object data to the Hedgehog Set Object Notation format.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="hsonPath">The path to the HSON Template Table to write this file with.</param>
        /// <param name="hsonName">The name to add to the HSON metadata.</param>
        /// <param name="hsonAuthor">The author to add to the HSON metadata.</param>
        /// <param name="hsonDescription">The description to add to the HSON metadata.</param>
        public void ExportHSON(string filepath, string hsonPath, string hsonName, string hsonAuthor, string hsonDescription)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

            // Create the HSON Project.
            Project hsonProject = Helpers.CreateHSONProject(hsonName, hsonAuthor, hsonDescription);

            // Loop through each object in this file.
            for (int objectIndex = 0; objectIndex < Data.Objects.Count; objectIndex++)
            {
                // Create a new HSON Object from this object.
                libHSON.Object hsonObject = Helpers.CreateHSONObject(Data.Objects[objectIndex].Type.ToString(), $"{Data.Objects[objectIndex].Type}{objectIndex}", Data.Objects[objectIndex].Position, Helpers.EulerToQuat(Data.Objects[objectIndex].Rotation), false);

                // Check for this object's type in the template sheet.
                if (templates.Data.Structs.ContainsKey(Data.Objects[objectIndex].Type))
                {
                    // Reference the object's type.
                    HSONTemplate.HSONStruct objStruct = templates.Data.Structs[Data.Objects[objectIndex].Type];

                    // Check the object has any parameters to write.
                    if (objStruct.Fields != null)
                    {
                        // Loop through each parameter defined in the template.
                        for (int hsonParameterIndex = 0; hsonParameterIndex < objStruct.Fields.Length; hsonParameterIndex++)
                        {
                            // Loop through each parameter in the object.
                            foreach (SetParameter param in Data.Objects[objectIndex].Parameters)
                            {
                                // Check the parameter's name against the one in the template.
                                if (param.Name == objStruct.Fields[hsonParameterIndex].Name)
                                {
                                    // Write this parameter's data depending on the type.
                                    switch (objStruct.Fields[hsonParameterIndex].Type)
                                    {
                                        case "float32": hsonObject.LocalParameters.Add(param.Name, new Parameter((float)param.Data)); break;
                                        case "uint32": hsonObject.LocalParameters.Add(param.Name, new Parameter((uint)param.Data)); break;
                                        default: throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                }

                // If this object wasn't in the parameter sheet, then write each parameter as an individual byte.
                else
                {
                    if (Data.Objects[objectIndex].Parameters != null)
                        for (int parameterIndex = 0; parameterIndex < Data.Objects[objectIndex].Parameters.Count; parameterIndex++)
                            hsonObject.LocalParameters.Add($"Parameter{parameterIndex}", new Parameter((byte)Data.Objects[objectIndex].Parameters[parameterIndex].Data));
                }

                // Add this object to the HSON Project.
                hsonProject.Objects.Add(hsonObject);
            }

            // Save this HSON.
            hsonProject.Save(filepath, jsonOptions: new JsonWriterOptions { Indented = true });
        }
    }
}
