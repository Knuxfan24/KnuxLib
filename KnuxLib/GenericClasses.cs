global using KnuxLib.Helpers;
global using KnuxLib.IO.Headers;
global using KnuxLib.IO;
global using Newtonsoft.Json;
global using System.Numerics;
global using System.Text;

// Generic classes for other formats.
namespace KnuxLib
{
    public class FileNode
    {
        /// <summary>
        /// The name of this file node.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The bytes that make up this file node.
        /// </summary>
        public byte[] Data { get; set; } = [];

        /// <summary>
        /// Displays this file node's path in the debugger.
        /// </summary>
        public override string ToString() => Name;

        /// <summary>
        /// Initialises this file node with default data.
        /// </summary>
        public FileNode() { }

        /// <summary>
        /// Initialises this file node with the provided data.
        /// </summary>
        public FileNode(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>
        /// Initialises this file node with the provided data.
        /// </summary>
        public FileNode(string name, string sourceFile)
        {
            Name = name;
            Data = File.ReadAllBytes(sourceFile);
        }
    }

    public class VertexColour
    {
        /// <summary>
        /// The Red value for this Vertex Colour.
        /// </summary>
        public byte Red { get; set; } = 255;

        /// <summary>
        /// The Green value for this Vertex Colour.
        /// </summary>
        public byte Green { get; set; } = 255;

        /// <summary>
        /// The Blue value for this Vertex Colour.
        /// </summary>
        public byte Blue { get; set; } = 255;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// The (optional) Alpha value for this Vertex Colour.
        /// </summary>
        public byte? Alpha { get; set; }

        /// <summary>
        /// Displays this vertex colour's values in the debugger.
        /// </summary>
        public override string ToString()
        {
            if (Alpha != null)
                return $"R: {Red} G: {Green} B: {Blue} A: {Alpha.Value}";
            else
                return $"R: {Red} G: {Green} B: {Blue}";
        }

        /// <summary>
        /// Initialises this vertex colour with default data.
        /// </summary>
        public VertexColour() { }

        /// <summary>
        /// Initialises this vertex colour with the provided data.
        /// </summary>
        public VertexColour(byte red, byte green, byte blue, byte? alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Reads an RGB vertex colour entry.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <param name="hasAlpha">Whether or not to read an alpha byte.</param>
        /// <param name="aRGB">Whether or not the alpha byte is stored first.</param>
        public void Read(ExtendedBinaryReader reader, bool hasAlpha = true, bool aRGB = false)
        {
            if (hasAlpha && aRGB)
                Alpha = reader.ReadByte();

            Red = reader.ReadByte();
            Green = reader.ReadByte();
            Blue = reader.ReadByte();

            if (hasAlpha && !aRGB)
                Alpha = reader.ReadByte();
        }

        /// <summary>
        /// Writes an RGB vertex colour entry.
        /// </summary>
        /// <param name="writer">The BinaryWriter to use</param>
        /// <param name="aRGB">Whether or not the alpha byte is stored first (if it exists).</param>
        public void Write(ExtendedBinaryWriter writer, bool aRGB = false)
        {
            if (Alpha != null && aRGB)
                writer.Write(Alpha.Value);

            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);

            if (Alpha != null && !aRGB)
                writer.Write(Alpha.Value);
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
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Initialises this decomposed matrix with default data.
        /// </summary>
        public DecomposedMatrix() { }

        /// <summary>
        /// Initialises this decomposed matrix by processing a regular matrix.
        /// </summary>
        public DecomposedMatrix(Matrix4x4 matrix) => Process(matrix);

        /// <summary>
        /// Process a matrix into a decomposed version.
        /// </summary>
        /// <param name="matrix">The matrix to decompose.</param>
        public void Process(Matrix4x4 matrix)
        {
            // Decompose the matrix.
            Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

            // Set the values for this decomposed matrix.
            Translation = translation;
            EulerRotation = RotationHelpers.ConvertQuaternionToEuler(rotation);
            Scale = scale;
        }
    }

    public class AABB
    {
        /// <summary>
        /// The minimum coordinate of the axis aligned bounding box.
        /// </summary>
        public Vector3 Min { get; set; }

        /// <summary>
        /// The maximum coordinate of the axis aligned bounding box.
        /// </summary>
        public Vector3 Max { get; set; }

        /// <summary>
        /// Initialises this axis aligned bounding box with default data.
        /// </summary>
        public AABB() { }

        /// <summary>
        /// Initialises this axis aligned bounding box with the provided data.
        /// </summary>
        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initialises this axis aligned bounding box by reading its data from a BinaryReader.
        /// </summary>
        public AABB(ExtendedBinaryReader reader) => Read(reader);

        /// <summary>
        /// Reads an axis aligned bounding box.
        /// </summary>
        public void Read(ExtendedBinaryReader reader)
        {
            Min = reader.ReadVector3();
            Max = reader.ReadVector3();
        }

        /// <summary>
        /// Writes an axis aligned bounding box.
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(Min);
            writer.Write(Max);
        }
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
    }
}
