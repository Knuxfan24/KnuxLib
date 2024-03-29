﻿namespace KnuxLib.Engines.Hedgehog
{
    // TODO: There is extra data after the Sector Table but before the String Table. What is it? The game seems to work without it?
    public class SectorVisibilityCollision_Wars : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SectorVisibilityCollision_Wars() { }
        public SectorVisibilityCollision_Wars(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".svcol", "")}.hedgehog.sectorvisiblitycollision_wars.json", Data);
        }

        // Classes for this format.
        public class SectorVisibilityShape
        {
            /// <summary>
            /// This shape's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// This shape's size.
            /// </summary>
            public Vector3 Size { get; set; }

            /// <summary>
            /// This shape's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This shape's rotation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This shape's axis aligned bounding box.
            /// </summary>
            public AABB AxisAlignedBoundingBox { get; set; } = new();

            /// <summary>
            /// A list of sectors this shape controls.
            /// </summary>
            public List<SectorVisiblitySector> Sectors { get; set; } = new();

            public override string ToString() => Name;
        }

        public class SectorVisiblitySector
        {
            /// <summary>
            /// The index of this sector.
            /// </summary>
            public byte Index { get; set; }

            /// <summary>
            /// Whether this sector is visible when in this shape or not.
            /// </summary>
            public bool Visible { get; set; }

            public override string ToString() => $"Sector {Index} visiblity is {Visible}.";
        }

        // Actual data presented to the end user.
        public List<SectorVisibilityShape> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "OCVS";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read the amount of shapes in this svcol.
            ulong shapeCount = reader.ReadUInt64();

            // Read the offset to this file's shape table.
            long shapeTableOffset = reader.ReadInt64();

            // Jump to this file's shape table.
            reader.JumpTo(shapeTableOffset, false);

            // Loop through and read each shape.
            for (ulong shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
            {
                // Set up a new shape.
                SectorVisibilityShape shape = new();

                // Read this shape's name.
                shape.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this shape's unknown integer value.
                shape.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this shape's size.
                shape.Size = Helpers.ReadHedgeLibVector3(reader);

                // Read this shape's position.
                shape.Position = Helpers.ReadHedgeLibVector3(reader);

                // Read this shape's rotation.
                shape.Rotation = Helpers.ReadHedgeLibQuaternion(reader);

                // Read this shape's axis aligned bounding box.
                shape.AxisAlignedBoundingBox.Min = Helpers.ReadHedgeLibVector3(reader);
                shape.AxisAlignedBoundingBox.Max = Helpers.ReadHedgeLibVector3(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this shape's sector count.
                ulong sectorCount = reader.ReadUInt64();

                // Read the offset to this shape's sector list.
                long sectorListOffset = reader.ReadInt64();

                // Save our position to jump back for the next shape.
                long position = reader.BaseStream.Position;

                // Jump to this shape's sector list.
                reader.JumpTo(sectorListOffset, false);

                // Loop through and read each of the sectors in this shape.
                for (ulong sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
                {
                    // Set up a new sector entry.
                    SectorVisiblitySector sector = new();

                    // Read this sector's index.
                    sector.Index = reader.ReadByte();

                    // Read this sector's visiblity.
                    sector.Visible = reader.ReadBoolean();

                    // Save this sector.
                    shape.Sectors.Add(sector);
                }

                // Jump back for the next shape.
                reader.JumpTo(position);

                // Save this shape.
                Data.Add(shape);
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

            // Write the amount of shapes in this file.
            writer.Write((ulong)Data.Count);

            // Add an offset to the shape table.
            writer.AddOffset("shapeTableOffset", 0x08);

            // Fill in the offset position.
            writer.FillInOffset("shapeTableOffset", false, false);

            // Loop through and write each shape's data.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add an offset for this shape's name.
                writer.AddString($"shape{dataIndex}name", Data[dataIndex].Name, 0x08);

                // Write this shape's unknown integer value.
                writer.Write(Data[dataIndex].UnknownUInt32_1);

                // Write this shape's size value.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Size);

                // Write this shape's position value.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Position);

                // Write this shape's rotation value.
                Helpers.WriteHedgeLibQuaternion(writer, Data[dataIndex].Rotation);

                // Write this shape's axis aligned bounding box.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].AxisAlignedBoundingBox.Min);
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].AxisAlignedBoundingBox.Max);

                // Write an unknown value of 0.
                writer.Write(0x00);

                // Write the amount of sectors in this shape.
                writer.Write((ulong)Data[dataIndex].Sectors.Count);

                // Add an offset for this shape's sector table.
                writer.AddOffset($"shape{dataIndex}SectorTableOffset", 0x08);
            }

            // Loop through and write each shape's sector table.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Fill in this shape's offset.
                writer.FillInOffset($"shape{dataIndex}SectorTableOffset", false, false);

                // Loop through each sector in this shape.
                foreach (SectorVisiblitySector sector in Data[dataIndex].Sectors)
                {
                    // Write this sector's index.
                    writer.Write(sector.Index);

                    // Write this sector's visiblity value.
                    writer.Write(sector.Visible);
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
