namespace KnuxLib.Engines.WorldAdventureWii
{
    // TODO: Add a way to export this format to an OBJ or other approriate model format.
    // TODO: Add a way to import this format.
    public class PathSpline : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline() { }
        public PathSpline(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath).Replace(".path", "")}.worldadventurewii.pathspline.json", Data);
        }

        // Classes for this format.
        public class SplinePath
        {
            /// <summary>
            /// The name of this path.
            /// </summary>
            public string Name { get; set; } = "";

            /// <summary>
            /// This path's position in 3D space.
            /// TODO: Confirm, just assumed.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This path's rotation in 3D space.
            /// TODO: Confirm, just assumed.
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// This path's scale.
            /// TODO: Confirm, just assumed.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// A set of unknown integer values, one for each spline set.
            /// TODO: What is this?
            /// </summary>
            public uint[] UnknownUInts_1 { get; set; } = Array.Empty<uint>();

            /// <summary>
            /// The points that make up each spline.
            /// </summary>
            public List<SplinePoint>[] Splines { get; set; } = Array.Empty<List<SplinePoint>>();

            public override string ToString() => Name.ToString();
        }

        public class SplinePoint
        {
            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this?
            /// TODO: Is this actually a Vector3?
            /// </summary>
            public Vector3 UnknownVector3_3 { get; set; }
        }

        // Actual data presented to the end user.
        public List<SplinePath> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read this file's signature.
            reader.ReadSignature(0x04, "sgnt");

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Read this file's path count.
            uint pathCount = reader.ReadUInt32();

            // Read the offset to this file's path name table.
            uint pathNameTableOffset = reader.ReadUInt32();

            // Read the offset to this file's path data table.
            uint pathDataTableOffset = reader.ReadUInt32();

            // Jump to the path name table (should already be here but lets play it safe).
            reader.JumpTo(pathNameTableOffset);

            // Read the size of the path name table.
            uint pathNameTableSize = reader.ReadUInt32();

            // Skip an unknown value that is always the same as pathCount.
            reader.JumpAhead(0x04);

            // Loop through each path in this file.
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                // Set up a new path entry.
                SplinePath path = new();

                // Skip an unknown, sequential value.
                reader.JumpAhead(0x02);
                
                // Read this path's name.
                path.Name = reader.ReadNullPaddedString(reader.ReadUInt16());
                
                // Save this path entry.
                Data.Add(path);
            }

            // Jump to the path data table (should already be here but lets play it safe).
            reader.JumpTo(pathDataTableOffset);

            // Read the size of the path data table.
            uint pathDataTableSize = reader.ReadUInt32();

            // Skip an unknown value that is always the same as pathCount.
            reader.JumpAhead(0x04);

            // Loop through each path in this file.
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                // Read this path's size.
                uint pathSize = reader.ReadUInt32();

                // Read this path's position value.
                Data[pathIndex].Position = reader.ReadVector3();

                // Read this path's rotation value.
                Data[pathIndex].Rotation = reader.ReadQuaternion();

                // Read this path's scale value.
                Data[pathIndex].Scale = reader.ReadVector3();

                // Read this path's unknown floating point value.
                Data[pathIndex].UnknownFloat_1 = reader.ReadSingle();

                // Read this path's spline table size.
                uint splineTableSize = reader.ReadUInt32();

                // Read this path's spline count.
                uint splineCount = reader.ReadUInt32();

                // Set up this path's unknown integer value count.
                Data[pathIndex].UnknownUInts_1 = new uint[splineCount];

                // Set up this path's spline count.
                Data[pathIndex].Splines = new List<SplinePoint>[splineCount];

                // Loop through each spline in this path.
                for (int splineIndex = 0; splineIndex < splineCount; splineIndex++)
                {
                    // Initialise the spline entry.
                    Data[pathIndex].Splines[splineIndex] = new();

                    // Read this spline's size.
                    uint splineSize = reader.ReadUInt32();

                    // Read this spline's unknown integer value.
                    Data[pathIndex].UnknownUInts_1[splineIndex] = reader.ReadUInt32();

                    // Read this spline's point count.
                    uint pointCount = reader.ReadUInt32();

                    // Loop through each spline point in this spline.
                    for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                    {
                        // Set up a new spline point entry.
                        SplinePoint spline = new();

                        // Read this spline point's unknown integer value.
                        spline.UnknownUInt32_1 = reader.ReadUInt32();

                        // Read this spline point's first unknown Vector3.
                        spline.UnknownVector3_1 = reader.ReadVector3();

                        // Read this spline point's second unknown Vector3.
                        spline.UnknownVector3_2 = reader.ReadVector3();

                        // Read this spline point's third unknown Vector3.
                        spline.UnknownVector3_3 = reader.ReadVector3();

                        // Save this spline point.
                        Data[pathIndex].Splines[splineIndex].Add(spline);
                    }
                }
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
            BinaryWriterEx writer = new(File.Create(filepath), true);

            // Write this file's signature.
            writer.WriteNullPaddedString("sgnt", 0x04);

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write this file's path count.
            writer.Write(Data.Count);

            // Add an offset for this file's path name table.
            writer.AddOffset("PathNameTableOffset");

            // Add an offset for this file's path data table.
            writer.AddOffset("PathDataTableOffset");

            // Fill in the offset for this file's path name table.
            writer.FillOffset("PathNameTableOffset");

            // Save the position of the start of the path name table.
            long PathNameTableStartPosition = writer.BaseStream.Position;

            // Write a placeholder for this file's path name table size.
            writer.Write("SIZE");

            // Write this file's path count.
            writer.Write(Data.Count);

            // Loop through each path.
            for (ushort dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write the index of this path.
                writer.Write(dataIndex);

                // Calculate this path's name length to the nearest 0x04.
                ushort stringSize = (ushort)Data[dataIndex].Name.Length;
                while (stringSize % 0x04 != 0)
                    stringSize++;

                // Write this path's name length.
                writer.Write(stringSize);

                // Write this path's name, padded to the previously calculated value.
                writer.WriteNullPaddedString(Data[dataIndex].Name, stringSize);
            }

            // Save the position of the end of the path name table.
            long PathNameTableEndPosition = writer.BaseStream.Position;

            // Fill in the path name table's size.
            writer.BaseStream.Position = PathNameTableStartPosition;
            writer.Write((uint)(PathNameTableEndPosition - PathNameTableStartPosition));
            writer.BaseStream.Position = PathNameTableEndPosition;

            // Fill in the offset for this file's path name table.
            writer.FillOffset("PathDataTableOffset");

            // Save the position of the start of the path data table.
            long PathDataTableStartPosition = writer.BaseStream.Position;

            // Write a placeholder for this file's path data table size.
            writer.Write("SIZE");

            // Write this file's path count.
            writer.Write(Data.Count);

            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Save the position of the start of this path.
                long PathStartPosition = writer.BaseStream.Position;

                // Write a placeholder for this path's size.
                writer.Write("SIZE");

                // Write this path's position.
                writer.Write(Data[dataIndex].Position);

                // Write this path's rotation.
                writer.Write(Data[dataIndex].Rotation);

                // Write this path's scale.
                writer.Write(Data[dataIndex].Scale);

                // Write this path's unknown floating point value.
                writer.Write(Data[dataIndex].UnknownFloat_1);

                // Save the position of the start of this path's spline table.
                long SplineTableStartPosition = writer.BaseStream.Position;

                // Write a placeholder for this path's spline table size.
                writer.Write("SIZE");

                // Write the amount of splines in this path.
                writer.Write(Data[dataIndex].Splines.Length);

                // Loop through each spline in this path.
                for (int splineIndex = 0; splineIndex < Data[dataIndex].Splines.Length; splineIndex++)
                {
                    // Save the position of the start of this spline.
                    long SplineStartPosition = writer.BaseStream.Position;

                    // Write a placeholder for this spline size.
                    writer.Write("SIZE");

                    // Write this spline's unknown integer value.
                    writer.Write(Data[dataIndex].UnknownUInts_1[splineIndex]);

                    // Write this spline's point count.
                    writer.Write(Data[dataIndex].Splines[splineIndex].Count);

                    // Loop through each point in this spline.
                    for (int pointIndex = 0; pointIndex < Data[dataIndex].Splines[splineIndex].Count; pointIndex++)
                    {
                        // Write this point's unknown integer value.
                        writer.Write(Data[dataIndex].Splines[splineIndex][pointIndex].UnknownUInt32_1);

                        // Write this point's first unknown Vector3.
                        writer.Write(Data[dataIndex].Splines[splineIndex][pointIndex].UnknownVector3_1);

                        // Write this point's second unknown Vector3.
                        writer.Write(Data[dataIndex].Splines[splineIndex][pointIndex].UnknownVector3_2);

                        // Write this point's third unknown Vector3.
                        writer.Write(Data[dataIndex].Splines[splineIndex][pointIndex].UnknownVector3_3);
                    }

                    // Save the position of the end of this spline.
                    long SplineEndPosition = writer.BaseStream.Position;

                    // Fill in this spline's size.
                    writer.BaseStream.Position = SplineStartPosition;
                    writer.Write((uint)(SplineEndPosition - SplineStartPosition));
                    writer.BaseStream.Position = SplineEndPosition;
                }

                // Save the position of the end of this path.
                long PathEndPosition = writer.BaseStream.Position;

                // Fill in this path's size.
                writer.BaseStream.Position = PathStartPosition;
                writer.Write((uint)(PathEndPosition - PathStartPosition));

                // Fill in this path's spline table's size.
                writer.BaseStream.Position = SplineTableStartPosition;
                writer.Write((uint)(PathEndPosition - SplineTableStartPosition));
                writer.BaseStream.Position = PathEndPosition;
            }

            // Fill in the path data table's size.
            writer.BaseStream.Position = PathDataTableStartPosition;
            writer.Write((uint)(writer.BaseStream.Length - PathDataTableStartPosition));

            // Fill in this file's size.
            writer.BaseStream.Position = 0x04;
            writer.Write((uint)(writer.BaseStream.Length));

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
