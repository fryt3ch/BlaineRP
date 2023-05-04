using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data.Fractions
{
    public class EMS : Fraction, IUniformable
    {
        public List<string> UniformNames { get; set; }

        public List<MapObject> TempObjects { get; set; }

        public EMS(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string LockerRoomPositionsStr, string CreationWorkbenchPricesJs, string BedPositionsJs) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs))
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(LockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Additional.Cylinder(pos, 1f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionLockerRoomInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var lockerRoomText = new TextLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
                {
                    Font = 0,
                };
            }

            Utils.Vector4[] mainColshapes = null;

            if (Type == Types.EMS_BLAINE)
            {
                UniformNames = new List<string>()
                {
                    "Доктор (полностью)",
                    "Доктор (только халат)",
                    "Фельдшер",
                };

                mainColshapes = new Utils.Vector4[]
                {
                    new Utils.Vector4(-255.972f, 6321.406f, 33.116f, 45f),

                    new Utils.Vector4(1830.509f, 3679.626f, 33.2749f, 45f),
                };
            }
            else if (Type == Types.EMS_LS)
            {
                mainColshapes = new Utils.Vector4[]
                {
                    new Utils.Vector4(311.2546f, -592.4204f, 42.32737f, 50f),
                };
            }

            Vector3[] bedPositions = RAGE.Util.Json.Deserialize<Vector3[]>(BedPositionsJs);

            foreach (var x in mainColshapes)
            {
                Additional.ExtraColshape cs = null;

                cs = new Additional.Circle(x.Position, x.RotationZ, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    Name = $"EMS_{(int)Type}",

                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                    OnEnter = async (cancel) =>
                    {
                        await RAGE.Game.Invoker.WaitAsync(1500);

                        if (!cs.IsInside)
                            return;

                        TempObjects = new List<MapObject>();

                        var bedHashes = new List<string>()
                        {
                            "v_med_bed2",
                            "v_med_bed1",
                        }.Select(x => RAGE.Util.Joaat.Hash(x)).ToList();

                        for (int i = 0; i < bedPositions.Length; i++)
                        {
                            var bedPos = bedPositions[i];

                            MapObject bedObj = null;

                            foreach (var x in bedHashes)
                            {
                                var bedHandle = RAGE.Game.Object.GetClosestObjectOfType(bedPos.X, bedPos.Y, bedPos.Z, 1f, x, false, true, true);

                                if (bedHandle <= 0)
                                    continue;

                                bedObj = new MapObject(bedHandle)
                                {
                                    Dimension = uint.MaxValue,
                                };

                                break;
                            }

                            if (bedObj == null)
                                continue;

                            TempObjects.Add(bedObj);

                            var bedIdx = i;

                            bedObj.SetData("EmsBedId", $"{(int)Type}_{bedIdx}");

                            bedObj.SetData("Interactive", true);

                            bedObj.SetData("CustomText", (Action<float, float>)((x, y) =>
                            {
                                Utils.DrawText($"Больничная койка", x, y - NameTags.Interval * 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                                var isOccupied = IsBedOccupied(bedIdx);

                                if (isOccupied)
                                    Utils.DrawText($"[Занята]", x, y - NameTags.Interval, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                                else
                                    Utils.DrawText($"[Свободна]", x, y - NameTags.Interval, 0, 255, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                            }));

                            bedObj.SetData("CustomAction", (Action<MapObject>)OnHealingBedPress);
                        }
                    },

                    OnExit = (cancel) =>
                    {
                        if (TempObjects != null)
                        {
                            foreach (var x in TempObjects)
                                x?.Destroy();

                            TempObjects.Clear();

                            TempObjects = null;
                        }
                    },
                };
            }
        }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }

        public bool IsBedOccupied(int bedIdx) => Sync.World.GetSharedData<bool>($"EMS::{(int)Type}::BED::{bedIdx}", false);

        private static async void OnHealingBedPress(MapObject obj)
        {
            var bedIds = (obj.GetData<string>("EmsBedId"))?.Split('_');

            if (bedIds == null)
                return;

            var fTypeNum = int.Parse(bedIds[0]);

            var bedIdx = int.Parse(bedIds[1]);

            if (!Utils.CanDoSomething(true, Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.OtherAnimation, Utils.Actions.Animation, Utils.Actions.Scenario, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot, Utils.Actions.IsSwimming, Utils.Actions.HasItemInHands, Utils.Actions.IsAttachedTo))
                return;

            var health = Player.LocalPlayer.GetRealHealth();

            if (health > 80)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, $"Для того, чтобы пройти лечение в больнице, кол-во Вашего здоровья должно быть не более, чем 80 ед., Ваше здоровье - {health} ед.");

                return;
            }

            if (Sync.Animations.LastSent.IsSpam(500, false, true))
                return;

            var heading = obj.GetHeading();
            var pos = obj.GetCoords(false);

            Sync.Animations.LastSent = Sync.World.ServerTime;

            var res = (bool)await Events.CallRemoteProc("EMS::BedOcc", fTypeNum, bedIdx);

            if (res)
            {
                Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                Player.LocalPlayer.SetHeading(heading + 240f);

                Sync.Animations.Play(Player.LocalPlayer, Sync.Animations.GeneralTypes.BedLie0);

                Additional.ExtraColshape cs = null;

                cs = new Additional.Cylinder(pos, 1.5f, 2f, false, Utils.RedColor, Player.LocalPlayer.Dimension, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                    Name = "ems_healing_bed",

                    OnExit = (cancel) =>
                    {
                        if (cs?.Exists != true)
                            return;

                        Events.CallRemote("EMS::BedFree");
                    },
                };

                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, "Вы легли на койку и начали процесс лечения!");
            }
        }
    }

    public class EMSEvents : Events.Script
    {
        public EMSEvents()
        {

        }
    }
}