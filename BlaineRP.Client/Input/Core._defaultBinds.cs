using System.Collections.Generic;
using BlaineRP.Client.Input.Enums;

namespace BlaineRP.Client.Input
{
    partial class Core
    {
        private static readonly Dictionary<BindTypes, RAGE.Ui.VirtualKeys[]> _defaultBinds =
            new Dictionary<BindTypes, RAGE.Ui.VirtualKeys[]>()
            {
                { BindTypes.Cursor, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.OEM3 } },
                { BindTypes.RadarSize, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Z } },
                { BindTypes.ChatInput, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.T } },
                { BindTypes.FingerPointStart, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
                { BindTypes.FingerPointStop, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
                { BindTypes.Crouch, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Control } },
                {
                    BindTypes.Crawl,
                    new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Control, RAGE.Ui.VirtualKeys.Shift }
                },
                { BindTypes.MicrophoneOn, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Tab } },
                { BindTypes.MicrophoneOff, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Tab } },
                { BindTypes.MicrophoneReload, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F3 } },
                { BindTypes.DoorsLock, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.L } },
                { BindTypes.Engine, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N } },
                { BindTypes.LeftArrow, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad4 } },
                { BindTypes.RightArrow, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad6 } },
                { BindTypes.BothArrows, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad5 } },
                { BindTypes.Lights, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Numpad8 } },
                { BindTypes.Belt, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.J } },
                { BindTypes.CruiseControl, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.K } },
                { BindTypes.AutoPilot, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.X } },
                { BindTypes.Interaction, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.G } },
                { BindTypes.Phone, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.P } },
                { BindTypes.Menu, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.M } },
                { BindTypes.Help, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F10 } },
                { BindTypes.HUD, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.TrunkLook, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.Inventory, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.I } },
                { BindTypes.TakeItem, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.E } },
                { BindTypes.ReloadWeapon, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.R } },
                { BindTypes.Whistle, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.Animations, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.U } },
                { BindTypes.CancelAnimation, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.C } },
                { BindTypes.BlipsMenu, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.AnchorBoat, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.Z } },
                { BindTypes.SendCoordsToDriver, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.O } },
                { BindTypes.FlashlightToggle, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.TakeScreenshot, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.ExtraAction0, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.ExtraAction1, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.EnterVehicle, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.F, } },
                { BindTypes.PlaneLandingGear, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.X, } },
                { BindTypes.weapon0, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N1 } },
                { BindTypes.weapon1, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N2 } },
                { BindTypes.weapon2, new RAGE.Ui.VirtualKeys[] { RAGE.Ui.VirtualKeys.N3 } },
                { BindTypes.pockets0, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets1, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets2, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets3, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets4, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets5, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets6, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets7, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets8, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets9, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets10, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets11, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets12, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets13, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets14, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets15, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets16, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets17, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets18, new RAGE.Ui.VirtualKeys[] { } },
                { BindTypes.pockets19, new RAGE.Ui.VirtualKeys[] { } },
            };
    }
}