namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Confirm that the contents of the transform table are actually correct.
    public class BulletSkeleton : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public BulletSkeleton() { }
        public BulletSkeleton(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".skl", "")}.hedgehog.bulletskeleton.json", Data);
        }

        // Classes for this format.
        public class Node
        {
            /// <summary>
            /// The index of this node's parent.
            /// </summary>
            public short ParentNodeIndex { get; set; }

            /// <summary>
            /// The name of this node.
            /// </summary>
            public string NodeName { get; set; } = "";

            /// <summary>
            /// This node's position in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This node's roation in 3D space.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This node's scale in 3D space.
            /// </summary>
            public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

            public override string ToString() => NodeName;
        }

        // Actual data presented to the end user.
        public List<Node> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "KSXP";

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

            // Skip an unknown value of 0x200.
            reader.JumpAhead(0x04);

            // Read the offset to this file's hierarchy table.
            long hierarchyTableOffset = reader.ReadInt64();

            // Read this file's node count.
            ulong nodeCount = reader.ReadUInt64();

            // Skip an unknown value that always matches NodeCount.
            reader.JumpAhead(0x08);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x08);

            // Read the offset to this file's string table.
            long stringTableOffset = reader.ReadInt64();

            // Skip two unknown values that always matches NodeCount.
            reader.JumpAhead(0x10);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x08);

            // Read the offset to this file's transform table.
            long transformTableOffset = reader.ReadInt64();

            // Skip two unknown values that always matches NodeCount.
            reader.JumpAhead(0x10);

            // Skip an unknown value of 0.
            reader.JumpAhead(0x08);

            // Jump to this file's hierarchy table.
            reader.JumpTo(hierarchyTableOffset, false);

            // Read each node's parent index.
            for (ulong nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                // Set up a new node entry.
                Node node = new();

                // Read this node's parent index.
                node.ParentNodeIndex = reader.ReadInt16();

                // Save this node.
                Data.Add(node);
            }

            // Jump to this file's string table.
            reader.JumpTo(stringTableOffset, false);

            // Read each node's string offset.
            for (ulong nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                // Read this node's name.
                Data[(int)nodeIndex].NodeName = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x08);
            }

            // Jump to this file's transform table.
            reader.JumpTo(transformTableOffset, false);

            // Read each node's transform.
            for (ulong nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                // Read this node's position.
                Data[(int)nodeIndex].Position = Helpers.ReadHedgeLibVector3(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Read this node's rotation.
                Data[(int)nodeIndex].Rotation = Helpers.ReadHedgeLibQuaternion(reader);

                // Read this node's scale.
                Data[(int)nodeIndex].Scale = Helpers.ReadHedgeLibVector3(reader);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);
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
            writer.Write(0x200);

            // Add an offset for this file's hierarchy table.
            writer.AddOffset("HierarchyTableOffset", 0x08);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write an unknown value of 0.
            writer.Write(0x00ul);

            // Add an offset for this file's string table.
            writer.AddOffset("StringTableOffset", 0x08);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write an unknown value of 0.
            writer.Write(0x00ul);

            // Add an offset for this file's transform table.
            writer.AddOffset("TransformTableOffset", 0x08);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write this file's node count.
            writer.Write((ulong)Data.Count);

            // Write an unknown value of 0.
            writer.Write(0x00ul);

            // Fill in the offset for this file's hierarchy table.
            writer.FillInOffset("HierarchyTableOffset", false, false);

            // Loop through each node.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this node's parent index.
                writer.Write(Data[dataIndex].ParentNodeIndex);
            }

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Fill in the offset for this file's string table.
            writer.FillInOffset("StringTableOffset", false, false);

            // Loop through each node.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Add a string entry for this node's name.
                writer.AddString($"node{dataIndex}name", Data[dataIndex].NodeName, 0x08);

                // Write an unknown value of 0.
                writer.Write(0x00ul);
            }

            // Fill in the offset for this file's transform table.
            writer.FillInOffset("TransformTableOffset", false, false);

            // Loop through each node.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this node's position.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Position);

                // Write an unknown value of 0.
                writer.Write(0x00);

                // Write this node's quaternion.
                Helpers.WriteHedgeLibQuaternion(writer, Data[dataIndex].Rotation);

                // Write this node's scale.
                Helpers.WriteHedgeLibVector3(writer, Data[dataIndex].Scale);

                // Write an unknown value of 0.
                writer.Write(0x00);
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
