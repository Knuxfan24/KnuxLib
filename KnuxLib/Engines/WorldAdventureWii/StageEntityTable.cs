using KnuxLib.HSON;
using libHSON;
using System.Text.Json;

namespace KnuxLib.Engines.WorldAdventureWii
{
    // TODO: Figure out object parameters.
    public class StageEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable() { }
        public StageEntityTable(string filepath, string hsonPath, FormatVersion version = FormatVersion.Wii, bool export = false, bool includePadding = false)
        {
            Load(filepath, hsonPath, version, includePadding);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.worldadventurewii.stageentitytable.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            PlayStation2 = 0,
            Wii = 1
        }

        public class SetObject
        {
            /// <summary>
            /// This object's type.
            /// </summary>
            public string Type { get; set; } = "";

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
            /// </summary>
            public List<SetParameter> Parameters { get; set; } = new();

            public override string ToString() => Type.ToString();
        }

        // Actual data presented to the end user.
        public List<SetObject> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="hsonPath">The path to the HSON Template Table to read this file with.</param>
        /// <param name="version">The system version to read this file as.</param>
        /// <param name="includePadding">Whether the always 0 values should be stored.</param>
        public void Load(string filepath, string hsonPath, FormatVersion version = FormatVersion.Wii, bool includePadding = false)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

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
                uint objType = reader.ReadUInt32();

                // Read this object's position.
                obj.Position = reader.ReadVector3();

                // Read this object's rotation.
                obj.Rotation = reader.ReadQuaternion();

                // Check for this object in the template table.
                if (templates.Data.Objects.ContainsKey($"0x{objType.ToString("X").PadLeft(8, '0')}"))
                {
                    // Make a reference to the object's struct. Just nicer than repeating this abomination of a line three times.
                    var objStruct = templates.Data.Structs[templates.Data.Objects[$"0x{objType.ToString("X").PadLeft(8, '0')}"].ObjectStruct];

                    // Set this object's type to the struct's name.
                    obj.Type = templates.Data.Objects[$"0x{objType.ToString("X").PadLeft(8, '0')}"].ObjectStruct;

                    // Check that this object actually has parameters.
                    if (objStruct.Fields != null)
                    {
                        // Loop through each parameter for this object.
                        for (int p = 0; p < objStruct.Fields.Length; p++)
                        {
                            // Set up a parameter entry with the name of the parameter in the template.
                            SetParameter param = new()
                            {
                                Name = objStruct.Fields[p].Name
                            };

                            // Read this parameter's data depending on its type.
                            switch (objStruct.Fields[p].Type)
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
                            if (!includePadding && objStruct.Fields[p].Name.Contains("Padding"))
                                continue;

                            // Save this parameter to the object.
                            obj.Parameters.Add(param);
                        }
                    }
                }

                // If this object is missing from the templates, then just read each byte individually.
                else
                {
                    // Set this object's type to the hex value.
                    obj.Type = $"0x{objType.ToString("X").PadLeft(8, '0')}";

                    // Loop through each byte in the parameter table and read them individually.
                    for (int p = 0; p < (objectLength - 0x24); p++)
                    {
                        SetParameter param = new()
                        {
                            Data = reader.ReadByte(),
                            DataType = typeof(byte)
                        };
                        obj.Parameters.Add(param);
                    }
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
        /// <param name="hsonPath">The path to the HSON Template Table to read this file with.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, string hsonPath, FormatVersion version = FormatVersion.Wii)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

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

                // Calculate and write this object's size.
                // Check this object exists in the template.
                if (templates.Data.Structs.ContainsKey(Data[i].Type))
                {
                    // Find this object's struct.
                    var objStruct = templates.Data.Structs[Data[i].Type];

                    // If this object's struct has any fields, then multiply the amount of them by four and add 0x24, then write that.
                    if (objStruct.Fields != null)
                        writer.Write((objStruct.Fields.Length * 0x04) + 0x24);

                    // If not, then just write 0x24.
                    else
                        writer.Write(0x24);
                }

                // If this object doesn't exist in the templates, then just write the amount of parameters plus 0x24.
                else
                {
                    writer.Write(Data[i].Parameters.Count + 0x24);
                }
            }

            // Loop through and write each object.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in the offset for this object.
                writer.FillOffset($"object{i}Offset");

