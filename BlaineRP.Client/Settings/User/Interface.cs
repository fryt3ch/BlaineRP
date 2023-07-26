using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Settings.User
{
    public static class Interface
    {
        private static bool _UseServerTime;
        private static bool _HideHints;
        private static bool _HideNames;
        private static bool _HideCID;
        private static bool _HideHUD;
        private static bool _HideQuest;

        private static bool _HideInteractionBtn;
        private static bool _HideIOGNames;
        private static bool _AutoReload;
        private static bool _FingerOn;

        public static bool UseServerTime
        {
            get => _UseServerTime;
            set
            {
                if (value != _UseServerTime)
                    RageStorage.SetData("Settings::Interface::UseServerTime", value);
                _UseServerTime = value;
                Menu.UpdateToggle("sett-time", value);
                HUD.UpdateTime();
            }
        }

        public static bool HideHints
        {
            get => _HideHints;
            set
            {
                if (value != _HideHints)
                    RageStorage.SetData("Settings::Interface::HideHints", value);
                _HideHints = value;
                Menu.UpdateToggle("sett-help", value);
                HUD.ToggleHints(!value);
                Inventory.SwitchHint(!value);
            }
        }

        public static bool HideNames
        {
            get => _HideNames;
            set
            {
                if (value != _HideNames)
                    RageStorage.SetData("Settings::Interface::HideNames", value);
                _HideNames = value;
                Menu.UpdateToggle("sett-names", value);
                NameTags.Enabled = !value;
            }
        }

        public static bool HideCID
        {
            get => _HideCID;
            set
            {
                if (value != _HideCID)
                    RageStorage.SetData("Settings::Interface::HideCID", value);
                _HideCID = value;
                Menu.UpdateToggle("sett-cid", value);
            }
        }

        public static bool HideHUD
        {
            get => _HideHUD;
            set
            {
                if (value != _HideHUD)
                    RageStorage.SetData("Settings::Interface::HideHUD", value);
                _HideHUD = value;
                Menu.UpdateToggle("sett-hud", value);
                HUD.ShowHUD(!value);
            }
        }

        public static bool HideQuest
        {
            get => _HideQuest;
            set
            {
                if (value != _HideQuest)
                    RageStorage.SetData("Settings::Interface::HideQuest", value);
                Menu.UpdateToggle("sett-quest", value);
                _HideQuest = value;
                HUD.EnableQuest(!value);
            }
        }

        public static bool HideInteractionBtn
        {
            get => _HideInteractionBtn;
            set
            {
                if (value != _HideInteractionBtn)
                    RageStorage.SetData("Settings::Interface::HideInteractionBtn", value);
                _HideInteractionBtn = value;
                Menu.UpdateToggle("sett-interact", value);
                Game.Management.Interaction.EnabledVisual = !value;
            }
        }

        public static bool HideIOGNames
        {
            get => _HideIOGNames;
            set
            {
                if (value != _HideIOGNames)
                    RageStorage.SetData("Settings::Interface::HideIOGNames", value);
                _HideIOGNames = value;
                Menu.UpdateToggle("sett-items", value);
            }
        }

        public static bool AutoReload
        {
            get => _AutoReload;
            set
            {
                if (value != _AutoReload)
                    RageStorage.SetData("Settings::Interface::AutoReload", value);
                _AutoReload = value;
                Menu.UpdateToggle("sett-reload", value);
            }
        }

        public static bool FingerOn
        {
            get => _FingerOn;
            set
            {
                if (value != _FingerOn)
                    RageStorage.SetData("Settings::Interface::FingerOn", value);
                _FingerOn = value;
                Menu.UpdateToggle("sett-finger", value);
            }
        }

        public static class Default
        {
            public static bool UseServerTime = true;
            public static bool HideHints = false;
            public static bool HideNames = false;
            public static bool HideCID = false;
            public static bool HideHUD = false;
            public static bool HideQuest = false;

            public static bool HideInteractionBtn = false;
            public static bool HideIOGNames = false;
            public static bool AutoReload = true;
            public static bool FingerOn = true;
        }
    }
}