using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Storybook
{
    // TODO: Figure out what the values for the points actually are and what they do, not so sure on some of the Vector3s.
    // TODO: Add proper exporting for this format.
    // TODO: Finish importing.
    public class PathSpline : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline() { }
        public PathSpline(string filepath, FormatVersion version = FormatVersion.SecretRings, bool export = false)
        {
            Load(filepath, version);

            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        public enum FormatVersion
        {
            SecretRings = 0,
            BlackKnight = 1
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PathType
        {
            Aircar = 0,
            Grind = 1,
            Standard = 2
        }

        public class FormatData
        {
            /// <summary>
            /// The total distance this spline covers.
            /// </summary>
            public float TotalDistance { get; set; }

            /// <summary>
            /// This path's type.
            /// </summary>
            public PathType Type { get; set; } = PathType.Standard;

            /// <summary>
            /// This path's name, only present in Black Knight files.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// The points that make up this path.
            /// </summary>
            public List<object> Points { get; set; } = new();

            public override string ToString() => Name;
        }

        public class SplinePointType1
        {
            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }
        }

        public class SplinePointType3
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? Seems to control rotation of the player in some way?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The distance between this point and the next.
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// How far the player can deviate left and right from this point's center.
            /// </summary>
            public float Deviation { get; set; }

            /// <summary>
            /// This point's surface type.
            /// </summary>
            public uint Surface { get; set; }

            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }
        }

        public class SplinePointType4
        {
            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }

            /// <summary>
            /// The distance between this point and the next.
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// How far the player can deviate left and right from this point's center.
            /// </summary>
            public float Deviation { get; set; }

            /// <summary>
            /// This point's surface type.
            /// </summary>
            public uint Surface { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The game version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the value that indicates the type of points this spline uses.
            ushort pointType = reader.ReadUInt16();

            // Read the amount of points that make up this path.
            ushort pointCount = reader.ReadUInt16();

            // Read this path's unknown floating point value.
            Data.TotalDistance = reader.ReadSingle();

            // Read the offset to this file's path table (always 0x10).
            uint pathTableOffset = reader.ReadUInt32();

            // Read this path's type.
            Data.Type = (PathType)reader.ReadUInt32();

            // Jump to the file's path table (should already be at this position but just to be safe).
            reader.JumpTo(pathTableOffset);

            // If this is a Black Knight file, then read the path name.
            if (version == FormatVersion.BlackKnight)
                Data.Name = reader.ReadNullPaddedString(0x20);

            // Loop through the points in this path.
            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                // Read the points differently depending on the type.
                switch (pointType)
                {
                    case 1:
                        // Skip an unknown value of 0.
                        reader.JumpAhead(0x04);

                        // Read the values that make up a Type 1 point.
                        Data.Points.Add(new SplinePointType1()
                        {
                            Position = reader.ReadVector3(),
                            UnknownFloat_1 = reader.ReadSingle()
                        });
                        break;

                    case 3:
                        // Read the values that make up a Type 3 point.
                        Data.Points.Add(new SplinePointType3()
                        {
                            UnknownUInt32_1 = reader.ReadUInt32(),
                            Distance = reader.ReadSingle(),
                            Deviation = reader.ReadSingle(),
                            Surface = reader.ReadUInt32(),
                            Position = reader.ReadVector3()
                        });
                        break;

                    case 4:
                        // Read the values that make up a Type 4 point.
                        Data.Points.Add(new SplinePointType4()
                        {
                            Position = reader.ReadVector3(),
                            UnknownVector3_1 = reader.ReadVector3(),
                            UnknownVector3_2 = reader.ReadVector3(),
                            Distance = reader.ReadSingle(),
                            Deviation = reader.ReadSingle(),
                            Surface = reader.ReadUInt32()
                        });
                        break;
                }
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Get the point type's class as a string.
            string pointType = Data.Points[0].GetType().ToString();

            // Write the point type value.
            switch (pointType)
            {
                case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType1": writer.Write((ushort)1); break;
                case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType3": writer.Write((ushort)3); break;
                case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType4": writer.Write((ushort)4); break;
            }

            // Write the point count.
            writer.Write((ushort)Data.Points.Count);

            // Write the unknown floating point value.
            writer.Write(Data.TotalDistance);

            // Add an offset for the path table.
            writer.AddOffset("PathTableOffset");

            // Write the path type.
            writer.Write((uint)Data.Type);

            // Fill in the offset for the path table.
            writer.FillOffset("PathTableOffset");

            // If this is a Black Knight file, then write the path name.
            if (version == FormatVersion.BlackKnight)
                writer.WriteNullPaddedString(Data.Name, 0x20);

            // Loop through and write each point.
            for (int pointIndex = 0; pointIndex < Data.Points.Count; pointIndex++)
            {
                // Write the points depending on their type.
                switch (pointType)
                {
                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType1":
                        // Write an unknown value of 0.
                        writer.Write(0);

                        // Write this point's position.
                        writer.Write(((SplinePointType1)Data.Points[pointIndex]).Position);

                        // Write this point's unknown floating point value.
                        writer.Write(((SplinePointType1)Data.Points[pointIndex]).UnknownFloat_1);
                        break;

                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType3":
                        // Write this point's unknown integer value.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).UnknownUInt32_1);

                        // Write this point's distance value.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).Distance);

                        // Write this point's deviation value.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).Deviation);

                        // Write this point's surface type.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).Surface);

                        // Write this point's position.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).Position);
                        break;

                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType4":
                        // Write this point's position.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).Position);

                        // Write this point's first unknown Vector3 value.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).UnknownVector3_1);

                        // Write this point's second unknown Vector3 value.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).UnknownVector3_2);

                        // Write this point's distance value.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).Distance);

                        // Write this point's deviation value.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).Deviation);

                        // Write this point's surface type.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).Surface);
                        break;
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
        
        /// <summary>
        /// Imports an OBJ exported from either 3DS Max or Blender 4.x and converts lines in it to paths.
        /// </summary>
        /// <param name="filepath">The OBJ file to import.</param>
        public void ImportOBJ(string filepath)
        {
            // Set up a value to handle the scale modifier.
            float scaleModifier = 1f;

            // Set up a list to store coordinates.
            List<Vector3> coordinates = new();

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

            #region Check that this file only has one spline in it.
            // Set up a boolean to check if we've found an object already.
            bool foundObject = false;

            // Loop through each line in this file.
            for (int lineIndex = 0; lineIndex < importedOBJ.Length; lineIndex++)
            {
                // If this line starts with an o and a space, then check if we've found an object already.
                // If so, throw an exception. If we haven't, then we set the foundObject to true and continue looping.
                if (importedOBJ[lineIndex].StartsWith("o "))
                {
                    if (foundObject) throw new NotSupportedException();
                    foundObject = true;
                }
            }
            #endregion

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
                            if (identifier == "max" && Data.Name != null)
                            {
                                // Process the data using the generic function.
                                ProcessOBJData(coordinates, doubleKnot);
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
                                    throw new NotSupportedException($"{Data.Name} appears to have more than two lines, this is not supported.");

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
                                ProcessOBJData(coordinates, doubleKnot);
                            }

                            // Split the line on the space and take the last element as the path name.
                            Data.Name = importedOBJ[lineIndex].Split(' ')[^1];
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Takes data read from an OBJ and creates a proper path from them.
        /// TODO: Tidy up and document.
        /// </summary>
        /// <param name="coordinates">The list of point coordinates.</param>
        /// <param name="doubleKnot">Whether or not this spline is double knotted or not.</param>
        private void ProcessOBJData(List<Vector3> coordinates, bool doubleKnot)
        {
            // TODO: Finish the single knot splines.
            if (!doubleKnot)
            {
                for (int pointIndex = 0; pointIndex < coordinates.Count; pointIndex++)
                {
                    SplinePointType3 point = new()
                    {
                        Position = coordinates[pointIndex]
                    };
                }
            }
            else
            {
                for (int pointIndex = 0; pointIndex < coordinates.Count / 2; pointIndex++)
                {
                    SplinePointType3 point = new();
                    point.Position = Vector3.Lerp(coordinates[pointIndex], coordinates[pointIndex + (coordinates.Count / 2)], 0.5f);
                    point.Deviation = (coordinates[pointIndex + (coordinates.Count / 2)] - coordinates[pointIndex]).X + (coordinates[pointIndex + (coordinates.Count / 2)] - coordinates[pointIndex]).Z;

                    if (pointIndex != (coordinates.Count / 2) - 1)
                    {
                        Vector3 nextPointPosition = Vector3.Lerp(coordinates[pointIndex + 1], coordinates[pointIndex + 1 + (coordinates.Count / 2)], 0.5f);
                        float xDist = point.Position.X - nextPointPosition.X;
                        float yDist = point.Position.Y - nextPointPosition.Y;
                        float zDist = point.Position.Z - nextPointPosition.Z;

                        point.Distance = (Math.Abs(xDist) + Math.Abs(yDist) + Math.Abs(zDist));
                    }

                    Data.Points.Add(point);
                }
            }

            for (int pointIndex = 0; pointIndex < Data.Points.Count; pointIndex++)
                Data.TotalDistance += (Data.Points[pointIndex] as SplinePointType3).Distance;
        }

        /// <summary>
        /// Exports the position values from this spline to an OBJ.
        /// TODO: Can we potentially calculate a double knot spline export using the deviation value?
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up the StreamWriter.
            StreamWriter obj = new(filepath);

            // Get the point type's class as a string.
            string pointType = Data.Points[0].GetType().ToString();

            // Write each point's positions depending on type.
            for (int pointIndex = 0; pointIndex < Data.Points.Count; pointIndex++)
            {
                switch (pointType)
                {
                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType1":
                        obj.WriteLine($"v {((SplinePointType1)Data.Points[pointIndex]).Position.X} {((SplinePointType1)Data.Points[pointIndex]).Position.Y} {((SplinePointType1)Data.Points[pointIndex]).Position.Z}");
                        break;

                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType3":
                        obj.WriteLine($"v {((SplinePointType3)Data.Points[pointIndex]).Position.X} {((SplinePointType3)Data.Points[pointIndex]).Position.Y} {((SplinePointType3)Data.Points[pointIndex]).Position.Z}");
                        break;

                    case "KnuxLib.Engines.Storybook.PathSpline+SplinePointType4":
                        obj.WriteLine($"v {((SplinePointType4)Data.Points[pointIndex]).Position.X} {((SplinePointType4)Data.Points[pointIndex]).Position.Y} {((SplinePointType4)Data.Points[pointIndex]).Position.Z}");
                        break;
                }
            }

            // Write this path's object name.
            obj.WriteLine($"o {Path.GetFileNameWithoutExtension(filepath)}");
            obj.WriteLine($"g {Path.GetFileNameWithoutExtension(filepath)}");

            // Write this path's object.
            obj.Write("l ");
            for (int pointIndex = 0; pointIndex < Data.Points.Count; pointIndex++)
                obj.Write($"{pointIndex + 1} ");

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
