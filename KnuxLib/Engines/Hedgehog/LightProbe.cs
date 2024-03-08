using System.Diagnostics;

namespace KnuxLib.Engines.Hedgehog
{
    public class LightProbe : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LightProbe() { }
        public LightProbe(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.lightprobe.json", Data);
        }

        // Classes for this format.
        public class Probe
        {
        }

        // Actual data presented to the end user.
        public List<Probe> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "DPIC";

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

            // Skip an unknown value of 2.
            reader.JumpAhead(0x04);

            // Get the 64 bit offset to the Probe Table and the count of the probes in it.
            long probeTableOffset = reader.ReadInt64();
            ulong probeCount = reader.ReadUInt64();

            for (ulong probeIndex = 0; probeIndex < probeCount; probeIndex++)
            {
                Vector3 UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);
                Vector3 UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                if (reader.ReadUInt32() != 0) Debugger.Break();
                reader.JumpAhead(0x50);
            }
        }
    }
}
