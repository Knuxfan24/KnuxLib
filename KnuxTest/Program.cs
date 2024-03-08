using HedgeLib.Sets;
using JeremyAnsel.Media.WavefrontObj;
using KnuxLib;
using Marathon;
using Marathon.IO;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace KnuxTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Enable shift-jis for HedgeLib# stuff.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Set the console's output to UTF8 rather than ASCII.
            Console.OutputEncoding = Encoding.UTF8;

            KnuxLib.Engines.Hedgehog.Scene_2013 hhd = new(@"C:\Users\Knuxf\Documents\w1a04.hhd");
            return;

            foreach (string cloudFile in Directory.GetFiles(@"C:\Users\Knuxf\Desktop\compare\orig"))
            {
                Console.WriteLine(cloudFile);
                KnuxLib.Engines.Hedgehog.Cloud cloud = new(cloudFile, true);
            }
            return;

            //KnuxLib.Engines.Hedgehog.PathSpline path = new();
            //path.ImportOBJ(@"C:\Users\Knuxf\Documents\3dsMax\export\grind.obj", KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013);
            //path.Save(@"D:\Steam\steamapps\common\Sonic Lost World\mods\Stupid Test Mod\disk\sonic2013_patch_0\w1a01\w1a01_misc\w1a01_path.path2.bin", KnuxLib.Engines.Hedgehog.PathSpline.FormatVersion.sonic_2013);
            //return;

            Marathon.Formats.Placement.ObjectPlacement s06Set = new();
            s06Set.Load(@"G:\Sonic '06\Game Dump\xenon\archives\scripts\xenon\scripts\mission\0201\set_wvoA_silver.set");

            KnuxLib.Engines.Hedgehog.StageEntityTable_2013 slwSet = new();

            List<KnuxLib.Engines.Hedgehog.StageEntityTable_2013.SetObject> slwObjects = new();

            for (int s06ObjIndex = 0; s06ObjIndex < s06Set.Data.Objects.Count; s06ObjIndex++)
            {
                KnuxLib.Engines.Hedgehog.StageEntityTable_2013.SetObject slwObj = new();

                switch (s06Set.Data.Objects[s06ObjIndex].Type)
                {
                    case "common_rainbowring":
                        slwObj = CreateSLWObject("NightsDashRing", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, (float)(s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data) / 5, typeof(float), "FirstSpeed");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data, typeof(float), "OutOfControl");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data, typeof(float), "KeepVelocityDistance");
                        break;

                    case "dashpanel":
                        slwObj = CreateSLWObject("DashPanel", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[1].Data, typeof(float), "OutOfControl");
                        AddSLWParameter(slwObj, (float)(s06Set.Data.Objects[s06ObjIndex].Parameters[0].Data) / 5, typeof(float), "Speed");
                        AddSLWParameter(slwObj, true, typeof(bool), "IsVisibleModel");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsSideView");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsDirectionPath");
                        break;

                    case "enemy":
                    case "enemyextra":
                        slwObj = CreateSLWObject("enmMotora", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, 0f, typeof(float), "Speed");
                        AddSLWParameter(slwObj, 0f, typeof(float), "MaxMoveDistance");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsEventOn");
                        AddSLWParameter(slwObj, false, typeof(bool), "MoveRigidBody");
                        break;

                    case "goalring":
                        slwObj = CreateSLWObject("GoalRing", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, false, typeof(bool), "IsMessageON");
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "GoalRingBattleInfo");
                        break;

                    case "itemboxa":
                    case "itemboxg":
                        slwObj = CreateSLWObject("ItemBox", s06Set.Data.Objects[s06ObjIndex]);
                        switch ((int)s06Set.Data.Objects[s06ObjIndex].Parameters[0].Data)
                        {
                            case 4: AddSLWParameter(slwObj, (byte)2, typeof(byte), "ItemType"); break;
                            case 5: AddSLWParameter(slwObj, (byte)0, typeof(byte), "ItemType"); break;
                            case 7: AddSLWParameter(slwObj, (byte)1, typeof(byte), "ItemType"); break;
                            default: AddSLWParameter(slwObj, (byte)3, typeof(byte), "ItemType"); break;
                        }
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "TargetRedRing");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "EventType");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "AreaType");
                        AddSLWParameter(slwObj, false, typeof(bool), "RocDriTempRetire");
                        break;

                    case "jumppanel":
                        slwObj = CreateSLWObject("Spring", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, (float)(s06Set.Data.Objects[s06ObjIndex].Parameters[1].Data) / 5, typeof(float), "FirstSpeed");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data, typeof(float), "OutOfControl");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data, typeof(float), "KeepVelocityDistance");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsPathChange");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsChangeCameraWhenPathChange");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsTo3D");
                        if ((uint)s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data != 4294967295)
                        {
                            AddSLWParameter(slwObj, true, typeof(bool), "Targeting");
                            AddSLWParameter(slwObj, new Vector3(s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data].Position.X / 10, s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data].Position.Y / 10, s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[3].Data].Position.Z / 10), typeof(Vector3), "TargetPos");
                        }
                        else
                        {
                            AddSLWParameter(slwObj, false, typeof(bool), "Targeting");
                            AddSLWParameter(slwObj, new Vector3(), typeof(Vector3), "TargetPos");
                        }
                        AddSLWParameter(slwObj, false, typeof(bool), "IsMoveCylinderFloor");
                        AddSLWParameter(slwObj, 0f, typeof(float), "MoveCylinderTime");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsEventOn");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsUpdateYaw");
                        break;

                    case "objectphysics":
                        slwObj = CreateSLWObject("Gismo", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[0].Data, typeof(string), "name");
                        AddSLWParameter(slwObj, new Vector3(1f, 1f, 1f), typeof(Vector3), "scale");
                        break;

                    case "ring":
                        slwObj = CreateSLWObject("Ring", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, 0f, typeof(float), "ResetTime");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsReset");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsLightSpeedDashTarget");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "SetPlaceType");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "EventType");
                        break;

                    case "savepoint":
                        slwObj = CreateSLWObject("PointMarker", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, 15f, typeof(float), "Width");
                        AddSLWParameter(slwObj, 15f, typeof(float), "Height");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "StageType");
                        AddSLWParameter(slwObj, (byte)0, typeof(byte), "PlacementType");
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "CheckNo");
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "WarpPoint");
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "WarpPassTarget");
                        AddSLWParameter(slwObj, (uint)0, typeof(uint), "WarpItem");
                        AddSLWParameter(slwObj, false, typeof(bool), "EnableWarpItem");
                        break;

                    case "spring":
                    case "widespring":
                        slwObj = CreateSLWObject("Spring", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, (float)(s06Set.Data.Objects[s06ObjIndex].Parameters[0].Data) / 5, typeof(float), "FirstSpeed");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[1].Data, typeof(float), "OutOfControl");
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Parameters[1].Data, typeof(float), "KeepVelocityDistance");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsPathChange");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsChangeCameraWhenPathChange");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsTo3D");
                        if (s06Set.Data.Objects[s06ObjIndex].Type == "spring")
                        {
                            AddSLWParameter(slwObj, true, typeof(bool), "Targeting");
                            AddSLWParameter(slwObj, new Vector3(s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data].Position.X / 10, s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data].Position.Y / 10, s06Set.Data.Objects[(int)(uint)s06Set.Data.Objects[s06ObjIndex].Parameters[2].Data].Position.Z / 10), typeof(Vector3), "TargetPos");
                        }
                        else
                        {
                            AddSLWParameter(slwObj, false, typeof(bool), "Targeting");
                            AddSLWParameter(slwObj, new Vector3(), typeof(Vector3), "TargetPos");
                            slwObj.Transform.Rotation = new(0, 0, 0, 1);
                        }
                        AddSLWParameter(slwObj, false, typeof(bool), "IsMoveCylinderFloor");
                        AddSLWParameter(slwObj, 0f, typeof(float), "MoveCylinderTime");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsEventOn");
                        AddSLWParameter(slwObj, false, typeof(bool), "IsUpdateYaw");
                        break;

                    default:
                        Console.WriteLine($"Handled object '{s06Set.Data.Objects[s06ObjIndex].Type}'");
                        slwObj = CreateSLWObject("Gismo", s06Set.Data.Objects[s06ObjIndex]);
                        AddSLWParameter(slwObj, s06Set.Data.Objects[s06ObjIndex].Type, typeof(string), "name");
                        AddSLWParameter(slwObj, new Vector3(1f, 1f, 1f), typeof(Vector3), "scale");
                        break;
                }

                slwObjects.Add(slwObj);
            }

            slwSet.Data = slwObjects.ToArray();

            slwSet.Save(@"D:\Steam\steamapps\common\Sonic Lost World\mods\Stupid Test Mod\disk\sonic2013_patch_0\set\w1a01_obj_01.orc");
            slwSet.ExportHSON(@"D:\Steam\steamapps\common\Sonic Lost World\mods\Stupid Test Mod\disk\sonic2013_patch_0\set\w1a01_obj_00.hson", @"C:\Users\Knuxf\Documents\GitHub\KnuxLib\KnuxLib\HSON\Templates\sonic_2013.json", "set_wvoA_silver", "Knuxfan24", $"Auto-converted from set_wvoA_silver.set");

            return;

            //control.Save(@"G:\Sonic Lost World\Game Dump\set\w6a05_obj_02.resave");
            //KnuxLib.Engines.Hedgehog.StageEntityTable_2013 largest = new(@"G:\Sonic Lost World\Game Dump\set\w1a04_obj_00.orc", @"C:\Users\Knuxf\Documents\GitHub\KnuxLib\KnuxLib\HSON\Templates\sonic_2013.json");

            foreach (string orcFile in Directory.GetFiles(@"G:\Sonic Lost World\Game Dump\set", "*.orc"))
            {
                //File.Copy(orcFile, $@"C:\Users\Knuxf\Desktop\compare\orig\{Path.GetFileName(orcFile)}", true);
                Console.WriteLine(orcFile);
                KnuxLib.Engines.Hedgehog.StageEntityTable_2013 orc = new(orcFile, @"C:\Users\Knuxf\Documents\GitHub\KnuxLib\KnuxLib\HSON\Templates\sonic_2013.json", true);
                //orc.Save($@"C:\Users\Knuxf\Desktop\compare\resave\{Path.GetFileName(orcFile)}");
                //orc.ExportHSON(Path.ChangeExtension(orcFile, ".hson"), @"C:\Users\Knuxf\Documents\GitHub\KnuxLib\KnuxLib\HSON\Templates\sonic_2013.json", Path.GetFileNameWithoutExtension(orcFile), "Knuxfan24", $"Auto-converted from {Path.GetFileName(orcFile)}");
            }
        }

        private static KnuxLib.Engines.Hedgehog.StageEntityTable_2013.SetObject CreateSLWObject(string type, Marathon.Formats.Placement.SetObject s06Obj)
        {
            KnuxLib.Engines.Hedgehog.StageEntityTable_2013.SetObject slwObj = new()
            {
                Index = (ushort)s06Obj.Index,
                Type = type
            };
            slwObj.Transform.Position = new(s06Obj.Position.X / 10, s06Obj.Position.Y / 10, s06Obj.Position.Z / 10);
            slwObj.Transform.Rotation = s06Obj.Rotation;

            return slwObj;
        }

        private static void AddSLWParameter(KnuxLib.Engines.Hedgehog.StageEntityTable_2013.SetObject slwObj, object value, Type type, string name)
        {
            slwObj.Parameters.Add(new()
            {
                Data = value,
                DataType = type,
                Name = name
            });
        }
    }
}