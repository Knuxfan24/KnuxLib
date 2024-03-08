using static KnuxLib.Engines.Hedgehog.Collision_Rangers;

namespace KnuxLib.Engines.Hedgehog
{
    public class Heightfield : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Heightfield() { }
        public Heightfield(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.heightfield.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// This heightfield's width.
            /// TODO: Should this be here? Feel almost like this should be calculated from the height array.
            /// </summary>
            public uint Width { get; set; }

            /// <summary>
            /// This heightfield's height.
            /// TODO: Should this be here? Feel almost like this should be calculated from the height array.
            /// </summary>
            public uint Height { get; set; }

            /// <summary>
            /// This heightfield's precision, used to calculate the height values, alongside the scale and a short value.
            /// </summary>
            public float Precision { get; set; }

            /// <summary>
            /// This heightfield's scale, used to calculate the height values, alongside the precision and a short value.
            /// </summary>
            public float Scale { get; set; }

            /// <summary>
            /// The height values in this heightfield.
            /// </summary>
            public float[] Heights { get; set; } = Array.Empty<float>();

            /// <summary>
            /// The collision types this heightfield uses.
            /// </summary>
            public Collision[] Collisions { get; set; } = Array.Empty<Collision>();

            /// <summary>
            /// The indices for what element uses what collision.
            /// TODO: How do these work? As there's less of them then there are height values.
            /// </summary>
            public byte[] CollisionIndices { get; set; } = Array.Empty<byte>();
        }

        public class Collision
        {
            /// <summary>
            /// This height's layer.
            /// </summary>
            public LayerType Layer { get; set; }

            /// <summary>
            /// This height's material.
            /// </summary>
            public Material Material { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Read this file's signature.
            reader.ReadSignature(0x04, "HTFD");

            // Read the size of this file.
            uint fileSize = reader.ReadUInt32();

            // Skip three unknown values of 1, 2 and 0.
            reader.JumpAhead(0x0C);

            // Read this heightfield's width.
            Data.Width = reader.ReadUInt32();

            // Read this heightfield's height.
            Data.Height = reader.ReadUInt32();

            // Skip two unknown floating point values that are always 1.
            reader.JumpAhead(0x08);

            // Read this heightfield's precision.
            Data.Precision = reader.ReadSingle();

            // Read this heightfield's scale.
            Data.Scale = reader.ReadSingle();

            // Initialise this heightfield's height array.
            Data.Heights = new float[Data.Width * Data.Height];

            // Loop through and read each height from this heightfield, calculating their values.
            for (int heightIndex = 0; heightIndex < Data.Heights.Length; heightIndex++)
                Data.Heights[heightIndex] = (reader.ReadInt16() * Data.Precision * Data.Scale);

            // Initialise this heightfield's collision types, using the value before the table.
            Data.Collisions = new Collision[reader.ReadUInt32()];

            // Loop through each collision value.
            for (int collisionIndex = 0; collisionIndex < Data.Collisions.Length; collisionIndex++)
            {
                // Skip three unknown values of 0.
                reader.JumpAhead(0x03);

                // Read this collision material and layer.
                Data.Collisions[collisionIndex] = new()
                {
                    Material = (Material)reader.ReadByte(),
                    Layer = (LayerType)reader.ReadByte()
                };

                // Skip three unknown values of 0.
                reader.JumpAhead(0x03);
            }

            // Initialise this heightfield's collision indices array.
            Data.CollisionIndices = new byte[(Data.Width - 1) * (Data.Height - 1)];

            // Loop through and read each collision index.
            for (int collisionIndex = 0; collisionIndex < Data.CollisionIndices.Length; collisionIndex++)
                Data.CollisionIndices[collisionIndex] = reader.ReadByte();

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

            // Write this file's HTFD signature.
            writer.Write("HTFD");

            // Write a temporary value for the file size.
            writer.Write("TEMP");

            // Write three unknown values of 1, 2 and 0.
            writer.Write(0x01);
            writer.Write(0x02);
            writer.Write(0x00);

            // Write this heightfield's width.
            writer.Write(Data.Width);

            // Write this heightfield's height.
            writer.Write(Data.Height);
            
            // Write two unknown floating point values that are always 1.
            writer.Write(1f);
            writer.Write(1f);

            // Write this heightfield's precision.
            writer.Write(Data.Precision);

            // Write this heightfield's scale.
            writer.Write(Data.Scale);

            // Loop through and write each height, calculating their values.
            // TODO: Can occasionally be off by one depending on the floating point. Fix this, as it is significant.
            for (int heightIndex = 0; heightIndex < Data.Heights.Length; heightIndex++)
                writer.Write((short)(Data.Heights[heightIndex] / Data.Precision / Data.Scale));

            // Write the count of collision types in this heightfield.
            writer.Write(Data.Collisions.Length);

            // Loop through each collision type.
            for (int collisionIndex = 0; collisionIndex < Data.Collisions.Length; collisionIndex++)
            {
                // Write three null bytes.
                writer.WriteNulls(0x03);

                // Write this collision's material type.
                writer.Write((byte)Data.Collisions[collisionIndex].Material);

                // Write this collision's layer type.
                writer.Write((byte)Data.Collisions[collisionIndex].Layer);

                // Write three null bytes.
                writer.WriteNulls(0x03);
            }

            // Loop through and write each collision index.
            for (int collisionIndex = 0; collisionIndex < Data.CollisionIndices.Length; collisionIndex++)
                writer.Write(Data.CollisionIndices[collisionIndex]);

            // Realign to 0x04 bytes.
            writer.FixPadding(0x04);

            // Jump back to 0x04.
            writer.BaseStream.Position = 0x04;

            // Overwrite the "TEMP" entry with the file size.
            writer.Write((uint)writer.BaseStream.Length);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
