using static KnuxLib.Engines.Nu2.Scene;

namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: What does SPEC stand for? Special?
    // TODO: What does the data in this chunk actually do?
    // TODO: This data is different in the PlayStation 2 version, figure out how that version is structured.
    public class SPECSet
    {
        public class SPECData
        {
            /// <summary>
            /// An unknown 4x4Matrix.
            /// TODO: What is this?
            /// </summary>
            public Matrix4x4 UnknownMatrix4x4_1 { get; set; }

            /// <summary>
            /// The index of the geometry entry in the file's geometry set that this entry should use.
            /// </summary>
            public uint ModelIndex { get; set; }

            /// <summary>
            /// The name of this entry.
            /// </summary>
            public string Name { get; set; } = "";

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
            /// A copy of this entry's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();

            public override string ToString() => Name;
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static List<SPECData> Read(BinaryReaderEx reader, FormatVersion version)
        {
            // Set up our list of data.
            List<SPECData> Data = new();

            // Read the count of entries in this file.
            uint entryCount = reader.ReadUInt32();

            // Loop through and read each entry.
            for (int i = 0; i < entryCount; i++)
            {
                // Set up an entry.
                SPECData entry = new();

                // Read this entry's unknown matrix.
                entry.UnknownMatrix4x4_1 = reader.ReadMatrix();

                // Read this entry's model index.
                entry.ModelIndex = reader.ReadUInt32();

                // Read the node name table offset for this data's name.
                entry.Name = Helpers.FindNu2SceneName(reader, reader.ReadUInt32(), version);

                // Read this entry's first unknown value.
                entry.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this entry's second unknown value.
                entry.UnknownUInt32_2 = reader.ReadUInt32();

                // Decompose this entry's matrix into a human readable format.
                entry.Transform.Process(entry.UnknownMatrix4x4_1);

                // Save this entry.
                Data.Add(entry);

            }

            // Return the list of unknown data read from this file.
            return Data;
        }
    }
}
