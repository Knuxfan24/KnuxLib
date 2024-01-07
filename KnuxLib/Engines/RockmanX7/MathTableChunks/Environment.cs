using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

namespace KnuxLib.Engines.RockmanX7.MathTableChunks
{
    // TODO: Finish reading.
    // TODO: Figure out why SCR04_06's environment chunk is so wildly different to all the others.
    // TODO: Split the main environment class up into its sub chunk components?
    public class StageChunkIdentifier
    {
        /// <summary>
        /// The index of this stage chunk, usually sequential, but a single beta environment doesn't have them sequentially.
        /// </summary>
        public byte Index { get; set; }

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
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_3 { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FaceType : uint
    {
        Triangle = 2,
        Quad = 3
    }

    public class CollisionFace
    {
        /// <summary>
        /// The type of polygon this collision face is.
        /// </summary>
        public FaceType FaceType { get; set; }

        /// <summary>
        /// An unknown short value.
        /// TODO: What is this?
        /// </summary>
        public ushort UnknownUShort_1 { get; set; }

        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_1 { get; set; }

        /// <summary>
        /// The stage chunk this collision face is associated with.
        /// </summary>
        public uint AssociatedChunk { get; set; }

        /// <summary>
        /// The coordinates that make up this polygon's vertex points.
        /// </summary>
        public Vector4[] VertexCoordinates { get; set; } = new Vector4[3];
    }

    public class MeshSet
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
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_3 { get; set; }

        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_4 { get; set; }

        /// <summary>
        /// The coordinates of the vertices that make up this mesh.
        /// </summary>
        public MeshVertex[] Vertices { get; set; } = Array.Empty<MeshVertex>();

        /// <summary>
        /// The coordinates of the textures that should be mapped onto this mesh's vertices.
        /// </summary>
        public Vector2[] TextureCoordinates { get; set; } = Array.Empty<Vector2>();

        /// <summary>
        /// The colours of the vertices for this mesh.
        /// </summary>
        public VertexColour[] VertexColours { get; set; } = Array.Empty<VertexColour>();
    }

    public class MeshVertex
    {
        /// <summary>
        /// This vertex's position.
        /// </summary>
        public Vector3 Coordinates { get; set; }

        /// <summary>
        /// An unknown integer value.
        /// TODO: What is this?
        /// </summary>
        public uint UnknownUInt32_1 { get; set; }

        public override string ToString() => Coordinates.ToString();
    }

    public class Environment
    {
        /// <summary>
        /// An unknown Vector3 value.
        /// TODO: What is this? SCR10_05 is the only fle where this isn't always 0, even then, only Y has a unique value (of -1.99999737739563).
        /// </summary>
        public Vector3 UnknownVector3_1 { get; set; }

        /// <summary>
        /// An unknown Vector3 value.
        /// TODO: What is this? SCR10_05 is the only fle where this isn't always 0, even then, only Y has a unique value (of -1.99999737739563).
        /// TODO: Should this and UnknownVector3_1 be the same entity considering that?
        /// </summary>
        public Vector3 UnknownVector3_2 { get; set; }

        /// <summary>
        /// The 4x4 Matrixs used in this environment chunk.
        /// TODO: Are these used to position the chunks themselves in the world?
        /// </summary>
        public Matrix4x4[] Matrices { get; set; } = Array.Empty<Matrix4x4>();

        /// <summary>
        /// Decomposed copies of the 4x4 Matrixs used in this environment chunk.
        /// </summary>
        public DecomposedMatrix[] Transforms { get; set; } = Array.Empty<DecomposedMatrix>();

        /// <summary>
        /// The stage chunk identifiers used in this environment chunk.
        /// </summary>
        public StageChunkIdentifier[] StageChunkIdentifiers { get; set; } = Array.Empty<StageChunkIdentifier>();

        /// <summary>
        /// The collision faces that make up this environment chunk's collision data.
        /// </summary>
        public List<CollisionFace[]> CollisionFaces { get; set; } = new List<CollisionFace[]>();

        /// <summary>
        /// The meshes that make up this environment chunk's appearance in 3D space.
        /// </summary>
        public List<MeshSet> Meshes { get; set; } = new List<MeshSet>();

