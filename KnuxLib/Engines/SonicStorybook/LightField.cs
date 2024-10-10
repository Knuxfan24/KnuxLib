using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.SonicStorybook
{
    public class LightField : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LightField() { }
        public LightField(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".sonicstorybook.lightfield.json";

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<FormatData>(filepath);

                //If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.BIN");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum LightFieldShape
        {
            Sphere = 0,
            Capsule = 1,
            Box = 2,
            OmniBox = 3
        }

        public class FormatData
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The light field entries in this file.
            /// </summary>
            public LightFieldEntry[] LightFields { get; set; } = [];

            /// <summary>
            /// The axis aligned bounding boxes in this file.
            /// </summary>
            public LightFieldAxisAlignedBoundingBox[] AxisAlignedBoundingBoxes { get; set; } = [];
        }

        public class LightFieldEntry
        {
            /// <summary>
            /// This light field's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// This light field's shape.
            /// </summary>
            public LightFieldShape Shape { get; set; }

            /// <summary>
            /// Which axis aligned bounding box this light field should use.
            /// </summary>
            public byte AxisAlignedBoundingBoxIndex { get; set; }

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_1 { get; set; }

            /// <summary>
            /// This object's scale, differs in type depending on the shape.
            /// </summary>
            public object Scale { get; set; } = new();

            /// <summary>
            /// This light field's origin.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This light field's rotation.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Light Index? Would need to RE the LIGHT.BIN format for that.
            /// </summary>
            public int UnknownInt32_1 { get; set; }

            /// <summary>
            /// Displays this light field's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this light field with default data.
            /// </summary>
            public LightFieldEntry() { }

            /// <summary>
            /// Initialises this light field with the provided data.
            /// </summary>
            public LightFieldEntry(string name, LightFieldShape shape, byte axisAlignedBoundingBoxIndex, ushort unknownUShort_1, object scale, Vector3 position, Quaternion rotation, int unknownInt32_1)
            {
                Name = name;
                Shape = shape;
                AxisAlignedBoundingBoxIndex = axisAlignedBoundingBoxIndex;
                UnknownUShort_1 = unknownUShort_1;
                Scale = scale;
                Position = position;
                Rotation = rotation;
                UnknownInt32_1 = unknownInt32_1;
            }

            /// <summary>
            /// Initialises this light field by reading its data from a BINAReader.
            /// </summary>
            public LightFieldEntry(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this light field.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read this light field's name.
                Name = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

                // Read this light field's shape type.
                Shape = (LightFieldShape)reader.ReadByte();

                // Read the index for this light field's axis aligned bounding box.
                AxisAlignedBoundingBoxIndex = reader.ReadByte();

                // Read this light field's unknown short value.
                UnknownUShort_1 = reader.ReadUInt16();

                // Read this light field's scale, depending on its shape.
                switch (Shape)
                {
                    case LightFieldShape.Sphere:
                        Scale = new SphereScale()
                        {
                            Radius = reader.ReadSingle(),
                            Margin = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x10);
                        break;

                    case LightFieldShape.Capsule:
                        Scale = new CapsuleScale()
                        {
                            Radius = reader.ReadSingle(),
                            Height = reader.ReadSingle(),
                            Margin = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x0C);
                        break;

                    case LightFieldShape.Box:
                        Scale = new BoxScale()
                        {
                            Length = reader.ReadSingle(),
                            Width = reader.ReadSingle(),
                            Height = reader.ReadSingle(),
                            Margin1 = reader.ReadSingle(),
                            Margin2 = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x04);
                        break;

                    case LightFieldShape.OmniBox:
                        Scale = new OmniBoxScale()
                        {
                            Length = reader.ReadSingle(),
                            Width = reader.ReadSingle(),
                            Height = reader.ReadSingle(),
                            Margin = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x08);
                        break;
                }

                // Read this light field's position.
                Position = reader.ReadVector3();

                // Read this light field's rotation.
                Rotation = reader.ReadQuaternion();

                // Read this light field's unknown integer value.
                UnknownInt32_1 = reader.ReadInt32();
            }

            /// <summary>
            /// Writes the data for this light field.
            /// </summary>
            public void Write(BINAWriter writer, int index)
            {
                // Add a string for this light field's name.
                writer.AddString($"LightField{index}Name", Name);

                // Write this light field's shape type.
                writer.Write((byte)Shape);

                // Write this light field's axis aligned bounding box index.
                writer.Write(AxisAlignedBoundingBoxIndex);

                // Write this light field's unknown short value.
                writer.Write(UnknownUShort_1);

                // If this light field's scale is a JObject, then we need to manually convert its values.
                if (Scale.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                {
                    // Set up a new scale object.
                    object scale = new();

                    // Setup the shape and read the keys depending on the shape type.
                    switch (Shape)
                    {
                        case LightFieldShape.Sphere:
                            scale = new SphereScale();
                            (scale as SphereScale).Radius = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Radius");
                            (scale as SphereScale).Margin = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;

                        case LightFieldShape.Capsule:
                            scale = new CapsuleScale();
                            (scale as CapsuleScale).Radius = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Radius");
                            (scale as CapsuleScale).Height = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as CapsuleScale).Margin = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;

                        case LightFieldShape.Box:
                            scale = new BoxScale();
                            (scale as BoxScale).Length = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Length");
                            (scale as BoxScale).Width = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Width");
                            (scale as BoxScale).Height = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as BoxScale).Margin1 = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin1");
                            (scale as BoxScale).Margin2 = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin2");
                            break;

                        case LightFieldShape.OmniBox:
                            scale = new OmniBoxScale();
                            (scale as OmniBoxScale).Length = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Length");
                            (scale as OmniBoxScale).Width = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Width");
                            (scale as OmniBoxScale).Height = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as OmniBoxScale).Margin = (float)(Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;
                    }

                    // Overwrite the JObject with our proper shape.
                    Scale = scale;
                }

                // Write this light field's scale, depending on its shape type.
                switch (Shape)
                {
                    case LightFieldShape.Sphere:
                        writer.Write((Scale as SphereScale).Radius);
                        writer.Write((Scale as SphereScale).Margin);
                        writer.WriteNulls(0x10);
                        break;

                    case LightFieldShape.Capsule:
                        writer.Write((Scale as CapsuleScale).Radius);
                        writer.Write((Scale as CapsuleScale).Height);
                        writer.Write((Scale as CapsuleScale).Margin);
                        writer.WriteNulls(0x0C);
                        break;

                    case LightFieldShape.Box:
                        writer.Write((Scale as BoxScale).Length);
                        writer.Write((Scale as BoxScale).Width);
                        writer.Write((Scale as BoxScale).Height);
                        writer.Write((Scale as BoxScale).Margin1);
                        writer.Write((Scale as BoxScale).Margin2);
                        writer.WriteNulls(0x04);
                        break;

                    case LightFieldShape.OmniBox:
                        writer.Write((Scale as OmniBoxScale).Length);
                        writer.Write((Scale as OmniBoxScale).Width);
                        writer.Write((Scale as OmniBoxScale).Height);
                        writer.Write((Scale as OmniBoxScale).Margin);
                        writer.WriteNulls(0x08);
                        break;
                }

                // Write this light field's position.
                writer.Write(Position);

                // Write this light field's rotation.
                writer.Write(Rotation);

                // Write this light field's unknown integer value.
                writer.Write(UnknownInt32_1);
            }
        }

        public class SphereScale
        {
            public float Radius { get; set; }

            public float Margin { get; set; }
        }

        public class CapsuleScale
        {
            public float Radius { get; set; }

            public float Height { get; set; }

            public float Margin { get; set; }
        }

        public class BoxScale
        {
            public float Length { get; set; }

            public float Width { get; set; }

            public float Height { get; set; }

            public float Margin1 { get; set; }

            public float Margin2 { get; set; }
        }

        public class OmniBoxScale
        {
            public float Length { get; set; }

            public float Width { get; set; }

            public float Height { get; set; }

            public float Margin { get; set; }
        }

        public class LightFieldAxisAlignedBoundingBox
        {
            /// <summary>
            /// Index to the left node in the Light Field AABB tree.
            /// https://hedgedocs.com/docs/hedgehog-engine/sonic2010/files/lightfield/
            /// </summary>
            public int LeftNodeIndex { get; set; }

            /// <summary>
            /// Index to the right node in the Light Field AABB tree. If leftNodeIndex is 0, this field serves as an index back into the Light Field object array.
            /// https://hedgedocs.com/docs/hedgehog-engine/sonic2010/files/lightfield/
            /// </summary>
            public int RightNodeIndex { get; set; }

            /// <summary>
            /// The actual axis aligned bounding box for this entry.
            /// </summary>
            public AABB AxisAlignedBoundingBox { get; set; } = new();

            /// <summary>
            /// Initialises this light field AABB tree with default data.
            /// </summary>
            public LightFieldAxisAlignedBoundingBox() { }

            /// <summary>
            /// Initialises this light field AABB tree with the provided data.
            /// </summary>
            public LightFieldAxisAlignedBoundingBox(int leftNodeIndex, int rightNodeIndex, AABB axisAlignedBoundingBox)
            {
                LeftNodeIndex = leftNodeIndex;
                RightNodeIndex = rightNodeIndex;
                AxisAlignedBoundingBox = axisAlignedBoundingBox;
            }

            /// <summary>
            /// Initialises this light field AABB tree by reading its data from a BINAReader.
            /// </summary>
            public LightFieldAxisAlignedBoundingBox(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this light field AABB tree.
            /// </summary>
            public void Read(BINAReader reader)
            {
                LeftNodeIndex = reader.ReadInt32();
                RightNodeIndex = reader.ReadInt32();
                AxisAlignedBoundingBox = new(reader);
            }

            /// <summary>
            /// Writes the data for this light field AABB tree.
            /// </summary>
            public void Write(BINAWriter writer)
            {
                writer.Write(LeftNodeIndex);
                writer.Write(RightNodeIndex);
                AxisAlignedBoundingBox.Write(writer);
            }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "LIGF");

            // Skip an unknown value that is always 0.
            reader.CheckValue(0x00);

            // Read this data's unknown integer value.
            Data.UnknownUInt32_1 = reader.ReadUInt32();

            // Read the count of light fields in this file.
            uint lightFieldCount = reader.ReadUInt32();

            // Read the offset to this file's light field table.
            uint lightFieldTableOffset = reader.ReadUInt32();

            // Read the count of axis aligned bounding boxes in this file.
            uint axisAlignedBoundingBoxCount = reader.ReadUInt32();

            // Read the offset to this file's axis aligned bounding box table.
            uint axisAlignedBoundingBoxTableOffset = reader.ReadUInt32();

            // Initalise the light field and axis aligned bounding box arrays.
            Data.LightFields = new LightFieldEntry[lightFieldCount];
            Data.AxisAlignedBoundingBoxes = new LightFieldAxisAlignedBoundingBox[axisAlignedBoundingBoxCount];

            // Jump to this file's light field table.
            reader.JumpTo(lightFieldTableOffset, false);

            // Loop through and read each light field entry.
            for (int lightFieldIndex = 0; lightFieldIndex < lightFieldCount; lightFieldIndex++)
                Data.LightFields[lightFieldIndex] = new(reader);

            // Jump to this file's axis aligned bounding box table.
            reader.JumpTo(axisAlignedBoundingBoxTableOffset, false);

            // Loop through and read each axis aligned bounding box.
            for (int axisAlignedBoundingBoxTableIndex = 0; axisAlignedBoundingBoxTableIndex < axisAlignedBoundingBoxCount; axisAlignedBoundingBoxTableIndex++)
                Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex] = new(reader);

            // Close our BinaryWriter.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 1 Header.
            BINAv1Header header = new();

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Write this file's signature.
            writer.Write("LIGF");

            // Write an unknown value of 0.
            writer.Write(0);

            // Write this file's unknown integer value.
            writer.Write(Data.UnknownUInt32_1);

            // Write this file's light field count.
            writer.Write(Data.LightFields.Length);

            // Add an offset to this file's light field table.
            writer.AddOffset("LightFieldTableOffset");

            // Write this file's axis aligned bounding box count.
            writer.Write(Data.AxisAlignedBoundingBoxes.Length);

            // Add an offset to this file's axis aligned bounding box table.
            writer.AddOffset("AABBTableOffset");

            // Fill in the offset to this file's light field table.
            writer.FillInOffset("LightFieldTableOffset", false);

            // Loop through each light field in this file.
            for (int lightFieldIndex = 0; lightFieldIndex < Data.LightFields.Length; lightFieldIndex++)
            {
                // Write this light field.
                Data.LightFields[lightFieldIndex].Write(writer, lightFieldIndex);

                // If this is the final light field in this file, then realign to 0x10 bytes.
                if (lightFieldIndex == Data.LightFields.Length - 1)
                    writer.FixPadding(0x10);
            }

            // Fill in the offset to this file's axis aligned bounding box table.
            writer.FillInOffset("AABBTableOffset", false);

            // Loop through and write each of this file's axis aligned bounding boxes.
            for (int axisAlignedBoundingBoxTableIndex = 0; axisAlignedBoundingBoxTableIndex < Data.AxisAlignedBoundingBoxes.Length; axisAlignedBoundingBoxTableIndex++)
                Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex].Write(writer);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
