namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Figure out the unknowns.
    // TODO: Test format saving more, the BINA Offset Table is wrong, but the game seems OK with it?
    // TODO: Replace the Forward and Up Vectors with calculations, tried before, but it caused NaN problems.
    // TODO: Support saving in little endian for Sonic Colours Ultimate.
    // TODO: Potentially merge this in with PathSpline.cs? Especially if replacing the vectors with calculated version goes through.
    public class PathSpline_2010 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline_2010() { }
        public PathSpline_2010(string filepath, bool export = false)
        {
            // Check if the input file is an OBJ.
            if (StringHelpers.GetExtension(filepath) == ".obj")
            {
                // Import this OBJ.
                Data = ImportOBJ(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.path.bin");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    ExportOBJ($@"{StringHelpers.GetExtension(filepath, true)}.obj");
            }
        }

        public class SplinePath
        {
            /// <summary>
            /// The name of this spline.
            /// </summary>
            public string Name { get; set; } = "objpath_001";

            /// <summary>
            /// This spline's distance array.
            /// TODO: Should we really be storing these values rather than just calculating them on the fly?
            /// </summary>
            public float[] Distance { get; set; } = [];

            /// <summary>
            /// An unknown array of (potentially) boolean values.
            /// TODO: What do these do?
            /// </summary>
            public bool[] UnknownBooleanArray_1 { get; set; } = [];

            /// <summary>
            /// This spline's knot points.
            /// </summary>
            public Vector3[] Knots { get; set; } = [];

            /// <summary>
            /// This spline's up vector array.
            /// TODO: Should we really be storing these values rather than just calculating them on the fly?
            /// </summary>
            public Vector3[] UpVector { get; set; } = [];

            /// <summary>
            /// This spline's forward vector array.
            /// TODO: Should we really be storing these values rather than just calculating them on the fly?
            /// </summary>
            public Vector3[] ForwardVector { get; set; } = [];

            /// <summary>
            /// An unknown array of floating point values.
            /// TODO: What do these do?
            /// </summary>
            public float[] UnknownFloatArray_1 { get; set; } = [];

            /// <summary>
            /// This spline's knot points for double splines.
            /// </summary>
            public Vector3[]? DoubleKnots { get; set; }

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
            public SplinePath(string name, float[] distance, bool[] unknownBooleanArray_1, Vector3[] knots, Vector3[] upVector, Vector3[] forwardVector, float[] unknownFloatArray_1, Vector3[]? doubleKnots)
            {
                Name = name;
                Distance = distance;
                UnknownBooleanArray_1 = unknownBooleanArray_1;
                Knots = knots;
                UpVector = upVector;
                ForwardVector = forwardVector;
                UnknownFloatArray_1 = unknownFloatArray_1;
                DoubleKnots = doubleKnots;
            }

            /// <summary>
            /// Initialises this path by reading its data from a BINAReader.
            /// </summary>
            public SplinePath(BINAReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this path.
            /// </summary>
            public void Read(BINAReader reader)
            {
                // Read this path's name.
                Name = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

                // Read an index used by objPath and Quick Step splines, this value always matches the number at the end of the path name, or is 0.
                uint objPathIndex = reader.ReadUInt32();

                // Read whether this path is open or closed.
                bool isOpen = reader.ReadBoolean(0x04);

                // Read this path's total distance. This value should match the final knot's distance value.
                float pathDistance = reader.ReadSingle();

                // Read the count of single knots in this path.
                uint pathSingleKnotCount = reader.ReadUInt32();

                // Read the offset to this path's single knot table.
                uint pathSingleKnotTableOffset = reader.ReadUInt32();

                // Read the count of double knots in this path.
                uint pathDoubleKnotCount = reader.ReadUInt32();

                // Read the offset to this path's double knot table.
                uint pathDoubleKnotTableOffset = reader.ReadUInt32();

                // Save our position so we can jump back for the next path.
                long position = reader.BaseStream.Position;

                // Initialise this path's data arrays.
                Distance = new float[pathSingleKnotCount];
                UnknownBooleanArray_1 = new bool[pathSingleKnotCount];
                Knots = new Vector3[pathSingleKnotCount];
                UpVector = new Vector3[pathSingleKnotCount];
                ForwardVector = new Vector3[pathSingleKnotCount];
                UnknownFloatArray_1 = new float[pathSingleKnotCount];

                // Jump to this path's single knot table.
                reader.JumpTo(pathSingleKnotTableOffset, false);

                // Loop through and read each knot.
                // For splines that have double knots, the positions of these knots will be the median of the double knot positions.
                for (int knotIndex = 0; knotIndex < pathSingleKnotCount; knotIndex++)
                {
                    Distance[knotIndex] = reader.ReadSingle();
                    UnknownBooleanArray_1[knotIndex] = reader.ReadBoolean(0x04);
                    Knots[knotIndex] = reader.ReadVector3();
                    UpVector[knotIndex] = reader.ReadVector3();
                    ForwardVector[knotIndex] = reader.ReadVector3();
                    UnknownFloatArray_1[knotIndex] = reader.ReadSingle();
                }

                // Check if this path has a double knot component.
                if (pathDoubleKnotCount > 0)
                {
                    // Initialise this path's double knot array.
                    DoubleKnots = new Vector3[pathDoubleKnotCount];

                    // Jump to this path's double knot table.
                    reader.JumpTo(pathDoubleKnotTableOffset, false);

                    // Loop through and read each double knot's position.
                    for (int doubleKnotIndex = 0; doubleKnotIndex < pathDoubleKnotCount; doubleKnotIndex++)
                        DoubleKnots[doubleKnotIndex] = reader.ReadVector3();
                }

                // Jump back to read the next path.
                reader.JumpTo(position);
            }

            /// <summary>
            /// Writes the data for this path.
            /// </summary>
            public void Write(BINAWriter writer, int pathIndex)
            {
                // Add this path's name and the writer's current position to our list of offsets.
                PathOffsets.Add(Name, writer.BaseStream.Position);

                // Add a string for this path's name. We use a different name for the offset as it seemed to break if we reused the same one.
                writer.AddString($"Path{pathIndex}NameReference", Name);

                // If this is an objpath path, then parse the path index and write it.
                if (Name.StartsWith("objpath"))
                    writer.Write(int.Parse(Name[(Name.LastIndexOf('_') + 1)..]));

                // If this is a Quick Step path, then write the final number's value here.
                // TODO: If a number somehow reaches higher than 9, this'll fall flat on its face.
                else if (Name.Contains("@QS"))
                    writer.Write(int.Parse(Name[^1..]));

                // If this isn't an objpath path, then just write a 0.
                else
                    writer.Write(0x00);

                // Determine and write this path's open/closed status.
                if (Knots[^1] != Knots[0])
                    writer.Write(true, 0x04);
                else
                    writer.Write(false, 0x04);

                // Write the final value in the distance array for the total distance.
                writer.Write(Distance[^1]);

                // Write the single knot count.
                writer.Write(Knots.Length);

                // Add an offset for the single knot table.
                writer.AddOffset($"Path{pathIndex}SingleKnotTable");

                // Check if this path has a double knot table.
                if (DoubleKnots != null)
                {
                    // Write the double knot count.
                    writer.Write(DoubleKnots.Length);

                    // Add an offset for the double knot table.
                    writer.AddOffset($"Path{pathIndex}DoubleKnotTable");
                }

                // If this path doesn't have a double knot table, just write eight null bytes instead.
                else
                    writer.WriteNulls(0x08);
            }

            /// <summary>
            /// Writes the data for each single knot in this path.
            /// </summary>
            public void WriteSingleKnots(BINAWriter writer, int pathIndex)
            {
                // Fill in the offset for this path's single knot table.
                writer.FillInOffset($"Path{pathIndex}SingleKnotTable");

                // Loop through and write each knot's data.
                for (int knotIndex = 0; knotIndex < Knots.Length; knotIndex++)
                {
                    writer.Write(Distance[knotIndex]);
                    writer.Write(UnknownBooleanArray_1[knotIndex], 0x04);
                    writer.Write(Knots[knotIndex]);
                    writer.Write(UpVector[knotIndex]);
                    writer.Write(ForwardVector[knotIndex]);
                    writer.Write(UnknownFloatArray_1[knotIndex]);
                }
            }

            /// <summary>
            /// Writes the data for each double knot in this path.
            /// </summary>
            public void WriteDoubleKnots(BINAWriter writer, int pathIndex)
            {
                // If this path doesn't have a double knot table, then just return.
                if (DoubleKnots == null)
                    return;

                // Fill in the offset for this path's double knot table.
                writer.FillInOffset($"Path{pathIndex}DoubleKnotTable");

                // Loop through and write each double knot.
                foreach (Vector3 doubleKnot in DoubleKnots)
                    writer.Write(doubleKnot);
            }
        }

        // Actual data presented to the end user.
        public SplinePath[] Data = [];

        // Internal dictionary used for writing.
        private static Dictionary<string, long> PathOffsets = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BINAReader.
            BINAReader reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "PATH");

            // Check for an unknown value of 0x01.
            reader.CheckValue(0x01);

            // Initialise the data array.
            Data = new SplinePath[reader.ReadUInt32()];

            // Read the offset to this file's path name table.
            // We don't actually bother to read the data here, as it just consists of an offset to each spline name, accompanied by a linear index.
            uint pathNameTableOffset = reader.ReadUInt32();

            // Check for a value matching the data array's length.
            // It's likely that this is the actual path count, and the previous value is meant to be a name count, but they both always match.
            reader.CheckValue(Data.Length);

            // Read the offset to this file's path table.
            uint pathTableOffset = reader.ReadUInt32();

            // Read an unknown offset that either matches pathTableOffset or is set to 0.
            uint UnknownTableOffset_1 = reader.ReadUInt32();

            // We don't parse the following data, as they're all tables of offsets to path names, seemingly to determine their type.
            // Read the count of entries in and the offset to this file's objpath table.
            uint objPathCount = reader.ReadUInt32();
            uint objPathTableOffset = reader.ReadUInt32();

            // Read the count of entries in and the offset to this file's Quick Step table.
            uint quickStepCount = reader.ReadUInt32();
            uint quickStepTableOffset = reader.ReadUInt32();

            // Read the count of entries in and the offset to this file's Side View table.
            uint sideViewCount = reader.ReadUInt32();
            uint sideViewTableOffset = reader.ReadUInt32();

            // Read the count of entries in and the offset to this file's Grind table.
            uint grindCount = reader.ReadUInt32();
            uint grindTableOffset = reader.ReadUInt32();

            // Check for two empty values, these likely would have been a count and offset like the ones above, but whatever spline type it was for was scrapped.
            reader.CheckValue(0x00, 0x02);

            // Jump to this file's path table.
            reader.JumpTo(pathTableOffset, false);

            // Loop through and read each path in this file.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex] = new(reader);

            // Close our BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Reset the PathOffsets dictionary.
            PathOffsets = [];

            // Set up values to hold the count of path types.
            int objPathCount = 0;
            int quickStepCount = 0;
            int sideViewCount = 0;
            int grindCount = 0;

            // Loop through each path to determine their type and increment the approriate counter.
            foreach (SplinePath path in Data)
            {
                if (path.Name.Contains("objpath")) objPathCount++;
                if (path.Name.Contains("@QS")) quickStepCount++;
                if (path.Name.Contains("@SV")) sideViewCount++;
                if (path.Name.Contains("@GR")) grindCount++;
            }

            // Set up a BINA Version 1 Header.
            BINAv1Header header = new();

            // Create this file through a BINAWriter.
            BINAWriter writer = new(File.Create(filepath), header);

            // Write this file's signature.
            writer.Write("PATH");

            // Write an unknown value of 0x01.
            writer.Write(0x01);

            // Write the count of paths in this file.
            writer.Write(Data.Length);

            // Add an offset to the path name table.
            writer.AddOffset("PathNameTable");

            // Write the count of paths in this file.
            writer.Write(Data.Length);

            // Add an offset to the path table.
            writer.AddOffset("PathTable");

            // Add another offset to the path table.
            // TODO: Some files have this as 0 rather than an offset, does that change anything?
            writer.AddOffset("PathTableDuplicate");

            // Write the count of objpath splines.
            writer.Write(objPathCount);

            // Add an offset to the objpath table.
            writer.AddOffset("ObjPathTableOffset");

            // Write the count of quick step splines.
            writer.Write(quickStepCount);

            // Add an offset to the quick step table.
            writer.AddOffset("QuickStepTableOffset");

            // Write the count of side view splines.
            writer.Write(sideViewCount);

            // Add an offset to the side view table.
            writer.AddOffset("SideViewTableOffset");

            // Write the count of grind splines.
            writer.Write(grindCount);

            // Add an offset to the grind table.
            writer.AddOffset("GrindTableOffset");

            // Write eight null bytes.
            writer.WriteNulls(0x08);

            // Fill in the offset for the path name table.
            writer.FillInOffset("PathNameTable");

            // Loop through and write each path's name and its linear index.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
            {
                writer.AddString($"Path{pathIndex}Name", Data[pathIndex].Name);
                writer.Write(pathIndex);
            }

            // Fill in the two path table offsets.
            writer.FillInOffset("PathTable");
            writer.FillInOffset("PathTableDuplicate");

            // Loop through and write each path's data.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex].Write(writer, pathIndex);

            // Loop through and write each path's single knots.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex].WriteSingleKnots(writer, pathIndex);

            // Loop through and write each path's double knots.
            for (int pathIndex = 0; pathIndex < Data.Length; pathIndex++)
                Data[pathIndex].WriteDoubleKnots(writer, pathIndex);

            // Write the four path type tables.
            WritePathTypeTable(objPathCount, writer, "ObjPathTableOffset", "objpath");
            WritePathTypeTable(quickStepCount, writer, "QuickStepTableOffset", "@QS");
            WritePathTypeTable(sideViewCount, writer, "SideViewTableOffset", "@SV");
            WritePathTypeTable(grindCount, writer, "GrindTableOffset", "@GR");

            // Close our BINAWriter.
            writer.Close(header);
        }

        /// <summary>
        /// Writes the path type offset tables.
        /// </summary>
        /// <param name="pathCount">The count of paths of this type.</param>
        /// <param name="writer">The BINAWriter to use.</param>
        /// <param name="offset">The offset to fill in.</param>
        /// <param name="pathType">The type of path this table is for.</param>
        private static void WritePathTypeTable(int pathCount, BINAWriter writer, string offset, string pathType)
        {
            // Check if we actually have any paths of this type.
            if (pathCount > 0)
            {
                // Set up an index to track which path we're writing.
                int pathIndex = 0;

                // Fill in the offset to this table.
                writer.FillInOffset(offset);

                // Loop through each path in the dictionary.
                foreach (KeyValuePair<string, long> path in PathOffsets)
                {
                    // Check if this path's name has our type.
                    if (path.Key.Contains(pathType))
                    {
                        // Add an offset for this path.
                        writer.AddOffset($"{pathType}{pathIndex}");

                        // Jump to this path's previously filled data.
                        writer.BaseStream.Position = path.Value;

                        // Fill in our offset.
                        writer.FillInOffset($"{pathType}{pathIndex}");

                        // Jump back to the end of the file to continue.
                        writer.BaseStream.Position = writer.BaseStream.Length;

                        // Increment our path index.
                        pathIndex++;
                    }
                }
            }
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
            obj.WriteLine("# KnuxLib Hedgehog Engine Path Spline OBJ Export");

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

            // Write an extra line so my importer code doesn't freak out.
            obj.WriteLine();

            // Close this StreamWriter.
            obj.Close();
        }

        /// <summary>
        /// Imports an OBJ exported from either 3DS Max or Blender 4.x and converts lines in it to paths.
        /// </summary>
        /// <param name="filepath">The OBJ file to import.</param>
        public static SplinePath[] ImportOBJ(string filepath)
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
            if (importedOBJ[0].Contains("# 3ds Max Wavefront OBJ Exporter") || importedOBJ[0].Contains("# KnuxLib Hedgehog Engine Path Spline OBJ Export"))
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
                                paths.Add(ProcessOBJData(path, coordinates, doubleKnot));

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
                                paths.Add(ProcessOBJData(path, coordinates, doubleKnot));

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
        private static SplinePath ProcessOBJData(SplinePath path, List<Vector3> coordinates, bool doubleKnot)
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
                    path.UpVector[knotIndex] = SplineHelpers.CalculateDoublePointUpVector(path.DoubleKnots[knotIndex * 2], path.DoubleKnots[knotIndex * 2 + 1], path.DoubleKnots[knotIndex * 2 + 2]);

                // Set the last up vector to the same as the one before it.
                path.UpVector[^1] = path.UpVector[^2];
            }

            // Initialise the unknown boolean array.
            path.UnknownBooleanArray_1 = new bool[path.Knots.Length];

            // Write a true for each value in this array.
            // TODO: If this array turns out to be important, then figure out how the data in it can be reflected.
            for (int boolIndex = 0; boolIndex < path.UnknownBooleanArray_1.Length; boolIndex++)
                path.UnknownBooleanArray_1[boolIndex] = true;

            // Initialise the unknown float array.
            path.UnknownFloatArray_1 = new float[path.Knots.Length];

            // Write a 0 for each value in this array.
            // TODO: If this array turns out to be important, then figure out how the data in it can be reflected.
            for (int floatIndex = 0; floatIndex < path.UnknownFloatArray_1.Length; floatIndex++)
                path.UnknownFloatArray_1[floatIndex] = 0f;

            // Initialise the distance array.
            path.Distance = new float[path.Knots.Length];

            // Loop through each knot to calculate the distance values.
            for (int knotIndex = 1; knotIndex < path.Knots.Length; knotIndex++)
                path.Distance[knotIndex] = SplineHelpers.CalculateDistance(path.Knots[knotIndex - 1], path.Knots[knotIndex]) + path.Distance[knotIndex - 1];

            // Initialise the forward vector array.
            path.ForwardVector = new Vector3[path.Knots.Length];

            // Loop through and calculate the forward vectors for each knot (other than the last).
            for (int knotIndex = 0; knotIndex < path.Knots.Length - 1; knotIndex++)
                path.ForwardVector[knotIndex] = SplineHelpers.CalculateForwardVector(path.Knots[knotIndex], path.Knots[knotIndex + 1]);

            // Set the last knot's forward vector to the same as the one before it.
            path.ForwardVector[^1] = path.ForwardVector[^2];

            // If this path isn't double knotted, then handle the up vector stuff here.
            if (!doubleKnot)
            {
                // Initialise the up vector array.
                path.UpVector = new Vector3[path.Knots.Length];

                // Loop through and calculate each knot's up vector value based on its forward vector.
                for (int knotIndex = 0; knotIndex < path.UpVector.Length; knotIndex++)
                    path.UpVector[knotIndex] = SplineHelpers.CalculateSinglePointUpVector(path.ForwardVector[knotIndex]);
            }

            // Save this path.
            return path;
        }
    }
}
