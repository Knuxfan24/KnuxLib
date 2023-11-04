namespace KnuxLib.Engines.Wayforward.MeshChunks
{
    public class Bone
    {
        /// <summary>
        /// The matrices of this bone.
        /// </summary>
        public Matrix4x4[] BoneMatrices { get; set; } = Array.Empty<Matrix4x4>();

        /// <summary>
        /// A copy of this bones matrices decomposed into a human readable format.
        /// </summary>
        public DecomposedMatrix[] DecomposedMatrices { get; set; } = Array.Empty<DecomposedMatrix>();

        public override string ToString() => "Bone";

        /// <summary>
        /// Read the data of this bone from the reader's current position.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        public static Bone Read(BinaryReaderEx reader)
        {
            // Initialise the bone.
            Bone bone = new();

            // Skip an unknown value of 0.
            reader.JumpAhead(0x04);

            // Read the amount of bones in this node.
            uint boneCount = reader.ReadUInt32();

            // Read the offset to this bone's data.
            uint boneDataOffset = reader.ReadUInt32();

            // Read the size (including the data magic and size) of the data at BoneDataOffset.
            uint boneDataSize = reader.ReadUInt32();

            // Save our position to jump back to after reading the bone data.
            long position = reader.BaseStream.Position;

            // Jump to this bone's data offset.
            reader.JumpTo(boneDataOffset);

            // Read and check this data's magic value.
            uint dataMagic = reader.ReadUInt32();
            if (dataMagic != 0xFFFFFF) throw new Exception($"DataMagic at 0x{boneDataOffset:X} was 0x{dataMagic:X} rather than 0xFFFFFFFF!");

            // Read this data's size.
            uint dataSize = reader.ReadUInt32();

            // Define the matrix arrays.
            bone.BoneMatrices = new Matrix4x4[boneCount];
            bone.DecomposedMatrices = new DecomposedMatrix[boneCount];

            // Loop through and read each matrix.
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                // Read and transpose the matrix for this bone.
                bone.BoneMatrices[boneIndex] = Matrix4x4.Transpose(reader.ReadMatrix());

                // Set up and process this bone matrix's decomposed version.
                bone.DecomposedMatrices[boneIndex] = new();
                bone.DecomposedMatrices[boneIndex].Process(bone.BoneMatrices[boneIndex]);
            }

            // Jump back to our saved position.
            reader.JumpTo(position);

            // Return our bone.
            return bone;
        }
    }
}
