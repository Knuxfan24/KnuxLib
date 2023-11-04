namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out all the unknowns.
    public class LightField_Rangers : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LightField_Rangers() { }
        public LightField_Rangers(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.lightfield_rangers.json", Data);
        }

        // Classes for this format.
        public class LightField
        {
            /// <summary>
            /// This light field's name.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// The probe counts for this light field.
            /// </summary>
            public uint[] Probes { get; set; } = new uint[3];

            /// <summary>
            /// This light field volume's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This light field volume's rotation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// This light field volume's size.
            /// </summary>
            public Vector3 Size { get; set; }

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
            for (int lightfieldIndex = 0; lightfieldIndex < lightfieldCount; lightfieldIndex++)
            {
                // Set up a new light field.
                LightField lightfield = new();

                // Read this light field's name.
                lightfield.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Loop through and read the three values of this light field's probes.
                for (int probe = 0; probe < 3; probe++)
                    lightfield.Probes[probe] = reader.ReadUInt32();

                // Read this light field's position.
                lightfield.Position = Helpers.ReadHedgeLibVector3(reader);

                // Read this light field's rotation.
                lightfield.Rotation = Helpers.ReadHedgeLibVector3(reader);

                // Read this light field's size.
                lightfield.Size = Helpers.ReadHedgeLibVector3(reader);

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
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add an offset for this light field's name.
                writer.AddString($"lightfield{dataIndex}name", Data[dataIndex].Name, 0x08);

                // Loop through and write the three values of this light field's probes.
                for (int probe = 0; probe < 3; probe++)
                    writer.Write(Data[dataIndex].Probes[probe]);

                // Write this light field's position.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Position);

                // Write this light field's rotation.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Rotation);

                // Write this light field's size.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Size);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }
    }
}
