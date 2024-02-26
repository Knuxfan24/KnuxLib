namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out the unknowns.
    // TODO: Figure out what's going on with the colour value.
    // TODO: Maybe make a way to visualise positions in a 3D format like an OBJ? Same with importing?
    public class GrassInstance : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public GrassInstance() { }
        public GrassInstance(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".grass", "")}.hedgehog.grassinstance.json", Data);
        }

        // Classes for this format.
        public class Grass
        {
            /// <summary>
            /// The model this grass instance should use.
            /// </summary>
            public string GrassModel { get; set; } = "";

            /// <summary>
            /// The model this grass instance should attach to.
            /// </summary>
            public string TargetModel { get; set; } = "";

            /// <summary>
            /// The material that should be applied to this grass instance's model.
            /// </summary>
            public string Material { get; set; } = "";

            /// <summary>
            /// An unknown 4x4 Matrix.
            /// TODO: What is this for?
            /// </summary>
            public Matrix4x4 UnknownMatrix_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An array of grass group entries for this instance.
            /// </summary>
            public GrassGroup[] GrassGroups { get; set; } = Array.Empty<GrassGroup>();

            /// <summary>
            /// A copy of this grass instance's unknown 4x4 Matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();
        }

        public class GrassGroup
        {
            /// <summary>
            /// This grass group's axis aligned bounding box.
            /// </summary>
            public AABB AxisAlignedBoundingBox { get; set; } = new();

            /// <summary>
            /// This grass group's individual grass entries.
            /// </summary>
            public GrassEntry[] GrassEntries { get; set; } = Array.Empty<GrassEntry>();
        }

        public class GrassEntry
        {
            /// <summary>
            /// This grass entry's position.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This grass entry's scale.
            /// </summary>
            public float Scale { get; set; }

            /// <summary>
            /// The colour to tint this grass entry.
            /// TODO: This seems to be set up weirdly?
            /// </summary>
            public VertexColour Colour { get; set; } = new();
                   
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Is it actually four seperate byte values?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Is it actually four seperate byte values?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_4 { get; set; }
        }

        // Actual data presented to the end user.
        public List<Grass> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "GRSS";

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

            // Skip an unknown sequence of bytes that is always 01 01 00 00 00 00 00 00 00 00 00 00.
            reader.JumpAhead(0x0C);

            // Read the amount of grass instances in this file.
            ulong grassCount = reader.ReadUInt64();

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x08);

            // Read the offset to this file's grass table.
            long grassTableOffset = reader.ReadInt64();

            // Jump to this file's grass table.
            reader.JumpTo(grassTableOffset, false);

            // Loop through each grass instance in this file.
            for (ulong grassIndex = 0; grassIndex < grassCount; grassIndex++)
            {
                // Set up a new grass instance.
                Grass grass = new();

                // Read this grass instance's model.
                grass.GrassModel = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this grass instance's target model.
                grass.TargetModel = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this grass instance's target material.
                grass.Material = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this grass instance's unknown matrix.
                // TODO: Does this need transposing?
                grass.UnknownMatrix_1 = Helpers.ReadHedgeLibMatrix(reader);

                // Process the decomposed version of this grass instance's unknown matrix.
                grass.Transform.Process(grass.UnknownMatrix_1);

                // Read the count of grass groups in this grass instance.
                uint grassGroupCount = reader.ReadUInt32();

                // Read this grass instance's unknown floating point value.
                grass.UnknownFloat_1 = reader.ReadSingle();

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x08);

                // Read the offset to this grass instance's grass groups.
                long grassGroupOffset = reader.ReadInt64();
                
                // Initialise this grass instance's group array.
                grass.GrassGroups = new GrassGroup[grassGroupCount];

                // Save our position so we can jump back for the next grass instance.
                long position = reader.BaseStream.Position;

                // Jump to the grass group offset.
                reader.JumpTo(grassGroupOffset, false);

                // Loop through each grass group in this grass instance.
                for (uint grassGroupIndex = 0; grassGroupIndex < grassGroupCount; grassGroupIndex++)
                {
                    // Set up a grass group entry.
                    GrassGroup grassGroup = new();

                    // Read this grass group's axis aligned bounding box.
                    grassGroup.AxisAlignedBoundingBox.Min = Helpers.ReadHedgeLibVector3(reader);
                    grassGroup.AxisAlignedBoundingBox.Max = Helpers.ReadHedgeLibVector3(reader);

                    // Read the count of grass entries in this group.
                    ulong grassEntryCount = reader.ReadUInt64();

                    // Read the offset to this group's grass entries.
                    long grassEntryOffset = reader.ReadInt64();

                    // Save our position so we can jump back for the next grass group.
                    long grassGroupPosition = reader.BaseStream.Position;

                    // Initialise this grass group's grass entry array.
                    grassGroup.GrassEntries = new GrassEntry[grassEntryCount];

                    // Jump to the grass entry offset.
                    reader.JumpTo(grassEntryOffset, false);

                    // Loop through each grass entry in this grass group.
                    for (ulong grassEntryIndex = 0; grassEntryIndex < grassEntryCount; grassEntryIndex++)
                    {
                        // Set up a new grass entry.
                        GrassEntry grassEntry = new();

                        // Read this grass entry's position.
                        grassEntry.Position = Helpers.ReadHedgeLibVector3(reader);

                        // Read this grass entry's scale.
                        grassEntry.Scale = reader.ReadSingle();

                        // Read this grass entry's colour.
                        grassEntry.Colour.Alpha = reader.ReadByte();
                        grassEntry.Colour.Red = reader.ReadByte();
                        grassEntry.Colour.Green = reader.ReadByte();
                        grassEntry.Colour.Blue = reader.ReadByte();

                        // Read this grass entry's first unknown integer value.
                        grassEntry.UnknownUInt32_1 = reader.ReadUInt32();

                        // Read this grass entry's second unknown integer value.
                        grassEntry.UnknownUInt32_2 = reader.ReadUInt32();

                        // Skip an unknown value that is always 0.
                        reader.JumpAhead(0x04);

                        // Read this grass entry's first unknown floating point value.
                        grassEntry.UnknownFloat_1 = reader.ReadSingle();

                        // Read this grass entry's second unknown floating point value.
                        grassEntry.UnknownFloat_2 = reader.ReadSingle();

                        // Read this grass entry's third unknown floating point value.
                        grassEntry.UnknownFloat_3 = reader.ReadSingle();

                        // Read this grass entry's fourth unknown floating point value.
                        grassEntry.UnknownFloat_4 = reader.ReadSingle();

                        // Save this grass entry.
                        grassGroup.GrassEntries[grassEntryIndex] = grassEntry;
                    }

                    // Save this grass group.
                    grass.GrassGroups[grassGroupIndex] = grassGroup;

                    // Jump back for the next grass group.
                    reader.JumpTo(grassGroupPosition);
                }

                // Save this grass instance.
                Data.Add(grass);

                // Jump back for the next grass instance.
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
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this file's signature.
            writer.WriteSignature(Signature);

            // Write an unknown sequence of bytes that is always 01 01 00 00 00 00 00 00 00 00 00 00.
            writer.Write((ushort)0x101);
            writer.WriteNulls(0x0A);

            // Write the count of grass instances in this file.
            writer.Write((ulong)Data.Count);

            // Write an unknown value that is always 0.
            writer.Write(0L);

            // Add an offset for this file's grass table.
            writer.AddOffset("GrassTableOffset", 0x08);

            // Fill in the offset for this file's grass table.
            writer.FillInOffset("GrassTableOffset", false);

            // Loop through each grass instance in this file.
            for (int grassIndex = 0; grassIndex < Data.Count; grassIndex++)
            {
                // Add a string for this grass instance's model.
                writer.AddString($"Grass{grassIndex}GrassModel", Data[grassIndex].GrassModel, 0x08);

                // Add a string for this grass instance's target model.
                writer.AddString($"Grass{grassIndex}TargetModel", Data[grassIndex].TargetModel, 0x08);

                // Add a string for this grass instance's target material.
                writer.AddString($"Grass{grassIndex}TargetMaterial", Data[grassIndex].Material, 0x08);

                // Write this grass instance's unknown matrix.
                Helpers.WriteHedgeLibMatrix(writer, Data[grassIndex].UnknownMatrix_1);

                // Write the count of grass groups in this instance.
                writer.Write(Data[grassIndex].GrassGroups.Length);

                // Write this grass instance's unknown floating point value
                writer.Write(Data[grassIndex].UnknownFloat_1);

                // Write an unknown value that is always 0.
                writer.Write(0L);

                // Add an offset for this grass instance's grass group table.
                writer.AddOffset($"Grass{grassIndex}GrassGroupOffset", 0x08);
            }

            // Loop through each grass instance in this file.
            for (int grassIndex = 0; grassIndex < Data.Count; grassIndex++)
            {
                // Fill in the offset for this grass instance's grass group table
                writer.FillInOffset($"Grass{grassIndex}GrassGroupOffset", false);
                
                // Loop through each grass group in this grass instance.
                for (int grassGroupIndex = 0; grassGroupIndex < Data[grassIndex].GrassGroups.Length; grassGroupIndex++)
                {
                    // Write this grass group's axis aligned bounding box.
                    Helpers.WriteHedgeLibVector3(writer, Data[grassIndex].GrassGroups[grassGroupIndex].AxisAlignedBoundingBox.Min);
                    Helpers.WriteHedgeLibVector3(writer, Data[grassIndex].GrassGroups[grassGroupIndex].AxisAlignedBoundingBox.Max);

                    // Write the count of grass entries in this grass group.
                    writer.Write((ulong)Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries.Length);

                    // Add an offset for this grass group's grass entry table.
                    writer.AddOffset($"Grass{grassIndex}GrassGroup{grassGroupIndex}GrassEntryTableOffset", 0x08);
                }

                // Loop through each grass group in this grass instance.
                for (int grassGroupIndex = 0; grassGroupIndex < Data[grassIndex].GrassGroups.Length; grassGroupIndex++)
                {
                    // Fill in the offset for this grass group's grass entry table.
                    writer.FillInOffset($"Grass{grassIndex}GrassGroup{grassGroupIndex}GrassEntryTableOffset", false);

                    // Loop through each of this grass group's grass entries.
                    for (int grassEntryIndex = 0; grassEntryIndex < Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries.Length; grassEntryIndex++)
                    {
                        // Write this grass entry's position.
                        Helpers.WriteHedgeLibVector3(writer, Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Position);

                        // Write this grass entry's scale.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Scale);

                        // Write this grass entry's colour.
                        writer.Write((byte)Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Colour.Alpha);
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Colour.Red);
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Colour.Green);
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].Colour.Blue);

                        // Write this grass entry's first unknown integer value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownUInt32_1);

                        // Write this grass entry's second unknown integer value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownUInt32_2);

                        // Write an unknown value that is always 0.
                        writer.Write(0);

                        // Write this grass entry's first floating point value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownFloat_1);

                        // Write this grass entry's second floating point value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownFloat_2);

                        // Write this grass entry's third floating point value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownFloat_3);

                        // Write this grass entry's fourth floating point value.
                        writer.Write(Data[grassIndex].GrassGroups[grassGroupIndex].GrassEntries[grassEntryIndex].UnknownFloat_4);
                    }
                }
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
