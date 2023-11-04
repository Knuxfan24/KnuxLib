using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Storybook
{
    // TODO: Figure out what the values for the points actually are and what they do, not so sure on some of the Vector3s.
    // TODO: Add proper exporting for this format.
    // TODO: Add a way to import this format.
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
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// This path's type.
            /// </summary>
            public PathType Type { get; set; }

            /// <summary>
            /// This path's name, only present in Black Knight files.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// The points that make up this path.
            /// </summary>
            public List<object> Points { get; set; } = new();
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
            /// TODO: What is this?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this? Seems to control how far away from the path the player can go.
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

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
            /// An unknown Vector3.
            /// TODO: What is this?
            /// </summary>
            public Vector3 UnknownVector3_3 { get; set; }
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
            Data.UnknownFloat_1 = reader.ReadSingle();

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
                            UnknownVector3_1 = reader.ReadVector3(),
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
                            UnknownVector3_3 = reader.ReadVector3()
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
            writer.Write(Data.UnknownFloat_1);

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

                        // Write this point's unknown Vector3 value.
                        writer.Write(((SplinePointType3)Data.Points[pointIndex]).UnknownVector3_1);

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

                        // Write this point's third unknown Vector3 value.
                        writer.Write(((SplinePointType4)Data.Points[pointIndex]).UnknownVector3_3);
                        break;
                }
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
