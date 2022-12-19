using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class Interaction : Events.Script
    {
        private static DateTime LastSwitched;

        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.Interaction_Character, Browser.IntTypes.Interaction_Vehicle_In, Browser.IntTypes.Interaction_Vehicle_Out, Browser.IntTypes.Interaction_Passengers); }

        private static Types CurrentType = Types.None;

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
        private enum Types
        {
            None = -1, PlayerMenu, InVehicle, OutVehicle
        }

        public enum PlayerActions
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

        public enum InVehicleActions
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

        public enum OutVehicleActions
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

        public enum PassengersMenuActions
        {
            Interact = 0, Kick,
        }
        #endregion

        public Interaction()
        {
            TempBinds = new List<int>();

            CurrentType = Types.None;

            LastSwitched = DateTime.Now;

            #region Events
            #region OutVehicle Select
            Events.Add("Interaction::OutVehicleSelect", (object[] args) =>
            {
                OutVehicleActions action = (OutVehicleActions)(int)args[0];

                CloseMenu();

                var vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle;

                if (vehicle == null)
                    return;

                switch (action)
                {
                    case OutVehicleActions.Doors:
                        Sync.Vehicles.Lock(null, vehicle);
                    break;

                    case OutVehicleActions.DoorsOpen:
                        Sync.Vehicles.Lock(false, vehicle);
                    break;

                    case OutVehicleActions.DoorsClose:
                        Sync.Vehicles.Lock(true, vehicle);
                    break;

                    case OutVehicleActions.Push:
                        Sync.PushVehicle.Toggle(vehicle);
                    break;

                    case OutVehicleActions.Trunk:
                        Sync.Vehicles.ShowContainer(vehicle);
                    break;

                    case OutVehicleActions.TrunkOpen:
                        Sync.Vehicles.ToggleTrunkLock(false, vehicle);
                    break;

                    case OutVehicleActions.TrunkLook:
                        Sync.Vehicles.ShowContainer(vehicle);
                    break;

                    case OutVehicleActions.TrunkClose:
                        Sync.Vehicles.ToggleTrunkLock(true, vehicle);
                    break;

                    case OutVehicleActions.HoodOpen:
                        Sync.Vehicles.ToggleHoodLock(false, vehicle);
                    break;

                    case OutVehicleActions.HoodClose:
                        Sync.Vehicles.ToggleHoodLock(true, vehicle);
                    break;

                    case OutVehicleActions.SeatOne:
                        Sync.Vehicles.SeatTo(0, vehicle);
                    break;

                    case OutVehicleActions.SeatTwo:
                        Sync.Vehicles.SeatTo(1, vehicle);
                    break;

                    case OutVehicleActions.SeatThree:
                        Sync.Vehicles.SeatTo(2, vehicle);
                    break;

                    case OutVehicleActions.SeatFour:
                        Sync.Vehicles.SeatTo(3, vehicle);
                    break;

                    case OutVehicleActions.SeatTrunk:
                        Sync.Vehicles.SeatTo(int.MaxValue, vehicle);
                    break;

                    case OutVehicleActions.Gas:
                        CEF.Gas.RequestShow(vehicle);
                    break;

                    case OutVehicleActions.Park:
                        Sync.Vehicles.Park(vehicle);
                    break;

                    case OutVehicleActions.RemoveNumberplate:
                        Sync.Vehicles.TakePlate(vehicle);
                    break;

                    case OutVehicleActions.SetNumberplate:
                        Sync.Vehicles.SetupPlate(vehicle);
                    break;

                    case OutVehicleActions.VehDocuments:
                        Events.CallRemote("Vehicles::ShowPass", vehicle);
                    break;
                }
            });
            #endregion

            #region InVehicle Select
            Events.Add("Interaction::InVehicleSelect", (object[] args) =>
            {
                InVehicleActions action = (InVehicleActions)(int)args[0];

                if (action != InVehicleActions.Passengers)
                    CloseMenu();

                var vehicle = BCRPClient.Interaction.CurrentEntity as Vehicle;

                if (vehicle == null)
                    return;

                switch (action)
                {
                    case InVehicleActions.VehDocuments:
                        Events.CallRemote("Vehicles::ShowPass", vehicle);
                    break;

                    case InVehicleActions.Doors:
                        Sync.Vehicles.Lock(null, vehicle);
                    break;

                    case InVehicleActions.DoorsOpen:
                        Sync.Vehicles.Lock(false, vehicle);
                    break;

                    case InVehicleActions.DoorsClose:
                        Sync.Vehicles.Lock(true, vehicle);
                    break;

                    case InVehicleActions.TrunkOpen:
                        Sync.Vehicles.ToggleTrunkLock(false, vehicle);
                    break;

                    case InVehicleActions.TrunkClose:
                        Sync.Vehicles.ToggleTrunkLock(true, vehicle);
                    break;

                    case InVehicleActions.HoodOpen:
                        Sync.Vehicles.ToggleHoodLock(false, vehicle);
                    break;

                    case InVehicleActions.HoodClose:
                        Sync.Vehicles.ToggleHoodLock(true, vehicle);
                    break;

                    case InVehicleActions.SeatOne:
                        Sync.Vehicles.SeatTo(0, vehicle);
                    break;

                    case InVehicleActions.SeatTwo:
                        Sync.Vehicles.SeatTo(1, vehicle);
                    break;

                    case InVehicleActions.SeatThree:
                        Sync.Vehicles.SeatTo(2, vehicle);
                    break;

                    case InVehicleActions.SeatFour:
                        Sync.Vehicles.SeatTo(3, vehicle);
                    break;

                    case InVehicleActions.Passengers:
                        ShowPassengers();
                    break;

                    case InVehicleActions.Park:
                        Sync.Vehicles.Park(vehicle);
                    break;

                    case InVehicleActions.Gas:
                        CEF.Gas.RequestShow(vehicle);
                    break;
                }
            });
            #endregion

            #region PassangersMenu Select
            Events.Add("Interaction::PassengersMenuSelect", (object[] args) =>
            {
                PassengersMenuActions action = (PassengersMenuActions)(int)args[0];
                int id = (int)args[1];

                CloseMenu();

                CurrentType = Types.InVehicle;

                if (action == PassengersMenuActions.Interact)
                {
                    PlayerInteraction(id);
                }
                else if (action == PassengersMenuActions.Kick)
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
                PlayerActions action = (PlayerActions)(int)args[0];

                CloseMenu();

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var player = BCRPClient.Interaction.CurrentEntity as Player;

                if (player == null)
                    return;

                switch (action)
                {
                    case PlayerActions.Settle:
                        var currentHouse = Player.LocalPlayer.GetData<Data.Locations.House>("House::CurrentHouse");

                        if (currentHouse == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.House.NotInAnyHouseOrApartments);

                            return;
                        }

                        if (!pData.OwnedHouses.Contains(currentHouse))
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.House.NotAllowed);

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.Settle);
                        break;

                    case PlayerActions.SellVehicle:
                        if (pData.OwnedVehicles.Count == 0)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.NoOwnedVehicles);

                            return;
                        }

                        CEF.Estate.ShowSellVehicle(player, true);
                        break;

                    case PlayerActions.SellBuisiness:
                        if (pData.OwnedBusinesses.Count == 0)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedBusiness);

                            return;
                        }

                        CEF.Estate.ShowSellBusiness(player, true);
                        break;

                    case PlayerActions.SellHouse:
