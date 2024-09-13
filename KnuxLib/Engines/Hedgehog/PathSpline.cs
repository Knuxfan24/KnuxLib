using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // Based on https://github.com/blueskythlikesclouds/SkythTools/tree/master/Sonic%20Forces/Path%20Scripts
    // TODO: Figure out and properly read the k-d tree data.
    // TODO: Check to see if Lost World and Frontiers handle anything other than their tags differently, if so, handle them with the FormatVersion check.
    // TODO: Slightly tidy this up after the porting process.
    public class PathSpline : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline() { }
        public PathSpline(string filepath, FormatVersion version = FormatVersion.Wars, bool export = false, string exportExtension = ".path", bool bigEndianSave = false)
        {
            // Check if the input file is an OBJ.
            if (Helpers.GetExtension(filepath) == ".obj")
            {
                // Import this OBJ.
                Data = ImportOBJ(filepath, version);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}{exportExtension}", version, bigEndianSave);
            }

            // Check if the input file isn't an OBJ.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    ExportOBJ($@"{Helpers.GetExtension(filepath, true)}.obj");
            }
        }

        // Classes for this format.
        public enum FormatVersion
        {
            sonic_2013 = 0,
            Wars = 1,
            Rangers = 2
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SplineType2013Wars : ulong
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

        [JsonConverter(typeof(StringEnumConverter))]
        public enum GrindSpeed : ulong
        {
            Normal = 0,
            Slow = 1,
            Fast = 2
        }

        public class SplinePath
        {
            /// <summary>
            /// The name of this spline.
            /// </summary>
            public string Name { get; set; } = "objpath_001";

            /// <summary>
            /// An unknown short value.
            /// TODO: What is this? Lost World uses it quite a bit, but objpath_001 in w7b02_path.path is the only time this is 0 rather than 1 in Forces.
            /// TODO: Could this indicate whether a spline is open or not? If this was the case then objpath_003 in Forces' w7a03_path.path and svpath_320_SV + svpath_321_SV in Frontiers' w6d08_sv_path.path should have it too? 
            /// </summary>
            public ushort UnknownUShort_1 { get; set; } = 0x01;

            /// <summary>
            /// An unknown array of boolean values. Forces and Frontiers have all of these set to true, but Lost World sometimes has values set to false in it.
            /// TODO: What do these do?
            /// </summary>
            public bool[] UnknownBooleanArray_1 { get; set; } = [];

            /// <summary>
            /// This spline's distance array.
            /// </summary>
            public float[] Distance { get; set; } = [];

            /// <summary>
            /// This spline's knot points.
            /// </summary>
            public Vector3[] Knots { get; set; } = [];

            /// <summary>
            /// This spline's up vector array.
            /// </summary>
            public Vector3[] UpVector { get; set; } = [];

            /// <summary>
            /// This spline's forward vector array.
            /// </summary>
            public Vector3[] ForwardVector { get; set; } = [];

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
            public object Type { get; set; } = SplineType2013Wars.Default;

            /// <summary>
            /// This path's UID, if it has one.
            /// </summary>
            public ulong? UID { get; set; }

            /// <summary>
            /// The name of the path that this one should flow into, if it has one.
            /// </summary>
            public string? NextPathName { get; set; }

            /// <summary>
            /// This path's grind speed, if it has one.
            /// </summary>
            public GrindSpeed? GrindSpeed { get; set; }

            /// <summary>
            /// This path's k-d tree.
            /// </summary>
            public KDTree KDTree { get; set; } = new();

            /// <summary>
            /// Displays this path's name in the debugger.
            /// </summary>
            public override string ToString() => Name;

            /// <summary>
            /// Initialises this path with default data.
            /// </summary>
            public SplinePath() { }

            /// <summary>
            /// Initialises this path with the provided data.
            /// </summary>
            public SplinePath(string name, ushort unknownUShort_1, bool[] unknownBooleanArray_1, float[] distance, Vector3[] knots, Vector3[] upVector, Vector3[] forwardVector, Vector3[]? doubleKnots, AABB axisAlignedBoundingBox, object type, ulong? uid, string? nextPathName, GrindSpeed? grindSpeed, KDTree kdTree)
            {
                Name = name;
                UnknownUShort_1 = unknownUShort_1;
                UnknownBooleanArray_1 = unknownBooleanArray_1;
                Distance = distance;
                Knots = knots;
                UpVector = upVector;
                ForwardVector = forwardVector;
                DoubleKnots = doubleKnots;
                AxisAlignedBoundingBox = axisAlignedBoundingBox;
                Type = type;
                UID = uid;
                NextPathName = nextPathName;
                GrindSpeed = grindSpeed;
                KDTree = kdTree;
            }

            /// <summary>
            /// Initialises this path by reading its data from a BINAReader.
            /// </summary>
            public SplinePath(BINAReader reader, FormatVersion version) => Read(reader, version);

            /// <summary>
            /// Reads the data for this path.
            /// </summary>
            public void Read(BINAReader reader, FormatVersion version)
            {
                // Read this path's name.
                if (version == FormatVersion.sonic_2013)
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                else
                    Name = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                // Read an unknown ushort value that is always 1 except for a single instance.
                UnknownUShort_1 = reader.ReadUInt16();

                // Read the count of spline knots for this path.
                ushort knotCount = reader.ReadUInt16();

                // Skip a floating point value which is always the same as the final value in the distance array.
                reader.JumpAhead(0x04);

                // Read the offset to an unknown table (of knotCount length) of booleans..
                long unknownBooleanTableOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read unknownBooleanTableOffset as a 32 bit integer instead.
                // TODO: Lost World can use false in this array, but Forces and Frontiers always have every value be true.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    unknownBooleanTableOffset = reader.ReadUInt32();
                }

                // Read the offset to this path's distance array.
                long distanceOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read distanceOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    distanceOffset = reader.ReadUInt32();
                }

                // Read the offset to this path's spline knot array.
                long knotOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read knotOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    knotOffset = reader.ReadUInt32();
                }

                // Read the offset to this path's up vector array.
                long upVectorOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read upVectorOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    upVectorOffset = reader.ReadUInt32();
                }

                // Read the offset to this path's forward vector array.
                long forwardVectorOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read forwardVectorOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    forwardVectorOffset = reader.ReadUInt32();
                }

                // Read offset to this path's double spline knot count.
                ulong doubleKnotCount = reader.ReadUInt64();

                // If this is a sonic_2013 format path, then jump back and read this path's double spline knot count as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    doubleKnotCount = reader.ReadUInt32();
                }

                // Read offset to this path's double spline knot array.
                long doubleKnotOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read doubleKnotOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    doubleKnotOffset = reader.ReadUInt32();
                }

                // Read this path's axis aligned bounding box.
                AxisAlignedBoundingBox = new(reader);

                // Read the count of tags in this path, usually 2 ("type" and "uid") but some don't have the "uid" one.
                ulong tagCount = reader.ReadUInt64();

                // If this is a sonic_2013 format path, then jump back and read the tag count as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    tagCount = reader.ReadUInt32();
                }

                // Read the offset to this path's tag data.
                long tagOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read tagOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    tagOffset = reader.ReadUInt32();
                }

                // Skip an unknown value that is always 0.
                if (version != FormatVersion.sonic_2013)
                    reader.CheckValue(0x00L);
                else
                    reader.CheckValue(0x00);

                // Read the offset to this path's k-d tree.
                long kdTreeOffset = reader.ReadInt64();

                // If this is a sonic_2013 format path, then jump back and read kdTreeOffset as a 32 bit integer instead.
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    kdTreeOffset = reader.ReadUInt32();
                }

                // Save our position so we can jump back for the next path.
                long position = reader.BaseStream.Position;

                // Jump to the unknown boolean array's offset.
                reader.JumpTo(unknownBooleanTableOffset, false);

                // Initialise this path's unknown boolean array.
                UnknownBooleanArray_1 = new bool[knotCount];

                // Read each value in this path's unknown boolean array.
                for (int booleanIndex = 0; booleanIndex < knotCount; booleanIndex++)
                    UnknownBooleanArray_1[booleanIndex] = reader.ReadBoolean();

                // Jump to the distance array's offset.
                reader.JumpTo(distanceOffset, false);

                // Initialise this path's distance array.
                Distance = new float[knotCount];

                // Read each value in this path's distance array.
                for (int distanceIndex = 0; distanceIndex < knotCount; distanceIndex++)
                    Distance[distanceIndex] = reader.ReadSingle();

                // Jump to the spline knot array's offset.
                reader.JumpTo(knotOffset, false);

                // Initialise this path's spline knot array.
                Knots = new Vector3[knotCount];

                // Read each knot for this spline.
                for (int knotIndex = 0; knotIndex < knotCount; knotIndex++)
                    Knots[knotIndex] = reader.ReadVector3();

                // Jump to the up vector array's offset.
                reader.JumpTo(upVectorOffset, false);

                // Initialise this path's up vector array.
                UpVector = new Vector3[knotCount];

                // Read each value in this path's up vector array.
                for (int upVectorIndex = 0; upVectorIndex < knotCount; upVectorIndex++)
                    UpVector[upVectorIndex] = reader.ReadVector3();

                // Jump to the forward vector array's offset.
                reader.JumpTo(forwardVectorOffset, false);

                // Initialise this path's forward vector array.
                ForwardVector = new Vector3[knotCount];

                // Read each value in this path's forward vector array.
                for (int forwardVectorIndex = 0; forwardVectorIndex < knotCount; forwardVectorIndex++)
                    ForwardVector[forwardVectorIndex] = reader.ReadVector3();

                // Only handle the double knot data if there is any, one path in Frontiers doesn't have any.
                if (doubleKnotCount != 0)
                {
                    // Jump to the double spline knot array's offset.
                    reader.JumpTo(doubleKnotOffset, false);

                    // Initialise this path's double spline knot array.
                    DoubleKnots = new Vector3[doubleKnotCount];

                    // Read each knot for this double spline.
                    for (ulong doubleKnotIndex = 0; doubleKnotIndex < doubleKnotCount; doubleKnotIndex++)
                        DoubleKnots[doubleKnotIndex] = reader.ReadVector3();
                }

                // Jump to this path's tag data offset.
                reader.JumpTo(tagOffset, false);

                // Loop through each tag in this path.
                for (ulong tagIndex = 0; tagIndex < tagCount; tagIndex++)
                {
                    // Set up a string for the tag type.
                    string tagType;

                    // Read this tag's type.
                    if (version == FormatVersion.sonic_2013)
                        tagType = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                    else
                        tagType = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);

                    // Skip an unused value that is either 2 (if the tag is "next") or 0 (in every other tag).
                    if (version == FormatVersion.sonic_2013)
                        reader.JumpAhead(0x04);
                    else
                        reader.JumpAhead(0x08);

                    // Read the next value depending on the tag type.
                    switch (tagType)
                    {
                        case "type":
                            // Read this path's type.
                            switch (version)
                            {
                                case FormatVersion.sonic_2013: Type = (SplineType2013Wars)reader.ReadUInt32(); break;
                                case FormatVersion.Wars:       Type = (SplineType2013Wars)reader.ReadUInt64(); break;
                                case FormatVersion.Rangers:    Type = (SplineTypeRangers)reader.ReadUInt64();  break;
                            }
                            break;

                        case "uid":
                            // Read this path's UID value.
                            if (version == FormatVersion.sonic_2013)
                                UID = reader.ReadUInt32();
                            else
                                UID = reader.ReadUInt64();
                            break;

                        case "next":
                            // Read this path's next name.
                            if (version == FormatVersion.sonic_2013)
                                NextPathName = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x04);
                            else
                                NextPathName = Helpers.ReadNullTerminatedStringTableEntry(reader, 0x08);
                            break;

                        case "grind_speed":
                            // Read this path's grind speed value.
                            if (version == FormatVersion.sonic_2013)
                                GrindSpeed = (GrindSpeed)reader.ReadUInt32();
                            else
                                GrindSpeed = (GrindSpeed)reader.ReadUInt64();
                            break;

                        default: throw new NotImplementedException($"Path tag with type '{tagType}' not yet handled!");
                    }
                }

                // Jump to the offset for this path's k-d tree.
                reader.JumpTo(kdTreeOffset, false);

                // Read this path's k-d tree.
                KDTree = new(reader, version);

                // Jump back to read the next path.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the table for this path.
            /// </summary>
            public void WriteTable(BINAWriter writer, FormatVersion version, int index)
            {
                // Add a string for this path's name.
                if (version == FormatVersion.sonic_2013)
                    writer.AddString($"Path{index}Name", Name, 0x04);
                else
                    writer.AddString($"Path{index}Name", Name, 0x08);

                // Write this path's unknown short value.
                writer.Write(UnknownUShort_1);

                // Write this path's knot count.
                writer.Write((ushort)Knots.Length);

                // Write this path's distance value.
                writer.Write(Distance[^1]);

                // Add an offset to this path's table of unknown, always true, booleans.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}UnknownBooleanTableOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}UnknownBooleanTableOffset", 0x08);

                // Add an offset to this path's distance array.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}DistanceOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}DistanceOffset", 0x08);

                // Add an offset to this path's spline knot array.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}KnotOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}KnotOffset", 0x08);

                // Add an offset to this path's up vector array.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}UpVectorOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}UpVectorOffset", 0x08);

                // Add an offset to this path's forward vector array.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}ForwardVectorOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}ForwardVectorOffset", 0x08);

                // Handle whether or not this spline uses double knots.
                if (DoubleKnots != null)
                {
                    // Write this spline's double knot count.
                    if (version == FormatVersion.sonic_2013)
                        writer.Write((uint)DoubleKnots.Length);
                    else
                        writer.Write((ulong)DoubleKnots.Length);

                    // Add an offset to this path's double spline knot table.
                    if (version == FormatVersion.sonic_2013)
                        writer.AddOffset($"Path{index}DoubleKnotOffset", 0x04);
                    else
                        writer.AddOffset($"Path{index}DoubleKnotOffset", 0x08);
                }
                else
                {
                    // Write two zero values in place of the double knot data.
                    if (version == FormatVersion.sonic_2013)
                    {
                        writer.Write(0);
                        writer.Write(0);
                    }
                    else
                    {
                        writer.Write(0L);
                        writer.Write(0L);
                    }
                }

                // Write this path's axis aligned bounding box.
                AxisAlignedBoundingBox.Write(writer);

                // Calculate the tag count, depending on if this path has a UID, Next Path and/or Grind Speed.
                ulong tagCount = 1;
                if (UID != null)
                    tagCount++;
                if (NextPathName != null)
                    tagCount++;
                if (GrindSpeed != null)
                    tagCount++;

                // Write the tag count.
                if (version == FormatVersion.sonic_2013)
                    writer.Write((uint)tagCount);
                else
                    writer.Write(tagCount);

                // Add an offset to this path's tag data.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}TagOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}TagOffset", 0x08);

                // Write an unknown value that is always 0.
                if (version == FormatVersion.sonic_2013)
                    writer.Write(0);
                else
                    writer.Write(0L);

                // Add an offset to this path's k-d tree.
                if (version == FormatVersion.sonic_2013)
                    writer.AddOffset($"Path{index}kdTreeOffset", 0x04);
                else
                    writer.AddOffset($"Path{index}kdTreeOffset", 0x08);
            }

            /// <summary>
            /// Writes the data for this path.
            /// </summary>
            public void WriteData(BINAWriter writer, FormatVersion version, int index)
            {
                // Fill in the offset for this path's unknown boolean table.
                writer.FillInOffset($"Path{index}UnknownBooleanTableOffset", false);

                // Write a the value for each boolean.
                for (int booleanIndex = 0; booleanIndex < UnknownBooleanArray_1.Length; booleanIndex++)
                    writer.Write(UnknownBooleanArray_1[booleanIndex]);

                // Realign to 0x04 bytes.
                writer.FixPadding(0x04);

                // Fill in the offset for this path's distance array.
                writer.FillInOffset($"Path{index}DistanceOffset", false);

                // Loop through and write each value in this path's distance array.
                for (int distanceIndex = 0; distanceIndex < Distance.Length; distanceIndex++)
                    writer.Write(Distance[distanceIndex]);

                // Fill in the offset for this path's spline knot table.
                writer.FillInOffset($"Path{index}KnotOffset", false);

                // Loop through and write each spline knot's position.
                for (int knotIndex = 0; knotIndex < Knots.Length; knotIndex++)
                    writer.Write(Knots[knotIndex]);

                // Fill in the offset for this path's up vector array.
                writer.FillInOffset($"Path{index}UpVectorOffset", false);

                // Loop through and write each value in this path's up vector array.
                for (int upVectorIndex = 0; upVectorIndex < UpVector.Length; upVectorIndex++)
                    writer.Write(UpVector[upVectorIndex]);

                // Fill in the offset for this path's forward vector array.
                writer.FillInOffset($"Path{index}ForwardVectorOffset", false);

                // Loop through and write each value in this path's forward vector array.
                for (int forwardVectorIndex = 0; forwardVectorIndex < ForwardVector.Length; forwardVectorIndex++)
                    writer.Write(ForwardVector[forwardVectorIndex]);

                // If this path has a double knot spline, then handle it.
                if (DoubleKnots != null)
                {
                    // Fill in the offset for this path's double knot spline.
                    writer.FillInOffset($"Path{index}DoubleKnotOffset", false);

                    // Loop through and write the position for each double spline knot.
                    for (int doubleKnotIndex = 0; doubleKnotIndex < DoubleKnots.Length; doubleKnotIndex++)
                        writer.Write(DoubleKnots[doubleKnotIndex]);
                }

                // Realign to 0x04 or 0x08 (depending on the format version).
                if (version == FormatVersion.sonic_2013)
                    writer.FixPadding(0x04);
                else
                    writer.FixPadding(0x08);

                // Fill in the offset for this path's type data.
                writer.FillInOffset($"Path{index}TagOffset", false);

                // Write the tags in different orders depending on the format version.
                // TODO: Can Forces and Frontiers use grind_speed and next? If not, then throw them out.
                if (version == FormatVersion.sonic_2013)
                {
                    WritePathTag(writer, version, index, "uid", UID, NextPathName);
                    WritePathTag(writer, version, index, "grind_speed", (ulong?)GrindSpeed, NextPathName);
                    WritePathTag(writer, version, index, "type", (ulong)Type, NextPathName);
                    WritePathTag(writer, version, index, "next", null, NextPathName);
                }
                else
                {
                    WritePathTag(writer, version, index, "grind_speed", (ulong?)GrindSpeed, NextPathName);
                    WritePathTag(writer, version, index, "type", (ulong)Type, NextPathName);
                    WritePathTag(writer, version, index, "uid", UID, NextPathName);
                    WritePathTag(writer, version, index, "next", null, NextPathName);
                }

                // Fill in the offset for this path's k-d tree.
                writer.FillInOffset($"Path{index}kdTreeOffset", false);

                KDTree.Write(writer, version, index);
            }
        }

        public class KDTree
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
            public byte[] UnknownData_1 { get; set; } = [];

            /// <summary>
            /// An unknown set of values, consisting of two integer values each.
            /// TODO: What is this?
            /// </summary>
            public List<uint[]> UnknownData_2 { get; set; } = [];

            /// <summary>
            /// An unknown list of integer values.
            /// TODO: What is this?
            /// </summary>
            public uint[] UnknownData_3 = [];

            /// <summary>
            /// Initialises this k-d tree with default data.
            /// </summary>
            public KDTree() { }

            /// <summary>
            /// Initialises this k-d tree with the provided data.
            /// </summary>
            public KDTree(uint unknownUInt32_1, uint unknownUInt32_2, byte[] unknownData_1, List<uint[]> unknownData_2, uint[] unknownData_3)
            {
                UnknownUInt32_1 = unknownUInt32_1;
                UnknownUInt32_2 = unknownUInt32_2;
                UnknownData_1 = unknownData_1;
                UnknownData_2 = unknownData_2;
                UnknownData_3 = unknownData_3;
            }

            /// <summary>
            /// Initialises this k-d tree by reading its data from a BINAReader.
            /// </summary>
            public KDTree(BINAReader reader, FormatVersion version) => Read(reader, version);

            /// <summary>
            /// Reads the data for this k-d tree.
            /// </summary>
            public void Read(BINAReader reader, FormatVersion version)
            {
                UnknownUInt32_1 = reader.ReadUInt32();

                UnknownUInt32_2 = reader.ReadUInt32();

                long UnknownData_1_Offset = reader.ReadInt64();
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    UnknownData_1_Offset = reader.ReadUInt32();
                }

                ulong UnknownData_2_Count = reader.ReadUInt64();
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    UnknownData_2_Count = reader.ReadUInt32();
                }

                long UnknownData_2_Offset = reader.ReadInt64();
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    UnknownData_2_Offset = reader.ReadUInt32();
                }

                ulong UnknownData_3_Count = reader.ReadUInt64();
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    UnknownData_3_Count = reader.ReadUInt32();
                }

                long UnknownData_3_Offset = reader.ReadInt64();
                if (version == FormatVersion.sonic_2013)
                {
                    reader.JumpBehind(0x08);
                    UnknownData_3_Offset = reader.ReadUInt32();
                }

                reader.JumpTo(UnknownData_1_Offset, false);

                UnknownData_1 = reader.ReadBytes((int)(UnknownData_2_Offset - UnknownData_1_Offset));

                reader.JumpTo(UnknownData_2_Offset, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownData_2_Count; unknownIndex++)
                {
                    uint[] unknownValues = [reader.ReadUInt32(), reader.ReadUInt32()];
                    UnknownData_2.Add(unknownValues);
                }

                UnknownData_3 = new uint[(int)UnknownData_3_Count];

                reader.JumpTo(UnknownData_3_Offset, false);

                for (ulong unknownIndex = 0; unknownIndex < UnknownData_3_Count; unknownIndex++)
                    UnknownData_3[unknownIndex] = reader.ReadUInt32();
            }

            /// <summary>
            /// Writes the data for this k-d tree.
            /// </summary>
            public void Write(BINAWriter writer, FormatVersion version, int index)
            {
                writer.Write(UnknownUInt32_1);
                writer.Write(UnknownUInt32_2);

                if (version == FormatVersion.sonic_2013)
                {
                    writer.AddOffset($"Path{index}kdTreeUnknownData1_Offset", 0x04);
                    writer.Write((uint)UnknownData_2.Count);
                    writer.AddOffset($"Path{index}kdTreeUnknownData2_Offset", 0x04);
                    writer.Write((uint)UnknownData_3.Length);
                    writer.AddOffset($"Path{index}kdTreeUnknownData3_Offset", 0x04);
                }
                else
                {
                    writer.AddOffset($"Path{index}kdTreeUnknownData1_Offset", 0x08);
                    writer.Write((ulong)UnknownData_2.Count);
                    writer.AddOffset($"Path{index}kdTreeUnknownData2_Offset", 0x08);
                    writer.Write((ulong)UnknownData_3.Length);
                    writer.AddOffset($"Path{index}kdTreeUnknownData3_Offset", 0x08);
                }

                writer.FillInOffset($"Path{index}kdTreeUnknownData1_Offset", false);

                writer.Write(UnknownData_1);

                writer.FillInOffset($"Path{index}kdTreeUnknownData2_Offset", false);

                foreach (uint[] value in UnknownData_2)
                {
                    writer.Write(value[0]);
                    writer.Write(value[1]);
                }

                writer.FillInOffset($"Path{index}kdTreeUnknownData3_Offset", false);

                foreach (uint value in UnknownData_3)
                    writer.Write(value);
            }
        }

        // Actual data presented to the end user.
        public SplinePath[] Data = [];

        // Internal values used for accurately resaving Sonic Lost World paths.
        bool sonic2013_HasPathOffsetTable = true;

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.Wars)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "HTAP");

            // Skip an unknown value that is always 0x200.
            reader.CheckValue(0x200);

            // Read the amount of paths in this file.
            long pathCount = reader.ReadInt64();

            // If this is a sonic_2013 format path, then jump back and read the path count as a 32 bit integer instead.
            if (version == FormatVersion.sonic_2013)
            {
                reader.JumpBehind(0x08);
                pathCount = reader.ReadInt32();
            }

            // Initialise the data array.
            Data = new SplinePath[pathCount];

            // If this is NOT a sonic_2013 format path, then read the path table offset and jump to it.
            if (version != FormatVersion.sonic_2013)
            {
                // Read the offset to this file's path table.
                long pathTableOffset = reader.ReadInt64();

                // Jump to this file's path table.
                reader.JumpTo(pathTableOffset, false);
            }

            // If this is a sonic_2013 format path, read an unknown value that is either 0x04, or 0x10.
            // If the value is 0x10, then its an offset to the path table, if it's 0x04, then I have no idea.
            else
            {
                if (reader.ReadUInt32() == 0x04)
                    sonic2013_HasPathOffsetTable = false;
            }

            // Loop through and read each path in this file.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex] = new(reader, version);

            // Close our BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        /// <param name="bigEndianSave">Whether this format should be saved in big endian for the Wii U version (sonic_2013 only).</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.Wars, bool bigEndianSave = false)
        {
            // Set up a BINA Version 2 Header.
            BINAv2Header header = new(210);

            // If this is a Sonic Lost World path, then change the BINA Header to v200.
            if (version == FormatVersion.sonic_2013)
                header = new BINAv2Header(200, bigEndianSave);

            // Set up our BINAWriter and write the BINAV2 header.
            BINAWriter writer = new(File.Create(filepath), header);

            // If this is a Wii U Sonic Lost World path, then flip the endianness.
            if (version == FormatVersion.sonic_2013 && bigEndianSave)
                writer.IsBigEndian = true;

            // Write this file's signature.
            if (!writer.IsBigEndian)
                writer.Write("HTAP");
            else
                writer.Write("PATH");

            // Write an unknown value that is always 0x200.
            writer.Write(0x200);

            // Write the count of splines in this file.
            if (version == FormatVersion.sonic_2013)
                writer.Write(Data.Length);
            else
                writer.Write((ulong)Data.Length);

            if (version == FormatVersion.sonic_2013)
            {
                // Determine if we need to write a 0x04, or an offset to the path table.
                if (sonic2013_HasPathOffsetTable)
                {
                    // Add an offset to the path table.
                    writer.AddOffset("PathTableOffset", 0x04);

                    // Fill in the path table offset.
                    writer.FillInOffset("PathTableOffset", false);
                }
                else
                {
                    writer.Write(0x04);
                }
            }
            else
            {
                // Add an offset to the path table.
                writer.AddOffset("PathTableOffset", 0x08);

                // Fill in the path table offset.
                writer.FillInOffset("PathTableOffset", false);
            }

            // Loop through and write each path's table.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex].WriteTable(writer, version, pathIndex);

            // Loop through and write each path's data.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex].WriteData(writer, version, pathIndex);

            // Close our BINAWriter.
            writer.Close(header);
        }

        /// <summary>
        /// Write a tag entry for this path.
        /// </summary>
        /// <param name="writer">The BINAWriter to use.</param>
        /// <param name="version">The format version, used to determine offset and data lengths.</param>
        /// <param name="pathIndex">The index of the path this tag belongs to.</param>
        /// <param name="tagType">The type of tag.</param>
        /// <param name="value">The data (if any) for this tag.</param>
        private static void WritePathTag(BINAWriter writer, FormatVersion version, int pathIndex, string tagType, ulong? value, string? nextPathName)
        {
            // If this tag doesn't have a value and isn't a "next" tag with a path name, then don't write anything.
            if (value == null)
                if (tagType != "next")
                    return;
                else if (nextPathName == null)
                    return;

            // Add the this tag type's string to the string table.
            if (version == FormatVersion.sonic_2013)
                writer.AddString($"Path{pathIndex}{tagType}", tagType, 0x04);
            else
                writer.AddString($"Path{pathIndex}{tagType}", tagType, 0x08);

            // Write an unknown value that is always 0 on every tag except for "next", which uses 2.
            if (version == FormatVersion.sonic_2013)
                if (tagType != "next")
                    writer.Write(0);
                else
                    writer.Write([0x02, 0x00, 0x00, 0x00]); // TODO: This is probably a byte then, considering it's still this sequence in the Wii U's big endian files.
            else
                if (tagType != "next")
                writer.Write(0L);
            else
                writer.Write(2L);

            // Write this path's data value, unless this is a "next" tag, then write a string entry instead.
            if (version == FormatVersion.sonic_2013)
                if (tagType != "next")
                    writer.Write((uint)value);
                else
                    writer.AddString($"Path{pathIndex}NextPath", nextPathName, 0x04);
            else
                if (tagType != "next")
                writer.Write((ulong)value);
            else
                writer.AddString($"Path{pathIndex}NextPath", nextPathName, 0x08);
        }

        /// <summary>
        /// Exports this path's splines to an OBJ file.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Write a comment that we can use on the import function (if the user wants to reimport this OBJ for some reason).
            obj.WriteLine("# KnuxLib PathSpline_WarsRangers OBJ Export");

            // Set up a variable to track vertices.
            int vertexCount = 0;

            // Loop through each path.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
            {
                // If this path uses double knots, then write those values.
                if (Data[pathIndex].DoubleKnots != null)
                {
                    // Starting from 0, write each knot value, incrementing by 2 rather than 1.
                    for (int vertexIndex = 0; vertexIndex < Data[pathIndex].DoubleKnots.Length; vertexIndex += 2)
                        obj.WriteLine($"v {Data[pathIndex].DoubleKnots[vertexIndex].X} {Data[pathIndex].DoubleKnots[vertexIndex].Y} {Data[pathIndex].DoubleKnots[vertexIndex].Z}");

                    // Write the remaining knot values, starting from 1 and also incrementing by 2.
                    for (int vertexIndex = 1; vertexIndex < Data[pathIndex].DoubleKnots.Length; vertexIndex += 2)
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
                if (Data[pathIndex].NextPathName == null)
                {
                    obj.WriteLine($"o {Data[pathIndex].Name}");
                    obj.WriteLine($"g {Data[pathIndex].Name}");
                }
                else
                {
                    obj.WriteLine($"o {Data[pathIndex].Name}_[{Data[pathIndex].NextPathName}]");
                    obj.WriteLine($"g {Data[pathIndex].Name}_[{Data[pathIndex].NextPathName}]");
                }

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

            // Write an extra line so my importer code doesn't freak out.
            obj.WriteLine();

            // Close this StreamWriter.
            obj.Close();
        }

        /// <summary>
        /// Imports an OBJ exported from either 3DS Max or Blender 4.x and converts lines in it to paths.
        /// </summary>
        /// <param name="filepath">The OBJ file to import.</param>
        /// <param name="version">The game version to import this file as.</param>
        public static SplinePath[] ImportOBJ(string filepath, FormatVersion version = FormatVersion.Wars)
        {
            List<SplinePath> paths = [];

            // Set up a value to handle the scale modifier.
            float scaleModifier = 1f;

            // Initialise a path.
            SplinePath path = new() { Name = "" };

            // Set up a list to store coordinates.
            List<Vector3> coordinates = [];

            // Set up a flag to check if a spline is single or double knotted.
            bool doubleKnot = false;

            // Set up a string to identify what exported the OBJ we're reading.
            string? identifier = null;

            // Read the OBJ.
            string[] importedOBJ = File.ReadAllLines(filepath);

            // Set the identifier to "max" if the 3DS Max OBJ Exporter comment (or the KnuxLib one) is present.
            if (importedOBJ[0].Contains("# 3ds Max Wavefront OBJ Exporter") || importedOBJ[0].Contains("# KnuxLib PathSpline_WarsRangers OBJ Export"))
                identifier = "max";

            // Set the identifier to "blender4" if the Blender 4.x comment is present. 
            if (importedOBJ[0].Contains("# Blender 4"))
                identifier = "blender4";

            // If the identifier line also has a scale modifier value added to it, then split and parse it.
            if (importedOBJ[0].Contains("Scale Modifier = "))
                scaleModifier = float.Parse(importedOBJ[0].Split("Scale Modifier = ")[1]);

            // Determine how to proceed based on the identifier.
            switch (identifier)
            {
                default:
                    throw new NotSupportedException();
                case "max":
                case "blender4":
                    // Set up a count to track Blender's different line system.
                    int blenderLineCount = 0;

                    // Loop through each line in the OBJ.
                    for (int lineIndex = 0; lineIndex < importedOBJ.Length; lineIndex++)
                    {
                        // If this line is the first vertex entry for an object or the last line in the OBJ, then handle finalising the path.
                        if ((importedOBJ[lineIndex].StartsWith("v ") && !importedOBJ[lineIndex - 1].StartsWith("v ")) || lineIndex == importedOBJ.Length - 1)
                        {
                            // Check if this path actually has a name so we don't save the first, completely empty, one.
                            // Due to how Blender OBJs are set up, this is only relevant for 3DS Max OBJs.
                            if (identifier == "max" && path.Name != "")
                            {
                                // Process the data using the generic function.
                                paths.Add(ProcessOBJData(path, coordinates, doubleKnot, version));

                                // Reset the coordinates list.
                                coordinates = [];

                                // Reset the double knot flag.
                                doubleKnot = false;

                                // Make a new path with an empty name.
                                path = new() { Name = "" };
                            }
                        }

                        // If this line starts with a v and a space, then handle it as a vertex coordinate.
                        if (importedOBJ[lineIndex].StartsWith("v "))
                        {
                            // Split the line on the space.
                            string[] split = importedOBJ[lineIndex].Split(' ');

                            // Parse the last three values in the split as floats, multiplying them by the scale modifier, and it to the coordinates array.
                            coordinates.Add(new(float.Parse(split[^3]) * scaleModifier, float.Parse(split[^2]) * scaleModifier, float.Parse(split[^1]) * scaleModifier));
                        }

                        // Handle setting the double knot flag on a 3DS Max OBJ.
                        if (identifier == "max")
                        {
                            // If this line starts with an l and a space and the next line does too, then set the double knot flag to true.
                            if (importedOBJ[lineIndex].StartsWith("l ") && importedOBJ[lineIndex + 1].StartsWith("l "))
                            {
                                // Check if the flag is already set as a ghetto way to detect splines with more than two lines.
                                if (doubleKnot)
                                    throw new NotSupportedException($"{path.Name} appears to have more than two lines, this is not supported.");

                                // Set the flag to indicate this spline has two lines.
                                doubleKnot = true;
                            }
                        }

                        // Calculate the line count on a Blender OBJ.
                        if (identifier == "blender4")
                            if (importedOBJ[lineIndex].StartsWith("l "))
                                blenderLineCount++;

                        // If this line starts with an o and a space, then split it on the space and take the last split as the path name.
                        // Blender OBJs put the name first, so we also need to handle the processing down here for those.
                        if (importedOBJ[lineIndex].StartsWith("o ") || (lineIndex == importedOBJ.Length - 1 && identifier == "blender4"))
                        {
                            // Handle processing this path if this is a Blender OBJ and we've actually read any lines.
                            if (identifier == "blender4" && blenderLineCount != 0)
                            {
                                // Determine if this spline needs the double knot flag.
                                // This doesn't block more than two splines like the Max one does, but OH WELL.
                                if (blenderLineCount % 2 == 0)
                                    doubleKnot = true;

                                // Process the data using the generic function.
                                paths.Add(ProcessOBJData(path, coordinates, doubleKnot, version));

                                // Reset the coordinates list.
                                coordinates = [];

                                // Reset the double knot flag.
                                doubleKnot = false;

                                // Make a new path with an empty name.
                                path = new() { Name = "" };

                                // Reset the line count tracker.
                                blenderLineCount = 0;
                            }

                            // Split the line on the space and take the last element as the path name.
                            path.Name = importedOBJ[lineIndex].Split(' ')[^1];
                        }
                    }

                    break;
            }

            return [.. paths];
        }

        /// <summary>
        /// Takes data read from an OBJ and creates a proper path from them.
        /// </summary>
        /// <param name="path">The path with the name preset.</param>
        /// <param name="coordinates">The list of point coordinates.</param>
        /// <param name="doubleKnot">Whether or not this spline is double knotted or not.</param>
        /// <param name="version">The format version to determine the spline type.</param>
        private static SplinePath ProcessOBJData(SplinePath path, List<Vector3> coordinates, bool doubleKnot, FormatVersion version)
        {
            // If this isn't a double knotted spline, then fill in the standard knot values.
            if (!doubleKnot)
            {
                // Initialise the knots array.
                path.Knots = new Vector3[coordinates.Count];

                // Loop through and write each coordinate value to the knot array.
                for (int coordinateIndex = 0; coordinateIndex < coordinates.Count; coordinateIndex++)
                    path.Knots[coordinateIndex] = coordinates[coordinateIndex];
            }
            else
            {
                // Split the coordinates array in half to determine the left and right splines.
                List<Vector3> leftSpline = coordinates.Take(coordinates.Count / 2).ToList();
                List<Vector3> rightSpline = coordinates.Skip(coordinates.Count / 2).ToList();

                // Check that the two splines for this path have the same number of knots.
                if (leftSpline.Count != rightSpline.Count)
                    throw new NotSupportedException($"{path.Name} has a different number of points for its two splines.");

                // Initialise the double knots array.
                path.DoubleKnots = new Vector3[coordinates.Count];

                // Initialise the knots array.
                path.Knots = new Vector3[leftSpline.Count];

                // Set up an index value to track values for the double knots.
                int index = 0;

                for (int coordinateIndex = 0; coordinateIndex < leftSpline.Count; coordinateIndex++)
                {
                    // Write the two double knot values.
                    path.DoubleKnots[index] = leftSpline[coordinateIndex];
                    path.DoubleKnots[index + 1] = rightSpline[coordinateIndex];

                    // Increment index by 2 for the next loop.
                    index += 2;

                    // Calculate the standard knot value for this pair.
                    path.Knots[coordinateIndex] = new(
                                                         (leftSpline[coordinateIndex].X + rightSpline[coordinateIndex].X) / 2,
                                                         (leftSpline[coordinateIndex].Y + rightSpline[coordinateIndex].Y) / 2,
                                                         (leftSpline[coordinateIndex].Z + rightSpline[coordinateIndex].Z) / 2
                                                     );
                }

                // Initalise the up vector array.
                path.UpVector = new Vector3[path.Knots.Length];

                // Loop through each knot and calculate its up vector.
                for (int knotIndex = 0; knotIndex < path.UpVector.Length - 1; knotIndex++)
                    path.UpVector[knotIndex] = CalculateDoublePointUpVector(path.DoubleKnots[knotIndex * 2], path.DoubleKnots[knotIndex * 2 + 1], path.DoubleKnots[knotIndex * 2 + 2]);

                // Set the last up vector to the same as the one before it.
                path.UpVector[^1] = path.UpVector[^2];
            }

            // Initialise the unknown boolean array.
            path.UnknownBooleanArray_1 = new bool[path.Knots.Length];

            // Write a true for each value in this array.
            // TODO: If this array turns out to be important, then figure out how the data in it can be reflected.
            for (int boolIndex = 0; boolIndex < path.UnknownBooleanArray_1.Length; boolIndex++)
                path.UnknownBooleanArray_1[boolIndex] = true;

            // Initialise the distance array.
            path.Distance = new float[path.Knots.Length];

            // Loop through each knot to calculate the distance values.
            for (int knotIndex = 1; knotIndex < path.Knots.Length; knotIndex++)
                path.Distance[knotIndex] = Helpers.CalculateDistance(path.Knots[knotIndex - 1], path.Knots[knotIndex]) + path.Distance[knotIndex - 1];

            // Set up lists to sort the x, y and z values of the coordinates as sorting a Vector3 list doesn't seem possible.
            List<float> x = [];
            List<float> y = [];
            List<float> z = [];

            // Loop through each coordinate and get the x, y and z values.
            foreach (var coordinate in coordinates)
            {
                x.Add(coordinate.X);
                y.Add(coordinate.Y);
                z.Add(coordinate.Z);
            }

            // Sort the lists to get the smallest and largest values.
            x.Sort();
            y.Sort();
            z.Sort();

            // Set up the axis aligned bounding box.
            path.AxisAlignedBoundingBox.Min = new(x[0], y[0], z[0]);
            path.AxisAlignedBoundingBox.Max = new(x[^1], y[^1], z[^1]);

            // Initialise the forward vector array.
            path.ForwardVector = new Vector3[path.Knots.Length];

            // Loop through and calculate the forward vectors for each knot (other than the last).
            for (int knotIndex = 0; knotIndex < path.Knots.Length - 1; knotIndex++)
                path.ForwardVector[knotIndex] = Helpers.CalculateForwardVector(path.Knots[knotIndex], path.Knots[knotIndex + 1]);

            // Set the last knot's forward vector to the same as the one before it.
            path.ForwardVector[^1] = path.ForwardVector[^2];

            // If this path isn't double knotted, then handle the up vector stuff here.
            if (!doubleKnot)
            {
                // Initialise the up vector array.
                path.UpVector = new Vector3[path.Knots.Length];

                // Loop through and calculate each knot's up vector value based on its forward vector.
                for (int knotIndex = 0; knotIndex < path.UpVector.Length; knotIndex++)
                    path.UpVector[knotIndex] = CalculateSinglePointUpVector(path.ForwardVector[knotIndex]);
            }

            #region Shamelessly copying the MaxScript for this as a potential placeholder.
            uint numberOfLineSegments = (uint)(path.Knots.Length - 1);
            if (doubleKnot)
                numberOfLineSegments = (uint)(path.DoubleKnots.Length - 2);
            path.KDTree.UnknownUInt32_1 = 0;
            path.KDTree.UnknownUInt32_2 = 2;
            path.KDTree.UnknownData_1 = [0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0];
            path.KDTree.UnknownData_2.Add([numberOfLineSegments, 0]);
            path.KDTree.UnknownData_3 = new uint[numberOfLineSegments];
            for (uint i = 0; i < numberOfLineSegments; i++)
                path.KDTree.UnknownData_3[i] = i;
            #endregion

            // Determine the generic type.
            string pathType = "default";
            if (path.Name.EndsWith("GR_spd1")) pathType = "grind_slow";
            if (path.Name.EndsWith("GR") || path.Name.EndsWith("GR_spd2")) pathType = "grind";
            if (path.Name.EndsWith("GR_spd3")) pathType = "grind_fast";
            if (path.Name.EndsWith("SV")) pathType = "side";

            // Set the NextPathName if needed.
            // TODO: Is this working? Does this need UIDs? My attempts to test this with Lost World is failing due to HedgeArcPack?
            try
            {
                path.NextPathName = path.Name[(path.Name.LastIndexOf('[') + 1)..path.Name.LastIndexOf(']')];
                path.Name = path.Name.Replace(path.Name[(path.Name.LastIndexOf('[') - 1)..(path.Name.LastIndexOf(']') + 1)], "");
            }
            catch { }

            // Determine the path type based on the end of the name (all of Sonic Team's official splines seem to follow this) and format version.
            switch (pathType)
            {
                case "grind_slow":
                case "grind":
                case "grind_fast":
                    if (version == FormatVersion.sonic_2013 || version == FormatVersion.Wars)
                        path.Type = SplineType2013Wars.GrindRail;
                    if (version == FormatVersion.Rangers)
                        path.Type = SplineTypeRangers.GrindRail;

                    if (pathType == "grind_slow") path.GrindSpeed = GrindSpeed.Slow;
                    if (pathType == "grind_fast") path.GrindSpeed = GrindSpeed.Fast;
                    break;

                case "side":
                    if (version == FormatVersion.sonic_2013 || version == FormatVersion.Wars)
                        path.Type = SplineType2013Wars.SideView;
                    if (version == FormatVersion.Rangers)
                        path.Type = SplineTypeRangers.SideView;
                    break;
            }

            // Save this path.
            return path;
        }

        /// <summary>
        /// Calculates a point's up vector for a single spline.
        /// </summary>
        /// <param name="forwardVector">The point's forward vector to calculate the up vector from.</param>
        /// <returns>The calculated up vector.</returns>
        private static Vector3 CalculateSinglePointUpVector(Vector3 forwardVector) => Vector3.Cross(Vector3.Cross(forwardVector, new(0, 1, 0)), forwardVector);

        /// <summary>
        /// Calculates a point's up vector for a double spline.
        /// </summary>
        /// <param name="pointA">The position of the first point.</param>
        /// <param name="pointB">The position of the point opposite the first one.</param>
        /// <param name="pointC">The position of the point connected to the first one.</param>
        private static Vector3 CalculateDoublePointUpVector(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            // Calculate the forward vector between pointA and pointC.
            Vector3 forwardVector = Helpers.CalculateForwardVector(pointA, pointC);

            // Calculate the forward vector between pointA and pointB to get the right vector.
            var rightVector = Helpers.CalculateForwardVector(pointA, pointB);

            // Cross the right vector with the forward vector and return the result.
            return Vector3.Normalize(Vector3.Cross(rightVector, forwardVector));
        }
    }
}
