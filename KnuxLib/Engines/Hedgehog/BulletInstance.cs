using HedgeLib.Headers;

namespace KnuxLib.Engines.Hedgehog
{
    public class BulletInstance : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public BulletInstance() { }
        public BulletInstance(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        public class Instance
        {
            /// <summary>
            /// The first name value of this instance.
            /// </summary>
            public string Name1 { get; set; } = "";

            /// <summary>
            /// The second name value of this instance.
            /// TODO: Investigate what this does, it's usually the same as Name1 but not always.
            /// </summary>
            public string Name2 { get; set; } = "";

            /// <summary>
            /// This instance's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This instance's roation in 3D space.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 = 1;

            /// <summary>
            /// This instance's scale in 3D space.
            /// </summary>
            public Vector3 Scale { get; set; }
        }

        // Actual data presented to the end user.
        public List<Instance> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "CPIC";

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

            // Check this file version. Always 2?
            uint version = reader.ReadUInt32();

            // Get the 64 bit offset to the Instance Table and the count of the instances in it.
            long instanceTableOffset = reader.ReadInt64();
            ulong instanceCount = reader.ReadUInt64();

            // Jump to the instance table (should already be here but lets play it safe).
            reader.JumpTo(instanceTableOffset, false);

            // Loop through each instance.
            for (ulong i = 0; i < instanceCount; i++)
            {
                // Set up a new instance.
                Instance inst = new()
                {
                    // Read the two name values for this instance, while they are usually the same, they can be different.
                    Name1 = Helpers.ReadNullTerminatedStringTableEntry(reader),
                    Name2 = Helpers.ReadNullTerminatedStringTableEntry(reader),

                    // Read this instance's position.
                    Position = Helpers.ReadHedgeLibVector3(reader),

                    // Read this instance's rotation.
                    Rotation = Helpers.ReadHedgeLibVector3(reader),

                    // Read this instance's unknown integer value.
                    UnknownUInt32_1 = reader.ReadUInt32(),

                    // Read this instance's scale.
                    Scale = Helpers.ReadHedgeLibVector3(reader),
                };

                // Skip four bytes that are always 0.
                reader.JumpAhead(0x04);

                // Realign for the next instance.
                reader.FixPadding(0x08);

                // Save this instance.
                Data.Add(inst);
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

            // Write the file version.
            writer.Write(2);

            // Set up the BINA Offset Table.
            writer.AddOffset("instanceOffset", 8);

            // Write how many instances are in this file.
            writer.Write((ulong)Data.Count);

            // Fill in the offset position.
            writer.FillInOffset("instanceOffset", false, false);

            // Loop through each instance.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add the two strings to the BINA String Table and write their offsets.
                writer.AddString($"instance{i}name1", Data[i].Name1, 8);
                writer.AddString($"instance{i}name2", Data[i].Name2, 8);

                // Write this instance's position.
                Helpers.WriteHedgeLibVector3(writer, Data[i].Position);

                // Write this instance's rotation.
                Helpers.WriteHedgeLibVector3(writer, Data[i].Rotation);

                // Write this instance's unknown integer value.
                writer.Write(Data[i].UnknownUInt32_1);

                // Write this instance's scale.
                Helpers.WriteHedgeLibVector3(writer, Data[i].Scale);

                // Write this instance's empty value.
                writer.Write(0);

                // All Data but the last one appear to be aligned, so if this isn't the last instance, align it.
                if (i != Data.Count - 1)
                    writer.FixPadding(0x08);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