/*                        if (pData.OwnedApartments.Count == 0 && pData.OwnedHouses.Count == 0 && pData.OwnedGarages.Count == 0)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedEstate);

                            return;
                        }*/

                        CEF.Estate.ShowSellEstate(player, true);
                        break;

                    case PlayerActions.Coin:
                        Sync.Offers.Request(player, Sync.Offers.Types.HeadsOrTails);
                        break;

                    case PlayerActions.DocumentsMedical:
                        if (pData.MedicalCard == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoMedicalCard);

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.ShowMedicalCard);
                        break;

                    case PlayerActions.DocumentsPassport:
                        Sync.Offers.Request(player, Sync.Offers.Types.ShowPassport);
                        break;

                    case PlayerActions.DocumentsVehicle:
                        var allVehs = pData.OwnedVehicles;

                        if (allVehs.Count == 0)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Vehicles.NoOwnedVehicles);

                            return;
                        }

                        if (allVehs.Count == 1)
                        {
                            Sync.Offers.Show(player, Sync.Offers.Types.ShowVehiclePassport, allVehs[0].VID);

                            return;
                        }

                        int t = 0;

                        CEF.ActionBox.ShowSelect(ActionBox.Contexts.VehiclePassportSelect, Locale.Actions.VehiclePassportSelectHeader, allVehs.Select(x => (t++, $"{x.Data.SubName} [#{x.VID}]")).ToArray());
                        break;

                    case PlayerActions.DocumentsLicenses:
                        Sync.Offers.Request(player, Sync.Offers.Types.ShowLicenses);
                        break;

                    case PlayerActions.Handshake:
                        Sync.Offers.Request(player, Sync.Offers.Types.Handshake);
                        break;

                    case PlayerActions.Trade:
                        Sync.Offers.Request(player, Sync.Offers.Types.Exchange);
                        break;

                    case PlayerActions.Carry:
                        Sync.Offers.Request(player, Sync.Offers.Types.Carry);
                        break;

                    case PlayerActions.Money:
                        if (pData.Cash <= 0)
                        {
                            CEF.Notification.Show("Trade::NotEnoughMoney");

                            return;
                        }

                        await CEF.ActionBox.ShowRange(ActionBox.Contexts.GiveCash, string.Format(Locale.Actions.GiveCash, player.GetName(true, false, true)), 1, pData.Cash, pData.Cash / 2, -1);
                        break;

                    case PlayerActions.Money_50:
                        if (pData.Cash < 50)
                        {
                            CEF.Notification.Show("Trade::NotEnoughMoney");

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.Cash, 50);
                        break;

                    case PlayerActions.Money_150:
                        if (pData.Cash < 150)
                        {
                            CEF.Notification.Show("Trade::NotEnoughMoney");

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.Cash, 150);
                        break;

                    case PlayerActions.Money_300:
                        if (pData.Cash < 300)
                        {
                            CEF.Notification.Show("Trade::NotEnoughMoney");

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.Cash, 300);
                        break;

                    case PlayerActions.Money_1000:
                        if (pData.Cash < 1000)
                        {
                            CEF.Notification.Show("Trade::NotEnoughMoney");

                            return;
                        }

                        Sync.Offers.Request(player, Sync.Offers.Types.Cash, 1000);
                        break;
                }
            });
            #endregion
            #endregion
        }

        #region Showers
        public static bool TryShowMenu(bool ignoreTimeout = false)
        {
            if (BCRPClient.Interaction.CurrentEntity == null || CurrentType != Types.None || IsActive)
                return false;

            if (!ignoreTimeout && LastSwitched.IsSpam(500, false, false))
                return false;

            Entity entity = BCRPClient.Interaction.CurrentEntity;

            if (Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheck))
                return false;

            BCRPClient.Interaction.Enabled = false;

            BCRPClient.Interaction.CurrentEntity = entity;

            if (entity is Vehicle vehicle)
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
            else if (entity is Player player)
            {
                ShowPlayerMenu();

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

            BCRPClient.Interaction.Enabled = true;

            return false;
        }

        public static void ShowOutVehicleMenu()
        {
            if (CurrentType != Types.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Types.OutVehicle;

            Browser.Switch(Browser.IntTypes.Interaction_Vehicle_Out, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        public static void ShowPlayerMenu()
        {
            if (CurrentType != Types.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Types.PlayerMenu;

            Browser.Switch(Browser.IntTypes.Interaction_Character, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        public static void ShowInVehicleMenu()
        {
            if (CurrentType != Types.None)
                return;

            LastSwitched = DateTime.Now;

            CurrentType = Types.InVehicle;

            Browser.Switch(Browser.IntTypes.Interaction_Vehicle_In, true);

            KeyBinds.Get(KeyBinds.Types.Interaction).Disable();

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        private static void ShowPassengers()
        {
            if (!IsActive || CurrentType != Types.InVehicle)
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
            if (CurrentType != Types.InVehicle)
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
            if (CurrentType != Types.InVehicle)
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
            if (CurrentType == Types.None)
                return;

            switch (CurrentType)
            {
                case Types.OutVehicle:
                    Browser.Switch(Browser.IntTypes.Interaction_Vehicle_Out, false);
                    break;

                case Types.InVehicle:
                    Browser.Switch(Browser.IntTypes.Interaction_Vehicle_In, false);
                    Browser.Switch(Browser.IntTypes.Interaction_Passengers, false);
                    break;

                case Types.PlayerMenu:
                    Browser.Switch(Browser.IntTypes.Interaction_Character, false);
                    break;
            }

            GameEvents.Render -= CheckEntityDistance;

            LastSwitched = DateTime.Now;

            CurrentType = Types.None;

            KeyBinds.Get(KeyBinds.Types.Interaction).Enable();

            foreach (var x in TempBinds)
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
