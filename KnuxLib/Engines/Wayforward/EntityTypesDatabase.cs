using System.Diagnostics;

namespace KnuxLib.Engines.Wayforward
{
    public class EntityTypesDatabase : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public EntityTypesDatabase() { }
        public EntityTypesDatabase(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.entitytypesdatabase.json", Data);
        }

        // Classes for this format.
        public class FormatData
        {
            public Entity[] Entities { get; set; } = Array.Empty<Entity>();

            public string[] Strings { get; set; } = Array.Empty<string>();
        }

        public class Entity
        {
            public uint TypeA { get; set; }

            public uint TypeB { get; set; }

            public string Name { get; set; } = "";

            // TODO: Read everything else.

            public override string ToString() => Name;
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

            // Skip an unknown value that is always 0.
            reader.JumpAhead(0x04);

            // Read this file's entity count.
            uint entityCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0x40. Likely an offset to the entity table.
            reader.JumpAhead(0x08);

            // Read this LVB file's string count.
            ulong stringCount = reader.ReadUInt64();

            // Read the offset to this file's string table.
            long stringTableOffset = reader.ReadInt64();

            reader.FixPadding(0x40);

            Data.Entities = new Entity[entityCount];
            Data.Strings = new string[stringCount];

            // Loop through each entity in this file.
            for (int entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                // Create a new entity.
                Entity entity = new();

                // Read this entity's offset.
                long entityOffset = reader.ReadInt64();

                // Save our position to jump back for the next entity.
                long position = reader.BaseStream.Position;

                // Jump to this entity's offset.
                reader.JumpTo(entityOffset);

                entity.TypeA = reader.ReadUInt32();

                uint unknown2 = reader.ReadUInt32();

                entity.TypeB = reader.ReadUInt32();

                uint unknown3 = reader.ReadUInt32();

                entity.Name = Helpers.ReadWayforwardLengthPrefixedString(reader, stringTableOffset, reader.ReadUInt32());

                // Jump back for our next entity.
                reader.JumpTo(position);

                // Save this entity.
                Data.Entities[entityIndex] = entity;
            }

            // Jump to this file's string table.
            reader.JumpTo(stringTableOffset);

            // Loop through each string in this file.
            for (ulong stringIndex = 0; stringIndex < stringCount; stringIndex++)
            {
                // Read this string's offset.
                long stringOffset = reader.ReadInt64();

                // Save our position to jump back for the next string.
                long position = reader.BaseStream.Position;

                // Jump to this string's offset.
                reader.JumpTo(stringOffset);

                // Read this string, prefixed with its length.
                Data.Strings[stringIndex] = reader.ReadNullPaddedString(reader.ReadInt32());

                // Jump back for our next string.
                reader.JumpTo(position);
            }
        }
    }
}
