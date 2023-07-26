using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;
using Camera = BlaineRP.Client.Game.UI.CEF.Phone.Apps.Camera;
using Chat = BlaineRP.Client.Game.UI.CEF.Chat;
using Interaction = BlaineRP.Client.Game.Management.Interaction;

namespace BlaineRP.Client.Game.Input
{
    partial class Core
    {
        public static void LoadAll()
        {
            // Toggle Chat Input
            Add(new ExtraBind(BindTypes.ChatInput,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                    {
                        Chat.ShowInput(true);
                    }
                },
                true,
                true) { Description = "Открыть чат" });

            // Open Menu
            Add(new ExtraBind(BindTypes.Menu,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        HUD.Menu.Switch(true, null);
                },
                true,
                true) { Description = "Меню" });

            // Toggle Radar Size
            Add(new ExtraBind(BindTypes.RadarSize,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        MiniMap.Toggle();
                },
                true,
                true) { Description = "Масштаб миникарты" });

            // Use Micro Start
            Add(new ExtraBind(BindTypes.MicrophoneOn,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(false, true))
                        Game.Management.Microphone.Core.Start();
                },
                true,
                true) { Description = "Голосовой чат" });

            // Use Micro Stop
            Add(new ExtraBind(BindTypes.MicrophoneOff,
                () => { Game.Management.Microphone.Core.Stop(); },
                false,
                false,
                BindTypes.MicrophoneOn));

            // Interaction
            Add(new ExtraBind(BindTypes.Interaction,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Game.UI.CEF.Interaction.TryShowMenu();
                },
                true,
                true) { Description = "Меню взаимодействия" });

            // Phone
            Add(new ExtraBind(BindTypes.Phone,
                () =>
                {
                    if (!Phone.Toggled)
                    {
                        if (Utils.Misc.CanShowCEF(true, true))
                            Phone.Toggle();
                    }
                    else
                    {
                        Phone.Toggle();
                    }
                },
                true,
                true) { Description = "Телефон" });

            // Finger Point Start
            Add(new ExtraBind(BindTypes.FingerPointStart,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Finger.Start();
                },
                true,
                true) { Description = "Показать пальцем" });

            // Finger Point Stop
            Add(new ExtraBind(BindTypes.FingerPointStop,
                () => { Finger.Stop(); },
                false,
                false,
                BindTypes.FingerPointStart));

            // Crouch
            Add(new ExtraBind(BindTypes.Crouch,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Crouch.Toggle();
                },
                true,
                true) { Description = "Присесть" });

            // Crawl
            Add(new ExtraBind(BindTypes.Crawl,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Crawl.Toggle();
                },
                true,
                true) { Description = "Ползти" });

            // Engine Toggle
            Add(new ExtraBind(BindTypes.Engine,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleEngine(Player.LocalPlayer.Vehicle, null);
                },
                true,
                true) { Description = "Двигатель Т/С" });

            // Cruise Control
            Add(new ExtraBind(BindTypes.CruiseControl,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleCruiseControl(false);
                },
                true,
                true) { Description = "Круиз-контроль" });

            // Auto Pilot
            Add(new ExtraBind(BindTypes.AutoPilot,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleAutoPilot(null);
                },
                true,
                true) { Description = "Автопилот" });

            // Vehicle Doors Lock Toggle
            Add(new ExtraBind(BindTypes.DoorsLock,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.Lock(null, Interaction.CurrentEntity as Vehicle);
                },
                true,
                true) { Description = "Блокировка Т/С" });

            // Vehicle Look in Trunk
            Add(new ExtraBind(BindTypes.TrunkLook,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true) && Interaction.CurrentEntity is Vehicle veh)
                        Vehicles.ShowContainer(veh);
                },
                true,
                true) { Description = "Смотреть багажник" });

            // Seat Belt Toggle
            Add(new ExtraBind(BindTypes.Belt,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleBelt();
                },
                true,
                true) { Description = "Пристегнуться" });

            // Left Arrow Veh
            Add(new ExtraBind(BindTypes.LeftArrow,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 1);
                },
                true,
                true) { Description = "Левый поворотник" });

            // Right Arrow Veh
            Add(new ExtraBind(BindTypes.RightArrow,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 0);
                },
                true,
                true) { Description = "Правый поворотник" });

            // Both Arrows Veh
            Add(new ExtraBind(BindTypes.BothArrows,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleIndicator(Player.LocalPlayer.Vehicle, 2);
                },
                true,
                true) { Description = "Аварийка" });

            // Lights Veh
            Add(new ExtraBind(BindTypes.Lights,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleLights(Player.LocalPlayer.Vehicle);
                },
                true,
                true) { Description = "Фары" });

            // Toggle HUD 
            Add(new ExtraBind(BindTypes.HUD,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(false, true))
                    {
                        Settings.User.Interface.HideHUD = !Settings.User.Interface.HideHUD;
                    }
                },
                true,
                true) { Description = "HUD" });

            // Reload Voice Chat 
            Add(new ExtraBind(BindTypes.MicrophoneReload,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(false, true))
                        Game.Management.Microphone.Core.Reload();
                },
                true,
                true) { Description = "Перезапустить голосовой чат" });

            // Inventory Open 
            Add(new ExtraBind(BindTypes.Inventory,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.Show(Inventory.Types.Inventory);
                },
                true,
                true) { Description = "Инвентарь" });

            // Take Item on Ground
            Add(new ExtraBind(BindTypes.TakeItem,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                    {
                        if (Game.World.Core.ClosestItemOnGround == null)
                            return;

                        Game.World.Core.ClosestItemOnGround.TakeItem();
                    }
                },
                true,
                true) { Description = "Подобрать предмет" });

            // ReloadWeapon
            Add(new ExtraBind(BindTypes.ReloadWeapon,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Game.Management.Weapons.Core.ReloadWeapon();
                },
                true,
                true) { Description = "Перезарядить оружие" });

            // Whistle
            Add(new ExtraBind(BindTypes.Whistle,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Game.Animations.Core.PlayFastSync(FastTypes.Whistle);
                },
                true,
                true) { Description = "Свистеть" });

            // Whistle
            Add(new ExtraBind(BindTypes.SendCoordsToDriver,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.SendCoordsToDriver();
                },
                true,
                true) { Description = "Передать метку водителю" });

            Add(new ExtraBind(BindTypes.Animations,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        UI.CEF.Animations.Open();
                },
                true,
                true) { Description = "Анимации" });

            Add(new ExtraBind(BindTypes.CancelAnimation,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        UI.CEF.Animations.Cancel();
                },
                true,
                true) { Description = "Отмена анимации" });

            Add(new ExtraBind(BindTypes.Help,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Menu.Show(Menu.SectionTypes.Help);
                },
                true,
                true) { Description = "Помощь" });

            Add(new ExtraBind(BindTypes.BlipsMenu,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                    {
                        BlipsMenu.Show();
                    }
                },
                true,
                true) { Description = "Меню меток" });

            Add(new ExtraBind(BindTypes.AnchorBoat,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                    {
                        Vehicles.ToggleAnchor();
                    }
                },
                true,
                true) { Description = "Якорь (для лодок)" });

            Add(new ExtraBind(BindTypes.FlashlightToggle,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                    {
                        Events.CallRemote("Players::FLT");
                    }
                },
                true,
                false) { Description = "Фонарик (вкл/выкл)" }); // deprecated

            Add(new ExtraBind(BindTypes.TakeScreenshot,
                () => { Camera.SavePicture(false, false, true); },
                true,
                true) { Description = "Сделать скриншот" });

            Add(new ExtraBind(BindTypes.ExtraAction0,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        CurrentExtraAction0?.Invoke();
                },
                true,
                true) { Description = "Быстрое действие 1" });

            Add(new ExtraBind(BindTypes.ExtraAction1,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        CurrentExtraAction1?.Invoke();
                },
                true,
                true) { Description = "Быстрое действие 2" });

            Add(new ExtraBind(BindTypes.EnterVehicle,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.TryEnterVehicle(Interaction.CurrentEntity as Vehicle, -1);
                },
                true,
                false,
                BindTypes.None,
                false));

            Add(new ExtraBind(BindTypes.PlaneLandingGear,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Vehicles.ToggleLandingGearState(Player.LocalPlayer.Vehicle);
                },
                true,
                false,
                BindTypes.None,
                false));

            // Inventory Binds
            Add(new ExtraBind(BindTypes.weapon0,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "weapon", 0);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.weapon1,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "weapon", 1);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.weapon2,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "weapon", 2);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets0,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 0);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets1,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 1);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets2,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 2);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets3,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 3);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets4,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 4);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets5,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 5);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets6,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 6);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets7,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 7);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets8,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 8);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets9,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 9);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets10,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 10);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets11,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 11);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets12,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 12);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets13,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 13);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets14,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 14);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets15,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 15);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets16,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 16);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets17,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 17);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets18,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 18);
                },
                true,
                true,
                BindTypes.None,
                true));

            Add(new ExtraBind(BindTypes.pockets19,
                () =>
                {
                    if (Utils.Misc.CanShowCEF(true, true))
                        Inventory.BindedAction(5, "pockets", 19);
                },
                true,
                true,
                BindTypes.None,
                true));
        }
    }
}