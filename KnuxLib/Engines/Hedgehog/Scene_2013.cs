using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    public class Scene_2013 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public Scene_2013() { }
        public Scene_2013(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.hedgehog.scene_2013.json", Data);
        }

        // Classes for this format.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GlobalIlluminationMode : byte
        {
            Normal = 0x00,
            Only = 0x01,
            None = 0x02,
            Shadow = 0x03,
            Seperated = 0x04
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LightFieldMode : byte
        {
            Normal = 0x00,
            Only = 0x01,
            None = 0x02
        }

        public class FormatData
        {
            public float GammaTVWiiU { get; set; }

            public float GammaDRCWiiU { get; set; }

            public bool FixedLDR { get; set; }

            public GlobalIlluminationMode GlobalIlluminationMode { get; set; }

            public LightFieldMode LightFieldMode { get; set; }

            public bool DrawLightFieldSamplingPoints { get; set; }

            public bool UpdateLightFieldEachFrame { get; set; }

            public bool DrawLightFieldRegion { get; set; }

            public uint ScreenshotLargeScale { get; set; }

            public bool DrawFXColGeometry { get; set; }

            public bool DrawFXColName { get; set; }

            public bool DrawLocalLightSphere { get; set; }

            public FXParameter[] Parameters { get; set; } = new FXParameter[4];
        }

        public class FXParameter
        {
            public FXParameterCulling Culling { get; set; } = new();

            public FXParameterScene Scene { get; set; } = new();
        }

        public class FXParameterCulling
        {
            public float Default { get; set; }

            public float Near { get; set; }

            public float Middle { get; set; }

            public float Far { get; set; }
        }

        public class FXParameterScene
        {
            public float SkyIntensistyScale { get; set; }

            public float SkyFollowupRatioY { get; set; }

            public bool PseudoFogEnabled { get; set; }

            public bool PseudoFogWithoutFar { get; set; }
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

            Data.GammaTVWiiU = reader.ReadSingle();

            Data.GammaDRCWiiU = reader.ReadSingle();

            Data.FixedLDR = reader.ReadBoolean();

            Data.GlobalIlluminationMode = (GlobalIlluminationMode)reader.ReadByte();

            Data.LightFieldMode = (LightFieldMode)reader.ReadByte();

            Data.DrawLightFieldSamplingPoints = reader.ReadBoolean();

            Data.UpdateLightFieldEachFrame = reader.ReadBoolean();

            Data.DrawLightFieldRegion = reader.ReadBoolean();

            reader.FixPadding(0x04);

            Data.ScreenshotLargeScale = reader.ReadUInt32();

            Data.DrawFXColGeometry = reader.ReadBoolean();

            Data.DrawFXColName = reader.ReadBoolean();

            Data.DrawLocalLightSphere = reader.ReadBoolean();

            reader.FixPadding(0x10);

            for (int parameterIndex = 0; parameterIndex < 4; parameterIndex++)
            {
                Data.Parameters[parameterIndex] = new();

                Data.Parameters[parameterIndex].Culling.Default = reader.ReadSingle();
                Data.Parameters[parameterIndex].Culling.Near = reader.ReadSingle();
                Data.Parameters[parameterIndex].Culling.Middle = reader.ReadSingle();
                Data.Parameters[parameterIndex].Culling.Far = reader.ReadSingle();
            }
        }
    }
}