        public static Environment Read(BinaryReaderEx reader)
        {
            // Initialise the Environment.
            Environment environment = new();

            // Set the reader's offset to the start of this Environment chunk.
            reader.Offset = (uint)reader.BaseStream.Position;

            // Read the amount of sub chunks in this Environment chunk. This is normally 7, but SCR04_06 is set to 59 instead.
            uint subChunkCount = reader.ReadUInt32();

            // Loop through each sub chunk.
            for (int subChunkIndex = 0; subChunkIndex < subChunkCount; subChunkIndex++)
            {
                // Read this sub chunk's offset.
                uint subChunkOffset = reader.ReadUInt32();

                // If this offset actually has a value, then handle it.
                if (subChunkOffset != 0)
                {
                    // Save our current position so we can jump back for the next sub chunk.
                    long position = reader.BaseStream.Position;

                    // Jump to this sub chunk, relative to the Environment chunk's location.
                    reader.JumpTo(subChunkOffset, true);

                    // Read this sub chunk's ID?
                    uint subChunkID = reader.ReadUInt32();

                    // Determine what to do based on this sub chunk's type.
                    switch (subChunkID)
                    {
                        case 0x00:
                            // Check that the remaining three values in this sub chunk are all 0. 
                            if (reader.ReadUInt32() != 0) Debugger.Break();
                            if (reader.ReadUInt32() != 0) Debugger.Break();
                            if (reader.ReadUInt32() != 0) Debugger.Break();
                            break;

                        case 0x1000040:
                        case 0x01010060:
                            // Read this sub chunk's first unknown byte. This is normally 1, but SCR04_06 is set to 0 instead.
                            byte UnknownByte_1 = reader.ReadByte();

                            // Skip 0x27 bytes that are always FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF 00 00 00 CD CC CC 3D.
                            if (subChunkID == 0x01010060)
                                reader.JumpAhead(0x27);

                            // Skip 0x07 bytes that are always 00 00 00 CD CC CC 3D.
                            if (subChunkID == 0x1000040)
                                reader.JumpAhead(0x07);

                            // TODO: SCR04_06 has this set of four values done differently?
                            // Read this sub chunk's UnknownCount, used by a few parts of this sub chunk, but not all of them?
                            byte UnknownCount = reader.ReadByte();

                            // Check that this next byte is 0.
                            if (reader.ReadByte() != 0) Debugger.Break();

                            // Check that this next byte matches UnknownCount. SCR12_04 doesn't?
                            if (reader.ReadByte() != UnknownCount) Debugger.Break();

                            // Check that this next byte is 0.
                            if (reader.ReadByte() != 0) Debugger.Break();

                            // Check that this next value is 0.
                            // TODO: This might be an offset?
                            if (reader.ReadUInt32() != 0) Debugger.Break();

                            // Read this sub chunk's matrix table offset.
                            uint matrixTableOffset = reader.ReadUInt32();

                            // Read an unknown offset that only exists in SCR04_06.
                            uint UnknownOffset_1 = reader.ReadUInt32();

                            // Read this sub chunk's level chunk table offset.
                            uint chunkTableOffset = reader.ReadUInt32();

                            // Read an unknown offset that links to a table of FF FF 02 00 repeated UnknownCount times.
                            uint UnknownOffset_2 = reader.ReadUInt32();

                            // Read an unknown offset that only exists in SCR04_06.
                            uint UnknownOffset_3 = reader.ReadUInt32();

                            // Read this sub chunk's collision table offset.
                            uint collisionOffset = reader.ReadUInt32();

                            // Read an unknown offset to some data that seems to be four floats repeated UnknownCount times.
                            uint UnknownOffset_4 = reader.ReadUInt32();

                            // Read this sub chunk's mesh table offset.
                            uint meshOffset = reader.ReadUInt32();

                            // Read this sub chunk's first unknown Vector3 value.
                            environment.UnknownVector3_1 = reader.ReadVector3();

                            // Read this sub chunk's second unknown Vector3 value.
                            // environment.UnknownVector3_2 = reader.ReadVector3();

                            // Add 0x90 to the reader's offset.
                            if (subChunkID == 0x01010060)
                                reader.Offset += 0x90;

                            if (subChunkID == 0x1000040)
                                reader.Offset += 0x70;

                            for (byte test = 0; test < UnknownCount; test++)
                                reader.JumpAhead(0x24);

                            reader.FixPadding(0x10);

                            if (reader.BaseStream.Position != (matrixTableOffset + reader.Offset)) Debugger.Break();

                            // Jump to the matrix table's offset.
                            reader.JumpTo(matrixTableOffset, true);

                            // Initalise the Matrices and Transforms arrays.
                            environment.Matrices = new Matrix4x4[UnknownCount];
                            environment.Transforms = new DecomposedMatrix[UnknownCount];

                            // Loop through each matrix.
                            for (byte stageChunkIndex = 0; stageChunkIndex < UnknownCount; stageChunkIndex++)
                            {
                                // Read this matrix.
                                // TODO: Do these need transposing or not?
                                environment.Matrices[stageChunkIndex] = reader.ReadMatrix();

                                // Initalise this matrix's decomposed version.
                                environment.Transforms[stageChunkIndex] = new();

                                // Process ths matrix's decomposed state.
                                environment.Transforms[stageChunkIndex].Process(environment.Matrices[stageChunkIndex]);
                            }

                            // Jump to the chunk table's offset.
                            reader.JumpTo(chunkTableOffset, true);

                            // Initalise the Stage Chunk Identifiers array.
                            environment.StageChunkIdentifiers = new StageChunkIdentifier[UnknownCount];

                            // Loop through each stage chunk.
                            for (byte stageChunkIndex = 0; stageChunkIndex < UnknownCount; stageChunkIndex++)
                            {
                                // Initalise this stage chunk.
                                environment.StageChunkIdentifiers[stageChunkIndex] = new();

                                // Check if the next byte is set to 1.
                                if (reader.ReadByte() != 1) Debugger.Break();

                                // Set the chunk index, this is usually the same as the for loop value, but not in SCR12_04?
                                environment.StageChunkIdentifiers[stageChunkIndex].Index = reader.ReadByte();

                                // Check if the next two bytes are set to 0.
                                if (reader.ReadByte() != 0) Debugger.Break();
                                if (reader.ReadByte() != 0) Debugger.Break();

                                // Read this stage chunk identifier's first unknown integer value.
                                environment.StageChunkIdentifiers[stageChunkIndex].UnknownUInt32_1 = reader.ReadUInt32();

                                // Read this stage chunk identifier's second unknown integer value.
                                environment.StageChunkIdentifiers[stageChunkIndex].UnknownUInt32_2 = reader.ReadUInt32();

                                // Read this stage chunk identifier's third unknown integer value.
                                environment.StageChunkIdentifiers[stageChunkIndex].UnknownUInt32_3 = reader.ReadUInt32();
                            }

                            // Jump to this sub chunk's second unknown offset.
                            reader.JumpTo(UnknownOffset_2, true);

                            // Loop through and check that each value is set to 0x2FFFF. Some of these in SCR12_04 have other values instead?
                            for (byte stageChunkIndex = 0; stageChunkIndex < UnknownCount; stageChunkIndex++)
                                if (reader.ReadUInt32() != 0x2FFFF)
                                    Debugger.Break();

                            // Jump to this chunk's collision table.
                            reader.JumpTo(collisionOffset, true);

                            // Calculate the length of this table.
                            // TODO: Is there a proper count value for this table somewhere?
                            long tableEndPos = reader.BaseStream.Position + reader.ReadUInt32();

                            // Jump back so we actually read the first value.
                            reader.JumpBehind(0x04);

                            // Loop while our reader's position is lower than the calculated value.
                            while (reader.BaseStream.Position < tableEndPos)
                            {
                                // Read this collision entry's vertex table offset.
                                uint collisionVertexTableOffset = reader.ReadUInt32();

                                // Read the count of faces in this collision entry.
                                uint collisionFaceCount = reader.ReadUInt32();

                                // If both of the values we read are NOT 0, then actually read the collision entry.
                                if (collisionVertexTableOffset != 0 && collisionFaceCount != 0)
                                {
                                    // Save our position so we can jump back for the next collision table.
                                    long collisionPosition = reader.BaseStream.Position;

                                    // Jump to the position of this collision entry's vertex table.
                                    reader.JumpTo(collisionOffset + collisionVertexTableOffset, true);

                                    // Check that the next value matches collisionFaceCount.
                                    if (reader.ReadUInt32() != collisionFaceCount) Debugger.Break();

                                    // Check that the next three values are always 0.
                                    if (reader.ReadUInt32() != 0) Debugger.Break();
                                    if (reader.ReadUInt32() != 0) Debugger.Break();
                                    if (reader.ReadUInt32() != 0) Debugger.Break();

                                    // Initalise an array of faces for this collision entry.
                                    CollisionFace[] faceArray = new CollisionFace[collisionFaceCount];

                                    // Loop through and read each face.
                                    for (int faceIndex = 0; faceIndex < collisionFaceCount; faceIndex++)
                                    {
                                        // Initalise this face entry.
                                        CollisionFace face = new();

                                        // Read this face's type.
                                        face.FaceType = (FaceType)reader.ReadUInt16();

                                        // Read this face's unknown ushort value.
                                        face.UnknownUShort_1 = reader.ReadUInt16();

                                        // Read this face's first unknown integer value.
                                        face.UnknownUInt32_1 = reader.ReadUInt32();

                                        // Read the index of the chunk this face is for.
                                        face.AssociatedChunk = reader.ReadUInt32();

                                        // Check that the next value is always 0.
                                        if (reader.ReadUInt32() != 0) Debugger.Break();

                                        // If this is a quad, then reinitalise the VertexCoordinates array with a fourth slot.
                                        if (face.FaceType == FaceType.Quad)
                                            face.VertexCoordinates = new Vector4[4];

                                        // Read the three vertex coordinates.
                                        // TODO: What is the fourth value in each one? It's often 1, but not always.
                                        face.VertexCoordinates[0] = reader.ReadVector4();
                                        face.VertexCoordinates[1] = reader.ReadVector4();
                                        face.VertexCoordinates[2] = reader.ReadVector4();

                                        // If this face is a triangle, then check that the next four values are always 0 for padding.
                                        if (face.FaceType == FaceType.Triangle)
                                        {
                                            if (reader.ReadUInt32() != 0) Debugger.Break();
                                            if (reader.ReadUInt32() != 0) Debugger.Break();
                                            if (reader.ReadUInt32() != 0) Debugger.Break();
                                            if (reader.ReadUInt32() != 0) Debugger.Break();
                                        }
                                        // If it's a quad, then read the fourth value, with the same TODO seen above.
                                        else
                                        {
                                            face.VertexCoordinates[3] = reader.ReadVector4();
                                        }

                                        // Save this face into the faceArray.
                                        faceArray[faceIndex] = face;
                                    }

                                    // Save this collision entry.
                                    environment.CollisionFaces.Add(faceArray);

                                    // Jump back for the next collision table.
                                    reader.JumpTo(collisionPosition);
                                }
                            }

                            // Jump to the chunk's fourth unknown offset.
                            // TODO: Read the data here, 4 floats each?
                            reader.JumpTo(UnknownOffset_4, true);
                            
                            // Jump to the chunk's mesh offset.
                            reader.JumpTo(meshOffset, true);

                            // Calculate the length of this table.
                            // TODO: Is there a proper count value for this table somewhere?
                            tableEndPos = reader.BaseStream.Position + reader.ReadUInt32();

                            // Jump back so we actually read the first value.
                            reader.JumpBehind(0x04);

                            // Loop while our reader's position is lower than the calculated value.
                            while (reader.BaseStream.Position < tableEndPos)
                            {
                                // Read this mesh entry's offset.
                                uint meshTableOffset = reader.ReadUInt32();

                                // Read this mesh entry's length.
                                uint meshTableLength = reader.ReadUInt32();

                                // If both of the values we read are NOT 0, then actually read the mesh entry.
                                if (meshTableOffset != 0 && meshTableLength != 0)
                                {
                                    // Save our position so we can jump back for the next mesh table.
                                    long meshTablePosition = reader.BaseStream.Position;

                                    // Jump to the position of this mesh table.
                                    reader.JumpTo(meshOffset + meshTableOffset, true);

                                    // Caculate where the mesh table ends.
                                    // TODO: Is there a proper count of how many entries are in a mesh set instead?
                                    long meshTableEndPosition = reader.BaseStream.Position + meshTableLength;

                                    // Loop while the reader's position is lower than the mesh table's end position.
                                    while (reader.BaseStream.Position < meshTableEndPosition)
                                    {
                                        // Initalise this mesh set.
                                        MeshSet meshSet = new();

                                        // Check that the next value is always 0.
                                        if (reader.ReadUInt32() != 0) Debugger.Break();

                                        // Read this mesh set's first unknown integer value.
                                        meshSet.UnknownUInt32_1 = reader.ReadUInt32();

                                        // Read this mesh set's second unknown integer value.
                                        meshSet.UnknownUInt32_2 = reader.ReadUInt32();

                                        // Check that the next value is always 0.
                                        if (reader.ReadUInt32() != 0) Debugger.Break();

                                        // Read this mesh set's third unknown integer value.
                                        meshSet.UnknownUInt32_3 = reader.ReadUInt32();

                                        // Read this mesh set's fourth unknown integer value.
                                        meshSet.UnknownUInt32_4 = reader.ReadUInt32();

                                        // Check that the next value is always 8.
                                        if (reader.ReadUInt32() != 8) Debugger.Break();

                                        // Check that the next value is always 0.
                                        if (reader.ReadUInt32() != 0) Debugger.Break();

                                        // Read the amount of vertices in this mesh set.
                                        uint meshVertexCount = reader.ReadUInt32();

                                        // Check that the next three values are always 0.
                                        if (reader.ReadUInt32() != 0) Debugger.Break();
                                        if (reader.ReadUInt32() != 0) Debugger.Break();
                                        if (reader.ReadUInt32() != 0) Debugger.Break();

                                        // Initialise the Vertices, Texture Coordinates and Vertex Colours arrays.
                                        meshSet.Vertices = new MeshVertex[meshVertexCount];
                                        meshSet.TextureCoordinates = new Vector2[meshVertexCount];
                                        meshSet.VertexColours = new VertexColour[meshVertexCount];

                                        // Loop through and read each vertex for this mesh set.
                                        for (int vertexIndex = 0; vertexIndex < meshVertexCount; vertexIndex++)
                                            meshSet.Vertices[vertexIndex] = new()
                                            {
                                                Coordinates = reader.ReadVector3(),
                                                UnknownUInt32_1 = reader.ReadUInt32()
                                            };

                                        // Loop through and read each texture coordinate for this mesh set.
                                        for (int vertexIndex = 0; vertexIndex < meshVertexCount; vertexIndex++)
                                            meshSet.TextureCoordinates[vertexIndex] = reader.ReadVector2();

                                        // Loop through and read each vertex colour for this mesh set.
                                        for (int vertexIndex = 0; vertexIndex < meshVertexCount; vertexIndex++)
                                            meshSet.VertexColours[vertexIndex] = new()
                                            {
                                                Red = reader.ReadByte(),
                                                Green = reader.ReadByte(),
                                                Blue = reader.ReadByte(),
                                                Alpha = reader.ReadByte()
                                            };

                                        // Save this mesh set.
                                        environment.Meshes.Add(meshSet);

                                    }

                                    // Jump back for the next mesh set.
                                    reader.JumpTo(meshTablePosition);
                                }
                            }

                            // Remove the added 0x90 from the reader's offset.
                            if (subChunkID == 0x01010060)
                                reader.Offset -= 0x90;

                            if (subChunkID == 0x1000040)
                                reader.Offset -= 0x70;

                            break;

                        case 0x00020014:
                            if (reader.ReadUInt16() != 0x01) Debugger.Break();
                            ushort UnknownCount_1 = reader.ReadUInt16();
                            break;

                        default: Console.WriteLine($"Environment sub chunk {Helpers.ReturnUIntAsHex(subChunkID)} not yet handled!"); break;
                    }

                    // Jump back for the next sub chunk.
                    reader.JumpTo(position);
                }
            }

            // Return this environment's read data.
            return environment;
        }
    }
}
