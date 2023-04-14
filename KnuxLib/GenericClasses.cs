global using HedgeLib.Headers;
global using Marathon.IO;
global using System.Numerics;

namespace KnuxLib
{
    // Generic classes for other formats.
    public class FileNode
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The bytes that make up this node.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public override string ToString() => Name;
    }

    public class VertexColour
    {
        /// <summary>
        /// The Red value for this Vertex Colour.
        /// </summary>
        public byte Red { get; set; }

        /// <summary>
        /// The Green value for this Vertex Colour.
        /// </summary>
        public byte Green { get; set; }

        /// <summary>
        /// The Blue value for this Vertex Colour.
        /// </summary>
        public byte Blue { get; set; }

        /// <summary>
        /// The (optional) Alpha value for this Vertex Colour.
        /// </summary>
        public byte? Alpha { get; set; }

        /// <summary>
        /// Reads an RGB vertex colour entry.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="hasAlpha">Whether or not to read an alpha byte.</param>
        public void Read(BinaryReaderEx reader, bool hasAlpha = true)
        {
            Red = reader.ReadByte();
            Blue = reader.ReadByte();
            Green = reader.ReadByte();

            if (hasAlpha)
                Alpha = reader.ReadByte();
        }
    }

    public class DecomposedMatrix
    {
        /// <summary>
        /// This matrix's position in 3D space.
        /// </summary>
        public Vector3 Translation { get; set; }

        /// <summary>
        /// This matrix's rotation, converted from a quaternion to euler angles.
        /// </summary>
        public Vector3 EulerRotation { get; set; }

        /// <summary>
        /// This matrix's scale factor.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Process a matrix into a decomposed version.
        /// </summary>
        /// <param name="matrix">The matrix to decompose.</param>
        public void Process(Matrix4x4 matrix)
        {
            // Decompose the matrix.
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            // Convert the quaternion to euler angles using HedgeLib#.
            HedgeLib.Vector3 hedgeLibV3 = new HedgeLib.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W).ToEulerAngles();

            // Set the values for this decomposed matrix.
            Translation = translation;
            EulerRotation = new(hedgeLibV3.X, hedgeLibV3.Y, hedgeLibV3.Z);
            Scale = scale;
        }
    }

    public class SetParameter
    {
        /// <summary>
        /// This parameter's name.
        /// TODO: Currently not used as these parameters just boil down to reading the table each byte at a time.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The data for this parameter.
        /// </summary>
        public object Data { get; set; } = (byte)0;

        /// <summary>
        /// The type of this parameter.
        /// </summary>
        public Type DataType { get; set; } = typeof(byte);

        public override string ToString()
        {
            if (Name != "")
                return Name;
            else
                return null;
        }
    }
}
