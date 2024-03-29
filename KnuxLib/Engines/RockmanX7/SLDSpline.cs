﻿namespace KnuxLib.Engines.RockmanX7
{
    // TODO: Saving.
    // TODO: Figure out the unknown data and how it interacts with the points.
        // ST08_03 (The inside part of Wind Crowrang's stage) has a branch with an obvious mistake that these numbers might be a part of?
    // TODO: What does SLD mean? Scripted Line Definition?
    // TODO: That unknown data doesn't actually seem to be halves, if st10_00.sld is to be believed.
    public class SLDSpline : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public SLDSpline() { }
        public SLDSpline(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The coordinates for the various vertices that make up this spline.
            /// </summary>
            public List<Vector3> Vertices { get; set; } = new();

            /// <summary>
            /// An unknown chunk of data for each vertex.
            /// TODO: What is all of this and are the numbers within them actually half floating points?
            /// </summary>
            public List<UnknownData_1> UnknownData_1 { get; set; } = new();
        }

        public class UnknownData_1
        {
            /// <summary>
            /// The index of this data. Not actually a thing, just used for reference.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_1 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_2 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_3 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_4 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_5 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_6 { get; set; }

            /// <summary>
            /// An unknown two byte floating point value (converted to a standard float for ease of reading in JSON).
            /// TODO: What is this?
            /// </summary>
            public float UnknownHalf_7 { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's point count.
            ushort pointCount = reader.ReadUInt16();

            // Loop through and read this spline's vertices.
            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                Data.Vertices.Add(reader.ReadVector3());

            // Loop through and read this spline's unknown data.
            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                Data.UnknownData_1.Add(new()
                {
                    Index = pointIndex,
                    UnknownHalf_1 = (float)reader.ReadHalf(),
                    UnknownHalf_2 = (float)reader.ReadHalf(),
                    UnknownHalf_3 = (float)reader.ReadHalf(),
                    UnknownHalf_4 = (float)reader.ReadHalf(),
                    UnknownHalf_5 = (float)reader.ReadHalf(),
                    UnknownHalf_6 = (float)reader.ReadHalf(),
                    UnknownHalf_7 = (float)reader.ReadHalf()
                });
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the count of vertices and unknown data chunks in this file.
            writer.Write((ushort)Data.Vertices.Count);

            // Loop through and write each vertex for this spline.
            for (int vertexIndex = 0; vertexIndex < Data.Vertices.Count; vertexIndex++)
                writer.Write(Data.Vertices[vertexIndex]);

            // Loop through and write each unknown chunk for this spline.
            for (int unknownIndex = 0; unknownIndex < Data.UnknownData_1.Count; unknownIndex++)
            {
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_1);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_2);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_3);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_4);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_5);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_6);
                writer.Write((Half)Data.UnknownData_1[unknownIndex].UnknownHalf_7);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Exports the position values from this spline to an OBJ.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Write each vertex's positions.
            for (int vertexIndex = 0; vertexIndex < Data.Vertices.Count; vertexIndex++)
                obj.WriteLine($"v {Data.Vertices[vertexIndex].X} {Data.Vertices[vertexIndex].Y} {Data.Vertices[vertexIndex].Z}");

            // Write this path's object name.
            obj.WriteLine($"o {Path.GetFileNameWithoutExtension(filepath)}");
            obj.WriteLine($"g {Path.GetFileNameWithoutExtension(filepath)}");

            // Write this path's object.
            obj.Write("l ");
            for (int vertexIndex = 0; vertexIndex < Data.Vertices.Count; vertexIndex++)
                obj.Write($"{vertexIndex + 1} ");

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
