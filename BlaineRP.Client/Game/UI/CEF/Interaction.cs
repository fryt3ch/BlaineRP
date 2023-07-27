using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Data.Vehicles;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Management.Attachments;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.Scripts;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Interaction
    {
        public enum PassengersMenuActions
        {
            Interact = 0,
            Kick,
        }

        public Interaction()
        {
            TempBinds = new List<int>();

            Events.Add("Interaction::Select",
                (args) =>
                {
                    if (args == null)
                        return;

                    string mainType = args.Length < 1 ? null : (string)args[0];

                    string subType = args.Length > 1 ? (string)args[1] : null;

                    string type = args.Length > 2 ? (string)args[2] : null;

                    if (mainType == null)
                        return;

                    if (subType == null)
                        return;

                    Action<Entity> action = InteractionInfo.GetAction(mainType, subType, type ?? string.Empty);

                    if (action == null)
                        return;

                    CloseMenu();

                    action.Invoke(Management.Interaction.CurrentEntity);
                }
            );

            Events.Add("Interaction::Close", (args) => CloseMenu());

            OutVehicleInteractionInfo.AddAction("doors",
                "open",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.Lock(false, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("doors",
                "close",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.Lock(true, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("doors",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.Lock(null, veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("push",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    PushVehicle.Toggle(veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("trunk",
                "look",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.ShowContainer(veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("trunk",
                "open",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.ToggleTrunkLock(false, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("trunk",
                "close",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.ToggleTrunkLock(true, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("trunk", "", OutVehicleInteractionInfo.GetAction("trunk", "look"));

            OutVehicleInteractionInfo.AddAction("hood",
                "look",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.LookHood(veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("hood",
                "open",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.ToggleHoodLock(false, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("hood",
                "close",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.ToggleHoodLock(true, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("hood", "", OutVehicleInteractionInfo.GetAction("hood", "look"));

            OutVehicleInteractionInfo.AddAction("seat",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;

                    var freeSeats = new List<(decimal, string)>();

                    for (int i = -1; i < veh.GetMaxNumberOfPassengers(); i++)
                    {
                        if (veh.IsSeatFree(i, 0))
                            freeSeats.Add((i + 1, Locale.Get("POLICE_PTOVEH_L_0", i + 2)));
                    }

                    if (Player.LocalPlayer.Vehicle != veh)
                    {
                        AttachmentEntity trunkAttach = Management.Attachments.Core.GetEntityEntityAttachments(veh)
                                                                ?.Where(x => x.Type == AttachmentType.VehicleTrunk)
                                                                 .FirstOrDefault();

                        if (trunkAttach == null && veh.DoesHaveDoor(5) > 0)
                            freeSeats.Add((int.MaxValue, Locale.Get("POLICE_PTOVEH_L_1")));
                    }

                    if (freeSeats.Count == 0)
                        Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_0"));
                    else if (freeSeats.Count == 1)
                        Vehicles.SeatTo((int)freeSeats[0].Item1, veh);
                    else
                        ActionBox.ShowSelect("PlayerInteractVehicleSeatToSelect",
                            Locale.Get("POLICE_PTOVEH_L_2"),
                            freeSeats.ToArray(),
                            null,
                            null,
                            ActionBox.DefaultBindAction,
                            (rType, id) =>
                            {
                                if (rType != ActionBox.ReplyTypes.OK)
                                {
                                    ActionBox.Close(true);

                                    return;
                                }

                                var seatIdx = (int)id;

                                ActionBox.Close(true);

                                Vehicles.SeatTo(seatIdx, veh);
                            },
                            null
                        );
                }
            );

            OutVehicleInteractionInfo.AddAction("seat",
                "s_one",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SeatTo(0, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("seat",
                "s_two",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SeatTo(1, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("seat",
                "s_three",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SeatTo(2, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("seat",
                "s_four",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SeatTo(3, veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("seat",
                "s_trunk",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SeatTo(int.MaxValue, veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("gas",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Gas.RequestShow(veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("park",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.Park(veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("other",
                "remove_np",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.TakePlate(veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("other",
                "put_np",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.SetupPlate(veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("other",
                "fix",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.FixVehicle(veh);
                }
            );
            OutVehicleInteractionInfo.AddAction("other",
                "junkyard",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    VehicleDestruction.VehicleDestruct(veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("vehdoc",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Events.CallRemote("Vehicles::ShowPass", veh);
                }
            );

            OutVehicleInteractionInfo.AddAction("other",
                "trailer",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    Vehicles.BoatFromTrailerToWater(veh);
                }
            );


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

            InVehicleInteractionInfo.AddAction("passengers",
                "",
                (entity) =>
                {
                    var veh = entity as RAGE.Elements.Vehicle;
                    if (veh == null)
                        return;
                    ShowPassengers();
                }
            );

            InVehicleInteractionInfo.AddAction("vehdoc", "", OutVehicleInteractionInfo.GetAction("vehdoc", ""));

            InVehicleInteractionInfo.AddAction("park", "", OutVehicleInteractionInfo.GetAction("park", ""));

            //InVehicleInteractionInfo.AddAction("other_down", "", OutVehicleInteractionInfo.GetAction("gas", ""));

            CharacterInteractionInfo.AddAction("interact",
                "coin",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    Offers.Request(player, OfferTypes.HeadsOrTails, null);
                }
            );
            CharacterInteractionInfo.AddAction("interact",
                "handshake",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    Offers.Request(player, OfferTypes.Handshake, null);
                }
            );
            CharacterInteractionInfo.AddAction("interact",
                "carry",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    Offers.Request(player, OfferTypes.Carry, null);
                }
            );

            CharacterInteractionInfo.AddAction("money",
                "money_50",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerCashRequest(player, 50);
                }
            );
            CharacterInteractionInfo.AddAction("money",
                "money_150",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerCashRequest(player, 150);
                }
            );
            CharacterInteractionInfo.AddAction("money",
                "money_300",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerCashRequest(player, 300);
                }
            );
            CharacterInteractionInfo.AddAction("money",
                "money_1000",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerCashRequest(player, 1000);
                }
            );
            CharacterInteractionInfo.AddAction("money",
                "",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerCashRequest(player, 0);
                }
            );

            CharacterInteractionInfo.AddAction("trade",
                "",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    Offers.Request(player, OfferTypes.Exchange, null);
                }
            );

            CharacterInteractionInfo.AddAction("property",
                "settle",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerSettleRequest(player);
                }
            );
            CharacterInteractionInfo.AddAction("property",
                "sell_house",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerSellPropertyRequest(player, 2);
                }
            );
            CharacterInteractionInfo.AddAction("property",
                "sell_car",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerSellPropertyRequest(player, 0);
                }
            );
            CharacterInteractionInfo.AddAction("property",
                "sell_buis",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerSellPropertyRequest(player, 1);
                }
            );

            CharacterInteractionInfo.AddAction("documents",
                "medbook",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerShowDocumentsRequest(player, 0);
                }
            );
            CharacterInteractionInfo.AddAction("documents",
                "passport",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerShowDocumentsRequest(player, 1);
                }
            );
            CharacterInteractionInfo.AddAction("documents",
                "char_veh",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerShowDocumentsRequest(player, 2);
                }
            );
            CharacterInteractionInfo.AddAction("documents",
                "license",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    PlayerShowDocumentsRequest(player, 3);
                }
            );

            CharacterInteractionInfo.AddAction("heal",
                "pulse",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    ResurrectPlayer(player);
                }
            );
            CharacterInteractionInfo.AddAction("heal",
                "bandage",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    GivePlayerHealingItem(player, "med_b_0");
                }
            );
            CharacterInteractionInfo.AddAction("heal",
                "cure_0",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    GivePlayerHealingItem(player, "med_kit_0");
                }
            );
            CharacterInteractionInfo.AddAction("heal",
                "cure_1",
                (entity) =>
                {
                    var player = entity as Player;
                    if (player == null)
                        return;
                    GivePlayerHealingItem(player, "med_kit_ems_0");
                }
            );

            Events.Add("Interaction::PassengersMenuSelect",
                (args) =>
                {
                    var action = (PassengersMenuActions)(int)args[0];
                    var id = Utils.Convert.ToUInt16(args[1]);

                    CloseMenu();

                    if (action == PassengersMenuActions.Interact)
                        PlayerInteraction(id);
                    else if (action == PassengersMenuActions.Kick)
                        PlayerKick(id);
                    else
                        return;
                }
            );
        }

        public static bool IsActive => Browser.IsActiveOr(Browser.IntTypes.Interaction, Browser.IntTypes.Interaction_Passengers);

        private static List<int> TempBinds { get; set; }

        public static InteractionInfo CharacterInteractionInfo { get; set; } = new InteractionInfo("char")
        {
            MainLabels = new List<string>()
            {
                "interact",
                "trade",
                "property",
                "money",
                "heal",
                "char_job",
                "documents",
            },
            ExtraLabels = new List<List<string>>()
            {
                new List<string>()
                {
                    "carry",
                    "coin",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "handshake",
                    "kiss",
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    "sell_house",
                    "sell_car",
                    "sell_buis",
                    "settle",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    "money_50",
                    "money_150",
                    "money_300",
                    "money_1000",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "pulse",
                    "bandage",
                    "cure_0",
                    "cure_1",
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "char_veh",
                    "medbook",
                    "resume",
                    "license",
                    "passport",
                },
            },
        };

        public static InteractionInfo InVehicleInteractionInfo { get; set; } = new InteractionInfo("in_veh")
        {
            MainLabels = new List<string>()
            {
                "doors",
                "seat",
                "trunk",
                "hood",
                "music",
                "passengers",
                "park",
                "vehdoc",
                "job",
                "other_down",
            },
            ExtraLabels = new List<List<string>>()
            {
                new List<string>()
                {
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    "s_one",
                    "s_two",
                    "s_three",
                    "s_four",
                    "s_trunk",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
            },
        };

        public static InteractionInfo OutVehicleInteractionInfo { get; set; } = new InteractionInfo("out_veh")
        {
            MainLabels = new List<string>()
            {
                "doors",
                "seat",
                "trunk",
                "hood",
                "push",
                "other",
                "park",
                "vehdoc",
                "job",
                "gas",
            },
            ExtraLabels = new List<List<string>>()
            {
                new List<string>()
                {
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    "s_one",
                    "s_two",
                    "s_three",
                    "s_four",
                    "s_trunk",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    "look",
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "look",
                    "open",
                    "close",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "junkyard",
                    "remove_np",
                    "put_np",
                    "fix",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
                new List<string>()
                {
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                },
            },
        };

        public static bool TryShowMenu()
        {
            if (Management.Interaction.CurrentEntity == null || IsActive)
                return false;

            Entity entity = Management.Interaction.CurrentEntity;

            if (Utils.Misc.IsAnyCefActive())
                return false;

            Management.Interaction.Enabled = false;

            Management.Interaction.CurrentEntity = entity;

            if (entity is RAGE.Elements.Vehicle vehicle)
            {
                if (Player.LocalPlayer.Vehicle == null)
                {
                    if (Data.Vehicles.Core.GetByModel(vehicle.Model)?.Type == VehicleTypes.Boat)
                        OutVehicleInteractionInfo.ReplaceExtraLabelTemp("other", 8, "trailer");

                    ShowMenu(OutVehicleInteractionInfo);
                }
                else
                {
                    ShowMenu(InVehicleInteractionInfo);
                }

                Main.Render -= CheckEntityDistance;
                Main.Render += CheckEntityDistance;

                return true;
            }
            else if (entity is Player player)
            {
                ShowMenu(CharacterInteractionInfo);

                Main.Render -= CheckEntityDistance;
                Main.Render += CheckEntityDistance;

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
                        if (obj.GetData<Furniture>("Furniture") is Furniture fData)
                            fData.InteractionAction?.Invoke(obj);
                    }
                    else if (obj.HasData("CustomAction"))
                    {
                        Action<MapObject> cAction = obj.GetData<Action<MapObject>>("CustomAction");

                        cAction?.Invoke(obj);
                    }
                }
                else
                {
                    if (obj.GetSharedData<int>("IOG") == 1)
                    {
                        var iog = ItemOnGround.GetItemOnGroundObject(obj);

                        if (iog != null)
                            ActionBox.ShowSelect("PlacedItemOnGroundSelect",
                                Locale.Actions.PlacedItemOnGroundSelectHeader,
                                new (decimal, string)[]
                                {
                                    (0, Locale.Actions.PlacedItemOnGroundSelectInteract),
                                    (1, iog.IsLocked ? Locale.Actions.PlacedItemOnGroundSelectUnlock : Locale.Actions.PlacedItemOnGroundSelectLock),
                                    (2, Locale.Actions.PlacedItemOnGroundSelectTake),
                                },
                                null,
                                null,
                                ActionBox.DefaultBindAction,
                                (rType, id) =>
                                {
                                    ActionBox.Close(true);

                                    if (rType == ActionBox.ReplyTypes.OK)
                                    {
                                        if (iog?.Object?.Exists != true)
                                            return;

                                        if (id == 0)
                                            Inventory.Show(Inventory.Types.Workbench, 0, iog.Uid);
                                        else if (id == 1)
                                            Events.CallRemote("Item::IOGL", iog.Uid, !iog.IsLocked);
                                        else if (id == 2)
                                            iog.TakeItem();
                                    }
                                },
                                null
                            );
                    }
                }
            }

            Management.Interaction.Enabled = true;

            return false;
        }

        public static void ShowMenu(InteractionInfo info)
        {
            Browser.Switch(Browser.IntTypes.Interaction, true);

            List<List<string>> extraLabels = info.ExtraLabels;

            if (info.ExtraLabelsTemp != null)
            {
                extraLabels = info.ExtraLabelsTemp;

                info.ExtraLabelsTemp = null;
            }

            Browser.Window.ExecuteJs("Interaction.draw", info.MainType, info.MainLabels, extraLabels.Select(x => x == null ? x : x.Select(y => y ?? "none").ToList()));

            Input.Core.Get(BindTypes.Interaction).Disable();

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Cursor.Show(true, true);
        }

        private static async void ShowPassengers()
        {
            RAGE.Elements.Vehicle veh = Player.LocalPlayer.Vehicle;

            if (veh == null)
                return;

            var vehData = VehicleData.GetData(veh);

            if (vehData == null)
                return;

            var players = new List<object>();

            foreach (Player x in Vehicles.GetPlayersInVehicle(veh))
            {
                if (x.Handle == Player.LocalPlayer.Handle)
                    continue;

                var data = PlayerData.GetData(x);

                players.Add(new object[]
                    {
                        x.RemoteId,
                        data == null ? "null" : x.GetName(true, false, true),
                    }
                );
            }

            // If no Passengers
            if (players.Count == 0)
            {
                Notification.ShowError(Locale.Get("VEHICLE_SEAT_E_1"));

                return;
            }

            await Browser.Render(Browser.IntTypes.Interaction_Passengers, true, true);

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => CloseMenu()));

            Browser.Window.ExecuteJs($"Passengers.fill", players);

            Cursor.Show(true, true);
        }

        public static void PlayerInteraction(ushort id)
        {
            if (Player.LocalPlayer.Vehicle == null)
                return;

            Player player = Entities.Players.GetAtRemote(id);

            if (player?.Exists != true)
                return;

            CloseMenu();

            Management.Interaction.Enabled = false;

            Management.Interaction.CurrentEntity = player;

            TryShowMenu();
        }

        public static void PlayerKick(ushort id)
        {
            if (Player.LocalPlayer.Vehicle == null)
                return;

            Player player = Entities.Players.GetAtRemote(id);

            if (player?.Exists != true)
                return;

            Vehicles.KickPassenger(player);

            CloseMenu();
        }

        public static void CloseMenu()
        {
            if (!IsActive)
                return;

            Browser.Switch(Browser.IntTypes.Interaction, false);
            Browser.Render(Browser.IntTypes.Interaction_Passengers, false);

            Main.Render -= CheckEntityDistance;

            Input.Core.Get(BindTypes.Interaction).Enable();

            foreach (int x in TempBinds)
            {
                Input.Core.Unbind(x);
            }

            TempBinds.Clear();

            Cursor.Show(false, false);

            Management.Interaction.Enabled = true;
        }

        private static void CheckEntityDistance()
        {
            if (Management.Interaction.CurrentEntity?.IsNull != false ||
                Vector3.Distance(Player.LocalPlayer.Position, Management.Interaction.CurrentEntity.Position) > Settings.App.Static.EntityInteractionMaxDistance)
            {
                Notification.Show(Notification.Types.Information, Locale.Notifications.Interaction.Header, Locale.Notifications.Interaction.DistanceTooLarge);

                CloseMenu();
            }
        }

        public static async void PlayerCashRequest(Player player, int amount)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (amount < 0)
                return;

            if (amount == 0)
            {
                if (pData.Cash <= 0)
                {
                    Notification.Show("Trade::NotEnoughMoney");

                    return;
                }

                await ActionBox.ShowRange("GiveCash",
                    string.Format(Locale.Actions.GiveCash, player.GetName(true, false, true)),
                    1,
                    pData.Cash,
                    pData.Cash / 2,
                    -1,
                    ActionBox.RangeSubTypes.Default,
                    ActionBox.DefaultBindAction,
                    (rType, amountD) =>
                    {
                        int amount;

                        if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                            return;

                        ActionBox.Close(true);

                        if (rType == ActionBox.ReplyTypes.OK)
                            if (player is Player targetPlayer)
                                Offers.Request(targetPlayer,
                                    OfferTypes.Cash,
                                    new
                                    {
                                        Amount = amount,
                                    }
                                );
                    },
                    null
                );
            }
            else
            {
                if (pData.Cash <= (ulong)amount)
                {
                    Notification.Show("Trade::NotEnoughMoney");

                    return;
                }

                Offers.Request(player,
                    OfferTypes.Cash,
                    new
                    {
                        Amount = amount,
                    }
                );
            }
        }

        public static void PlayerSettleRequest(Player player)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            HouseBase currentHouse = Player.LocalPlayer.GetData<HouseBase>("House::CurrentHouse");

            if (currentHouse == null)
            {
                Notification.ShowError(Locale.Notifications.House.NotInAnyHouseOrApartments);

                return;
            }

            if (!pData.OwnedHouses.Contains(currentHouse))
            {
                Notification.ShowError(Locale.Notifications.House.NotAllowed);

                return;
            }

            Offers.Request(player, OfferTypes.Settle, null);
        }

        public static void PlayerSellPropertyRequest(Player player, byte type)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (type == 0)
            {
                if (pData.OwnedVehicles.Count == 0)
                {
                    Notification.ShowError(Locale.Notifications.Vehicles.NoOwnedVehicles);

                    return;
                }

                Estate.ShowSellVehicle(player, true);
            }
            else if (type == 1)
            {
                if (pData.OwnedBusinesses.Count == 0)
                {
                    Notification.ShowError(Locale.Notifications.General.NoOwnedBusiness);

                    return;
                }

                Estate.ShowSellBusiness(player, true);
            }
            else if (type == 2)
            {
                if (pData.OwnedApartments.Count == 0 && pData.OwnedHouses.Count == 0 && pData.OwnedGarages.Count == 0)
                {
                    Notification.ShowError(Locale.Notifications.General.NoOwnedEstate);

                    return;
                }

                Estate.ShowSellEstate(player, true);
            }
        }

        public static void PlayerShowDocumentsRequest(Player player, byte type)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (type == 0)
            {
                if (pData.MedicalCard == null)
                {
                    Notification.ShowError(Locale.Notifications.General.NoMedicalCard);

                    return;
                }

                Offers.Request(player, OfferTypes.ShowMedicalCard, null);
            }
            else if (type == 1)
            {
                Offers.Request(player, OfferTypes.ShowPassport, null);
            }
            else if (type == 2)
            {
                List<(uint VID, Data.Vehicles.Vehicle Data)> allVehs = pData.OwnedVehicles;

                if (allVehs.Count == 0)
                {
                    Notification.ShowError(Locale.Notifications.Vehicles.NoOwnedVehicles);

                    return;
                }

                /*                if (allVehs.Count == 1)
                                {
                                    Sync.Offers.Request(player, Sync.Offers.Types.ShowVehiclePassport, allVehs[0].VID);

                                    return;
                                }*/

                var t = 0;

                ActionBox.ShowSelect("VehiclePassportSelect",
                    Locale.Actions.VehiclePassportSelectHeader,
                    allVehs.Select(x => ((decimal)t++, $"{x.Data.SubName} [#{x.VID}]")).ToArray(),
                    null,
                    null,
                    ActionBox.DefaultBindAction,
                    (rType, idD) =>
                    {
                        var id = (int)idD;

                        if (rType == ActionBox.ReplyTypes.OK)
                        {
                            var pData = PlayerData.GetData(Player.LocalPlayer);

                            if (pData == null)
                                return;

                            List<(uint VID, Data.Vehicles.Vehicle Data)> allVehs = pData.OwnedVehicles;

                            if (allVehs.Count <= id)
                            {
                                ActionBox.Close(true);

                                return;
                            }

                            ActionBox.Close(true);

                            Offers.Request(player,
                                OfferTypes.ShowVehiclePassport,
                                new
                                {
                                    VID = allVehs[id].VID,
                                }
                            );
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
                    null
                );
            }
            else if (type == 3)
            {
                Offers.Request(player, OfferTypes.ShowLicenses, null);
            }
        }

        public static async void ResurrectPlayer(Player player)
        {
            var pData = PlayerData.GetData(player);

            if (pData == null)
                return;

            if (!pData.IsKnocked)
            {
                Notification.ShowError(Locale.Get("NTFC_PLAYER_RESURRECT_E_0"));

                return;
            }

            var items = new Dictionary<string, int>();

            for (var i = 0; i < Inventory.ItemsParams.Length; i++)
            {
                Inventory.ItemParams item = Inventory.ItemsParams[i];

                if (item == null)
                    continue;

                if (item.Id == "med_kit_0" || item.Id == "med_kit_ems_0")
                    items.TryAdd(item.Id, i);
            }

            if (items.Count == 0)
            {
                Notification.ShowError(Locale.Get("NTFC_ITEMS_NO_MED_KIT_RESURRECT_0"));

                return;
            }
            else if (items.Count == 1)
            {
                proceed(items.FirstOrDefault().Value);

                return;
            }

            await ActionBox.ShowSelect("RESURRECT_PLAYER_ITEM",
                "Реанимировать {0}",
                items.Select(x => ((decimal)x.Value, Items.Core.GetName(x.Key))).ToArray(),
                null,
                null,
                ActionBox.DefaultBindAction,
                async (rType, id) =>
                {
                    if (rType == ActionBox.ReplyTypes.Cancel)
                        ActionBox.Close(true);
                    else
                        proceed((int)id);
                },
                null
            );

            async void proceed(int itemIdx)
            {
                var res = (int)await Events.CallRemoteProc("Player::ResurrectItem", player, itemIdx);
            }
        }

        public static void GivePlayerHealingItem(Player player, string itemId)
        {
            var pData = PlayerData.GetData(player);

            if (pData == null)
                return;

            int itemIdx = -1;

            for (var i = 0; i < Inventory.ItemsParams.Length; i++)
            {
                if (Inventory.ItemsParams[i]?.Id == itemId)
                {
                    itemIdx = i;

                    break;
                }
            }

            if (itemIdx < 0)
            {
                Notification.Show("Inventory::NoItem");

                return;
            }

            Offers.Request(player,
                OfferTypes.GiveHealingItem,
                new
                {
                    ItemIdx = itemIdx,
                }
            );
        }

        public class InteractionInfo
        {
            public InteractionInfo(string MainType)
            {
                this.MainType = MainType;
            }

            private static Dictionary<string, Dictionary<string, Dictionary<string, Action<Entity>>>> Actions { get; set; } =
                new Dictionary<string, Dictionary<string, Dictionary<string, Action<Entity>>>>();

            public string MainType { get; private set; }

            public List<string> MainLabels { get; set; }

            public List<List<string>> ExtraLabels { get; set; }

            public List<List<string>> ExtraLabelsTemp { get; set; }

            public static void AddAction(string mainType, string subType, string type, Action<Entity> action)
            {
                Dictionary<string, Dictionary<string, Action<Entity>>> dict = Actions.GetValueOrDefault(mainType);

                if (dict == null)
                {
                    dict = new Dictionary<string, Dictionary<string, Action<Entity>>>();

                    Actions.Add(mainType, dict);
                }

                Dictionary<string, Action<Entity>> subDict = dict.GetValueOrDefault(subType);

                if (subDict == null)
                {
                    subDict = new Dictionary<string, Action<Entity>>();

                    dict.Add(subType, subDict);
                }

                if (!subDict.TryAdd(type, action))
                    subDict[type] = action;
            }

            public void AddAction(string subType, string type, Action<Entity> action)
            {
                AddAction(MainType, subType, type, action);
            }

            public static Action<Entity> GetAction(string mainType, string subType, string type)
            {
                return Actions.GetValueOrDefault(mainType)?.GetValueOrDefault(subType)?.GetValueOrDefault(type);
            }

            public Action<Entity> GetAction(string subType, string type)
            {
                return GetAction(MainType, subType, type);
            }

            public void ReplaceExtraLabel(string subType, int pIdx, string type)
            {
                int mIdx = MainLabels.IndexOf(subType);

                if (mIdx < 0)
                    return;

                /*                if (pIdx < 0 || pIdx >= MainLabels.Count * 2)
                                    return;*/

                List<string> eLabels = ExtraLabels[mIdx];

                eLabels[pIdx] = type;
            }

            public void ReplaceExtraLabelTemp(string subType, int pIdx, string type)
            {
                int mIdx = MainLabels.IndexOf(subType);

                if (mIdx < 0)
                    return;

                if (pIdx < 0 || pIdx >= MainLabels.Count * 2)
                    return;

                if (ExtraLabelsTemp == null)
                {
                    ExtraLabelsTemp = new List<List<string>>();

                    foreach (List<string> x in ExtraLabels)
                    {
                        var t = new List<string>();

                        t.AddRange(x);

                        ExtraLabelsTemp.Add(t);
                    }
                }

                List<string> eLabels = ExtraLabelsTemp[mIdx];

                eLabels[pIdx] = type;
            }
        }
    }
}