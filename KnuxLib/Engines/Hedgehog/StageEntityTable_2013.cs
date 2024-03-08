using KnuxLib.HSON;
using libHSON;
using System.Text.Json;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Get writing accurate (or as accurate as I can).
    // TODO: HSON Importing. Waiting on a more accurate HSON Template Sheet before I do.
    public class StageEntityTable_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public StageEntityTable_2013() { }
        public StageEntityTable_2013(string filepath, string hsonPath, bool export = false)
        {
            Load(filepath, hsonPath);

            if (export)
                ExportHSON(Path.ChangeExtension(filepath, ".hson"), hsonPath, Path.GetFileNameWithoutExtension(filepath), Environment.UserName, $"Autoconverted from {filepath}");    
        }

        // Classes for this format.
        public class SetObject
        {
            /// <summary>
            /// This object's type.
            /// </summary>
            public string Type { get; set; } = "";

            /// <summary>
            /// This object's index, stored as they aren't linear in the original files.
            /// </summary>
            public ushort Index { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_1 { get; set; } = 0;

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; } = 10f;

            /// <summary>
            /// The distance that this object spawns at.
            /// </summary>
            public float RangeIn { get; set; } = 1000f;

            /// <summary>
            /// The distance that this object despawns at.
            /// </summary>
            public float RangeOut { get; set; } = 1200f;

            /// <summary>
            /// The index of this object's parent.
            /// </summary>
            public uint ParentIndex { get; set; } = 0;

            /// <summary>
            /// This object's parameters.
            /// </summary>
            public List<SetParameter> Parameters { get; set; } = new();

            /// <summary>
            /// This object's transform.
            /// </summary>
            public Transform Transform { get; set; } = new();

            /// <summary>
            /// This object's child transforms, if it has any.
            /// </summary>
            public Transform[]? ChildTransforms { get; set; }

            public override string ToString() => $"{Type}{Index}";
        }

        public class Transform
        {
            /// <summary>
            /// This transform's world space position.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This transform's world space rotation, converted from radians to a quaternion.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This transform's local space position.
            /// </summary>
            public Vector3 LocalSpacePosition { get; set; }

            /// <summary>
            /// This transform's local space rotation, converted from radians to a quaternion.
            /// </summary>
            public Quaternion LocalSpaceRotation { get; set; }
        }

        // Actual data presented to the end user.
        public SetObject[] Data = Array.Empty<SetObject>();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(200);

        // Set up the Signature we expect.
        public new const string Signature = "JBOS";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, string hsonPath)
        {
            // Read the HSON Templates for this SET.
            HSONTemplate templates = new(hsonPath);

            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value that is always 0x01.
            reader.JumpAhead(0x04);

            // Read the count of object types in this SET.
            uint objectTypeCount = reader.ReadUInt32();

            // Read the offset to this SET's object type table.
            uint objectTypeTableOffset = reader.ReadUInt32();

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the offset to this SET's object table.
            uint objectTableOffset = reader.ReadUInt32();

            // Read the count of objects in this SET.
            uint objectCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the count of transform enteries in this SET.
            uint transformsCount = reader.ReadUInt32();

            // Initialise the object array for this SET.
            Data = new SetObject[objectCount];

            // Jump to this SET's object type table.
            reader.JumpTo(objectTypeTableOffset, false);

            // Loop through each object type in this SET.
            for (int objectTypeIndex = 0; objectTypeIndex < objectTypeCount; objectTypeIndex++)
            {
                // Read this object type.
                string objectType = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                // Read how many times this object type is used.
                uint objectTypeAmount = reader.ReadUInt32();

                // Read the offset to the table of indices for the objects that use this type.
                uint objectIndicesOffset = reader.ReadUInt32();

                // Save our position so we can jump back for the next object type.
                long position = reader.BaseStream.Position;

                // Jump to this object type's index table.
                reader.JumpTo(objectIndicesOffset, false);

                // Loop through each object in this object type's index table.
                for (int objectIndex = 0; objectIndex < objectTypeAmount; objectIndex++)
                {
                    // Initialise a new object with this object type.
                    SetObject obj = new() { Type = objectType };

                    // Read the index into the object table for this object.
                    ushort objectTableIndex = reader.ReadUInt16();

                    // Save our position so we can jump back for the next object index.
                    long objectIndicesPosition = reader.BaseStream.Position;

                    // Jump to this object's position in the object table.
                    reader.JumpTo(objectTableOffset + (0x04 * objectTableIndex), false);

                    // Jump to the offset at this point in the object table.
                    reader.JumpTo(reader.ReadUInt32(), false);

                    // Read this object's index.
                    obj.Index = reader.ReadUInt16();

                    // Read this object's unknown short value.
                    obj.UnknownUShort_1 = reader.ReadUInt16();

                    // Read this object's unknown integer value.
                    obj.UnknownUInt32_1 = reader.ReadUInt32();

                    // Skip an unknown value that is always 0.
                    reader.JumpAhead(0x04);

                    // Read this object's unknown floating point value.
                    obj.UnknownFloat_1 = reader.ReadSingle();

                    // Read this object's range in value.
                    obj.RangeIn = reader.ReadSingle();

                    // Read this object's range out value.
                    obj.RangeOut = reader.ReadSingle();

                    // Read this object's parent index.
                    obj.ParentIndex = reader.ReadUInt32();

                    // Read the offset to this object's transform data.
                    uint transformsOffset = reader.ReadUInt32();

                    // Read how many transforms this object uses.
                    uint transformCount = reader.ReadUInt32();

                    // Skip three unknown values that are always 0.
                    reader.JumpAhead(0x0C);

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
                                    case "array":
                                        // Realign to 0x04 bytes.
                                        reader.FixPadding(0x04);

                                        // Read this array's offset.
                                        uint arrayOffset = reader.ReadUInt32();

                                        // Read this array's entry count
                                        uint arrayEntryCount = reader.ReadUInt32();

                                        // Skip an unknown value that is always 0.
                                        reader.JumpAhead(0x04);

                                        // Save our position so we can jump back for the next parameter.
                                        long arrayPosition = reader.BaseStream.Position;

                                        // Jump to this parameter's array.
                                        reader.JumpTo(arrayOffset, false);

                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(uint[]);

                                        // Initialise this parameter's array.
                                        param.Data = new uint[arrayEntryCount];

                                        // Loop through and read each entry in this array.
                                        for (int arrayIndex = 0; arrayIndex < arrayEntryCount; arrayIndex++)
                                            (param.Data as uint[])[arrayIndex] = reader.ReadUInt32();

                                        // Jump back for the next parameter.
                                        reader.JumpTo(arrayPosition);
                                        break;

                                    case "bool":
                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(bool);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadBoolean();
                                        break;

                                    case "float32":
                                        // Realign to 0x04 bytes.
                                        reader.FixPadding(0x04);

                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(float);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadSingle();
                                        break;

                                    case "int8":
                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(sbyte);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadSByte();
                                        break;

                                    case "int32":
                                        // Realign to 0x04 bytes.
                                        reader.FixPadding(0x04);

                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(int);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadInt32();
                                        break;

                                    case "string":
                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(string);

                                        // Read this parameter's value.
                                        param.Data = Helpers.ReadNullTerminatedStringTableEntry(reader, false);

                                        // Skip an unknown value that is always 0.
                                        reader.JumpAhead(0x04);
                                        break;

                                    case "vector3":
                                        // Realign to 0x10 bytes.
                                        reader.FixPadding(0x10);

                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(Vector3);

                                        // Read this parameter's value.
                                        param.Data = Helpers.ReadHedgeLibVector3(reader);
                                        break;

                                    case "uint8":
                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(byte);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadByte();
                                        break;

                                    case "uint16":
                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(ushort);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadUInt16();
                                        break;

                                    case "uint32":
                                        // Realign to 0x04 bytes.
                                        reader.FixPadding(0x04);

                                        // Set up the data type of this parameter.
                                        param.DataType = typeof(uint);

                                        // Read this parameter's value.
                                        param.Data = reader.ReadUInt32();
                                        break;

                                    default: throw new NotImplementedException();
                                }

                                // Save this parameter to the object.
                                obj.Parameters.Add(param);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Object of type '{objectType}' is missing a template entry!");
                    }

                    // Jump to this object's transform table.
                    reader.JumpTo(transformsOffset, false);

                    // Read this object's world space position.
                    obj.Transform.Position = Helpers.ReadHedgeLibVector3(reader);

                    // Read this object's world space rotation and convert it to a quaternion.
                    HedgeLib.Quaternion rotation = new(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), true);
                    obj.Transform.Rotation = new(rotation.X, rotation.Y, rotation.Z, rotation.W);

                    // Read this object's local space position.
                    obj.Transform.LocalSpacePosition = Helpers.ReadHedgeLibVector3(reader);

                    // Read this object's local space rotation and convert it to a quaternion.
                    rotation = new(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), true);
                    obj.Transform.LocalSpaceRotation = new(rotation.X, rotation.Y, rotation.Z, rotation.W);

                    // If this object has more than one transform, then read its child transforms.
                    if (transformCount != 1)
                    {
                        // Initialise this object's child transforms array.
                        obj.ChildTransforms = new Transform[transformCount - 1];

                        // Loop through each child transform in this object.
                        for (int childTransformIndex = 0; childTransformIndex < transformCount - 1; childTransformIndex++)
                        {
                            // Set up a new child transform entry.
                            Transform childTransform = new();

                            // Read this child transform's world space position.
                            childTransform.Position = Helpers.ReadHedgeLibVector3(reader);

                            // Read this child transform's world space rotation and convert it to a quaternion.
                            rotation = new(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), true);
                            childTransform.Rotation = new(rotation.X, rotation.Y, rotation.Z, rotation.W);

                            // Read this child transform's local space position.
                            childTransform.LocalSpacePosition = Helpers.ReadHedgeLibVector3(reader);

                            // Read this child transform's local space rotation and convert it to a quaternion.
                            rotation = new(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()), true);
                            childTransform.LocalSpaceRotation = new(rotation.X, rotation.Y, rotation.Z, rotation.W);

                            // Save this child transform.
                            obj.ChildTransforms[childTransformIndex] = childTransform;
                        }
                    }

                    // Save this object.
                    Data[objectTableIndex] = obj;

                    // Jump back for the next object of this type.
                    reader.JumpTo(objectIndicesPosition);
                }

                // Jump back for the next object type.
                reader.JumpTo(position);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a dictonary of object types.
            Dictionary<string, List<ushort>> objectTypes = new();

            // Set up a value to calculate the amount of transforms in this SET.
            int transformCount = 0;

            // Loop through each object in this SET.
            foreach (SetObject obj in Data)
            {
                // If we haven't already, add a key to the dictonary for this object type.
                if (!objectTypes.ContainsKey(obj.Type))
                    objectTypes.Add(obj.Type, new List<ushort>());

                // Add this object's index to the type's list.
                objectTypes[obj.Type].Add(obj.Index);

                // Increment transform count to factor in this object's transform.
                transformCount++;

                // If this object has any child transforms, then include them in the transform count.
                if (obj.ChildTransforms != null)
                    transformCount += obj.ChildTransforms.Length;
            }

            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write the JBOS signature.
            writer.WriteSignature(Signature);

            // Write an unknown value that is always 1.
            writer.Write(0x01);

            // Write the count of object types in this SET.
            writer.Write(objectTypes.Count);

            // Add an offset to this SET's type table.
            writer.AddOffset("ObjectTypeTableOffset");

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Add an offset to this SET's object table.
            writer.AddOffset("ObjectTableOffset");

            // Write the count of objects in this SET.
            writer.Write(Data.Length);

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write the count of transform entries in this SET.
            writer.Write(transformCount);

            // Fill in the offset for this SET's type table.
            writer.FillInOffset("ObjectTypeTableOffset", false);

            // Loop through each object type.
            for (int objectTypeIndex = 0; objectTypeIndex < objectTypes.Count; objectTypeIndex++)
            {
                // Add a string for this object type's name.
                writer.AddString($"ObjectType{objectTypeIndex}Name", objectTypes.ElementAt(objectTypeIndex).Key);

                // Write the count of objects that use this type.
                writer.Write(objectTypes.ElementAt(objectTypeIndex).Value.Count);

                // Add an offset to this type's indices table.
                writer.AddOffset($"ObjectType{objectTypeIndex}Indices");
            }

            // Loop through each object type.
            for (int objectTypeIndex = 0; objectTypeIndex < objectTypes.Count; objectTypeIndex++)
            {
                // Fill in the offset to this type's indices table.
                writer.FillInOffset($"ObjectType{objectTypeIndex}Indices", false);

                // Loop through each object and check if it's this type. If so, write its index.
                for (ushort objectIndex = 0; objectIndex < Data.Length; objectIndex++)
                    if (Data[objectIndex].Type == objectTypes.ElementAt(objectTypeIndex).Key)
                        writer.Write(objectIndex);

                // Realign to 0x04 bytes.
                writer.FixPadding(0x04);
            }

            // Fill in the offset for the object table.
            writer.FillInOffset("ObjectTableOffset", false);

            // Loop through each object and add an offset for them.
            for (int objectIndex = 0; objectIndex < Data.Length; objectIndex++)
                writer.AddOffset($"Object{objectIndex}");

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Loop through each object.
            for (int objectIndex = 0; objectIndex < Data.Length; objectIndex++)
            {
                // Fill in this object's offset.
                writer.FillInOffset($"Object{objectIndex}", false);

                // Write this object's index.
                writer.Write(Data[objectIndex].Index);

                // Write this object's unknown short value.
                writer.Write(Data[objectIndex].UnknownUShort_1);

                // Write this object's unknown integer value.
                writer.Write(Data[objectIndex].UnknownUInt32_1);

                // Write an unknown value that is always 0.
                writer.Write(0);

                // Write this object's unknown floating point value.
                writer.Write(Data[objectIndex].UnknownFloat_1);

                // Write this object's range in value.
                writer.Write(Data[objectIndex].RangeIn);

                // Write this object's range out value.
                writer.Write(Data[objectIndex].RangeOut);

                // Write this object's parent index.
                writer.Write(Data[objectIndex].ParentIndex);

                // Add an offset for this object's transforms.
                writer.AddOffset($"Object{objectIndex}TransformsOffset");

                // Calculate how many transforms this object has.
                transformCount = 1;
                if (Data[objectIndex].ChildTransforms != null)
                    transformCount += Data[objectIndex].ChildTransforms.Length;

                // Write this object's transform count.
                writer.Write(transformCount);

                // Write three unknown values that are always 0.
                writer.WriteNulls(0x0C);

                // Loop through each parameter in this object.
                for (int parameterIndex = 0; parameterIndex < Data[objectIndex].Parameters.Count; parameterIndex++)
                {
                    // Write this parameter's data depending on its type.
                    switch (Data[objectIndex].Parameters[parameterIndex].DataType.ToString())
                    {
                        case "System.Boolean":
                            // Write this boolean value.
                            writer.Write((bool)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.Byte":
                            // Write this byte value.
                            writer.Write((byte)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.Int32":
                            // Realign to 0x04 bytes.
                            writer.FixPadding(0x04);

                            // Write this integer value.
                            writer.Write((int)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.SByte":
                            // Write this signed byte value.
                            writer.Write((sbyte)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.Single":
                            // Realign to 0x04 bytes.
                            writer.FixPadding(0x04);

                            // Write this floating point value.
                            writer.Write((float)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.String":
                            // Add an offset for this string.
                            writer.AddOffset($"Object{objectIndex}Parameter{parameterIndex}String");

                            // Write an unknown value that is always 0.
                            writer.Write(0);
                            break;

                        case "System.Numerics.Vector3":
                            // Realign to 0x10 bytes.
                            writer.FixPadding(0x10);

                            // Write this Vector3.
                            Helpers.WriteHedgeLibVector3(writer, (Vector3)Data[objectIndex].Parameters[parameterIndex].Data);

                            // Realign to 0x10 bytes.
                            writer.FixPadding(0x10);
                            break;

                        case "System.UInt16":
                            // Write this unsigned short value.
                            writer.Write((ushort)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.UInt32":
                            // Realign to 0x04 bytes.
                            writer.FixPadding(0x04);

                            // Write this unsigned integer value.
                            writer.Write((uint)Data[objectIndex].Parameters[parameterIndex].Data);
                            break;

                        case "System.UInt32[]":
                            // Realign to 0x04 bytes.
                            writer.FixPadding(0x04);

                            // If this array is not empty, then add an offset for it. If it is, then just write a 0 instead.
                            if ((Data[objectIndex].Parameters[parameterIndex].Data as uint[]).Length != 0)
                                writer.AddOffset($"Object{objectIndex}Parameter{parameterIndex}ArrayOffset");
                            else
                                writer.Write(0);

                            // Write this array's length.
                            writer.Write((Data[objectIndex].Parameters[parameterIndex].Data as uint[]).Length);

                            // Write an unknown value that is always 0.
                            writer.Write(0);
                            break;

                        default: throw new NotImplementedException();
                    }
                }

                // Loop through each parameter in this object.
                for (int parameterIndex = 0; parameterIndex < Data[objectIndex].Parameters.Count; parameterIndex++)
                {
                    // Check if this parameter is a string that needs writing.
                    if (Data[objectIndex].Parameters[parameterIndex].DataType.ToString() == "System.String")
                    {
                        // Realign to 0x04 bytes.
                        writer.FixPadding(0x04);

                        // Fill in this parameter's string offset.
                        writer.FillInOffset($"Object{objectIndex}Parameter{parameterIndex}String", false);

                        // Check this string actually has a value.
                        if (Data[objectIndex].Parameters[parameterIndex].Data as string != null)
                        {
                            // Write this parameter's string.
                            writer.WriteNullTerminatedString(Data[objectIndex].Parameters[parameterIndex].Data as string);

                            // Realign to 0x04 bytes.
                            writer.FixPadding(0x04);
                        }
                    }

                    // If this isn't the last object, then realign to 0x10 bytes.
                    if (objectIndex != Data.Length - 1)
                        writer.FixPadding(0x10);
                }
            }

            // Loop through each object.
            for (int objectIndex = 0; objectIndex < Data.Length; objectIndex++)
            {
                // Fill in the offset for this object's transforms.
                writer.FillInOffset($"Object{objectIndex}TransformsOffset", false);
                
                // Write this object's position.
                Helpers.WriteHedgeLibVector3(writer, Data[objectIndex].Transform.Position);

                // Convert this object's rotation to radians and write the result.
                HedgeLib.Quaternion quaternion = new(Data[objectIndex].Transform.Rotation.X, Data[objectIndex].Transform.Rotation.Y, Data[objectIndex].Transform.Rotation.Z, Data[objectIndex].Transform.Rotation.W);
                writer.Write(quaternion.ToEulerAngles(true));

                // Write this object's local space position.
                Helpers.WriteHedgeLibVector3(writer, Data[objectIndex].Transform.LocalSpacePosition);

                // Convert this object's local space rotation to radians and write the result.
                quaternion = new(Data[objectIndex].Transform.LocalSpaceRotation.X, Data[objectIndex].Transform.LocalSpaceRotation.Y, Data[objectIndex].Transform.LocalSpaceRotation.Z, Data[objectIndex].Transform.LocalSpaceRotation.W);
                writer.Write(quaternion.ToEulerAngles(true));

                // Check if this object has any child transforms.
                if (Data[objectIndex].ChildTransforms != null)
                {
                    // Loop through each of this object's child transforms.
                    foreach (Transform childTransform in Data[objectIndex].ChildTransforms)
                    {
                        // Write this child transform's position.
                        Helpers.WriteHedgeLibVector3(writer, childTransform.Position);

                        // Convert this child transform's rotation to radians and write the result.
                        quaternion = new(childTransform.Rotation.X, childTransform.Rotation.Y, childTransform.Rotation.Z, childTransform.Rotation.W);
                        writer.Write(quaternion.ToEulerAngles(true));

                        // Write this child transform's local space position.
                        Helpers.WriteHedgeLibVector3(writer, childTransform.LocalSpacePosition);

                        // Convert this child transform's local space rotation to radians and write the result.
                        quaternion = new(childTransform.LocalSpaceRotation.X, childTransform.LocalSpaceRotation.Y, childTransform.LocalSpaceRotation.Z, childTransform.LocalSpaceRotation.W);
                        writer.Write(quaternion.ToEulerAngles(true));
                    }
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
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
            for (int objectIndex = 0; objectIndex < Data.Length; objectIndex++)
            {
                var combinedRotation = Data[objectIndex].Transform.Rotation + Data[objectIndex].Transform.LocalSpaceRotation;
                HedgeLib.Quaternion quaternionRotation = new(new(combinedRotation.X, combinedRotation.Y, combinedRotation.Z), true);

                // Create a new HSON Object from this object.
                libHSON.Object hsonObject = Helpers.CreateHSONObject(Data[objectIndex].Type.ToString(), $"{Data[objectIndex].Type}{Data[objectIndex].Index}", Data[objectIndex].Transform.Position + Data[objectIndex].Transform.LocalSpacePosition, new(quaternionRotation.X, quaternionRotation.Y, quaternionRotation.Z, quaternionRotation.W), false);

                // Check for this object's type in the template sheet.
                if (templates.Data.Structs.ContainsKey($"Obj{Data[objectIndex].Type}Spawner"))
                {
                    // Reference the object's type.
                    HSONTemplate.HSONStruct objStruct = templates.Data.Structs[$"Obj{Data[objectIndex].Type}Spawner"];

                    // Check the object has any parameters to write.
                    if (objStruct.Fields != null)
                    {
                        // Loop through each parameter defined in the template.
                        for (int hsonParameterIndex = 0; hsonParameterIndex < objStruct.Fields.Length; hsonParameterIndex++)
                        {
                            // Loop through each parameter in the object.
                            foreach (SetParameter param in Data[objectIndex].Parameters)
                            {
                                // Check the parameter's name against the one in the template.
                                if (param.Name == objStruct.Fields[hsonParameterIndex].Name)
                                {
                                    // Write this parameter's data depending on the type.
                                    switch (objStruct.Fields[hsonParameterIndex].Type)
                                    {
                                        case "array":
                                            // Create a parameter with the array type.
                                            Parameter hsonParameterArray = new(ParameterType.Array);

                                            // Loop through each entry in the array and add it to the parameter array.
                                            foreach (uint arrayEntry in param.Data as uint[])
                                                hsonParameterArray.ValueArray.Add(new Parameter(arrayEntry));

                                            // Add the array as a HSON parameter.
                                            hsonObject.LocalParameters.Add(param.Name, hsonParameterArray);
                                            break;

                                        case "bool":
                                            // Add a HSON parameter with this parameter's name and boolean value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((bool)param.Data));
                                            break;

                                        case "float32":
                                            // Add a HSON parameter with this parameter's name and floating point value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((float)param.Data));
                                            break;

                                        case "int8":
                                            // Add a HSON parameter with this parameter's name and signed byte value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((sbyte)param.Data));
                                            break;

                                        case "int32":
                                            // Add a HSON parameter with this parameter's name and integer value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((int)param.Data));
                                            break;

                                        case "string":
                                            // Add a HSON parameter with this parameter's name and string value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((string)param.Data));
                                            break;

                                        case "vector3":
                                            // Read this parameter's value as a Vector3.
                                            Vector3 parameterAsVector3 = (Vector3)param.Data;

                                            // Create a parameter with the array type.
                                            Parameter hsonParameterVector3 = new(ParameterType.Array);

                                            // Add the X, Y and Z values from the Vector3 as individual parameters to the parameter array.
                                            hsonParameterVector3.ValueArray.Add(new Parameter(parameterAsVector3.X));
                                            hsonParameterVector3.ValueArray.Add(new Parameter(parameterAsVector3.Y));
                                            hsonParameterVector3.ValueArray.Add(new Parameter(parameterAsVector3.Z));

                                            // Add the array as a HSON parameter.
                                            hsonObject.LocalParameters.Add(param.Name, hsonParameterVector3);
                                            break;

                                        case "uint8":
                                            // Add a HSON parameter with this parameter's name and byte value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((byte)param.Data));
                                            break;

                                        case "uint16":
                                            // Add a HSON parameter with this parameter's name and unsigned short value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((ushort)param.Data));
                                            break;

                                        case "uint32":
                                            // Add a HSON parameter with this parameter's name and unsigned integer value.
                                            hsonObject.LocalParameters.Add(param.Name, new Parameter((uint)param.Data));
                                            break;

                                        default: throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                }

                // Add this object to the HSON Project.
                hsonProject.Objects.Add(hsonObject);
            }

            // Loop through each object in this file.
            for (int objectIndex = 0; objectIndex < Data.Length; objectIndex++)
            {
                // Check if this object has a parent.
                if (Data[objectIndex].ParentIndex != 0)
                {
                    // Check if this object's parent index exists, as one SET has some Rings that have a value higher than the total number of objects for some stupid reason.
                    if (Data.Length >= Data[objectIndex].ParentIndex)
                        hsonProject.Objects[objectIndex].Parent = hsonProject.Objects[(int)Data[objectIndex].ParentIndex];
                    else
                        Console.WriteLine($"Object {hsonProject.Objects[objectIndex].Name} has a parent index higher than the total number of objects?");
                }
            }

            // Save this HSON.
            hsonProject.Save(filepath, jsonOptions: new JsonWriterOptions { Indented = true });
        }
    }
}
