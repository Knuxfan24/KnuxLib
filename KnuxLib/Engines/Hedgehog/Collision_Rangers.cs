using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // TODO: Read the bounding volume hierarchy for the meshes.
    // TODO: Figure out what the unknown tag in the meshes controls in terms of collision behaviour.
    // TODO: Figure out what the unknown data chunk is for.
    // TODO: Format saving.
    // TODO: Mesh importing.
    public class Collision_Rangers : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision_Rangers() { }
        public Collision_Rangers(string filepath, bool export = false)
        {
            Load(filepath);
            
            if (export)
                ExportOBJ($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.obj");
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Material : byte
        {
            None = 0x00,
            Stone = 0x01,
            Earth = 0x02,
            Wood = 0x03,
            Grass = 0x04,
            Iron = 0x05,
            Sand = 0x06,
            Lava = 0x07,
            Glass = 0x08,
            Snow = 0x09,
            NoEntry = 0x0A,
            Ice = 0x0B,
            Water = 0x0C,
            Sea = 0x0D,
            Damage = 0x0E,
            Dead = 0x0F,
            Flower0 = 0x10,
            Flower1 = 0x11,
            Flower2 = 0x12,
            Air = 0x13,
            DeadLeaves = 0x14,
            WireMesh = 0x15,
            DeadAnyDirection = 0x16,
            DamageThrough = 0x17,
            DryGrass = 0x18,
            Relic = 0x19,
            Giant = 0x1A,
            Gravel = 0x1B,
            HUDWater = 0x1C,
            Sand2 = 0x1D,
            Sand3 = 0x1E
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LayerType : byte
        {
            None = 0x00,
            Solid = 0x01,
            Liquid = 0x02,
            Through = 0x03,
            Camera = 0x04,
            Solid_OneWay = 0x05,
            Solid_Through = 0x06,
            Solid_Tiny = 0x07,
            Solid_Detail = 0x08,
            Leaf = 0x09,
            Land = 0x0A,
            Ray_Block = 0x0B,
            Event = 0x0C,
            Reserved13 = 0x0D, // ?
            Reserved14 = 0x0E, // ?
            Player = 0x0F,
            Enemy = 0x10,
            Enemy_Body = 0x11,
            Gimmick = 0x12,
            Dynamics = 0x13,
            Ring = 0x14,
            Character_Control = 0x15,
            Player_Only = 0x16,
            Dynamics_Through = 0x17,
            Enemy_Only = 0x18,
            Sensor_Player = 0x19,
            Sensor_Ring = 0x1A,
            Sensor_Gimmick = 0x1B,
            Sensor_Land = 0x1C,
            Sensor_All = 0x1D,
            Reserved30 = 0x1E, // ?
            Reserved31 = 0x1F // ?
        }

        public class FormatData
        {
            /// <summary>
            /// The meshes that make up this collision file.
            /// </summary>
            public Mesh[]? Meshes { get; set; }

            /// <summary>
            /// An unknown chunk of data.
            /// TODO: What is this?
            /// </summary>
            public UnknownData[]? UnknownData { get; set; }
        }

        public class Mesh
        {
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
            /// The vertices that make up this mesh.
            /// </summary>
            public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();

            /// <summary>
            /// The faces that make up this mesh, some meshes just don't have any for some reason?
            /// </summary>
            public Face[]? Faces { get; set; }

            /// <summary>
            /// This mesh's layer, only used if there aren't any faces.
            /// </summary>
            public LayerType? Layer { get; set; }

            /// <summary>
            /// An unknown byte, only used if there aren't any faces.
            /// TODO: What is this?
            /// </summary>
            public byte? UnknownCollisionValue { get; set; }

            /// <summary>
            /// This mesh's material type, only used if there aren't any faces.
            /// </summary>
            public Material? Material { get; set; }
        }

        public class Face
        {
            /// <summary>
            /// The index of this face's first vertex.
            /// </summary>
            public uint IndexA { get; set; }

            /// <summary>
            /// The index of this face's second vertex.
            /// </summary>
            public uint IndexB { get; set; }

            /// <summary>
            /// The index of this face's third vertex.
            /// </summary>
            public uint IndexC { get; set; }

            /// <summary>
            /// This face's layer.
            /// </summary>
            public LayerType Layer { get; set; }

            /// <summary>
            /// An unknown byte.
            /// TODO: What is this?
            /// </summary>
            public byte UnknownCollisionValue { get; set; }

            /// <summary>
            /// This face's material.
            /// </summary>
            public Material Material { get; set; }
        }

        public class UnknownData
        {
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
            /// An unknown Vector3.
            /// TODO: What is this? Position of some sort?
            /// </summary>
            public Vector3 UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown Quaternion.
            /// TODO: What is this? Rotation of some sort?
            /// </summary>
            public Quaternion UnknownQuaternion { get; set; }

            /// <summary>
            /// An unknown Vector3.
            /// TODO: What is this? Scale of some sort?
            /// </summary>
            public Vector3 UnknownVector3_2 { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Skip two unknown values of 0 and 3, likely a version indicator?
            reader.JumpAhead(0x08);

            // Read the offset to this file's mesh table.
            long meshesOffset = reader.ReadInt64();

            // Read the amount of meshes in this file.
            uint meshCount = reader.ReadUInt32();

            // Read the amount of unknown data blocks in this file.
            // TODO: What is this data?
            uint unknownCount_1 = reader.ReadUInt32();

            // Read the offset to this file's unknown data.
            // TODO: What is this data?
            long unknownOffset_1 = reader.ReadInt64();

            // Create an array of meshes if this file has any.
            if (meshCount != 0)
                Data.Meshes = new Mesh[meshCount];

            // Create an array of the unknown data chunks if this file has any.
            if (unknownCount_1 != 0)
                Data.UnknownData = new UnknownData[unknownCount_1];

            // Jump to the mesh table.
            reader.JumpTo(meshesOffset, false);

            // Loop through each mesh.
            for (ulong meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                // Create a new mesh.
                Mesh mesh = new();

                // Read this mesh's first unknown integer value.
                mesh.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this mesh's second unknown integer value.
                mesh.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this mesh's vertex count.
                uint vertexCount = reader.ReadUInt32();

                // Read this mesh's face count.
                uint faceCount = reader.ReadUInt32();

                // Read the size of this mesh's bounding volume hierarchy.
                uint boundingVolumeHeirarchyLength = reader.ReadUInt32();

                // Read this mesh's tag count, this always matches FaceCount, unless that value is 0, in which case this is always 1 instead.
                uint tagCount = reader.ReadUInt32();

                // Skip two unknown values that are always 0.
                reader.JumpAhead(0x08);

                // Read the offset to this mesh's vertex table.
                long vertexOffset = reader.ReadInt64();

                // Read the offset to this mesh's face table.
                long faceOffset = reader.ReadInt64();

                // Read the offset to this mesh's bounding volume hierarchy.
                long boundingVolumeHeirarchyOffset = reader.ReadInt64();

                // Read the offset to this mesh's tag table.
                long tagTableOffset = reader.ReadInt64();

                // Create an array of Vector3s for the vertices.
                mesh.Vertices = new Vector3[vertexCount];

                // If this mesh has faces, then create the face array too.
                if (faceCount > 0)
                    mesh.Faces = new Face[faceCount];

                // Save our current position so we can jump back for the next shape.
                long position = reader.BaseStream.Position;

                // Jump to this mesh's vertex table.
                reader.JumpTo(vertexOffset, false);

                // Loop through and read each vertex for this mesh.
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                    mesh.Vertices[vertexIndex] = Helpers.ReadHedgeLibVector3(reader);

                // Jump to this mesh's face table.
                reader.JumpTo(faceOffset, false);

                // Loop through and read each face for this mesh.
                for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                {
                    mesh.Faces[faceIndex] = new()
                    {
                        IndexA = reader.ReadUInt16(),
                        IndexB = reader.ReadUInt16(),
                        IndexC = reader.ReadUInt16()
                    };
                }

                // Jump to this mesh's bounding volume hierarchy.
                reader.JumpTo(boundingVolumeHeirarchyOffset, false);

                // TODO: Read this mesh's bounding volume hierarchy.

                // Jump to this mesh's tag table.
                reader.JumpTo(tagTableOffset, false);

                // If this mesh doesn't have any faces, then read the one layer and material value set that's here.
                if (faceCount == 0)
                {
                    // Read this mesh's flags.
                    mesh.Layer = (LayerType)reader.ReadByte();

                    // Read this mesh's unknown collision value.
                    mesh.UnknownCollisionValue = reader.ReadByte();

                    // Skip an unknown value of 0.
                    reader.JumpAhead(0x01);
                    
                    // Read this mesh's material type.
                    mesh.Material = (Material)reader.ReadByte();
                }

                // If this mesh does have faces, then read the layer and material value for each one.
                else
                {
                    for (int tagIndex = 0; tagIndex < tagCount; tagIndex++)
                    {
                        // Read this face's flags.
                        mesh.Faces[tagIndex].Layer = (LayerType)reader.ReadByte();

                        // Read this face's unknown collision value.
                        mesh.Faces[tagIndex].UnknownCollisionValue = reader.ReadByte();

                        // Skip an unknown value of 0.
                        reader.JumpAhead(0x01);

                        // Read this face's material type.
                        mesh.Faces[tagIndex].Material = (Material)reader.ReadByte();
                    }
                }

                // Jump back for the next mesh.
                reader.JumpTo(position);

                // Save this mesh.
                Data.Meshes[meshIndex] = mesh;
            }

            // Jump to the table of unknown data.
            reader.JumpTo(unknownOffset_1, false);

            // Loop through each unknown chunk.
            for (ulong unknownIndex = 0; unknownIndex < unknownCount_1; unknownIndex++)
            {
                // Create a new unknown data chunk.
                UnknownData unknown = new();

                // Read this chunk's first unknown integer value.
                unknown.UnknownUInt32_1 = reader.ReadUInt32();

                // Read this chunk's second unknown integer value.
                unknown.UnknownUInt32_2 = reader.ReadUInt32();

                // Read this chunk's first unknown Vector3.
                unknown.UnknownVector3_1 = Helpers.ReadHedgeLibVector3(reader);

                // Read this chunk's unknown Quaternion.
                unknown.UnknownQuaternion = Helpers.ReadHedgeLibQuaternion(reader);

                // Read this chunk's second unknown Vector3.
                unknown.UnknownVector3_2 = Helpers.ReadHedgeLibVector3(reader);

                // Save this unknown data chunk.
                Data.UnknownData[unknownIndex] = unknown;
            }

            // Close HedgeLib#'s BINAReader.
            reader.Close();
        }

        /// <summary>
        /// Exports this collision model to a standard Wavefront OBJ.
        /// TODO: Export material tags in a less stupid way maybe?
        /// TODO: Potentially replace this with a solution that integrates with the point cloud files?
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            if (Data.Meshes != null)
            {
                // Collisions that are meant to be instanced by a point cloud shouldn't have _col at the end, remove them if they do.
                if (filepath.EndsWith("_ins_col.obj"))
                    filepath = filepath.Replace("_ins_col.obj", "_ins.obj");

                // Set up the StreamWriter.
                StreamWriter obj = new(filepath);

                // Set up a variable to track vertices.
                int vertexCount = 0;

                // Loop through each mesh in the model to write its vertices.
                for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
                    foreach (Vector3 vertex in Data.Meshes[meshIndex].Vertices)
                        obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write this mesh's name.
                obj.WriteLine($"o {Path.GetFileNameWithoutExtension(filepath)}");
                obj.WriteLine($"g {Path.GetFileNameWithoutExtension(filepath)}");

                // Loop through each mesh in the model to write its faces.
                for (int meshIndex = 0; meshIndex < Data.Meshes.Length; meshIndex++)
                {
                    if (Data.Meshes[meshIndex].Faces != null)
                    {
                        // Set up default material type and flag.
                        Material materialType = Material.None;
                        LayerType materialLayer = LayerType.None;

                        // Write this mesh's faces.
                        foreach (var face in Data.Meshes[meshIndex].Faces)
                        {
                            // Check if the type and flags have been changed since the last face.
                            bool changedValue = false;

                            // Check and set the face's material type.
                            if (face.Material != materialType)
                            {
                                materialType = face.Material;
                                changedValue = true;
                            }

                            // Check and set the face's material layer.
                            if (face.Layer != materialLayer)
                            {
                                materialLayer = face.Layer;
                                changedValue = true;
                            }

                            // If either the flags or type have changed, then write a material entry.
                            if (changedValue)
                                obj.WriteLine($"usemtl {materialType}_{materialLayer}");

                            obj.WriteLine($"f {face.IndexA + 1 + vertexCount} {face.IndexB + 1 + vertexCount} {face.IndexC + 1 + vertexCount}");
                        }

                        // Increment vertexCount.
                        vertexCount += Data.Meshes[meshIndex].Vertices.Length;
                    }
                }

                // Close this StreamWriter.
                obj.Close();
            }
        }
    }
}
