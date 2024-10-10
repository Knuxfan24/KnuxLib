using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.SonicStorybook
{
    // TODO: Figure out what the values for the points actually are and what they do.
    // TODO: Format importing.
    public class PathSpline : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public PathSpline() { }
        public PathSpline(string filepath, FormatVersion version = FormatVersion.SecretRings, bool export = false)
        {
            // Check if the input file is an OBJ.
            if (StringHelpers.GetExtension(filepath) == ".obj")
            {
                // Import this OBJ.
                //Data = ImportOBJ(filepath, version);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.PTH", version);
            }

            // Check if the input file isn't an OBJ.
            else
            {
                // Load this file.
                Load(filepath, version);

                // If the export flag is set, then export this format.
                if (export)
                    ExportOBJ($@"{StringHelpers.GetExtension(filepath, true)}.obj");
            }
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
            /// TODO: Should we really be storing this value rather than just calculating it on the fly?
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
            public object[] Points { get; set; } = [];

            /// <summary>
            /// Displays this path's name (if it has one) in the debugger.
            /// </summary>
            public override string ToString() => Name;
        }

        // TODO: Is this one right? There's only six examples of them in a Secret Rings test stage and they look really off.
        public class SplinePointType1
        {
            /// <summary>
            /// The distance between this point and the next.
            /// TODO: Should we really be storing this value rather than just calculating it on the fly?
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Initialises this spline point with default data.
            /// </summary>
            public SplinePointType1() { }

            /// <summary>
            /// Initialises this spline point with the provided data.
            /// </summary>
            public SplinePointType1(float distance, Vector3 position)
            {
                Distance = distance;
                Position = position;
            }

            /// <summary>
            /// Initialises this spline point by reading its data from a BinaryReader.
            /// </summary>
            public SplinePointType1(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this spline point.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                reader.CheckValue(0x00);
                Distance = reader.ReadSingle();
                Position = reader.ReadVector3();
            }

            /// <summary>
            /// Writes the data for this spline point.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.WriteNulls(0x04);
                writer.Write(Distance);
                writer.Write(Position);
            }
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
            /// TODO: Should we really be storing this value rather than just calculating it on the fly?
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// How far the player can deviate left and right from this point's center.
            /// </summary>
            public float Deviation { get; set; }

            /// <summary>
            /// This point's surface type.
            /// TODO: Figure out the possible values for this.
            /// </summary>
            public uint Surface { get; set; }

            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Initialises this spline point with default data.
            /// </summary>
            public SplinePointType3() { }

            /// <summary>
            /// Initialises this spline point with the provided data.
            /// </summary>
            public SplinePointType3(uint unknownUInt32_1, float distance, float deviation, uint surface, Vector3 position)
            {
                UnknownUInt32_1 = unknownUInt32_1;
                Distance = distance;
                Deviation = deviation;
                Surface = surface;
                Position = position;
            }

            /// <summary>
            /// Initialises this spline point by reading its data from a BinaryReader.
            /// </summary>
            public SplinePointType3(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this spline point.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                UnknownUInt32_1 = reader.ReadUInt32();
                Distance = reader.ReadSingle();
                Deviation = reader.ReadSingle();
                Surface = reader.ReadUInt32();
                Position = reader.ReadVector3();
            }

            /// <summary>
            /// Writes the data for this spline point.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.Write(UnknownUInt32_1);
                writer.Write(Distance);
                writer.Write(Deviation);
                writer.Write(Surface);
                writer.Write(Position);
            }
        }

        public class SplinePointType4
        {
            /// <summary>
            /// This point's in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// This point's forward vector (using a Z-UP schema).
            /// TODO: Should we really be storing this value rather than just calculating it on the fly?
            /// </summary>
            public Vector3 ForwardVector { get; set; }

            /// <summary>
            /// The distance between this point and the next.
            /// TODO: Should we really be storing this value rather than just calculating it on the fly?
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// How far the player can deviate left and right from this point's center.
            /// </summary>
            public float Deviation { get; set; }

            /// <summary>
            /// This point's surface type.
            /// TODO: Figure out the possible values for this.
            /// </summary>
            public uint Surface { get; set; }

            /// <summary>
            /// Initialises this spline point with default data.
            /// </summary>
            public SplinePointType4() { }

            /// <summary>
            /// Initialises this spline point with the provided data.
            /// </summary>
            public SplinePointType4(Vector3 position, Vector3 forwardVector, float distance, float deviation, uint surface)
            {
                Position = position;
                ForwardVector = forwardVector;
                Distance = distance;
                Deviation = deviation;
                Surface = surface;
            }

            /// <summary>
            /// Initialises this spline point by reading its data from a BinaryReader.
            /// </summary>
            public SplinePointType4(ExtendedBinaryReader reader) => Read(reader);

            /// <summary>
            /// Reads the data for this spline point.
            /// </summary>
            public void Read(ExtendedBinaryReader reader)
            {
                Position = reader.ReadVector3();
                ForwardVector = reader.ReadVector3();
                reader.CheckValue(0x00, 0x03);
                Distance = reader.ReadSingle();
                Deviation = reader.ReadSingle();
                Surface = reader.ReadUInt32();
            }

            /// <summary>
            /// Writes the data for this spline point.
            /// </summary>
            public void Write(ExtendedBinaryWriter writer)
            {
                writer.Write(Position);
                writer.Write(ForwardVector);
                writer.WriteNulls(0x0C);
                writer.Write(Distance);
                writer.Write(Deviation);
                writer.Write(Surface);
            }
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
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath));

            // Read the value that indicates the type of points this spline uses.
            ushort pointType = reader.ReadUInt16();

            // Initialise the points array with the correct type.
            switch (pointType)
            {
                case 1: Data.Points = new SplinePointType1[reader.ReadUInt16()]; break;
                case 3: Data.Points = new SplinePointType3[reader.ReadUInt16()]; break;
                case 4: Data.Points = new SplinePointType4[reader.ReadUInt16()]; break;
                default: throw new Exception($"Got unhandled point type of {pointType}!");
            }

            // Read this path's total distance value.
            Data.TotalDistance = reader.ReadSingle();

            // Read the offset to this file's path table (should always be 0x10?).
            uint pathTableOffset = reader.ReadUInt32();

            // Read this path's type.
            Data.Type = (PathType)reader.ReadUInt32();

            // Jump to the file's path table (should already be at this position but just to be safe).
            reader.JumpTo(pathTableOffset);

            // If this is a Black Knight file, then read the path name.
            if (version == FormatVersion.BlackKnight)
                Data.Name = reader.ReadNullPaddedString(0x20);

            // Loop through each point in this path.
            for (int pointIndex = 0; pointIndex < Data.Points.Length; pointIndex++)
            {
                // Read this point depending on this path's point type.
                switch (pointType)
                {
                    case 1: Data.Points[pointIndex] = new SplinePointType1(reader); break;
                    case 3: Data.Points[pointIndex] = new SplinePointType3(reader); break;
                    case 4: Data.Points[pointIndex] = new SplinePointType4(reader); break;
                    default: throw new Exception($"Got unhandled point type of {pointType}!");
                }
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The game version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.SecretRings)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath));

            // Write this path's point type.
            if (Data.Points.GetType() == typeof(SplinePointType1[])) writer.Write((ushort)1);
            if (Data.Points.GetType() == typeof(SplinePointType3[])) writer.Write((ushort)3);
            if (Data.Points.GetType() == typeof(SplinePointType4[])) writer.Write((ushort)4);

            // Write the count of points in this path.
            writer.Write((ushort)Data.Points.Length);

            // Write the total distance of this path.
            writer.Write(Data.TotalDistance);

            // Add an offset for this path's table.
            writer.AddOffset("PathTableOffset");

            // Write this path's type.
            writer.Write((uint)Data.Type);

            // Fill in the offset to this path's table.
            writer.FillInOffset("PathTableOffset");

            // If we're saving a Black Knight path, then write its name.
            // TODO: Maybe fill this in from the file name if it doesn't have one?
            if (version == FormatVersion.BlackKnight)
                writer.WriteNullPaddedString(Data.Name, 0x20);

            // Loop through and write each point's data depending on the type.
            for (int pointIndex = 0; pointIndex < Data.Points.Length; pointIndex++)
            {
                if (Data.Points.GetType() == typeof(SplinePointType1[])) ((SplinePointType1)Data.Points[pointIndex]).Write(writer);
                if (Data.Points.GetType() == typeof(SplinePointType3[])) ((SplinePointType3)Data.Points[pointIndex]).Write(writer);
                if (Data.Points.GetType() == typeof(SplinePointType4[])) ((SplinePointType4)Data.Points[pointIndex]).Write(writer);
            }

            // Close our BinaryWriter.
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

            // Write a comment that we can use on the import function (if the user wants to reimport this OBJ for some reason).
            obj.WriteLine("# KnuxLib Sonic Storybook Engine Path Spline OBJ Export");

            // Write a comment showing the path type.
            obj.WriteLine($"# Path Type: {Data.Type}");

            // Loop through and write each point's position.
            for (int pointIndex = 0; pointIndex < Data.Points.Length; pointIndex++)
            {
                if (Data.Points.GetType() == typeof(SplinePointType1[])) obj.WriteLine($"v {((SplinePointType1)Data.Points[pointIndex]).Position.X} {((SplinePointType1)Data.Points[pointIndex]).Position.Y} {((SplinePointType1)Data.Points[pointIndex]).Position.Z}");
                if (Data.Points.GetType() == typeof(SplinePointType3[])) obj.WriteLine($"v {((SplinePointType3)Data.Points[pointIndex]).Position.X} {((SplinePointType3)Data.Points[pointIndex]).Position.Y} {((SplinePointType3)Data.Points[pointIndex]).Position.Z}");
                if (Data.Points.GetType() == typeof(SplinePointType4[])) obj.WriteLine($"v {((SplinePointType4)Data.Points[pointIndex]).Position.X} {((SplinePointType4)Data.Points[pointIndex]).Position.Y} {((SplinePointType4)Data.Points[pointIndex]).Position.Z}");
            }

            // Write this path's object name.
            obj.WriteLine($"o {Path.GetFileNameWithoutExtension(filepath)}");
            obj.WriteLine($"g {Path.GetFileNameWithoutExtension(filepath)}");

            // Write this path's object.
            obj.Write("l ");
            for (int pointIndex = 0; pointIndex < Data.Points.Length; pointIndex++)
                obj.Write($"{pointIndex + 1} ");

            // Close this StreamWriter.
            obj.Close();
        }
    }
}
