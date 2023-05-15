using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace BCRPClient.Additional
{
    public class ExtraColshapes : Events.Script
    {
        public static AsyncTask PolygonCreationTask { get; set; }

        public static Vector3 TempPosition { get; set; }

        public static Polygon TempPolygon { get; set; }

        public static bool CancelLastColshape { get; set; }

        public static object[] FormatArgsLastIntColshape { get; set; }

        private ExtraColshapes()
        {
            FormatArgsLastIntColshape = new string[] { };

            ExtraColshape.LastSent = DateTime.MinValue;
            ExtraColshape.InteractionColshapesAllowed = true;

            GameEvents.Render -= ExtraColshape.Render;
            GameEvents.Render += ExtraColshape.Render;

            Events.Add("ExtraColshape::New", (object[] args) =>
            {
                var cs = (RAGE.Elements.Colshape)args[0];

                if (cs == null)
                    return;

                var tNum = cs.GetSharedData<int?>("Type", null);

                if (tNum == null)
                    return;

                var type = (ExtraColshape.Types)tNum;

                ExtraColshape data = null;

                var pos = RAGE.Util.Json.Deserialize<Vector3>(cs.GetSharedData<string>("Position"));
                var isVisible = cs.GetSharedData<bool>("IsVisible");
                var dim = RAGE.Util.Json.Deserialize<uint>(cs.GetSharedData<string>("Dimension"));
                var colour = RAGE.Util.Json.Deserialize<Utils.Colour>(cs.GetSharedData<string>("Colour"));

                if (type == ExtraColshape.Types.Circle)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Circle(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Sphere)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Sphere(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Cylinder)
                {
                    var radius = cs.GetSharedData<float>("Radius");
                    var height = cs.GetSharedData<float>("Height");

                    data = new Cylinder(pos, radius, height, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Polygon)
                {
                    var vertices = RAGE.Util.Json.Deserialize<List<Vector3>>(cs.GetSharedData<string>("Vertices"));
                    var height = cs.GetSharedData<float>("Height");
                    var heading = cs.GetSharedData<float>("Heading");

                    data = new Polygon(vertices, height, heading, isVisible, colour, dim, cs);
                }

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>(cs.GetSharedData<string>("Data"));

                var aType = (ExtraColshape.ActionTypes)cs.GetSharedData<int>("ActionType");
                var iType = (ExtraColshape.InteractionTypes)cs.GetSharedData<int>("InteractionType");

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
                data.ActionType = aType;
                data.InteractionType = iType;
            });

            Events.Add("ExtraColshape::Del", (object[] args) =>
            {
                var data = ExtraColshape.GetByRemoteId((int)args[0]);

                if (data == null)
                    return;

                data.Destroy();
            });

            Events.AddDataHandler("Data", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>((string)value);

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
            });

            Events.AddDataHandler("ActionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.ActionType = (ExtraColshape.ActionTypes)(int)value;
            });

            Events.AddDataHandler("InteractionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.InteractionType = (ExtraColshape.InteractionTypes)(int)value;
            });

            Events.AddDataHandler("IsVisible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.IsVisible = (bool)value;
            });

            Events.AddDataHandler("Position", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Position = RAGE.Util.Json.Deserialize<Vector3>((string)value);
            });

            Events.AddDataHandler("Dimension", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Dimension = RAGE.Util.Json.Deserialize<uint>((string)value);
            });

            Events.AddDataHandler("Height", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Polygon)
                {
                    (data as Polygon).Height = (float)value;
                }
                else if (data is Cylinder)
                {
                    (data as Cylinder).Height = (float)value;
                }
            });

            Events.AddDataHandler("Heading", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (!(data is Polygon))
                    return;

                (data as Polygon).Heading = (float)value;
            });

            Events.AddDataHandler("Radius", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Sphere)
                {
                    (data as Sphere).Radius = (float)value;
                }
                else if (data is Circle)
                {
                    (data as Circle).Radius = (float)value;
                }
            });

            Events.AddDataHandler("Colour", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Colour = RAGE.Util.Json.Deserialize<Utils.Colour>((string)value);
            });

            Events.OnPlayerEnterColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (data.ActionType != ExtraColshape.ActionTypes.None)
                {
                    var action = ExtraColshape.GetEnterAction(data.ActionType);

                    action?.Invoke(data);
                }

                if (CancelLastColshape)
                {
                    CancelLastColshape = false;

                    data.IsInside = false;

                    return;
                }

                if (data.IsInteraction)
                {
                    if (!ExtraColshape.InteractionColshapesAllowed)
                        return;

                    var func = ExtraColshape.GetInteractionAction(data.InteractionType);

                    CEF.HUD.InteractionAction = func;

                    CEF.HUD.SwitchInteractionText(true, string.Format(Locale.Interaction.Names.GetValueOrDefault(data.InteractionType) ?? Locale.Interaction.Names.GetValueOrDefault(Additional.ExtraColshape.InteractionTypes.Interact) ?? "null", FormatArgsLastIntColshape));
                }

                data.OnEnter?.Invoke(null);
            };

            Events.OnPlayerExitColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (cs.GetData<bool>("PendingDeletion"))
                {
                    ExtraColshape.All.Remove(data);
                }

                if (data.IsInteraction)
                {
                    CEF.HUD.InteractionAction = null;

                    CEF.HUD.SwitchInteractionText(false, string.Empty);
                }

                if (data.ActionType != ExtraColshape.ActionTypes.None)
                {
                    var action = ExtraColshape.GetExitAction(data.ActionType);

                    action?.Invoke(data);
                }

                data.OnExit?.Invoke(null);
            };
        }
    }

    public abstract class ExtraColshape
    {
        private static bool _InteractionColshapesAllowed { get; set; }

        /// <summary>Доступны ли в данный момент для взаимодействия соответствующие колшейпы?</summary>
        public static bool InteractionColshapesAllowed { get => _InteractionColshapesAllowed && !Utils.IsAnyCefActive(true) && !SkyCamera.IsFadedOut; set { _InteractionColshapesAllowed = value; } }

        public static bool InteractionColshapesDisabledThisFrame { get; set; }

        /// <summary>Время последней отправки на сервер, используя колшейп</summary>
        public static DateTime LastSent;

        /// <summary>Словарь всех колшэйпов</summary>
        public static List<ExtraColshape> All { get; private set; } = new List<ExtraColshape>();

        /// <summary>Список колшэйпов, находящихся в зоне стрима игрока</summary>
        public static List<ExtraColshape> Streamed { get; private set; } = new List<ExtraColshape>();

        /// <summary>Получить колшейп по айди (локальный)</summary>
        public static ExtraColshape GetById(int id) => All.Where(x => x?.Colshape?.Id == id).FirstOrDefault();

        /// <summary>Получить колшейп по айди (серверный)</summary>
        public static ExtraColshape GetByRemoteId(int id) => All.Where(x => x?.Colshape?.RemoteId == id).FirstOrDefault();

        /// <summary>Получить колшейп по его держателю</summary>
        public static ExtraColshape Get(Colshape colshape) => colshape == null ? null : All.Where(x => x.Colshape == colshape).FirstOrDefault();

        public static List<ExtraColshape> GetAllByName(string name) => All.Where(x => x.Name == name).ToList();

        public static Action<ExtraColshape> GetEnterAction(ActionTypes aType) => Actions.GetValueOrDefault(aType)?.GetValueOrDefault(true);

        public static Action<ExtraColshape> GetExitAction(ActionTypes aType) => Actions.GetValueOrDefault(aType)?.GetValueOrDefault(false);

        public static Action GetInteractionAction(InteractionTypes iType) => InteractionActions.GetValueOrDefault(iType);

        /// <summary>Типы колшейпов</summary>
        public enum Types
        {
            /// <summary>Сферический (трехмерный)</summary>
            Sphere = 0,
            /// <summary>Круговой (двумерный)</summary>
            Circle,
            /// <summary>Цилиндрический (трехмерный)</summary>
            Cylinder,
            /// <summary>Многогранник (трехмерный/двумерный)</summary>
            /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный</remarks>
            Polygon,
            /// <summary>Кубический (трехмерный/двумерный)</summary>
            /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный<br/>Фактически - Polygon</remarks>
            Cuboid,
        }

        public enum ApproveTypes
        {
            None = -1,

            OnlyByFoot = 0,

            OnlyVehicle = 1,
            OnlyVehicleDriver = 2,

            OnlyLocalVehicle = 3,
            OnlyLocalVehicleDriver = 4,

            OnlyServerVehicle = 5,
            OnlyServerVehicleDriver = 6,
        }

        public enum InteractionTypes
        {
            None = -1,

            BusinessEnter,
            BusinessInfo,

            HouseEnter,
            HouseExit,

            GarageExit,

            Locker,
            Fridge,
            Wardrobe,

            Interact,

            NpcDialogue,

            ATM,

            TuningEnter,
            ShootingRangeEnter,

            ApartmentsRootEnter,
            ApartmentsRootExit,
            ApartmentsRootElevator,

            GarageRootEnter,

            ContainerInteract,
            FractionCreationWorkbenchInteract,
            FractionLockerRoomInteract,

            DrivingSchoolInteract,

            EstateAgencyInteract,

            FractionPoliceArrestMenuInteract,

            ElevatorInteract,

            CasinoRouletteInteract,
            CasinoLuckyWheelInteract,
            CasinoSlotMachineInteract,
            CasinoBlackjackInteract,
            CasinoPockerInteract,
        }

        public enum ActionTypes
        {
            /// <summary>Никакой, в таком случае нужно в ручную прописывать действия через OnEnter/OnExit</summary>
            None = -1,

            /// <summary>Зеленая (безопасная) зона</summary>
            GreenZone,

            /// <summary>Область, в которой можно заправлять транспорт (на заправках)</summary>
            GasStation,

            HouseEnter,
            HouseExit,

            BusinessInfo,

            IPL,

            NpcDialogue,

            ATM,

            TuningEnter,

            ReachableBlip,
            ShootingRangeEnter,

            ApartmentsRootEnter,
            ApartmentsRootExit,
            ApartmentsRootElevator,

            GarageRootEnter,

            VehicleSpeedLimit,

            ContainerInteract,
            FractionInteract,

            DrivingSchoolInteract,
            EstateAgencyInteract,

            ElevatorInteract,

            CasinoInteract,
        }

        public static Dictionary<ApproveTypes, Func<bool>> ApproveFuncs = new Dictionary<ApproveTypes, Func<bool>>()
        {
            {
                ApproveTypes.OnlyByFoot,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle != null)
                        return false;

                    return true;
                }
            },

            {
                ApproveTypes.OnlyVehicle,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        return true;
                    }

                    return false;
                }
            },

            {
                ApproveTypes.OnlyVehicleDriver,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                            return true;
                    }

                    return false;
                }
            },

            {
                ApproveTypes.OnlyLocalVehicle,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        if (veh.IsLocal)
                            return true;
                    }

                    return false;
                }
            },

            {
                ApproveTypes.OnlyLocalVehicleDriver,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        if (veh.IsLocal && veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                            return true;
                    }

                    return false;
                }
            },

            {
                ApproveTypes.OnlyServerVehicle,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        var vData = Sync.Vehicles.GetData(veh);

                        if (vData != null)
                            return true;
                    }

                    return false;
                }
            },

            {
                ApproveTypes.OnlyServerVehicleDriver,

                () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        var vData = Sync.Vehicles.GetData(veh);

                        if (vData != null)
                        {
                            if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                                return true;
                        }
                    }

                    return false;
                }
            },
        };

        public static Dictionary<InteractionTypes, Action> InteractionActions = new Dictionary<InteractionTypes, Action>()
        {
            {
                InteractionTypes.CasinoBlackjackInteract, async () =>
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var casinoStrData = Player.LocalPlayer.GetData<string>("CurrentCasinoGameData")?.Split('_');

                    if (casinoStrData == null)
                        return;

                    var casinoId = int.Parse(casinoStrData[0]);
                    var tableId = int.Parse(casinoStrData[1]);

                    var casino = BCRPClient.Data.Locations.Casino.GetById(casinoId);

                    var table = casino.GetBlackjackById(tableId);

                    LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::BLJE", casinoId, tableId))?.Split('^');

                    if (res == null)
                        return;

                    if (table?.TableObject?.Exists != true)
                        return;

                    var balance = decimal.Parse(res[0]);
                    var seatIdx = byte.Parse(res[1]);
                    var stateData = (string)res[2];

                    if (table.CurrentStateData != stateData)
                    {
                        BCRPClient.Data.Locations.Casino.Blackjack.OnCurrentStateDataUpdated(casinoId, tableId, stateData, true);
                    }

                    BCRPClient.Data.Minigames.Casino.Casino.ShowBlackjack(casino, table, seatIdx, balance);
                }
            },

            {
                InteractionTypes.CasinoSlotMachineInteract, async () =>
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var casinoStrData = Player.LocalPlayer.GetData<string>("CurrentCasinoGameData")?.Split('_');

                    if (casinoStrData == null)
                        return;

                    var casinoId = int.Parse(casinoStrData[0]);
                    var slotMachineId = int.Parse(casinoStrData[1]);

                    var casino = BCRPClient.Data.Locations.Casino.GetById(casinoId);

                    var slotMachine = casino.GetSlotMachineById(slotMachineId);

                    LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::SLME", casinoId, slotMachineId))?.Split('^');

                    if (res == null)
                        return;

                    if (slotMachine?.MachineObj?.Exists != true)
                        return;

                    var balance = decimal.Parse(res[0]);
                    var jackpot = decimal.Parse(res[1]);

                    BCRPClient.Data.Minigames.Casino.Casino.ShowSlotMachine(casino, slotMachine, balance, jackpot);
                }
            },

            {
                InteractionTypes.CasinoLuckyWheelInteract, () =>
                {
                    if (LastSent.IsSpam(2000, false, true))
                        return;

                    var casinoStrData = Player.LocalPlayer.GetData<string>("CurrentCasinoGameData")?.Split('_');

                    if (casinoStrData == null)
                        return;

                    var casinoId = int.Parse(casinoStrData[0]);
                    var luckyWheelId = int.Parse(casinoStrData[1]);

                    var casino = BCRPClient.Data.Locations.Casino.GetById(casinoId);

                    var luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                    LastSent = Sync.World.ServerTime;

                    Events.CallRemote("Casino::LCWS", casinoId, luckyWheelId);
                }
            },

            {
                InteractionTypes.CasinoRouletteInteract, async () =>
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var casinoStrData = Player.LocalPlayer.GetData<string>("CurrentCasinoGameData")?.Split('_');

                    if (casinoStrData == null)
                        return;

                    var casinoId = int.Parse(casinoStrData[0]);
                    var rouletteId = int.Parse(casinoStrData[1]);

                    LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::RLTJ", casinoId, rouletteId))?.Split('^');

                    if (res == null)
                        return;

                    var chipsBalance = decimal.Parse(res[0]);
                    var stateData = (string)res[1];

                    var casino = BCRPClient.Data.Locations.Casino.GetById(casinoId);

                    var roulette = casino.GetRouletteById(rouletteId);

                    if (roulette.TextLabel == null)
                        return;

                    if (roulette.CurrentStateData != stateData)
                    {
                        BCRPClient.Data.Locations.Casino.Roulette.OnCurrentStateDataUpdated(casinoId, rouletteId, stateData, true);
                    }

                    BCRPClient.Data.Minigames.Casino.Casino.ShowRoulette(casino, roulette, chipsBalance);
                }
            },

            {
                InteractionTypes.ElevatorInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var elevatorId = Player.LocalPlayer.GetData<uint>("EXED::ElevatorId");

                    var elevatorData = BCRPClient.Data.Locations.Elevator.Get(elevatorId);

                    if (elevatorData == null)
                        return;

                    await CEF.ActionBox.ShowSelect
                    (
                        "ElevatorSelect", "Выбор места перемещения", elevatorData.LinkedElevators.Select(x => ((decimal)x, BCRPClient.Data.Locations.Elevator.GetName(x))).ToArray(), null, null,

                        CEF.ActionBox.DefaultBindAction,

                        async (rType, idD) =>
                        {
                            var toId = (uint)idD;

                            if (rType == ActionBox.ReplyTypes.OK)
                            {
                                if (CEF.ActionBox.LastSent.IsSpam(500, false, true))
                                    return;

                                CEF.ActionBox.LastSent = Sync.World.ServerTime;

                                var res = (bool)await Events.CallRemoteProc("Elevator::TP", elevatorId, toId);

                                if (res)
                                    CEF.ActionBox.Close();
                            }
                            else
                            {
                                CEF.ActionBox.Close();
                            }
                        },

                        null
                    );
                }
            },

            {
                InteractionTypes.FractionPoliceArrestMenuInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = BCRPClient.Data.Fractions.Fraction.Get((BCRPClient.Data.Fractions.Types)int.Parse(fTypeS[0]));

                    if (pData.CurrentFraction != fData)
                    {
                        CEF.Notification.Show("Fraction::NM");

                        return;
                    }

                    var menuIdx = byte.Parse(fTypeS[1]);

                    var arrestsData = fData?.GetCurrentData<List<BCRPClient.Data.Fractions.Police.ArrestInfo>>("Arrests");

                    if (arrestsData == null)
                        return;

                    await CEF.ArrestsMenu.Show(fData.Type, menuIdx, arrestsData);
                }
            },

            {
                InteractionTypes.EstateAgencyInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var estAgencyIds = Player.LocalPlayer.GetData<string>("EXED::DriveSchoolId")?.Split('_');

                    if (estAgencyIds == null)
                        return;

                    var id = int.Parse(estAgencyIds[0]);
                    var posId = int.Parse(estAgencyIds[1]);

                    LastSent = Sync.World.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("EstAgency::GD", id, posId))?.Split('_');

                    if (res == null)
                        return;

                    await CEF.EstateAgency.Show(id, posId, decimal.Parse(res[0]), 0, 0);
                }
            },

            {
                InteractionTypes.DrivingSchoolInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    if (!Player.LocalPlayer.HasData("EXED::DriveSchoolId"))
                        return;

                    var schoolId = Player.LocalPlayer.GetData<int>("EXED::DriveSchoolId");

                    var lics = pData.Licenses;

                    var notOwnedLics = BCRPClient.Data.Locations.Autoschool.Prices.Keys.Where(x => !lics.Contains(x)).ToList();

                    if (notOwnedLics.Count == 0)
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, "Вы уже владеете всеми лицензиями для транспорта, так держать!", -1);

                        return;
                    }

                    await CEF.ActionBox.ShowSelect
                    (
                        "DrivingSchoolSelect", "Выбор категории", notOwnedLics.Select(x => ((decimal)x, Locale.General.Players.LicenseNames.GetValueOrDefault(x) ?? "null")).ToArray(), null, null,

                        CEF.ActionBox.DefaultBindAction,

                        (rType, id) =>
                        {
                            CEF.ActionBox.Close(true);

                            if (rType == CEF.ActionBox.ReplyTypes.OK)
                            {
                                var licType = (Sync.Players.LicenseTypes)id;

                                CEF.AutoschoolTest.Show(schoolId, licType, BCRPClient.Data.Locations.Autoschool.Prices.GetValueOrDefault(licType));
                            }
                        },

                        null
                    );
                }
            },

            {
                InteractionTypes.FractionLockerRoomInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = BCRPClient.Data.Fractions.Fraction.Get((BCRPClient.Data.Fractions.Types)int.Parse(fTypeS[0]));

                    var fDataUnif = fData as BCRPClient.Data.Fractions.IUniformable;

                    if (fDataUnif == null)
                        return;

                    var lockerIdx = byte.Parse(fTypeS[1]);

                    if (pData.CurrentFraction != fData)
                    {
                        CEF.Notification.Show("Fraction::NM");

                        return;
                    }

                    var res = (int)await Events.CallRemoteProc("Fraction::UNIFS", (int)fData.Type, lockerIdx);

                    if (res == int.MinValue)
                        return;

                    var allButtons = new List<(decimal, string)>();

                    if (res >= 0)
                        allButtons.Add((-1, "[Завершить рабочий день]"));

                    for (int i = 0; i < fDataUnif.UniformNames.Count; i++)
                    {
                        if (res == i)
                            continue;

                        allButtons.Add((i, fDataUnif.UniformNames[i]));
                    }

                    await CEF.ActionBox.ShowSelect
                    (
                        "FractionUniformSelect", Locale.Actions.FractionUniformSelectTitle, allButtons.ToArray(), null, null,

                        CEF.ActionBox.DefaultBindAction,

                        (rType, id) =>
                        {
                            if (rType == CEF.ActionBox.ReplyTypes.OK)
                            {
                                Events.CallRemote("Fraction::UNIFC", (int)fData.Type, lockerIdx, id);

                                CEF.ActionBox.Close(true);
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
            },

            {
                InteractionTypes.FractionCreationWorkbenchInteract, async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = BCRPClient.Data.Fractions.Fraction.Get((BCRPClient.Data.Fractions.Types)int.Parse(fTypeS[0]));

                    if (fData == null)
                        return;

                    var wbIdx = byte.Parse(fTypeS[1]);

                    if (pData.CurrentFraction != fData)
                    {
                        CEF.Notification.Show("Fraction::NM");

                        return;
                    }
                    
                    if (fData.CreationWorkbenchPrices.Count == 0)
                    {
                        CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "На данный момент здесь нельзя создать ни один предмет!");

                        return;
                    }

                    LastSent = Sync.World.ServerTime;

                    var res = (int)await Events.CallRemoteProc("Fraction::CWBS", (int)fData.Type, wbIdx);

                    if (res == byte.MaxValue)
                    {
                        if (Utils.IsAnyCefActive(true))
                            return;

                        CEF.MaterialWorkbench.Show(MaterialWorkbench.Types.Fraction, fData.CreationWorkbenchPrices, fData.Materials, fData.Type, wbIdx);
                    }
                }
            },

            {
                InteractionTypes.ContainerInteract, () =>
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    if (!Player.LocalPlayer.HasData("EXED::ContId"))
                        return;

                    LastSent = Sync.World.ServerTime;

                    Events.CallRemote("Container::Show", Player.LocalPlayer.GetData<uint>("EXED::ContId"));
                }
            },

            {
                InteractionTypes.BusinessEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                        return;

                    LastSent = Sync.World.ServerTime;

                    Events.CallRemote("Business::Enter", Player.LocalPlayer.GetData<int>("CurrentBusiness"));
                }
            },

            {
                InteractionTypes.BusinessInfo, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                        return;

                    CEF.Estate.ShowBusinessInfo(Player.LocalPlayer.GetData<BCRPClient.Data.Locations.Business>("CurrentBusiness"), true);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.HouseEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentHouse"))
                        return;

                    CEF.Estate.ShowHouseBaseInfo(Player.LocalPlayer.GetData<BCRPClient.Data.Locations.HouseBase>("CurrentHouse"), true);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.HouseExit, async () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                        return;

                    var house = Player.LocalPlayer.GetData<BCRPClient.Data.Locations.HouseBase>("House::CurrentHouse");

                    if (house is Data.Locations.House rHouse && rHouse.GarageType != null)
                    {
                        await CEF.ActionBox.ShowSelect
                        (
                            "HouseExit", Locale.Actions.HouseExitActionBoxHeader, new (decimal, string)[] { (0, Locale.Actions.HouseExitActionBoxOutside), (1, Locale.Actions.HouseExitActionBoxToGarage) }, null, null,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, id) =>
                            {
                                CEF.ActionBox.Close(true);

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                if (rType == CEF.ActionBox.ReplyTypes.OK)
                                {
                                    // house/houseGarage -> outside
                                    if (id == 0)
                                    {
                                        Events.CallRemote("House::Exit");
                                    }
                                    // house -> garage
                                    else if (id == 1)
                                    {
                                        Events.CallRemote("House::Garage", true);
                                    }
                                }
                            },

                            null
                        );
                    }
                    else
                    {
                        Events.CallRemote("House::Exit");

                        LastSent = Sync.World.ServerTime;
                    }
                }
            },

            {
                InteractionTypes.GarageExit, async () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                    {
                        Events.CallRemote("Garage::Exit");
                    }
                    else
                    {
                        await CEF.ActionBox.ShowSelect
                        (
                            "HouseExit", Locale.Actions.HouseExitActionBoxHeader, new (decimal, string)[] { (0, Locale.Actions.HouseExitActionBoxOutside), (1, Locale.Actions.HouseExitActionBoxToHouse) }, null, null,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, id) =>
                            {
                                CEF.ActionBox.Close(true);

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                if (rType == CEF.ActionBox.ReplyTypes.OK)
                                {
                                    // house/houseGarage -> outside
                                    if (id == 0)
                                    {
                                        Events.CallRemote("House::Exit");
                                    }
                                    // garage -> house
                                    else if (id == 1)
                                    {
                                        Events.CallRemote("House::Garage", false);
                                    }
                                }
                            },

                            null
                        );
                    }
                }
            },

            {
                InteractionTypes.NpcDialogue, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentNPC"))
                        return;

                    var npc = Player.LocalPlayer.GetData<Data.NPC>("CurrentNPC");

                    if (npc == null)
                        return;

                    npc.SwitchDialogue(true);

                    npc.ShowDialogue(npc.DefaultDialogueId);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.ATM, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentATM"))
                        return;

                    Events.CallRemote("Bank::Show", true, Player.LocalPlayer.GetData<BCRPClient.Data.Locations.ATM>("CurrentATM").Id);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.TuningEnter, async () =>
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentTuning"))
                        return;

                    var baseVeh = Player.LocalPlayer.Vehicle;

                    if (baseVeh == null)
                        return;

                    var bVehData = Sync.Vehicles.GetData(baseVeh);

                    if (bVehData == null)
                        return;

                    var trVehHandle = baseVeh.GetTrailerVehicle();

                    if (trVehHandle > 0 && Utils.GetVehicleByHandle(trVehHandle, true) is Vehicle trVeh)
                    {
                        if (trVeh.GetData<Vehicle>("TrailerSync::Owner") is Vehicle boat)
                        {
                            var boatData = Sync.Vehicles.GetData(boat);

                            if (boatData == null)
                                return;

                            var tuningId = Player.LocalPlayer.GetData<BCRPClient.Data.Locations.TuningShop>("CurrentTuning").Id;

                            await CEF.ActionBox.ShowSelect
                            (
                                "VehicleTuningVehicleSelect", Locale.Actions.VehicleTuningVehicleSelect, new (decimal Id, string Text)[] { (1, $"{bVehData.Data.SubName} [#{bVehData.VID}]"), (2, $"{boatData.Data.SubName} [#{boatData.VID}]") }, null, null,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, id) =>
                                {
                                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                                    if (pData == null)
                                        return;

                                    if (rType == CEF.ActionBox.ReplyTypes.OK)
                                    {
                                        if (id != 1 && id != 2)
                                            return;

                                        Events.CallRemote("TuningShop::Enter", tuningId, id == 1 ? baseVeh : boat);

                                        CEF.ActionBox.Close(true);
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

                            return;
                        }
                    }

                    Events.CallRemote("TuningShop::Enter", Player.LocalPlayer.GetData<BCRPClient.Data.Locations.TuningShop>("CurrentTuning").Id, baseVeh);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.ShootingRangeEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentShootingRange"))
                        return;

                    Events.CallRemote("SRange::Enter::Shop", Player.LocalPlayer.GetData<BCRPClient.Data.Locations.WeaponShop>("CurrentShootingRange").Id);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.ApartmentsRootEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentApartmentsRoot"))
                        return;

                    Events.CallRemote("ARoot::Enter", (int)Player.LocalPlayer.GetData<BCRPClient.Data.Locations.ApartmentsRoot>("CurrentApartmentsRoot").Type);

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.ApartmentsRootExit, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("ApartmentsRoot::Current"))
                        return;

                    Events.CallRemote("ARoot::Exit");

                    LastSent = Sync.World.ServerTime;
                }
            },

            {
                InteractionTypes.ApartmentsRootElevator, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("ApartmentsRoot::Current"))
                        return;

                    var aRoot = Player.LocalPlayer.GetData<BCRPClient.Data.Locations.ApartmentsRoot>("ApartmentsRoot::Current");

                    if (aRoot == null)
                        return;

                    var curFloor = aRoot.GetFloor(Player.LocalPlayer.Position);

                    if (curFloor is int floor)
                    {
                        CEF.Elevator.Show(aRoot.StartFloor + aRoot.FloorsAmount - 1, floor, Elevator.ContextTypes.ApartmentsRoot);

                        LastSent = Sync.World.ServerTime;
                    }
                }
            },

            {
                InteractionTypes.GarageRootEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentGarageRoot"))
                        return;

                    CEF.GarageMenu.Show(Player.LocalPlayer.GetData<BCRPClient.Data.Locations.GarageRoot>("CurrentGarageRoot"));

                    LastSent = Sync.World.ServerTime;
                }
            },
        };

        public static Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>> Actions = new Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>>()
        {
            {
                ActionTypes.CasinoInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is string str)
                            {
                                var d = str.Split('_');

                                var casino = BCRPClient.Data.Locations.Casino.GetById(int.Parse(d[0]));
                                var roulette = casino.GetRouletteById(int.Parse(d[1]));

                                Player.LocalPlayer.SetData("CurrentCasinoGameData", str);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentCasinoGameData");
                        }
                    },
                }
            },

            {
                ActionTypes.ElevatorInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is uint id)
                                Player.LocalPlayer.SetData("EXED::ElevatorId", id);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::ElevatorId");
                        }
                    }
                }
            },

            {
                ActionTypes.EstateAgencyInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is string id)
                                Player.LocalPlayer.SetData("EXED::DriveSchoolId", id);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::DriveSchoolId");
                        }
                    }
                }
            },

            {
                ActionTypes.DrivingSchoolInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int id)
                                Player.LocalPlayer.SetData("EXED::DriveSchoolId", id);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::DriveSchoolId");
                        }
                    }
                }
            },

            {
                ActionTypes.FractionInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is string str)
                                Player.LocalPlayer.SetData("EXED::CFractionId", str);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::CFractionId");
                        }
                    }
                }
            },

            {
                ActionTypes.ContainerInteract,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is uint contId)
                                Player.LocalPlayer.SetData("EXED::ContId", contId);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::ContId");
                        }
                    }
                }
            },

            {
                ActionTypes.GasStation,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int data)
                            {
                                Player.LocalPlayer.SetData("CurrentGasStation", data);

                                //CEF.Notification.ShowHint(Locale.Notifications.Hints.GasStationColshape, false, 2500);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentGasStation");

                            CEF.Gas.Close(true);
                        }
                    },
                }
            },

            {
                ActionTypes.GreenZone,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            Sync.WeaponSystem.DisabledFiring = true;

                            Player.LocalPlayer.SetData("InGreenZone", true);

                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, true);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Sync.WeaponSystem.DisabledFiring = false;

                            Player.LocalPlayer.ResetData("InGreenZone");

                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, false);
                        }
                    },
                }
            },

            {
                ActionTypes.IPL,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Sync.IPLManager.IPLInfo ipl)
                            {
                                ipl.Load();
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            if (cs.Data is Sync.IPLManager.IPLInfo ipl)
                            {
                                ipl.Unload();
                            }
                        }
                    },
                }
            },

            {
                ActionTypes.BusinessInfo,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.Locations.Business biz)
                            {
                                Player.LocalPlayer.SetData("CurrentBusiness", biz);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentBusiness");
                        }
                    },
                }
            },

            {
                ActionTypes.HouseEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (!(cs.Data is Data.Locations.HouseBase houseBase))
                                return;

                            Player.LocalPlayer.SetData("CurrentHouse", houseBase);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentHouse");
                        }
                    },
                }
            },

            {
                ActionTypes.NpcDialogue,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.NPC npc)
                            {
                                Player.LocalPlayer.SetData("CurrentNPC", npc);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentNPC");
                        }
                    },
                }
            },

            {
                ActionTypes.ATM,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.Locations.ATM atm)
                            {
                                Player.LocalPlayer.SetData("CurrentATM", atm);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentATM");
                        }
                    },
                }
            },

            {
                ActionTypes.TuningEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.Locations.TuningShop ts)
                            {
                                Player.LocalPlayer.SetData("CurrentTuning", ts);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentTuning");
                        }
                    },
                }
            },

            {
                ActionTypes.ReachableBlip,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Additional.ExtraBlip blip)
                            {
                                if (blip.Type == ExtraBlip.Types.AutoPilot)
                                {
                                    Sync.Vehicles.ToggleAutoPilot(false, true);
                                }

                                blip.Destroy();

                                if (Player.LocalPlayer.Vehicle != null)
                                    CEF.Notification.Show(Notification.Types.Success, Locale.Notifications.Blip.Header, Locale.Notifications.Blip.ReachedGPS);
                            }
                        }
                    },
                }
            },

            {
                ActionTypes.ShootingRangeEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is BCRPClient.Data.Locations.WeaponShop ws)
                            {
                                ExtraColshapes.FormatArgsLastIntColshape = new object[] { BCRPClient.Data.Locations.WeaponShop.ShootingRangePrice };

                                Player.LocalPlayer.SetData("CurrentShootingRange", ws);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentShootingRange");
                        }
                    },
                }
            },

            {
                ActionTypes.ApartmentsRootEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is BCRPClient.Data.Locations.ApartmentsRoot aRoot)
                            {
                                Player.LocalPlayer.SetData("CurrentApartmentsRoot", aRoot);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentApartmentsRoot");
                        }
                    },
                }
            },

            {
                ActionTypes.GarageRootEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is BCRPClient.Data.Locations.GarageRoot gRoot)
                            {
                                Player.LocalPlayer.SetData("CurrentGarageRoot", gRoot);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentGarageRoot");
                        }
                    },
                }
            },

            {
                ActionTypes.VehicleSpeedLimit,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is float maxSpeed && maxSpeed > 0f)
                            {
                                Player.LocalPlayer.SetData("ColshapeVehicleSpeedLimited", maxSpeed);

                                if (Player.LocalPlayer.Vehicle is Vehicle veh)
                                {
                                    Sync.Vehicles.SetColshapeVehicleMaxSpeed(veh, maxSpeed);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("ColshapeVehicleSpeedLimited");

                            if (Player.LocalPlayer.Vehicle is Vehicle veh)
                            {
                                Sync.Vehicles.SetColshapeVehicleMaxSpeed(veh, float.MinValue);
                            }
                        }
                    },
                }
            },
        };

        public abstract string ShortData { get; }

        public bool Exists => All.Contains(this);

        /// <summary>Сущность-держатель колшейпа, не имеет функциональности</summary>
        public Colshape Colshape { get; set; }

        /// <summary>Тип колшейпа</summary>
        public Types Type { get; set; }

        /// <summary>Тип действия при входе/выходе в колшейп</summary>
        public ActionTypes ActionType { get; set; }

        /// <summary>Тип действия для взаимодействия</summary>
        public InteractionTypes InteractionType { get; set; }

        /// <summary>Тип действия для проверки на возможность взаимодействия с колшейпом</summary>
        public ApproveTypes ApproveType { get; set; }

        /// <summary>Позиция</summary>
        public Vector3 Position { get; set; }

        /// <summary>Для взаимодействия ли колшейп?</summary>
        /// <remarks>Если колшейп используется для взаимодействия, то ивенты OnEnter и OnExit будут срабатывать также в зависимости от того, открыт ли какой либо интерфейс у игрока</remarks>
        public bool IsInteraction { get => InteractionType != InteractionTypes.None; }

        /// <summary>Измерение</summary>
        /// <remarks>Если используется uint.MaxValue, то колшейп работает независимо от измерения игрока</remarks>
        public uint Dimension { get; set; }

        /// <summary>Цвет</summary>
        public Utils.Colour Colour { get; set; }

        /// <summary>Видимый ли?</summary>
        /// <remarks>Если колшейп видимый, то его будут видеть все игроки, иначе - только администраторы, и то, при включенной настройке на стороне клиента</remarks>
        public bool IsVisible { get; set; }

        /// <summary>Находится ли игрок внутри?</summary>
        public bool IsInside { get; set; }

        /// <summary>Название колшейпа</summary>
        public string Name { get; set; }

        /// <summary>Метод для отрисовки колшейпа на экране</summary>
        public abstract void Draw();

        /// <summary>Метод для проверки, находится ли точка в колшейпе</summary>
        /// <param name="point">Точка</param>
        public abstract bool IsPointInside(Vector3 point);

        /// <summary>Метод для задания новой позиции колшейпа</summary>
        /// <param name="position">Позиция</param>
        public virtual void SetPosition(Vector3 position) => Position = position;

        /// <summary>Метод для проверки, находится ли колшейп в зоне стрима для игрока</summary>
        public virtual bool IsStreamed() => Colshape?.IsNull == false && (Dimension == uint.MaxValue || Player.LocalPlayer.Dimension == Dimension);

        /// <summary>Данные колшейпа</summary>
        public object Data { get; set; }

        public Colshape.ColshapeEventDelegate OnEnter { get => Colshape?.OnEnter; set { if (Colshape?.IsNull != false) return; Colshape.OnEnter = value; } }
        public Colshape.ColshapeEventDelegate OnExit { get => Colshape?.OnExit; set { if (Colshape?.IsNull != false) return; Colshape.OnExit = value; } }

        public void Destroy()
        {
            if (!Exists)
                return;

            Streamed.Remove(this);

            if (Colshape != null)
            {
                if (IsInside)
                {
                    Colshape.SetData("PendingDeletion", true);

                    Events.OnPlayerExitColshape?.Invoke(Colshape, null);

                    All.Remove(this);
                }
                else
                {
                    All.Remove(this);
                }

                Colshape.ResetData();

                Colshape.Destroy();
            }
        }

        public ExtraColshape(Types Type, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null, InteractionTypes InteractionType = InteractionTypes.None, ActionTypes ActionType = ActionTypes.None)
        {
            this.Colshape = Colshape ?? new RAGE.Elements.SphereColshape(Vector3.Zero, 0f, Settings.STUFF_DIMENSION);

            this.Type = Type;
            this.Colour = Colour;
            this.Dimension = Dimension;
            this.IsVisible = IsVisible;

            this.InteractionType = InteractionType;
            this.ActionType = ActionType;

            this.ApproveType = ApproveTypes.OnlyByFoot;

            All.Add(this);
        }

        public static void Render()
        {
            for (int i = 0; i < Streamed.Count; i++)
            {
                var curColshape = Streamed[i];

                if (curColshape.IsVisible || Settings.Other.ColshapesVisible)
                    curColshape.Draw();
            }
        }

        private static Timer streamUpdateTimer { get; set; }
        private static Timer updateTimer { get; set; }

        public static void Activate()
        {
            streamUpdateTimer = new Timer(async (obj) =>
            {
                await RAGE.Game.Invoker.WaitAsync(0);

                UpdateInside();
            }, null, 0, 200);

            updateTimer = new Timer(async (obj) =>
            {
                await RAGE.Game.Invoker.WaitAsync(0);

                UpdateStreamed();
            }, null, 0, 1000);
        }

        public static void UpdateStreamed()
        {
            for (int i = 0; i < All.Count; i++)
            {
                var cs = All[i];

                var state = cs?.IsStreamed();

                if (state == null)
                    continue;

                if (state == true)
                {
                    if (Streamed.Contains(cs))
                        continue;

                    Streamed.Add(cs);
                }
                else
                {
                    if (Streamed.Remove(cs))
                    {
                        if (cs.IsInside)
                        {
                            cs.IsInside = false;

                            Events.OnPlayerExitColshape?.Invoke(cs.Colshape, null);
                        }
                    }
                }
            }
        }

        public static void UpdateInside()
        {
            var interactionAllowed = InteractionColshapesAllowed;

            if (InteractionColshapesDisabledThisFrame)
            {
                interactionAllowed = false;

                InteractionColshapesDisabledThisFrame = false;
            }

            var pos = Player.LocalPlayer.Vehicle is Vehicle veh ? veh.Position : Player.LocalPlayer.Position;

            Streamed.OrderByDescending(x => x.IsInside).ToList().ForEach(curPoly =>
            {
                if (curPoly.IsInside)
                {
                    /*                    if (curPoly?.Colshape?.IsNull != false)
                                        {
                                            if (curPoly?.Colshape != null)
                                                All.Remove(curPoly.Colshape);

                                            continue;
                                        }*/

                    if ((curPoly.IsInteraction && !interactionAllowed) || !curPoly.IsPointInside(pos) || !(ApproveFuncs.GetValueOrDefault(curPoly.ApproveType)?.Invoke() ?? true))
                    {
                        curPoly.IsInside = false;

                        Events.OnPlayerExitColshape?.Invoke(curPoly.Colshape, null);
                    }
                }
                else
                {
                    if ((curPoly.IsInteraction && !interactionAllowed) || !(ApproveFuncs.GetValueOrDefault(curPoly.ApproveType)?.Invoke() ?? true))
                        return;

                    if (curPoly.IsPointInside(pos))
                    {
                        curPoly.IsInside = true;

                        Events.OnPlayerEnterColshape?.Invoke(curPoly.Colshape, null);
                    }
                }
            });
        }
    }

    public class Sphere : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public Sphere(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Sphere, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            Utils.DrawSphere(Position, Radius, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha / 255f);

            if (Settings.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Radius + Settings.STREAM_DISTANCE;
        }

        public override bool IsPointInside(Vector3 point) => Vector3.Distance(point, Position) <= Radius;
    }

    public class Circle : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}";

        public float Radius { get; set; }

        public Circle(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Circle, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            var diameter = Radius * 2;

            RAGE.Game.Graphics.DrawMarker(1, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, 10f, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

            if (Settings.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= Radius + Settings.STREAM_DISTANCE;
        }

        public override bool IsPointInside(Vector3 point) => point.DistanceIgnoreZ(Position) <= Radius;
    }

    public class Cylinder : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Pos: {RAGE.Util.Json.Serialize(Position)}, Radius: {Radius}, Height: {Height}";

        public float Radius { get; set; }
        public float Height { get; set; }

        public Cylinder(Vector3 Position, float Radius, float Height, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Cylinder, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;
            this.Height = Height;

            this.Position = Position;

            if (IsStreamed())
                Streamed.Add(this);
        }

        public override void Draw()
        {
            var diameter = Radius * 2f;

            RAGE.Game.Graphics.DrawMarker(1, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

            if (Settings.Other.DebugLabels)
            {
                float screenX = 0f, screenY = 0f;

                if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            if (point.Z < Position.Z || point.Z > Position.Z + Height)
                return false;

            return Position.DistanceIgnoreZ(point) <= Radius;
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Height + Radius + Settings.STREAM_DISTANCE;
        }
    }

    public class Cuboid : Polygon
    {
        public override string ShortData => $"Type: {Type}, CenterPos: {RAGE.Util.Json.Serialize(Position)}, Width: {Width}, Depth: {Depth}, Height: {Height}, Heading: {Heading}";

        public float Width { get; set; }

        public float Depth { get; set; }

        public Cuboid(Vector3 Position, float Width, float Depth, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = 7, Colshape Colshape = null) : base(Types.Cuboid, GetBaseVertices(Position, Width, Depth, Height), Height, Heading, IsVisible, Colour, Dimension, Colshape)
        {
            this.Width = Width;
            this.Depth = Depth;
        }

        public static List<Vector3> GetBaseVertices(Vector3 centerPos, float width, float depth, float height)
        {
            var zCoord = centerPos.Z - height / 2;

            var width2 = width / 2f;
            var depth2 = depth / 2f;

            return new List<Vector3>()
            {
                new Vector3(centerPos.X - width2, centerPos.Y - depth2, zCoord),
                new Vector3(centerPos.X + width2, centerPos.Y - depth2, zCoord),
                new Vector3(centerPos.X + width2, centerPos.Y + depth2, zCoord),
                new Vector3(centerPos.X - width2, centerPos.Y + depth2, zCoord),
            };
        }

        public void SetWidth(float value)
        {
            Width = value;

            value /= 2f;

            var depth2 = Depth / 2f;

            Vertices[0].X = Position.X - value;
            Vertices[1].X = Position.X + value;
            Vertices[2].X = Position.X + value;
            Vertices[3].X = Position.X - value;

            Vertices[0].Y = Position.Y - depth2;
            Vertices[1].Y = Position.Y - depth2;
            Vertices[2].Y = Position.Y + depth2;
            Vertices[3].Y = Position.Y + depth2;

            UpdatePolygonCenterAndMaxRange();

            var heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }

        public void SetDepth(float value)
        {
            Depth = value;

            value /= 2f;

            var width2 = Width / 2f;

            Vertices[0].Y = Position.Y - value;
            Vertices[1].Y = Position.Y - value;
            Vertices[2].Y = Position.Y + value;
            Vertices[3].Y = Position.Y + value;

            Vertices[0].X = Position.X - width2;
            Vertices[1].X = Position.X + width2;
            Vertices[2].X = Position.X + width2;
            Vertices[3].X = Position.X - width2;

            UpdatePolygonCenterAndMaxRange();

            var heading = Heading;

            Heading = 0f;

            SetHeading(heading);
        }
    }

    public class Polygon : ExtraColshape
    {
        public override string ShortData => $"Type: {Type}, Vertices: {RAGE.Util.Json.Serialize(Vertices)}, Height: {Height}, Heading: {Heading}";

        public float MaxRange { get; set; }

        public float Height { get; set; }

        public float Heading { get; set; }

        public List<Vector3> Vertices { get; set; }

        public bool Is3D => Height > 0;

        protected Polygon(Types Type, List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Type, IsVisible, Colour, Dimension, Colshape)
        {
            this.Height = Height;

            this.Heading = 0;

            this.Vertices = Vertices;

            this.Position = GetCenterPosition(Vertices, Height);

            UpdatePolygonCenterAndMaxRange();

            SetHeading(Heading);

            if (IsStreamed())
                Streamed.Add(this);
        }

        public Polygon(List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : this(Types.Polygon, Vertices, Height, Heading, IsVisible, Colour, Dimension, Colshape)
        {

        }

        protected void UpdatePolygonCenterAndMaxRange()
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < Vertices.Count; i++)
            {
                centerPos.X += Vertices[i].X;
                centerPos.Y += Vertices[i].Y;
                centerPos.Z += Vertices[i].Z;
            }

            centerPos.X /= Vertices.Count;
            centerPos.Y /= Vertices.Count;
            centerPos.Z /= Vertices.Count;

            centerPos.Z += Height / 2;

            Position = centerPos;
            MaxRange = Vertices.Max(x => x.DistanceTo(centerPos));
        }

        public void SetHeading(float heading) => Rotate(heading - Heading);

        public static Vector3 GetCenterPosition(List<Vector3> vertices, float height)
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < vertices.Count; i++)
            {
                centerPos.X += vertices[i].X;
                centerPos.Y += vertices[i].Y;
                centerPos.Z += vertices[i].Z;
            }

            centerPos.X /= vertices.Count;
            centerPos.Y /= vertices.Count;
            centerPos.Z /= vertices.Count;

            centerPos.Z += height / 2;

            return centerPos;
        }

        public void Rotate(float angle)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Utils.RotatePoint(Vertices[i], Position, angle);

            Heading += angle;
        }

        public override void SetPosition(Vector3 position)
        {
            float diffX = position.X - Position.X;
            float diffY = position.Y - Position.Y;
            float diffZ = position.Z - Position.Z;

            if (diffX == 0f && diffY == 0f && diffZ == 0f)
                return;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var curVertice = Vertices[i];

                curVertice.X += diffX;
                curVertice.Y += diffY;
                curVertice.Z += diffZ;

                Vertices[i] = curVertice;
            }

            base.SetPosition(position);
        }

        public void SetHeight(float height)
        {
            if (height < 0f)
                height = 0;

            Height = height;

            UpdatePolygonCenterAndMaxRange();
        }

        public void AddVertice(Vector3 vertice)
        {
            if (Type == Types.Cuboid)
                return;

            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            Vertices.Add(vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void InsertVertice(int idx, Vector3 vertice)
        {
            if (Type == Types.Cuboid)
                return;

            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            if (idx >= Vertices.Count)
                return;

            Vertices.Insert(idx, vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void RemoveVertice(int verticeId)
        {
            if (Type == Types.Cuboid)
                return;

            if (verticeId < 0 || verticeId >= Vertices.Count)
                return;

            Vertices.RemoveAt(verticeId);

            if (Vertices.Count == 0)
            {
                Destroy();

                return;
            }

            UpdatePolygonCenterAndMaxRange();
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= MaxRange + Settings.STREAM_DISTANCE;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            var vertIdLimiter = Vertices.Count <= 50 ? 1 : 10;

            if (Vertices.Count == 1)
            {
                var vertice = Vertices[0];

                RAGE.Game.Graphics.DrawLine(vertice.X, vertice.Y, vertice.Z, vertice.X, vertice.Y, vertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
            }
            else if (Settings.Other.HighPolygonsMode)
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Position.X, Position.Y, Position.Z + Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z - Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            }
            else
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(nextVertice.X, nextVertice.Y, nextVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            }

            if (Settings.Other.DebugLabels)
            {
                if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Vertices: {Vertices.Count} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            double angleSum = 0f;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var p1 = new Vector3(Vertices[i].X - point.X, Vertices[i].Y - point.Y, Vertices[i].Z - point.Z);
                var p2 = new Vector3(Vertices[(i + 1) % Vertices.Count].X - point.X, Vertices[(i + 1) % Vertices.Count].Y - point.Y, Vertices[(i + 1) % Vertices.Count].Z - point.Z);

                var m1 = Math.Sqrt((p1.X * p1.X) + (p1.Y * p1.Y) + (p1.Z * p1.Z));
                var m2 = Math.Sqrt((p2.X * p2.X) + (p2.Y * p2.Y) + (p2.Z * p2.Z));

                if (m1 * m2 <= float.Epsilon)
                {
                    angleSum = Math.PI * 2;

                    break;
                }
                else
                    angleSum += Math.Acos((p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z) / (m1 * m2));
            }

            var polygonPoints2d = new List<RAGE.Ui.Cursor.Vector2>();

            if (Height == 0)
            {
                for (int i = 0; i < Vertices.Count; i++)
                    polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
            }
            else
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (point.Z >= Vertices[i].Z && point.Z <= (Vertices[i].Z + Height) || angleSum >= 5.8f)
                        polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
                    else
                        return false;
                }
            }

            bool inside = false;

            for (int i = 0, j = polygonPoints2d.Count - 1; i < polygonPoints2d.Count; j = i++)
            {
                float xi = polygonPoints2d[i].X, yi = polygonPoints2d[i].Y;
                float xj = polygonPoints2d[j].X, yj = polygonPoints2d[j].Y;

                if (((yi > point.Y) != (yj > point.Y)) && (point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi))
                    inside = !inside;
            }

            return inside;
        }
    }
}
