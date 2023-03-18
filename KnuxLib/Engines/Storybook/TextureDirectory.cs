namespace KnuxLib.Engines.Storybook
{
    public class TextureDirectory : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public TextureDirectory() { }
        public TextureDirectory(string filepath, bool extract = false)
        {
            Load(filepath);

            if (extract)
                Extract($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}");
        }

        // Actual data presented to the end user.
        public List<FileNode> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath), true);

            // Read this file's signature.
            reader.ReadSignature(0x04, "TXAG");

            // Read the amount of textures in this file.
            uint textureCount = reader.ReadUInt32();

            // Loop through each texture in this file.
            for (int i = 0; i < textureCount; i++)
            {
                // Create a texture entry.
                FileNode texture = new();

                // Read the offset to this texture's data.
                uint textureOffset = reader.ReadUInt32();

                // Read the size of this texture's data in bytes.
                int textureSize = reader.ReadInt32();

                // Read this texture's name.
                texture.Name = reader.ReadNullPaddedString(0x20);

                // Save our current position so we can jump back for the next texture.
                long position = reader.BaseStream.Position;

                // Jump to the texture's offset.
                reader.JumpTo(textureOffset);

                // Read the texture's data.
                texture.Data = reader.ReadBytes(textureSize);

                // Jump back for the next texture.
                reader.JumpTo(position);

                // Save this texture.
                Data.Add(texture);
            }
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath), true);

            // Write this file's signature.
            writer.WriteNullPaddedString("TXAG", 0x04);

            // Write the amount of textures in this file.
            writer.Write(Data.Count);

            // Loop through each texture to write the offset table.
            for (int i = 0; i < Data.Count; i++)
            {
                // Add an offset for this texture.
                writer.AddOffset($"Texture{i}Offset");

                // Write this texture's size in bytes.
                writer.Write(Data[i].Data.Length);

                // Write this texture's name, padded to 0x20 bytes.
                writer.WriteNullPaddedString(Data[i].Name, 0x20);
            }

            // Realign to 0x20.
            // TODO: Test this is accurate on every file.
            writer.FixPadding(0x20);

            // Loop through each texture to write their data.
            for (int i = 0; i < Data.Count; i++)
            {
                // Fill in this texture's offset.
                writer.FillOffset($"Texture{i}Offset");

                // Write this texture's data.
                writer.Write(Data[i].Data);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }

        /// <summary>
        /// Extracts the textures in this format to disc.
        /// </summary>
        /// <param name="directory">The directory to extract to.</param>
        public void Extract(string directory)
        {
            // Create the extraction directory.
            Directory.CreateDirectory(directory);

            // Loop through each node to extract.
            foreach (FileNode node in Data)
            {
                // Print the name of the texture we're extracting.
                Console.WriteLine($"Extracting {node.Name}.");

                // Extract the texture.
                File.WriteAllBytes($@"{directory}\{node.Name}.gvr", node.Data);
            }
        }

        /// <summary>
        /// Imports files from a directory into a texture directory.
        /// </summary>
        /// <param name="directory">The directory to import, excluding sub directories.</param>
        public void Import(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.gvr"))
            {
                FileNode node = new()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Data = File.ReadAllBytes(file)
                };
                Data.Add(node);
            }
        }
    }
}
