#define DEBUGGING

using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using Players = BlaineRP.Client.Game.Scripts.Sync.Players;

namespace BlaineRP.Client.Game.Management.Weapons
{
    [Script(int.MaxValue)]
    public class Core
    {
        private const float IN_VEHICLE_DAMAGE_COEF = 0.75f;

        public const uint UnarmedHash = 0xA2719263;
        public const uint MobileHash = 966099553;

        public static bool Reloading { get; private set; }

        public static (Player Player, int Damage, int BoneIdx, DateTime Time) LastAttackerInfo { get; private set; }

        public static List<WeaponInfo> WeaponList = new List<WeaponInfo>()
        {
            // Ближний бой
            new WeaponInfo("weapon_unarmed", 3f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_bottle", 6f, 0f, 5f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_flashlight", 4f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_hammer", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_hatchet", 8f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_nightstick", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_stungun", 5f, 7.5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_bat", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_crowbar", 7f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_knuckle", 5f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_poolcue", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_golfclub", 6f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_machete", 12f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_switchblade", 7f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_dagger", 9f, 5f, 0f, 1.5f, 1f, 0.5f, false),
            new WeaponInfo("weapon_knife", 10f, 5f, 0f, 1.5f, 1f, 0.5f, false),    

            // Пистолеты (9мм)
            new WeaponInfo("weapon_pistol", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                }
            },
            new WeaponInfo("weapon_pistol_mk2", 10f, 90f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH_02") },
                }
            },
            new WeaponInfo("weapon_combatpistol", 10f, 75f, 0.12f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                }
            },
            new WeaponInfo("weapon_heavypistol", 12f, 85f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                }
            },
            new WeaponInfo("weapon_vintagepistol", 7f, 70f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                }
            },
            new WeaponInfo("weapon_ceramicpistol", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_CERAMICPISTOL_SUPP") },
                }
            },
            new WeaponInfo("weapon_appistol", 5f, 60f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                }
            },

            // Полуавтоматические винтовки (5.56мм)
            new WeaponInfo("weapon_smg", 6f, 60f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO_02") },
                }
            },
            new WeaponInfo("weapon_smg_mk2", 7f, 70f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO_02_SMG_MK2") },
                }
            },
            new WeaponInfo("weapon_assaultsmg", 8f, 75f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO") },
                }
            },
            new WeaponInfo("weapon_combatpdw", 8f, 80f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_SMALL") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_machinepistol", 6f, 60f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_SUPP") },
                }
            },
            new WeaponInfo("weapon_microsmg", 8f, 55f, 0.12f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO") },
                }
            },
            new WeaponInfo("weapon_minismg)", 7f, 70f, 0.1f, 1.5f, 1f, 0.5f)
            {

            },

            // Штурмовые винтовки (7.62мм)
            new WeaponInfo("weapon_carbinerifle", 9f, 100f, 0.09f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MEDIUM") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_assaultrifle", 9f, 110f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_assaultriflemk2", 9f, 110f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP_02") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO_MK2") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP_02") },
                }
            },
            new WeaponInfo("weapon_compactrifle", 8f, 80f, 0.1f, 1.5f, 1f, 0.5f)
            {

            },
            new WeaponInfo("weapon_militaryrifle", 10f, 120f, 0.08f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_SMALL") },
                }
            },
            new WeaponInfo("weapon_advancedrifle", 9f, 90f, 0.1f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_SMALL") },
                }
            },
            new WeaponInfo("weapon_heavyrifle", 12f, 120f, 0.09f, 1.5f, 1f, 0.5f)
            {

            },

            // Пулеметы (7.62мм)
            new WeaponInfo("weapon_combatmg", 9f, 70f, 0.12f, 1.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_SMALL_02") },
                }
            },
            new WeaponInfo("weapon_gusenberg", 8f, 60f, 0.13f, 1.5f, 1f, 0.5f)
            {

            },

            // Револьверы (11.43мм)
            new WeaponInfo("weapon_revolver", 40f, 70f, 0.5f, 2f, 1f, 0.5f)
            {

            },
            new WeaponInfo("weapon_revolver_mk2", 45f, 80f, 0.4f, 2.5f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_PI_FLSH") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO_MK2") },
                }
            },
            new WeaponInfo("weapon_doubleaction", 35f, 60f, 1.75f, 1.5f, 1f, 0.5f)
            {

            },
            new WeaponInfo("weapon_marksmanpistol", 25f, 30f, 0.8f, 1.5f, 1f, 0.5f)
            {

            },
            new WeaponInfo("weapon_navyrevolver", 30f, 70f, 0.4f, 1.75f, 1f, 0.5f)
            {

            },

            // Дробовики (12мм)
            new WeaponInfo("weapon_pumpshotgun", 40f, 10f, 4f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_SR_SUPP") },
                }
            },
            new WeaponInfo("weapon_pumpshotgun_mk2", 50f, 15f, 3f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_SR_SUPP_03") },
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_MACRO_MK2") },
                }
            },
            new WeaponInfo("weapon_sawnoffshotgun", 35f, 5f, 7f, 2f, 1f, 0.5f)
            {

            },
            new WeaponInfo("weapon_assaultshotgun", 45f, 15f, 3f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_heavyshotgun", 60f, 15f, 4f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_musket", 50f, 25f, 2f, 2f, 1f, 0.5f)
            {

            },

            // Снайперские винтовки (12.7мм)
            new WeaponInfo("weapon_marksmanrifle", 80f, 100f, 0.3f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Flashlight, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_FLSH") },
                    { WeaponComponentTypes.Suppressor, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_SUPP") },
                    { WeaponComponentTypes.Grip, RAGE.Util.Joaat.Hash("COMPONENT_AT_AR_AFGRIP") },
                }
            },
            new WeaponInfo("weapon_heavysniper", 150f, 500f, 0.25f, 2f, 1f, 0.5f)
            {
                ComponentsHashes = new Dictionary<WeaponComponentTypes, uint>()
                {
                    { WeaponComponentTypes.Scope, RAGE.Util.Joaat.Hash("COMPONENT_AT_SCOPE_LARGE") },
                }
            },
        };

        private static Dictionary<int, BodyPartTypes> PedParts = new Dictionary<int, BodyPartTypes>()
        {
            { 20, BodyPartTypes.Head },

            { 0, BodyPartTypes.Chest },
            { 7, BodyPartTypes.Chest },
            { 8, BodyPartTypes.Chest },
            { 9, BodyPartTypes.Chest },
            { 10, BodyPartTypes.Chest },
            { 11, BodyPartTypes.Chest },
            { 15, BodyPartTypes.Chest },
            { 19, BodyPartTypes.Chest },

            // Left Hand
            { 12, BodyPartTypes.Limb },
            { 13, BodyPartTypes.Limb },
            { 14, BodyPartTypes.Limb },

            // Right Hand
            { 16, BodyPartTypes.Limb },
            { 17, BodyPartTypes.Limb },
            { 18, BodyPartTypes.Limb },

            // Left Leg
            { 1, BodyPartTypes.Limb },
            { 2, BodyPartTypes.Limb },
            { 3, BodyPartTypes.Limb },

            // Right Leg
            { 4, BodyPartTypes.Limb },
            { 5, BodyPartTypes.Limb },
            { 6, BodyPartTypes.Limb },
        };

        private static Dictionary<int, BodyPartTypes> VehicleParts = new Dictionary<int, BodyPartTypes>()
        {
            // Engine
            { 6, BodyPartTypes.Head },

            // Windows
            { 1, BodyPartTypes.Zero },
            { 2, BodyPartTypes.Zero },
            { 19, BodyPartTypes.Zero },
            { 20, BodyPartTypes.Zero },
            { 21, BodyPartTypes.Zero },
            { 22, BodyPartTypes.Zero },
            { 23, BodyPartTypes.Zero },
            { 24, BodyPartTypes.Zero },

            // Doors
            { 5, BodyPartTypes.Limb },
            { 4, BodyPartTypes.Limb },
            { 15, BodyPartTypes.Limb },
            { 16, BodyPartTypes.Limb },

            // Root
            { 0, BodyPartTypes.Chest },
            { 7, BodyPartTypes.Chest },
            { 8, BodyPartTypes.Chest },
            { 13, BodyPartTypes.Chest },
        };

        private static Dictionary<BodyPartTypes, float> VehicleRatios = new Dictionary<BodyPartTypes, float>()
        {
            { BodyPartTypes.Head, 1.5f },
            { BodyPartTypes.Zero, 0f },
            { BodyPartTypes.Limb, 0.5f },
            { BodyPartTypes.Chest, 1f },
        };

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

        public Core()
        {
            Invoker
            .JsEval
            (
                @"mp.game.weapon.unequipEmptyWeapons = false;

                mp.events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => { mp.game.weapon.setCurrentDamageEventAmount(1); });
                mp.events.add('outgoingDamage', (sourceEntity, targetEntity, sourcePlayer, weapon, boneIndex, damage) => { mp.game.weapon.setCurrentDamageEventAmount(boneIndex); });"
            );

            LastAttackerInfo = (Player.LocalPlayer, 0, -1, World.Core.ServerTime);

            Reloading = false;

            Events.OnIncomingDamage += IncomingDamage;
            Events.OnOutgoingDamage += OutgoingDamage;

            Main.Update += DamageWatcher;

            // Armour Broken
            OnDamage += ArmourCheck;

            OnDamage += ((int healthLoss, int armourLoss) =>
            {
                Inventory.UpdateStates();
            });

