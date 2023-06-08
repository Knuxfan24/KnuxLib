using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out what the data in the BSP Nodes is, as it seems to be two values that can alternate between an integer and a floating point number.
    // TOOD: Figure out what the values are for.
    // TODO: Attempt to support the older version of the format seen in Sonic Lost World.
    public class SceneEffectCollision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SceneEffectCollision() { }
        public SceneEffectCollision(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".fxcol", "")}.hedgehog.sceneeffectcollision.json", Data);
        }

        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SceneEffectShapeType : byte
        {
            Sphere = 0,
            Capsule = 1,
            Box = 2,
            OmniBox = 3
        }

        public class FormatData
        {
            /// <summary>
            /// A list of shapes in this fxcol file.
            /// </summary>
            public List<EffectVisiblityShape> Shapes { get; set; } = new();

            /// <summary>
            /// A list of groups in this fxcol file.
            /// </summary>
            public List<EffectVisiblityGroup> Groups { get; set; } = new();

            /// <summary>
            /// A list of BSP nodes in this fxcol file.
            /// TODO: These are clearly not ulongs, it's just convenient to store them as this for now until the actual data is figured out.
            /// </summary>
            public List<ulong> BSPNodes { get; set; } = new();
        }

        public class EffectVisiblityShape
        {
            /// <summary>
            /// The name of this shape.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The type of this shape.
            /// </summary>
            public SceneEffectShapeType Type { get; set; }

            /// <summary>
            /// Whether this shape has a light?
            /// TODO: What actually is this?
            /// </summary>
            public bool HasLight { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Should this and UnknownVector3_2 actually be treated as one entity?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Should this and UnknownVector3_1 actually be treated as one entity?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }

            /// <summary>
            /// This shape's group index.
            /// TODO: What is? The index of the scene to use from the RFL?
            /// </summary>
            public uint GroupIndex { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// This shape's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This shape's rotation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

            public override string ToString() => Name;
        }

        public class EffectVisiblityGroup
        {
            /// <summary>
            /// The amount of BSP nodes that make up this group.
            /// </summary>
            public uint BSPNodeCount { get; set; }

            /// <summary>
            /// The index of the first BSP node in this group.
            /// </summary>
            public uint BSPFirstNodeIndex { get; set; }

            /// <summary>
            /// This group's Axis-Aligned Bounding Box.
            /// </summary>
            public float[] AABB { get; set; } = new float[6];
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "OCXF";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read this file's shape count.
            ulong shapeCount = reader.ReadUInt64();

            // Read this file's offset to the shape table.
            long shapeTableOffset = reader.ReadInt64();

            // Read this file's group count.
            ulong groupCount = reader.ReadUInt64();

            // Read this file's offset to the group table.
            long groupTableOffset = reader.ReadInt64();

            // Read this file's BSP node count.
            ulong bspNodeCount = reader.ReadUInt64();

            // Read this file's offset to the BSP node table.
            long bspNodeTableOffset = reader.ReadInt64();

            // Jump to this file's shape table.
            reader.JumpTo(shapeTableOffset, false);

            // Loop through each shape in this file.
            for (ulong i = 0; i < shapeCount; i++)
            {
                // Set up a new shape.
                EffectVisiblityShape shape = new();

                // Read this shape's name.
                shape.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this shape's type.
                shape.Type = (SceneEffectShapeType)reader.ReadByte();

                // Read this shape's HasLight value.
                shape.HasLight = reader.ReadBoolean();

                // Realign to 0x04 bytes.
                reader.FixPadding(0x04);

                // Read this shape's first unknown Vector3.
                shape.UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);

                // Read this shape's second unknown Vector3.
                shape.UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);

                // Read this shape's group index.
                shape.GroupIndex = reader.ReadUInt32();

                // Read this shape's unknown integer value.
                shape.UnknownUInt32_1 = reader.ReadUInt32();

                // Skip 0x1C bytes that are always 0.
                reader.JumpAhead(0x1C);

                // Skip the offset for the "none" string, as every shape uses it.
                reader.JumpAhead(0x08);

                // Read this shape's position.
                shape.Position = Helpers.ReadHedgeLibVector3(reader);

                // Read this shape's rotation.
                shape.Rotation = Helpers.ReadHedgeLibQuaternion(reader);

                // Skip four bytes of padding (the last shape doesn't have these, but we jump right after reading the last shape so it's not a problem).
                reader.JumpAhead(0x04);

                // Save this shape.
                Data.Shapes.Add(shape);
            }

            // Jump to this file's BSP node table.
            reader.JumpTo(bspNodeTableOffset, false);

            // Read the specified amount of ulongs.
            // TODO: These are clearly not ulongs and are actually two different values, what are they?
            for (ulong i = 0; i < bspNodeCount; i++)
                Data.BSPNodes.Add(reader.ReadUInt64());

            // Jump to this file's group table.
            reader.JumpTo(groupTableOffset, false);

            // Loop through each group in this file.
            for (ulong i = 0; i < groupCount; i++)
            {
                // Set up a new group.
                EffectVisiblityGroup group = new();

                // Read this group's node count.
                group.BSPNodeCount = reader.ReadUInt32();

                // Read this group's first BSP node index.
                group.BSPFirstNodeIndex = reader.ReadUInt32();

                // Loop through and read the six values of this group's Axis-Aligned Bounding Box.
                for (int aabb = 0; aabb < 6; aabb++)
                    group.AABB[aabb] = reader.ReadSingle();

                // Save this group.
                Data.Groups.Add(group);
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
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this file's signature.
            writer.WriteSignature(Signature);

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            // Write this file's shape count.
            writer.Write((ulong)Data.Shapes.Count);

            // Add an offset for this file's shape table.
            writer.AddOffset("shapeTableOffset", 0x08);

            // Write this file's group count.
            writer.Write((ulong)Data.Groups.Count);

            // Add an offset for this file's group table.
            writer.AddOffset("groupTableOffset", 0x08);

            // Write this file's BSP node count.
            writer.Write((ulong)Data.BSPNodes.Count);

            // Add an offset for this file's BSP node table.
            writer.AddOffset("bspNodeTableOffset", 0x08);

            // Fill in the offset for this file's shape table.
            writer.FillInOffset("shapeTableOffset", false, false);

            // Loop through and write the data for each shape.
            for (int i = 0; i < Data.Shapes.Count; i++)
            {
                // Add an offset for this shape's name.
                writer.AddString($"shape{i}name", Data.Shapes[i].Name, 0x08);

                // Write this shape's type.
                writer.Write((byte)Data.Shapes[i].Type);

                // Write this shape's HasLight value.
                writer.Write(Data.Shapes[i].HasLight);

                // Realign to 0x04 bytes.
                writer.FixPadding(0x04);

                // Write this shape's first unknown Vector3.
                Helpers.WriteHedgeLibVector3(writer, Data.Shapes[i].UnknownVector3_1);

                // Write this shape's second unknown Vector3.
                Helpers.WriteHedgeLibVector3(writer, Data.Shapes[i].UnknownVector3_2);

                // Write this shape's group index.
                writer.Write(Data.Shapes[i].GroupIndex);

                // Write this shape's unknown integer value.
                writer.Write(Data.Shapes[i].UnknownUInt32_1);

                // Write 0x1C bytes of nulls.
                writer.WriteNulls(0x1C);

                // Add an offset for this shape's "none" string.
                writer.AddString($"shape{i}none", "none", 0x08);

                // Write this shape's position.
                Helpers.WriteHedgeLibVector3(writer, Data.Shapes[i].Position);

                // Write this shape's rotation.
                Helpers.WriteHedgeLibQuaternion(writer, Data.Shapes[i].Rotation);

                // If this is NOT the last shape, then write an extra four bytes as padding.
                if (i < Data.Shapes.Count - 1)
                    writer.Write(0x00);
            }

            // Fill in the offset for this file's BSP node table.
            writer.FillInOffset("bspNodeTableOffset", false, false);

            // Write each BSP node's temporary ulong.
            for (int i = 0; i < Data.BSPNodes.Count; i++)
                writer.Write(Data.BSPNodes[i]);

            // Fill in the offset for this file's group table.
            writer.FillInOffset("groupTableOffset", false, false);

            // Loop through and write the data for each group.
            for (int i = 0; i < Data.Groups.Count; i++)
            {
                // Write this group's BSP node count.
                writer.Write(Data.Groups[i].BSPNodeCount);

                // Write the index of this group's first BSP node.
                writer.Write(Data.Groups[i].BSPFirstNodeIndex);

                // Loop through and write the six values of this group's Axis-Aligned Bounding Box.
                for (int aabb = 0; aabb < 6; aabb++)
                    writer.Write(Data.Groups[i].AABB[aabb]);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
