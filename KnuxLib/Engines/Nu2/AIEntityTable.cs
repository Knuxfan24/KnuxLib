﻿namespace KnuxLib.Engines.Nu2
{
    public class AIEntityTable : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public AIEntityTable() { }
        public AIEntityTable(string filepath, FormatVersion version = FormatVersion.GameCube, bool export = false)
        {
            Load(filepath, version);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.nu2.aientitytable.json", Data);
        }

        // Classes for this format.
        public enum FormatVersion
        {
            GameCube = 0,
            PlayStation2Xbox = 1
        }

        public class AIEntity
        {
            // The type of entity this is.
            public string Type { get; set; } = "";

            // The points in space this entity needs to reference.
            public List<Vector3> Positions { get; set; } = new();

            public override string ToString() => Type;
        }

        // Actual data presented to the end user.
        public List<AIEntity> Data = new();

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        /// <param name="version">The system version to read this file as.</param>
        public void Load(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryReader.
            BinaryReaderEx reader = new(File.OpenRead(filepath));

            // If this is a GameCube ai file, then switch the reader's endianness to big endian.
            if (version == FormatVersion.GameCube)
                reader.IsBigEndian = true;

            // Red this file's entity count.
            uint entityCount = reader.ReadUInt32();

            // Loop through each entity in this file.
            for (int entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                // Set up a new entity entry.
                AIEntity entity = new();

                // Read this entity's type.
                entity.Type = reader.ReadNullPaddedString(0x10);

                // Read this entity's position count.
                uint entityPositionCount = reader.ReadUInt32();

                // Read each of this entity's position values.
                for (int entityPositionIndex = 0; entityPositionIndex < entityPositionCount; entityPositionIndex++)
                    entity.Positions.Add(reader.ReadVector3());

                // Save this entity.
                Data.Add(entity);
            }

            // Close Marathon's BinaryReader.
            reader.Close();
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        /// <param name="version">The system version to save this file as.</param>
        public void Save(string filepath, FormatVersion version = FormatVersion.GameCube)
        {
            // Set up Marathon's BinaryWriter.
            BinaryWriterEx writer = new(File.Create(filepath));

            // If this is a GameCube ai file, then switch the writer's endianness to big endian.
            if (version == FormatVersion.GameCube)
                writer.IsBigEndian = true;

            // Write the count of entities in this file.
            writer.Write(Data.Count);

            // Loop through each entity in this file.
            for (int dataIndex = 0; dataIndex < Data.Count; dataIndex++)
            {
                // Write this entity's type.
                writer.WriteNullPaddedString(Data[dataIndex].Type, 0x10);

                // Write the amount of position values this entity uses.
                writer.Write(Data[dataIndex].Positions.Count);
                
                // Write each of this entity's position values.
                foreach (Vector3 position in Data[dataIndex].Positions)
                    writer.Write(position);
            }

            // Close Marathon's BinaryWriter.
            writer.Close();
        }
    }
}
