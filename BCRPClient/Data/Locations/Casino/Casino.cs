using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            public Blackjack[] Blackjacks { get; set; }

            public Additional.ExtraColshape MainColshape { get; set; }

            public WallScreenTypes CurrentWallScreenType => (WallScreenTypes)Utils.ToByte(Sync.World.GetSharedData<object>($"CASINO_{Id}_WST", WallScreenTypes.None));

            public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Length ? null : Roulettes[id];
            public LuckyWheel GetLuckyWheelById(int id) => id < 0 || id >= LuckyWheels.Length ? null : LuckyWheels[id];
            public SlotMachine GetSlotMachineById(int id) => id < 0 || id >= SlotMachines.Length ? null : SlotMachines[id];
            public Blackjack GetBlackjackById(int id) => id < 0 || id >= Blackjacks.Length ? null : Blackjacks[id];

            public Vehicle Vehicle { get; set; }

            public ushort BuyChipPrice { get; set; }
            public ushort SellChipPrice { get; set; }

            public Casino(int Id, ushort BuyChipPrice, ushort SellChipPrice, string RoulettesDataJs, string BlackjacksDataJs)
            {
                All.Add(this);

                this.BuyChipPrice = BuyChipPrice;
                this.SellChipPrice = SellChipPrice;

                var roulettesData = RAGE.Util.Json.Deserialize<object[]>(RoulettesDataJs);
                var blackjacksData = RAGE.Util.Json.Deserialize<object[]>(BlackjacksDataJs);

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

                    Blackjacks = new Blackjack[]
                    {
                        new Blackjack(Id, 0, "vw_prop_casino_blckjack_01", 1010.524f, 67.42336f, 72.27832f, 45f - 32.01f + 90f),
                        new Blackjack(Id, 1, "vw_prop_casino_blckjack_01", 1013.893f, 65.31736f, 72.27832f, 135f - 32.01f + 90f),

                        new Blackjack(Id, 2, "vw_prop_casino_blckjack_01b", 1013.794f, 72.33979f, 72.27832f, -45f - 32.01f + 90f),
                        new Blackjack(Id, 3, "vw_prop_casino_blckjack_01b", 1017.087f, 70.42752f, 72.27832f, -135f - 32.01f + 90f),
                    };

                    LuckyWheels = new LuckyWheel[]
                    {
                        new LuckyWheel(Id, 0, 977.5012f, 49.64366f, 73.67611f, -30f),
                    };

                    SlotMachines = new SlotMachine[]
                    {
                        new SlotMachine(Id, 0, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 1014.009f, 54.50948f, 72.2761f, 66.9941f),
                        new SlotMachine(Id, 1, SlotMachine.ModelTypes.vw_prop_casino_slot_08a, 1013.83f, 55.30534f, 72.2761f, 138.9941f),
                        new SlotMachine(Id, 2, SlotMachine.ModelTypes.vw_prop_casino_slot_07a, 1013.017f, 55.37876f, 72.2761f, 210.9941f),
                        new SlotMachine(Id, 3, SlotMachine.ModelTypes.vw_prop_casino_slot_06a, 1012.695f, 54.62857f, 72.2761f, 282.9941f),
                        new SlotMachine(Id, 4, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 1013.308f, 54.09158f, 72.2761f, 354.9941f),

                        new SlotMachine(Id, 5, SlotMachine.ModelTypes.vw_prop_casino_slot_01a, 1012.662f, 60.35477f, 72.2761f, 66.9941f),
                        new SlotMachine(Id, 6, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 1012.48f, 61.15062f, 72.2761f, 138.9941f),
                        new SlotMachine(Id, 7, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 1011.667f, 61.22473f, 72.2761f, 210.9941f),
                        new SlotMachine(Id, 8, SlotMachine.ModelTypes.vw_prop_casino_slot_03a, 1011.346f, 60.47492f, 72.2761f, 282.9941f),
                        new SlotMachine(Id, 9, SlotMachine.ModelTypes.vw_prop_casino_slot_02a, 1011.961f, 59.93698f, 72.2761f, 354.9941f),

                        new SlotMachine(Id, 10, SlotMachine.ModelTypes.vw_prop_casino_slot_03a, 973.071f, 52.94492f, 73.47611f, 3.9941f),
                        new SlotMachine(Id, 11, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 972.3029f, 53.44027f, 73.47611f, 291.9941f),
                        new SlotMachine(Id, 12, SlotMachine.ModelTypes.vw_prop_casino_slot_02a, 973.7795f, 53.52237f, 73.47611f, 75.9941f),
                        new SlotMachine(Id, 13, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 972.5366f, 54.32385f, 73.47611f, 219.9941f),
                        new SlotMachine(Id, 14, SlotMachine.ModelTypes.vw_prop_casino_slot_01a, 973.4492f, 54.3746f, 73.47611f, 147.9941f),

                        new SlotMachine(Id, 15, SlotMachine.ModelTypes.vw_prop_casino_slot_06a, 978.3527f, 54.91812f, 73.47611f, 3.9941f),
                        new SlotMachine(Id, 16, SlotMachine.ModelTypes.vw_prop_casino_slot_07a, 977.5847f, 55.41346f, 73.47611f, 291.9941f),
                        new SlotMachine(Id, 17, SlotMachine.ModelTypes.vw_prop_casino_slot_08a, 977.8184f, 56.29704f, 73.47611f, 219.9941f),
                        new SlotMachine(Id, 18, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 978.731f, 56.34779f, 73.47611f, 147.9941f),
                        new SlotMachine(Id, 19, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 979.0612f, 55.49556f, 73.47611f, 75.9941f),

                        new SlotMachine(Id, 20, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 982.2341f, 52.22181f, 73.47611f, 291.9941f),
                        new SlotMachine(Id, 21, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 982.4678f, 53.1054f, 73.47611f, 219.9941f),
                        new SlotMachine(Id, 22, SlotMachine.ModelTypes.vw_prop_casino_slot_01a, 983.3804f, 53.15614f, 73.47611f, 147.9941f),
                        new SlotMachine(Id, 23, SlotMachine.ModelTypes.vw_prop_casino_slot_02a, 983.7107f, 52.30392f, 73.47611f, 75.9941f),
                        new SlotMachine(Id, 24, SlotMachine.ModelTypes.vw_prop_casino_slot_03a, 983.0022f, 51.72647f, 73.47611f, 3.9941f),

                        new SlotMachine(Id, 25, SlotMachine.ModelTypes.vw_prop_casino_slot_08a, 982.4944f, 47.44504f, 73.47611f, 219.9941f),
                        new SlotMachine(Id, 26, SlotMachine.ModelTypes.vw_prop_casino_slot_04a, 983.407f, 47.49578f, 73.47611f, 147.9941f),
                        new SlotMachine(Id, 27, SlotMachine.ModelTypes.vw_prop_casino_slot_05a, 983.7373f, 46.64355f, 73.47611f, 75.9941f),
                        new SlotMachine(Id, 28, SlotMachine.ModelTypes.vw_prop_casino_slot_06a, 983.0288f, 46.06611f, 73.47611f, 3.9941f),
                        new SlotMachine(Id, 29, SlotMachine.ModelTypes.vw_prop_casino_slot_07a, 982.2607f, 46.56145f, 73.47611f, 291.9941f),
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

                        Blackjack.LoadAllRequired();

                        if (!Utils.IsTaskStillPending(taskKey, task))
                            return;

                        var intId = RAGE.Game.Interior.GetInteriorAtCoords(MainColshape.Position.X, MainColshape.Position.Y, MainColshape.Position.Z);

                        if (!RAGE.Game.Interior.IsValidInterior(intId))
                            return;

                        while (!RAGE.Game.Interior.IsInteriorReady(intId))
                            await RAGE.Game.Invoker.WaitAsync(10);

                        if (!Utils.IsTaskStillPending(taskKey, task))
                            return;

                        var renderTargetHandle = Utils.CreateNamedRenderTargetForModel("casinoscreen_01", RAGE.Util.Joaat.Hash("vw_vwint01_video_overlay"));

                        UpdateCasinoWalls(CurrentWallScreenType);

                        GameEvents.Render -= CasinoWallsRender;
                        GameEvents.Render += CasinoWallsRender;

                        var csName = $"CASINO_G_{Id}";

                        for (int i = 0; i < Roulettes.Length; i++)
                        {
                            var x = Roulettes[i];

                            while (x.TableObject?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!Utils.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            var coords = x.TableObject.GetCoords(false);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z - 1f), 2f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoRouletteInteract,

                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                Data = $"{Id}_{i}",

                                Name = csName,
                            };

                            x.TextLabel = new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2f), "", new RGBA(255, 255, 255, 255), 5f, 90, false, x.TableObject.Dimension)
                            {
                                LOS = false,
                                Font = 4,
                            };

                            x.TextLabel.SetData("Info", new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2.25f), $"Рулетка #{i + 1}", new RGBA(255, 255, 255, 255), 15f, 0, false, x.TableObject.Dimension) { LOS = false, Font = 7, });

                            Roulette.OnCurrentStateDataUpdated(Id, i, x.CurrentStateData, true);

                            x.LastBets = new List<Roulette.BetTypes>();
                        }

                        for (int i = 0; i < Blackjacks.Length; i++)
                        {
                            var x = Blackjacks[i];

                            while (x.TableObject?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!Utils.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            var coords = x.TableObject.GetCoords(false);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z - 1f), 2f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoBlackjackInteract,

                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                Data = $"{Id}_{i}",

                                Name = csName,
                            };

                            x.TextLabel = new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2f), "", new RGBA(255, 255, 255, 255), 5f, 90, false, x.TableObject.Dimension)
                            {
                                LOS = false,
                                Font = 4,
                            };

                            x.TextLabel.SetData("Info", new Additional.ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2.25f), $"Блэкджек #{i + 1}", new RGBA(255, 255, 255, 255), 15f, 0, false, x.TableObject.Dimension) { LOS = false, Font = 7, });

                            Blackjack.OnCurrentStateDataUpdated(Id, i, x.CurrentStateData, true);
                        }

                        for (int i = 0; i < LuckyWheels.Length; i++)
                        {
                            var x = LuckyWheels[i];

                            while (x.BaseObj?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!Utils.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            var coords = x.BaseObj.GetCoords(false);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 2f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoLuckyWheelInteract,

                                Data = $"{Id}_{i}",

                                Name = csName,
                            };
                        }

                        for (int i = 0; i < SlotMachines.Length; i++)
                        {
                            var x = SlotMachines[i];

                            int objHandle = 0;

                            while ((objHandle = RAGE.Game.Object.GetClosestObjectOfType(x.Position.X, x.Position.Y, x.Position.Z, 1f, RAGE.Util.Joaat.Hash(x.ModelType.ToString()), false, true, true)) <= 0)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!Utils.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            var obj = new MapObject(objHandle)
                            {
                                Dimension = uint.MaxValue,
                            };

                            x.MachineObj = obj;

                            var coords = obj.GetOffsetFromInWorldCoords(0f, -1.15f, 0f);

                            var cs = new Additional.Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 0.95f, 2f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                            {
                                ActionType = Additional.ExtraColshape.ActionTypes.CasinoInteract,

                                InteractionType = Additional.ExtraColshape.InteractionTypes.CasinoSlotMachineInteract,

                                Data = $"{Id}_{i}",

                                Name = csName,
                            };
                        }

                        SlotMachine.SoundId = RAGE.Game.Audio.GetSoundId();

