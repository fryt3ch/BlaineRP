using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BCRPClient.CEF
{
    [Script(int.MaxValue)]
    public class Interaction 
    {
        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.Interaction, Browser.IntTypes.Interaction_Passengers); }

        private static List<int> TempBinds { get; set; }

        public class InteractionInfo
        {
            private static Dictionary<string, Dictionary<string, Dictionary<string, Action<Entity>>>> Actions { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<string, Action<Entity>>>>();

            public string MainType { get; private set; }

            public List<string> MainLabels { get; set; }

            public List<List<string>> ExtraLabels { get; set; }

            public List<List<string>> ExtraLabelsTemp { get; set; }

            public InteractionInfo(string MainType)
            {
                this.MainType = MainType;
            }

            public static void AddAction(string mainType, string subType, string type, Action<Entity> action)
            {
                var dict = Actions.GetValueOrDefault(mainType);

                if (dict == null)
                {
                    dict = new Dictionary<string, Dictionary<string, Action<Entity>>>();

                    Actions.Add(mainType, dict);
                }

                var subDict = dict.GetValueOrDefault(subType);

                if (subDict == null)
                {
                    subDict = new Dictionary<string, Action<Entity>>();

                    dict.Add(subType, subDict);
                }

                if (!subDict.TryAdd(type, action))
                    subDict[type] = action;
            }

            public void AddAction(string subType, string type, Action<Entity> action) => AddAction(MainType, subType, type, action);

            public static Action<Entity> GetAction(string mainType, string subType, string type) => Actions.GetValueOrDefault(mainType)?.GetValueOrDefault(subType)?.GetValueOrDefault(type);

            public Action<Entity> GetAction(string subType, string type) => GetAction(MainType, subType, type);

            public void ReplaceExtraLabel(string subType, int pIdx, string type)
            {
                var mIdx = MainLabels.IndexOf(subType);

                if (mIdx < 0)
                    return;

/*                if (pIdx < 0 || pIdx >= MainLabels.Count * 2)
                    return;*/

                var eLabels = ExtraLabels[mIdx];

                eLabels[pIdx] = type;
            }

            public void ReplaceExtraLabelTemp(string subType, int pIdx, string type)
            {
                var mIdx = MainLabels.IndexOf(subType);

                if (mIdx < 0)
                    return;

                if (pIdx < 0 || pIdx >= MainLabels.Count * 2)
                    return;

                if (ExtraLabelsTemp == null)
                {
                    ExtraLabelsTemp = new List<List<string>>();

                    foreach (var x in ExtraLabels)
                    {
                        var t = new List<string>();

                        t.AddRange(x);

                        ExtraLabelsTemp.Add(t);
                    }
                }

                var eLabels = ExtraLabelsTemp[mIdx];

                eLabels[pIdx] = type;
            }
        }

        public static InteractionInfo CharacterInteractionInfo { get; set; } = new InteractionInfo("char")
        {
            MainLabels = new List<string>()
            {
                "interact", "trade", "property", "money", "heal", "char_job", "documents",
            },

            ExtraLabels = new List<List<string>>()
            {
                new List<string>() { "carry", "coin", null, null, null, null, null, null, null, null, null, null, null, null, "handshake", "kiss", },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, "sell_house", "sell_car", "sell_buis", "settle", null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, "money_50", "money_150", "money_300", "money_1000", null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, "pulse", "bandage", "cure_0", "cure_1", null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, "char_veh", "medbook", "resume", "license", "passport", },
            },
        };

        public static InteractionInfo InVehicleInteractionInfo { get; set; } = new InteractionInfo("in_veh")
        {
            MainLabels = new List<string>()
            {
                "doors", "seat", "trunk", "hood", "music", "passengers", "park", "vehdoc", "job", "other_down",
            },

            ExtraLabels = new List<List<string>>()
            {
                new List<string>() { "open", "close", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, "s_one", "s_two", "s_three", "s_four", "s_trunk", null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, "open", "close", null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, "open", "close", null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            },
        };

        public static InteractionInfo OutVehicleInteractionInfo { get; set; } = new InteractionInfo("out_veh")
        {
            MainLabels = new List<string>()
            {
                "doors", "seat", "trunk", "hood", "push", "other", "park", "vehdoc", "job", "gas",
            },

            ExtraLabels = new List<List<string>>()
            {
                new List<string>() { "open", "close", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, "s_one", "s_two", "s_three", "s_four", "s_trunk", null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, "look", "open", "close", null, null, null, null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, "look", "open", "close", null, null, null, null, null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, "junkyard", "remove_np", "put_np", "fix", null, null, null, null, null, null, null, },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },

                new List<string>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            },
        };

        public enum PassengersMenuActions
        {
            Interact = 0, Kick,
        }

        public Interaction()
        {
            TempBinds = new List<int>();

            Events.Add("Interaction::Select", (args) =>
            {
                if (args == null)
                    return;

                var mainType = args.Length < 1 ? null : (string)args[0];

                var subType = args.Length > 1 ? (string)args[1] : null;

                var type = args.Length > 2 ? (string)args[2] : null;

                if (mainType == null)
                    return;

                if (subType == null)
                    return;

                var action = InteractionInfo.GetAction(mainType, subType, type ?? string.Empty);

                if (action == null)
                    return;

                CloseMenu();

                action.Invoke(BCRPClient.Interaction.CurrentEntity);
            });

            Events.Add("Interaction::Close", (args) => CloseMenu());

            OutVehicleInteractionInfo.AddAction("doors", "open", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.Lock(false, veh); });
            OutVehicleInteractionInfo.AddAction("doors", "close", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.Lock(true, veh); });
            OutVehicleInteractionInfo.AddAction("doors", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.Lock(null, veh); });

            OutVehicleInteractionInfo.AddAction("push", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.PushVehicle.Toggle(veh); });

            OutVehicleInteractionInfo.AddAction("trunk", "look", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.ShowContainer(veh); });
            OutVehicleInteractionInfo.AddAction("trunk", "open", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.ToggleTrunkLock(false, veh); });
            OutVehicleInteractionInfo.AddAction("trunk", "close", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.ToggleTrunkLock(true, veh); });
            OutVehicleInteractionInfo.AddAction("trunk", "", OutVehicleInteractionInfo.GetAction("trunk", "look"));

            OutVehicleInteractionInfo.AddAction("hood", "look", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.LookHood(veh); });
            OutVehicleInteractionInfo.AddAction("hood", "open", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.ToggleHoodLock(false, veh); });
            OutVehicleInteractionInfo.AddAction("hood", "close", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.ToggleHoodLock(true, veh); });
            OutVehicleInteractionInfo.AddAction("hood", "", OutVehicleInteractionInfo.GetAction("hood", "look"));

            OutVehicleInteractionInfo.AddAction("seat", "", (entity) =>
            {
                var veh = entity as Vehicle; if (veh == null) return;

                var freeSeats = new List<(decimal, string)>();

                for (int i = -1; i < veh.GetMaxNumberOfPassengers(); i++)
                {
                    if (veh.IsSeatFree(i, 0))
                        freeSeats.Add((i + 1, Locale.Get("POLICE_PTOVEH_L_0", i + 2)));
                }

                if (Player.LocalPlayer.Vehicle != veh)
                {
                    var trunkAttach = Sync.AttachSystem.GetEntityEntityAttachments(veh)?.Where(x => x.Type == Sync.AttachSystem.Types.VehicleTrunk).FirstOrDefault();

                    if (trunkAttach == null && veh.DoesHaveDoor(5) > 0)
                        freeSeats.Add((int.MaxValue, Locale.Get("POLICE_PTOVEH_L_1")));
                }

                if (freeSeats.Count == 0)
                {
                    CEF.Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_0"));
                }
                else if (freeSeats.Count == 1)
                {
                    Sync.Vehicles.SeatTo((int)freeSeats[0].Item1, veh);
                }
                else
                {
                    CEF.ActionBox.ShowSelect("PlayerInteractVehicleSeatToSelect", Locale.Get("POLICE_PTOVEH_L_2"), freeSeats.ToArray(), null, null, CEF.ActionBox.DefaultBindAction, (rType, id) =>
                    {
                        if (rType != CEF.ActionBox.ReplyTypes.OK)
                        {
                            CEF.ActionBox.Close(true);

                            return;
                        }

                        var seatIdx = (int)id;

                        CEF.ActionBox.Close(true);

                        Sync.Vehicles.SeatTo(seatIdx, veh);
                    }, null);
                }
            });

            OutVehicleInteractionInfo.AddAction("seat", "s_one", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SeatTo(0, veh); });
            OutVehicleInteractionInfo.AddAction("seat", "s_two", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SeatTo(1, veh); });
            OutVehicleInteractionInfo.AddAction("seat", "s_three", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SeatTo(2, veh); });
            OutVehicleInteractionInfo.AddAction("seat", "s_four", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SeatTo(3, veh); });
            OutVehicleInteractionInfo.AddAction("seat", "s_trunk", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SeatTo(int.MaxValue, veh); });

            OutVehicleInteractionInfo.AddAction("gas", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; CEF.Gas.RequestShow(veh); });

            OutVehicleInteractionInfo.AddAction("park", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.Park(veh); });

            OutVehicleInteractionInfo.AddAction("other", "remove_np", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.TakePlate(veh); });
            OutVehicleInteractionInfo.AddAction("other", "put_np", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.SetupPlate(veh); });
            OutVehicleInteractionInfo.AddAction("other", "fix", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.FixVehicle(veh); });
            OutVehicleInteractionInfo.AddAction("other", "junkyard", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Data.Locations.VehicleDestruction.VehicleDestruct(veh); });

            OutVehicleInteractionInfo.AddAction("vehdoc", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Events.CallRemote("Vehicles::ShowPass", veh); });

            OutVehicleInteractionInfo.AddAction("other", "trailer", (entity) => { var veh = entity as Vehicle; if (veh == null) return; Sync.Vehicles.BoatFromTrailerToWater(veh); });


            InVehicleInteractionInfo.AddAction("doors", "open", OutVehicleInteractionInfo.GetAction("doors", "open"));
            InVehicleInteractionInfo.AddAction("doors", "close", OutVehicleInteractionInfo.GetAction("doors", "close"));
            InVehicleInteractionInfo.AddAction("doors", "", OutVehicleInteractionInfo.GetAction("doors", ""));

            InVehicleInteractionInfo.AddAction("trunk", "open", OutVehicleInteractionInfo.GetAction("trunk", "open"));
            InVehicleInteractionInfo.AddAction("trunk", "close", OutVehicleInteractionInfo.GetAction("trunk", "close"));

            InVehicleInteractionInfo.AddAction("hood", "open", OutVehicleInteractionInfo.GetAction("hood", "open"));
            InVehicleInteractionInfo.AddAction("hood", "close", OutVehicleInteractionInfo.GetAction("hood", "close"));

            InVehicleInteractionInfo.AddAction("seat", "", OutVehicleInteractionInfo.GetAction("seat", ""));

            InVehicleInteractionInfo.AddAction("seat", "s_one", OutVehicleInteractionInfo.GetAction("seat", "s_one"));
            InVehicleInteractionInfo.AddAction("seat", "s_two", OutVehicleInteractionInfo.GetAction("seat", "s_two"));
            InVehicleInteractionInfo.AddAction("seat", "s_three", OutVehicleInteractionInfo.GetAction("seat", "s_three"));
            InVehicleInteractionInfo.AddAction("seat", "s_four", OutVehicleInteractionInfo.GetAction("seat", "s_four"));

            InVehicleInteractionInfo.AddAction("passengers", "", (entity) => { var veh = entity as Vehicle; if (veh == null) return; ShowPassengers(); });

            InVehicleInteractionInfo.AddAction("vehdoc", "", OutVehicleInteractionInfo.GetAction("vehdoc", ""));

            InVehicleInteractionInfo.AddAction("park", "", OutVehicleInteractionInfo.GetAction("park", ""));

            //InVehicleInteractionInfo.AddAction("other_down", "", OutVehicleInteractionInfo.GetAction("gas", ""));

            CharacterInteractionInfo.AddAction("interact", "coin", (entity) => { var player = entity as Player; if (player == null) return; Sync.Offers.Request(player, Sync.Offers.Types.HeadsOrTails, null); });
            CharacterInteractionInfo.AddAction("interact", "handshake", (entity) => { var player = entity as Player; if (player == null) return; Sync.Offers.Request(player, Sync.Offers.Types.Handshake, null); });
            CharacterInteractionInfo.AddAction("interact", "carry", (entity) => { var player = entity as Player; if (player == null) return; Sync.Offers.Request(player, Sync.Offers.Types.Carry, null); });

            CharacterInteractionInfo.AddAction("money", "money_50", (entity) => { var player = entity as Player; if (player == null) return; PlayerCashRequest(player, 50); });
            CharacterInteractionInfo.AddAction("money", "money_150", (entity) => { var player = entity as Player; if (player == null) return; PlayerCashRequest(player, 150); });
            CharacterInteractionInfo.AddAction("money", "money_300", (entity) => { var player = entity as Player; if (player == null) return; PlayerCashRequest(player, 300); });
            CharacterInteractionInfo.AddAction("money", "money_1000", (entity) => { var player = entity as Player; if (player == null) return; PlayerCashRequest(player, 1000); });
            CharacterInteractionInfo.AddAction("money", "", (entity) => { var player = entity as Player; if (player == null) return; PlayerCashRequest(player, 0); });

            CharacterInteractionInfo.AddAction("trade", "", (entity) => { var player = entity as Player; if (player == null) return; Sync.Offers.Request(player, Sync.Offers.Types.Exchange, null); });

            CharacterInteractionInfo.AddAction("property", "settle", (entity) => { var player = entity as Player; if (player == null) return; PlayerSettleRequest(player); });
            CharacterInteractionInfo.AddAction("property", "sell_house", (entity) => { var player = entity as Player; if (player == null) return; PlayerSellPropertyRequest(player, 2); });
            CharacterInteractionInfo.AddAction("property", "sell_car", (entity) => { var player = entity as Player; if (player == null) return; PlayerSellPropertyRequest(player, 0); });
            CharacterInteractionInfo.AddAction("property", "sell_buis", (entity) => { var player = entity as Player; if (player == null) return; PlayerSellPropertyRequest(player, 1); });

            CharacterInteractionInfo.AddAction("documents", "medbook", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocumentsRequest(player, 0); });
            CharacterInteractionInfo.AddAction("documents", "passport", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocumentsRequest(player, 1); });
            CharacterInteractionInfo.AddAction("documents", "char_veh", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocumentsRequest(player, 2); });
            CharacterInteractionInfo.AddAction("documents", "license", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocumentsRequest(player, 3); });

            CharacterInteractionInfo.AddAction("heal", "pulse", (entity) => { var player = entity as Player; if (player == null) return; ResurrectPlayer(player); });
            CharacterInteractionInfo.AddAction("heal", "bandage", (entity) => { var player = entity as Player; if (player == null) return; GivePlayerHealingItem(player, "med_b_0"); });
            CharacterInteractionInfo.AddAction("heal", "cure_0", (entity) => { var player = entity as Player; if (player == null) return; GivePlayerHealingItem(player, "med_kit_0"); });
            CharacterInteractionInfo.AddAction("heal", "cure_1", (entity) => { var player = entity as Player; if (player == null) return; GivePlayerHealingItem(player, "med_kit_ems_0"); });

            Events.Add("Interaction::PassengersMenuSelect", (args) =>
            {
                var action = (PassengersMenuActions)(int)args[0];
                var id = Utils.ToUInt16(args[1]);

                CloseMenu();

                if (action == PassengersMenuActions.Interact)
                {
                    PlayerInteraction(id);
                }
                else if (action == PassengersMenuActions.Kick)
                {
                    PlayerKick(id);
                }
                else
                {
                    return;
                }
            });
        }

        public static bool TryShowMenu()
        {
            if (BCRPClient.Interaction.CurrentEntity == null || IsActive)
                return false;

            var entity = BCRPClient.Interaction.CurrentEntity;

            if (Utils.IsAnyCefActive())
                return false;

            BCRPClient.Interaction.Enabled = false;

            BCRPClient.Interaction.CurrentEntity = entity;

            if (entity is Vehicle vehicle)
            {
                if (RAGE.Elements.Player.LocalPlayer.Vehicle == null)
                {
                    if (Data.Vehicles.GetByModel(vehicle.Model)?.Type == Data.Vehicles.Vehicle.Types.Boat)
                    {
                        OutVehicleInteractionInfo.ReplaceExtraLabelTemp("other", 8, "trailer");
                    }

                    ShowMenu(OutVehicleInteractionInfo);
                }
                else
                {
                    ShowMenu(InVehicleInteractionInfo);
                }

                GameEvents.Render -= CheckEntityDistance;
                GameEvents.Render += CheckEntityDistance;

                return true;
            }
            else if (entity is Player player)
            {
                ShowMenu(CharacterInteractionInfo);

                GameEvents.Render -= CheckEntityDistance;
                GameEvents.Render += CheckEntityDistance;

                return true;
            }
            else if (entity is Ped ped)
            {
                // todo, if needed
            }
            else if (entity is MapObject obj)
            {
                if (obj.IsLocal)
                {
                    if (obj.HasData("Furniture"))
                    {
                        if (obj.GetData<Data.Furniture>("Furniture") is Data.Furniture fData)
                        {
                            fData.InteractionAction?.Invoke(obj);
                        }
                    }
                    else if (obj.HasData("CustomAction"))
                    {
                        var cAction = obj.GetData<Action<MapObject>>("CustomAction");

                        cAction?.Invoke(obj);
                    }
                }
                else
                {
                    if (obj.GetSharedData<int>("IOG") == 1)
                    {
                        var iog = Sync.World.ItemOnGround.GetItemOnGroundObject(obj);

                        if (iog != null)
                        {
                            CEF.ActionBox.ShowSelect
                            (
                                "PlacedItemOnGroundSelect", Locale.Actions.PlacedItemOnGroundSelectHeader, new (decimal, string)[] { (0, Locale.Actions.PlacedItemOnGroundSelectInteract), (1, iog.IsLocked ? Locale.Actions.PlacedItemOnGroundSelectUnlock : Locale.Actions.PlacedItemOnGroundSelectLock), (2, Locale.Actions.PlacedItemOnGroundSelectTake) }, null, null,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, id) =>
                                {
                                    CEF.ActionBox.Close(true);

                                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                                    {
                                        if (iog?.Object?.Exists != true)
                                            return;

                                        if (id == 0)
                                        {
                                            CEF.Inventory.Show(Inventory.Types.Workbench, 0, iog.Uid);
                                        }
                                        else if (id == 1)
                                        {
                                            Events.CallRemote("Item::IOGL", iog.Uid, !iog.IsLocked);
                                        }
                                        else if (id == 2)
                                        {
                                            iog.TakeItem();
                                        }
                                    }
                                },

                                null
                            );
                        }
                    }
                }
            }

            BCRPClient.Interaction.Enabled = true;

            return false;
        }

        public static void ShowMenu(InteractionInfo info)
        {
            Browser.Switch(Browser.IntTypes.Interaction, true);

            var extraLabels = info.ExtraLabels;

            if (info.ExtraLabelsTemp != null)
            {
                extraLabels = info.ExtraLabelsTemp;

                info.ExtraLabelsTemp = null;
            }

            Browser.Window.ExecuteJs("Interaction.draw", info.MainType, info.MainLabels, extraLabels.Select(x => x == null ? x : x.Select(y => y ?? "none").ToList()));

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        private static async void ShowPassengers()
        {
            var veh = Player.LocalPlayer.Vehicle;

            if (veh == null)
                return;

            var vehData = Sync.Vehicles.GetData(veh);

            if (vehData == null)
                return;

            var players = new List<object>();

            foreach (var x in Sync.Vehicles.GetPlayersInVehicle(veh))
            {
                if (x.Handle == Player.LocalPlayer.Handle)
                    continue;

                var data = Sync.Players.GetData(x);

                players.Add(new object[] { x.RemoteId, data == null ? "null" : x.GetName(true, false, true) });
            }

            // If no Passengers
            if (players.Count == 0)
            {
                CEF.Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_1"));

                return;
            }

            await Browser.Render(Browser.IntTypes.Interaction_Passengers, true, true);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Browser.Window.ExecuteJs($"Passengers.fill", players);

            Cursor.Show(true, true);
        }

        public static void PlayerInteraction(ushort id)
        {
            if (Player.LocalPlayer.Vehicle == null)
                return;

            var player = Entities.Players.GetAtRemote(id);

            if (player?.Exists != true)
                return;

            CloseMenu();

            BCRPClient.Interaction.Enabled = false;

            BCRPClient.Interaction.CurrentEntity = player;

            TryShowMenu();
        }

        public static void PlayerKick(ushort id)
        {
            if (Player.LocalPlayer.Vehicle == null)
                return;

            var player = Entities.Players.GetAtRemote(id);

            if (player?.Exists != true)
                return;

            Sync.Vehicles.KickPassenger(player);

            CloseMenu();
        }

        public static void CloseMenu()
        {
            if (!IsActive)
                return;

            Browser.Switch(Browser.IntTypes.Interaction, false);
            Browser.Render(Browser.IntTypes.Interaction_Passengers, false);

            GameEvents.Render -= CheckEntityDistance;

            KeyBinds.Get(KeyBinds.Types.Interaction).Enable();

            foreach (var x in TempBinds)
                KeyBinds.Unbind(x);

            TempBinds.Clear();

            Cursor.Show(false, false);

            BCRPClient.Interaction.Enabled = true;
        }

        private static void CheckEntityDistance()
        {
            if (BCRPClient.Interaction.CurrentEntity?.IsNull != false || Vector3.Distance(Player.LocalPlayer.Position, BCRPClient.Interaction.CurrentEntity.Position) > Settings.App.Static.EntityInteractionMaxDistance)
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                CloseMenu();
            }
        }

        public static async void PlayerCashRequest(Player player, int amount)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (amount < 0)
                return;

            if (amount == 0)
            {
                if (pData.Cash <= 0)
                {
                    CEF.Notification.Show("Trade::NotEnoughMoney");

                    return;
                }

                await CEF.ActionBox.ShowRange
                (
                    "GiveCash", string.Format(Locale.Actions.GiveCash, player.GetName(true, false, true)), 1, pData.Cash, pData.Cash / 2, -1, ActionBox.RangeSubTypes.Default,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, amountD) =>
                    {
                        int amount;

                        if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                            return;

                        CEF.ActionBox.Close(true);

                        if (rType == CEF.ActionBox.ReplyTypes.OK)
                        {
                            if (player is Player targetPlayer)
                                Sync.Offers.Request(targetPlayer, Sync.Offers.Types.Cash, new { Amount = amount, });
                        }
                    },

                    null
                );
            }
            else
            {
                if (pData.Cash <= (ulong)amount)
                {
                    CEF.Notification.Show("Trade::NotEnoughMoney");

                    return;
                }

                Sync.Offers.Request(player, Sync.Offers.Types.Cash, new { Amount = amount, });
            }
        }

        public static void PlayerSettleRequest(Player player)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var currentHouse = Player.LocalPlayer.GetData<Data.Locations.HouseBase>("House::CurrentHouse");

            if (currentHouse == null)
            {
                CEF.Notification.ShowError(Locale.Notifications.House.NotInAnyHouseOrApartments);

                return;
            }

            if (!pData.OwnedHouses.Contains(currentHouse))
            {
                CEF.Notification.ShowError(Locale.Notifications.House.NotAllowed);

                return;
            }

            Sync.Offers.Request(player, Sync.Offers.Types.Settle, null);
        }

        public static void PlayerSellPropertyRequest(Player player, byte type)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (type == 0)
            {
                if (pData.OwnedVehicles.Count == 0)
                {
                    CEF.Notification.ShowError(Locale.Notifications.Vehicles.NoOwnedVehicles);

                    return;
                }

                CEF.Estate.ShowSellVehicle(player, true);
            }
            else if (type == 1)
            {
                if (pData.OwnedBusinesses.Count == 0)
                {
                    CEF.Notification.ShowError(Locale.Notifications.General.NoOwnedBusiness);

                    return;
                }

                CEF.Estate.ShowSellBusiness(player, true);
            }
            else if (type == 2)
            {
                if (pData.OwnedApartments.Count == 0 && pData.OwnedHouses.Count == 0 && pData.OwnedGarages.Count == 0)
                {
                    CEF.Notification.ShowError(Locale.Notifications.General.NoOwnedEstate);

                    return;
                }

                CEF.Estate.ShowSellEstate(player, true);
            }
        }

        public static void PlayerShowDocumentsRequest(Player player, byte type)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (type == 0)
            {
                if (pData.MedicalCard == null)
                {
                    CEF.Notification.ShowError(Locale.Notifications.General.NoMedicalCard);

                    return;
                }

                Sync.Offers.Request(player, Sync.Offers.Types.ShowMedicalCard, null);
            }
            else if (type == 1)
            {
                Sync.Offers.Request(player, Sync.Offers.Types.ShowPassport, null);
            }
            else if (type == 2)
            {
                var allVehs = pData.OwnedVehicles;

                if (allVehs.Count == 0)
                {
                    CEF.Notification.ShowError(Locale.Notifications.Vehicles.NoOwnedVehicles);

                    return;
                }

/*                if (allVehs.Count == 1)
                {
                    Sync.Offers.Request(player, Sync.Offers.Types.ShowVehiclePassport, allVehs[0].VID);

                    return;
                }*/

                var t = 0;

                CEF.ActionBox.ShowSelect
                (
                    "VehiclePassportSelect", Locale.Actions.VehiclePassportSelectHeader, allVehs.Select(x => ((decimal)t++, $"{x.Data.SubName} [#{x.VID}]")).ToArray(), null, null,

                    CEF.ActionBox.DefaultBindAction,

                    (rType, idD) =>
                    {
                        var id = (int)idD;

                        if (rType == CEF.ActionBox.ReplyTypes.OK)
                        {
                            var pData = Sync.Players.GetData(Player.LocalPlayer);

                            if (pData == null)
                                return;

                            var allVehs = pData.OwnedVehicles;

                            if (allVehs.Count <= id)
                            {
                                CEF.ActionBox.Close(true);

                                return;
                            }

                            CEF.ActionBox.Close(true);

                            Sync.Offers.Request(player, Sync.Offers.Types.ShowVehiclePassport, new { VID = allVehs[id].VID, });
                        }
                        else if (rType == CEF.ActionBox.ReplyTypes.Cancel)
                        {
                            CEF.ActionBox.Close(true);
                        }
                        else
                            return;
                    },

                    null
                );
            }
            else if (type == 3)
            {
                Sync.Offers.Request(player, Sync.Offers.Types.ShowLicenses, null);
            }
        }

        public static async void ResurrectPlayer(Player player)
        {
            var pData = Sync.Players.GetData(player);

            if (pData == null)
                return;

            if (!pData.IsKnocked)
            {
                CEF.Notification.ShowError(Locale.Get("NTFC_PLAYER_RESURRECT_E_0"));

                return;
            }

            var items = new Dictionary<string, int>();

            for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
            {
                var item = CEF.Inventory.ItemsParams[i];

                if (item == null)
                    continue;

                if (item.Id == "med_kit_0" || item.Id == "med_kit_ems_0")
                    items.TryAdd(item.Id, i);
            }

            if (items.Count == 0)
            {
                CEF.Notification.ShowError(Locale.Get("NTFC_ITEMS_NO_MED_KIT_RESURRECT_0"));

                return;
            }
            else if (items.Count == 1)
            {
                proceed(items.FirstOrDefault().Value);

                return;
            }

            await CEF.ActionBox.ShowSelect("RESURRECT_PLAYER_ITEM", "Реанимировать {0}", items.Select(x => ((decimal)x.Value, Data.Items.GetName(x.Key))).ToArray(), null, null, CEF.ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType == ActionBox.ReplyTypes.Cancel)
                {
                    CEF.ActionBox.Close(true);
                }
                else
                {
                    proceed((int)id);
                }
            }, null);

            async void proceed(int itemIdx)
            {
                var res = (int)await Events.CallRemoteProc("Player::ResurrectItem", player, itemIdx);
            }
        }

        public static void GivePlayerHealingItem(Player player, string itemId)
        {
            var pData = Sync.Players.GetData(player);

            if (pData == null)
                return;

            int itemIdx = -1;

            for (int i = 0; i < CEF.Inventory.ItemsParams.Length; i++)
            {
                if (CEF.Inventory.ItemsParams[i]?.Id == itemId)
                {
                    itemIdx = i;

                    break;
                }
            }

            if (itemIdx < 0)
            {
                CEF.Notification.Show("Inventory::NoItem");

                return;
            }

            Sync.Offers.Request(player, Sync.Offers.Types.GiveHealingItem, new { ItemIdx = itemIdx, });
        }
    }
}
