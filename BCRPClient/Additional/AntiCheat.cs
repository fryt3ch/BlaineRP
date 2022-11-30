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
        private static GameEvents.UpdateHandler Update { get; set; }
        private static AsyncTask Loop { get; set; }

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
            Events.Add("AC::State::TP", async (object[] args) =>
            {
                var pos = (Vector3)args[0];

                var heading = args[2] is float fHeading ? fHeading : Player.LocalPlayer.GetHeading();

                var onGround = (bool)args[1];

                var fade = (bool)args[3];

                Data.NPC.CurrentNPC?.SwitchDialogue(false);

                if (fade)
                {
                    Additional.SkyCamera.FadeScreen(true, 500, -1);

                    await RAGE.Game.Invoker.WaitAsync(500);

                    Additional.SkyCamera.FadeScreen(false, 1500, -1);
                }

                LastAllowedPos = pos;

                AllowTP.Push(true);

                if (Player.LocalPlayer.Vehicle == null)
                {
                    Player.LocalPlayer.Position = pos;

                    Player.LocalPlayer.SetHeading(heading);

                    if (onGround)
                    {
                        RAGE.Game.Player.StartPlayerTeleport(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, heading, false, onGround, true);
                    }
                }
                else
                {
                    Player.LocalPlayer.Vehicle.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                    Player.LocalPlayer.Vehicle.SetHeading(heading);
                }

                Utils.ResetGameplayCameraRotation();

                await RAGE.Game.Invoker.WaitAsync(2000);

                AllowTP.Pop();

                if (AllowTP.Count == 0)
                    AllowTP.Push(false);
            });
            #endregion

            #region Health
            Events.Add("AC::State::HP", async (object[] args) =>
            {
                var value = (int)args[0];

                LastAllowedHP = value;
                Player.LocalPlayer.SetRealHealth(value);

                AllowHP.Push(true);

                await RAGE.Game.Invoker.WaitAsync(2000);

                AllowHP.Pop();

                if (AllowHP.Count == 0)
                    AllowHP.Push(false);
            });
            #endregion

            #region Armour
            Events.Add("AC::State::Arm", async (object[] args) =>
            {
                var value = (int)args[0];

                Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;

                LastAllowedArm = value;
                Player.LocalPlayer.SetArmour(value);

                AllowArm.Push(true);

                await RAGE.Game.Invoker.WaitAsync(100);

                Sync.WeaponSystem.OnDamage -= Sync.WeaponSystem.ArmourCheck;
                Sync.WeaponSystem.OnDamage += Sync.WeaponSystem.ArmourCheck;

                await RAGE.Game.Invoker.WaitAsync(1900);

                AllowArm.Pop();

                if (AllowArm.Count == 0)
                    AllowArm.Push(false);
            });
            #endregion

            #region Transparency
            Events.Add("AC::State::Alpha", async (object[] args) =>
            {
                var value = (int)args[0];

                LastAllowedAlpha = value;

                Player.LocalPlayer.SetAlpha(value, false);

                AllowAlpha.Push(true);

                await RAGE.Game.Invoker.WaitAsync(2000);

                AllowAlpha.Pop();

                if (AllowAlpha.Count == 0)
                    AllowAlpha.Push(false);
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
            Events.Add("AC::State::Weapon", async (object[] args) =>
            {
                LastAllowedAmmo = (int)args[0];

                if (args.Length > 1)
                {
                    LastAllowedWeapon = ((int)args[1]).ToUInt32();
                }

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

                await RAGE.Game.Invoker.WaitAsync(2000);

                AllowWeapon.Pop();

                if (AllowWeapon.Count == 0)
                    AllowWeapon.Push(false);
            });
            #endregion
            #endregion

            Events.Add("AC::Vehicle::Health", async (object[] args) =>
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

                await RAGE.Game.Invoker.WaitAsync(2000);

                AllowVehicleHealth.Remove((vehicle, true));
            });
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

            Update += Check;

            Loop = new AsyncTask(() => Update?.Invoke(), 1000, true);
            Loop.Run();
        }

        private static void Check()
        {
            #region Teleport
            if (!AllowTP.Peek())
            {
                var diff = Vector3.Distance(Player.LocalPlayer.Position, LastPosition);

                if ((Player.LocalPlayer.Vehicle == null && Player.LocalPlayer.IsRagdoll() && Player.LocalPlayer.IsInAir()) || (Player.LocalPlayer.Vehicle != null))
                    diff = Math.Abs(diff - Player.LocalPlayer.GetSpeed());

                if (diff >= 50f)
                {
                    Events.CallRemote("AC::Detect::TP", diff);

                    Utils.ConsoleOutput("ASDAS");
                }
            }
            else
            {
                if (Vector3.Distance(Player.LocalPlayer.Position, LastAllowedPos) >= 50f)
                    Player.LocalPlayer.Position = LastAllowedPos;
            }

            LastPosition = Player.LocalPlayer.Position;

            #endregion

            #region Health
            if (!AllowHP.Peek() && !LastAllowedInvincible)
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
