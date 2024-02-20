using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // Based on https://github.com/blueskythlikesclouds/SkythTools/tree/master/Sonic%20Forces/Path%20Scripts
    // TODO: Figure out and properly read the k-d tree data.
    // TODO: Format importing.
    // TODO: Check to see if Frontiers does anything other than the type differently, if so, handle it with the FormatVersion check.
    public class PathSpline_WarsRangers : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline_WarsRangers() { }
        public PathSpline_WarsRangers(string filepath, FormatVersion version = FormatVersion.Wars, bool export = false)
        {
            Load(filepath, version);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        public enum FormatVersion
        {
            Wars = 0,
            Rangers = 1
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SplineTypeWars : ulong
        {
            Default = 0,
            SideView = 1,
            GrindRail = 2
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SplineTypeRangers : ulong
        {
            Default = 0,
            GrindRail = 1,
            SideView = 2
        }

        public class SplinePath
        {
            /// <summary>
            /// The name of this spline.
            /// </summary>
            public string Name { get; set; } = "objpath_001";

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this? objpath_001 in Forces' w7b02_path.path is the only time this is 0 rather than 1.
            /// </summary>
            public ushort UnknownUShort_1 { get; set; } = 0x01;

            /// <summary>
            /// This spline's distance array.
            /// </summary>
            public float[] Distance { get; set; } = Array.Empty<float>();

            /// <summary>
            /// This spline's knot points.
            /// </summary>
            public Vector3[] Knots { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// This spline's up vector array.
            /// </summary>
            public Vector3[] UpVector { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// This spline's forward vector array.
            /// </summary>
            public Vector3[] ForwardVector { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// This spline's knot points for double splines.
            /// </summary>
            public Vector3[]? DoubleKnots { get; set; }

            /// <summary>
            /// This spline's axis aligned bounding box.
            /// </summary>
            public AABB AxisAlignedBoundingBox { get; set; } = new();

            /// <summary>
            /// This spline's type.
            /// </summary>
            public object Type { get; set; } = new();

            /// <summary>
            /// This path's UID, if it has one.
            /// </summary>
            public ulong? UID { get; set; }

            /// <summary>
            /// This path's k-d tree.
            /// </summary>
            public kdTree kdTree { get; set; } = new();

            public override string ToString() => Name;
        }

        public class kdTree
        {
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
            /// An unknown chunk of data.
            /// TODO: What is this?
            /// </summary>
            public byte[] UnknownData_1 { get; set; } = Array.Empty<byte>();

            /// <summary>
            /// An unknown set of values, consisting of two integer values each.
            /// TODO: What is this?
            /// </summary>
            public List<uint[]> UnknownData_2 { get; set; } = new();

            /// <summary>
            /// An unknown list of integer values.
            /// TODO: What is this?
            /// </summary>
            public uint[] UnknownData_3 = Array.Empty<uint>();
        }

        // Actual data presented to the end user.
        public List<SplinePath> Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        // Set up the Signature we expect.
        public new const string Signature = "HTAP";

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.Wars)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Check this file's signature.
            string signature = reader.ReadSignature();
            if (signature != Signature)
                throw new Exception($"Invalid signature, got '{signature}', expected '{Signature}'.");

            // Skip an unknown value that is always 0x200.
            reader.JumpAhead(0x04);

            // Read the amount of paths in this file.
            ulong pathCount = reader.ReadUInt64();

            // Read the offset to this file's path table.
            long pathTableOffset = reader.ReadInt64();

            // Jump to this file's path table.
            reader.JumpTo(pathTableOffset, false);

            // Loop through each path in this file.
            for (ulong pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                // Set up a new path entry.
                SplinePath path = new();

                // Read this path's name.
                path.Name = Helpers.ReadNullTerminatedStringTableEntry(reader);

                // Read an unknown ushort value that is always 1 except for a single instance.
                path.UnknownUShort_1 = reader.ReadUInt16();

                // Read the count of spline knots for this path.
                ushort knotCount = reader.ReadUInt16();

                // Skip a floating point value which is always the same as the final value in the distance array.
                reader.JumpAhead(0x04);

                // Read the offset to an unknown table (of knotCount length) of booleans that are always true.
                long unknownBooleanTableOffset = reader.ReadInt64();

                // Read the offset to this path's distance array.
                long distanceOffset = reader.ReadInt64();

                // Read the offset to this path's spline knot array.
                long knotOffset = reader.ReadInt64();

                // Read the offset to this path's up vector array.
                long upVectorOffset = reader.ReadInt64();

                // Read the offset to this path's forward vector array.
                long forwardVectorOffset = reader.ReadInt64();

                // Read offset to this path's double spline knot count.
                ulong doubleKnotCount = reader.ReadUInt64();

                // Read offset to this path's double spline knot array.
                long doubleKnotOffset = reader.ReadInt64();

                // Read this path's axis aligned bounding box.
                path.AxisAlignedBoundingBox.Min = Helpers.ReadHedgeLibVector3(reader);
                path.AxisAlignedBoundingBox.Max = Helpers.ReadHedgeLibVector3(reader);

                // Read the count of type entries in this path, usually 2 ("type" and "uid") but some only have 1.
                ulong typeCount = reader.ReadUInt64();

                // Read the offset to this path's type data.
                long typeOffset = reader.ReadInt64();

                // Skip an unknown value that is always 0.
                reader.JumpAhead(0x08);

                // Read the offset to this path's k-d tree.
                long kdTreeOffset = reader.ReadInt64();

                // Save our position so we can jump back for the next path.
                long position = reader.BaseStream.Position;

                // Jump to the distance array's offset.
                reader.JumpTo(distanceOffset, false);

                // Initialise this path's distance array.
                path.Distance = new float[knotCount];

                // Read each value in this path's distance array.
                for (int distanceIndex = 0; distanceIndex < knotCount; distanceIndex++)
                    path.Distance[distanceIndex] = reader.ReadSingle();

                // Jump to the spline knot array's offset.
                reader.JumpTo(knotOffset, false);

                // Initialise this path's spline knot array.
                path.Knots = new Vector3[knotCount];

                // Read each knot for this spline.
                for (int knotIndex = 0; knotIndex < knotCount; knotIndex++)
                    path.Knots[knotIndex] = Helpers.ReadHedgeLibVector3(reader);

                // Jump to the up vector array's offset.
                reader.JumpTo(upVectorOffset, false);

                // Initialise this path's up vector array.
                path.UpVector = new Vector3[knotCount];

                // Read each value in this path's up vector array.
                for (int upVectorIndex = 0; upVectorIndex < knotCount; upVectorIndex++)
                    path.UpVector[upVectorIndex] = Helpers.ReadHedgeLibVector3(reader);

                // Jump to the forward vector array's offset.
                reader.JumpTo(forwardVectorOffset, false);

                // Initialise this path's forward vector array.
                path.ForwardVector = new Vector3[knotCount];

                // Read each value in this path's forward vector array.
                for (int forwardVectorIndex = 0; forwardVectorIndex < knotCount; forwardVectorIndex++)
                    path.ForwardVector[forwardVectorIndex] = Helpers.ReadHedgeLibVector3(reader);

                // Only handle the double knot data if there is any, one path in Frontiers doesn't have any.
                if (doubleKnotCount != 0)
                {
                    // Jump to the double spline knot array's offset.
                    reader.JumpTo(doubleKnotOffset, false);

                    // Initialise this path's double spline knot array.
                    path.DoubleKnots = new Vector3[doubleKnotCount];

                    // Read each knot for this double spline.
                    for (ulong doubleKnotIndex = 0; doubleKnotIndex < doubleKnotCount; doubleKnotIndex++)
                        path.DoubleKnots[doubleKnotIndex] = Helpers.ReadHedgeLibVector3(reader);
                }

                // Jump to this path's type data offset.
                reader.JumpTo(typeOffset, false);

                // Skip an offset that always points to the word "type" in the string table.
                reader.JumpAhead(0x08);

                // Skip an unknown value of 0.
                reader.JumpAhead(0x08);

                // Read this path's type.
                switch (version)
                {
                    case FormatVersion.Wars: path.Type = (SplineTypeWars)reader.ReadUInt64(); break;
                    case FormatVersion.Rangers: path.Type = (SplineTypeRangers)reader.ReadUInt64(); break;
                }

                // If there is 2 types in this path, then read the UID entry.
                if (typeCount == 2)
                {
                    // Skip an offset that always points to the word "uid" in the string table.
                    reader.JumpAhead(0x08);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x08);

                    // Read this path's UID value.
                    path.UID = reader.ReadUInt64();
                }

                #region TODO: Properly reverse engineer the k-d tree's data.
                // Jump to this path's k-d tree.
                reader.JumpTo(kdTreeOffset, false);
                path.kdTree.UnknownUInt32_1 = reader.ReadUInt32();

                path.kdTree.UnknownUInt32_2 = reader.ReadUInt32(); 

                long UnknownData_1_Offset = reader.ReadInt64();

                ulong UnknownData_2_Count = reader.ReadUInt64();

                long UnknownData_2_Offset = reader.ReadInt64();

                ulong UnknownData_3_Count = reader.ReadUInt64();

                long UnknownData_3_Offset = reader.ReadInt64();

                reader.JumpTo(UnknownData_1_Offset, false);

                path.kdTree.UnknownData_1 = reader.ReadBytes((int)(UnknownData_2_Offset - UnknownData_1_Offset));

                reader.JumpTo(UnknownData_2_Offset, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownData_2_Count; unknownIndex++)
                {
                    uint[] unknownValues = new uint[2];

                    unknownValues[0] = reader.ReadUInt32();
                    unknownValues[1] = reader.ReadUInt32();

                    path.kdTree.UnknownData_2.Add(unknownValues);
                }

                path.kdTree.UnknownData_3 = new uint[(int)UnknownData_3_Count];

                reader.JumpTo(UnknownData_3_Offset, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownData_3_Count; unknownIndex++)
                    path.kdTree.UnknownData_3[unknownIndex] = reader.ReadUInt32();
                #endregion

                // Save this path.
                Data.Add(path);

                // Jump back to read the next path.
                reader.JumpTo(position);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.Wars)
        {
            // Set up our BINAWriter and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write the HTAP signature.
            writer.WriteSignature(Signature);

            // Write an unknown value that is always 0x200.
            writer.Write(0x200);

            // Write the count of splines in this file.
            writer.Write((ulong)Data.Count);

            // Add an offset to the path table.
            writer.AddOffset("PathTableOffset", 0x08);

            // Fill in the path table offset.
            writer.FillInOffset("PathTableOffset", false);

            // Loop through each path.
            for (int pathIndex = 0; pathIndex < Data.Count; pathIndex++)
            {
                // Add a string for this path's name.
                writer.AddString($"Path{pathIndex}Name", Data[pathIndex].Name, 0x08);

                // Write this path's unknown short value.
                writer.Write(Data[pathIndex].UnknownUShort_1);

                // Write this path's knot count.
                writer.Write((ushort)Data[pathIndex].Knots.Length);

                // Write this path's distance value.
                writer.Write(Data[pathIndex].Distance[^1]);

                // Add an offset to this path's table of unknown, always true, booleans.
                writer.AddOffset($"Path{pathIndex}UnknownBooleanTableOffset", 0x08);

                // Add an offset to this path's distance array.
                writer.AddOffset($"Path{pathIndex}DistanceOffset", 0x08);

                // Add an offset to this path's spline knot array.
                writer.AddOffset($"Path{pathIndex}KnotOffset", 0x08);

                // Add an offset to this path's up vector array.
                writer.AddOffset($"Path{pathIndex}UpVectorOffset", 0x08);

                // Add an offset to this path's forward vector array.
                writer.AddOffset($"Path{pathIndex}ForwardVectorOffset", 0x08);

                // Handle whether or not this spline uses double knots.
                if (Data[pathIndex].DoubleKnots != null)
                {
                    // Write this spline's double knot count.
                    writer.Write((ulong)Data[pathIndex].DoubleKnots.Length);

                    // Add an offset to this path's double spline knot table.
                    writer.AddOffset($"Path{pathIndex}DoubleKnotOffset", 0x08);
                }
                else
                {
                    // Write two zero values in place of the double knot data.
                    writer.Write(0L);
                    writer.Write(0L);
                }

                // Write this path's axis aligned bounding box.
                Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].AxisAlignedBoundingBox.Min);
                Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].AxisAlignedBoundingBox.Max);

                // Write the type count, depending on if this path has a UID or not.
                if (Data[pathIndex].UID != null)
                    writer.Write(2L);
                else
                    writer.Write(1L);

                // Add an offset to this path's type data.
                writer.AddOffset($"Path{pathIndex}TypeOffset", 0x08);
                
                // Write an unknown value that is always 0.
                writer.Write(0L);

                // Add an offset to this path's k-d tree.
                writer.AddOffset($"Path{pathIndex}kdTreeOffset", 0x08);
            }

            // Loop through each path.
            for (int pathIndex = 0; pathIndex < Data.Count; pathIndex++)
            {
                // Fill in the offset for this path's unknown boolean table.
                writer.FillInOffset($"Path{pathIndex}UnknownBooleanTableOffset", false);

                // Write a true value for each knot.
                for (int knotIndex = 0; knotIndex < Data[pathIndex].Knots.Length; knotIndex++)
                    writer.Write(true);

                // Realign to 0x04 bytes.
                writer.FixPadding(0x04);

                // Fill in the offset for this path's distance array.
                writer.FillInOffset($"Path{pathIndex}DistanceOffset", false);

                // Loop through and write each value in this path's distance array.
                for (int distanceIndex = 0; distanceIndex < Data[pathIndex].Distance.Length; distanceIndex++)
                    writer.Write(Data[pathIndex].Distance[distanceIndex]);

                // Fill in the offset for this path's spline knot table.
                writer.FillInOffset($"Path{pathIndex}KnotOffset", false);

                // Loop through and write each spline knot's position.
                for (int knotIndex = 0; knotIndex < Data[pathIndex].Knots.Length; knotIndex++)
                    Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].Knots[knotIndex]);

                // Fill in the offset for this path's up vector array.
                writer.FillInOffset($"Path{pathIndex}UpVectorOffset", false);

                // Loop through and write each value in this path's up vector array.
                for (int upVectorIndex = 0; upVectorIndex < Data[pathIndex].UpVector.Length; upVectorIndex++)
                    Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].UpVector[upVectorIndex]);

                // Fill in the offset for this path's forward vector array.
                writer.FillInOffset($"Path{pathIndex}ForwardVectorOffset", false);

                // Loop through and write each value in this path's forward vector array.
                for (int forwardVectorIndex = 0; forwardVectorIndex < Data[pathIndex].ForwardVector.Length; forwardVectorIndex++)
                    Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].ForwardVector[forwardVectorIndex]);

                // If this path has a double knot spline, then handle it.
                if (Data[pathIndex].DoubleKnots != null)
                {
                    // Fill in the offset for this path's double knot spline.
                    writer.FillInOffset($"Path{pathIndex}DoubleKnotOffset", false);

                    // Loop through and write the position for each double spline knot.
                    for (int doubleKnotIndex = 0; doubleKnotIndex < Data[pathIndex].DoubleKnots.Length; doubleKnotIndex++)
                        Helpers.WriteHedgeLibVector3(writer, Data[pathIndex].DoubleKnots[doubleKnotIndex]);
                }

                // Realign to 0x08.
                writer.FixPadding(0x08);

                // Fill in the offset for this path's type data.
                writer.FillInOffset($"Path{pathIndex}TypeOffset", false);

                // Add the "type" string to the string table.
                writer.AddString($"Path{pathIndex}Type", "type", 0x08);

                // Write an unknown value that is always 0.
                writer.Write(0L);

                // Write this path's type identifier, depending on the format version.
                switch (version)
                {
                    case FormatVersion.Wars: writer.Write((ulong)(SplineTypeWars)Data[pathIndex].Type); break;
                    case FormatVersion.Rangers: writer.Write((ulong)(SplineTypeRangers)Data[pathIndex].Type); break;
                }

                // If this path as a UID, then write the type entry for it as well.
                if (Data[pathIndex].UID != null)
                {
                    // Add the "uid" string to the string table.
                    writer.AddString($"Path{pathIndex}UID", "uid", 0x08);

                    // Write an unknown value that is always 0.
                    writer.Write(0L);

                    // Write this path's UID value.
                    writer.Write((ulong)Data[pathIndex].UID);
                }

                // Fill in the offset for this path's k-d tree.
                writer.FillInOffset($"Path{pathIndex}kdTreeOffset", false);

                #region TODO: Properly write this when the k-d tree is properly reverse engineered.
                writer.Write(Data[pathIndex].kdTree.UnknownUInt32_1);
                writer.Write(Data[pathIndex].kdTree.UnknownUInt32_2);

                writer.AddOffset($"Path{pathIndex}kdTreeUnknownData1_Offset", 0x08);

                writer.Write((ulong)Data[pathIndex].kdTree.UnknownData_2.Count);

                writer.AddOffset($"Path{pathIndex}kdTreeUnknownData2_Offset", 0x08);

                writer.Write((ulong)Data[pathIndex].kdTree.UnknownData_3.Length);

                writer.AddOffset($"Path{pathIndex}kdTreeUnknownData3_Offset", 0x08);

                writer.FillInOffset($"Path{pathIndex}kdTreeUnknownData1_Offset", false);

                writer.Write(Data[pathIndex].kdTree.UnknownData_1);

                writer.FillInOffset($"Path{pathIndex}kdTreeUnknownData2_Offset", false);

                foreach (uint[] value in Data[pathIndex].kdTree.UnknownData_2)
                {
                    writer.Write(value[0]);
                    writer.Write(value[1]);
                }

                writer.FillInOffset($"Path{pathIndex}kdTreeUnknownData3_Offset", false);

                foreach (uint value in Data[pathIndex].kdTree.UnknownData_3)
                    writer.Write(value);
                #endregion
            }

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter.
            writer.Close();
        }

        /// <summary>
        /// Exports this path's splines to an OBJ file.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Set up a variable to track vertices.
            int vertexCount = 0;

            // Loop through each path.
            for (int pathIndex = 0; pathIndex < Data.Count; pathIndex++)
            {
                // If this path uses double knots, then write those values.
                if (Data[pathIndex].DoubleKnots != null)
                {
                    // Starting from 0, write each knot value, incrementing by 2 rather than 1.
                    for (int vertexIndex = 0; vertexIndex < Data[pathIndex].DoubleKnots.Length; vertexIndex+=2)
                        obj.WriteLine($"v {Data[pathIndex].DoubleKnots[vertexIndex].X} {Data[pathIndex].DoubleKnots[vertexIndex].Y} {Data[pathIndex].DoubleKnots[vertexIndex].Z}");

                    // Write the remaining knot values, starting from 1 and also incrementing by 2.
                    for (int vertexIndex = 1; vertexIndex < Data[pathIndex].DoubleKnots.Length; vertexIndex+=2)
                        obj.WriteLine($"v {Data[pathIndex].DoubleKnots[vertexIndex].X} {Data[pathIndex].DoubleKnots[vertexIndex].Y} {Data[pathIndex].DoubleKnots[vertexIndex].Z}");
                }
                // If this path doesn't use double knots, then write the regular knot values instead.
                else
                {
                    // Loop through and write each single knot value with no special tricks.
                    for (int vertexIndex = 0; vertexIndex < Data[pathIndex].Knots.Length; vertexIndex++)
                        obj.WriteLine($"v {Data[pathIndex].Knots[vertexIndex].X} {Data[pathIndex].Knots[vertexIndex].Y} {Data[pathIndex].Knots[vertexIndex].Z}");
                }

                // Write this path's name.
                obj.WriteLine($"o {Data[pathIndex].Name}");
                obj.WriteLine($"g {Data[pathIndex].Name}");

                // If this path uses double knots, then write two line objects.
                if (Data[pathIndex].DoubleKnots != null)
                {
                    // Write the first line idenitifer.
                    obj.Write("l ");

                    // Write the first path in this spline.
                    for (int vertexIndex = 0; vertexIndex < Data[pathIndex].DoubleKnots.Length / 2; vertexIndex++)
                        obj.Write($"{vertexIndex + 1 + vertexCount} ");

                    // Write the second line identifier.
                    obj.Write("\r\nl ");

                    // Write the second path in this spline.
                    for (int vertexIndex = Data[pathIndex].DoubleKnots.Length / 2; vertexIndex < Data[pathIndex].DoubleKnots.Length; vertexIndex++)
                        obj.Write($"{vertexIndex + 1 + vertexCount} ");

                    // Write a line break to end the line object.
                    obj.WriteLine();
                }
                else
                {
                    // Write the line idenitifer.
                    obj.Write("l ");

                    // Write the path in this spline.
                    for (int vertexIndex = 0; vertexIndex < Data[pathIndex].Knots.Length; vertexIndex++)
                        obj.Write($"{vertexIndex + 1 + vertexCount} ");

                    // Write a line break to end the line object.
                    obj.WriteLine();
                }

                // Increment the vertexCount based on whether this is a double knotted spline or not.
                if (Data[pathIndex].DoubleKnots != null)
                    vertexCount += Data[pathIndex].DoubleKnots.Length;
                else
                    vertexCount += Data[pathIndex].Knots.Length;
            }

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
