namespace KnuxLib.Engines.Wayforward
{
    // TODO: Everything.
    public class LevelBinary : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public LevelBinary() { }
        public LevelBinary(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.wayforward.levelbinary.json", Data);
        }

        public class FormatData
        {
            public Entity[] Entities { get; set; } = Array.Empty<Entity>();
            
            public Spline[] Splines { get; set; } = Array.Empty<Spline>();

            // TODO: Throw these out when the entites are all read, as I don't want to deal with this shit.
            public string[] Strings { get; set; } = Array.Empty<string>();
        }

        public class Entity
        {
            public uint Type { get; set; }

            public string Name { get; set; } = "";

            // TODO: Read everything else.

            public override string ToString() => Name;
        }

        public class Spline
        {
            public uint Flags { get; set; } // ?

            public string Name { get; set; } = "";

            // TODO: Read everything else.

            public override string ToString() => Name;
        }

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

            // Read this LVB file's object count.
            uint objectCount = reader.ReadUInt32();

            // Skip an unknown value that is always 0x40. Likely an offset to the object table.
            reader.JumpAhead(0x08);

            // Read this LVB file's spline count.
            ulong splineCount = reader.ReadUInt64();

            // Read the offset to this file's spline table.
            long splineTableOffset = reader.ReadInt64();

            // Skip an unknown count that is always 0 and its associated offset.
            reader.JumpAhead(0x10);

            // Read this LVB file's string count.
            ulong stringCount = reader.ReadUInt64();

            // Read the offset to this file's string table.
            long stringTableOffset = reader.ReadInt64();

            // Define this file's data arrays.
            Data.Entities = new Entity[objectCount];
            Data.Splines = new Spline[splineCount];
            Data.Strings = new string[stringCount];

            // Loop through each object in this file.
            for (int objectIndex = 0; objectIndex < objectCount; objectIndex++)
            {
                // Create a new entity.
                Entity entity = new();

                // Read this object's offset.
                long objectOffset = reader.ReadInt64();

                // Save our position to jump back for the next object.
                long position = reader.BaseStream.Position;

                // Jump to this object's offset.
                reader.JumpTo(objectOffset);

                // Print stuff for an unknown value.
                Console.WriteLine($"TODO: Unknown 0x00 value of {reader.ReadUInt32()}");

                // Read this entity's type?
                entity.Type = reader.ReadUInt32();

                // Print stuff for an unknown value.
                Console.WriteLine($"TODO: Unknown 0x0C value of {reader.ReadUInt32()}");

                // Read this entity's name.
                entity.Name = Helpers.ReadWayforwardLengthPrefixedString(reader, stringTableOffset, reader.ReadUInt32());

                // Skip an unknown value of 0.
                reader.JumpAhead(0x04);

                // Skip an unknown value of -1.
                reader.JumpAhead(0x08);

                // TODO: This is where everything falls apart.
                    // How the hell do these objects work???
                    // Q_Q

                // Jump back for our next object.
                reader.JumpTo(position);

                // Save this entity.
                Data.Entities[objectIndex] = entity;
            }

            // Jump to this file's spline table.
            reader.JumpTo(splineTableOffset);

            // Loop through each spline in this file.
            for (ulong splineIndex = 0; splineIndex < splineCount; splineIndex++)
            {
                // Create a new spline.
                Spline spline = new();

                // Read this spline's offset.
                long splineOffset = reader.ReadInt64();

                // Save our position to jump back for the next spline.
                long position = reader.BaseStream.Position;

                // Jump to this spline's offset.
                reader.JumpTo(splineOffset);

                // Read this spline's flags.
                spline.Flags = reader.ReadUInt32();

                // Read this spline's name.
                spline.Name = reader.ReadNullPaddedString(0x20);

                // TODO: Read the rest of the spline data.

                // Jump back for our next spline.
                reader.JumpTo(position);

                // Save this spline.
                Data.Splines[splineIndex] = spline;
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
