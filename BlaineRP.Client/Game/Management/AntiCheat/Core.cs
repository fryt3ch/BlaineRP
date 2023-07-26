using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management.Misc;
using BlaineRP.Client.Game.Management.Weapons;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.AntiCheat
{
    [Script(int.MaxValue)]
    public partial class Core
    {
        public const string TeleportTaskKey = "AC_TP_T";
        public const string TeleportGroundTaskKey = "AC_TPG_T";

        public const string HealthTaskKey = "AC_HP_T";
        public const string ArmourTaskKey = "AC_ARM_T";

        public const string WeaponTaskKey = "AC_WEAPON_T";

        public const string AlphaTaskKey = "AC_ALPHA_T";

        public Core()
        {
            Events.Add("AC::Ped::TP",
                async (args) =>
                {
                    var remoteId = Convert.ToUInt16(args[0]);

                    Ped ped = Entities.Peds.GetAtRemote(remoteId);

                    if (ped == null || ped.Controller != Player.LocalPlayer)
                        return;

                    var pos = (Vector3)args[1];
                    float? heading = args[2] == null ? (float?)null : Convert.ToSingle(args[2]);

                    if (pos != null)
                        ped.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);

                    if (heading is float headingF)
                        ped.SetHeading(headingF);
                }
            );

            Events.Add("AC::State::TP",
                async (object[] args) =>
                {
                    AsyncTask.Methods.CancelPendingTask(TeleportTaskKey);
                    AsyncTask.Methods.CancelPendingTask(TeleportGroundTaskKey);

                    if (args.Length == 1)
                    {
                        Players.CloseAll(false);

                        Events.OnPlayerSpawn?.Invoke(null);

                        return;
                    }

                    AsyncTask task = null;

                    LastAllowedPos = (Vector3)args[1] ?? Player.LocalPlayer.GetCoords(false);

                    var onGround = (bool)args[2];

                    LastTeleportWasGround = onGround;

                    float? heading = args[3] == null ? (float?)null : Convert.ToSingle(args[3]);

                    var fade = (bool)args[4];

                    bool withVehicle = args.Length > 5;

                    Main.DisableAllControls(true);
                    Input.Core.DisableAll();

                    Player.LocalPlayer.Detach(false, false);

                    Player.LocalPlayer.FreezePosition(true);

                    Vehicle veh = Player.LocalPlayer.Vehicle;

                    if (withVehicle && veh != null)
                    {
                        veh.Detach(false, false);

                        var vData = VehicleData.GetData(veh);

                        if (vData != null)
                            if (!vData.IsFrozen)
                                veh.FreezePosition(true);
                    }

                    if (DisableCloseAllOnTeleportCounter > 0)
                        DisableCloseAllOnTeleportCounter--;
                    else
                        Players.CloseAll(false);

                    task = new AsyncTask(async () =>
                        {
                            if (fade)
                            {
                                SkyCamera.FadeScreen(true, 500, -1);

                                await RAGE.Game.Invoker.WaitAsync(1000);

                                SkyCamera.FadeScreen(false, 1500, -1);
                            }

                            if (withVehicle && veh != null)
                            {
                                var vData = VehicleData.GetData(veh);

                                if (vData != null)
                                {
                                    Player.LocalPlayer.FreezePosition(false);

                                    veh.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                                    if (heading is float headingF)
                                    {
                                        veh.SetHeading(headingF);
                                        Utils.Game.Camera.ResetGameplayCameraRotation();
                                    }

                                    if (!vData.IsFrozen)
                                        veh.FreezePosition(false);

                                    AsyncTask.Methods.Run(() =>
                                        {
                                            Attachments.Core.ReattachObjects(veh);
                                        },
                                        500
                                    );

                                    if (onGround)
                                    {
                                        var coordZ = 0f;

                                        AsyncTask task = null;

                                        task = new AsyncTask(async () =>
                                            {
                                                for (float coordZr = LastAllowedPos.Z; coordZr <= 1000f;)
                                                {
                                                    if (veh?.Exists != true || Player.LocalPlayer.Vehicle != veh)
                                                        break;

                                                    if (!AsyncTask.Methods.IsTaskStillPending(TeleportGroundTaskKey, task))
                                                        return;

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

                                                AsyncTask.Methods.CancelPendingTask(TeleportGroundTaskKey);
                                            },
                                            0,
                                            false,
                                            0
                                        );

                                        AsyncTask.Methods.SetAsPending(task, TeleportGroundTaskKey);
                                    }
                                }
                            }
                            else
                            {
                                Player.LocalPlayer.ClearTasksImmediately();

                                if (Players.CharacterLoaded)
                                    Player.LocalPlayer.FreezePosition(false);

                                Player.LocalPlayer.SetCoordsNoOffset(LastAllowedPos.X, LastAllowedPos.Y, LastAllowedPos.Z, false, false, false);

                                if (heading is float headingF)
                                {
                                    Player.LocalPlayer.SetHeading(headingF);
                                    Utils.Game.Camera.ResetGameplayCameraRotation();
                                }

                                if (onGround)
                                {
                                    var coordZ = 0f;

                                    float coordZr = LastAllowedPos.Z;

                                    AsyncTask task = null;

                                    task = new AsyncTask(async () =>
                                        {
                                            for (float coordZr = LastAllowedPos.Z; coordZr <= 1000f;)
                                            {
                                                if (!AsyncTask.Methods.IsTaskStillPending(TeleportGroundTaskKey, task))
                                                    return;

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

                                            AsyncTask.Methods.CancelPendingTask(TeleportGroundTaskKey);
                                        },
                                        0,
                                        false,
                                        0
                                    );

                                    AsyncTask.Methods.SetAsPending(task, TeleportGroundTaskKey);
                                }
                            }

                            Main.DisableAllControls(false);
                            Input.Core.EnableAll();

                            Events.OnPlayerSpawn?.Invoke(null);

                            await RAGE.Game.Invoker.WaitAsync(2000);

                            if (!AsyncTask.Methods.IsTaskStillPending(TeleportTaskKey, task))
                                return;

                            AsyncTask.Methods.CancelPendingTask(TeleportTaskKey);
                        },
                        0,
                        false,
                        0
                    );

                    AsyncTask.Methods.SetAsPending(task, TeleportTaskKey);
                }
            );

            Events.Add("AC::State::HP",
                async (object[] args) =>
                {
                    AsyncTask.Methods.CancelPendingTask(HealthTaskKey);

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                        {
                            var value = (int)args[0];

                            LastAllowedHP = value;
                            Player.LocalPlayer.SetRealHealth(value);

                            await RAGE.Game.Invoker.WaitAsync(2000);

                            if (!AsyncTask.Methods.IsTaskStillPending(HealthTaskKey, task))
                                return;

                            AsyncTask.Methods.CancelPendingTask(HealthTaskKey);
                        },
                        0,
                        false,
                        0
                    );

                    AsyncTask.Methods.SetAsPending(task, HealthTaskKey);
                }
            );

            Events.Add("AC::State::Arm",
                async (object[] args) =>
                {
                    AsyncTask.Methods.CancelPendingTask(ArmourTaskKey);

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                        {
                            var value = (int)args[0];

                            Weapons.Core.OnDamage -= Weapons.Core.ArmourCheck;

                            LastAllowedArm = value;
                            Player.LocalPlayer.SetArmour(value);

                            await RAGE.Game.Invoker.WaitAsync(100);

                            Weapons.Core.OnDamage -= Weapons.Core.ArmourCheck;
                            Weapons.Core.OnDamage += Weapons.Core.ArmourCheck;

                            if (!AsyncTask.Methods.IsTaskStillPending(ArmourTaskKey, task))
                                return;

                            await RAGE.Game.Invoker.WaitAsync(1900);

                            if (!AsyncTask.Methods.IsTaskStillPending(ArmourTaskKey, task))
                                return;

                            AsyncTask.Methods.CancelPendingTask(ArmourTaskKey);
                        }
                    );

                    AsyncTask.Methods.SetAsPending(task, ArmourTaskKey);
                }
            );

            Events.Add("AC::State::Alpha",
                async (object[] args) =>
                {
                    AsyncTask.Methods.CancelPendingTask(AlphaTaskKey);

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                        {
                            var value = (int)args[0];

                            LastAllowedAlpha = value;

                            Player.LocalPlayer.SetAlpha(value, false);

                            await RAGE.Game.Invoker.WaitAsync(2000);

                            if (!AsyncTask.Methods.IsTaskStillPending(AlphaTaskKey, task))
                                return;

                            AsyncTask.Methods.CancelPendingTask(AlphaTaskKey);
                        }
                    );

                    AsyncTask.Methods.SetAsPending(task, AlphaTaskKey);
                }
            );

            Events.Add("AC::State::Invincible",
                (object[] args) =>
                {
                    var value = (bool)args[0];

                    LastAllowedInvincible = value;

                    Player.LocalPlayer.SetInvincible(value);
                    Player.LocalPlayer.SetCanBeDamaged(!value);

                    Main.Render -= InvincibleRender;

                    if (value)
                        Main.Render += InvincibleRender;
                }
            );

            Events.Add("AC::State::Weapon",
                async (object[] args) =>
                {
                    AsyncTask.Methods.CancelPendingTask(WeaponTaskKey);

                    AsyncTask task = null;

                    task = new AsyncTask(async () =>
                        {
                            LastAllowedAmmo = (int)args[0];

                            if (args.Length > 1)
                            {
                                WeaponInfo curGunData = Weapons.Core.WeaponList.Where(x => x.Hash == LastAllowedWeapon).FirstOrDefault();

                                if (curGunData != null)
                                    if (curGunData.ComponentsHashes != null)
                                        foreach (uint x in curGunData.ComponentsHashes.Values)
                                        {
                                            if (Player.LocalPlayer.HasGotWeaponComponent(LastAllowedWeapon, x))
                                                Player.LocalPlayer.RemoveWeaponComponentFrom(LastAllowedWeapon, x);
                                        }

                                LastAllowedWeapon = Convert.ToUInt32(args[1]);

                                Player.LocalPlayer.SetCurrentWeapon(LastAllowedWeapon, true);
                            }

                            Player.LocalPlayer.SetAmmo(LastAllowedWeapon, LastAllowedAmmo, 1);

                            if (Weapons.Core.WeaponList.Where(x => x.Hash == LastAllowedWeapon && x.HasAmmo).FirstOrDefault() != null)
                            {
                                HUD.SetAmmo(LastAllowedAmmo);

                                HUD.SwitchAmmo(true);

                                Main.Update -= Weapons.Core.UpdateWeapon;
                                Main.Update += Weapons.Core.UpdateWeapon;
                            }
                            else
                            {
                                HUD.SwitchAmmo(false);

                                Main.Update -= Weapons.Core.UpdateWeapon;
                            }

                            await RAGE.Game.Invoker.WaitAsync(2000);

                            if (!AsyncTask.Methods.IsTaskStillPending(WeaponTaskKey, task))
                                return;

                            AsyncTask.Methods.CancelPendingTask(WeaponTaskKey);
                        }
                    );

                    AsyncTask.Methods.SetAsPending(task, WeaponTaskKey);
                }
            );
        }

        public static bool LastTeleportWasGround { get; set; }

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

        private static uint DisableCloseAllOnTeleportCounter { get; set; }

        public static void DisableCloseAllOnNextTeleport()
        {
            DisableCloseAllOnTeleportCounter++;
        }

        public static void Enable()
        {
            Player player = Player.LocalPlayer;

            LastPosition = player.GetCoords(false);
            LastArmour = player.GetArmour();
            LastHealth = player.GetRealHealth();

            LastAllowedPos = new Vector3(LastPosition.X, LastPosition.Y, LastPosition.Z);
            LastAllowedHP = player.GetRealHealth();
            LastAllowedArm = player.GetArmour();
            LastAllowedAlpha = 255;
            LastAllowedWeapon = Weapons.Core.UnarmedHash;
            LastAllowedAmmo = 0;

            LastAllowedInvincible = false;

            var task = new AsyncTask(() =>
                {
                    Update();
                },
                1_000,
                true,
                0
            );

            task.Run();
        }

        private static void InvincibleRender()
        {
            RAGE.Game.Player.SetPlayerInvincible(true);
        }

        private static void AntiAltF4Vehicle()
        {
            if (Player.LocalPlayer.IsInAnyVehicle(false))
                if (RAGE.Game.Ui.IsWarningMessageActive() && Utils.Game.Misc.GetWarningMessageTitleHash() == 1246147334)
                {
                    Player.LocalPlayer.ClearTasksImmediately();

                    Main.CloseGameNow("BETTER YOU DON'T ALT+F4 WHILE USING VEHICLE, MY FRIEND!");
                }
        }
    }
}