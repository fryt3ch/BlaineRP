using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public enum WallScreenTypes : byte
            {
                None = 0,

                CASINO_DIA_PL,
                CASINO_DIA_SLW_PL,
                CASINO_HLW_PL,
                CASINO_SNWFLK_PL,
                CASINO_WIN_PL,
                CASINO_WIN_SLW_PL,
            }

            public static DateTime LastSent;

            public static List<Casino> All { get; set; } = new List<Casino>();

            public static Casino GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

            public int Id => All.IndexOf(this);

            public Roulette[] Roulettes { get; set; }
            public LuckyWheel[] LuckyWheels { get; set; }
            public SlotMachine[] SlotMachines { get; set; }

            public Additional.ExtraColshape MainColshape { get; set; }

            public WallScreenTypes CurrentWallScreenType => (WallScreenTypes)Convert.ToByte(Sync.World.GetSharedData<object>($"CASINO_{Id}_WST", WallScreenTypes.None));

            public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Length ? null : Roulettes[id];
            public LuckyWheel GetLuckyWheelById(int id) => id < 0 || id >= LuckyWheels.Length ? null : LuckyWheels[id];

            public Vehicle Vehicle { get; set; }

            public ushort BuyChipPrice { get; set; }
            public ushort SellChipPrice { get; set; }

            public Casino(int Id, ushort BuyChipPrice, ushort SellChipPrice, string RoulettesDataJs)
            {
                All.Add(this);

                this.BuyChipPrice = BuyChipPrice;
                this.SellChipPrice = SellChipPrice;

                var roulettesData = RAGE.Util.Json.Deserialize<object[]>(RoulettesDataJs);

                if (Id == 0)
                {
                    MainColshape = new Additional.Circle(new Vector3(963.4196f, 47.85423f, 74.31705f), 80f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {

                    };

                    Roulettes = new Roulette[]
                    {
                        new Roulette(Id, 0, "brp_p_casino_roulette_0", 1024.319f, 62.52872f, 71.4761f, 314.69f - 32.01f + 90f),
                        new Roulette(Id, 1, "brp_p_casino_roulette_0", 1025.035f, 58.33623f, 71.4761f, 134.69f - 32.01f + 90f),

                        new Roulette(Id, 2, "brp_p_casino_roulette_0", 1031.199f, 64.15562f, 71.4761f, 314.69f - 32.01f + 90f),
                        new Roulette(Id, 3, "brp_p_casino_roulette_0", 1032.156f, 60.01989f, 71.4761f, 134.69f - 32.01f + 90f),
                    };

                    LuckyWheels = new LuckyWheel[]
                    {
                        new LuckyWheel(Id, 0, 977.5012f, 49.64366f, 73.67611f, -30f),
                    };

                    SlotMachines = new SlotMachine[]
                    {
                        new SlotMachine(Id, 0, SlotMachine.ModelTypes.vw_prop_casino_slot_03a, 973.071f, 52.94492f, 73.47611f),
                        new SlotMachine(Id, 1, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 972.3029f, 53.44027f, 73.47611f),
                        new SlotMachine(Id, 2, SlotMachine.ModelTypes.vw_prop_casino_slot_02a, 973.7795f, 53.52237f, 73.47611f),
                        new SlotMachine(Id, 3, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 972.5366f, 54.32385f, 73.47611f),
                    };

                    var cashier = new Data.NPC($"Casino@Cashier_{Id}_0", "Анна", NPC.Types.Talkable, "u_f_m_casinocash_01", new Vector3(978.074f, 38.62385f, 74.88191f), 51.26f, Settings.MAIN_DIMENSION)
                    {
                        DefaultDialogueId = "casino_cashier_def",

                        Data = this,
                    };

                    cashier.Ped.SetStreamInCustomAction((entity) =>
                    {
                        var ped = entity as Ped;

                        if (ped == null)
                            return;

                        Sync.Animations.Play(ped, new Sync.Animations.Animation("mini@strip_club@leaning@base", "base_female", 8f, 0f, -1, 0, 0f, false, false, false), -1);
                    });

                    //new Additional.RadioEmitter("Casino_0", new Vector3(956.087f, 40.37049f, 79.03804f), 25f, uint.MaxValue, Additional.RadioEmitter.EmitterTypes.se_vw_dlc_casino_main_rm_shop_radio, Sync.Radio.StationTypes.NSPFM);

                    //new Additional.RadioEmitter("Casino_1", new Vector3(959.2058f, 29.13904f, 79.03769f), 25f, uint.MaxValue, Additional.RadioEmitter.EmitterTypes.DLC_IE_Steal_Pool_Party_Lake_Vine_Radio_Emitter, Sync.Radio.StationTypes.NSPFM);
                }

                Sync.World.AddDataHandler($"CASINO_{Id}_WST", OnWallScreenTypeChanged);

                MainColshape.ApproveType = Additional.ExtraColshape.ApproveTypes.None;

                MainColshape.OnEnter = (cancel) =>
                {
                    var taskKey = "CASINO_TASK";

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                    {
                        Vehicle?.Destroy();

                        if (Id == 0)
                        {
                            Vehicle = new RAGE.Elements.Vehicle(RAGE.Util.Joaat.Hash("reaper"), new Vector3(963.3792f, 47.93621f, 75.18184f + 1f), 238.3463f, "CASINO", 255, true, 0, 0, Settings.MAIN_DIMENSION);
                        }

                        Vehicle.SetStreamInCustomAction((entity) =>
                        {
                            var veh = entity as Vehicle;

                            if (veh?.Exists != true)
                                return;

                            veh.SetInvincible(true);
                            veh.SetCanBeDamaged(false);
                            veh.SetWheelsCanBreak(false);

                            veh.FreezePosition(true);

                            veh.SetNumberPlateTextIndex(1);

                            veh.SetCustomPrimaryColour(255, 0, 0);
                            veh.SetCustomSecondaryColour(0, 0, 0);

                            var spinTask = new AsyncTask(() =>
                            {
                                if (!veh.Exists)
                                    return;

                                veh.SetHeading(veh.GetHeading() + 0.5f);
                            }, 25, true, 0);

                            spinTask.Run();

                            veh.SetData("SpinTask", spinTask);
                        });

                        Vehicle.SetStreamOutCustomAction((entity) =>
                        {
                            var veh = entity as Vehicle;

                            if (veh == null)
                                return;

                            veh.GetData<AsyncTask>("SpinTask")?.Cancel();
                        });

                        await Utils.RequestStreamedTextureDict("Prop_Screen_Vinewood");

                        if (!Utils.IsTaskStillPending(taskKey, task))
                            return;

                        var intId = RAGE.Game.Interior.GetInteriorAtCoords(MainColshape.Position.X, MainColshape.Position.Y, MainColshape.Position.Z);

                        if (!RAGE.Game.Interior.IsValidInterior(intId))
                            return;

                        while (!RAGE.Game.Interior.IsInteriorReady(intId))
                            await RAGE.Game.Invoker.WaitAsync(100);

                        if (!Utils.IsTaskStillPending(taskKey, task))
                            return;

                        var renderTargetHandle = Utils.CreateNamedRenderTargetForModel("casinoscreen_01", RAGE.Util.Joaat.Hash("vw_vwint01_video_overlay"));

                        UpdateCasinoWalls(CurrentWallScreenType);

                        GameEvents.Render -= CasinoWallsRender;
                        GameEvents.Render += CasinoWallsRender;

                        for (int i = 0; i < Roulettes.Length; i++)
                        {
                            var x = Roulettes[i];

                            var coords = x.TableObject.GetCoords(false);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z - 1f), 2f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoRouletteInteract,

                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                Data = $"{Id}_{i}",

                                Name = "CASINO_RLTI",
                            };

                            x.TextLabel = new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2f), "", new RGBA(255, 255, 255, 255), 5f, 90, false, x.TableObject.Dimension)
                            {
                                LOS = false,
                                Font = 4,
                            };

                            x.TextLabel.SetData("Info", new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2.25f), $"Рулетка #{i + 1}", new RGBA(255, 255, 255, 255), 15f, 0, false, x.TableObject.Dimension) { LOS = false, Font = 7, });

                            Roulette.OnCurrentStateDataUpdated($"CASINO_{Id}_RLTS_{i}", Roulette.GetCurrentStateData(Id, i), "ON_LOAD");

                            x.LastBets = new List<Roulette.BetTypes>();
                        }

                        for (int i = 0; i < LuckyWheels.Length; i++)
                        {
                            var x = LuckyWheels[i];

                            var coords = x.BaseObj.GetCoords(false);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 2f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoLuckyWheelInteract,

                                Data = $"{Id}_{i}",

                                Name = $"CASINO_LCWI",
                            };
                        }

                        for (int i = 0; i < SlotMachines.Length; i++)
                        {
                            var x = SlotMachines[i];

                            var objHandle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.ModelType.ToString()), false, true, true);

                            if (objHandle <= 0)
                                continue;

                            var obj = new MapObject(objHandle);

                            //var coords = obj.GetWorldPositionOfBone(obj.GetBoneIndexByName("Chair_Seat_01"));
                            var coords = obj.GetOffsetFromInWorldCoords(0f, -1.25f, 0f);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 1f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoLuckyWheelInteract,

                                Data = $"{Id}_{i}",

                                Name = $"CASINO_LCWI",
                            };
                        }

                        Utils.CancelPendingTask("CASINO_TASK");
                    }, 0, false, 0);

                    Utils.SetTaskAsPending(taskKey, task);
                };

                MainColshape.OnExit = (cancel) =>
                {
                    Utils.CancelPendingTask("CASINO_TASK");

                    if (Vehicle != null)
                    {
                        Vehicle.Destroy();

                        Vehicle = null;
                    }

                    GameEvents.Render -= CasinoWallsRender;

                    UpdateCasinoWalls(WallScreenTypes.None);

                    Additional.ExtraColshape.GetAllByName("CASINO_RLTI").ForEach(x => x.Destroy());
                    Additional.ExtraColshape.GetAllByName("CASINO_LCWI").ForEach(x => x.Destroy());

                    foreach (var x in Roulettes)
                    {
                        if (x.TextLabel != null)
                        {
                            x.TextLabel.GetData<AsyncTask>("StateTask")?.Cancel();

                            x.TextLabel.GetData<Additional.ExtraLabel>("Info")?.Destroy();

                            x.TextLabel.Destroy();

                            x.TextLabel = null;
                        }

                        if (x.LastBets != null)
                        {
                            x.LastBets.Clear();

                            x.LastBets = null;
                        }

                        if (x.ActiveBets != null)
                        {
                            foreach (var ab in x.ActiveBets)
                            {
                                ab.MapObject?.Destroy();
                            }

                            x.ActiveBets.Clear();

                            x.ActiveBets = null;
                        }

                        if (x.BallObject != null)
                        {
                            x.BallObject.Destroy();

                            x.BallObject = null;
                        }
                    }
                };

                for (int i = 0; i < roulettesData.Length; i++)
                {
                    var data = ((JArray)roulettesData[i]).ToObject<object[]>();

                    Roulettes[i].MinBet = Convert.ToUInt32(data[0]);
                    Roulettes[i].MaxBet = Convert.ToUInt32(data[1]);
                }

                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Z, true, () =>
                {
                    /*                    Roulette.ActiveBets = new HashSet<Roulette.BetData>()
                                        {
                                            new Roulette.BetData() { BetType = Roulette.BetTypes._00, Amount = 5_124, },
                                        };

                                        Roulettes[0].StartGame();*/

                    Roulette.CurrentRoulette?.Spin((byte)Utils.Random.Next(0, 38 + 1));
                });
            }

            private void CasinoWallsRender()
            {
                var renderTargetHandle = RAGE.Game.Ui.GetNamedRendertargetRenderId("casinoscreen_01");

                RAGE.Game.Invoker.Invoke(0x5F15302936E07111, renderTargetHandle); // SetTextRenderId

                RAGE.Game.Invoker.Invoke(0x61BB1D9B3A95D802, 4); // SetScriptGfxDrawOrder
                RAGE.Game.Invoker.Invoke(0xC6372ECD45D73BCD, true); // SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU
                RAGE.Game.Invoker.Invoke(0x2BC54A8188768488, "Prop_Screen_Vinewood", "BG_Wall_Colour_4x4", 0.25f, 0.5f, 0.5f, 1.0f, 0.0f, 255, 255, 255, 255); // _DRAW_INTERACTIVE_SPRITE
                RAGE.Game.Graphics.DrawTvChannel(0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 255, 255, 255, 255);
                RAGE.Game.Invoker.Invoke(0x5F15302936E07111, 1); // SetTextRenderId
            }

            private static void UpdateCasinoWalls(WallScreenTypes wsType)
            {
                RAGE.Game.Graphics.SetTvChannel(-1);

                if (wsType == WallScreenTypes.None)
                    return;

                Utils.SetTvChannelPlaylist(0, wsType.ToString(), false);

                RAGE.Game.Graphics.SetTvAudioFrontend(true);
                RAGE.Game.Graphics.SetTvVolume(-100f);
                RAGE.Game.Graphics.SetTvChannel(0);
            }

            private static void OnWallScreenTypeChanged(string key, object value, object oldValue)
            {
                var keyD = key.Split('_');

                var casinoId = int.Parse(keyD[1]);

                var casino = Casino.GetById(casinoId);

                if (!casino.MainColshape.IsInside || Utils.IsTaskStillPending("CASINO_TASK", null))
                    return;

                var newType = (WallScreenTypes)Convert.ToByte(value ?? WallScreenTypes.None);

                UpdateCasinoWalls(newType);
            }
        }

        public class CasinoEvents : Events.Script
        {
            public CasinoEvents()
            {
                Events.Add("Casino::RTSP", (args) =>
                {
                    var casinoId = (int)args[0];
                    var rouletteId = (int)args[1];

                    var casino = Casino.GetById(casinoId);

                    var roulette = casino.GetRouletteById(rouletteId);

                    if (roulette.TableObject?.Exists != true || roulette.NPC?.Ped?.Exists != true)
                        return;

                    var targetNumber = Convert.ToByte(args[2]);

                    roulette.Spin(targetNumber);
                });

                Events.Add("Casino::CB", (args) =>
                {
                    var newBalance = Convert.ToUInt32(args[0]);

                    if (Data.Minigames.Casino.Roulette.IsActive)
                    {
                        Data.Minigames.Casino.Roulette.UpdateBalance(newBalance);
                    }
                    else
                    {

                    }
                });
            }
        }
    }
}