                // Calculate and write this object's size.
                // Check this object exists in the template.
                if (templates.Data.Structs.ContainsKey(Data[i].Type))
                {
                    // Find this object's struct.
                    var objStruct = templates.Data.Structs[Data[i].Type];

                    // If this object's struct has any fields, then multiply the amount of them by four and add 0x24, then write that.
                    if (objStruct.Fields != null)
                        writer.Write((objStruct.Fields.Length * 0x04) + 0x24);

                    // If not, then just write 0x24.
                    else
                        writer.Write(0x24);
                }

                // If this object doesn't exist in the templates, then just write the amount of parameters plus 0x24.
                else
                {
                    writer.Write(Data[i].Parameters.Count + 0x24);
                }

                // Determine this object's type.
                int? objType = null;

                // Try and convert the stored type to a base 16 integer.
                try { objType = Convert.ToInt32(Data[i].Type, 16); }

                // If that fails, then loop through the templates to look for it.
                catch
                {
                    foreach (var obj in templates.Data.Objects)
                        if (obj.Value.ObjectStruct == Data[i].Type)
                            objType = Convert.ToInt32(obj.Key, 16);
                }

                // If the type is still null for whatever reason, give up.
                if (objType == null)
                    throw new Exception();

                // Write the determined type.
                writer.Write((int)objType);

                // Write this object's position.
                writer.Write(Data[i].Position);

                // Write this object's rotation.
                writer.Write(Data[i].Rotation);

                // Check for this object's type in the template sheet.
                if (templates.Data.Structs.ContainsKey(Data[i].Type))
                {
                    // Reference the object's type.
                    var objStruct = templates.Data.Structs[Data[i].Type];

                    // Check the object has any parameters to write.
                    if (objStruct.Fields != null)
                    {
                        // Loop through each parameter defined in the template.
                        for (int p = 0; p < objStruct.Fields.Length; p++)
                        {
                            // Set up a check for if this parameter was found in the object.
                            bool foundParam = false;

                            // Loop through each parameter in the object.
                            foreach (var param in Data[i].Parameters)
                            {
                                // Check the parameter's name against the one in the template.
                                if (param.Name == objStruct.Fields[p].Name)
                                {
                                    // Mark that this parameter has been found.
                                    foundParam = true;

                                    // Write this parameter's data depending on the type.
                                    switch (objStruct.Fields[p].Type)
                                    {
                                        case "float32": writer.Write((float)param.Data); break;
                                        case "uint32": writer.Write((uint)param.Data); break;
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

                // If this object wasn't in the parameter sheet, then write each parameter as an individual byte.
                else
                {
                    for (int p = 0; p < Data[i].Parameters.Count; p++)
                        writer.Write((byte)Data[i].Parameters[p].Data);
                }
            }

            // Write the file size.
            writer.IsBigEndian = false;
            writer.BaseStream.Position = 0x05;
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Exports this format's object data to the Hedgehog Set Object Notation format.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="hsonName">The name to add to the HSON metadata.</param>
        /// <param name="hsonAuthor">The author to add to the HSON metadata.</param>
        /// <param name="hsonDescription">The description to add to the HSON metadata.</param>
        public void ExportHSON(string filepath, string hsonName, string hsonAuthor, string hsonDescription)
        {
            // Create the HSON Project.
            Project hsonProject = Helpers.CreateHSONProject(hsonName, hsonAuthor, hsonDescription);

            // Loop through each object in this file.
            for (int i = 0; i < Data.Count; i++)
            {
                // Create a new HSON Object from this object.
                libHSON.Object hsonObject = Helpers.CreateHSONObject(Data[i].Type.ToString(), $"{Data[i].Type}{i}", Data[i].Position, Data[i].Rotation, false);

                // Write each parameter byte.
                // TODO: Unhardcode this when parameter types are figured out.
                for (int p = 0; p < Data[i].Parameters.Count; p++)
                    hsonObject.LocalParameters.Add($"Parameter{p}", new Parameter((byte)Data[i].Parameters[p].Data));

                // Add this object to the HSON Project.
                hsonProject.Objects.Add(hsonObject);
            }
            
            // Save this HSON.
            hsonProject.Save(filepath, jsonOptions: new JsonWriterOptions { Indented = true, });
        }
    }
}
