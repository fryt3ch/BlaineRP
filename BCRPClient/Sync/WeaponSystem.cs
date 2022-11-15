#define DEBUGGING

using RAGE;
using RAGE.Elements;
using RAGE.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPClient.Sync
{
    class WeaponSystem : Events.Script
    {
        private const float IN_VEHICLE_DAMAGE_COEF = 0.75f;

        public const uint UnarmedHash = 0xA2719263;
        public const uint MobileHash = 966099553;

        public static bool Reloading;

        public static (Player Player, int Damage, int BoneIdx, DateTime Time) LastAttackerInfo { get; private set; }

        #region Supported Weapons
        public class Weapon
        {
            public uint Hash { get; set; }
            public string Name { get; set; }
            public string GameName { get; set; }
            public float BaseDamage { get; set; }
            public float MaxDistance { get; set; }
            public float DistanceRatio { get; set; }

            private Dictionary<PartTypes, float> BodyRatios;

            public bool HasAmmo { get; set; }

            public float GetBodyRatio(PartTypes type)
            {
                float ratio = 1f;

                BodyRatios?.TryGetValue(type, out ratio);

                return ratio;
            }

            public Weapon(string Name, string GameName, float BaseDamage, float MaxDistance, float DistanceRatio, float HeadRatio, float ChestRatio, float LimbRatio, bool HasAmmo = true)
            {
                this.Hash = RAGE.Util.Joaat.Hash(GameName);
                this.Name = Name;
                this.GameName = GameName;
                this.BaseDamage = BaseDamage;
                this.MaxDistance = MaxDistance;
                this.DistanceRatio = DistanceRatio;

                this.BodyRatios = new Dictionary<PartTypes, float>()
                {
                    { PartTypes.Head, HeadRatio },
                    { PartTypes.Chest, ChestRatio },
                    { PartTypes.Limb, LimbRatio },
                };

                this.HasAmmo = HasAmmo;
            }
        }

        public static List<Weapon> WeaponList = new List<Weapon>()
        {
            // Ближний бой
            new Weapon("Кулак", "weapon_unarmed", 3f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Разбитая бутылка", "weapon_bottle", 6f, 0f, 5f, 1.5f, 1f, 0.5f, false),
            new Weapon("Фонарик", "weapon_flashlight", 4f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Молоток", "weapon_hammer", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Топор", "weapon_hatchet", 8f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Резиновая дубинка", "weapon_nightstick", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Тазер", "weapon_stungun", 5f, 7.5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Бейсбольная бита", "weapon_bat", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Лом", "weapon_crowbar", 7f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Кастет", "weapon_knuckle", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Кий", "weapon_poolcue", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Клюшка для гольфа", "weapon_golfclub", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Мачете", "weapon_machete", 12f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Перочинный нож", "weapon_switchblade", 7f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Клинок", "weapon_dagger", 9f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new Weapon("Нож", "weapon_knife", 10f, 5f, 0f, 1.5f, 1f, 0.5f, false),    

            // Пистолеты (9мм)
            new Weapon("Пистолет", "weapon_pistol", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Пистолет MK2", "weapon_pistol_mk2", 10f, 90f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Боевой пистолет", "weapon_combatpistol", 10f, 75f, 0.12f, 1.5f, 1f, 0.5f),
            new Weapon("Тяжёлый пистолет", "weapon_heavypistol", 12f, 85f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Старинный пистолет", "weapon_vintagepistol", 7f, 70f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Керамический пистолет", "weapon_ceramicpistol", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Скорострельный пистолет", "weapon_appistol", 5f, 60f, 0.08f, 1.5f, 1f, 0.5f),

            // Полуавтоматические винтовки (5.56мм)
            new Weapon("SMG", "weapon_smg", 6f, 60f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("SMG MK2", "weapon_smg_mk2", 7f, 70f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Штурмовая SMG", "weapon_assaultsmg", 8f, 75f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Малокалиберная винтовка", "weapon_combatpdw", 8f, 80f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Автоматический пистолет", "weapon_machinepistol", 6f, 60f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Микро SMG", "weapon_microsmg", 8f, 55f, 0.12f, 1.5f, 1f, 0.5f),
            new Weapon("Мини SMG", "weapon_minismg)", 7f, 70f, 0.1f, 1.5f, 1f, 0.5f),

            // Штурмовые винтовки (7.62мм)
            new Weapon("Карабин", "weapon_carbinerifle", 9f, 100f, 0.09f, 1.5f, 1f, 0.5f),
            new Weapon("Штурмовая винтовка", "weapon_assaultrifle", 9f, 110f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Штурмовая винтовка MK2", "weapon_assaultriflemk2", 9f, 110f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Компактная винтовка", "weapon_compactrifle", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Военная винтовка", "weapon_militaryrifle", 10f, 120f, 0.08f, 1.5f, 1f, 0.5f),
            new Weapon("Улучшенная винтовка", "weapon_advancedrifle", 9f, 90f, 0.1f, 1.5f, 1f, 0.5f),
            new Weapon("Тяжелая винтовка", "weapon_heavyrifle", 12f, 120f, 0.09f, 1.5f, 1f, 0.5f),

            // Пулеметы (7.62мм)
            new Weapon("Боевой пулемёт", "weapon_combatmg", 9f, 70f, 0.12f, 1.5f, 1f, 0.5f),
            new Weapon("Пулемёт Гузенберга", "weapon_gusenberg", 8f, 60f, 0.13f, 1.5f, 1f, 0.5f),

            // Револьверы (11.43мм)
            new Weapon("Тяжелый револьвер", "weapon_revolver", 40f, 70f, 0.5f, 2f, 1f, 0.5f),
            new Weapon("Тяжелый револьвер MK2", "weapon_revolver_mk2", 45f, 80f, 0.4f, 2.5f, 1f, 0.5f),
            new Weapon("Классический револьвер", "weapon_doubleaction", 35f, 60f, 1.75f, 1.5f, 1f, 0.5f),
            new Weapon("Короткий мушкет", "weapon_marksmanpistol", 25f, 30f, 0.8f, 1.5f, 1f, 0.5f),
            new Weapon("Длинный револьвер", "weapon_navyrevolver", 30f, 70f, 0.4f, 1.75f, 1f, 0.5f),

            // Дробовики (12мм)
            new Weapon("Помповый дробовик", "weapon_pumpshotgun", 40f, 10f, 4f, 2f, 1f, 0.5f),
            new Weapon("Помповый дробовик MK2", "weapon_pumpshotgun_mk2", 50f, 15f, 3f, 2f, 1f, 0.5f),
            new Weapon("Обрез", "weapon_sawnoffshotgun", 35f, 5f, 7f, 2f, 1f, 0.5f),
            new Weapon("Штурмовой дробовик", "weapon_assaultshotgun", 45f, 15f, 3f, 2f, 1f, 0.5f),
            new Weapon("Тяжелый дробовик", "weapon_heavyshotgun", 60f, 15f, 4f, 2f, 1f, 0.5f),
            new Weapon("Охотничье ружье", "weapon_musket", 50f, 25f, 2f, 2f, 1f, 0.5f),

            // Снайперские винтовки (12.7мм)
            new Weapon("Винтовка Марксмана", "weapon_marksmanrifle", 80f, 100f, 0.3f, 2f, 1f, 0.5f),
            new Weapon("Тяжелая снайперская винтовка", "weapon_heavysniper", 150f, 500f, 0.25f, 2f, 1f, 0.5f)
        };
        #endregion

        #region Parts Of Body
        public enum PartTypes
        {
            Zero = -1,
            Head = 0,
            Chest,
            Limb
        }

        private static Dictionary<int, PartTypes> PedParts = new Dictionary<int, PartTypes>()
        {
            { 20, PartTypes.Head },

            { 0, PartTypes.Chest },
            { 7, PartTypes.Chest },
            { 8, PartTypes.Chest },
            { 9, PartTypes.Chest },
            { 10, PartTypes.Chest },
            { 11, PartTypes.Chest },
            { 15, PartTypes.Chest },
            { 19, PartTypes.Chest },

            // Left Hand
            { 12, PartTypes.Limb },
            { 13, PartTypes.Limb },
            { 14, PartTypes.Limb },

            // Right Hand
            { 16, PartTypes.Limb },
            { 17, PartTypes.Limb },
            { 18, PartTypes.Limb },

            // Left Leg
            { 1, PartTypes.Limb },
            { 2, PartTypes.Limb },
            { 3, PartTypes.Limb },

            // Right Leg
            { 4, PartTypes.Limb },
            { 5, PartTypes.Limb },
            { 6, PartTypes.Limb },
        };

        private static Dictionary<int, PartTypes> VehicleParts = new Dictionary<int, PartTypes>()
        {
            // Engine
            { 6, PartTypes.Head },

            // Windows
            { 1, PartTypes.Zero },
            { 2, PartTypes.Zero },
            { 19, PartTypes.Zero },
            { 20, PartTypes.Zero },
            { 21, PartTypes.Zero },
            { 22, PartTypes.Zero },
            { 23, PartTypes.Zero },
            { 24, PartTypes.Zero },

            // Doors
            { 5, PartTypes.Limb },
            { 4, PartTypes.Limb },
            { 15, PartTypes.Limb },
            { 16, PartTypes.Limb },

            // Root
            { 0, PartTypes.Chest },
            { 7, PartTypes.Chest },
            { 8, PartTypes.Chest },
            { 13, PartTypes.Chest },
        };

        private static Dictionary<PartTypes, float> VehicleRatios = new Dictionary<PartTypes, float>()
        {
            { PartTypes.Head, 1.5f },
            { PartTypes.Zero, 0f },
            { PartTypes.Limb, 0.5f },
            { PartTypes.Chest, 1f },
        };
        #endregion

        private static DateTime LastSentReload;
        private static DateTime LastSentUpdate;
        public static DateTime LastWeaponShot;
        public static DateTime LastSentPedDamage;
        public static DateTime LastSentVehicleDamage;

        public static DateTime LastArmourLoss;

        private static int _DisabledFiringCounter { get; set; }
        public static bool DisabledFiring
        {
            get => _DisabledFiringCounter > 0;

            set
            {
                if (!value)
                {
                    if (_DisabledFiringCounter > 0)
                        _DisabledFiringCounter--;
                }
                else
                {
                    _DisabledFiringCounter++;
                }
            }
        }

        public delegate void DamageHandler(int healthLoss, int armourLoss);
        public static event DamageHandler OnDamage;

        public WeaponSystem()
        {
            Utils.JsEval
            (
                @"mp.game.weapon.unequipEmptyWeapons = false;

                mp.events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => { mp.game.weapon.setCurrentDamageEventAmount(1); });
                mp.events.add('outgoingDamage', (sourceEntity, targetEntity, sourcePlayer, weapon, boneIndex, damage) => { mp.game.weapon.setCurrentDamageEventAmount(boneIndex); });"
            );

            LastAttackerInfo = (Player.LocalPlayer, 0, -1, DateTime.Now);

            Reloading = false;

            Events.OnIncomingDamage += IncomingDamage;
            Events.OnOutgoingDamage += OutgoingDamage;

            GameEvents.Update += DamageWatcher;

            // Armour Broken
            OnDamage += ArmourCheck;

            OnDamage += ((int healthLoss, int armourLoss) =>
            {
                CEF.Inventory.UpdateStates();
            });

            #if DEBUGGING
            OnDamage += (hpLoss, armLoss) => Utils.ConsoleOutput($"DAMAGE! HP_LOSS: {hpLoss} | ARM_LOSS: {armLoss}");
            #endif

            RAGE.Game.Graphics.RequestStreamedTextureDict("shared", true);

            // disable headshot critical
            RAGE.Elements.Player.LocalPlayer.SetSuffersCriticalHits(false);

            RAGE.Game.Player.DisablePlayerVehicleRewards();

            LastArmourLoss = DateTime.Now;

            LastSentReload = DateTime.Now;
            LastSentUpdate = DateTime.Now;
            LastWeaponShot = DateTime.Now;

            LastSentPedDamage = DateTime.Now;
            LastSentVehicleDamage = DateTime.Now;

            _DisabledFiringCounter = 0;

            #region Render
            GameEvents.Update += () =>
            {
                if (DisabledFiring)
                    RAGE.Game.Player.DisablePlayerFiring(true);

                if (Player.LocalPlayer.IsPerformingStealthKill())
                    Player.LocalPlayer.ClearTasksImmediately();

                // Disable Weapon Wheel
                RAGE.Game.Pad.DisableControlAction(24, 157, true);
                RAGE.Game.Pad.DisableControlAction(24, 158, true);
                RAGE.Game.Pad.DisableControlAction(24, 37, true);

                RAGE.Game.Pad.DisableControlAction(32, 14, true);
                RAGE.Game.Pad.DisableControlAction(32, 15, true);

                // Disable Reload Btn
                RAGE.Game.Pad.DisableControlAction(32, 45, true);

                // Disable Vehicle Weapon Change
                RAGE.Game.Pad.DisableControlAction(32, 99, true);
                RAGE.Game.Pad.DisableControlAction(32, 100, true);
                RAGE.Game.Pad.DisableControlAction(32, 115, true);
                RAGE.Game.Pad.DisableControlAction(32, 116, true);

                // Disable Melee Atack (R&Q)
                RAGE.Game.Pad.DisableControlAction(2, 140, true);
                RAGE.Game.Pad.DisableControlAction(2, 141, true);
                RAGE.Game.Pad.DisableControlAction(2, 263, true);
                RAGE.Game.Pad.DisableControlAction(2, 264, true);

                // Disable Stealth Mode (ctrl)
                RAGE.Game.Pad.DisableControlAction(32, 36, true);

                // Disable Alternate Weapon Attack

                if (RAGE.Game.Weapon.GetWeaponDamageType(RAGE.Elements.Player.LocalPlayer.GetSelectedWeapon()) != 2)
                    RAGE.Game.Pad.DisableControlAction(2, 142, true);

                if (Player.LocalPlayer.IsUsingActionMode())
                 Player.LocalPlayer.SetUsingActionMode(false, -1, "DEFAULT_ACTION");
            };

            GameEvents.Update += () =>
            {
                if (RAGE.Game.Invoker.Invoke<bool>(RAGE.Game.Natives.IsAimCamActive) || RAGE.Game.Invoker.Invoke<bool>(RAGE.Game.Natives.IsFirstPersonAimCamActive))
                {
                    if (Settings.Aim.Type == Settings.Aim.Types.Default)
                        return;

                    RAGE.Game.Ui.HideHudComponentThisFrame(14);

                    if (Settings.Aim.Type == Settings.Aim.Types.Cross)
                    {
                        float scale = 3f * Settings.Aim.Scale;

                        if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded("shared"))
                            RAGE.Game.Graphics.DrawSprite("shared", "menuplus_32", 0.5f, 0.5f, scale * 32 / GameEvents.ScreenResolution.X, scale * 32 / GameEvents.ScreenResolution.Y, 0f, Settings.Aim.Color.Red, Settings.Aim.Color.Green, Settings.Aim.Color.Blue, (int)Math.Floor(Settings.Aim.Alpha * 255), 0);
                        else
                            RAGE.Game.Graphics.RequestStreamedTextureDict("shared", true);
                    }
                    else if (Settings.Aim.Type == Settings.Aim.Types.Dot)
                    {
                        float scale = 1f * Settings.Aim.Scale;

                        if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded("shared"))
                            RAGE.Game.Graphics.DrawSprite("shared", "medaldot_32", 0.5f, 0.5f, scale * 32 / GameEvents.ScreenResolution.X, scale * 32 / GameEvents.ScreenResolution.Y, 0f, Settings.Aim.Color.Red, Settings.Aim.Color.Green, Settings.Aim.Color.Blue, (int)Math.Floor(Settings.Aim.Alpha * 255), 0);
                        else
                            RAGE.Game.Graphics.RequestStreamedTextureDict("shared", true);
                    }
                }
            };
            #endregion

            Events.OnPlayerDeath += (Player player, uint reason, Player killer, Events.CancelEventArgs cancel) =>
            {
                cancel.Cancel = true;

                if (player?.Handle != Player.LocalPlayer.Handle)
                    return;

                var pData = Sync.Players.GetData(player);

                if (pData == null || pData.IsInvincible)
                {
                    var pos = Player.LocalPlayer.GetCoords(false);
                    var heading = Player.LocalPlayer.GetHeading();

                    RAGE.Game.Network.NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z, heading, true, false, 0);

                    RAGE.Game.Misc.SetFadeOutAfterDeath(false);

                    Events.OnPlayerSpawn?.Invoke(null);
                }
                else
                {
                    Sync.Players.CloseAll(false);

                    if ((killer?.Exists != true || killer.Handle == Player.LocalPlayer.Handle) && DateTime.Now.Subtract(LastAttackerInfo.Time).TotalMilliseconds <= 1000)
                        killer = LastAttackerInfo.Player;

                    if (Sync.Players.GetData(killer) == null)
                        killer = Player.LocalPlayer;

                    Additional.Scaleform.ShowWasted(reason, killer);

                    Events.CallRemote("Players::OnDeath", killer);
                }
            };

            Events.OnPlayerWeaponShot += (Vector3 targetPos, Player target, Events.CancelEventArgs cancel) =>
            {
                LastWeaponShot = DateTime.Now;

                Additional.AntiCheat.LastAllowedAmmo--;
            };

            #region Events
            Events.Add("Weapon::TaskReload", (object[] args) =>
            {
                if (Reloading)
                    return;

                Reloading = true;

                var weapon = Player.LocalPlayer.GetSelectedWeapon();
                var ammo = Player.LocalPlayer.GetAmmoInWeapon(weapon);

                Player.LocalPlayer.SetAmmoInClip(weapon, 0);
                Player.LocalPlayer.SetAmmo(weapon, ammo, 1);

                (new AsyncTask(() =>
                {
                    Reloading = false;
                }, 2000, false, 0)).Run();
            });
            #endregion
        }

        #region Stuff
        public static void UpdateWeapon()
        {
            var weapon = RAGE.Elements.Player.LocalPlayer.GetSelectedWeapon();

            var curAmmo = RAGE.Elements.Player.LocalPlayer.GetAmmoInWeapon(weapon);

            Player.LocalPlayer.SetCurrentWeapon(weapon, true);

            CEF.HUD.SetAmmo(curAmmo);

            // AutoReload
            if (curAmmo == 0 && RAGE.Game.Player.IsPlayerFreeAiming() && (RAGE.Game.Pad.IsControlPressed(32, 24) || RAGE.Game.Pad.IsDisabledControlPressed(32, 24)) && Settings.Interface.AutoReload)
            {
                if (Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheckReload))
                    return;

                if (!LastSentReload.IsSpam(2000, false, false))
                {
                    Events.CallRemote("Weapon::Reload", curAmmo);

                    LastSentReload = DateTime.Now;
                }
            }
        }

        private static Utils.Actions[] ActionsToCheckReload = new Utils.Actions[]
        {
            Utils.Actions.Knocked,
            Utils.Actions.Frozen,
            Utils.Actions.Cuffed,

            Utils.Actions.Crawl,
            Utils.Actions.Finger,
            Utils.Actions.PushingVehicle,

            Utils.Actions.Animation,
            Utils.Actions.FastAnimation,
            Utils.Actions.Scenario,

            Utils.Actions.InWater,
            Utils.Actions.Shooting, Utils.Actions.Reloading,
            Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, //Utils.Actions.OnFoot,
        };

        public static void ReloadWeapon()
        {
            if (Utils.IsAnyCefActive() || !Utils.CanDoSomething(ActionsToCheckReload))
                return;

            if (LastSentReload.IsSpam(2000, false, false) || CEF.Inventory.LastSent.IsSpam(250, false, false) || LastWeaponShot.IsSpam(250, false, false))
                return;

            var curWeapon = Player.LocalPlayer.GetSelectedWeapon();
            var weapProp = WeaponList.Where(x => x.Hash == curWeapon).FirstOrDefault();

            if (weapProp == null || !weapProp.HasAmmo)
                return;

            // add an check if weapon is full of ammo

            Events.CallRemote("Weapon::Reload", Player.LocalPlayer.GetAmmoInWeapon(curWeapon));

            LastSentReload = DateTime.Now;
        }
        #endregion

        #region Incoming Damage Handler
        private static int LastHealth = 0;
        private static int LastArmour = 0;

        public static void DamageWatcher()
        {
            int healthLoss = 0, armourLoss = 0;

            var curHealth = Player.LocalPlayer.GetRealHealth();
            var curArmour = Player.LocalPlayer.GetArmour();

            if (LastHealth > curHealth)
                healthLoss = LastHealth - curHealth;

            if (LastArmour > curArmour)
                armourLoss = LastArmour - curArmour;

            LastHealth = curHealth;
            LastArmour = curArmour;

            if (curHealth <= 0 && !Player.LocalPlayer.IsDeadOrDying(true))
            {
                Player.LocalPlayer.SetRealHealth(0);
            }

            if (healthLoss == 0 && armourLoss == 0)
                return;

            OnDamage?.Invoke(healthLoss, armourLoss);

            if (armourLoss > 0)
                LastArmourLoss = DateTime.Now;
        }

        public static void ArmourCheck(int healthLoss, int armourLoss)
        {
            if (armourLoss > 0 && Player.LocalPlayer.GetArmour() == 0)
                Events.CallRemote("Players::ArmourBroken");
        }

        private static void IncomingDamage(Player sourcePlayer, Entity sourceEntity, Entity targetEntity, ulong weaponHashLong, ulong notWorkingShit, int boneIdx, Events.CancelEventArgs cancel)
        {
            var weaponHash = (uint)weaponHashLong;

            var gun = WeaponList.Where(x => x.Hash == weaponHash).FirstOrDefault();

            //Utils.ConsoleOutput($"SourceEntityRID: {sourceEntity.RemoteId}, TargetEntityRID: {targetEntity.RemoteId}, BoneIdx: {boneIdx}");
            //Utils.ConsoleOutput($"SourceEntity: {sourceEntity.Type}, TargetEntityRID: {targetEntity.Type}, BoneIdx: {boneIdx}");

            cancel.Cancel = true;

            if (gun == null || sourceEntity == null || targetEntity == null)
                return;

            if (Player.LocalPlayer.HasData("InGreenZone"))
                return;

            if (sourceEntity is Player sP)
            {
                sourcePlayer = sP;

                if (!sourcePlayer.Exists || sourcePlayer.GetSelectedWeapon() != weaponHash)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);
                var sData = Sync.Players.GetData(sourcePlayer);

                if (pData == null || sData == null)
                    return;

                var sPos = sourcePlayer.GetCoords(false);

                if (targetEntity is Player targetPlayer)
                {
                    if (targetPlayer.Handle != Player.LocalPlayer.Handle)
                        return;

                    if (pData.IsInvincible)
                        return;

                    var pPos = Player.LocalPlayer.GetCoords(false);

                    float distance = RAGE.Vector3.Distance(pPos, sPos);

                    if (distance > gun.MaxDistance)
                        return;

                    if (!pData.IsKnocked)
                        cancel.Cancel = false;

                    PartTypes pType = PartTypes.Limb;

                    PedParts.TryGetValue(boneIdx, out pType);

                    float boneRatio = gun.GetBodyRatio(pType);

                    var customDamage = (int)((gun.BaseDamage - (gun.DistanceRatio * distance)) * boneRatio * (sourcePlayer.IsSittingInAnyVehicle() ? IN_VEHICLE_DAMAGE_COEF : 1f)) - (pData.IsKnocked ? 0 : 1);

                    if (customDamage >= 0)
                    {
                        LastAttackerInfo = (sourcePlayer, customDamage + 1, boneIdx, DateTime.Now);

                        bool isBullet = RAGE.Game.Weapon.GetWeaponDamageType(weaponHash) == 3;

                        Player.LocalPlayer.ApplyDamageTo(customDamage, isBullet);

                        var hp = Player.LocalPlayer.GetRealHealth();
                        var arm = Player.LocalPlayer.GetArmour();

                        if (hp <= 20 && !pData.IsKnocked)
                        {
                            Player.LocalPlayer.SetInvincible(true);

                            Player.LocalPlayer.ApplyDamageTo(1, isBullet);

                            AsyncTask.RunSlim(() => Player.LocalPlayer.SetInvincible(false), 25);
                        }

                        #if DEBUGGING
                        Utils.ConsoleOutputLimited($"Игрок: #{sData.CID} | Урон: {customDamage + 1} | Дистанция: {distance} | Часть тела: {boneIdx}", true, 1000);
                        #endif
                    }
                    else
                    {
                        cancel.Cancel = true;

                        return;
                    }
                }
                else if (targetEntity is Vehicle veh)
                {
                    if (!veh.Exists|| veh.Controller?.Handle != Player.LocalPlayer.Handle)
                        return;

                    var vData = Sync.Vehicles.GetData(targetEntity as Vehicle);

                    if (vData == null)
                        return;

                    if (vData.IsInvincible)
                        return;

                    var pPos = veh.GetCoords(false);

                    float distance = RAGE.Vector3.Distance(pPos, sPos);

                    if (distance > gun.MaxDistance)
                        return;

                    cancel.Cancel = false;

                    PartTypes pType = PartTypes.Limb;

                    VehicleParts.TryGetValue(boneIdx, out pType);

                    float boneRatio = VehicleRatios[pType];

                    var customDamage = (float)((gun.BaseDamage - (gun.DistanceRatio * distance)) * boneRatio) - 1;

                    if (customDamage <= 0)
                        return;

                    veh.SetEngineHealth(veh.GetEngineHealth() - customDamage);
                }
                else
                    return;
            }
            else
                return;
        }
        #endregion

        private static void OutgoingDamage(Entity sourceEntity, Entity targetEntity, Player targetPlayer, ulong weaponHashLong, ulong boneIdx, int damage, Events.CancelEventArgs cancel)
        {
            //Utils.ConsoleOutput($"SourceEntityRID: {sourceEntity.RemoteId}, TargetEntityRID: {targetEntity.RemoteId}, TargetPlayerRID: {targetPlayer.RemoteId}, BoneIdx: {(int)boneIdx}");
            //Utils.ConsoleOutput($"SourceEntity: {sourceEntity.Type}, TargetEntityRID: {targetEntity.Type}, TargetPlayerRID: {targetPlayer.Type}, Damage: {damage}");

            cancel.Cancel = true;

            if (Player.LocalPlayer.HasData("InGreenZone"))
                return;

            if (targetEntity?.Type == RAGE.Elements.Type.Vehicle)
            {
                var data = Sync.Vehicles.GetData(targetEntity as Vehicle);

                if (data == null || data.IsInvincible)
                    return;

                if (LastSentVehicleDamage.IsSpam(100, false, false))
                {
                    return;
                }
                else
                {
                    cancel.Cancel = false;

                    LastSentVehicleDamage = DateTime.Now;
                }
            }
            else
            {
                if (LastSentPedDamage.IsSpam(100, false, false))
                {
                    return;
                }
                else
                {
                    cancel.Cancel = false;

                    LastSentPedDamage = DateTime.Now;
                }
            }
        }
    }
}
