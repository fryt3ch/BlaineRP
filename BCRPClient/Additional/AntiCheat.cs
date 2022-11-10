using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Additional
{
    /*
        Anticheat System for RAGE MP by frytech
     */

    class AntiCheat : Events.Script
    {
        private static GameEvents.UpdateHandler Update;
        private static AsyncTask Loop;

        #region Variables
        private static Vector3 LastPosition;
        private static int LastHealth;
        private static int LastArmour;

        private static Vector3 LastAllowedPos;
        private static uint LastAllowedDimension;
        private static int LastAllowedHP;
        private static int LastAllowedArm;
        public static int LastAllowedAlpha;
        public static bool LastAllowedInvincible { get; private set; }

        private static uint LastAllowedWeapon;
        public static int LastAllowedAmmo;

        private static Stack<bool> AllowTP;
        private static Stack<bool> AllowHP;
        private static Stack<bool> AllowArm;
        private static Stack<bool> AllowAlpha;
        private static Stack<bool> AllowWeapon;

        private static List<(Vehicle, bool)> AllowVehicleHealth;
        #endregion

        public AntiCheat()
        {
            AllowTP = new Stack<bool>();
            AllowHP = new Stack<bool>();
            AllowArm = new Stack<bool>();
            AllowAlpha = new Stack<bool>();
            AllowWeapon = new Stack<bool>();

            AllowVehicleHealth = new List<(Vehicle, bool)>();

            AllowTP.Push(false);
            AllowHP.Push(false);
            AllowArm.Push(false);
            AllowAlpha.Push(false);
            AllowWeapon.Push(false);

            #region Events
            #region Teleport
            Events.Add("AC::State::TP", (object[] args) =>
            {
                LastAllowedPos = (Vector3)args[0] ?? Player.LocalPlayer.Position;
                var onGround = (bool)args[1];
                LastAllowedDimension = RAGE.Util.Json.Deserialize<uint>((string)args[2]);

                Player.LocalPlayer.Position = LastAllowedPos;

                if (LastAllowedPos.DistanceTo(Player.LocalPlayer.Position) > 0)
                    RAGE.Game.Player.StartPlayerTeleport(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, Player.LocalPlayer.GetHeading(), false, onGround, true);

                Player.LocalPlayer.Dimension = LastAllowedDimension;

                AllowTP.Push(true);

                /*                int counter = 0;

                                if (onGround)
                                {
                                    // To wait some time before location is streamed
                                    (new AsyncTask(() =>
                                    {
                                        float z = LastAllowedPos.Z;

                                        RAGE.Game.Misc.GetGroundZFor3dCoord(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, ref z, false);

                                        counter++;

                                        if (z != 0 && z != 20)
                                        {
                                            LastAllowedPos = new Vector3(LastAllowedPos.X, LastAllowedPos.Y, z);

                                            Player.LocalPlayer.Position = LastAllowedPos;

                                            return true;
                                        }

                                        if (counter > 10)
                                            return true;

                                        return false;

                                    }, 250, true)).Run();
                                }*/

                (new AsyncTask(() =>
                {
                    AllowTP.Pop();

                    if (AllowTP.Count == 0)
                        AllowTP.Push(false);
                }, 2000, false)).Run();
            });
            #endregion

            #region Health
            Events.Add("AC::State::HP", (object[] args) =>
            {
                var value = (int)args[0];

                LastAllowedHP = value;
                Player.LocalPlayer.SetRealHealth(value);

                AllowHP.Push(true);

                (new AsyncTask(() =>
                {
                    AllowHP.Pop();

                    if (AllowHP.Count == 0)
                        AllowHP.Push(false);
                }, 2000, false)).Run();
            });
            #endregion

            #region Armour
            Events.Add("AC::State::Arm", (object[] args) =>
            {
                var value = (int)args[0];

                Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;

                LastAllowedArm = value;
                Player.LocalPlayer.SetArmour(value);

                AllowArm.Push(true);

                (new AsyncTask(() =>
                {
                    Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;
                    Sync.WeaponSystem.OnDamage += Sync.WeaponSystem.ArmourCheck;
                }, 100, false)).Run();

                (new AsyncTask(() =>
                {
                    AllowArm.Pop();

                    if (AllowArm.Count == 0)
                        AllowArm.Push(false);
                }, 2000, false)).Run();
            });
            #endregion

            #region Transparency
            Events.Add("AC::State::Alpha", (object[] args) =>
            {
                var value = (int)args[0];

                LastAllowedAlpha = value;

                Player.LocalPlayer.SetAlpha(value, false);

                AllowAlpha.Push(true);

                (new AsyncTask(() =>
                {
                    AllowAlpha.Pop();

                    if (AllowAlpha.Count == 0)
                        AllowAlpha.Push(false);
                }, 2000, false)).Run();
            });
            #endregion

            Events.Add("AC::State::Invincible", (object[] args) =>
            {
                var value = (bool)args[0];

                LastAllowedInvincible = value;

                Player.LocalPlayer.SetInvincible(value);
                Player.LocalPlayer.SetCanBeDamaged(!value);
            });

            #region Weapon
            Events.Add("AC::State::Weapon", (object[] args) =>
            {
                if ((string)args[0] != "-1")
                    LastAllowedWeapon = RAGE.Util.Json.Deserialize<uint>((string)args[0]);

                LastAllowedAmmo = (int)args[1];

                Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);

                Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);

                AllowWeapon.Push(true);

                Sync.Phone.Off();

                if (Sync.WeaponSystem.WeaponList.Where(x => x.Hash == LastAllowedWeapon && x.HasAmmo).FirstOrDefault() != null)
                {
                    KeyBinds.Binds[KeyBinds.Types.ReloadWeapon].Enable();

                    CEF.HUD.SetAmmo(LastAllowedAmmo);

                    CEF.HUD.SwitchAmmo(true);

                    GameEvents.Update -= Sync.WeaponSystem.UpdateWeapon;
                    GameEvents.Update += Sync.WeaponSystem.UpdateWeapon;
                }
                else
                {
                    KeyBinds.Binds[KeyBinds.Types.ReloadWeapon].Disable();

                    CEF.HUD.SwitchAmmo(false);

                    GameEvents.Update -= Sync.WeaponSystem.UpdateWeapon;
                }

                (new AsyncTask(() =>
                {
                    AllowWeapon.Pop();

                    if (AllowWeapon.Count == 0)
                        AllowWeapon.Push(false);
                }, 2000, false)).Run();
            });
            #endregion
            #endregion

            Events.Add("AC::Vehicle::Health", (object[] args) =>
            {
                var vehicle = (Vehicle)args[0];
                var value = (float)args[1];

                var data = Sync.Vehicles.GetData(vehicle);

                if (data == null)
                    return;

                data.LastAllowedHealth = value;

                if (value >= 1000)
                {
                    vehicle.SetFixed();
                    vehicle.SetDeformationFixed();

                    vehicle.SetEngineHealth(1000);
                }
                else
                {
                    vehicle.SetEngineHealth(value);
                }

                AllowVehicleHealth.Add((vehicle, true));

                (new AsyncTask(() =>
                {
                    AllowVehicleHealth.Remove((vehicle, true));
                }, 2000, false)).Run();
            });
        }

        public static void Enable()
        {
            var player = Player.LocalPlayer;

            Sync.Players.PlayerData data = Sync.Players.GetData(player);

            LastPosition = player.Position;
            LastArmour = player.GetArmour();
            LastHealth = player.GetRealHealth();

            LastAllowedPos = player.Position;
            LastAllowedHP = player.GetRealHealth();
            LastAllowedArm = player.GetArmour();
            LastAllowedDimension = player.Dimension;
            LastAllowedAlpha = 255;
            LastAllowedWeapon = Sync.WeaponSystem.UnarmedHash;
            LastAllowedAmmo = 0;

            LastAllowedInvincible = false;

            Update += Check;

            Loop = new AsyncTask(() => Update?.Invoke(), 1000, true);
            Loop.Run();
        }

        public static void Check()
        {
            #region Teleport
            if (!AllowTP.Peek())
            {
                var diff = Vector3.Distance(Player.LocalPlayer.Position, LastPosition);

                if ((Player.LocalPlayer.Vehicle == null && Player.LocalPlayer.IsRagdoll() && Player.LocalPlayer.IsInAir()) || (Player.LocalPlayer.Vehicle != null))
                    diff = Math.Abs(diff - Player.LocalPlayer.GetSpeed());

                if (diff >= 50f)
                    Events.CallRemote("AC::Detect::TP", diff);

                if (Player.LocalPlayer.Dimension != LastAllowedDimension)
                    Player.LocalPlayer.Dimension = LastAllowedDimension;
            }
            else
            {
                if (Vector3.Distance(Player.LocalPlayer.Position, LastAllowedPos) >= 50f)
                    Player.LocalPlayer.Position = LastAllowedPos;

                if (Player.LocalPlayer.Dimension != LastAllowedDimension)
                    Player.LocalPlayer.Dimension = LastAllowedDimension;
            }

            LastPosition = Player.LocalPlayer.Position;

            #endregion

            #region Health
            if (!AllowHP.Peek())
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
            if (!AllowArm.Peek())
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
            if (!AllowAlpha.Peek())
            {
                if (Player.LocalPlayer.GetAlpha() != LastAllowedAlpha)
                    Player.LocalPlayer.SetAlpha(LastAllowedAlpha, false);
            }
            #endregion

            #region Weapon
            if (!AllowWeapon.Peek())
            {
                var curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Sync.WeaponSystem.MobileHash)
                {
                    //Player.LocalPlayer.RemoveAllWeapons(true);

                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);
                }

                if (Player.LocalPlayer.GetAmmoInWeapon(curWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(curWeapon, 1, 1);
            }
            else
            {
                var curWeapon = Player.LocalPlayer.GetSelectedWeapon();

                if (curWeapon != LastAllowedWeapon && curWeapon != Sync.WeaponSystem.MobileHash)
                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);

                if (Player.LocalPlayer.GetAmmoInWeapon(LastAllowedWeapon) > LastAllowedAmmo)
                    Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);
            }
            #endregion

            for (int i = 0; i < Sync.Vehicles.ControlledVehicles.Count; i++)
            {
                var veh = Sync.Vehicles.ControlledVehicles[i];

                var data = Sync.Vehicles.GetData(veh);

                if (data == null)
                    continue;

                if (!AllowVehicleHealth.Contains((veh, true)))
                {
                    var diff = veh.GetEngineHealth() - data.LastHealth;

                    if (diff > 0)
                        veh.SetEngineHealth(data.LastHealth);
                }
                else if (veh.GetEngineHealth() > data.LastAllowedHealth)
                    veh.SetEngineHealth(data.LastAllowedHealth);

                data.LastHealth = veh.GetEngineHealth();
            }

            RAGE.Game.Player.RestorePlayerStamina(100);
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0);
        }
    }
}
