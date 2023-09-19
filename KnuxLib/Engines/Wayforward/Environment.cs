namespace KnuxLib.Engines.Wayforward
{
    public class Environment : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Environment() { }
        public Environment(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.environment.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            /// <summary>
            /// The entities in this file.
            /// </summary>
            public Entity[] Entities { get; set; } = Array.Empty<Entity>();

            /// <summary>
            /// The names for entities in this file.
            /// </summary>
            public string[] Names { get; set; } = Array.Empty<string>();
        }

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
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// An unknown floating point value.
            /// TODO: What is this?
            /// TODO: Caused some weird rotational stuff?
            /// </summary>
            public float UnknownFloat_1 { get; set; }

            /// <summary>
            /// The index of the layer (definied in the stage's LGB file) that this entity is part of.
            /// </summary>
            public uint LayerIndex { get; set; }

            /// <summary>
            /// The name of the wf3d mesh this entity should use.
            /// </summary>
            public string ModelName { get; set; } = "";

            public override string ToString() => EntityName;
        }

        // Actual data presented to the end user.
        public Entity[] Data = Array.Empty<Entity>();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read the count of entities this environment file defines.
            uint entityCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0x40. Likely an offset to the entity table.
            reader.JumpAhead(0x08);

            // Read the count of names in this file.
            ulong entityNameCount = reader.ReadUInt64();

            // Read the offset to the name table.
            long entityNameTableOffset = reader.ReadInt64();

            // Realign to 0x40 bytes.
            reader.FixPadding(0x40);

            // Create the entity array.
            Data = new Entity[entityCount];

            // Loop through and read each entity.
            for (ulong i = 0; i < entityCount; i++)
            {
                // Create a new entity entry.
                Entity entity = new();

                // Read the offset for this entity's data.
                long entityOffset = reader.ReadInt64();

                // Save our position to jump back for the next entity.
                long position = reader.BaseStream.Position;

                // Jump to this entity's data.
                reader.JumpTo(entityOffset);

                // Skip an unknown sequence of bytes that is always 00 00 00 00 10 01 0A 57 00 00 00 00
                reader.JumpAhead(0x0C);

                // Read the entity's name.
                entity.EntityName = Helpers.ReadWayforwardLengthPrefixedString(reader, entityNameTableOffset, reader.ReadUInt32());

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Skip two unknown values that are either 0 or 0xFFFFFFFF.
                // TODO: Does this matter?
                reader.JumpAhead(0x08);

                // Read the first unknown integer value.
                entity.UnknownUInt32_1 = reader.ReadUInt32();

                // Read an unknown boolean value.
                entity.UnknownBoolean_1 = reader.ReadBoolean(0x04);

                // Read this entity's colour modifer.
                entity.ColourModifier = reader.ReadVector3();

                // Read this entity's horizontal texture scroll speed value.
                entity.HorizontalTextureScrollSpeed = reader.ReadSingle();

                // Read this entity's vertical texture scroll speed value.
                entity.VerticalTextureScrollSpeed = reader.ReadSingle();

                // Skip three unknown floating point values that are always 0.5.
                reader.JumpAhead(0x0C);

                // Skip three unknown values that are always 1.
                reader.JumpAhead(0x18);

                // Skip two unknown values that are always 0xFFFFFFFF.
                reader.JumpAhead(0x08);

                // Skip an unknown floating point value that is always 0.463772833.
                reader.JumpAhead(0x04);

                // Skip an unknown value of 0x30.
                reader.JumpAhead(0x04);

                // Read the entity's position.
                entity.Position = reader.ReadVector3();

                // Read the entity's scale.
                entity.Scale = reader.ReadVector3();

                // Read the entity's rotation.
                entity.Rotation = reader.ReadVector3();

                // Read the first unknown floating point value.
                entity.UnknownFloat_1 = reader.ReadSingle();

                // Skip an unknown sequenece of bytes that is always 0B 01 8A A1
                reader.JumpAhead(0x04);

                // Skip an unknown value of 0x30.
                reader.JumpAhead(0x04);

                // Read this entity's layer index.
                entity.LayerIndex = reader.ReadUInt32();

                // Skip an unknown sequence of bytes that is always 00 00 00 00 1A 52 9A 1A 30 00 00 00
                reader.JumpAhead(0x0C);

                // Read the entity's model name.
                entity.ModelName = Helpers.ReadWayforwardLengthPrefixedString(reader, entityNameTableOffset, reader.ReadUInt32());

                // Save this entity.
                Data[i] = entity;

                // Jump back for the next entity.
                reader.JumpTo(position);
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

            // Set up a list of names used by this file so we don't end up writing the same string multiple times.
            List<string> names = new();

            // Loop through each entity.
            foreach (var entity in Data)
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
            writer.Write((ulong)names.Count);

            // Add an offset for the entity name table.
            writer.AddOffset("EntityNameTableOffset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Add offsets for all the entities.
            for (int i = 0; i < Data.Length; i++)
                writer.AddOffset($"Entity{i}Offset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through each entity.
            for (int i = 0; i < Data.Length; i++)
            {
                // Fill in this entity's offset.
                writer.FillOffset($"Entity{i}Offset");

                // Write an unknown sequence of bytes that is always 00 00 00 00 10 01 0A 57 00 00 00 00
                writer.Write(0);
                writer.Write(0x570A0110);
                writer.Write(0);

                // Write the index of this entity's name.
                writer.Write(names.IndexOf(Data[i].EntityName));

                // Write an unknown value of 0.
                writer.Write(0);

                // Write two unknown values of 0xFFFFFFFF.
                // TODO: Some files have this as 0s instead, does it matter?
                writer.Write(0xFFFFFFFF);
                writer.Write(0xFFFFFFFF);

                // Write the first unknown integer value.
                writer.Write(Data[i].UnknownUInt32_1);

                // Write an unknown boolean value.
                writer.Write(Data[i].UnknownBoolean_1);
                writer.FixPadding(0x04);

                // Write this entity's colour modifier.
                writer.Write(Data[i].ColourModifier);

                // Write this entity's horizontal texture scroll speed value.
                writer.Write(Data[i].HorizontalTextureScrollSpeed);

                // Write this entity's vertical texture scroll speed value.
                writer.Write(Data[i].VerticalTextureScrollSpeed);

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
                writer.Write(0x30);

                // Write this entity's position.
                writer.Write(Data[i].Position);

                // Write this entity's scale.
                writer.Write(Data[i].Scale);

                // Write this entity's rotation.
                writer.Write(Data[i].Rotation);

                // Write this entity's first unknown floating point value.
                writer.Write(Data[i].UnknownFloat_1);

                // Write an unknown sequenece of bytes that is always 0B 01 8A A1
                writer.Write(0xA18A010B);

                // Write an unknown value of 0x30.
                writer.Write(0x30);

                // Write this entity's layer index.
                writer.Write(Data[i].LayerIndex);

                // Write an unknown sequence of bytes that is always 00 00 00 00 1A 52 9A 1A 30 00 00 00
                writer.Write(0);
                writer.Write(0x1A9A521A);
                writer.Write(0x30);

                // Write the index of this entity's model name.
                writer.Write(names.IndexOf(Data[i].ModelName));

                // Realign to 0x40 bytes.
                writer.FixPadding(0x40);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Fill in the offset for this file's name table.
            writer.FillOffset("EntityNameTableOffset");

            // Add offsets for all the names.
            for (int i = 0; i < names.Count; i++)
                writer.AddOffset($"Name{i}Offset", 0x08);

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Loop through each name.
            for (int i = 0; i < names.Count; i++)
            {
                // Fill in this name's offset.
                writer.FillOffset($"Name{i}Offset");

                // Write the length of this name.
                writer.Write(names[i].Length);

                // Write this name's string.
                writer.WriteNullTerminatedString(names[i]);

                // Realign to 0x08 bytes.
                writer.FixPadding(0x08);
            }

            // Realign to 0x40 bytes.
            writer.FixPadding(0x40);

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
