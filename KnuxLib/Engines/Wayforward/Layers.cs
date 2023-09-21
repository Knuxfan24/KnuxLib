namespace KnuxLib.Engines.Wayforward
{
    // TODO: Are the layer names really 40 characters? Experiment in game and see if that is the actual limit or if it's something like 32 instead.
    public class Layers : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Layers() { }
        public Layers(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.layers.json", Data);
        }

        // Actual data presented to the end user.
        public List<string> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read the count of layers in this file.
            ulong layerCount = reader.ReadUInt64();

            // Skip an unknown value of 0x1000.
            reader.JumpAhead(0x04);

            // Skip three unknown values of 0x40.
            reader.JumpAhead(0x0C);

            // Skip an unknown value of 0x04.
            reader.JumpAhead(0x04);

            // Loop through each layer.
            for (ulong i = 0; i < layerCount; i++)
            {
                // Read this layer's name.
                Data.Add(reader.ReadNullPaddedString(0x28));

                // Skip three unknown values. A float of 1, an integer of -1 and an integer of 0.
                reader.JumpAhead(0x0C);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // Write the amount of layers in this file.
            writer.Write((ulong)Data.Count);

            // Write an unknown value of 0x1000.
            writer.Write(0x1000);

            // Write three unknown values of 0x40.
            writer.Write(0x40);
            writer.Write(0x40);
            writer.Write(0x40);

            // Write an unknown value of 0x04.
            writer.Write(0x04);

            // Loop through each layer.
            for (int i = 0; i < Data.Count; i++)
            {
                // Throw an exception if this layer's name is too large.
                if (Data[i].Length > 0x28)
                    throw new Exception($"Layer Name '{Data[i]} is larger than 40 characters!");

                // Write this layer's name.
                writer.WriteNullPaddedString(Data[i], 0x28);

                // Write three unknown values. A float of 1, an integer of -1 and an integer of 0.
                writer.Write(1f);
                writer.Write(-1);
                writer.Write(0);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