/*                        KeyBinds.Bind(RAGE.Ui.VirtualKeys.X, true, async () =>
                        {
                            var table = Blackjacks[0];

                            for (int i = 0; i < Blackjack.CardOffsets.Length; i++)
                            {
                                for (int j = 0; j < Blackjack.CardOffsets[i].Length; j++)
                                {
                                    var x = Blackjack.CardOffsets[i][j];

                                    var coords = table.TableObject.GetOffsetFromInWorldCoords(x.X, x.Y, x.Z);

                                    var model = RAGE.Util.Joaat.Hash(Blackjack.GetCardModelByType(Blackjack.CardTypes.Club_Ace));

                                    await Utils.RequestModel(model);

                                    var obj = new MapObject(model, coords, new Vector3(0f, 0f, table.TableObject.GetHeading() + x.RotationZ), 255)
                                    {
                                        Dimension = 0,
                                    };

                                    if (i > 0)
                                    {
                                        await table.DealerGiveCard(i - 1, obj);
                                    }
                                    else
                                    {
                                        await table.DealerGiveSelfCard((byte)j, obj);
                                    }
                                }
                            }
                        });*/

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

                    Additional.ExtraColshape.GetAllByName($"CASINO_G_{Id}").ForEach(x => x.Destroy());

                    for (int i = 0; i < Roulettes.Length; i++)
                    {
                        var x = Roulettes[i];

                        Utils.CancelPendingTask($"CASINO_ROULETTE_{Id}_{i}");

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

                        x.CurrentStateData = null;
                    }

                    for (int i = 0; i < Blackjacks.Length; i++)
                    {
                        var x = Blackjacks[i];

                        Utils.CancelPendingTask($"CASINO_BLJ_S_{Id}_{i}");
                        Utils.CancelPendingTask($"CASINO_BLJ_F_{Id}_{i}");
                        Utils.CancelPendingTask($"CASINO_BLJ_P_{Id}_{i}");

                        if (x.TextLabel != null)
                        {
                            x.TextLabel.GetData<AsyncTask>("StateTask")?.Cancel();

                            x.TextLabel.GetData<Additional.ExtraLabel>("Info")?.Destroy();

                            x.TextLabel.Destroy();

                            x.TextLabel = null;
                        }

                        var bets = x.NPC.Ped.GetData<List<Blackjack.BetData>>("Bets");

                        if (bets != null)
                        {
                            foreach (var b in bets)
                            {
                                b.MapObject?.Destroy();
                            }

                            x.NPC.Ped.ResetData("Bets");
                        }

                        var dealerHand = x.NPC.Ped.GetData<List<Blackjack.CardData>>("DHand");

                        if (dealerHand != null)
                        {
                            foreach (var h in dealerHand)
                            {
                                h.MapObject?.Destroy();
                            }

                            x.NPC.Ped.ResetData("DHand");
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            var key = $"PHand{j}";

                            var playerHand = x.NPC.Ped.GetData<List<Blackjack.CardData>>(key);

                            if (playerHand != null)
                            {
                                foreach (var h in dealerHand)
                                {
                                    h.MapObject?.Destroy();
                                }

                                x.NPC.Ped.ResetData(key);
                            }
                        }

                        x.CurrentStateData = null;
                    }

                    for (int i = 0; i < LuckyWheels.Length; i++)
                    {
                        var x = LuckyWheels[i];

                        Utils.CancelPendingTask($"CASINO_LUCKYWHEEL_{Id}_{i}");
                    }

                    for (int i = 0; i < SlotMachines.Length; i++)
                    {
                        var x = SlotMachines[i];

                        Utils.CancelPendingTask($"CASINO_SLOTMACHINE_{Id}_{i}");

                        if (x.Reels != null)
                        {
                            for (int j = 0; j < x.Reels.Length; j++)
                                x.Reels[j]?.Destroy();

                            x.Reels = null;
                        }

                        if (x.MachineObj != null)
                        {
                            x.MachineObj.Destroy();

                            x.MachineObj = null;
                        }
                    }

                    if (SlotMachine.SoundId > -1)
                    {
                        RAGE.Game.Audio.StopSound(SlotMachine.SoundId);
                        RAGE.Game.Audio.ReleaseSoundId(SlotMachine.SoundId);

                        SlotMachine.SoundId = -1;
                    }
                };

                for (int i = 0; i < roulettesData.Length; i++)
                {
                    var data = ((JArray)roulettesData[i]).ToObject<object[]>();

                    Roulettes[i].MinBet = Utils.ToUInt32(data[0]);
                    Roulettes[i].MaxBet = Utils.ToUInt32(data[1]);
                }

                for (int i = 0; i < blackjacksData.Length; i++)
                {
                    var data = ((JArray)blackjacksData[i]).ToObject<object[]>();

                    Blackjacks[i].MinBet = Utils.ToUInt32(data[0]);
                    Blackjacks[i].MaxBet = Utils.ToUInt32(data[1]);
                }

                Roulettes[1].TableObject.SetStreamInCustomAction((entity) => (entity as MapObject)?.SetTextureVariant(2));
                Roulettes[0].TableObject.SetStreamInCustomAction((entity) => (entity as MapObject)?.SetTextureVariant(3));

                Blackjacks[1].TableObject.SetStreamInCustomAction((entity) => (entity as MapObject)?.SetTextureVariant(2));
                Blackjacks[0].TableObject.SetStreamInCustomAction((entity) => (entity as MapObject)?.SetTextureVariant(3));
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

                var newType = (WallScreenTypes)Utils.ToByte(value ?? WallScreenTypes.None);

                UpdateCasinoWalls(newType);
            }

            public static string GetChipPropByAmount(decimal amount)
            {
                if (amount < 10)
                    return "vw_prop_vw_coin_01a";
                else if (amount < 50)
                    return "vw_prop_chip_10dollar_x1";
                else if (amount < 100)
                    return "vw_prop_chip_50dollar_x1";
                else if (amount < 500)
                    return "vw_prop_chip_100dollar_x1";
                else if (amount < 1000)
                    return "vw_prop_chip_500dollar_x1";
                else if (amount < 5000)
                    return "vw_prop_chip_1kdollar_x1";
                else if (amount < 10000)
                    return "vw_prop_chip_5kdollar_x1";

                return "vw_prop_chip_10kdollar_x1";
            }
        }

        public class CasinoEvents : Events.Script
        {
            public CasinoEvents()
            {
                Events.Add("Casino::CB", (args) =>
                {
                    var newBalance = Utils.ToUInt32(args[0]);

                    if (Data.Minigames.Casino.Casino.IsActive)
                    {
                        Data.Minigames.Casino.Casino.UpdateBalance(newBalance);
                    }
                    else
                    {

                    }
                });

                Events.Add("Casino::RLTS", (args) =>
                {
                    var casinoId = (int)args[0];
                    var rouletteId = (int)args[1];
                    var stateData = (string)args[2];

                    Casino.Roulette.OnCurrentStateDataUpdated(casinoId, rouletteId, stateData, false);
                });

                Events.Add("Casino::BLJS", (args) =>
                {
                    var casinoId = (int)args[0];
                    var tableId = (int)args[1];
                    var stateData = (string)args[2];

                    Casino.Blackjack.OnCurrentStateDataUpdated(casinoId, tableId, stateData, false);
                });

                Events.Add("Casino::BLJM", async (args) =>
                {
                    var type = Utils.ToByte(args[0]);

                    if (type == 0) // player anim
                    {
                        var animType = Utils.ToByte(args[1]);

                        var player = RAGE.Elements.Entities.Players.GetAtRemote(Utils.ToUInt16(args[2]));

                        if (player?.Exists != true)
                            return;

                        if (animType == 1)
                        {
                            Sync.Animations.Play(player, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@blackjack@player", "decline_card_001", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                        }
                        else if (animType == 2)
                        {
                            Sync.Animations.Play(player, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@blackjack@player", "request_card", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                        }
                    }
                    else if (type == 1) // chip add
                    {
                        var casinoId = (int)args[1];
                        var tableId = (int)args[2];

                        var casino = Casino.GetById(casinoId);

                        var table = casino.GetBlackjackById(tableId);

                        var ped = table?.NPC?.Ped;

                        if (ped?.Exists != true || table.TableObject?.Exists != true)
                            return;

                        var seatIdx = Utils.ToByte(args[3]);

                        var amount = Utils.ToUInt32(args[4]);

                        var player = RAGE.Elements.Entities.Players.GetAtRemote(Utils.ToUInt16(args[5]));

                        if (player?.Exists == true)
                        {
                            Sync.Animations.Play(player, new Sync.Animations.Animation("anim_casino_b@amb@casino@games@blackjack@player", "place_bet_small", 8f, 1f, -1, 32, 0f, false, false, false), -1);
                        }

                        if (Casino.Blackjack.CurrentTable == table && Casino.Blackjack.CurrentSeatIdx == seatIdx)
                        {
                            if (Data.Minigames.Casino.Casino.CurrentType == Minigames.Casino.Casino.Types.Blackjack)
                            {
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(0, false);
                                Data.Minigames.Casino.Casino.ShowBlackjackButton(1, false);

                                Data.Minigames.Casino.Casino.ShowBlackjackButton(2, amount <= 0);
                            }
                        }

                        var oBets = ped.GetData<List<Casino.Blackjack.BetData>>("Bets");

                        if (oBets == null)
                        {
                            oBets = new List<Casino.Blackjack.BetData>() { new Casino.Blackjack.BetData(), new Casino.Blackjack.BetData(), new Casino.Blackjack.BetData(), new Casino.Blackjack.BetData() };

                            ped.SetData("Bets", oBets);
                        }

                        oBets[seatIdx].Amount = amount;

                        if (amount <= 0)
                            return;

                        var tableHeading = table.TableObject.GetHeading();

                        var offsetInfo = Casino.Blackjack.BetOffsets[seatIdx][0];

                        var objModelStr = Casino.GetChipPropByAmount(amount);

                        var objModelhash = RAGE.Util.Joaat.Hash(objModelStr);

                        Utils.RequestModelNow(objModelhash);

                        var coords = table.TableObject.GetOffsetFromInWorldCoords(offsetInfo.X, offsetInfo.Y, offsetInfo.Z);

                        oBets[seatIdx].MapObject?.Destroy();

                        oBets[seatIdx].MapObject = new MapObject(RAGE.Game.Object.CreateObjectNoOffset(objModelhash, coords.X, coords.Y, coords.Z, false, false, false))
                        {
                            Dimension = uint.MaxValue,
                        };

                        oBets[seatIdx].MapObject.SetRotation(0f, 0f, tableHeading + offsetInfo.RotationZ, 0, false);
                    }
                });

                Events.Add("Casino::LCWS", (args) =>
                {
                    var casinoId = (int)args[0];
                    var luckyWheelId = (int)args[1];

                    var casino = Casino.GetById(casinoId);

                    if (!casino.MainColshape.IsInside || Utils.IsTaskStillPending("CASINO_TASK", null))
                        return;

                    var luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                    var player = RAGE.Elements.Entities.Players.GetAtRemote(Utils.ToUInt16(args[2]));

                    var targetZoneType = (Casino.LuckyWheel.ZoneTypes)Utils.ToByte(args[3]);

                    var resultOffset = Utils.ToSingle(args[4]);

                    luckyWheel.Spin(casinoId, luckyWheelId, player, targetZoneType, resultOffset);
                });

                var casinoSlotMachineIdle0Anim = Sync.Animations.GeneralAnimsList.GetValueOrDefault(Sync.Animations.GeneralTypes.CasinoSlotMachineIdle0);

                if (casinoSlotMachineIdle0Anim != null)
                {
                    casinoSlotMachineIdle0Anim.StartAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(true);
                        ped.SetCollision(false, true);
                    };

                    casinoSlotMachineIdle0Anim.StopAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(false);
                        ped.SetCollision(true, false);
                    };
                }
            }
        }
    }
}