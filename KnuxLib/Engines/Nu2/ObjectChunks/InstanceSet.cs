namespace KnuxLib.Engines.Nu2.ObjectChunks
{
    // TODO: Work out all the unknown values.
    public class InstanceSet
    {
        // Classes for this NuObject chunk.
        public class InstanceData
        {
            /// <summary>
            /// A list of the instances in this file.
            /// </summary>
            public List<Instance> Instances { get; set; } = new();

            /// <summary>
            /// A(n optional) list of the unknown instances in this file.
            /// </summary>
            public List<UnknownInstance>? UnknownInstances { get; set; }
        }

        public class Instance
        {
            /// <summary>
            /// The matrix that makes up this instance's transform data.
            /// </summary>
            public Matrix4x4 Matrix { get; set; }

            /// <summary>
            /// The index of the geometry entry in the file's geometry set that this instance should use.
            /// </summary>
            public uint GeometryIndex { get; set; }

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
            /// A copy of this instance's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();
        }

        public class UnknownInstance
        {
            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_2 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_3 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_4 { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// </summary>
            public float UnknownFloat_5 { get; set; }
        }

        /// <summary>
        /// Reads this NuObject chunk and returns a list of the data within.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static InstanceData Read(BinaryReaderEx reader)
        {
            // Set up our instance data.
            InstanceData Instances = new();

            // Read the amount of instances in this file.
            uint instanceCount = reader.ReadUInt32();

            // Loop through and read each instance.
            for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
            {
                // Set up an instance entry.
                Instance instance = new();

                // Read this instance's matrix.
                instance.Matrix = reader.ReadMatrix();

                // Read this instance's geometry index.
                instance.GeometryIndex = reader.ReadUInt32();
                
                // Read this instance's first unknown value.
                instance.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this instance's second unknown value.
                instance.UnknownUInt32_2 = reader.ReadUInt32();

                // Skip an unknown value of 0x00.
                reader.JumpAhead(0x04);

                // Decompose this instance's matrix into a human readable format.
                instance.Transform.Process(instance.Matrix);

                // Save this instance.
                Instances.Instances.Add(instance);
            }

            // Reread the instance count for the table.
            instanceCount = reader.ReadUInt32();

            // Only read the second table if it actually exists.
            if (instanceCount != 0)
            {
                // Initialise the list of unknown instances.
                Instances.UnknownInstances = new();

                // Loop through and read each unknown instance.
                for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
                {
                    // Set up an instance entry.
                    UnknownInstance instance = new();

                    // Skip 0x40 bytes that are always 0x00.
                    reader.JumpAhead(0x40);

                    // Read this instance's first unknown value.
                    instance.UnknownFloat_1 = reader.ReadSingle();

                    // Read this instance's second unknown value.
                    instance.UnknownFloat_2 = reader.ReadSingle();

                    // Read this instance's third unknown value.
                    instance.UnknownFloat_3 = reader.ReadSingle();

                    // Skip an unknown value of 0x3F800000 (1 as a floating point number).
                    reader.JumpAhead(0x04);

                    // Read this instance's fourth unknown value.
                    instance.UnknownFloat_4 = reader.ReadSingle();
                    
                    // Skip two unknown values of 0x00.
                    reader.JumpAhead(0x8);

                    // Read this instance's fifth unknown value.
                    instance.UnknownFloat_5 = reader.ReadSingle();

                    // Save this instance.
                    Instances.UnknownInstances.Add(instance);
                }
            }

            // Return the instance data read from the file.
            return Instances;
        }
    }
}
