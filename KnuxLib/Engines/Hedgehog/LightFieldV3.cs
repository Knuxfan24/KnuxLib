namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Is V3 the right name for this? Assuming Generations and Lost World are V1 and Forces is V2.
    // TODO: Figure out all the unknowns.
    public class LightFieldV3 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LightFieldV3() { }
        public LightFieldV3(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.lightfieldv3.json", Data);
        }

        // Classes for this format.
        public class LightField
        {
            /// <summary>
            /// This light field's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_3 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_3 { get; set; }

            public override string ToString() => Name;
        }

        // Actual data presented to the end user.
        public List<LightField> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip a sequence of bytes that is always a 1 then 0x90 nulls.
            reader.JumpAhead(0x94);

            // Read the count of light fields in this file.
            uint lightfieldCount = reader.ReadUInt32();

            // Read the offset to this file's light field table.
            long lightfieldTableOffset = reader.ReadInt64();

            // Jump to the light field table.
            reader.JumpTo(lightfieldTableOffset, false);

            // Loop through and read each light field.
            for (int i = 0; i < lightfieldCount; i++)
            {
                // Set up a new light field.
                LightField lightfield = new();

                // Read this light field's name.
                lightfield.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read this light field's first unknown integer value.
                lightfield.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this light field's second unknown integer value.
                lightfield.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this light field's third unknown integer value.
                lightfield.UnknownUInt32_3 = reader.ReadUInt32();

                // Read this light field's first unknown Vector3.
                lightfield.UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);

                // Read this light field's second unknown Vector3.
                lightfield.UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);

                // Read this light field's third unknown Vector3.
                lightfield.UnknownVector3_3 = Helpers.ReadHedgeLibVector3(reader);

                // Save this light field.
                Data.Add(lightfield);
            }

            // Close HedgeLib#'s BINAReader for the gismop file.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter for the gismod file and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            // Write 0x90 null bytes.
            writer.WriteNulls(0x90);

            // Write this file's light field count.
            writer.Write(Data.Count);

            // Add an offset to the light field table.
            writer.AddOffset("lightfieldTableOffset", 0x08);

            // Fill in the offset position.
            writer.FillInOffset("lightfieldTableOffset", false, false);

            // Loop through each light field in this file.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add an offset for this light field's name.
                writer.AddString($"lightfield{i}name", Data[i].Name, 0x08);

                // Write this light field's first unknown integer value.
                writer.Write(Data[i].UnknownUInt32_1);

                // Write this light field's second unknown integer value.
                writer.Write(Data[i].UnknownUInt32_2);

                // Write this light field's third unknown integer value.
                writer.Write(Data[i].UnknownUInt32_3);

                // Read this light field's first unknown Vector3.
                Helpers.WriteHedgeLibVector3(writer, Data[i].UnknownVector3_1);

                // Read this light field's second unknown Vector3.
                Helpers.WriteHedgeLibVector3(writer, Data[i].UnknownVector3_2);

                // Read this light field's third unknown Vector3.
                Helpers.WriteHedgeLibVector3(writer, Data[i].UnknownVector3_3);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }
    }
}
