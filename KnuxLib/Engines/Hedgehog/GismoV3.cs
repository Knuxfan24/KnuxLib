using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KnuxLib.Engines.Hedgehog
{
    // Based on: https://github.com/blueskythlikesclouds/RflTemplates/blob/master/SonicFrontiers/Uncategorized/GismoConfigDesignData.bt
    // TODO: Is V3 the right name for this? Assuming Lost World is V1 and Forces is V2.
    public class GismoV3 : FileBase
    {
        // Generic VS stuff to allow creating an object that instantly loads a file.
        public GismoV3() { }
        public GismoV3(string filepath, bool export = false)
        {
            Load(filepath);

            if (export)
                JsonSerialise($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}.json", Data);
        }

        // Classes for this format.
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignShapeType : byte
        {
            Box = 0,
            Sphere = 1,
            Capsule = 2,
            Cylinder = 3,
            Mesh = 4,
            None = 5
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignBasePoint : byte
        {
            Centre = 0,
            ZPlane = 1,
            XPlane = 2,
            YPlane = 3
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignRigidBodyType : byte
        {
            None = 0,
            Static = 1,
            Dynamic = 2
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignRigidBodyMaterial : byte
        {
            None = 0,
            Wood = 1,
            Iron = 2
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignMotionType : byte
        {
            Swing = 0,
            Rotate = 1,
            LinearSwing = 2
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoDesignKillType : byte
        {
            None = 0,
            Kill = 1,
            Break = 2,
            Motion = 3
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GismoPlanContactDamageType : byte
        {
            None = 0,
            LowSpeed = 1,
            MiddleSpeed = 2,
            HighSpeed = 3
        }

        public class FormatData
        {
            /// <summary>
            /// The data for the design format of a Gismo.
            /// </summary>
            public GismoDesign Design = new();

            /// <summary>
            /// The data for the plan format of a Gismo.
            /// </summary>
            public GismoPlan Plan = new();
        }

        public class GismoDesign
        {
            /// <summary>
            /// TODO: Unused?
            /// </summary>
            public float RangeIn { get; set; }

            /// <summary>
            /// TODO: Unused?
            /// </summary>
            public float RangeDistance { get; set; }

            /// <summary>
            /// This Gismo's basic parameters.
            /// </summary>
            public GismoDesignBasicParam BasicParameters { get; set; } = new();

            /// <summary>
            /// This Gismo's collision data.
            /// </summary>
            public GismoDesignCollision Collision { get; set; } = new();

            /// <summary>
            /// This Gismo's rigid body data.
            /// </summary>
            public GismoDesignRigidBody RigidBody { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when idle.
            /// </summary>
            public GismoDesignReactionData ReactionIdle { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when the player enters its collision.
            /// </summary>
            public GismoDesignReactionData ReactionEnter { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when the player leaves its collision.
            /// </summary>
            public GismoDesignReactionData ReactionLeave { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when the player is stationary within its collision.
            /// </summary>
            public GismoDesignReactionData ReactionStay { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when the player moves within its collision.
            /// </summary>
            public GismoDesignReactionData ReactionStayMove { get; set; } = new();

            /// <summary>
            /// How this Gismo reacts when the damaged.
            /// </summary>
            public GismoDesignReactionData ReactionDamage { get; set; } = new();

            /// <summary>
            /// Whether this Gismo should respawn normally or require a Starfall.
            /// </summary>
            public bool IgnoreMeteorShowerAndRespawnOnReenterRange { get; set; }
        }

        public class GismoDesignBasicParam
        {
            /// <summary>
            /// The name of the model this Gismo should use.
            /// </summary>
            public string ModelName { get; set; } = "";

            /// <summary>
            /// The name of the skeleton for this Gismo.
            /// </summary>
            public string? SkeletonName { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool NoInstance { get; set; }
        }

        public class GismoDesignCollision
        {
            /// <summary>
            /// The shape of this Gismo's collision volume.
            /// </summary>
            public GismoDesignShapeType Shape { get; set; }

            /// <summary>
            /// The origin point of this Gismo's collision volume.
            /// </summary>
            public GismoDesignBasePoint BasePoint { get; set; }

            /// <summary>
            /// The size of this Gismo's collision volume.
            /// </summary>
            public Vector3 Size { get; set; }

            /// <summary>
            /// The name of the collision mesh to use if Shape is set to Mesh (4).
            /// </summary>
            public string? Mesh { get; set; }

            /// <summary>
            /// How far out should this Gismo's collision volume be offset.
            /// </summary>
            public Vector3 ShapeOffset { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float ShapeSizeOffset { get; set; }
        }

        public class GismoDesignRigidBody
        {
            /// <summary>
            /// The type of this Gismo's Rigid Body.
            /// </summary>
            public GismoDesignRigidBodyType Type { get; set; }

            /// <summary>
            /// The material (from a very limited selection) for this Gismo's collision.
            /// </summary>
            public GismoDesignRigidBodyMaterial Material { get; set; }

            /// <summary>
            /// This Gismo's physics data.
            /// </summary>
            public GismoDesignPhysicsParam PhysicsParam { get; set; } = new();
        }

        public class GismoDesignPhysicsParam
        {
            /// <summary>
            /// This Gismo's mass.
            /// </summary>
            public float Mass { get; set; }

            /// <summary>
            /// This Gismo's frction.
            /// </summary>
            public float Friction { get; set; }

            /// <summary>
            /// How strongly gravity affects this Gismo?
            /// </summary>
            public float GravityFactor { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float Restitution { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float LinearDamping { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float AngularDamping { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float MaxLinearVelocity { get; set; }
        }

        public class GismoDesignReactionData
        {
            /// <summary>
            /// This Reaction Data's motion data.
            /// </summary>
            public GismoDesignMotionData Motion { get; set; } = new();

            /// <summary>
            /// This Reaction Data's mirage animations.
            /// </summary>
            public GismoDesignMirageAnimationData MirageAnimations { get; set; } = new();

            /// <summary>
            /// This Reaction Data's program motion.
            /// </summary>
            public GismoDesignProgramMotionData ProgramMotion { get; set; } = new();

            /// <summary>
            /// This Reaction Data's particle effect data.
            /// </summary>
            public GismoDesignEffectData Effect { get; set; } = new();

            /// <summary>
            /// The sound cue to be played by this Reaction Data.
            /// </summary>
            public string? SoundCue { get; set; }

            /// <summary>
            /// This Reaction Data's kill data.
            /// </summary>
            public GismoDesignKillData Kill { get; set; } = new();
        }

        public class GismoDesignMotionData
        {
            /// <summary>
            /// The name of the animation this Motion should use?
            /// </summary>
            public string? MotionName { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool SyncFrame { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool StopEndFrame { get; set; }
        }

        public class GismoDesignMirageAnimationData
        {
            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public string?[] TexSrtAnimNames { get; set; } = new string?[3] { null, null, null };

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public string?[] TexPatAnimNames { get; set; } = new string?[3] { null, null, null };

            /// <summary>
            /// The names of up to three material animations used by this Reaction Data.
            /// </summary>
            public string?[] MatAnimNames { get; set; } = new string?[3] { null, null, null };
        }

        public class GismoDesignProgramMotionData
        {
            /// <summary>
            /// How this Gismo should move in this Reaction Data.
            /// </summary>
            public GismoDesignMotionType Type { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public Vector3 Axis { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float Power { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float SpeedScale { get; set; }

            /// <summary>
            /// How long it takes for a full loop of this motion.
            /// </summary>
            public float Time { get; set; }
        }

        public class GismoDesignEffectData
        {
            /// <summary>
            /// The particle to use by this effect.
            /// </summary>
            public string? EffectName { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool LinkMotionStop { get; set; }
        }

        public class GismoDesignKillData
        {
            /// <summary>
            /// How this Gismo should be killed.
            /// </summary>
            public GismoDesignKillType Type { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public float KillTime { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public string? BreakMotionName { get; set; }

            /// <summary>
            /// The Debris data for this Reaction Data's kill state.
            /// </summary>
            public GismoDesignDebrisData Debris { get; set; } = new();
        }

        public class GismoDesignDebrisData
        {
            /// <summary>
            /// The strength of the gravity on these debris?
            /// </summary>
            public float Gravity { get; set; }

            /// <summary>
            /// How long these debris last.
            /// </summary>
            public float LifeTime { get; set; }

            /// <summary>
            /// The mass of these debris.
            /// </summary>
            public float Mass { get; set; }

            /// <summary>
            /// The friction applied to these debris.
            /// </summary>
            public float Friction { get; set; }

            /// <summary>
            /// How far out these debris should explode upon spawning?
            /// </summary>
            public float ExplosionScale { get; set; }

            /// <summary>
            /// The strength of motion applied to these debris upon spawning?
            /// </summary>
            public float ImpulseScale { get; set; }
        }

        public class GismoPlan
        {
            /// <summary>
            /// Which speed value needs to be used for this Gismo to register damage.
            /// </summary>
            public GismoPlanContactDamageType ContactDamageType { get; set; }

            /// <summary>
            /// Whether a simple spin jump will damage this Gismo.
            /// </summary>
            public bool NoneDamageSpin { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool RideOnDamage { get; set; }

            /// <summary>
            /// TODO: What does this do?
            /// </summary>
            public bool AerialBounce { get; set; }
        }

        // Actual data presented to the end user.
        public FormatData Data = new();

        // HedgeLib# BinaryReader specific variables.
        // Set up HedgeLib#'s BINAV2Header.
        public HedgeLib.Headers.BINAHeader Header = new BINAv2Header(210);

        /// <summary>
        /// Loads and parses this format's file.
        /// </summary>
        /// <param name="filepath">The path to the file to load and parse.</param>
        public override void Load(string filepath)
        {
            // Set up HedgeLib#'s BINAReader for the gismod file and read the BINAV2 header.
            HedgeLib.IO.BINAReader reader = new(File.OpenRead(filepath));
            Header = reader.ReadHeader();

            // Read the Range values.
            Data.Design.RangeIn = reader.ReadSingle();
            Data.Design.RangeDistance = reader.ReadSingle();

            #region Read the GismoDesignBasicParam struct
            // Read this Gismo's model name.
            Data.Design.BasicParameters.ModelName = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip eight null bytes.
            reader.JumpAhead(0x08);

            // Read this Gismo's skeleton name.
            Data.Design.BasicParameters.SkeletonName = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip eight null bytes.
            reader.JumpAhead(0x08);

            // Read this Gismo's NoInstance boolean.
            Data.Design.BasicParameters.NoInstance = reader.ReadBoolean();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);
            #endregion

            #region Read the GismoDesignCollision struct
            // Read this Gismo's Collision Volume Shape.
            Data.Design.Collision.Shape = (GismoDesignShapeType)reader.ReadByte();

            // Read this Gismo's Collision Volume Base Point.
            Data.Design.Collision.BasePoint = (GismoDesignBasePoint)reader.ReadByte();

            // Realign to 0x04 bytes.
            reader.FixPadding(0x04);

            // Read the size for this Gismo's Collision Volume.
            Data.Design.Collision.Size = Helpers.ReadHedgeLibVector3(reader);

            // Read this Gismo's Collision Mesh.
            Data.Design.Collision.Mesh = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read the offset for this Gismo's Collision Volume.
            Data.Design.Collision.ShapeOffset = Helpers.ReadHedgeLibVector3(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read the shape size offset for this Gismo's Collision Volume.
            Data.Design.Collision.ShapeSizeOffset = reader.ReadSingle();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);
            #endregion

            #region Read the GismoDesignRigidBody struct.
            // Read this Gismo's RigidBody Type.
            Data.Design.RigidBody.Type = (GismoDesignRigidBodyType)reader.ReadByte();

            // Read this Gismo's RigidBody Material.
            Data.Design.RigidBody.Material = (GismoDesignRigidBodyMaterial)reader.ReadByte();

            // Realign to 0x04 bytes.
            reader.FixPadding(0x4);

            // Read this Gismo's Mass value.
            Data.Design.RigidBody.PhysicsParam.Mass = reader.ReadSingle();

            // Read this Gismo's Friction value.
            Data.Design.RigidBody.PhysicsParam.Friction = reader.ReadSingle();

            // Read this Gismo's Gravity Factor.
            Data.Design.RigidBody.PhysicsParam.GravityFactor = reader.ReadSingle();

            // Read this Gismo's Restitution value.
            Data.Design.RigidBody.PhysicsParam.Restitution = reader.ReadSingle();

            // Read this Gismo's Linear Damping value.
            Data.Design.RigidBody.PhysicsParam.LinearDamping = reader.ReadSingle();

            // Read this Gismo's Angular Damping value.
            Data.Design.RigidBody.PhysicsParam.AngularDamping = reader.ReadSingle();

            // Read this Gismo's Max Linear Velocity value.
            Data.Design.RigidBody.PhysicsParam.MaxLinearVelocity = reader.ReadSingle();
            #endregion

            // Read all the Reaction Data entries.
            ReadReactionData(reader, Data.Design.ReactionIdle);
            ReadReactionData(reader, Data.Design.ReactionEnter);
            ReadReactionData(reader, Data.Design.ReactionLeave);
            ReadReactionData(reader, Data.Design.ReactionStay);
            ReadReactionData(reader, Data.Design.ReactionStayMove);
            ReadReactionData(reader, Data.Design.ReactionDamage);

            // Read this Gismo's Starfall Respawn value.
            Data.Design.IgnoreMeteorShowerAndRespawnOnReenterRange = reader.ReadBoolean();

            // Close HedgeLib#'s BINAReader for the gismod file.
            reader.Close();

            // Set up HedgeLib#'s BINAReader for the gismop file and read the BINAV2 header.
            reader = new(File.OpenRead($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}_pln.gismop"));
            Header = reader.ReadHeader();

            // Read this Gismo's Contact Damage Type.
            Data.Plan.ContactDamageType = (GismoPlanContactDamageType)reader.ReadByte();

            // Read this Gismo's None Damage Spin value.
            Data.Plan.NoneDamageSpin = reader.ReadBoolean();

            // Read this Gismo's Ride on Damage value.
            Data.Plan.RideOnDamage = reader.ReadBoolean();

            // Read this Gismo's Aerial Bounce value.
            Data.Plan.AerialBounce = reader.ReadBoolean();

            // Close HedgeLib#'s BINAReader for the gismop file.
            reader.Close();
        }

        /// <summary>
        /// Reads a Design Gismo's Reaction Data struct.
        /// </summary>
        /// <param name="reader">The HedgeLib# BINAReader to use.</param>
        /// <param name="data">The Reaction Data struct we're filling in.</param>
        private static void ReadReactionData(HedgeLib.IO.BINAReader reader, GismoDesignReactionData data)
        {
            #region Read the GismoDesignMotionData struct
            // Read this Motion's Name.
            data.Motion.MotionName = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read this Motion Data's sync frame value.
            data.Motion.SyncFrame = reader.ReadBoolean();

            // Read this Motion Data's stop end frame value.
            data.Motion.StopEndFrame = reader.ReadBoolean();

            // Realign to 0x08 bytes.
            reader.FixPadding(0x08);
            #endregion

            #region Read the GismoDesignMirageAnimationData struct
            // Read all the TexSrtAnimNames.
            data.MirageAnimations.TexSrtAnimNames[0] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.TexSrtAnimNames[1] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.TexSrtAnimNames[2] = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip 0x18 null bytes.
            reader.JumpAhead(0x18);

            // Read all the TexPatAnimNames.
            data.MirageAnimations.TexPatAnimNames[0] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.TexPatAnimNames[1] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.TexPatAnimNames[2] = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip 0x18 null bytes.
            reader.JumpAhead(0x18);

            // Read all the MatAnimNames.
            data.MirageAnimations.MatAnimNames[0] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.MatAnimNames[1] = Helpers.ReadNullTerminatedStringTableEntry(reader);
            data.MirageAnimations.MatAnimNames[2] = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip 0x18 null bytes.
            reader.JumpAhead(0x20);
            #endregion

            #region Read the GismoDesignProgramMotionData struct
            // Read this Program Motion's type.
            data.ProgramMotion.Type = (GismoDesignMotionType)reader.ReadByte();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read this Program Motion's axis.
            data.ProgramMotion.Axis = Helpers.ReadHedgeLibVector3(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read this Program Motion's power value.
            data.ProgramMotion.Power = reader.ReadSingle();

            // Read this Program Motion's speed scale value.
            data.ProgramMotion.SpeedScale = reader.ReadSingle();

            // Read this Program Motion's time value.
            data.ProgramMotion.Time = reader.ReadSingle();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);
            #endregion

            #region Read the GismoDesignEffectData struct
            // Read this Effect's name.
            data.Effect.EffectName = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read this Effect's link motion stop value.
            data.Effect.LinkMotionStop = reader.ReadBoolean();

            // Realign to 0x08 bytes.
            reader.FixPadding(0x08);
            #endregion

            // Read this Reaction Data's sound cue.
            data.SoundCue = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Skip 0x08 null bytes.
            reader.JumpAhead(0x08);

            #region Read the GismoDesignKillData struct
            // Read this Reaction Data's kill type.
            data.Kill.Type = (GismoDesignKillType)reader.ReadByte();

            // Realign to 0x04 bytes.
            reader.FixPadding(0x04);

            // Read this Reaction Data's kill time value.
            data.Kill.KillTime = reader.ReadSingle();

            // Read this Reaction Data's break motion name.
            data.Kill.BreakMotionName = Helpers.ReadNullTerminatedStringTableEntry(reader);

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);

            // Read the gravity value for this kill data's debris.
            data.Kill.Debris.Gravity = reader.ReadSingle();

            // Read the life time value for this kill data's debris.
            data.Kill.Debris.LifeTime = reader.ReadSingle();

            // Read the mass value for this kill data's debris.
            data.Kill.Debris.Mass = reader.ReadSingle();

            // Read the friction value for this kill data's debris.
            data.Kill.Debris.Friction = reader.ReadSingle();

            // Read the explosion scale value for this kill data's debris.
            data.Kill.Debris.ExplosionScale = reader.ReadSingle();

            // Read the impulse scale value for this kill data's debris.
            data.Kill.Debris.ImpulseScale = reader.ReadSingle();

            // Realign to 0x10 bytes.
            reader.FixPadding(0x10);
            #endregion
        }

        /// <summary>
        /// Saves this format's file.
        /// </summary>
        /// <param name="filepath">The path to save to.</param>
        public void Save(string filepath)
        {
            // Set up our BINAWriter for the gismod file and write the BINAV2 header.
            HedgeLib.IO.BINAWriter writer = new(File.Create(filepath), Header);

            // Write this Gismo's range vaalues.
            writer.Write(Data.Design.RangeIn);
            writer.Write(Data.Design.RangeDistance);

            #region Write the GismoDesignBasicParam struct
            // Write this Gismo's Model Name.
            writer.AddString("ModelNameOffset", Data.Design.BasicParameters.ModelName, 0x08);

            // Write eight null bytes.
            writer.WriteNulls(0x08);

            // Write this Gismo's Skeleton Name.
            writer.AddString("SkeletonNameOffset", Data.Design.BasicParameters.SkeletonName, 0x08);

            // Write eight null bytes.
            writer.WriteNulls(0x08);

            // Write this Gismo's No Instance value
            writer.Write(Data.Design.BasicParameters.NoInstance);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);
            #endregion

            #region Write the GismoDesignCollision struct
            // Write this Gismo's Collision Volume Shape.
            writer.Write((byte)Data.Design.Collision.Shape);

            // Write this Gismo's Collision Volume Base Point.
            writer.Write((byte)Data.Design.Collision.BasePoint);

            // Realign to 0x04 bytes.
            writer.FixPadding(0x4);

            // Write the size for this this Collision's Volume.
            Helpers.WriteHedgeLibVector3(writer, Data.Design.Collision.Size);

            // Write this Gismo's Mesh Name.
            writer.AddString("MeshNameOffset", Data.Design.Collision.Mesh, 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write the offset for this this Collision's Volume.
            Helpers.WriteHedgeLibVector3(writer, Data.Design.Collision.ShapeOffset);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write the shape size offset for this Gismo's Collision Volume.
            writer.Write(Data.Design.Collision.ShapeSizeOffset);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);
            #endregion

            #region Write the GismoDesignRigidBody struct.
            // Write this Gismo's RigidBody Type.
            writer.Write((byte)Data.Design.RigidBody.Type);

            // Write this Gismo's RigidBody Material.
            writer.Write((byte)Data.Design.RigidBody.Material);

            // Realign to 0x04 bytes.
            writer.FixPadding(0x4);

            // Write this Gismo's Mass value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.Mass);

            // Write this Gismo's Friction value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.Friction);

            // Write this Gismo's Gravity Factor.
            writer.Write(Data.Design.RigidBody.PhysicsParam.GravityFactor);

            // Write this Gismo's Restitution value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.Restitution);

            // Write this Gismo's Linear Damping value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.LinearDamping);

            // Write this Gismo's Angular Damping value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.AngularDamping);

            // Write this Gismo's Max Linear Velocity value.
            writer.Write(Data.Design.RigidBody.PhysicsParam.MaxLinearVelocity);
            #endregion

            // Write all the Reaction Data entries.
            WriteReactionData(writer, Data.Design.ReactionIdle, 0);
            WriteReactionData(writer, Data.Design.ReactionEnter, 1);
            WriteReactionData(writer, Data.Design.ReactionLeave, 2);
            WriteReactionData(writer, Data.Design.ReactionStay, 3);
            WriteReactionData(writer, Data.Design.ReactionStayMove, 4);
            WriteReactionData(writer, Data.Design.ReactionDamage, 5);

            // Write this Gismo's Starfall Respawn value.
            writer.Write(Data.Design.IgnoreMeteorShowerAndRespawnOnReenterRange);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();

            // Set up our BINAWriter for the gismop file and write the BINAV2 header.
            writer = new(File.Create($@"{Path.GetDirectoryName(filepath)}\{Path.GetFileNameWithoutExtension(filepath)}_pln.gismop"), Header);

            // Write this Gismo's Contact Damage Type.
            writer.Write((byte)Data.Plan.ContactDamageType);

            // Write this Gismo's None Damage Spin value.
            writer.Write(Data.Plan.NoneDamageSpin);

            // Write this Gismo's Ride on Damage value.
            writer.Write(Data.Plan.RideOnDamage);

            // Write this Gismo's Aerial Bounce value.
            writer.Write(Data.Plan.AerialBounce);

            // Finish writing the BINA information.
            writer.FinishWrite(Header);

            // Close HedgeLib#'s BINAWriter for the gismod file.
            writer.Close();
        }

        /// <summary>
        /// Writes a Design Gismo's Reaction Data struct to a file.
        /// </summary>
        /// <param name="writer">The HedgeLib# BINAWrite to use.</param>
        /// <param name="data">The Reaction Data struct we're writing the data of.</param>
        /// <param name="reactionIndex">The index of the Reaction Data we're filling in (used for String Offset adding).</param>
        private static void WriteReactionData(HedgeLib.IO.BINAWriter writer, GismoDesignReactionData data, int reactionIndex)
        {
            #region Write the GismoDesignMotionData struct
            // Write this Motions' Name.
            writer.AddString($"MotionNameOffset_{reactionIndex}", data.Motion.MotionName, 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write this Motion Data's sync frame value.
            writer.Write(data.Motion.SyncFrame);

            // Write this Motion Data's stop end frame value.
            writer.Write(data.Motion.StopEndFrame);

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);
            #endregion

            #region Write the GismoDesignMirageAnimationData struct
            // Write the TexSrtAnimNames.
            writer.AddString($"TexSrtAnimName0Offset_{reactionIndex}", data.MirageAnimations.TexSrtAnimNames[0], 0x08);
            writer.AddString($"TexSrtAnimName1Offset_{reactionIndex}", data.MirageAnimations.TexSrtAnimNames[1], 0x08);
            writer.AddString($"TexSrtAnimName2Offset_{reactionIndex}", data.MirageAnimations.TexSrtAnimNames[2], 0x08);

            // Write 0x18 null bytes.
            writer.WriteNulls(0x18);

            // Write the TexPatAnimNames.
            writer.AddString($"TexPatAnimName0Offset_{reactionIndex}", data.MirageAnimations.TexPatAnimNames[0], 0x08);
            writer.AddString($"TexPatAnimName1Offset_{reactionIndex}", data.MirageAnimations.TexPatAnimNames[1], 0x08);
            writer.AddString($"TexPatAnimName2Offset_{reactionIndex}", data.MirageAnimations.TexPatAnimNames[2], 0x08);

            // Write 0x18 null bytes.
            writer.WriteNulls(0x18);

            // Write the MatAnimNames.
            writer.AddString($"MatAnimName0Offset_{reactionIndex}", data.MirageAnimations.MatAnimNames[0], 0x08);
            writer.AddString($"MatAnimName1Offset_{reactionIndex}", data.MirageAnimations.MatAnimNames[1], 0x08);
            writer.AddString($"MatAnimName2Offset_{reactionIndex}", data.MirageAnimations.MatAnimNames[2], 0x08);

            // Write 0x18 null bytes.
            writer.WriteNulls(0x20);
            #endregion

            #region Write the GismoDesignProgramMotionData struct
            // Write this Program Motion's type.
            writer.Write((byte)data.ProgramMotion.Type);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write this Program Motion's axis.
            Helpers.WriteHedgeLibVector3(writer, data.ProgramMotion.Axis);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write this Program Motion's power value.
            writer.Write(data.ProgramMotion.Power);

            // Write this Program Motion's speed scale value.
            writer.Write(data.ProgramMotion.SpeedScale);

            // Write this Program Motion's time value.
            writer.Write(data.ProgramMotion.Time);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);
            #endregion

            #region Write the GismoDesignEffectData struct
            // Write this Effect's name.
            writer.AddString($"EffectNameOffset_{reactionIndex}", data.Effect.EffectName, 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write this Effect's link motion stop value.
            writer.Write(data.Effect.LinkMotionStop);

            // Realign to 0x08 bytes.
            writer.FixPadding(0x08);
            #endregion

            // Write this Reaction Data's sound cue.
            writer.AddString($"SoundCueOffset_{reactionIndex}", data.SoundCue, 0x08);

            // Write 0x08 null bytes.
            writer.WriteNulls(0x08);

            #region Write the GismoDesignKillData struct
            // Write this Reaction Data's kill type.
            writer.Write((byte)data.Kill.Type);

            // Realign to 0x04 bytes.
            writer.FixPadding(0x04);

            // Write this Reaction Data's kill time value.
            writer.Write(data.Kill.KillTime);

            // Write this Reaction Data's break motion name.
            writer.AddString($"BreakMotionNameOffset_{reactionIndex}", data.Kill.BreakMotionName, 0x08);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);

            // Write the gravity value for this kill data's debris.
            writer.Write(data.Kill.Debris.Gravity);

            // Write the life time value for this kill data's debris.
            writer.Write(data.Kill.Debris.LifeTime);

            // Write the mass value for this kill data's debris.
            writer.Write(data.Kill.Debris.Mass);

            // Write the friction value for this kill data's debris.
            writer.Write(data.Kill.Debris.Friction);

            // Write the explosion scale value for this kill data's debris.
            writer.Write(data.Kill.Debris.ExplosionScale);

            // Write the impulse scale value for this kill data's debris.
            writer.Write(data.Kill.Debris.ImpulseScale);

            // Realign to 0x10 bytes.
            writer.FixPadding(0x10);
            #endregion
        }
    }
}
