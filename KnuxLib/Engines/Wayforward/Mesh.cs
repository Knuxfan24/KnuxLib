using KnuxLib.Engines.Wayforward.MeshChunks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Wayforward
{
    // Based on https://github.com/meh2481/wfLZEx/blob/master/wf3dEx.cpp
    // TODO: Reverse engineer the data at the unknown offsets in each of the chunks.
    // TODO: Figure out an export solution.
    // TODO: Figure out an import solution.
    // TODO: Get saving fully working once the unknown data in the chunks is sorted out.
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

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up a list of offsets.
            List<long> Offsets = new();

            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the WFSN signature.
            writer.Write("WFSN");

            // Add an offset to the first data chunk. The .wf3d files don't seem to need this, but the .gpu ones do.
            writer.AddOffset("FirstDataChunk");

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write the root node's type.
            writer.Write(0);

            // Write the amount of nodes in this file.
            writer.Write(Data.Count);

            // Add an offset for the root node's table.
            writer.AddOffset("RootNodeOffsetTable", 0x08);

            // Loop through each node for the main writing.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add an offset for this node.
                Offsets.Add(writer.BaseStream.Position);

                // Write the node based on its type.
                switch (Data[i].GetType().Name)
                {
                    case "Texture":     (Data[i] as Texture).Write(writer, i);     break;
                    case "VertexTable": (Data[i] as VertexTable).Write(writer, i); break;
                    case "FaceTable":   (Data[i] as FaceTable).Write(writer, i);   break;
                    case "TextureMap":  (Data[i] as TextureMap).Write(writer);     break;
                    case "Group":       (Data[i] as Group).Write(writer, i);       break;
                    case "ObjectMap":   (Data[i] as ObjectMap).Write(writer);      break;

                    default: Console.WriteLine($"Writing of node type '{Data[i].GetType().Name}' not yet implemented."); break;
                }
            }

            // Loop through each node for the sub nodes.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write the sub nodes based on the parent node's type.
                switch (Data[i].GetType().Name)
                {
                    case "Group": (Data[i] as Group).WriteSubNodes(writer, i); break;
                }
            }

            // Fill in the offset for the root node's table.
            writer.FillOffset("RootNodeOffsetTable");

            // Write all the offsets for the nodes.
            foreach (long offset in Offsets)
                writer.Write(offset);

            // Fill in the offset for the first data chunk.
            writer.FillOffset("FirstDataChunk");

            // Loop through each node for their names.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write the node name based on the type.
                switch (Data[i].GetType().Name)
                {
                    case "Texture": (Data[i] as Texture).WriteName(writer, i); break;
                    case "Group":   (Data[i] as Group).WriteName(writer, i);   break;
                }
            }

            // Loop through each node for their extra data chunks.
            for (int i = 0; i < Data.Count; i++)
            {
                // Write the extra data based on the node type.
                switch (Data[i].GetType().Name)
                {
                    case "Texture":     (Data[i] as Texture).WriteData(writer, i);     break;
                    case "VertexTable": (Data[i] as VertexTable).WriteData(writer, i); break;
                    case "FaceTable":   (Data[i] as FaceTable).WriteData(writer, i);   break;
                }
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
