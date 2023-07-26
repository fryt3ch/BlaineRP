using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Game.Animations.Core;

namespace BlaineRP.Client.Game.Fractions
{
    public class EMS : Fraction, IUniformable
    {
        public string[] UniformNames { get; set; }

        public List<MapObject> TempObjects { get; set; }

        public EMS(FractionTypes type, string name, uint storageContainerId, string containerPos, string cWbPos, byte maxRank, string lockerRoomPositionsStr, string creationWorkbenchPricesJs, uint metaFlags, string bedPositionsJs) : base(type, name, storageContainerId, containerPos, cWbPos, maxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(creationWorkbenchPricesJs), metaFlags)
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(lockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Cylinder(pos, 1f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionLockerRoomInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)type}_{i}",
                };

                var lockerRoomText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };
            }

            Utils.Vector4[] mainColshapes = null;

            if (type == FractionTypes.EMS_BLAINE)
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
            else if (type == FractionTypes.EMS_LS)
            {
                mainColshapes = new Utils.Vector4[]
                {
                    new Utils.Vector4(311.2546f, -592.4204f, 42.32737f, 50f),
                };
            }

            Vector3[] bedPositions = RAGE.Util.Json.Deserialize<Vector3[]>(bedPositionsJs);

            foreach (var x in mainColshapes)
            {
                ExtraColshape cs = null;

                cs = new Circle(x.Position, x.RotationZ, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    Name = $"EMS_{(int)type}",

                    ApproveType = ApproveTypes.None,

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

                                bedObj.SetData("EmsBedId", $"{(int)type}_{bedIdx}");

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

            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("documents", 0, "fraction_docs");

            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 10, "fraction_invite");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 11, "ems_heal");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 12, "ems_diag");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 13, "ems_medcard");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 14, "ems_narco");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 15, "ems_psych");
            UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 0, "ems_sellmask");

            UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 16, "player_to_veh");
            UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 17, "player_from_veh");

            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_heal", (entity) => { var player = entity as Player; if (player == null) return; PlayerHeal(player); });
            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_diag", (entity) => { var player = entity as Player; if (player == null) return; PlayerDiagnostics(player); });
            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_medcard", (entity) => { var player = entity as Player; if (player == null) return; PlayerMedicalCard(player); });
            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_narco", (entity) => { var player = entity as Player; if (player == null) return; PlayerDrugHeal(player); });
            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_psych", (entity) => { var player = entity as Player; if (player == null) return; PlayerPsychHeal(player); });
            UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "ems_sellmask", (entity) => { var player = entity as Player; if (player == null) return; PlayerSellMask(player); });

            /*            CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_to_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerToVehicle(veh); });
                        CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_from_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerFromVehicle(veh); });*/
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }

        public bool IsBedOccupied(int bedIdx) => Game.World.Core.GetSharedData<bool>($"EMS::{(int)Type}::BED::{bedIdx}", false);

        private void PlayerHeal(Player player)
        {
            Offers.Request(player, OfferTypes.EmsHeal, null);
        }

        private void PlayerDiagnostics(Player player)
        {
            Offers.Request(player, OfferTypes.EmsDiagnostics, null);
        }

        private void PlayerMedicalCard(Player player)
        {
            Offers.Request(player, OfferTypes.EmsMedicalCard, null);
        }

        private void PlayerDrugHeal(Player player)
        {
            Offers.Request(player, OfferTypes.EmsDrugHeal, null);
        }

        private void PlayerPsychHeal(Player player)
        {
            Offers.Request(player, OfferTypes.EmsPsychHeal, null);
        }

        private void PlayerSellMask(Player player)
        {
            Offers.Request(player, OfferTypes.EmsSellMask, null);
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
                Notification.ShowError($"Для того, чтобы пройти лечение в больнице, кол-во Вашего здоровья должно быть не более, чем 80 ед., Ваше здоровье - {health} ед.");

                return;
            }

            if (Core.LastSent.IsSpam(500, false, true))
                return;

            var heading = obj.GetHeading();
            var pos = obj.GetCoords(false);

            Core.LastSent = Game.World.Core.ServerTime;

            var res = (bool)await RAGE.Events.CallRemoteProc("EMS::BedOcc", fTypeNum, bedIdx);

            if (res)
            {
                Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                Player.LocalPlayer.SetHeading(heading + 240f);

                Core.Play(Player.LocalPlayer, GeneralTypes.BedLie0);

                ExtraColshape cs = null;

                cs = new Cylinder(pos, 1.5f, 2f, false, Utils.Misc.RedColor, Player.LocalPlayer.Dimension, null)
                {
                    ApproveType = ApproveTypes.None,

                    Name = "ems_healing_bed",

                    OnExit = (cancel) =>
                    {
                        if (cs?.Exists != true)
                            return;

                        RAGE.Events.CallRemote("EMS::BedFree");
                    },
                };

                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Вы легли на койку и начали процесс лечения!");
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

                var text = Locale.Get("EMS_DIAGNOSTICS_TEXT_0", health, mood, satiety, drugAddiction, isWounded ? Locale.Get("GEN_TEXT_YES_0") : Locale.Get("GEN_TEXT_NO_0"), Locale.Get(MedicalCard.GetDiagnoseNameKey(preDiagnosisType)));

                await ActionBox.ShowText("EMS_SHOWPLAYERDIAGNOSTICS", Locale.Get("EMS_DIAGNOSTICS_TEXT_L", player.GetName(true, false, true)), text, null, null, ActionBox.DefaultBindAction, (rType) =>
                {
                    ActionBox.Close(true);
                }, null);
            });
        }
    }
}