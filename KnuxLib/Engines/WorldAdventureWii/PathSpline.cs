namespace KnuxLib.Engines.WorldAdventureWii
{
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
            public string Name { get; set; } = "";

            public Vector3 Position { get; set; }

            public Quaternion Rotation { get; set; }

            public Vector3 Scale { get; set; }

            public float UnknownFloat_1 { get; set; }

            public uint[] UnknownUInts_1 { get; set; } = Array.Empty<uint>();

            public List<SplinePoint>[] Splines { get; set; } = Array.Empty<List<SplinePoint>>();

            public override string ToString() => Name.ToString();
        }

        public class SplinePoint
        {
            public uint UnknownUInt32_1 { get; set; }

            public Vector3 UnknownVector3_1 { get; set; }

            public Vector3 UnknownVector3_2 { get; set; }

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

            reader.ReadSignature(0x04, "sgnt");

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            uint pathCount = reader.ReadUInt32();

            uint pathTableOffset = reader.ReadUInt32();

            uint pathDataTableOffset = reader.ReadUInt32();

            reader.JumpTo(pathTableOffset);

            uint pathTableSize = reader.ReadUInt32();

            // Skip an unknown value that is always the same as pathCount.
            reader.JumpAhead(0x04);

            for (int i = 0; i < pathCount; i++)
            {
                SplinePath path = new();
                // Skip an unknown, sequential value.
                reader.JumpAhead(0x02);
                path.Name = reader.ReadNullPaddedString(reader.ReadUInt16());
                Data.Add(path);
            }

            reader.JumpTo(pathDataTableOffset);

            uint pathDataTableSize = reader.ReadUInt32();

            // Skip an unknown value that is always the same as pathCount.
            reader.JumpAhead(0x04);

            for (int i = 0; i < pathCount; i++)
            {
                uint pathSize = reader.ReadUInt32();
                Data[i].Position = reader.ReadVector3();
                Data[i].Rotation = reader.ReadQuaternion();
                Data[i].Scale = reader.ReadVector3();
                Data[i].UnknownFloat_1 = reader.ReadSingle();

                uint splineTableSize = reader.ReadUInt32();
                uint splineCount = reader.ReadUInt32();
                Data[i].UnknownUInts_1 = new uint[splineCount];
                Data[i].Splines = new List<SplinePoint>[splineCount];

                for (int s = 0; s < splineCount; s++)
                {
                    Data[i].Splines[s] = new();
                    uint splineSize = reader.ReadUInt32();
                    Data[i].UnknownUInts_1[s] = reader.ReadUInt32();
                    uint pointCount = reader.ReadUInt32();

                    for (int p = 0; p < pointCount; p++)
                    {
                        SplinePoint spline = new();
                        spline.UnknownUInt32_1 = reader.ReadUInt32();
                        spline.UnknownVector3_1 = reader.ReadVector3();
                        spline.UnknownVector3_2 = reader.ReadVector3();
                        spline.UnknownVector3_3 = reader.ReadVector3();
                        Data[i].Splines[s].Add(spline);
                    }
                }
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }
    }
}
