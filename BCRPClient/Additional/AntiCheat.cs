using Newtonsoft.Json.Linq;
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
        #endregion

        public AntiCheat()
        {
            AllowTP = new Stack<bool>();
            AllowHP = new Stack<bool>();
            AllowArm = new Stack<bool>();
            AllowAlpha = new Stack<bool>();
            AllowWeapon = new Stack<bool>();

            AllowTP.Push(false);
            AllowHP.Push(false);
            AllowArm.Push(false);
            AllowAlpha.Push(false);
            AllowWeapon.Push(false);

            #region Events
            #region Teleport
            Events.Add("AC::State::TP", async (object[] args) =>
            {
                var dim = (int)args[0] < 0 ? Player.LocalPlayer.Dimension : (uint)(int)args[0];

                if (args.Length == 1)
                {
                    Sync.Players.CloseAll(false);

                    Utils.ResetGameplayCameraRotation();

                    Events.OnPlayerSpawn?.Invoke(null);

                    return;
                }

                LastAllowedPos = (Vector3)args[1] ?? Player.LocalPlayer.Position;

                var onGround = (bool)args[2];

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

                Sync.Players.CloseAll(false);

                if (fade)
                {
                    Additional.SkyCamera.FadeScreen(true, 500, -1);

                    await RAGE.Game.Invoker.WaitAsync(1000);

                    Additional.SkyCamera.FadeScreen(false, 1500, -1);
                }

                AllowTP.Push(true);

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
                    }
                }
                else
                {
                    Player.LocalPlayer.ClearTasksImmediately();

                    Player.LocalPlayer.FreezePosition(false);

                    Player.LocalPlayer.Position = LastAllowedPos;

                    Player.LocalPlayer.SetHeading(heading);

                    if (onGround)
                    {
                        RAGE.Game.Player.StartPlayerTeleport(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, heading, false, onGround, true);
                    }
                }

                GameEvents.DisableAllControls(false);
                KeyBinds.EnableAll();

                Utils.ResetGameplayCameraRotation();

                Events.OnPlayerSpawn?.Invoke(null);

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

                GameEvents.Render -= InvincibleRender;

                if (value)
                    GameEvents.Render += InvincibleRender;
            });

            #region Weapon
            Events.Add("AC::State::Weapon", async (object[] args) =>
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

                    LastAllowedWeapon = args[1].ToUInt32();

                    Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);
                }

                Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);

                AllowWeapon.Push(true);

                if (Sync.WeaponSystem.UnarmedHash != LastAllowedWeapon)
                    Sync.Phone.DestroyLocalPhone();

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

                AllowWeapon.Pop();

                if (AllowWeapon.Count == 0)
                    AllowWeapon.Push(false);
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

            Update += Check;

            Loop = new AsyncTask(() => Update?.Invoke(), 1000, true);
            Loop.Run();
        }

        private static void Check()
        {
            if (Player.LocalPlayer.Vehicle != null && Player.LocalPlayer.Vehicle.IsLocal)
            {
                if (Player.LocalPlayer.Dimension == Settings.MAIN_DIMENSION)
                {
                    Player.LocalPlayer.Vehicle.Destroy();
                }
            }

            #region Teleport
            if (!AllowTP.Peek())
            {
                var diff = Vector3.Distance(Player.LocalPlayer.Position, LastPosition);

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

                if (!vData.IsInvincible)
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
                }

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

                if (vData.FrozenPosition is string posStr && (Player.LocalPlayer.Vehicle != veh || AllowTP.Peek()))
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
                                    Events.CallRemoteUnreliable("votc", veh, trailerVeh);
                            }
                        }
                        else
                        {
                            if (vData.IsAttachedToVehicle == null)
                                Events.CallRemoteUnreliable("votc", veh, trailerVeh);
                        }
                    }
                }
                else
                {
                    var actualAttach = vData.IsAttachedToVehicle;

                    if (actualAttach != null && (actualAttach.Type == Sync.AttachSystem.Types.VehicleTrailerObjBoat))
                    {
                        Events.CallRemoteUnreliable("votc", veh, null);
                    }
                }
            }

            RAGE.Game.Player.RestorePlayerStamina(1000f);
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0f);
        }

        private static void InvincibleRender() => RAGE.Game.Player.SetPlayerInvincible(true);
    }
}
