﻿namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out how the Matrix affects the placement of things.
    // TODO: Figure out if the Matrix should be transposed or not.
    // TODO: Figure out what the two floats at the end of the file are for.
    public class Cloud : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Cloud() { }
        public Cloud(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.cloud.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The cloud instances in this file.
            /// </summary>
            public CloudInstance[] Instances { get; set; } = Array.Empty<CloudInstance>();

            /// <summary>
            /// An unknown set of two floating points values.
            /// TODO: What is this?
            /// </summary>
            public Vector2 UnknownVector2_1 { get; set; }
        }

        public class CloudInstance
        {
            /// <summary>
            /// This cloud instance's matrix.
            /// </summary>
            public Matrix4x4 Matrix { get; set; }

            /// <summary>
            /// A copy of this cloud instance's 4x4 Matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(200);

        // Set up the Signature we expect.
        public new const string Signature = "SHCP";

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

            // Skip an unknown value that is always 0x03.
            reader.JumpAhead(0x04);

            // Read the amount of cloud instances in this file.
            uint cloudInstanceCount = reader.ReadUInt32();

            // Read the offset to this file's cloud instance table.
            uint cloudInstanceTableOffset = reader.ReadUInt32();

            // Read an unknown offset.
            uint UnknownOffset_1 = reader.ReadUInt32();

            // Initialise the cloud instance array.
            Data.Instances = new CloudInstance[cloudInstanceCount];

            // Jump to this file's cloud instance table.
            reader.JumpTo(cloudInstanceTableOffset, false);

            // Loop through each cloud instance in this file.
            for (int cloudInstanceIndex = 0; cloudInstanceIndex < cloudInstanceCount; cloudInstanceIndex++)
            {
                // Set up a new cloud instance entry.
                CloudInstance cloud = new();

                // Read this cloud instance's matrix.
                // TODO: Does this need to be transposed?
                cloud.Matrix = Helpers.ReadHedgeLibMatrix(reader);

                // Decompose this cloud instance's matrix.
                cloud.Transform.Process(cloud.Matrix);

                // Save this cloud instance.
                Data.Instances[cloudInstanceIndex] = cloud;
            }

            // Jump to this file's unknown offset.
            reader.JumpTo(UnknownOffset_1, false);

            // Read this file's unknown floating point pair.
            Data.UnknownVector2_1 = new(reader.ReadSingle(), reader.ReadSingle());

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

            // Write the SHCP signature.
            writer.WriteSignature(Signature);

            // Write an unknown value that is always 0x03.
            writer.Write(0x03);

            // Write the amount of cloud instances in this file.
            writer.Write(Data.Instances.Length);

            // Add an offset for this file's cloud instance table.
            writer.AddOffset("CloudInstanceTableOffset");

            // Add an offset to the unknown floating points values at the end of this file.
            writer.AddOffset("UnknownOffset_1");

            // Fill in the offset for this file's cloud instance table.
            writer.FillInOffset("CloudInstanceTableOffset", false);

            // Loop through and write each cloud instance.
            for (int cloudInstanceIndex = 0; cloudInstanceIndex < Data.Instances.Length; cloudInstanceIndex++)
                Helpers.WriteHedgeLibMatrix(writer, Data.Instances[cloudInstanceIndex].Matrix);

            // Fill in the offset to the unknown floating points values at the end of this file.
            writer.FillInOffset("UnknownOffset_1", false);

            // Write the two unknown floating point values.
            writer.Write(Data.UnknownVector2_1.X);
            writer.Write(Data.UnknownVector2_1.Y);

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }
    }
}
