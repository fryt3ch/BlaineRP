using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    class BlipsMenu : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.BlipsMenu); }

        public class LocalBlip
        {
            public RAGE.Elements.Blip Blip { get; set; }

            public int Sprite { get; set; }

            public string Name { get; set; }

            public int Colour { get; set; }

            public float Scale { get; set; }

            public float Alpha { get; set; }

            public Vector3 Position { get; set; }

            public bool Enabled { get; set; }

            public bool ShortRange { get; set; }

            public LocalBlip(int Sprite, string Name, int Colour, float Scale, float Alpha, Vector3 Position, bool ShortRange = false, bool Enabled = true)
            {
                this.Sprite = Sprite;
                this.Name = Name;
                this.Colour = Colour;
                this.Scale = Scale;
                this.Alpha = Alpha;
                this.Position = Position;
                this.ShortRange = ShortRange;
                this.Enabled = Enabled;
            }

            public LocalBlip()
            {

            }

            public void Toggle(bool state)
            {
                if (state)
                {
                    Blip = new RAGE.Elements.Blip((uint)Sprite, Position, Name, Scale, Colour, (int)Math.Floor(Alpha * 255), 0f, false, 0, 0f, Settings.MAIN_DIMENSION);
                }
                else
                {
                    Blip?.Destroy();
                }
            }
        }

        private static List<int> TempBinds { get; set; }

        private static DateTime LastToggled;

        private static bool UseCurrentPosition { get; set; }

        public BlipsMenu()
        {
            UseCurrentPosition = true;

            LastToggled = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("BlipMenu::Local::Create", (object[] args) =>
            {
                JObject obj = JObject.Parse((string)args[0]);

                var sprite = int.Parse(obj["icon"].ToObject<string>().Replace("Blip_", ""));
                var colour = int.Parse(obj["color"].ToObject<string>().Replace("Blip_colour_", ""));

                var scale = obj["size"].ToObject<float>();
                var alpha = obj["opacity"].ToObject<float>();

                var name = obj["name"].ToObject<string>();

                var allBlips = Settings.Other.LocalBlips;

                var blip = new LocalBlip(sprite, name, colour, scale, alpha, (UseCurrentPosition ? Player.LocalPlayer.Position : GameEvents.WaypointPosition) ?? Player.LocalPlayer.Position, false, true);

                allBlips.Add(blip);

                Settings.Other.LocalBlips = allBlips;

                blip.Toggle(true);
            });

            Events.Add("BlipMenu::Local::Edit", (object[] args) =>
            {
                var idx = (int)args[0];

                JObject obj = JObject.Parse((string)args[1]);

                var sprite = int.Parse(obj["icon"].ToObject<string>().Replace("Blip_", ""));
                var colour = int.Parse(obj["color"].ToObject<string>().Replace("Blip_colour_", ""));

                var scale = obj["size"].ToObject<float>();
                var alpha = obj["opacity"].ToObject<float>();

                var name = obj["name"].ToObject<string>();

                var allBlips = Settings.Other.LocalBlips;

                var blip = allBlips[idx];

                blip.Sprite = sprite;
                blip.Colour = colour;
                blip.Scale = scale;
                blip.Alpha = alpha;

                blip.Name = name;

                Settings.Other.LocalBlips = allBlips;

                blip.Toggle(false);
                blip.Toggle(blip.Enabled);
            });

            Events.Add("BlipMenu::Local::Toggle", (object[] args) =>
            {
                var state = (bool)args[0];

                var idx = (int)args[1];

                var allBlips = Settings.Other.LocalBlips;

                allBlips[idx]?.Toggle(state);

                Settings.Other.LocalBlips = allBlips;
            });

            Events.Add("BlipMenu::Local::Delete", (object[] args) =>
            {
                var idx = (int)args[0];

                var allBlips = Settings.Other.LocalBlips;

                allBlips[idx]?.Toggle(false);

                Settings.Other.LocalBlips = allBlips;
            });

            Events.Add("BlipMenu::Local::SetProperty", (object[] args) =>
            {

            });


            Events.Add("BlipMenu::Close", (object[] args) => Close(false));
        }

        public static async void Show()
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.BlipsMenu, true, true);

            var data = Settings.Other.LocalBlips.Select(x => new object[] { x.Name, x.Enabled, x.Colour, x.Sprite, x.Scale, x.Alpha });

            CEF.Browser.Window.ExecuteJs("Blips.fillBlips", new object[] { data });

            CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));

            LastToggled = DateTime.Now;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (!ignoreTimeout && LastToggled.IsSpam(500, false, false))
                return;

            CEF.Browser.Render(Browser.IntTypes.BlipsMenu, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();
        }
    }
}