#if DEBUGGING
            OnDamage += (hpLoss, armLoss) => Utils.Console.Output($"DAMAGE! HP_LOSS: {hpLoss} | ARM_LOSS: {armLoss}");
#endif

            RAGE.Game.Graphics.RequestStreamedTextureDict("shared", true);

            RAGE.Game.Player.DisablePlayerVehicleRewards();

            _DisabledFiringCounter = 0;

            #region Render
            Main.Update += () =>
            {
                Player.LocalPlayer.SetSuffersCriticalHits(false);

                /*                RAGE.Game.Player.SetPlayerMeleeWeaponDamageModifier(0.15f, 1);
                                RAGE.Game.Player.SetPlayerMeleeWeaponDefenseModifier(-9999999f);
                                RAGE.Game.Player.SetPlayerWeaponDefenseModifier(-9999999f);
                                RAGE.Game.Ped.SetAiWeaponDamageModifier(0f);
                                RAGE.Game.Ped.SetAiMeleeWeaponDamageModifier(1f);*/

                if (DisabledFiring || Cursor.IsActive)
                {
                    RAGE.Game.Player.DisablePlayerFiring(true);

                    RAGE.Game.Pad.DisableControlAction(32, 25, true);
                }

                /*                if (Player.LocalPlayer.IsPerformingStealthKill())
                                    Player.LocalPlayer.ClearTasksImmediately();*/

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

                /*                // Disable Alternate Weapon Attack

                                if (!RAGE.Game.Weapon.IsPedArmed(Player.LocalPlayer.Handle, 1))
                                    RAGE.Game.Pad.DisableControlAction(2, 142, true);*/

                if (Player.LocalPlayer.IsUsingActionMode())
                    Player.LocalPlayer.SetUsingActionMode(false, -1, "DEFAULT_ACTION");

                Player.LocalPlayer.SetResetFlag(200, true);
            };

            Main.Update += () =>
            {
                if (Player.LocalPlayer.HasWeapon() && RAGE.Game.Cam.IsAimCamActive())
                {
                    if (Settings.User.Aim.Type == Settings.User.Aim.Types.Default)
                        return;

                    RAGE.Game.Ui.HideHudComponentThisFrame(14);

                    if (Settings.User.Aim.Type == Settings.User.Aim.Types.Cross)
                    {
                        var scale = 3f * Settings.User.Aim.Scale;

                        if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded("shared"))
                            RAGE.Game.Graphics.DrawSprite("shared", "menuplus_32", 0.5f, 0.5f, scale * 32 / Main.ScreenResolution.X, scale * 32 / Main.ScreenResolution.Y, 0f, Settings.User.Aim.Color.Red, Settings.User.Aim.Color.Green, Settings.User.Aim.Color.Blue, (int)System.Math.Floor(Settings.User.Aim.Alpha * 255), 0);
                        else
                            RAGE.Game.Graphics.RequestStreamedTextureDict("shared", true);
                    }
                    else if (Settings.User.Aim.Type == Settings.User.Aim.Types.Dot)
                    {
                        var scale = 1f * Settings.User.Aim.Scale;

                        if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded("shared"))
                            RAGE.Game.Graphics.DrawSprite("shared", "medaldot_32", 0.5f, 0.5f, scale * 32 / Main.ScreenResolution.X, scale * 32 / Main.ScreenResolution.Y, 0f, Settings.User.Aim.Color.Red, Settings.User.Aim.Color.Green, Settings.User.Aim.Color.Blue, (int)System.Math.Floor(Settings.User.Aim.Alpha * 255), 0);
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

                var pData = PlayerData.GetData(player);

                if (pData != null)
                {
                    Players.CloseAll(false);

                    if ((killer?.Exists != true || killer.Handle == Player.LocalPlayer.Handle) && World.Core.ServerTime.Subtract(LastAttackerInfo.Time).TotalMilliseconds <= 1000)
                        killer = LastAttackerInfo.Player;

                    if (PlayerData.GetData(killer) == null)
                        killer = Player.LocalPlayer;

                    Events.CallRemote("Players::OnDeath", killer);
                }
                else if (pData == null)
                {
                    var pos = Player.LocalPlayer.GetCoords(false);

                    pos.Z = Utils.Game.Misc.GetGroundZCoord(pos, false);

                    var heading = Player.LocalPlayer.GetHeading();

                    RAGE.Game.Network.NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z, heading, true, false, 0);

                    Player.LocalPlayer.Resurrect();

                    RAGE.Game.Misc.SetFadeOutAfterDeath(false);

                    Events.OnPlayerSpawn?.Invoke(null);
                }
            };

            Events.OnPlayerWeaponShot += (Vector3 targetPos, Player target, Events.CancelEventArgs cancel) =>
            {
                LastWeaponShot = World.Core.ServerTime;

                if (Game.Management.AntiCheat.Core.LastAllowedAmmo > 0)
                {
                    Game.Management.AntiCheat.Core.LastAllowedAmmo--;

                    Events.CallRemote("opws");
                }
            };

            Events.OnExplosion += OnExplosion;

            #region Events
            Events.Add("Weapon::TaskReload", async (object[] args) =>
            {
                if (Reloading)
                    return;

                Reloading = true;

                var weapon = Player.LocalPlayer.GetSelectedWeapon();
                var ammo = Player.LocalPlayer.GetAmmoInWeapon(weapon);

                Player.LocalPlayer.SetAmmoInClip(weapon, 0);
                Player.LocalPlayer.SetAmmo(weapon, ammo, 1);

                await RAGE.Game.Invoker.WaitAsync(2000);

                Reloading = false;
            });
            #endregion
        }

        #region Stuff

        public static void UpdateWeaponComponents(Player player, string strData)
        {
            var wcData = strData.Split('_');

            var wHash = uint.Parse(wcData[0]);

            var wTint = int.Parse(wcData[1]);

            var curGunData = WeaponList.Where(x => x.Hash == wHash).FirstOrDefault();

            if (player.GetWeaponTintIndex(wHash) != wTint)
                player.SetWeaponTintIndex(wHash, wTint);

            if (curGunData?.ComponentsHashes != null)
            {
                foreach (var x in curGunData.ComponentsHashes.Values)
                    player.RemoveWeaponComponentFrom(wHash, x);

                wcData.Skip(2).Where(x => x.Length > 0).Select(x => curGunData.GetComponentHash((WeaponComponentTypes)int.Parse(x))).ToList().ForEach((x) =>
                {
                    if (x is uint hash)
                    {
                        player.GiveWeaponComponentTo(curGunData.Hash, hash);
                    }
                });

                player.SetCurrentWeapon(wHash, true);
            }
        }

        public static void UpdateWeaponObjectComponents(int objHandle, uint wHash, string strData)
        {
            var wcData = strData.Split('_');

            var wTint = int.Parse(wcData[0]);

            var curGunData = WeaponList.Where(x => x.Hash == wHash).FirstOrDefault();

            if (RAGE.Game.Weapon.GetWeaponObjectTintIndex(objHandle) != wTint)
                RAGE.Game.Weapon.SetWeaponObjectTintIndex(objHandle, wTint);

            if (curGunData?.ComponentsHashes != null)
            {
                wcData.Skip(1).Where(x => x.Length > 0).Select(x => curGunData.GetComponentHash((WeaponComponentTypes)int.Parse(x))).ToList().ForEach((x) =>
                {
                    if (x is uint hash)
                    {
                        RAGE.Game.Weapon.GiveWeaponComponentToWeaponObject(objHandle, hash);
                    }
                });
            }
        }

        public static void UpdateWeapon()
        {
            var weapon = Player.LocalPlayer.GetSelectedWeapon();

            var curAmmo = Game.Management.AntiCheat.Core.LastAllowedAmmo;

            HUD.SetAmmo(curAmmo);

            Player.LocalPlayer.SetAmmo(weapon, curAmmo < 0 ? 9999 : Game.Management.AntiCheat.Core.LastAllowedAmmo, 1);

            // AutoReload
            if (curAmmo == 0 && RAGE.Game.Player.IsPlayerFreeAiming() && (RAGE.Game.Pad.IsControlPressed(32, 24) || RAGE.Game.Pad.IsDisabledControlPressed(32, 24)) && Settings.User.Interface.AutoReload)
            {
                if (PlayerActions.IsAnyActionActive(false, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.Crawl, PlayerActions.Types.Finger, PlayerActions.Types.OtherAnimation, PlayerActions.Types.Animation, PlayerActions.Types.FastAnimation, PlayerActions.Types.Scenario, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.IsAttachedTo))
                    return;

                if (!LastSentReload.IsSpam(2000, false, false))
                {
                    Events.CallRemote("Weapon::Reload");

                    LastSentReload = World.Core.ServerTime;
                }
            }
        }

        public static void ReloadWeapon()
        {
            if (Game.Management.AntiCheat.Core.LastAllowedAmmo < 0)
                return;

            var curWeapon = Player.LocalPlayer.GetSelectedWeapon();
            var weapProp = WeaponList.Where(x => x.Hash == curWeapon).FirstOrDefault();

            if (weapProp == null || !weapProp.HasAmmo)
                return;

            if (Utils.Misc.IsAnyCefActive())
                return;

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.Crawl, PlayerActions.Types.Finger, PlayerActions.Types.OtherAnimation, PlayerActions.Types.Animation, PlayerActions.Types.FastAnimation, PlayerActions.Types.Scenario, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.IsAttachedTo))
                return;

            if (LastSentReload.IsSpam(2000, false, false) || Inventory.LastSent.IsSpam(250, false, false) || LastWeaponShot.IsSpam(250, false, false))
                return;

            // add an check if weapon is full of ammo

            Events.CallRemote("Weapon::Reload");

            LastSentReload = World.Core.ServerTime;
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

            /*            if (Player.LocalPlayer.WasKilledByTakedown() || Player.LocalPlayer.GetConfigFlag(69, true) || Player.LocalPlayer.GetConfigFlag(70, true) || Player.LocalPlayer.GetConfigFlag(71, true))
                        {
                            //Player.LocalPlayer.SetProofs(false, false, false, false, true, false, false, false);

                            Player.LocalPlayer.SetConfigFlag(69, false);
                            Player.LocalPlayer.SetConfigFlag(70, false);
                            Player.LocalPlayer.SetConfigFlag(71, false);

                            Player.LocalPlayer.SetRealHealth(LastHealth);
                            Player.LocalPlayer.SetArmour(LastArmour);

                            return;
                        }*/

            if (curHealth <= 0 && !Player.LocalPlayer.IsDeadOrDying(true))
            {
                Player.LocalPlayer.SetRealHealth(0);
            }

            if (healthLoss == 0 && armourLoss == 0)
                return;

            OnDamage?.Invoke(healthLoss, armourLoss);

            if (armourLoss > 0)
                LastArmourLoss = World.Core.ServerTime;
        }

        public static void ArmourCheck(int healthLoss, int armourLoss)
        {
            if (armourLoss > 0 && Player.LocalPlayer.GetArmour() <= 0)
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

                var pData = PlayerData.GetData(Player.LocalPlayer);
                var sData = PlayerData.GetData(sourcePlayer);

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

                    var distance = Vector3.Distance(pPos, sPos);

                    if (distance > gun.MaxDistance)
                        return;

                    if (!pData.IsKnocked)
                        cancel.Cancel = false;

                    BodyPartTypes pType;

                    if (!PedParts.TryGetValue(boneIdx, out pType))
                        pType = BodyPartTypes.Limb;

                    var boneRatio = gun.GetBodyRatio(pType);

                    var customDamage = (int)((gun.BaseDamage - (gun.DistanceRatio * distance)) * boneRatio * (sourcePlayer.IsSittingInAnyVehicle() ? IN_VEHICLE_DAMAGE_COEF : 1f)) - (pData.IsKnocked ? 0 : 1);

                    if (customDamage >= 0)
                    {
                        LastAttackerInfo = (sourcePlayer, customDamage + 1, boneIdx, World.Core.ServerTime);

                        var isBullet = RAGE.Game.Weapon.GetWeaponDamageType(weaponHash) == 3;

                        if (isBullet)
                        {
                            if (!pData.IsWounded && !pData.IsKnocked)
                            {
                                var randRes = Utils.Misc.Random.NextDouble();

                                if (randRes <= Settings.App.Static.WeaponSystemWoundChance) // wounded chance
                                {
                                    Events.CallRemote("dmswme");
                                }
                            }
                        }

                        Player.LocalPlayer.ApplyDamageTo(customDamage, isBullet);

                        var hp = Player.LocalPlayer.GetRealHealth();
                        var arm = Player.LocalPlayer.GetArmour();

                        if (hp <= 20 && !pData.IsKnocked)
                        {
                            Player.LocalPlayer.SetInvincible(true);

                            Player.LocalPlayer.ApplyDamageTo(1, isBullet);

                            AsyncTask.Methods.Run(() => Player.LocalPlayer.SetInvincible(false), 25);
                        }
                        Utils.Console.OutputLimited($"Игрок: #{sData.CID} | Урон: {customDamage + 1} | Дистанция: {distance} | Часть тела: {boneIdx}", true, 1000);
                    }
                    else
                    {
                        cancel.Cancel = true;

                        return;
                    }
                }
                else if (targetEntity is Vehicle veh)
                {
                    if (!veh.Exists || veh.Controller?.Handle != Player.LocalPlayer.Handle)
                        return;

                    var vData = VehicleData.GetData(veh);

                    if (vData == null)
                        return;

                    if (!veh.GetCanBeDamaged())
                        return;

                    var pPos = veh.GetCoords(false);

                    var distance = Vector3.Distance(pPos, sPos);

                    if (distance > gun.MaxDistance)
                        return;

                    cancel.Cancel = false;

                    BodyPartTypes pType;

                    if (!VehicleParts.TryGetValue(boneIdx, out pType))
                        pType = BodyPartTypes.Limb;

                    var boneRatio = VehicleRatios[pType];

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

            if (targetEntity is Vehicle veh)
            {
                if (LastSentVehicleDamage.IsSpam(100, false, false))
                {
                    return;
                }
                else
                {
                    if (veh.Controller?.Handle == Player.LocalPlayer.Handle)
                    {
                        IncomingDamage(null, Player.LocalPlayer, veh, weaponHashLong, 0, (int)boneIdx, cancel);
                    }
                    else
                    {
                        cancel.Cancel = false;
                    }

                    LastSentVehicleDamage = World.Core.ServerTime;
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

                    LastSentPedDamage = World.Core.ServerTime;
                }
            }
        }

        private static void OnExplosion(Player player, uint explosionType, Vector3 position, Events.CancelEventArgs cancel)
        {
            cancel.Cancel = true;

            return;
        }
    }
}
