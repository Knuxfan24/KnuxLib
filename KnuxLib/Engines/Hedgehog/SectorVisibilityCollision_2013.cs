namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Confirm that this is correct.
    public class SectorVisibilityCollision_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SectorVisibilityCollision_2013() { }
        public SectorVisibilityCollision_2013(string filepath, bool export = false, bool bigEndianSave = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.sectorvisiblitycollision_2013.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<SectorVisibilityShape[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.svcol.bin", bigEndianSave);
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
            /// An unknown integer value.
            /// TODO: What is this? In terrain shapes the offset to the sectors table is here instead.
            /// </summary>
            public uint? UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An array of sectors that are visibile when in this shape.
            /// </summary>
            public short[]? Sectors { get; set; }

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
            public SectorVisibilityShape(string name, Vector3 size, Vector3 position, Quaternion rotation, AABB axisAlignedBoundingBox, uint? unknownUInt32_1, short[]? sectors)
            {
                Name = name;
                Size = size;
                Position = position;
                Rotation = rotation;
                AxisAlignedBoundingBox = axisAlignedBoundingBox;
                UnknownUInt32_1 = unknownUInt32_1;
                Sectors = sectors;
            }

            /// <summary>
            /// Initialises this shape by reading its data from a BINAReader.
            /// </summary>
            public SectorVisibilityShape(BINAReader reader, bool isTerrain) => Read(reader, isTerrain);

            /// <summary>
            /// Reads the data for this shape.
            /// </summary>
            public void Read(BINAReader reader, bool isTerrain)
            {
                Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                reader.CheckValue((byte)0x02);
                reader.FixPadding(0x04);
                Size = reader.ReadVector3();
                Position = reader.ReadVector3();
                Rotation = reader.ReadQuaternion();
                AxisAlignedBoundingBox = new(reader);

                if (!isTerrain)
                    UnknownUInt32_1 = reader.ReadUInt32();

                else
                {
                    // Read the offset to this terrain shape's sector table.
                    uint sectorTableOffset = reader.ReadUInt32();

                    // Skip an unknown value that is always this terrain shape's index, followed by 12 0xFF bytes.
                    reader.JumpAhead(0x10);

                    // Save our current position so we can jump back for the next shape.
                    long position = reader.BaseStream.Position;

                    // Jump to this terrain shape's sector table.
                    reader.JumpTo(sectorTableOffset, false);

                    // Initialise the sectors array.
                    Sectors = new short[10];

                    // Loop through and read each sector for this terrain shape.
                    for (int sectorIndex = 0; sectorIndex < Sectors.Length; sectorIndex++)
                        Sectors[sectorIndex] = reader.ReadInt16();

                    // Jump back for the next terrain.
                    reader.JumpTo(position);
                }
            }

            /// <summary>
            /// Writes the data for this shape.
            /// </summary>
            public void Write(BINAWriter writer, int index, int actualIndex = 0)
            {
                writer.AddString($"Shape{index}Name", Name);
                writer.Write((byte)0x02);
                writer.FixPadding(0x04);
                writer.Write(Size);
                writer.Write(Position);
                writer.Write(Rotation);
                AxisAlignedBoundingBox.Write(writer);

                if (UnknownUInt32_1 != null)
                    writer.Write(UnknownUInt32_1.Value);
                else
                {
                    writer.AddOffset($"Shape{index}SectorTable");
                    writer.Write(actualIndex);
                    writer.Write([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);
                }
            }

            /// <summary>
            /// Writes the sector table for this shape.
            /// </summary>
            public void WriteSectorTable(BINAWriter writer, int index)
            {
                writer.FillInOffset($"Shape{index}SectorTable");

                foreach (short sectorIndex in Sectors)
                    writer.Write(sectorIndex);
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
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "TRSV");

            // Skip an unknown value of 0x1010000.
            reader.CheckValue(0x1010000);

            // Read the count of standard shapes in this file.
            uint shapeCount = reader.ReadUInt32();

            // Read the offset to the standard shape table.
            uint shapeTableOffset = reader.ReadUInt32();

            // Read the count of terrain shapes in this file.
            uint terrainShapeCount = reader.ReadUInt32();

            // Read the offset to the terrain shape table.
            uint terrainShapeTableOffset = reader.ReadUInt32();

            // Initialise the data array.
            Data = new SectorVisibilityShape[shapeCount + terrainShapeCount];

            // Skip an unknown value of 0.
            reader.CheckValue(0x00);

            // Jump to the standard shape table (should already be here but lets play it safe).
            reader.JumpTo(shapeTableOffset, false);
            
            // Loop through and read each standard shape.
            for (int shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
                Data[shapeIndex] = new(reader, false);

            // Jump to the terrain shape table (should already be here but lets play it safe).
            reader.JumpTo(terrainShapeTableOffset, false);

            // Loop through and read each terrain shape.
            for (int shapeIndex = 0; shapeIndex < terrainShapeCount; shapeIndex++)
                Data[shapeIndex + shapeCount] = new(reader, true);

            // Close our BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="bigEndianSave">Whether this format should be saved in big endian for the Wii U version.</param>
        public void Save(string filepath, bool bigEndianSave = false)
        {
            // Set up lists to seperate the two shape types.
            List<SectorVisibilityShape> nonTerrainShapes = [];
            List<SectorVisibilityShape> terrainShapes = [];

            // Loop through each shape and add it to the approriate list.
            foreach (SectorVisibilityShape shape in Data)
                if (shape.UnknownUInt32_1 != null)
                    nonTerrainShapes.Add(shape);
                else
                    terrainShapes.Add(shape);

            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(200, bigEndianSave);

            // Set up our BINAWriter and write the BINAV2 header.
            BINAWriter writer = new(File.Create(filepath), header);

            // If the user has specified it, then switch the reader to big endian.
            writer.IsBigEndian = bigEndianSave;

            // Write this file's signature.
            writer.Write("TRSV");

            // Write an unknown value of 0x1010000.
            writer.Write(0x1010000);

            // Write the count of non-terrain shapes.
            writer.Write(nonTerrainShapes.Count);

            // Add an offset to the table for the non-terrain shapes.
            writer.AddOffset("NonTerrainShapeTableOffset");

            // Write the count of terrain shapes.
            writer.Write(terrainShapes.Count);

            // Add an offset to the table for the terrain shapes.
            writer.AddOffset("TerrainShapeTableOffset");

            // Write an unknown value of 0.
            writer.Write(0x00);

            // Fill in the offset to the non-terrain shape table.
            writer.FillInOffset("NonTerrainShapeTableOffset");

            // Loop through and write each non-terrain shape.
            for (int shapeIndex = 0; shapeIndex < nonTerrainShapes.Count; shapeIndex++)
                nonTerrainShapes[shapeIndex].Write(writer, shapeIndex);

            // Fill in the offset to the terrain shape table.
            writer.FillInOffset("TerrainShapeTableOffset");

            // Loop through and write each terrain shape.
            for (int shapeIndex = 0; shapeIndex < terrainShapes.Count; shapeIndex++)
                terrainShapes[shapeIndex].Write(writer, shapeIndex + nonTerrainShapes.Count, shapeIndex);

            // Loop through and write each terrain shape's sector table.
            for (int shapeIndex = 0; shapeIndex < terrainShapes.Count; shapeIndex++)
                terrainShapes[shapeIndex].WriteSectorTable(writer, shapeIndex + nonTerrainShapes.Count);

            // Close our BINAWriter.
            writer.Close(header);
        }
    }
}
