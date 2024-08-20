namespace KnuxLib.Engines.Hedgehog
{
    // TODO: There is extra data after the Sector Table but before the String Table.
    // What is it? The game seems to work without it?
    // The BINA stuff is correct even with this missing, which would imply no offset or anything points to it.
    public class SectorVisibilityCollision_Wars : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SectorVisibilityCollision_Wars() { }
        public SectorVisibilityCollision_Wars(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.sectorvisiblitycollision_wars.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<SectorVisibilityShape[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.svcol.bin");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
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
            public SectorVisiblitySector[] Sectors { get; set; } = [];

            /// <summary>
            /// Displays this shape's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this shape with default data.
            /// </summary>
            public SectorVisibilityShape() { }

            /// <summary>
            /// Initialises this shape with the provided data.
            /// </summary>
            public SectorVisibilityShape(string name, uint unknownUInt32_1, Vector3 size, Vector3 position, Quaternion rotation, AABB axisAlignedBoundingBox, SectorVisiblitySector[] sectors)
            {
                Name = name;
                UnknownUInt32_1 = unknownUInt32_1;
                Size = size;
                Position = position;
                Rotation = rotation;
                AxisAlignedBoundingBox = axisAlignedBoundingBox;
                Sectors = sectors;
            }

            /// <summary>
            /// Initialises this shape by reading its data from a BINAReader.
            /// </summary>
            public SectorVisibilityShape(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this shape.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read this shape's name.
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                // Read this shape's unknown integer value.
                UnknownUInt32_1 = reader.ReadUInt32();

                // Read this shape's size.
                Size = reader.ReadVector3();

                // Read this shape's position.
                Position = reader.ReadVector3();

                // Read this shape's rotation.
                Rotation = reader.ReadQuaternion();

                // Read this shape's axis aligned bounding box.
                AxisAlignedBoundingBox = new(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this shape's sector count.
                Sectors = new SectorVisiblitySector[reader.ReadUInt64()];

                // Read the offset to this shape's sector list.
                long sectorListOffset = reader.ReadInt64();

                // Save our position to jump back for the next shape.
                long position = reader.BaseStream.Position;

                // Jump to this shape's sector list.
                reader.JumpTo(sectorListOffset, false);

                for (int sectorIndex = 0; sectorIndex < Sectors.Length; sectorIndex++)
                    Sectors[sectorIndex] = new(reader);

                // Jump back for the next shape.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the data for this shape.
            /// </summary>
            public void WriteData(BINAWriter writer, int index)
            {
                // Add an offset for this shape's name.
                writer.AddString($"shape{index}name", Name, 0x08);

                // Write this shape's unknown integer value.
                writer.Write(UnknownUInt32_1);

                // Write this shape's size value.
                writer.Write(Size);

                // Write this shape's position value.
                writer.Write(Position);

                // Write this shape's rotation value.
                writer.Write(Rotation);

                // Write this shape's axis aligned bounding box.
                AxisAlignedBoundingBox.Write(writer);

                // Write an unknown value of 0.
                writer.Write(0x00);

                // Write the amount of sectors in this shape.
                writer.Write((ulong)Sectors.Length);

                // Add an offset for this shape's sector table.
                writer.AddOffset($"shape{index}SectorTableOffset", 0x08);
            }

            /// <summary>
            /// Writes the sector table for this shape.
            /// </summary>
            public void WriteSectors(BINAWriter writer, int index)
            {
                // Fill in this shape's offset.
                writer.FillInOffset($"shape{index}SectorTableOffset", false, false);

                // Loop through each sector in this shape.
                foreach (SectorVisiblitySector sector in Sectors)
                    sector.Write(writer);
            }
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

            /// <summary>
            /// Displays this sector's index and visibility in the debugger.
            /// </summary>
            public override string ToString() => $"Sector {Index} visiblity is {Visible}.";

            /// <summary>
            /// Initialises this sector with default data.
            /// </summary>
            public SectorVisiblitySector() { }

            /// <summary>
            /// Initialises this sector with the provided data.
            /// </summary>
            public SectorVisiblitySector(byte index, bool visible)
            {
                Index = index;
                Visible = visible;
            }

            /// <summary>
            /// Initialises this sector by reading its data from a BINAReader.
            /// </summary>
            public SectorVisiblitySector(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this sector.
            /// </summary>
            public void Read(BINAReader reader)
            {
                Index = reader.ReadByte();
                Visible = reader.ReadBoolean();
            }

            /// <summary>
            /// Writes the data for this sector.
            /// </summary>
            public void Write(BINAWriter writer)
            {
                writer.Write(Index);
                writer.Write(Visible);
            }
        }

        // Actual data presented to the end user.
        public SectorVisibilityShape[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "OCVS");

            // Skip an unknown value of 0x01.
            reader.JumpAhead(0x04);

            // Read the amount of shapes in this svcol.
            Data = new SectorVisibilityShape[reader.ReadUInt64()];

            // Read the offset to this file's shape table.
            long shapeTableOffset = reader.ReadInt64();

            // Jump to this file's shape table.
            reader.JumpTo(shapeTableOffset, false);

            // Loop through and read each shape.
            for (int shapeIndex = 0; shapeIndex < Data.Length; shapeIndex++)
                Data[shapeIndex] = new(reader);

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(210);

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Write this file's signature.
            writer.Write("OCVS");

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            // Write the amount of shapes in this file.
            writer.Write((ulong)Data.Length);

            // Add an offset to the shape table.
            writer.AddOffset("shapeTableOffset", 0x08);

            // Fill in the offset position.
            writer.FillInOffset("shapeTableOffset", false, false);

            // Loop through and write each shape's data.
            for (int shapeIndex = 0; shapeIndex < Data.Length; shapeIndex++)
                Data[shapeIndex].WriteData(writer, shapeIndex);

            // Loop through and write each shape's sector table.
            for (int shapeIndex = 0; shapeIndex < Data.Length; shapeIndex++)
                Data[shapeIndex].WriteSectors(writer, shapeIndex);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
