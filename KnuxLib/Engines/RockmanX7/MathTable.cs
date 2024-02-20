using System.Diagnostics;
using System.Drawing;

namespace KnuxLib.Engines.RockmanX7
{
    // TODO: Make a start on reading skinned meshes.
    // TODO: See if there's another type as well, as some UI stuff use this format too, do they store more than just textures?
    public class MathTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public MathTable() { }
        public MathTable(string filepath, FormatVersion version = FormatVersion.environment, bool export = false)
        {
            Load(filepath, version);

            if (export)
                ExportCollisionOBJ(Path.ChangeExtension(filepath, ".obj"));
        }

        // Classes for this format.
        public enum FormatVersion
        {
            environment = 0,
            skinned = 1
        }

        public class FormatData
        {
            public List<MathTableChunks.Environment>? Environments { get; set; }

            public List<Bitmap> Textures { get; set; } = new();
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.environment)
        {
            // If this is an environment table, then initalise the list.
            if (version == FormatVersion.environment)
                Data.Environments = new();

            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the count of chunks in this file.
            uint chunkCount = reader.ReadUInt32();

            // Read the length of this header, should always be 0x800?
            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != 0x800) Debugger.Break();

            // Read this file's MATH_TBL signature.
            reader.ReadSignature(0x08, "MATH_TBL");

            // Loop through each chunk in this file.
            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                // Read the size of this chunk.
                int chunkSize = reader.ReadInt32();

                // TODO: What is this for? Called BLOCKCOUNT in a QuickBMS script.
                uint chunkBlockCount = reader.ReadUInt16();

                // TODO: What is this for?
                ushort chunkUnknownUShort_1 = reader.ReadUInt16();

                // TODO: What is this for?
                ushort chunkUnknownUShort_2 = reader.ReadUInt16();

                // TODO: Make sure this is correct and, if so, figure out what the different chunk types do.
                ushort chunkType = reader.ReadUInt16();

                // Realign to 0x10 bytes.
                reader.FixPadding(0x10);

                // Save our current position so we can jump back for the next chunk.
                long position = reader.BaseStream.Position;

                // Jump to the current headerEnd value.
                reader.JumpTo(headerEnd);

                // Check the chunk type, if it's not a texture one, then read it depending on the type of file.
                if (chunkType != 0xFFFF && version == FormatVersion.environment)
                    Data.Environments.Add(MathTableChunks.Environment.Read(reader));
                else if (chunkType != 0xFFFF & version == FormatVersion.skinned)
                    Console.WriteLine($"Skinned chunk type {chunkType} not handled.");
                else if (chunkType == 0xFFFF)
                    Data.Textures = MathTableChunks.Texture.Read(reader);

                // Jump ahead by the size of this chunk so we can reach the next one.
                reader.JumpAhead(chunkSize);

                // Realign to 0x800 so that we're in the right place.
                reader.FixPadding(0x800);

                // Update the headerEnd position as a hacky way to make jumping to the next chunk doable.
                headerEnd = (uint)reader.BaseStream.Position;

                // Jump back for the next chunk.
                reader.JumpTo(position);
            }
        }

        /// <summary>
        /// Exports the collision from this Math Table's environments to an OBJ.
        /// TODO: Will this be just temporary or will this do?
        /// </summary>
        /// <param name="filepath">The directory to export to.</param>
        public void ExportCollisionOBJ(string filepath)
        {
            // Only do this if an environment exists.
            if (Data.Environments == null)
                return;

            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 1;

            // Create the StreamWriter.
            StreamWriter obj = new(filepath);

            // Loop through each environment set.
            for (int environmentIndex = 0; environmentIndex < Data.Environments.Count; environmentIndex++)
            {
                // Loop through each collision group in this environment set.
                for (int collisionGroupIndex = 0; collisionGroupIndex < Data.Environments[environmentIndex].CollisionFaces.Count; collisionGroupIndex++)
                {
                    // Write the Vertex Comment for this model.
                    obj.WriteLine($"# Environment {environmentIndex} Collision Group {collisionGroupIndex} Vertices\r\n");

                    // Loop through each collision set in this collision group.
                    for (int collisionIndex = 0; collisionIndex < Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex].Length; collisionIndex++)
                    {
                        // Read the translation for the chunk that this collision is associated with.
                        Vector3 translation = Data.Environments[environmentIndex].Transforms[Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex][collisionIndex].AssociatedChunk].Translation;

                        // Write each vertex for this collision set.
                        foreach (Vector4 vertex in Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex][collisionIndex].VertexCoordinates)
                            obj.WriteLine($"v {vertex.X + translation.X} {vertex.Y + translation.Y} {vertex.Z + translation.Z}");
                    }

                    // Write the Faces Comment for this model.
                    obj.WriteLine($"\r\n# Environment {environmentIndex} Collision Group {collisionGroupIndex} Faces\r\n");

                    // Write the object name for this model.
                    obj.WriteLine($"g environment{environmentIndex}_collision{collisionGroupIndex}");
                    obj.WriteLine($"o environment{environmentIndex}_collision{collisionGroupIndex}");

                    // Loop through and write each face for this model.
                    for (int collisionIndex = 0; collisionIndex < Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex].Length; collisionIndex++)
                    {
                        // If this is a quad, then write four values.
                        if (Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex][collisionIndex].FaceType == MathTableChunks.FaceType.Quad)
                            obj.WriteLine($"f {vertexCount++} {vertexCount++} {vertexCount++} {vertexCount++}");

                        // If this is a triangle, then write three values.
                        if (Data.Environments[environmentIndex].CollisionFaces[collisionGroupIndex][collisionIndex].FaceType == MathTableChunks.FaceType.Triangle)
                            obj.WriteLine($"f {vertexCount++} {vertexCount++} {vertexCount++}");
                    }

                    // Write an empty line for neatness.
                    obj.WriteLine();
                }
            }

            // Close the StreamWriter.
            obj.Close();
        }
    }
}
