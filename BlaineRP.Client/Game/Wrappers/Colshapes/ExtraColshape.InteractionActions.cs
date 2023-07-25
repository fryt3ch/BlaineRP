using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Data;
using BlaineRP.Client.Data.Minigames.Casino;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Fractions.Types;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using RAGE;
using RAGE.Elements;
using NPC = BlaineRP.Client.Game.NPCs.NPC;
using Vehicle = RAGE.Elements.Vehicle;
using VehicleData = BlaineRP.Client.Game.EntitiesData.VehicleData;

namespace BlaineRP.Client.Game.Wrappers.Colshapes
{
    public abstract partial class ExtraColshape
    {
        private static readonly Dictionary<InteractionTypes, Action> _interactionActions = new Dictionary<InteractionTypes, Action>()
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

                    var casino = Locations.Casino.GetById(casinoId);

                    var table = casino.GetBlackjackById(tableId);

                    LastSent = World.Core.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::BLJE", casinoId, tableId))?.Split('^');

                    if (res == null)
                        return;

                    if (table?.TableObject?.Exists != true)
                        return;

                    var balance = decimal.Parse(res[0]);
                    var seatIdx = byte.Parse(res[1]);
                    var stateData = (string)res[2];

                    if (table.CurrentStateData != stateData)
                        Locations.Casino.Blackjack.OnCurrentStateDataUpdated(casinoId, tableId, stateData, true);

                    Casino.ShowBlackjack(casino, table, seatIdx, balance);
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

                    var casino = Locations.Casino.GetById(casinoId);

                    var slotMachine = casino.GetSlotMachineById(slotMachineId);

                    LastSent = World.Core.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::SLME", casinoId, slotMachineId))?.Split('^');

                    if (res == null)
                        return;

/*                    Utils.ConsoleOutput(slotMachineId);
                    Utils.ConsoleOutput(slotMachine?.MachineObj.Handle ?? -1);*/

                    if (slotMachine?.MachineObj?.Exists != true)
                        return;

                    var balance = decimal.Parse(res[0]);
                    var jackpot = decimal.Parse(res[1]);

                    Casino.ShowSlotMachine(casino, slotMachine, balance, jackpot);
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

                    var casino = Locations.Casino.GetById(casinoId);

                    var luckyWheel = casino.GetLuckyWheelById(luckyWheelId);

                    LastSent = World.Core.ServerTime;

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

                    LastSent = World.Core.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("Casino::RLTJ", casinoId, rouletteId))?.Split('^');

                    if (res == null)
                        return;

                    var chipsBalance = decimal.Parse(res[0]);
                    var stateData = (string)res[1];

                    var casino = Locations.Casino.GetById(casinoId);

                    var roulette = casino.GetRouletteById(rouletteId);

                    if (roulette.TextLabel == null)
                        return;

                    if (roulette.CurrentStateData != stateData)
                        Locations.Casino.Roulette.OnCurrentStateDataUpdated(casinoId, rouletteId, stateData, true);

