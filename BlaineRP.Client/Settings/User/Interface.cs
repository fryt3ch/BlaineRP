namespace BlaineRP.Client.Settings.User
{
    public static class Interface
    {
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

        public static bool UseServerTime { get => _UseServerTime; set { if (value != _UseServerTime) Additional.Storage.SetData("Settings::Interface::UseServerTime", value); _UseServerTime = value; CEF.Menu.UpdateToggle("sett-time", value); CEF.HUD.UpdateTime(); } }
        public static bool HideHints { get => _HideHints; set { if (value != _HideHints) Additional.Storage.SetData("Settings::Interface::HideHints", value); _HideHints = value; CEF.Menu.UpdateToggle("sett-help", value); CEF.HUD.ToggleHints(!value); CEF.Inventory.SwitchHint(!value); } }
        public static bool HideNames { get => _HideNames; set { if (value != _HideNames) Additional.Storage.SetData("Settings::Interface::HideNames", value); _HideNames = value; CEF.Menu.UpdateToggle("sett-names", value); NameTags.Enabled = !value; } }
        public static bool HideCID { get => _HideCID; set { if (value != _HideCID) Additional.Storage.SetData("Settings::Interface::HideCID", value); _HideCID = value; CEF.Menu.UpdateToggle("sett-cid", value); } }
        public static bool HideHUD { get => _HideHUD; set { if (value != _HideHUD) Additional.Storage.SetData("Settings::Interface::HideHUD", value); _HideHUD = value; CEF.Menu.UpdateToggle("sett-hud", value); CEF.HUD.ShowHUD(!value); } }
        public static bool HideQuest { get => _HideQuest; set { if (value != _HideQuest) Additional.Storage.SetData("Settings::Interface::HideQuest", value); CEF.Menu.UpdateToggle("sett-quest", value); _HideQuest = value; CEF.HUD.EnableQuest(!value); } }

        public static bool HideInteractionBtn { get => _HideInteractionBtn; set { if (value != _HideInteractionBtn) Additional.Storage.SetData("Settings::Interface::HideInteractionBtn", value); _HideInteractionBtn = value; CEF.Menu.UpdateToggle("sett-interact", value); Interaction.EnabledVisual = !value; } }
        public static bool HideIOGNames { get => _HideIOGNames; set { if (value != _HideIOGNames) Additional.Storage.SetData("Settings::Interface::HideIOGNames", value); _HideIOGNames = value; CEF.Menu.UpdateToggle("sett-items", value); } }
        public static bool AutoReload { get => _AutoReload; set { if (value != _AutoReload) Additional.Storage.SetData("Settings::Interface::AutoReload", value); _AutoReload = value; CEF.Menu.UpdateToggle("sett-reload", value); } }
        public static bool FingerOn { get => _FingerOn; set { if (value != _FingerOn) Additional.Storage.SetData("Settings::Interface::FingerOn", value); _FingerOn = value; CEF.Menu.UpdateToggle("sett-finger", value); } }
    }
}
