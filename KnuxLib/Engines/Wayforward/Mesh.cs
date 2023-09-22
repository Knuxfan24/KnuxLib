using KnuxLib.Engines.Wayforward.MeshChunks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward
{
    // Based on https://github.com/meh2481/wfLZEx/blob/master/wf3dEx.cpp
    // TODO: Reverse engineer the data at the unknown offsets in each of the chunks.
    // TODO: Figure out an export solution.
    // TODO: Figure out an import solution.
    // TODO: Figure out a way to save this format accurately.
    // TODO: Texture decompression/recompression.
    public class Mesh : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Mesh() { }
        public Mesh(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.mesh.json", Data);
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum NodeType : uint
        {
            Root = 0x0,
            Texture = 0x1,
            VertexTable = 0x2,
            FaceTable = 0x3,
            TextureMap = 0x4,
            Group = 0x5,
            ObjectMap = 0x6,
            Unknown_1 = 0x7,
            BoneName = 0x8,
            Bone = 0x9,
            Collision = 0xA,
            ObjectData = 0xB,
            Unknown_2 = 0xC
        }

        // Actual data presented to the end user.
        public List<object> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the file signature.
            reader.ReadSignature(4, "WFSN");

            // Always set to 0 in .wf3d files, has a varying value in .gpu files.
            // TODO: This seems to be important in some way, either store it or find out what controls it.
            uint UnknownUInt32_1 = reader.ReadUInt32();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Check the type of the root node, which should always be 0.
            if (reader.ReadUInt32() != 0) throw new Exception($"First node in {filepath} is not a Root Node!");

            // Read the amount of nodes in this file.
            uint nodeCount = reader.ReadUInt32();

            // Read the position of the node offset table for this file.
            long nodeOffsetTable = reader.ReadInt64();

            // Jump to the node offset table.
            reader.JumpTo(nodeOffsetTable);

            // Loop through and read each node.
            for (int i = 0; i < nodeCount; i++)
                ReadNode(reader, Data);

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Reads a node's type information and determines the functions to be used for reading.
        /// </summary>
        /// <param name="reader">The Marathon BinaryReader to use.</param>
        /// <param name="Data">The user end data to write the read data to.</param>
        private static void ReadNode(BinaryReaderEx reader, List<object> Data)
        {
            // Read the offset for this node's data from the offset table.
            long nodeOffset = reader.ReadInt64();

            // Save our current position so we can jump back for the next node.
            long position = reader.BaseStream.Position;

            // Jump to this node's data.
            reader.JumpTo(nodeOffset);

            // Read this node's type.
            NodeType nodeType = (NodeType)reader.ReadUInt32();

            // Read the count of sub entries in this node.
            uint nodeSubEntryCount = reader.ReadUInt32();

            // Read the offset to this node's sub entry table.
            long nodeSubEntryOffsetTable = reader.ReadInt64();

            // Read each node based on their type.
            switch (nodeType)
            {
                case NodeType.Texture:     Data.Add(Texture.Read(reader));       break;
                case NodeType.VertexTable: Data.Add(VertexTable.Read(reader));   break;
                case NodeType.FaceTable:   Data.Add(FaceTable.Read(reader));     break;
                case NodeType.TextureMap:  Data.Add(TextureMap.Read(reader));    break;
                case NodeType.Group:       Data.Add(Group.Read(reader));         break;
                case NodeType.ObjectMap:   Data.Add(ObjectMap.Read(reader));     break;
                case NodeType.Unknown_1:   Data.Add(Unknown1.Read(reader));      break;
                case NodeType.BoneName:    Data.Add(BoneName.Read(reader));      break;
                case NodeType.Bone:        Data.Add(Bone.Read(reader));          break;
                case NodeType.Collision:   Data.Add(MeshCollision.Read(reader)); break;
                case NodeType.ObjectData:  Data.Add(ObjectData.Read(reader));    break;
                case NodeType.Unknown_2:   Data.Add(Unknown2.Read(reader));      break;
                default:                   throw new NotImplementedException();
            }

            // If this node has any sub entries, then read them as well.
            if (nodeSubEntryCount > 0)
            {
                // Jump to the sub entry offset table.
                reader.JumpTo(nodeSubEntryOffsetTable);

                // Loop through and read each sub node based on ther type of their parent.
                for (int i = 0; i < nodeSubEntryCount; i++)
                {
                    switch (nodeType)
                    {
                        case NodeType.Group:     ReadNode(reader, (Data.Last() as Group).SubNodes);         break;
                        case NodeType.Unknown_1: ReadNode(reader, (Data.Last() as Unknown1).SubNodes);      break;
                        case NodeType.Collision: ReadNode(reader, (Data.Last() as MeshCollision).SubNodes); break;
                        default:                 throw new NotImplementedException();
                    }
                }
            }

            // Jump back for the next node.
            reader.JumpTo(position);
        }
    }
}