                    Casino.ShowRoulette(casino, roulette, chipsBalance);
                }
            },
            {
                InteractionTypes.ElevatorInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var elevatorId = Player.LocalPlayer.GetData<uint>("EXED::ElevatorId");

                    var elevatorData = Locations.Elevator.Get(elevatorId);

                    if (elevatorData == null)
                        return;

                    await ActionBox.ShowSelect("ElevatorSelect",
                        "Выбор места перемещения",
                        elevatorData.LinkedElevators.Select(x => ((decimal)x, Locations.Elevator.GetName(x))).ToArray(),
                        null,
                        null,
                        ActionBox.DefaultBindAction,
                        async (rType, idD) =>
                        {
                            var toId = (uint)idD;

                            if (rType == ActionBox.ReplyTypes.OK)
                            {
                                if (ActionBox.LastSent.IsSpam(500, false, true))
                                    return;

                                ActionBox.LastSent = World.Core.ServerTime;

                                var res = (bool)await Events.CallRemoteProc("Elevator::TP", elevatorId, toId);

                                if (res)
                                    ActionBox.Close();
                            }
                            else
                            {
                                ActionBox.Close();
                            }
                        },
                        null);
                }
            },
            {
                InteractionTypes.FractionPoliceArrestMenuInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = Fraction.Get((Fractions.Enums.FractionTypes)int.Parse(fTypeS[0]));

                    if (pData.CurrentFraction != fData)
                    {
                        Notification.Show("Fraction::NM");

                        return;
                    }

                    var menuIdx = byte.Parse(fTypeS[1]);

                    var arrestsData = fData?.GetCurrentData<List<Police.ArrestInfo>>("Arrests");

                    if (arrestsData == null)
                        return;

                    await ArrestsMenu.Show(fData.Type, menuIdx, arrestsData);
                }
            },
            {
                InteractionTypes.EstateAgencyInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var estAgencyIds = Player.LocalPlayer.GetData<string>("EXED::DriveSchoolId")?.Split('_');

                    if (estAgencyIds == null)
                        return;

                    var id = int.Parse(estAgencyIds[0]);
                    var posId = int.Parse(estAgencyIds[1]);

                    LastSent = World.Core.ServerTime;

                    var res = ((string)await Events.CallRemoteProc("EstAgency::GD", id, posId))?.Split('_');

                    if (res == null)
                        return;

                    await EstateAgency.Show(id, posId, decimal.Parse(res[0]), 0, 0);
                }
            },
            {
                InteractionTypes.DrivingSchoolInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    if (!Player.LocalPlayer.HasData("EXED::DriveSchoolId"))
                        return;

                    var schoolId = Player.LocalPlayer.GetData<int>("EXED::DriveSchoolId");

                    var lics = pData.Licenses;

                    var notOwnedLics = Locations.Autoschool.Prices.Keys.Where(x => !lics.Contains(x)).ToHashSet();

                    if (notOwnedLics.Count == 0)
                    {
                        Notification.ShowError("Вы уже владеете всеми лицензиями для транспорта!", -1);

                        return;
                    }

                    await ActionBox.ShowSelect("DrivingSchoolSelect",
                        "Выбор категории",
                        notOwnedLics.Select(x => ((decimal)x, Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(x.GetType(), x.ToString(), "NAME_0") ?? "null"))).ToArray(),
                        null,
                        null,
                        ActionBox.DefaultBindAction,
                        (rType, id) =>
                        {
                            ActionBox.Close(true);

                            if (rType == ActionBox.ReplyTypes.OK)
                            {
                                var licType = (LicenseTypes)id;

                                AutoschoolTest.Show(schoolId, licType, Locations.Autoschool.Prices.GetValueOrDefault(licType));
                            }
                        },
                        null);
                }
            },
            {
                InteractionTypes.FractionLockerRoomInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = Fraction.Get((Fractions.Enums.FractionTypes)int.Parse(fTypeS[0]));

                    var fDataUnif = fData as IUniformable;

                    if (fDataUnif == null)
                        return;

                    var lockerIdx = byte.Parse(fTypeS[1]);

                    if (pData.CurrentFraction != fData)
                    {
                        Notification.Show("Fraction::NM");

                        return;
                    }

                    var res = (int)await Events.CallRemoteProc("Fraction::UNIFS", (int)fData.Type, lockerIdx);

                    if (res == int.MinValue)
                        return;

                    var allButtons = new List<(decimal, string)>();

                    if (res >= 0)
                        allButtons.Add((-1, Locale.Get("FRACTION_LOCKER_0")));

                    for (var i = 0; i < fDataUnif.UniformNames.Length; i++)
                    {
                        if (res == i)
                            continue;

                        allButtons.Add((i, fDataUnif.UniformNames[i]));
                    }

                    await ActionBox.ShowSelect("FractionUniformSelect",
                        Locale.Get("FRACTION_LOCKER_1"),
                        allButtons.ToArray(),
                        null,
                        null,
                        ActionBox.DefaultBindAction,
                        (rType, id) =>
                        {
                            if (rType == ActionBox.ReplyTypes.OK)
                            {
                                Events.CallRemote("Fraction::UNIFC", (int)fData.Type, lockerIdx, id);

                                ActionBox.Close(true);
                            }
                            else if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                ActionBox.Close(true);
                            }
                            else
                            {
                                return;
                            }
                        },
                        null);
                }
            },
            {
                InteractionTypes.FractionCreationWorkbenchInteract, async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    var fTypeS = Player.LocalPlayer.GetData<string>("EXED::CFractionId")?.Split('_');

                    if (fTypeS == null)
                        return;

                    var fData = Fraction.Get((Fractions.Enums.FractionTypes)int.Parse(fTypeS[0]));

                    if (fData == null)
                        return;

                    var wbIdx = byte.Parse(fTypeS[1]);

                    if (pData.CurrentFraction != fData)
                    {
                        Notification.Show("Fraction::NM");

                        return;
                    }

                    if (fData.CreationWorkbenchPrices.Count == 0)
                    {
                        Notification.ShowError("На данный момент здесь нельзя создать ни один предмет!");

                        return;
                    }

                    LastSent = World.Core.ServerTime;

                    var res = (int)await Events.CallRemoteProc("Fraction::CWBS", (int)fData.Type, wbIdx);

                    if (res == byte.MaxValue)
                    {
                        if (Utils.Misc.IsAnyCefActive(true))
                            return;

                        MaterialWorkbench.Show(MaterialWorkbench.Types.Fraction, fData.CreationWorkbenchPrices, fData.Materials, fData.Type, wbIdx);
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

                    LastSent = World.Core.ServerTime;

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

                    LastSent = World.Core.ServerTime;

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

                    Estate.ShowBusinessInfo(Player.LocalPlayer.GetData<Locations.Business>("CurrentBusiness"), true);

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.HouseEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentHouse"))
                        return;

                    Estate.ShowHouseBaseInfo(Player.LocalPlayer.GetData<Locations.HouseBase>("CurrentHouse"), true);

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.HouseExit, async () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                        return;

                    var house = Player.LocalPlayer.GetData<Locations.HouseBase>("House::CurrentHouse");

                    if (house is Locations.House rHouse && rHouse.GarageType != null)
                    {
                        await ActionBox.ShowSelect("HouseExit",
                            Locale.Actions.HouseExitActionBoxHeader,
                            new (decimal, string)[] { (0, Locale.Actions.HouseExitActionBoxOutside), (1, Locale.Actions.HouseExitActionBoxToGarage), },
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            (rType, id) =>
                            {
                                ActionBox.Close(true);

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                if (rType == ActionBox.ReplyTypes.OK)
                                {
                                    // house/houseGarage -> outside
                                    if (id == 0)
                                        Events.CallRemote("House::Exit");
                                    // house -> garage
                                    else if (id == 1)
                                        Events.CallRemote("House::Garage", true);
                                }
                            },
                            null);
                    }
                    else
                    {
                        Events.CallRemote("House::Exit");

                        LastSent = World.Core.ServerTime;
                    }
                }
            },
            {
                InteractionTypes.GarageExit, async () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                        Events.CallRemote("Garage::Exit");
                    else
                        await ActionBox.ShowSelect("HouseExit",
                            Locale.Actions.HouseExitActionBoxHeader,
                            new (decimal, string)[] { (0, Locale.Actions.HouseExitActionBoxOutside), (1, Locale.Actions.HouseExitActionBoxToHouse), },
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            (rType, id) =>
                            {
                                ActionBox.Close(true);

                                if (LastSent.IsSpam(500, false, true))
                                    return;

                                if (rType == ActionBox.ReplyTypes.OK)
                                {
                                    // house/houseGarage -> outside
                                    if (id == 0)
                                        Events.CallRemote("House::Exit");
                                    // garage -> house
                                    else if (id == 1)
                                        Events.CallRemote("House::Garage", false);
                                }
                            },
                            null);
                }
            },
            {
                InteractionTypes.NpcDialogue, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentNPC"))
                        return;

                    var npc = Player.LocalPlayer.GetData<NPC>("CurrentNPC");

                    if (npc == null)
                        return;

                    npc.SwitchDialogue(true);

                    npc.ShowDialogue(npc.DefaultDialogueId);

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.ATM, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentATM"))
                        return;

                    Events.CallRemote("Bank::Show", true, Player.LocalPlayer.GetData<Locations.ATM>("CurrentATM").Id);

                    LastSent = World.Core.ServerTime;
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

                    var bVehData = VehicleData.GetData(baseVeh);

                    if (bVehData == null)
                        return;

                    var trailerVehHandle = baseVeh.GetTrailerVehicle();

                    if (trailerVehHandle > 0 && Utils.Game.Misc.GetVehicleByHandle(trailerVehHandle, false) is Vehicle trailerVeh)
                        if (trailerVeh.GetData<Vehicle>("TrailerSync::Owner") is Vehicle boat)
                        {
                            var boatData = VehicleData.GetData(boat);

                            if (boatData == null)
                                return;

                            var tuningId = Player.LocalPlayer.GetData<Locations.TuningShop>("CurrentTuning").Id;

                            await ActionBox.ShowSelect("VehicleTuningVehicleSelect",
                                Locale.Actions.VehicleTuningVehicleSelect,
                                new (decimal Id, string Text)[] { (1, $"{bVehData.Data.SubName} [#{bVehData.VID}]"), (2, $"{boatData.Data.SubName} [#{boatData.VID}]"), },
                                null,
                                null,
                                ActionBox.DefaultBindAction,
                                (rType, id) =>
                                {
                                    var pData = PlayerData.GetData(Player.LocalPlayer);

                                    if (pData == null)
                                        return;

                                    if (rType == ActionBox.ReplyTypes.OK)
                                    {
                                        if (id != 1 && id != 2)
                                            return;

                                        Events.CallRemote("TuningShop::Enter", tuningId, id == 1 ? baseVeh : boat);

                                        ActionBox.Close(true);
                                    }
                                    else if (rType == ActionBox.ReplyTypes.Cancel)
                                    {
                                        ActionBox.Close(true);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                },
                                null);

                            return;
                        }

                    Events.CallRemote("TuningShop::Enter", Player.LocalPlayer.GetData<Locations.TuningShop>("CurrentTuning").Id, baseVeh);

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.ShootingRangeEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentShootingRange"))
                        return;

                    Events.CallRemote("SRange::Enter::Shop", Player.LocalPlayer.GetData<Locations.WeaponShop>("CurrentShootingRange").Id);

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.ApartmentsRootEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    if (!Player.LocalPlayer.HasData("CurrentApartmentsRoot"))
                        return;

                    Events.CallRemote("ARoot::Enter", Player.LocalPlayer.GetData<Locations.ApartmentsRoot>("CurrentApartmentsRoot").Id);

                    LastSent = World.Core.ServerTime;
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

                    LastSent = World.Core.ServerTime;
                }
            },
            {
                InteractionTypes.ApartmentsRootElevator, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    var aRoot = Player.LocalPlayer.GetData<Locations.ApartmentsRoot>("ApartmentsRoot::Current");

                    if (aRoot == null)
                        return;

                    int elevI, elevJ;

                    if (aRoot.Shell.GetClosestElevator(Player.LocalPlayer.Position, out elevI, out elevJ))
                    {
                        LastSent = World.Core.ServerTime;

                        var shell = aRoot.Shell;

                        Elevator.Show(shell.StartFloor + shell.FloorsAmount - 1, null, Elevator.ContextTypes.ApartmentsRoot);
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

                    GarageMenu.Show(Player.LocalPlayer.GetData<Locations.GarageRoot>("CurrentGarageRoot"));

                    LastSent = World.Core.ServerTime;
                }
            },
            { InteractionTypes.MarketStallInteract, Locations.MarketStall.OnInteractionKeyPressed },
        };
    }
}