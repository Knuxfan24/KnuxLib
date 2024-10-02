namespace KnuxLib.Engines.Wayforward
{
    // TODO: Format saving.
    // TODO: Figure out the unknown behaviour flags for the different versions of this format.
        // Especially the Ducktales Remastered ones, as I've never even played it and am trying to go off videos at a quick glance.
    // TODO: Figure out the unknown values in non Shantae: Half Genie Hero versions of this format.
    // TODO: Figure out the massive chunk of data that controls screen transitions(?) in Shantae and the Seven Sirens.
    // TODO: Format importing.
    public class Collision : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Collision() { }
        public Collision(string filepath, FormatVersion version = FormatVersion.hero, bool bigEndian = false, bool export = false)
        {
            // Load this file.
            Load(filepath, version, bigEndian);

            // If the export flag is set, then export this format.
            if (export)
                ExportOBJ($@"{Helpers.GetExtension(filepath, true)}.obj");
        }

        // Classes for this format.
        public enum FormatVersion
        {
            /// <summary>
            /// Ducktales Remastered.
            /// </summary>
            duck = 0,

            /// <summary>
            /// Shantae: Half-Genie Hero.
            /// </summary>
            hero = 1,

            /// <summary>
            /// Shantae and the Seven Sirens.
            /// </summary>
            sevensirens = 2
        }

        [Flags]
        public enum Behaviour_Duck
        {
            // TODO: Some models in exeanm.clb have a behaviour value of 0?
            Solid         = 0x00000001,
            Unknown1      = 0x00000010,
            BottomlessPit = 0x00000020,
            Water         = 0x00000040,
            Unknown2      = 0x00000080, // Area transition maybe? Just based on placements.
            Rail          = 0x00000100, // Not all rails seem to have this? Just some end pieces?
            Unknown3      = 0x00000800, // Only used on a single block in vesuvius.clb
            Snow          = 0x00100000, // Only used through himalayas.clb, verify that this is correct.
            Ice           = 0x00200000, // Only used through himalayas.clb, verify that this is correct.
            Unknown4      = 0x00800000  // Only used on two platforms in amazon.clb, one block in himalayas.clb and a roof in moon.clb
        }

        [Flags]
        public enum Behaviour_Hero : uint
        {
            Solid         = 0x00000001,
            TopSolid      = 0x00000002,
            Boundry       = 0x00000004, // TODO: Confirm
            Spikes        = 0x00000008,
            NoMonkey      = 0x00000010,
            BottomlessPit = 0x00000020,
            Unknown1      = 0x00000100,
            WoodSound     = 0x00000400,
            Unknown2      = 0x00004000, // Often found out of bounds?
            Slide         = 0x00400000,
            Unknown3      = 0x00800000, // Only used on a single block at the top of Mermaid Falls Act 1? Something to do with the mouse maybe?
            Unknown4      = 0x01000000, // Used in Tassle Town Act 1 and Hypno Baron's Castle Act 1 for the down left slopes. Also found on some walls in the Intro Caves.
            Unknown5      = 0x02000000, // Used in Tassle Town Act 1 and Hypno Baron's Castle Act 1 for the down right slopes. Also found on a single wall in the Intro Workshop.
            Unknown6      = 0x10000000, // Used in a couple of odd spots in Burning Town Act 1 and multiple acts of Risky's Hideout.
            Lava          = 0x80000000  // TODO: Confirm, only assumed based on placement, as its all over Risky's Hideout and nowhere else.
        }

        [Flags]
        public enum Behaviour_SevenSirens
        {
            Solid         = 0x00000001,
            TopSolid      = 0x00000002,
            Unknown1      = 0x00000005, // Often placed on weird corners? Always has the NoNewt tag as well for some reason.
            Spikes        = 0x00000008,
            NoNewt        = 0x00000010,
            BottomlessPit = 0x00000020,
            DamageZone    = 0x00000040, // Has to be paired with Water to use.
            HealingZone   = 0x00000400, // Has to be paired with Water to use.
            DrillZone     = 0x00008000, // How does this one actually work?
            Water         = 0x00200000,
            Unknown2      = 0x10000000, // Seems to be some sort of door way almost? Just based on placement.
            Unknown3      = 0x20000000  // Only used on four pillars in level_labyrinth_02.clb.
        }

        public class FormatData
        {
            public Model[] Models { get; set; } = [];

            public byte[]? ScreenTransition { get; set; }
        }

        public class Model
        {
            /// <summary>
            /// This model's axis aligned bounding box.
            /// </summary>
            public AABB? AxisAlignedBoundingBox { get; set; }

            /// <summary>
            /// An unknown Vector3 value that is only present in Ducktales Remastered.
            /// TODO: What is this? Do this and the next value replace the AABB in some way?
            /// </summary>
            public Vector3? UnknownVector3_1 { get; set; }

            /// <summary>
            /// An unknown integer value that is only present in Ducktales Remastered.
            /// TODO: What is this? Do this and the last value replace the AABB in some way?
            /// </summary>
            public uint? UnknownUInt32_1 { get; set; }

            /// <summary>
            /// The behaviour/surface type of this collision.
            /// TODO: Figure out the values for each type.
            /// </summary>
            public object Behaviour { get; set; }

            /// <summary>
            /// An unknown integer value.
            /// TODO: What is this? It's always 0 except for a couple of instances in Seven Sirens.
            /// </summary>
            public uint UnknownUInt32_2 { get; set; }

            /// <summary>
            /// An unknown 64 bit(?) integer value that is only present in Seven Sirens.
            /// TODO: What is this?
            /// </summary>
            public ulong? UnknownULong_1 { get; set; }

            /// <summary>
            /// The coordinates for the various vertices that make up this model.
            /// </summary>
            public Vector3[] Vertices { get; set; } = [];
            
            /// <summary>
            /// The faces that make up this model.
            /// </summary>
            public Face[] Faces { get; set; } = [];

            /// <summary>
            /// Initialises this model with default data.
            /// </summary>
            public Model() { }

            /// <summary>
            /// Initialises this model by reading its data from a BinaryReader.
            /// </summary>
            public Model(ExtendedBinaryReader reader, FormatVersion version) => Read(reader, version);

            /// <summary>
            /// Reads the data for this model.
            /// </summary>
            public void Read(ExtendedBinaryReader reader, FormatVersion version)
            {
                // If this isn't a Ducktales Remastered format, then read this model's axis aligned bounding box.
                if (version != FormatVersion.duck)
                    AxisAlignedBoundingBox = new(reader);

                // Check if this is a Ducktales Remastered format.
                else
                {
                    // Read an unknown Vector3.
                    UnknownVector3_1 = reader.ReadVector3();

                    // Read an unknown integer value.
                    UnknownUInt32_1 = reader.ReadUInt32();
                }

                // Read this model's behaviour flag, depending on the format version.
                switch (version)
                {
                    case FormatVersion.duck: Behaviour = (Behaviour_Duck)reader.ReadUInt32(); break;
                    case FormatVersion.hero: Behaviour = (Behaviour_Hero)reader.ReadUInt32(); break;
                    case FormatVersion.sevensirens: Behaviour = (Behaviour_SevenSirens)reader.ReadUInt32(); break;
                    default: Behaviour = reader.ReadUInt32(); break;
                }

                // Read an unknown integer value.
                UnknownUInt32_2 = reader.ReadUInt32();

                // If this is a Shantae and the Seven Sirens format, then read an unknown long value.
                if (version == FormatVersion.sevensirens)
                    UnknownULong_1 = reader.ReadUInt64();

                // Read the offset to this model's data.
                ulong modelDataOffset = reader.ReadUInt64();

                // Save our current position so we can jump back for the next model.
                long position = reader.BaseStream.Position;

                // Jump to this model's data.
                reader.JumpTo(modelDataOffset);

                // Check for a value of 0.
                reader.CheckValue(0x00);

                // Initialise this model's vertex array.
                Vertices = new Vector3[reader.ReadUInt32()];

                // Read the offset to this model's vertex table, additive to modelDataOffset. This seems to always be 0x20.
                ulong vertexTableOffset = reader.ReadUInt64();

                // Check for a value of 0.
                reader.CheckValue(0x00);

                // Initialise this model's face array.
                Faces = new Face[reader.ReadUInt32()];

                // Read the offset to this model's face table, additive to modelDataOffset.
                ulong faceTableOffset = reader.ReadUInt64();

                // Jump to this model's vertex table.
                reader.JumpTo(modelDataOffset + vertexTableOffset);

                // Loop through and read all of this model's vertices.
                for (int vertexIndex = 0; vertexIndex < Vertices.Length; vertexIndex++)
                    Vertices[vertexIndex] = reader.ReadVector3();

                // Jump to this model's face table.
                reader.JumpTo(modelDataOffset + faceTableOffset);

                // Loop through and read each of this model's faces.
                for (int faceIndex = 0; faceIndex < Faces.Length; faceIndex++)
                {
                    Faces[faceIndex] = new()
                    {
                        IndexA = reader.ReadUInt32(),
                        IndexB = reader.ReadUInt32(),
                        IndexC = reader.ReadUInt32()
                    };
                }

                // Jump back for the next model.
                reader.JumpTo(position);
            }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The format version to read this file as.</param>
        /// <param name="bigEndian">Whether we need to read this file in big endian or not.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.hero, bool bigEndian = false)
        {
            // Load this file into a BinaryReader.
            ExtendedBinaryReader reader = new(File.OpenRead(filepath), bigEndian);

            // Skip an unknown value that is always 0.
            reader.CheckValue(0x00);

            // Initialise this file's model array.
            Data.Models = new Model[reader.ReadUInt32()];

            // Read the offset to this file's model table.
            ulong modelTableOffset = reader.ReadUInt64();

            // Read whether or not this file has any screen transition data. It seems that only Shantae and the Seven Sirens does?
            bool hasScreenTransitionData = reader.ReadBoolean(0x08);

            // Read the offset to this file's screen transition data, will be 0 if it has none.
            ulong screenTransitionTableOffset = reader.ReadUInt64();

            // Realign to 0x40 bytes.
            reader.FixPadding(0x40);

            // Jump to this file's model table offset.
            reader.JumpTo(modelTableOffset);

            // Loop through and read each model.
            for (int modelIndex = 0; modelIndex < Data.Models.Length; modelIndex++)
                Data.Models[modelIndex] = new(reader, version);

            // Check if this file has screen transition data.
            if (hasScreenTransitionData)
            {
                // Jump to the offset for this data.
                reader.JumpTo(screenTransitionTableOffset);

                // TODO: Actually read this data properly.

                // Calculate the length of this file's screen transition data.
                uint length = (uint)(reader.BaseStream.Length - reader.BaseStream.Position);

                // Read the bytes that make up this file's screen transition data.
                Data.ScreenTransition = reader.ReadBytes(length);
            }

            // Close our BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Exports this collision's model data to an OBJ file.
        /// TODO: Figure out what I want to do with the screen transition data when it comes to this.
        /// TODO: Figure out how I want to present the tags, the @ system is what I want but 3DS Max's OBJ importer is stupid and changes most special characters to an underscore.
        /// </summary>
        /// <param name="filepath">The filepath to export to.</param>
        public void ExportOBJ(string filepath)
        {
            // Set up an integer to keep track of the amount of vertices.
            int vertexCount = 0;

            // Create the StreamWriter.
            StreamWriter obj = new(filepath);

            // Loop through each model.
            for (int modelIndex = 0; modelIndex < Data.Models.Length; modelIndex++)
            {
                // Write the Vertex Comment for this model.
                obj.WriteLine($"# Model {modelIndex} Vertices\r\n");

                // Write each vertex.
                foreach (Vector3 vertex in Data.Models[modelIndex].Vertices)
                    obj.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");

                // Write the Name/Behaviour Tags Comment for this model.
                obj.WriteLine($"\r\n# Model {modelIndex} Name and Behaviour Tags\r\n");

                // Split the flags for this model.
                string flags = $"@{string.Join('@', Data.Models[modelIndex].Behaviour.ToString().Split(", "))}";

                // Write this model's name and flags.
                obj.WriteLine($"g model{modelIndex}{flags}");
                obj.WriteLine($"o model{modelIndex}{flags}");

                // Write the Faces Comment for this model.
                obj.WriteLine($"\r\n# Model {modelIndex} Faces\r\n");

                // Write each face for this model, with the indices incremented by 1 (and the current value of vertexCount) due to OBJ counting from 1 not 0.
                foreach (Face face in Data.Models[modelIndex].Faces)
                    obj.WriteLine($"f {face.IndexA + 1 + vertexCount} {face.IndexB + 1 + vertexCount} {face.IndexC + 1 + vertexCount}");

                // Add the amount of vertices in this model to the count.
                vertexCount += Data.Models[modelIndex].Vertices.Length;

                // Write an empty line for neatness.
                obj.WriteLine();
            }

            // Close the StreamWriter.
            obj.Close();
        }
    }
}
