using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino
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

    public partial class Casino
    {
        public static DateTime LastSent;

        public Casino(int Id, ushort BuyChipPrice, ushort SellChipPrice, string RoulettesDataJs, string BlackjacksDataJs)
        {
            All.Add(this);

            this.BuyChipPrice = BuyChipPrice;
            this.SellChipPrice = SellChipPrice;

            object[] roulettesData = RAGE.Util.Json.Deserialize<object[]>(RoulettesDataJs);
            object[] blackjacksData = RAGE.Util.Json.Deserialize<object[]>(BlackjacksDataJs);

            if (Id == 0)
            {
                MainColshape = new Circle(new Vector3(963.4196f, 47.85423f, 74.31705f), 80f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
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

                var cashier = new NPC($"Casino@Cashier_{Id}_0",
                    "Эмили",
                    NPC.Types.Talkable,
                    "u_f_m_casinocash_01",
                    new Vector3(978.074f, 38.62385f, 74.88191f),
                    51.26f,
                    Settings.App.Static.MainDimension
                )
                {
                    SubName = "NPC_SUBNAME_CASINO_CASHIER",
                    DefaultDialogueId = "casino_cashier_def",
                    Data = this,
                };
                cashier.Ped.StreamInCustomActionsAdd((entity) =>
                    {
                        var ped = entity as Ped;

                        if (ped == null)
                            return;

                        Animations.Core.Play(ped, new Animation("mini@strip_club@leaning@base", "base_female", 8f, 0f, -1, 0, 0f, false, false, false), -1);
                    }
                );

                //new Additional.RadioEmitter("Casino_0", new Vector3(956.087f, 40.37049f, 79.03804f), 25f, uint.MaxValue, Additional.RadioEmitter.EmitterTypes.se_vw_dlc_casino_main_rm_shop_radio, Sync.Radio.StationTypes.NSPFM);

                //new Additional.RadioEmitter("Casino_1", new Vector3(959.2058f, 29.13904f, 79.03769f), 25f, uint.MaxValue, Additional.RadioEmitter.EmitterTypes.DLC_IE_Steal_Pool_Party_Lake_Vine_Radio_Emitter, Sync.Radio.StationTypes.NSPFM);
            }

            World.Core.AddDataHandler($"CASINO_{Id}_WST", OnWallScreenTypeChanged);

            MainColshape.ApproveType = ApproveTypes.None;

            MainColshape.OnEnter = (cancel) =>
            {
                var taskKey = "CASINO_TASK";

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                    {
                        Vehicle?.Destroy();

                        if (Id == 0)
                            Vehicle = new Vehicle(RAGE.Util.Joaat.Hash("reaper"),
                                new Vector3(963.3792f, 47.93621f, 75.18184f + 1f),
                                238.3463f,
                                "CASINO",
                                255,
                                true,
                                0,
                                0,
                                Settings.App.Static.MainDimension
                            );
                        Vehicle.StreamInCustomActionsAdd((entity) =>
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
                                    },
                                    25,
                                    true,
                                    0
                                );

                                spinTask.Run();

                                veh.SetData("SpinTask", spinTask);
                            }
                        );
                        Vehicle.StreamOutCustomActionsAdd((entity) =>
                            {
                                var veh = entity as Vehicle;

                                if (veh == null)
                                    return;

                                veh.GetData<AsyncTask>("SpinTask")?.Cancel();
                            }
                        );

                        await Streaming.RequestStreamedTextureDict("Prop_Screen_Vinewood");

                        Blackjack.LoadAllRequired();

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            return;

                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(MainColshape.Position.X, MainColshape.Position.Y, MainColshape.Position.Z);

                        if (!RAGE.Game.Interior.IsValidInterior(intId))
                            return;

                        while (!RAGE.Game.Interior.IsInteriorReady(intId))
                        {
                            await RAGE.Game.Invoker.WaitAsync(10);
                        }

                        if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                            return;

                        int renderTargetHandle = Utils.Game.Misc.CreateNamedRenderTargetForModel("casinoscreen_01", RAGE.Util.Joaat.Hash("vw_vwint01_video_overlay"));

                        UpdateCasinoWalls(CurrentWallScreenType);

                        Main.Render -= CasinoWallsRender;
                        Main.Render += CasinoWallsRender;

                        var csName = $"CASINO_G_{Id}";

                        for (var i = 0; i < Roulettes.Length; i++)
                        {
                            Roulette x = Roulettes[i];

                            while (x.TableObject?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            Vector3 coords = x.TableObject.GetCoords(false);

                            var cs = new Cylinder(new Vector3(coords.X, coords.Y, coords.Z - 1f), 2f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                            {
                                InteractionType = InteractionTypes.CasinoRouletteInteract,
                                ActionType = ActionTypes.CasinoInteract,
                                Data = $"{Id}_{i}",
                                Name = csName,
                            };

                            x.TextLabel = new ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2f), "", new RGBA(255, 255, 255, 255), 5f, 90, false, x.TableObject.Dimension)
                            {
                                LOS = false,
                                Font = 4,
                            };

                            x.TextLabel.SetData("Info",
                                new ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2.25f),
                                    $"Рулетка #{i + 1}",
                                    new RGBA(255, 255, 255, 255),
                                    15f,
                                    0,
                                    false,
                                    x.TableObject.Dimension
                                )
                                {
                                    LOS = false,
                                    Font = 7,
                                }
                            );

                            Roulette.OnCurrentStateDataUpdated(Id, i, x.CurrentStateData, true);

                            x.LastBets = new List<Roulette.BetTypes>();
                        }

                        for (var i = 0; i < Blackjacks.Length; i++)
                        {
                            Blackjack x = Blackjacks[i];

                            while (x.TableObject?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            Vector3 coords = x.TableObject.GetCoords(false);

                            var cs = new Cylinder(new Vector3(coords.X, coords.Y, coords.Z - 1f), 2f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                            {
                                InteractionType = InteractionTypes.CasinoBlackjackInteract,
                                ActionType = ActionTypes.CasinoInteract,
                                Data = $"{Id}_{i}",
                                Name = csName,
                            };

                            x.TextLabel = new ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2f), "", new RGBA(255, 255, 255, 255), 5f, 90, false, x.TableObject.Dimension)
                            {
                                LOS = false,
                                Font = 4,
                            };

                            x.TextLabel.SetData("Info",
                                new ExtraLabel(new Vector3(coords.X, coords.Y, coords.Z + 2.25f),
                                    $"Блэкджек #{i + 1}",
                                    new RGBA(255, 255, 255, 255),
                                    15f,
                                    0,
                                    false,
                                    x.TableObject.Dimension
                                )
                                {
                                    LOS = false,
                                    Font = 7,
                                }
                            );

                            Blackjack.OnCurrentStateDataUpdated(Id, i, x.CurrentStateData, true);
                        }

                        for (var i = 0; i < LuckyWheels.Length; i++)
                        {
                            LuckyWheel x = LuckyWheels[i];

                            while (x.BaseObj?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            Vector3 coords = x.BaseObj.GetCoords(false);

                            var cs = new Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 2f, 2f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                            {
                                ActionType = ActionTypes.CasinoInteract,
                                InteractionType = InteractionTypes.CasinoLuckyWheelInteract,
                                Data = $"{Id}_{i}",
                                Name = csName,
                            };
                        }

                        for (var i = 0; i < SlotMachines.Length; i++)
                        {
                            SlotMachine x = SlotMachines[i];

                            while (x.MachineObj?.Exists != true)
                            {
                                await RAGE.Game.Invoker.WaitAsync(5);

                                if (!AsyncTask.Methods.IsTaskStillPending(taskKey, task))
                                    return;
                            }

                            Vector3 coords = x.MachineObj.GetOffsetFromInWorldCoords(0f, -1.15f, 0f);

                            var cs = new Cylinder(new Vector3(coords.X, coords.Y, coords.Z), 0.95f, 2f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                            {
                                ActionType = ActionTypes.CasinoInteract,
                                InteractionType = InteractionTypes.CasinoSlotMachineInteract,
                                Data = $"{Id}_{i}",
                                Name = csName,
                            };
                        }

                        SlotMachine.SoundId = RAGE.Game.Audio.GetSoundId();

                        AsyncTask.Methods.CancelPendingTask("CASINO_TASK");
                    },
                    0,
                    false,
                    0
                );

                AsyncTask.Methods.SetAsPending(task, taskKey);
            };

            MainColshape.OnExit = (cancel) =>
            {
                AsyncTask.Methods.CancelPendingTask("CASINO_TASK");

                if (Vehicle != null)
                {
                    Vehicle.Destroy();

                    Vehicle = null;
                }

                Main.Render -= CasinoWallsRender;

                UpdateCasinoWalls(WallScreenTypes.None);

                ExtraColshape.GetAllByName($"CASINO_G_{Id}").ForEach(x => x.Destroy());

                for (var i = 0; i < Roulettes.Length; i++)
                {
                    Roulette x = Roulettes[i];

                    AsyncTask.Methods.CancelPendingTask($"CASINO_ROULETTE_{Id}_{i}");

                    if (x.TextLabel != null)
                    {
                        x.TextLabel.GetData<AsyncTask>("StateTask")?.Cancel();

                        x.TextLabel.GetData<ExtraLabel>("Info")?.Destroy();

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
                        foreach (Roulette.BetData ab in x.ActiveBets)
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

                for (var i = 0; i < Blackjacks.Length; i++)
                {
                    Blackjack x = Blackjacks[i];

                    AsyncTask.Methods.CancelPendingTask($"CASINO_BLJ_S_{Id}_{i}");
                    AsyncTask.Methods.CancelPendingTask($"CASINO_BLJ_F_{Id}_{i}");
                    AsyncTask.Methods.CancelPendingTask($"CASINO_BLJ_P_{Id}_{i}");

                    if (x.TextLabel != null)
                    {
                        x.TextLabel.GetData<AsyncTask>("StateTask")?.Cancel();

                        x.TextLabel.GetData<ExtraLabel>("Info")?.Destroy();

                        x.TextLabel.Destroy();

                        x.TextLabel = null;
                    }

                    List<Blackjack.BetData> bets = x.NPC.Ped.GetData<List<Blackjack.BetData>>("Bets");

                    if (bets != null)
                    {
                        foreach (Blackjack.BetData b in bets)
                        {
                            b.MapObject?.Destroy();
                        }

                        x.NPC.Ped.ResetData("Bets");
                    }

                    List<Blackjack.CardData> dealerHand = x.NPC.Ped.GetData<List<Blackjack.CardData>>("DHand");

                    if (dealerHand != null)
                    {
                        foreach (Blackjack.CardData h in dealerHand)
                        {
                            h.MapObject?.Destroy();
                        }

                        x.NPC.Ped.ResetData("DHand");
                    }

                    for (var j = 0; j < 4; j++)
                    {
                        var key = $"PHand{j}";

                        List<Blackjack.CardData> playerHand = x.NPC.Ped.GetData<List<Blackjack.CardData>>(key);

                        if (playerHand != null)
                        {
                            foreach (Blackjack.CardData h in dealerHand)
                            {
                                h.MapObject?.Destroy();
                            }

                            x.NPC.Ped.ResetData(key);
                        }
                    }

                    x.CurrentStateData = null;
                }

                for (var i = 0; i < LuckyWheels.Length; i++)
                {
                    LuckyWheel x = LuckyWheels[i];

                    AsyncTask.Methods.CancelPendingTask($"CASINO_LUCKYWHEEL_{Id}_{i}");
                }

                for (var i = 0; i < SlotMachines.Length; i++)
                {
                    SlotMachine x = SlotMachines[i];

                    AsyncTask.Methods.CancelPendingTask($"CASINO_SLOTMACHINE_{Id}_{i}");

                    if (x.Reels != null)
                    {
                        for (var j = 0; j < x.Reels.Length; j++)
                        {
                            x.Reels[j]?.Destroy();
                        }

                        x.Reels = null;
                    }
                }

                if (SlotMachine.SoundId > -1)
                {
                    RAGE.Game.Audio.StopSound(SlotMachine.SoundId);
                    RAGE.Game.Audio.ReleaseSoundId(SlotMachine.SoundId);

                    SlotMachine.SoundId = -1;
                }
            };

            for (var i = 0; i < roulettesData.Length; i++)
            {
                object[] data = ((JArray)roulettesData[i]).ToObject<object[]>();

                Roulettes[i].MinBet = Utils.Convert.ToUInt32(data[0]);
                Roulettes[i].MaxBet = Utils.Convert.ToUInt32(data[1]);
            }

            for (var i = 0; i < blackjacksData.Length; i++)
            {
                object[] data = ((JArray)blackjacksData[i]).ToObject<object[]>();

                Blackjacks[i].MinBet = Utils.Convert.ToUInt32(data[0]);
                Blackjacks[i].MaxBet = Utils.Convert.ToUInt32(data[1]);
            }

            Roulettes[1].TableObject.StreamInCustomActionsAdd((entity) => (entity as MapObject)?.SetTextureVariant(2));
            Roulettes[0].TableObject.StreamInCustomActionsAdd((entity) => (entity as MapObject)?.SetTextureVariant(3));
            Blackjacks[1].TableObject.StreamInCustomActionsAdd((entity) => (entity as MapObject)?.SetTextureVariant(2));
            Blackjacks[0].TableObject.StreamInCustomActionsAdd((entity) => (entity as MapObject)?.SetTextureVariant(3));
        }

        public static List<Casino> All { get; set; } = new List<Casino>();

        public int Id => All.IndexOf(this);

        public Roulette[] Roulettes { get; set; }
        public LuckyWheel[] LuckyWheels { get; set; }
        public SlotMachine[] SlotMachines { get; set; }
        public Blackjack[] Blackjacks { get; set; }

        public ExtraColshape MainColshape { get; set; }

        public WallScreenTypes CurrentWallScreenType => (WallScreenTypes)Utils.Convert.ToByte(World.Core.GetSharedData<object>($"CASINO_{Id}_WST", WallScreenTypes.None));

        public Vehicle Vehicle { get; set; }

        public ushort BuyChipPrice { get; set; }
        public ushort SellChipPrice { get; set; }

        public static Casino GetById(int id)
        {
            return id < 0 || id >= All.Count ? null : All[id];
        }

        public Roulette GetRouletteById(int id)
        {
            return id < 0 || id >= Roulettes.Length ? null : Roulettes[id];
        }

        public LuckyWheel GetLuckyWheelById(int id)
        {
            return id < 0 || id >= LuckyWheels.Length ? null : LuckyWheels[id];
        }

        public SlotMachine GetSlotMachineById(int id)
        {
            return id < 0 || id >= SlotMachines.Length ? null : SlotMachines[id];
        }

        public Blackjack GetBlackjackById(int id)
        {
            return id < 0 || id >= Blackjacks.Length ? null : Blackjacks[id];
        }

        private void CasinoWallsRender()
        {
            int renderTargetHandle = RAGE.Game.Ui.GetNamedRendertargetRenderId("casinoscreen_01");

            RAGE.Game.Invoker.Invoke(0x5F15302936E07111, renderTargetHandle); // SetTextRenderId

            RAGE.Game.Invoker.Invoke(0x61BB1D9B3A95D802, 4); // SetScriptGfxDrawOrder
            RAGE.Game.Invoker.Invoke(0xC6372ECD45D73BCD, true); // SET_SCRIPT_GFX_DRAW_BEHIND_PAUSEMENU
            RAGE.Game.Invoker.Invoke(0x2BC54A8188768488,
                "Prop_Screen_Vinewood",
                "BG_Wall_Colour_4x4",
                0.25f,
                0.5f,
                0.5f,
                1.0f,
                0.0f,
                255,
                255,
                255,
                255
            ); // _DRAW_INTERACTIVE_SPRITE
            RAGE.Game.Graphics.DrawTvChannel(0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 255, 255, 255, 255);
            RAGE.Game.Invoker.Invoke(0x5F15302936E07111, 1); // SetTextRenderId
        }

        private static void UpdateCasinoWalls(WallScreenTypes wsType)
        {
            RAGE.Game.Graphics.SetTvChannel(-1);

            if (wsType == WallScreenTypes.None)
                return;

            Utils.Game.Misc.SetTvChannelPlaylist(0, wsType.ToString(), false);

            RAGE.Game.Graphics.SetTvAudioFrontend(true);
            RAGE.Game.Graphics.SetTvVolume(-100f);
            RAGE.Game.Graphics.SetTvChannel(0);
        }

        private static void OnWallScreenTypeChanged(string key, object value, object oldValue)
        {
            string[] keyD = key.Split('_');

            var casinoId = int.Parse(keyD[1]);

            Casino casino = GetById(casinoId);

            if (!casino.MainColshape.IsInside || AsyncTask.Methods.IsTaskStillPending("CASINO_TASK", null))
                return;

            var newType = (WallScreenTypes)Utils.Convert.ToByte(value ?? WallScreenTypes.None);

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
}