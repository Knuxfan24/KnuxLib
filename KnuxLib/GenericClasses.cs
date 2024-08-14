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
}
