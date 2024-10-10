namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Are the Unleashed and Generations terrain-instanceinfo files the same?
    // As the SCHG page (https://info.sonicretro.org/SCHG:Sonic_Generations#.instanceinfo) marks the root node type as "likely 5"?
    public class InstanceInfo : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public InstanceInfo() { }
        public InstanceInfo(string filepath, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".hedgehog.instanceinfo.json";

            // Check if the input path is a directory rather than a file.
            if (Directory.Exists(filepath))
            {
                // If so, then convert the terrain-instanceinfo and terrain-models in the directory to a point cloud set.
                ConvertDirectoryToPointCloud(filepath);

                // Stop running any more code for this format.
                return;
            }

            // Check if the input file is this format's JSON.
            if (StringHelpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<FormatData>(filepath);

                //If the export flag is set, then save this format.
                if (export)
                    Save($@"{StringHelpers.GetExtension(filepath, true)}.terrain-instanceinfo");
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{StringHelpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The name of the model used by this instance.
            /// </summary>
            public string ModelName { get; set; } = "";

            /// <summary>
            /// The name of this instance.
            /// </summary>
            public string InstanceName { get; set; } = "";

            /// <summary>
            /// The matrix that makes up this instance's transform data.
            /// </summary>
            public Matrix4x4 Matrix { get; set; }

            /// <summary>
            /// A copy of this instance's matrix decomposed into a human readable format.
            /// </summary>
            public DecomposedMatrix Transform { get; set; } = new();

            /// <summary>
            /// Displays this instance's name in the debugger.
            /// </summary>
            public override string ToString() => InstanceName;
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), true);

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Skip the root node type, as it's always 0.
            reader.CheckValue(0x00);

            // Read the root node size.
            uint rootNodeSize = reader.ReadUInt32();

            // Read the offset to the root node.
            uint rootNodeOffset = reader.ReadUInt32();

            // Read the offset to the footer.
            uint footerOffset = reader.ReadUInt32();

            // Skip the file end offset, as it's always 0.
            reader.CheckValue(0x00);

            // Set the reader's offset to the value in rootNodeOffset.
            reader.Offset = rootNodeOffset;

            // Jump to rootNodeOffset.
            reader.JumpTo(rootNodeOffset);

            // Read this instance's model name.
            Data.ModelName = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

            // Read the offset to this instance's matrix.
            uint matrixOffset = reader.ReadUInt32();

            // Read this instance's name.
            Data.InstanceName = StringHelpers.ReadNullTerminatedStringTableEntry(reader, 0x04);

            // Jump to this instance's matrix offset.
            reader.JumpTo(matrixOffset, false);

            // Read this instance's transposed matrix.
            Data.Matrix = Matrix4x4.Transpose(reader.ReadMatrix());

            // Decompose this instance's matrix into a human readable format.
            Data.Transform = new(Data.Matrix);

            // We skip the file's footer, as it's always the same sixteen bytes.

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Create this file through a BinaryWriter, setting the file offset to 0x18.
            ExtendedBinaryWriter writer = new(File.Create(filepath), true) { Offset = 0x18 };

            // Write a placeholder for this file's size.
            writer.Write("SIZE");

            // Write the root node type constant.
            writer.Write(0x00);

            // Write a placeholder for the root node's size.
            writer.Write("SIZE");

            // Add an offset for the root node.
            writer.AddOffset("RootNodeOffset");

            // Add an offset for the footer.
            writer.AddOffset("FooterOffset");

            // Write the file end offset constant.
            writer.Write(0x00);

            // Fill in the root node offset.
            writer.FillInOffset("RootNodeOffset");

            // Add an offset for the model name.
            writer.AddOffset("ModelNameOffset");

            // Add an offset for the matrix.
            writer.AddOffset("MatrixOffset");

            // Add an offset for the instance name.
            writer.AddOffset("InstanceNameOffset");

            // Fill in the model name's offset.
            writer.FillInOffset("ModelNameOffset", false);

            // Write the model name.
            writer.WriteNullTerminatedString(Data.ModelName);

            // Fill in the matrix's offset.
            writer.FillInOffset("MatrixOffset", false);

            // Write the transposed matrix.
            writer.Write(Matrix4x4.Transpose(Data.Matrix));

            // Fill in the instance name's offset.
            writer.FillInOffset("InstanceNameOffset", false);

            // Write the instance name.
            writer.WriteNullTerminatedString(Data.ModelName);

            // Calculate the root node size value.
            uint rootNodeSize = (uint)(writer.BaseStream.Length - 0x18);

            // Fill in the offset for the footer.
            writer.FillInOffset("FooterOffset");

            // Write the constant values for the footer.
            writer.Write(0x03);
            writer.Write(0x00);
            writer.Write(0x04);
            writer.Write(0x08);

            // Write the file size.
            writer.BaseStream.Position = 0x00;
            writer.Write((uint)writer.BaseStream.Length);

            // Write the root node size.
            writer.BaseStream.Position = 0x08;
            writer.Write(rootNodeSize);

            // Close our BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Converts a folder of terrain-instanceinfo and terrain-models into a Sonic Frontiers pcmodel.
        /// </summary>
        /// <param name="directory">The directory to get terrain-instanceinfo and terrain-model files from.</param>
        public static void ConvertDirectoryToPointCloud(string directory)
        {
            // Set up a list of used assets. Frontiers requires every model to have a point cloud entry, so we need to create some.
            List<string> usedAssets = [];

            // Set up a list of point cloud instances.
            List<PointCloud.Instance> pointCloudInstances = [];

            // Set up the point cloud file to save to.
            PointCloud pcmodel = new();

            // Loop through all the terrain-instanceinfo files in the given directory.
            foreach (string instanceInfoFile in Directory.GetFiles(directory, "*.terrain-instanceinfo"))
            {
                // Load this terrain-instanceinfo file.
                InstanceInfo instanceInfo = new(instanceInfoFile);

                // If the model this instance references isn't already in the usedAssets list, then add it.
                if (!usedAssets.Contains(instanceInfo.Data.ModelName))
                    usedAssets.Add(instanceInfo.Data.ModelName);

                // Add an entry to the point cloud file for this instance, converting the rotation to radians.
                pointCloudInstances.Add(new(instanceInfo.Data.InstanceName,
                                            instanceInfo.Data.ModelName,
                                            instanceInfo.Data.Transform.Translation,
                                            new((float)(Math.PI / 180) * instanceInfo.Data.Transform.EulerRotation.X, (float)(Math.PI / 180) * instanceInfo.Data.Transform.EulerRotation.Y, (float)(Math.PI / 180) * instanceInfo.Data.Transform.EulerRotation.Z),
                                            1,
                                            instanceInfo.Data.Transform.Scale));
            }

            // Loop through all the terrain-model files in the given directory.
            // If this terrain-model was not referenced by an instance, then add a basic entry for it.
            foreach (string terrainModelFile in Directory.GetFiles(directory, "*.terrain-model"))
                if (!usedAssets.Contains(Path.GetFileNameWithoutExtension(terrainModelFile)))
                    pointCloudInstances.Add(new(Path.GetFileNameWithoutExtension(terrainModelFile),
                                                Path.GetFileNameWithoutExtension(terrainModelFile),
                                                Vector3.One,
                                                Vector3.Zero,
                                                1,
                                                Vector3.One));

            // Set the point cloud's data to our list, converted to an array.
            pcmodel.Data = [.. pointCloudInstances];

            // Save the point cloud file.
            pcmodel.Save($@"{directory}.pcmodel");
        }
    }
}
