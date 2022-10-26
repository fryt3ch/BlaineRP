using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using static BCRPClient.Locale.Notifications.Vehicles;

namespace BCRPClient.CEF
{
    class Interaction : Events.Script
    {
        private static DateTime LastSwitched;

        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.Interaction_Character, Browser.IntTypes.Interaction_Vehicle_In, Browser.IntTypes.Interaction_Vehicle_Out, Browser.IntTypes.Interaction_Passengers); }

        private static Type CurrentType = Type.None;

        private static Utils.Actions[] ActionsToCheck = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            //Utils.Actions.Crouch,
            Utils.Actions.Crawl,
            Utils.Actions.Finger,
            Utils.Actions.PushingVehicle,

            //Utils.Actions.Animation,
            //Utils.Actions.CustomAnimation,
            //Utils.Actions.Scenario,

            //Utils.Actions.InVehicle,
            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading, //Utils.Actions.HasWeapon,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, //Utils.Actions.OnFoot,
        };

        private static List<int> TempBinds { get; set; }

        #region Enums
        private enum Type
        {
            None = -1, PlayerMenu, InVehicle, OutVehicle
        }

        public enum PlayerAction
        {
            Interact = 0, Carry, Coin, Handshake, Kiss,
            Trade,
            Property, SellHouse, SellVehicle, SellBuisiness, Settle,
            Money, Money_50, Money_150, Money_300, Money_1000,
            Heal, HealFirst, HealBandage, HealCure,
            Job,
            Documents, DocumentsVehicle, DocumentsMedical, DocumentsLicenses, DocumentsPassport,
            Close
        }

        public enum InVehicleAction
        {
            Doors = 0, DoorsOpen, DoorsClose,
            Seat, SeatOne, SeatTwo, SeatThree, SeatFour, SeatTrunk,
            Trunk, TrunkLook, TrunkOpen, TrunkClose,
            Hood, HoodLook, HoodOpen, HoodClose,
            Music,
            Passengers,
            Park,
            VehDocuments,
            Job,
            Gas,
            Close
        }

        public enum OutVehicleAction
        {
            Doors = 0, DoorsOpen, DoorsClose,
            Seat, SeatOne, SeatTwo, SeatThree, SeatFour, SeatTrunk,
            Trunk, TrunkLook, TrunkOpen, TrunkClose,
            Hood, HoodLook, HoodOpen, HoodClose,
            Push,
            Other, Fix, SetNumberplate, RemoveNumberplate, Junkyard,
            Park,
            VehDocuments,
            Job,
            Gas,
            Close
        }

        public enum PassengersMenuAction
        {
            Interact = 0, Kick,
        }
        #endregion

        public Interaction()
        {
            TempBinds = new List<int>();

            CurrentType = Type.None;

            LastSwitched = DateTime.Now;

            #region Events
            #region OutVehicle Select
            Events.Add("Interaction::OutVehicleSelect", (object[] args) =>
            {
                OutVehicleAction action = (OutVehicleAction)(int)args[0];

                CloseMenu();

                switch (action)
                {
                    case OutVehicleAction.Doors:
                        Sync.Vehicles.Lock(null, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.DoorsOpen:
                        Sync.Vehicles.Lock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.DoorsClose:
                        Sync.Vehicles.Lock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.Push:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Vehicle)
                            Sync.PushVehicle.Toggle(BCRPClient.Interaction.CurrentEntity as Vehicle);
                    break;

                    case OutVehicleAction.Trunk:
                        CEF.Inventory.Show(Inventory.Types.Container);
                    break;

                    case OutVehicleAction.TrunkOpen:
                        Sync.Vehicles.ToggleTrunkLock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.TrunkLook:
                        CEF.Inventory.Show(Inventory.Types.Container);
                    break;

                    case OutVehicleAction.TrunkClose:
                        Sync.Vehicles.ToggleTrunkLock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.HoodOpen:
                        Sync.Vehicles.ToggleHoodLock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.HoodClose:
                        Sync.Vehicles.ToggleHoodLock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.SeatOne:
                        Sync.Vehicles.SeatTo(0, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.SeatTwo:
                        Sync.Vehicles.SeatTo(1, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.SeatThree:
                        Sync.Vehicles.SeatTo(2, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.SeatFour:
                        Sync.Vehicles.SeatTo(3, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case OutVehicleAction.SeatTrunk:
                        //Sync.AttachSystem.AttachEntity(BCRPClient.Interaction.CurrentEntity, Sync.AttachSystem.Types.VehicleTrunk, Player.LocalPlayer.RemoteId, RAGE.Elements.Type.Player, false);
                        //Sync.Animations.Play(Player.LocalPlayer, Sync.Animations.GeneralTypes.LieInTrunk);
                    break;
                }
            });
            #endregion

            #region InVehicle Select
            Events.Add("Interaction::InVehicleSelect", (object[] args) =>
            {
                InVehicleAction action = (InVehicleAction)(int)args[0];

                if (action != InVehicleAction.Passengers)
                    CloseMenu();

                switch (action)
                {
                    case InVehicleAction.Doors:
                        Sync.Vehicles.Lock(null, BCRPClient.Interaction.CurrentEntity);
                        break;

                    case InVehicleAction.DoorsOpen:
                        Sync.Vehicles.Lock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.DoorsClose:
                        Sync.Vehicles.Lock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.TrunkOpen:
                        Sync.Vehicles.ToggleTrunkLock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.TrunkClose:
                        Sync.Vehicles.ToggleTrunkLock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.HoodOpen:
                        Sync.Vehicles.ToggleHoodLock(false, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.HoodClose:
                        Sync.Vehicles.ToggleHoodLock(true, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.SeatOne:
                        Sync.Vehicles.SeatTo(0, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.SeatTwo:
                        Sync.Vehicles.SeatTo(1, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.SeatThree:
                        Sync.Vehicles.SeatTo(2, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.SeatFour:
                        Sync.Vehicles.SeatTo(3, BCRPClient.Interaction.CurrentEntity);
                    break;

                    case InVehicleAction.Passengers:
                        ShowPassengers();
                    break;

                    case InVehicleAction.Park:

                    break;
                }
            });
            #endregion

            #region PassangersMenu Select
            Events.Add("Interaction::PassengersMenuSelect", (object[] args) =>
            {
                PassengersMenuAction action = (PassengersMenuAction)(int)args[0];
                int id = (int)args[1];

                CloseMenu();

                CurrentType = Type.InVehicle;

                if (action == PassengersMenuAction.Interact)
                {
                    PlayerInteraction(id);
                }
                else if (action == PassengersMenuAction.Kick)
                {
                    PlayerKick(id);
                }
                else
                    return;
            });
            #endregion

            #region PlayerMenu Select
            Events.Add("Interaction::PlayerMenuSelect", async (object[] args) =>
            {
                PlayerAction action = (PlayerAction)(int)args[0];

                CloseMenu();

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                switch (action)
                {
                    case PlayerAction.Handshake:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Handshake);
                    break;

                    case PlayerAction.Trade:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Exchange);
                        break;

                    case PlayerAction.Carry:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Carry);
                        break;

                    case PlayerAction.Money:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                        {
                            if (pData.Cash <= 0)
                            {
                                CEF.Notification.Show("Trade::NotEnoughMoney");

                                return;
                            }

                            await CEF.ActionBox.ShowRange(ActionBox.Contexts.GiveCash, string.Format(Locale.Actions.GiveCash, (BCRPClient.Interaction.CurrentEntity as Player).GetName(true, false, true)), 1, pData.Cash, pData.Cash / 2, -1);
                        }
                        break;

                    case PlayerAction.Money_50:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                        {
                            if (pData.Cash < 50)
                            {
                                CEF.Notification.Show("Trade::NotEnoughMoney");

                                return;
                            }

                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Cash, 50);
                        }
                        break;

                    case PlayerAction.Money_150:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                        {
                            if (pData.Cash < 150)
                            {
                                CEF.Notification.Show("Trade::NotEnoughMoney");

                                return;
                            }

                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Cash, 150);
                        }
                        break;

                    case PlayerAction.Money_300:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                        {
                            if (pData.Cash < 300)
                            {
                                CEF.Notification.Show("Trade::NotEnoughMoney");

                                return;
                            }

                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Cash, 300);
                        }
                        break;

                    case PlayerAction.Money_1000:
                        if (BCRPClient.Interaction.CurrentEntity?.Type == RAGE.Elements.Type.Player)
                        {
                            if (pData.Cash < 1000)
                            {
                                CEF.Notification.Show("Trade::NotEnoughMoney");

                                return;
                            }

                            Sync.Offers.Request(BCRPClient.Interaction.CurrentEntity as Player, Sync.Offers.Types.Cash, 1000);
                        }
                        break;
                }
            });
            #endregion
            #endregion
        }

        #region Showers
        public static bool TryShowMenu(bool ignoreTimeout = false)
        {
            if (BCRPClient.Interaction.CurrentEntity == null || CurrentType != Type.None || IsActive || Data.NPC.CurrentNPC != null)
                return false;

            if (!ignoreTimeout && LastSwitched.IsSpam(500, false, false))
                return false;

            Entity entity = BCRPClient.Interaction.CurrentEntity;

            if (Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheck))
                return false;

            BCRPClient.Interaction.Enabled = false;

            BCRPClient.Interaction.CurrentEntity = entity;

            if (entity.Type == RAGE.Elements.Type.Vehicle)
            {
                // out veh menu
                if (RAGE.Elements.Player.LocalPlayer.Vehicle == null)
                    ShowOutVehicleMenu();
                else
                    ShowInVehicleMenu();

                GameEvents.Render -= CheckEntityDistance;
                GameEvents.Render += CheckEntityDistance;

                return true;
            }
            else if (entity.Type == RAGE.Elements.Type.Player)
            {
                ShowPlayerMenu();

                GameEvents.Render -= CheckEntityDistance;
                GameEvents.Render += CheckEntityDistance;

                return true;
            }
            else if (entity.Type == RAGE.Elements.Type.Ped)
            {
                var data = Data.NPC.GetData(entity as Ped);

                if (data != null)
                {
                    GameEvents.Render -= CheckEntityDistance;
                    GameEvents.Render += CheckEntityDistance;

                    KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

                    data.Interact(true);

                    return true;
                }
            }

            BCRPClient.Interaction.Enabled = true;

            return false;
        }

        public static void ShowOutVehicleMenu()
        {
            if (CurrentType != Type.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Type.OutVehicle;

            Browser.Switch(Browser.IntTypes.Interaction_Vehicle_Out, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        public static void ShowPlayerMenu()
        {
            if (CurrentType != Type.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Type.PlayerMenu;

            Browser.Switch(Browser.IntTypes.Interaction_Character, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        public static void ShowInVehicleMenu()
        {
            if (CurrentType != Type.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Type.InVehicle;

            Browser.Switch(Browser.IntTypes.Interaction_Vehicle_In, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        private static void ShowPassengers()
        {
            if (!IsActive || CurrentType != Type.InVehicle)
                return;

            Browser.Switch(Browser.IntTypes.Interaction_Vehicle_In, false);

            Vehicle veh = Player.LocalPlayer.Vehicle;

            if (veh == null)
            {
                CloseMenu();

                return;
            }

            var vehData = Sync.Vehicles.GetData(veh);

            if (vehData == null)
            {
                CloseMenu();

                return;
            }

            List<object> players = new List<object>();

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
                Notification.Show(Notification.Types.Error, Locale.Notifications.DefHeader, Locale.Notifications.Vehicles.Passengers.None);

                CloseMenu();

                return;
            }

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Browser.Window.ExecuteJs($"Passengers.fill", new object[] { players });
            Browser.Switch(Browser.IntTypes.Interaction_Passengers, true);

            Cursor.Show(true, true);
        }
        #endregion

        #region PassangersMenu Select
        public static void PlayerInteraction(int id)
        {
            if (CurrentType != Type.InVehicle)
                return;

            if (Player.LocalPlayer.Vehicle == null)
                return;

            var player = Utils.GetPlayerByRemoteId(id, true);

            if (player?.Exists != true)
                return;

            CloseMenu();

            BCRPClient.Interaction.Enabled = false;

            BCRPClient.Interaction.CurrentEntity = player;

            TryShowMenu(true);
        }

        public static void PlayerKick(int id)
        {
            if (CurrentType != Type.InVehicle)
                return;

            if (Player.LocalPlayer.Vehicle == null)
                return;

            var player = Utils.GetPlayerByRemoteId(id, true);

            if (player?.Exists != true)
                return;

            Sync.Vehicles.KickPassenger(player);

            CloseMenu();
        }
        #endregion

        public static void CloseMenu()
        {
            if (!IsActive && Data.NPC.CurrentNPC == null)
                return;

            switch (CurrentType)
            {
                case Type.OutVehicle:
                    Browser.Switch(Browser.IntTypes.Interaction_Vehicle_Out, false);
                    break;

                case Type.InVehicle:
                    Browser.Switch(Browser.IntTypes.Interaction_Vehicle_In, false);
                    Browser.Switch(Browser.IntTypes.Interaction_Passengers, false);
                    break;

                case Type.PlayerMenu:
                    Browser.Switch(Browser.IntTypes.Interaction_Character, false);
                    break;
            }

            if (Data.NPC.CurrentNPC != null)
                Data.NPC.CurrentNPC.Interact(false);

            GameEvents.Render -= CheckEntityDistance;

            LastSwitched = DateTime.Now;

            CurrentType = Type.None;

            KeyBinds.Get(KeyBinds.Types.Interaction).Enable();

            foreach (var x in TempBinds.ToList())
                RAGE.Input.Unbind(x);

            TempBinds.Clear();

            Cursor.Show(false, false);

            BCRPClient.Interaction.Enabled = true;
        }

        #region Renders
        private static void CheckEntityDistance()
        {
            if (BCRPClient.Interaction.CurrentEntity?.IsNull != false || Vector3.Distance(Player.LocalPlayer.Position, BCRPClient.Interaction.CurrentEntity.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
            {
                CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                CloseMenu();
            }
        }
        #endregion
    }
}
