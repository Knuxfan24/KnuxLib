namespace KnuxLib.Engines.Wayforward
{
    // TODO: Tidy up a bit.
    public class Environment : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Environment() { }
        public Environment(string filepath, bool bigEndian = false, bool export = false)
        {
            // Set this format's JSON file extension (usually in the form of engine.format.json).
            string jsonExtension = ".wayforward.environment.json";

            // Check if the input file is this format's JSON.
            if (Helpers.GetExtension(filepath) == jsonExtension)
            {
                // Deserialise the input JSON.
                Data = JsonDeserialise<Entity[]>(filepath);

                // If the export flag is set, then save this format.
                if (export)
                    Save($@"{Helpers.GetExtension(filepath, true)}.env", bigEndian);
            }

            // Check if the input file isn't this format's JSON.
            else
            {
                // Load this file.
                Load(filepath, bigEndian);

                // If the export flag is set, then export this format.
                if (export)
                    JsonSerialise($@"{Helpers.GetExtension(filepath, true)}{jsonExtension}", Data);
            }
        }

        // Classes for this format.
        public class Entity
        {
            /// <summary>
            /// The name of this entity.
            /// </summary>
            public string EntityName { get; set; } = "";

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this?
            /// TODO: Causes some weird changes to lighting on the mesh?
            /// </summary>
            public uint UnknownUInt32_1 { get; set; }

            /// <summary>
            /// An unknown 32-bit boolean value.
            /// TODO: What is this?
            /// TODO: Seems to be a full bright thing? Though some meshes end up with weird UVs?
            /// </summary>
            public bool UnknownBoolean_1 { get; set; }

            /// <summary>
            /// A 0-1 RGB value that modifies a model's colour. Only appears to work on certain mesh types?
            /// </summary>
            public Vector3 ColourModifier { get; set; }

            /// <summary>
            /// How fast this mesh's textures should scroll on the X axis.
            /// </summary>
            public float HorizontalTextureScrollSpeed { get; set; }

            /// <summary>
            /// How fast this mesh's textures should scroll on the Y axis.
            /// </summary>
            public float VerticalTextureScrollSpeed { get; set; }

            /// <summary>
            /// The position of this entity in 3D space.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The scale of this entity's model.
            /// </summary>
            public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

            /// <summary>
            /// The rotation of this entity.
            /// </summary>
            public Quaternion Rotation { get; set; } = new(1f, 0f, 0f, 0f);

            /// <summary>
            /// The index of the layer (definied in the stage's LGB file) that this entity is part of.
            /// </summary>
            public uint LayerIndex { get; set; }

            /// <summary>
            /// The name of the wf3d mesh this entity should use.
            /// </summary>
            public string ModelName { get; set; } = "";

            /// <summary>
            /// Displays this entity's name in the debugger.
            /// </summary>
            public override string ToString() => EntityName;

            /// <summary>
            /// Initialises this entity with default data.
            /// </summary>
            public Entity() { }

            /// <summary>
            /// Initialises this entity with the provided data.
            /// </summary>
            public Entity(string entityName, uint unknownUInt32_1, bool unknownBoolean_1, Vector3 colourModifier, float horizontalTextureScrollSpeed, float verticalTextureScrollSpeed, Vector3 position, Vector3 scale, Quaternion rotation, uint layerIndex, string modelName)
            {
                EntityName = entityName;
                UnknownUInt32_1 = unknownUInt32_1;
                UnknownBoolean_1 = unknownBoolean_1;
                ColourModifier = colourModifier;
                HorizontalTextureScrollSpeed = horizontalTextureScrollSpeed;
                VerticalTextureScrollSpeed = verticalTextureScrollSpeed;
                Position = position;
                Scale = scale;
                Rotation = rotation;
                LayerIndex = layerIndex;
                ModelName = modelName;
            }

            /// <summary>
            /// Initialises this entity by reading its data from a BinaryReader.
            /// </summary>
            public Entity(ExtendedBinaryReader reader, long entityNameTableOffset) => Read(reader, entityNameTableOffset);

            /// <summary>
            /// Reads the data for this entity.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, long entityNameTableOffset)
            {
                // Read the offset for this entity's data.
                long entityOffset = reader.ReadInt64();

                // Save our position to jump back for the next entity.
                long position = reader.BaseStream.Position;

                // Jump to this entity's data.
                reader.JumpTo(entityOffset);

                // Skip an unknown sequence of bytes that is always 00 00 00 00 10 01 0A 57 00 00 00 00
                reader.CheckValue(0x00);
                reader.CheckValue(0x570A0110);
                reader.CheckValue(0x00);

                // Read the entity's name.
                EntityName = Helpers.ReadWayforwardLengthPrefixedString(reader, entityNameTableOffset, (uint)reader.ReadUInt64());

                // Skip two unknown values that are either 0 or 0xFFFFFFFF.
                // TODO: Does this matter?
                reader.JumpAhead(0x08);

                // Read the first unknown integer value.
                UnknownUInt32_1 = reader.ReadUInt32();

                // Read an unknown boolean value.
                UnknownBoolean_1 = reader.ReadBoolean(0x04);

                // Read this entity's colour modifer.
                ColourModifier = reader.ReadVector3();

                // Read this entity's horizontal texture scroll speed value.
                HorizontalTextureScrollSpeed = reader.ReadSingle();

                // Read this entity's vertical texture scroll speed value.
                VerticalTextureScrollSpeed = reader.ReadSingle();

                // Skip three unknown floating point values that are always 0.5.
                reader.CheckValue(0.5f, 0x03);

                // Skip three unknown values that are always 1.
                reader.CheckValue(0x01L, 0x03);

                // Skip two unknown values that are always 0xFFFFFFFF.
                reader.CheckValue(-1L);

                // Skip an unknown floating point value that is always 0.463772833.
                reader.CheckValue(0.463772833f);

                // Skip an unknown value of 0x30.
                reader.CheckValue((byte)0x30);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);

                // Read the entity's position.
                Position = reader.ReadVector3();

                // Read the entity's scale.
                Scale = reader.ReadVector3();

                // Read the entity's rotation.
                Rotation = reader.ReadQuaternion();

                // Skip an unknown sequenece of bytes that is always 0B 01 8A A1
                reader.CheckValue(-1584791285);

                // Skip an unknown value of 0x30.
                reader.CheckValue((byte)0x30);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);

                // Read this entity's layer index.
                LayerIndex = reader.ReadUInt32();

                // Skip an unknown sequence of bytes that is always 00 00 00 00 1A 52 9A 1A 30 00 00 00
                reader.CheckValue(0x00);
                reader.CheckValue(0x1A9A521A);
                reader.CheckValue((byte)0x30);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);
                reader.CheckValue((byte)0x00);

                // Read the entity's model name.
                ModelName = Helpers.ReadWayforwardLengthPrefixedString(reader, entityNameTableOffset, (uint)reader.ReadUInt64());

                // Jump back for the next entity.
                reader.JumpTo(position);
            }

            public void Write(ExtendedBinaryWriter writer, int index, List<string> names)
            {
                // Fill in this entity's offset.
                writer.FillInOffsetLong($"Entity{index}Offset");

                // Write an unknown sequence of bytes that is always 00 00 00 00 10 01 0A 57 00 00 00 00
                writer.Write(0);
                writer.Write(0x570A0110);
                writer.Write(0);

                // Write the index of this entity's name.
                writer.Write((ulong)names.IndexOf(EntityName));

                // Write two unknown values of 0xFFFFFFFF.
                // TODO: Some files have this as 0s instead, does it matter?
                writer.Write(0xFFFFFFFF);
                writer.Write(0xFFFFFFFF);

                // Write the first unknown integer value.
                writer.Write(UnknownUInt32_1);

                // Write an unknown boolean value.
                if (writer.IsBigEndian)
                {
                    writer.WriteNulls(0x03);
                    writer.Write(UnknownBoolean_1);
                }
                else
                {
                    writer.Write(UnknownBoolean_1);
                    writer.FixPadding(0x04);
                }

                // Write this entity's colour modifier.
                writer.Write(ColourModifier);

                // Write this entity's horizontal texture scroll speed value.
                writer.Write(HorizontalTextureScrollSpeed);

                // Write this entity's vertical texture scroll speed value.
                writer.Write(VerticalTextureScrollSpeed);

                // Write three unknown floating point values that are always 0.5.
                writer.Write(0.5f);
                writer.Write(0.5f);
                writer.Write(0.5f);

                // Write three unknown values that are always 1.
                writer.Write(1L);
                writer.Write(1L);
                writer.Write(1L);

                // Write two unknown values of 0xFFFFFFFF.
                writer.Write(0xFFFFFFFF);
                writer.Write(0xFFFFFFFF);

                // Write an unknown floating point value of 0.463772833.
                writer.Write(0.463772833f);

                // Write an unknown value of 0x30.
                writer.Write([0x30, 0x00, 0x00, 0x00]);

                // Write this entity's position.
                writer.Write(Position);

                // Write this entity's scale.
                writer.Write(Scale);

                // Write this entity's rotation.
                writer.Write(Rotation);

                // Write an unknown sequenece of bytes that is always 0B 01 8A A1
                writer.Write(0xA18A010B);

                // Write an unknown value of 0x30.
                writer.Write([0x30, 0x00, 0x00, 0x00]);

                // Write this entity's layer index.
                writer.Write(LayerIndex);

                // Write an unknown sequence of bytes that is always 00 00 00 00 1A 52 9A 1A 30 00 00 00
                writer.Write(0);
                writer.Write(0x1A9A521A);
                writer.Write([0x30, 0x00, 0x00, 0x00]);

                // Write the index of this entity's model name.
                writer.Write((ulong)names.IndexOf(ModelName));

                // Realign to 0x40 bytes.
                writer.FixPadding(0x40);
            }
        }

        // Actual data presented to the end user.
        public Entity[] Data = [];

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public void Load(string filepath, bool bigEndian = false)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), bigEndian);

            // Skip an unknown value that is always 0.
            reader.CheckValue(0x00);

            // Read the count of entities this environment file defines.
            Data = new Entity[reader.ReadUInt32()];

            // Skip an unknown value that is always 0x40. Likely an offset to the entity table.
            reader.CheckValue(0x40L);

            // Read the count of names in this file.
            uint entityNameCount = reader.ReadUInt32();
            reader.CheckValue(0x00);

            // Read the offset to the name table.
            long entityNameTableOffset = reader.ReadInt64();

            // Realign to 0x40 bytes.
            reader.FixPadding(0x40);

            // Loop through and read each entity.
            for (int entityIndex = 0; entityIndex < Data.Length; entityIndex++)
                Data[entityIndex] = new(reader, entityNameTableOffset);

            // Close our BinaryWriter.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath, bool bigEndian = false)
        {
            // Create this file through a BinaryWriter.
            ExtendedBinaryWriter writer = new(File.Create(filepath), bigEndian);

            // Set up a list of names used by this file so we don't end up writing the same string multiple times.
            List<string> names = [];

            // Loop through each entity.
            foreach (Entity entity in Data)
            {
                // Add this entity's name to the list if it isn't already there.
                if (!names.Contains(entity.EntityName))
                    names.Add(entity.EntityName);

                // Add this entity's model name to the list if it isn't already there.
                if (!names.Contains(entity.ModelName))
                    names.Add(entity.ModelName);
            }

            // Write an unknown value that is always 0.
            writer.Write(0);

            // Write the count of entities in this file.
            writer.Write(Data.Length);

            // Write a value that is always 0x40. This is likely an offset, but as there's no table like a BINA Format, I don't need to worry about it.
            writer.Write(0x40L);

            // Write the count of names in this file.
            writer.Write(names.Count);

            writer.Write(0x00);

            // Add an offset for the entity name table.
            writer.AddOffset("EntityNameTableOffset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Add offsets for all the entities.
            for (int entityIndex = 0; entityIndex < Data.Length; entityIndex++)
                writer.AddOffset($"Entity{entityIndex}Offset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through each entity.
            for (int entityIndex = 0; entityIndex < Data.Length; entityIndex++)
                Data[entityIndex].Write(writer, entityIndex, names);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Fill in the offset for this file's name table.
            writer.FillInOffsetLong("EntityNameTableOffset");

            // Add offsets for all the names.
            for (int nameIndex = 0; nameIndex < names.Count; nameIndex++)
                writer.AddOffset($"Name{nameIndex}Offset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through each name.
            for (int nameIndex = 0; nameIndex < names.Count; nameIndex++)
            {
                // Fill in this name's offset.
                writer.FillInOffsetLong($"Name{nameIndex}Offset");

                // Write the length of this name.
                writer.Write(names[nameIndex].Length);

                // Write this name's string.
                writer.WriteNullTerminatedString(names[nameIndex]);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Close our BinaryReader.
            writer.Close();
        }
    }
}
