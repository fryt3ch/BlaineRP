using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Animations.Enums;
using BlaineRP.Client.EntitiesData.Components;
using BlaineRP.Client.Sync;
using Players = BlaineRP.Client.Sync.Players;
using Script = BlaineRP.Client.Animations.Script;

namespace BlaineRP.Client.Data.Fractions
{
    public class EMS : Fraction, IUniformable
    {
        public string[] UniformNames { get; set; }

        public List<MapObject> TempObjects { get; set; }

        public EMS(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string LockerRoomPositionsStr, string CreationWorkbenchPricesJs, uint MetaFlags, string BedPositionsJs) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs), MetaFlags)
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(LockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Additional.Cylinder(pos, 1f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionLockerRoomInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var lockerRoomText = new Additional.ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };
            }

            Utils.Vector4[] mainColshapes = null;

            if (Type == Types.EMS_BLAINE)
            {
                UniformNames = new string[]
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

                cs = new Additional.Circle(x.Position, x.RotationZ, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    Name = $"EMS_{(int)Type}",

                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                    OnEnter = async (cancel) =>
                    {
                        var taskKey = "EMS_LOAD";

                        AsyncTask task = null;

                        task = new AsyncTask(async () =>
                        {
                            await RAGE.Game.Invoker.WaitAsync(1500);

                            if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
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
                                    Graphics.DrawText($"Больничная койка", x, y - NameTags.Interval * 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                                    var isOccupied = IsBedOccupied(bedIdx);

                                    if (isOccupied)
                                        Graphics.DrawText($"[Занята]", x, y - NameTags.Interval, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                                    else
                                        Graphics.DrawText($"[Свободна]", x, y - NameTags.Interval, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                                }));

                                bedObj.SetData("CustomAction", (Action<MapObject>)OnHealingBedPress);
                            }

                            AsyncTask.Methods.CancelPendingTask(taskKey);
                        }, 0, false, 0);

                        AsyncTask.Methods.SetAsPending(task, "EMS_LOAD");
                    },

                    OnExit = (cancel) =>
                    {
                        AsyncTask.Methods.CancelPendingTask("EMS_LOAD");

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

            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("documents", 0, "fraction_docs");

            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 10, "fraction_invite");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 11, "ems_heal");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 12, "ems_diag");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 13, "ems_medcard");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 14, "ems_narco");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 15, "ems_psych");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 0, "ems_sellmask");

            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 16, "player_to_veh");
            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 17, "player_from_veh");

            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_heal", (entity) => { var player = entity as Player; if (player == null) return; PlayerHeal(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_diag", (entity) => { var player = entity as Player; if (player == null) return; PlayerDiagnostics(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_medcard", (entity) => { var player = entity as Player; if (player == null) return; PlayerMedicalCard(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_narco", (entity) => { var player = entity as Player; if (player == null) return; PlayerDrugHeal(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_psych", (entity) => { var player = entity as Player; if (player == null) return; PlayerPsychHeal(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_sellmask", (entity) => { var player = entity as Player; if (player == null) return; PlayerSellMask(player); });

            /*            CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_to_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerToVehicle(veh); });
                        CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_from_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerFromVehicle(veh); });*/
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }

        public bool IsBedOccupied(int bedIdx) => Sync.World.GetSharedData<bool>($"EMS::{(int)Type}::BED::{bedIdx}", false);

        private void PlayerHeal(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsHeal, null);
        }

        private void PlayerDiagnostics(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsDiagnostics, null);
        }

        private void PlayerMedicalCard(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsMedicalCard, null);
        }

        private void PlayerDrugHeal(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsDrugHeal, null);
        }

        private void PlayerPsychHeal(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsPsychHeal, null);
        }

        private void PlayerSellMask(Player player)
        {
            Sync.Offers.Request(player, Sync.Offers.Types.EmsSellMask, null);
        }

        private static async void OnHealingBedPress(MapObject obj)
        {
            var bedIds = (obj.GetData<string>("EmsBedId"))?.Split('_');

            if (bedIds == null)
                return;

            var fTypeNum = int.Parse(bedIds[0]);

            var bedIdx = int.Parse(bedIds[1]);

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.OtherAnimation, PlayerActions.Types.Animation, PlayerActions.Types.Scenario, PlayerActions.Types.FastAnimation, PlayerActions.Types.InVehicle, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot, PlayerActions.Types.IsSwimming, PlayerActions.Types.HasItemInHands, PlayerActions.Types.IsAttachedTo))
                return;

            var health = Player.LocalPlayer.GetRealHealth();

            if (health > 80)
            {
                CEF.Notification.ShowError($"Для того, чтобы пройти лечение в больнице, кол-во Вашего здоровья должно быть не более, чем 80 ед., Ваше здоровье - {health} ед.");

                return;
            }

            if (Script.LastSent.IsSpam(500, false, true))
                return;

            var heading = obj.GetHeading();
            var pos = obj.GetCoords(false);

            Script.LastSent = Sync.World.ServerTime;

            var res = (bool)await Events.CallRemoteProc("EMS::BedOcc", fTypeNum, bedIdx);

            if (res)
            {
                Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                Player.LocalPlayer.SetHeading(heading + 240f);

                Script.Play(Player.LocalPlayer, GeneralTypes.BedLie0);

                Additional.ExtraColshape cs = null;

                cs = new Additional.Cylinder(pos, 1.5f, 2f, false, Utils.Misc.RedColor, Player.LocalPlayer.Dimension, null)
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

                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Вы легли на койку и начали процесс лечения!");
            }
        }
    }

    [Script(int.MaxValue)]
    public class EMSEvents
    {
        public EMSEvents()
        {
            Events.Add("Ems::ShowPlayerDiagnostics", async (args) =>
            {
                var player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[0]));

                if (player == null)
                    return;

                var infoObj = (JObject)args[1];

                var health = infoObj["hp"].ToObject<decimal>();
                var mood = infoObj["mood"].ToObject<decimal>();
                var satiety = infoObj["satiety"].ToObject<decimal>();

                var drugAddiction = infoObj["da"].ToObject<decimal>();

                var isWounded = infoObj["ws"].ToObject<bool>();

                var preDiagnosisType = infoObj["dType"].ToObject<MedicalCard.DiagnoseTypes>();

                var text = Locale.Get("EMS_DIAGNOSTICS_TEXT_0", health, mood, satiety, drugAddiction, isWounded ? Locale.Get("GEN_TEXT_YES_0") : Locale.Get("GEN_TEXT_NO_0"), Locale.Get(MedicalCard.GetDiagnoseNameId(preDiagnosisType)));

                await CEF.ActionBox.ShowText("EMS_SHOWPLAYERDIAGNOSTICS", Locale.Get("EMS_DIAGNOSTICS_TEXT_L", player.GetName(true, false, true)), text, null, null, CEF.ActionBox.DefaultBindAction, (rType) =>
                {
                    CEF.ActionBox.Close(true);
                }, null);
            });
        }
    }
}