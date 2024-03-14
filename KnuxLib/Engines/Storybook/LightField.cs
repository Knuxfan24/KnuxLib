using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Storybook
{
    public class LightField : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LightField() { }
        public LightField(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.storybook.lightfield.json", Data);
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
            public LightFieldEntry[] LightFields { get; set; } = Array.Empty<LightFieldEntry>();

            /// <summary>
            /// The axis aligned bounding boxes in this file.
            /// </summary>
            public LightFieldAxisAlignedBoundingBox[] AxisAlignedBoundingBoxes { get; set; } = Array.Empty<LightFieldAxisAlignedBoundingBox>();
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
            /// An unknown byte value.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownByte_1 { get; set; }

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
            /// TODO: What is this? Light Index? Need to RE the LIGHT.BIN format for that.
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            public override string ToString() => Name.ToString();
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
            public uint LeftNodeIndex { get; set; }

            /// <summary>
            /// Index to the right node in the Light Field AABB tree. If leftNodeIndex is 0, this field serves as an index back into the Light Field object array.
            /// https://hedgedocs.com/docs/hedgehog-engine/sonic2010/files/lightfield/
            /// </summary>
            public uint RightNodeIndex { get; set; }

            /// <summary>
            /// The actual axis aligned bounding box for this entry.
            /// </summary>
            public AABB AxisAlignedBoundingBox { get; set; } = new();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV1Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv1Header();

        // Set up the Signature we expect.
        public new const string Signature = "LIGF";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader and set its offset to the length of the BINA Header.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true) { Offset = 0x20 };

            // Skip the BINA Header, as this format lacks the BINA signature, so HedgeLib# doesn't accept it.
            reader.JumpTo(0x20);

            // Read the LIGF signature in this file.
            reader.ReadSignature(4, Signature);

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

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
            reader.JumpTo(lightFieldTableOffset, true);

            // Loop through each light field entry.
            for (int lightFieldIndex = 0; lightFieldIndex < lightFieldCount; lightFieldIndex++)
            {
                // Set up a new light field entry.
                LightFieldEntry lightField = new();

                // Read this light field's name.
                lightField.Name = Helpers.ReadNullTerminatedStringTableEntry(reader, true);

                // Read this light field's shape type.
                lightField.Shape = (LightFieldShape)reader.ReadByte();

                // Read the index for this light field's axis aligned bounding box.
                lightField.AxisAlignedBoundingBoxIndex = reader.ReadByte();

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x01);

                // Read this light field's unknown byte value.
                lightField.UnknownByte_1 = reader.ReadByte();

                // Read this light field's scale, depending on its shape.
                switch (lightField.Shape)
                {
                    case LightFieldShape.Sphere:
                        lightField.Scale = new SphereScale()
                        {
                            Radius = reader.ReadSingle(),
                            Margin = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x10);
                        break;

                    case LightFieldShape.Capsule:
                        lightField.Scale = new CapsuleScale()
                        {
                            Radius = reader.ReadSingle(),
                            Height = reader.ReadSingle(),
                            Margin = reader.ReadSingle()
                        };
                        reader.JumpAhead(0x0C);
                        break;

                    case LightFieldShape.Box:
                        lightField.Scale = new BoxScale()
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
                        lightField.Scale = new OmniBoxScale()
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
                lightField.Position = reader.ReadVector3();

                // Read this light field's rotation.
                lightField.Rotation = reader.ReadQuaternion();

                // Read this light field's unknown integer value.
                lightField.UnknownUInt32_1 = reader.ReadUInt32();

                // Save this light field.
                Data.LightFields[lightFieldIndex] = lightField;
            }

            // Jump to this file's axis aligned bounding box table.
            reader.JumpTo(axisAlignedBoundingBoxTableOffset, true);

            // Loop through each axis aligned bounding box.
            for (int axisAlignedBoundingBoxTableIndex = 0; axisAlignedBoundingBoxTableIndex < axisAlignedBoundingBoxCount; axisAlignedBoundingBoxTableIndex++)
            {
                // Set up a new axis aligned bounding box entry.
                LightFieldAxisAlignedBoundingBox aabb = new();

                // Read this axis aligned bounding box's left node index.
                aabb.LeftNodeIndex = reader.ReadUInt32();

                // Read this axis aligned bounding box's right node index.
                aabb.RightNodeIndex = reader.ReadUInt32();

                // Read this axis aligned bounding box.
                aabb.AxisAlignedBoundingBox.Min = reader.ReadVector3();
                aabb.AxisAlignedBoundingBox.Max = reader.ReadVector3();

                // Save this axis aligned bounding box.
                Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex] = aabb;
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
            // Set up our BINAWriter and write the BINAV1 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this file's signature.
            writer.WriteSignature(Signature);

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
                // Add a string for this light field's name.
                writer.AddString($"LightField{lightFieldIndex}Name", Data.LightFields[lightFieldIndex].Name);

                // Write this light field's shape type.
                writer.Write((byte)Data.LightFields[lightFieldIndex].Shape);

                // Write this light field's axis aligned bounding box index.
                writer.Write(Data.LightFields[lightFieldIndex].AxisAlignedBoundingBoxIndex);

                // Write an unknown value of 0.
                writer.Write((byte)0);

                // Write this light field's unknown byte value.
                writer.Write(Data.LightFields[lightFieldIndex].UnknownByte_1);

                // If this light field's scale is a JObject, then we need to manually convert its values.
                if (Data.LightFields[lightFieldIndex].Scale.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                {
                    // Set up a new scale object.
                    object scale = new();

                    // Setup the shape and read the keys depending on the shape type.
                    switch (Data.LightFields[lightFieldIndex].Shape)
                    {
                        case LightFieldShape.Sphere:
                            scale = new SphereScale();
                            (scale as SphereScale).Radius = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Radius");
                            (scale as SphereScale).Margin = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;

                        case LightFieldShape.Capsule:
                            scale = new CapsuleScale();
                            (scale as CapsuleScale).Radius = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Radius");
                            (scale as CapsuleScale).Height = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as CapsuleScale).Margin = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;

                        case LightFieldShape.Box:
                            scale = new BoxScale();
                            (scale as BoxScale).Length = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Length");
                            (scale as BoxScale).Width = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Width");
                            (scale as BoxScale).Height = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as BoxScale).Margin1 = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin1");
                            (scale as BoxScale).Margin2 = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin2");
                            break;

                        case LightFieldShape.OmniBox:
                            scale = new OmniBoxScale();
                            (scale as OmniBoxScale).Length = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Length");
                            (scale as OmniBoxScale).Width = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Width");
                            (scale as OmniBoxScale).Height = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Height");
                            (scale as OmniBoxScale).Margin = (float)(Data.LightFields[lightFieldIndex].Scale as Newtonsoft.Json.Linq.JObject).SelectToken("Margin");
                            break;
                    }

                    // Overwrite the JObject with our proper shape.
                    Data.LightFields[lightFieldIndex].Scale = scale;
                }

                // Write this light field's scale, depending on its shape type.
                switch (Data.LightFields[lightFieldIndex].Shape)
                {
                    case LightFieldShape.Sphere:
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as SphereScale).Radius);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as SphereScale).Margin);
                        writer.WriteNulls(0x10);
                        break;

                    case LightFieldShape.Capsule:
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as CapsuleScale).Radius);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as CapsuleScale).Height);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as CapsuleScale).Margin);
                        writer.WriteNulls(0x0C);
                        break;

                    case LightFieldShape.Box:
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as BoxScale).Length);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as BoxScale).Width);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as BoxScale).Height);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as BoxScale).Margin1);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as BoxScale).Margin2);
                        writer.WriteNulls(0x04);
                        break;

                    case LightFieldShape.OmniBox:
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as OmniBoxScale).Length);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as OmniBoxScale).Width);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as OmniBoxScale).Height);
                        writer.Write((Data.LightFields[lightFieldIndex].Scale as OmniBoxScale).Margin);
                        writer.WriteNulls(0x08);
                        break;
                }

                // Write this light field's position.
                Helpers.WriteHedgeLibVector3(writer, Data.LightFields[lightFieldIndex].Position);

                // Write this light field's rotation.
                Helpers.WriteHedgeLibQuaternion(writer, Data.LightFields[lightFieldIndex].Rotation);

                // Write this light field's unknown integer value.
                writer.Write(Data.LightFields[lightFieldIndex].UnknownUInt32_1);

                // If this is the final light field in this file, then realign to 0x10 bytes.
                if (lightFieldIndex == Data.LightFields.Length - 1)
                    writer.FixPadding(0x10);
            }

            // Fill in the offset to this file's axis aligned bounding box table.
            writer.FillInOffset("AABBTableOffset", false);

            // Loop through each of this file's axis aligned bounding boxes.
            for (int axisAlignedBoundingBoxTableIndex = 0; axisAlignedBoundingBoxTableIndex < Data.AxisAlignedBoundingBoxes.Length; axisAlignedBoundingBoxTableIndex++)
            {
                // Write this axis aligned bounding box's left node index.
                writer.Write(Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex].LeftNodeIndex);

                // Write this axis aligned bounding box's right node index.
                writer.Write(Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex].RightNodeIndex);

                // Write this axis aligned bounding box.
                Helpers.WriteHedgeLibVector3(writer, Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex].AxisAlignedBoundingBox.Min);
                Helpers.WriteHedgeLibVector3(writer, Data.AxisAlignedBoundingBoxes[axisAlignedBoundingBoxTableIndex].AxisAlignedBoundingBox.Max);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
