using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Casino;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Management.IPLs;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils.Game;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Helpers.Colshapes
{
    public abstract partial class ExtraColshape
    {
        private static readonly Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>> _actions = new Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>>()
        {
            {
                ActionTypes.MarketStallInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is MarketStall marketStall)
                            {
                                ushort currentRenterRid = marketStall.CurrentRenterRID;

                                string interactionText = null;

                                if (currentRenterRid == Player.LocalPlayer.RemoteId)
                                {
                                    interactionText = Locale.Get("INTERACTION_L_MARKETSTALL_1");
                                }
                                else if (currentRenterRid == ushort.MaxValue)
                                {
                                }
                                else
                                {
                                    interactionText = Locale.Get("INTERACTION_L_MARKETSTALL_2",
                                        Players.GetPlayerName(Entities.Players.GetAtRemote(currentRenterRid), true, false, false),
                                        currentRenterRid
                                    );
                                }

                                if (interactionText != null)
                                    Core.OverrideInteractionText = interactionText;

                                Player.LocalPlayer.SetData("CurrentMarketStall", marketStall);
                            }
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentMarketStall");
                        }
                    },
                }
            },
            {
                ActionTypes.CasinoInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is string str)
                            {
                                string[] d = str.Split('_');

                                var casino = Casino.Casino.GetById(int.Parse(d[0]));
                                Roulette roulette = casino.GetRouletteById(int.Parse(d[1]));

                                Player.LocalPlayer.SetData("CurrentCasinoGameData", str);
                            }
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentCasinoGameData");
                        }
                    },
                }
            },
            {
                ActionTypes.ElevatorInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is uint id)
                                Player.LocalPlayer.SetData("EXED::ElevatorId", id);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::ElevatorId");
                        }
                    },
                }
            },
            {
                ActionTypes.EstateAgencyInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is string id)
                                Player.LocalPlayer.SetData("EXED::DriveSchoolId", id);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::DriveSchoolId");
                        }
                    },
                }
            },
            {
                ActionTypes.DrivingSchoolInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is int id)
                                Player.LocalPlayer.SetData("EXED::DriveSchoolId", id);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::DriveSchoolId");
                        }
                    },
                }
            },
            {
                ActionTypes.FractionInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is string str)
                                Player.LocalPlayer.SetData("EXED::CFractionId", str);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::CFractionId");
                        }
                    },
                }
            },
            {
                ActionTypes.ContainerInteract, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is uint contId)
                                Player.LocalPlayer.SetData("EXED::ContId", contId);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("EXED::ContId");
                        }
                    },
                }
            },
            {
                ActionTypes.GasStation, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is int data)
                                Player.LocalPlayer.SetData("CurrentGasStation", data);
                            //CEF.Notification.ShowHint(Locale.Notifications.Hints.GasStationColshape, false, 2500);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentGasStation");

                            Gas.Close(true);
                        }
                    },
                }
            },
            {
                ActionTypes.GreenZone, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            Management.Weapons.Core.DisabledFiring = true;

                            Player.LocalPlayer.SetData("InGreenZone", true);

                            HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, true);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Management.Weapons.Core.DisabledFiring = false;

                            Player.LocalPlayer.ResetData("InGreenZone");

                            HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, false);
                        }
                    },
                }
            },
            {
                ActionTypes.IPL, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is IPLInfo ipl)
                                ipl.Load();
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            if (cs.Data is IPLInfo ipl)
                                ipl.Unload();
                        }
                    },
                }
            },
            {
                ActionTypes.BusinessInfo, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is Business biz)
                                Player.LocalPlayer.SetData("CurrentBusiness", biz);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentBusiness");
                        }
                    },
                }
            },
            {
                ActionTypes.HouseEnter, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (!(cs.Data is HouseBase houseBase))
                                return;

                            Player.LocalPlayer.SetData("CurrentHouse", houseBase);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentHouse");
                        }
                    },
                }
            },
            {
                ActionTypes.NpcDialogue, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is NPCs.NPC npc)
                                Player.LocalPlayer.SetData("CurrentNPC", npc);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentNPC");
                        }
                    },
                }
            },
            {
                ActionTypes.ATM, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is Misc.ATM atm)
                                Player.LocalPlayer.SetData("CurrentATM", atm);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentATM");
                        }
                    },
                }
            },
            {
                ActionTypes.TuningEnter, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is TuningShop ts)
                                Player.LocalPlayer.SetData("CurrentTuning", ts);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentTuning");
                        }
                    },
                }
            },
            {
                ActionTypes.ReachableBlip, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is ExtraBlip blip)
                            {
                                if (blip.Type == ExtraBlip.Types.AutoPilot)
                                    Scripts.Sync.Vehicles.ToggleAutoPilot(false, true);

                                blip.Destroy();

                                if (Player.LocalPlayer.Vehicle != null)
                                    Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_GPS"), Locale.Get("BLIP_GEN_GPS_FINISHED_0"));
                            }
                        }
                    },
                }
            },
            {
                ActionTypes.ShootingRangeEnter, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is WeaponShop ws)
                            {
                                Core.OverrideInteractionText = string.Format(Locale.Interaction.Names.GetValueOrDefault(InteractionTypes.ShootingRangeEnter, "null"),
                                    WeaponShop.ShootingRangePrice
                                );

                                Player.LocalPlayer.SetData("CurrentShootingRange", ws);
                            }
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentShootingRange");
                        }
                    },
                }
            },
            {
                ActionTypes.ApartmentsRootEnter, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is ApartmentsRoot aRoot)
                                Player.LocalPlayer.SetData("CurrentApartmentsRoot", aRoot);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentApartmentsRoot");
                        }
                    },
                }
            },
            {
                ActionTypes.GarageRootEnter, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is GarageRoot gRoot)
                                Player.LocalPlayer.SetData("CurrentGarageRoot", gRoot);
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentGarageRoot");
                        }
                    },
                }
            },
            {
                ActionTypes.VehicleSpeedLimit, new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true, (cs) =>
                        {
                            if (cs.Data is float maxSpeed && maxSpeed > 0f)
                            {
                                Player.LocalPlayer.SetData("ColshapeVehicleSpeedLimited", maxSpeed);

                                if (Player.LocalPlayer.Vehicle is Vehicle veh)
                                    Scripts.Sync.Vehicles.SetColshapeVehicleMaxSpeed(veh, maxSpeed);
                            }
                        }
                    },
                    {
                        false, (cs) =>
                        {
                            Player.LocalPlayer.ResetData("ColshapeVehicleSpeedLimited");

                            if (Player.LocalPlayer.Vehicle is Vehicle veh)
                                Scripts.Sync.Vehicles.SetColshapeVehicleMaxSpeed(veh, float.MinValue);
                        }
                    },
                }
            },
        };
    }
}