using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // Based on https://github.com/blueskythlikesclouds/SkythTools/tree/master/Sonic%20Forces/Path%20Scripts
    // TODO: Figure out and read the k-d tree data.
    // TODO: Format saving.
    // TODO: Format importing.
    // TODO: Proper Rangers support? The paths seem identical besides the type value changing (swapping SideView and GrindRail for some reason???).
    public class PathSpline_Wars : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline_Wars() { }
        public PathSpline_Wars(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SplineType : ulong
        {
            Default = 0,
            SideView = 1,
            GrindRail = 2
        }

        public class SplinePath
        {
            /// <summary>
            /// The name of this spline.
            /// </summary>
            public string Name { get; set; } = "objpath_001";

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this?
            /// </summary>
            public ushort UnknownUShort_1 { get; set; } = 0x01;

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

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
            public SplineType Type { get; set; } = SplineType.Default;

            /// <summary>
            /// This path's UID, if it has one.
            /// </summary>
            public ulong? UID { get; set; }

            public override string ToString() => Name;
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
        public override void Load(string filepath)
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

                // Read an unknown ushort value that is always 1 except for a single spline in w7b02_path.path.
                path.UnknownUShort_1 = reader.ReadUInt16();

                // Read the count of spline knots for this path.
                ushort knotCount = reader.ReadUInt16();

                // Read an unknown floating point value.
                path.UnknownFloat_1 = reader.ReadSingle();

                // Read the offset to an unknown table of booleans that are always true.
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
                path.Type = (SplineType)reader.ReadUInt64();

                if (typeCount == 2)
                {
                    // Skip an offset that always points to the word "uid" in the string table.
                    reader.JumpAhead(0x08);

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x08);

                    path.UID = reader.ReadUInt64();
                }

                // TODO: How does this data work?
                // Jump to this path's k-d tree.
                reader.JumpTo(kdTreeOffset, false);
                uint UnknownUInt32_1 = reader.ReadUInt32();
                uint UnknownUInt32_2 = reader.ReadUInt32();
                long UnknownOffset_2 = reader.ReadInt64();

                ulong UnknownULong_1 = reader.ReadUInt64();
                long UnknownOffset_3 = reader.ReadInt64();

                ulong UnknownULong_2 = reader.ReadUInt64();
                long UnknownOffset_4 = reader.ReadInt64();

                reader.JumpTo(UnknownOffset_2, false);

                // TODO: Structure of the data at this offset.

                reader.JumpTo(UnknownOffset_3, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownULong_1; unknownIndex++)
                {
                    reader.JumpAhead(0x04);
                    reader.JumpAhead(0x04);
                }

                reader.JumpTo(UnknownOffset_4, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownULong_2; unknownIndex++)
                    reader.JumpAhead(0x04);

                // Save this path.
                Data.Add(path);

                // Jump back to read the next path.
                reader.JumpTo(position);
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
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
