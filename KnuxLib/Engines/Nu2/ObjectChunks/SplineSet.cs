using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    public class SplineSet
    {
        // Classes for this NuObject chunk.
        public class SplineData
        {
            public string Name { get; set; } = "";

            public List<Vector3> Vertices { get; set; } = new();

            public override string ToString() => Name;
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="version">The system version to read this chunk as.</param>
        public static List<SplineData> Read(BinaryReaderEx reader, FormatVersion version)
        {
            // Set up our list of splines.
            List<SplineData> Splines = new();

            // Read the count of splines in this file.
            uint splineCount = reader.ReadUInt32();

            // Skip an unknown value that appears to be the chunk's size but minus 0x10.
            reader.JumpAhead(0x04);

            // Loop through and read each spline.
            for (int splineIndex = 0; splineIndex < splineCount; splineIndex++)
            {
                // Set up a spline entry.
                SplineData spline = new();

                // Read the amount of vertices in this spline.
                uint vertexCount = reader.ReadUInt32();

                // Get this spline's name.
                spline.Name = Helpers.FindNu2SceneName(reader, reader.ReadUInt32(), version);

                // Loop through and read each of this spline's vertices.
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                    spline.Vertices.Add(reader.ReadVector3());

                // Save this spline.
                Splines.Add(spline);
            }

            // Return the list of splines read from the file.
            return Splines;
        }
    }
}
