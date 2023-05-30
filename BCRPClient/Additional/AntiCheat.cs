using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Additional
{
    /*
        Anticheat System for RAGE MP by frytech
     */

    class AntiCheat : Events.Script
    {
        private static Additional.ExtraTimer Timer { get; set; }

        public static bool LastTeleportWasGround { get; set; }

        private static AsyncTask GroundTeleportTask { get; set; }

        #region Variables
        public static Vector3 LastPosition { get; set; }

        private static int LastHealth { get; set; }

        private static int LastArmour { get; set; }

        private static Vector3 LastAllowedPos { get; set; }

        private static int LastAllowedHP { get; set; }

        private static int LastAllowedArm { get; set; }

        private static int LastAllowedAlpha { get; set; }

        private static bool LastAllowedInvincible { get; set; }

        private static uint LastAllowedWeapon { get; set; }

        public static int LastAllowedAmmo { get; set; }

        public const string TeleportTaskKey = "AC_TP_T";

        public const string HealthTaskKey = "AC_HP_T";
        public const string ArmourTaskKey = "AC_ARM_T";

        public const string WeaponTaskKey = "AC_WEAPON_T";

        public const string AlphaTaskKey = "AC_ALPHA_T";

        private static uint DisableCloseAllOnTeleportCounter { get; set; }
        #endregion

        public static void DisableCloseAllOnNextTeleport() => DisableCloseAllOnTeleportCounter++;

        public AntiCheat()
        {
            #region Events
            #region Teleport

            Events.Add("AC::Ped::TP", async (args) =>
            {
                var remoteId = Utils.ToUInt16(args[0]);

                var ped = RAGE.Elements.Entities.Peds.GetAtRemote(remoteId);

                if (ped == null || ped.Controller != Player.LocalPlayer)
                    return;

                var pos = (Vector3)args[1];
                var heading = args[2] == null ? (float?)null : Utils.ToSingle(args[2]);

                if (pos != null)
                {
                    ped.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);
                }

                if (heading is float headingF)
                {
                    ped.SetHeading(headingF);
                }
            });

            Events.Add("AC::State::TP", async (object[] args) =>
            {
                var dim = (int)args[0] < 0 ? Player.LocalPlayer.Dimension : Utils.ToUInt32(args[0]);

                if (args.Length == 1)
                {
                    Sync.Players.CloseAll(false);

                    Utils.ResetGameplayCameraRotation();

                    Events.OnPlayerSpawn?.Invoke(null);

                    return;
                }

                Utils.CancelPendingTask(TeleportTaskKey);

                AsyncTask task = null;

                if (GroundTeleportTask != null)
                {
                    GroundTeleportTask.Cancel();

                    GroundTeleportTask = null;
                }

                LastAllowedPos = (Vector3)args[1] ?? Player.LocalPlayer.GetCoords(false);

                var onGround = (bool)args[2];

                LastTeleportWasGround = onGround;

                var heading = args[3] is float fHeading ? fHeading : Player.LocalPlayer.GetHeading();

                var fade = (bool)args[4];

                var withVehicle = args.Length > 5;

                GameEvents.DisableAllControls(true);
                KeyBinds.DisableAll();

                Player.LocalPlayer.Detach(false, false);

                Player.LocalPlayer.FreezePosition(true);

                var veh = Player.LocalPlayer.Vehicle;

                if (withVehicle && veh != null)
                {
                    veh.Detach(false, false);

                    var vData = Sync.Vehicles.GetData(veh);

                    if (vData != null)
                    {
                        if (!vData.IsFrozen)
                            veh.FreezePosition(true);
                    }
                }

                if (DisableCloseAllOnTeleportCounter > 0)
                    DisableCloseAllOnTeleportCounter--;
                else
                    Sync.Players.CloseAll(false);

                task = new AsyncTask(async () =>
                {
                    if (fade)
                    {
                        Additional.SkyCamera.FadeScreen(true, 500, -1);

                        await RAGE.Game.Invoker.WaitAsync(1000);

                        Additional.SkyCamera.FadeScreen(false, 1500, -1);
                    }

                    if (withVehicle && veh != null)
                    {
                        var vData = Sync.Vehicles.GetData(veh);

                        if (vData != null)
                        {
                            Player.LocalPlayer.FreezePosition(false);

                            veh.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                            veh.SetHeading(heading);

                            if (!vData.IsFrozen)
                                veh.FreezePosition(false);

                            AsyncTask.RunSlim(() =>
                            {
                                Sync.AttachSystem.ReattachObjects(veh);
                            }, 500);

                            if (onGround)
                            {
                                var coordZ = 0f;

                                GroundTeleportTask = new AsyncTask(async () =>
                                {
                                    for (float coordZr = LastAllowedPos.Z; coordZr <= 1000f;)
                                    {
                                        if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                            break;

                                        if (RAGE.Game.Misc.GetGroundZFor3dCoord(LastAllowedPos.X, LastAllowedPos.Y, coordZr, ref coordZ, true))
                                        {
                                            veh.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, coordZ + 1f, false, false, false);

                                            return;
                                        }
                                        else
                                        {
                                            veh.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, coordZr += 25f, false, false, false);

                                            await RAGE.Game.Invoker.WaitAsync(5);
                                        }
                                    }

                                    veh.Position = LastAllowedPos;
                                }, 0, false, 0);

                                GroundTeleportTask.Run();
                            }
                        }
                    }
                    else
                    {
                        Player.LocalPlayer.ClearTasksImmediately();

                        Player.LocalPlayer.FreezePosition(false);

                        Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                        Player.LocalPlayer.SetHeading(heading);

                        if (onGround)
                        {
                            var coordZ = 0f;

                            var coordZr = LastAllowedPos.Z;

                            GroundTeleportTask = new AsyncTask(async () =>
                            {
                                for (float coordZr = LastAllowedPos.Z; coordZr <= 1000f;)
                                {
                                    if (RAGE.Game.Misc.GetGroundZFor3dCoord(LastAllowedPos.X, LastAllowedPos.Y, coordZr, ref coordZ, true))
                                    {
                                        Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, coordZ + 1f, false, false, false);

                                        return;
                                    }
                                    else
                                    {
                                        Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, coordZr += 25f, false, false, false);

                                        await RAGE.Game.Invoker.WaitAsync(5);
                                    }
                                }

                                Player.LocalPlayer.Position = LastAllowedPos;
                            }, 0, false, 0);

                            GroundTeleportTask.Run();
                        }
                    }

                    GameEvents.DisableAllControls(false);
                    KeyBinds.EnableAll();

                    Utils.ResetGameplayCameraRotation();

                    Events.OnPlayerSpawn?.Invoke(null);

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    if (!Utils.IsTaskStillPending(TeleportTaskKey, task))
                        return;

                    Utils.CancelPendingTask(TeleportTaskKey);

                }, 0, false, 0);

                Utils.SetTaskAsPending(TeleportTaskKey, task);
            });
            #endregion

            #region Health
            Events.Add("AC::State::HP", async (object[] args) =>
            {
                Utils.CancelPendingTask(HealthTaskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    var value = (int)args[0];

                    LastAllowedHP = value;
                    Player.LocalPlayer.SetRealHealth(value);

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    if (!Utils.IsTaskStillPending(HealthTaskKey, task))
                        return;

                    Utils.CancelPendingTask(HealthTaskKey);
                }, 0, false, 0);

                Utils.SetTaskAsPending(HealthTaskKey, task);
            });
            #endregion

            #region Armour
            Events.Add("AC::State::Arm", async (object[] args) =>
            {
                Utils.CancelPendingTask(ArmourTaskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    var value = (int)args[0];

                    Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;

                    LastAllowedArm = value;
                    Player.LocalPlayer.SetArmour(value);

                    await RAGE.Game.Invoker.WaitAsync(100);

                    Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;
                    Sync.WeaponSystem.OnDamage += Sync.WeaponSystem.ArmourCheck;

                    if (!Utils.IsTaskStillPending(ArmourTaskKey, task))
                        return;

                    await RAGE.Game.Invoker.WaitAsync(1900);

                    if (!Utils.IsTaskStillPending(ArmourTaskKey, task))
                        return;

                    Utils.CancelPendingTask(ArmourTaskKey);
                });

                Utils.SetTaskAsPending(ArmourTaskKey, task);
            });
            #endregion

            #region Transparency
            Events.Add("AC::State::Alpha", async (object[] args) =>
            {
                Utils.CancelPendingTask(AlphaTaskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    var value = (int)args[0];

                    LastAllowedAlpha = value;

                    Player.LocalPlayer.SetAlpha(value, false);

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    if (!Utils.IsTaskStillPending(AlphaTaskKey, task))
                        return;

                    Utils.CancelPendingTask(AlphaTaskKey);
                });

                Utils.SetTaskAsPending(AlphaTaskKey, task);
            });
            #endregion

            Events.Add("AC::State::Invincible", (object[] args) =>
            {
                var value = (bool)args[0];

                LastAllowedInvincible = value;

                Player.LocalPlayer.SetInvincible(value);
                Player.LocalPlayer.SetCanBeDamaged(!value);

                GameEvents.Render -= InvincibleRender;

                if (value)
                    GameEvents.Render += InvincibleRender;
            });

            #region Weapon
            Events.Add("AC::State::Weapon", async (object[] args) =>
            {
                Utils.CancelPendingTask(WeaponTaskKey);

                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    LastAllowedAmmo = (int)args[0];

                    if (args.Length > 1)
                    {
                        var curGunData = Sync.WeaponSystem.WeaponList.Where(x => x.Hash == LastAllowedWeapon).FirstOrDefault();

                        if (curGunData != null)
                        {
                            if (curGunData.ComponentsHashes != null)
                            {
                                foreach (var x in curGunData.ComponentsHashes.Values)
                                    if (Player.LocalPlayer.HasGotWeaponComponent(LastAllowedWeapon, x))
                                        Player.LocalPlayer.RemoveWeaponComponentFrom(LastAllowedWeapon, x);
                            }
                        }

                        LastAllowedWeapon = Utils.ToUInt32(args[1]);

                        Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);
                    }

                    Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);

                    if (Sync.WeaponSystem.WeaponList.Where(x => x.Hash == LastAllowedWeapon && x.HasAmmo).FirstOrDefault() != null)
                    {
                        CEF.HUD.SetAmmo(LastAllowedAmmo);

                        CEF.HUD.SwitchAmmo(true);

                        GameEvents.Update -= Sync.WeaponSystem.UpdateWeapon;
                        GameEvents.Update += Sync.WeaponSystem.UpdateWeapon;
                    }
                    else
                    {
                        CEF.HUD.SwitchAmmo(false);

                        GameEvents.Update -= Sync.WeaponSystem.UpdateWeapon;
                    }

                    await RAGE.Game.Invoker.WaitAsync(2000);

                    if (!Utils.IsTaskStillPending(WeaponTaskKey, task))
                        return;

                    Utils.CancelPendingTask(WeaponTaskKey);
                });

                Utils.SetTaskAsPending(WeaponTaskKey, task);
            });
            #endregion
            #endregion
        }

        public static void Enable()
        {
            var player = Player.LocalPlayer;

            LastPosition = player.Position;
            LastArmour = player.GetArmour();
            LastHealth = player.GetRealHealth();

            LastAllowedPos = player.Position;
            LastAllowedHP = player.GetRealHealth();
            LastAllowedArm = player.GetArmour();
            LastAllowedAlpha = 255;
            LastAllowedWeapon = Sync.WeaponSystem.UnarmedHash;
            LastAllowedAmmo = 0;

            LastAllowedInvincible = false;

            Timer = new ExtraTimer(async (obj) =>
            {
                await RAGE.Game.Invoker.WaitAsync(0);

                Check();
            }, null, 0, 1_000);
        }

        private static void Check()
        {
            /*            if (Player.LocalPlayer.Vehicle is Vehicle fakeVeh && fakeVeh.IsLocal)
                        {
                            if (Player.LocalPlayer.Dimension == Settings.MAIN_DIMENSION)
                            {
                                fakeVeh.Destroy();
                            }
                        }*/

            var curPos = Player.LocalPlayer.GetCoords(false);

            AntiAltF4Vehicle();

            #region Teleport
            if (!Utils.IsTaskStillPending(TeleportTaskKey, null))
            {
                var diff = Vector3.Distance(curPos, LastPosition);

                if ((Player.LocalPlayer.Vehicle == null && Player.LocalPlayer.IsRagdoll() && Player.LocalPlayer.IsInAir()) || (Player.LocalPlayer.Vehicle != null))
                    diff = Math.Abs(diff - Player.LocalPlayer.GetSpeed());

                if (diff >= 50f)
                {
                    Events.CallRemote("AC::Detect::TP", diff);

                    //Utils.ConsoleOutput("ASDAS");
                }
            }
            else
            {
                if ((LastTeleportWasGround ? curPos.DistanceIgnoreZ(LastAllowedPos) : Vector3.Distance(curPos, LastAllowedPos)) >= 50f)
                {
                    Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                    curPos = LastAllowedPos;
                }
            }

            LastPosition = curPos;

            #endregion

            #region Health
            if (!Utils.IsTaskStillPending(HealthTaskKey, null) && !LastAllowedInvincible)
            {
                var diff = Player.LocalPlayer.GetRealHealth() - LastHealth;

                if (diff > 0)
                    Player.LocalPlayer.SetRealHealth(LastHealth);
            }
            else if (Player.LocalPlayer.GetRealHealth() > LastAllowedHP)
                Player.LocalPlayer.SetRealHealth(LastAllowedHP);

            LastHealth = Player.LocalPlayer.GetRealHealth();

            #endregion

            #region Armour
            if (!Utils.IsTaskStillPending(ArmourTaskKey, null))
            {
                var diff = Player.LocalPlayer.GetArmour() - LastArmour;

                if (diff > 0)
                    Player.LocalPlayer.SetArmour(0);
            }
            else if (Player.LocalPlayer.GetArmour() > LastAllowedArm)
                Player.LocalPlayer.SetArmour(LastAllowedArm);

            LastArmour = Player.LocalPlayer.GetArmour();
            #endregion

            #region Transparency
            if (!Utils.IsTaskStillPending(AlphaTaskKey, null))
            {
                if (Player.LocalPlayer.GetAlpha() != LastAllowedAlpha)
                    Player.LocalPlayer.SetAlpha(LastAllowedAlpha, false);
            }
            #endregion

            #region Weapon
            if (!Utils.IsTaskStillPending(WeaponTaskKey, null))
            {
                var curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Sync.WeaponSystem.MobileHash)
                {
                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);
                }

                if (LastAllowedAmmo >= 0 && Player.LocalPlayer.GetAmmoInWeapon(curWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(curWeapon, 0, 1);
            }
            else
            {
                var curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Sync.WeaponSystem.MobileHash)
                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);

                if (LastAllowedAmmo >= 0 && Player.LocalPlayer.GetAmmoInWeapon(LastAllowedWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);
            }
            #endregion

            for (int i = 0; i < Sync.Vehicles.ControlledVehicles.Count; i++)
            {
                var veh = Sync.Vehicles.ControlledVehicles[i];

                if (veh?.Exists != true)
                    continue;

                var vData = Sync.Vehicles.GetData(veh);

                if (vData == null)
                    continue;

                var lastHp = veh.GetData<float?>("LastHealth") ?? 1000f;
                var curHp = veh.GetEngineHealth();

                /*                if (!vData.IsInvincible)
                                {
                                    if (Player.LocalPlayer.Vehicle == veh || curHp < 0)
                                    {
                                        if (!veh.GetCanBeDamaged())
                                        {
                                            veh.SetCanBeDamaged(true);
                                            veh.SetInvincible(false);
                                        }
                                    }
                                    else
                                    {
                                        if (veh.GetCanBeDamaged())
                                        {
                                            veh.SetCanBeDamaged(false);
                                            veh.SetInvincible(true);
                                        }
                                    }
                                }*/

                if (veh.IsDead(0))
                {
                    if (curHp > -4000f)
                        veh.SetEngineHealth(-4000f);
                }
                else
                {
                    if (curHp - lastHp > 0)
                    {
                        veh.SetEngineHealth(lastHp);
                    }
                    else
                    {
                        veh.SetData("LastHealth", curHp);
                    }
                }

                if (vData.FrozenPosition is string posStr && (Player.LocalPlayer.Vehicle != veh || Utils.IsTaskStillPending(TeleportTaskKey, null)))
                {
                    var posData = posStr.Split('_');

                    var vect = new Vector3(float.Parse(posData[0]), float.Parse(posData[1]), float.Parse(posData[2]));

                    var tpVeh = veh;

                    if (vData.IsAttachedToLocalTrailer is Vehicle trVeh)
                        tpVeh = trVeh;

                    if (tpVeh.GetCoords(false).DistanceTo(vect) > 0.01f)
                        tpVeh.SetCoordsNoOffset(vect.X, vect.Y, vect.Z, false, false, false);

                    if (posData.Length > 3)
                    {
                        var heading = float.Parse(posData[3]);

                        if (tpVeh.GetHeading() != heading)
                            tpVeh.SetHeading(heading);
                    }
                }

                var trailerVehHandle = -1;

                if (veh.GetTrailerVehicle(ref trailerVehHandle))
                {
                    var trailerVeh = Utils.GetVehicleByHandle(trailerVehHandle, true);

                    if (trailerVeh?.Exists != true)
                    {
                        //veh.DetachFromTrailer();
                    }
                    else
                    {
                        if (trailerVeh.IsLocal)
                        {
                            trailerVeh = trailerVeh.GetData<Vehicle>("TrailerSync::Owner");

                            if (trailerVeh?.Exists != true)
                            {

                            }
                            else
                            {
                                if (vData.IsAttachedToVehicle == null)
                                {
                                    veh.DetachFromTrailer();

                                    Events.CallRemote("votc", veh, trailerVeh);
                                }
                            }
                        }
                        else
                        {
                            if (vData.IsAttachedToVehicle == null)
                            {
                                veh.DetachFromTrailer();

                                Events.CallRemote("votc", veh, trailerVeh);
                            }
                        }
                    }
                }
                else
                {
                    var actualAttach = vData.IsAttachedToVehicle;

                    if (actualAttach != null && (actualAttach.Type == Sync.AttachSystem.Types.VehicleTrailerObjBoat))
                    {
                        Events.CallRemote("votc", veh, null);
                    }
                }
            }

            RAGE.Game.Player.ResetPlayerStamina();
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0f);
        }

        private static void InvincibleRender() => RAGE.Game.Player.SetPlayerInvincible(true);

        private static void AntiAltF4Vehicle()
        {
            if (Player.LocalPlayer.IsInAnyVehicle(false))
            {
                if (RAGE.Game.Ui.IsWarningMessageActive() && Utils.GetWarningMessageTitleHash() == 1246147334)
                {
                    Player.LocalPlayer.ClearTasksImmediately();

                    GameEvents.CloseGameNow("BETTER YOU DON'T ALT+F4 WHILE USING VEHICLE, MY FRIEND!");
                }
            }
        }
    }
}