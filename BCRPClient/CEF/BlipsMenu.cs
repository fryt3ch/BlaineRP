using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPClient.CEF
{
    class BlipsMenu : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.BlipsMenu); }

        private static Additional.ExtraBlip TempBlip { get; set; }

        private static int LastEdited { get; set; }

        private static bool WasCreating { get; set; }

        private static bool FirstOpen { get; set; }

        private static AsyncTask UpdateTask { get; set; }

        public class LocalBlip
        {
            [JsonIgnore]
            public Additional.ExtraBlip Blip { get; set; }

            public int Sprite { get; set; }

            public string Name { get; set; }

            public byte Colour { get; set; }

            public float Scale { get; set; }

            public float Alpha { get; set; }

            public Vector3 Position { get; set; }

            public bool Enabled { get; set; }

            public bool ShortRange { get; set; }

            public LocalBlip(int Sprite, string Name, byte Colour, float Scale, float Alpha, Vector3 Position, bool ShortRange = false, bool Enabled = true)
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

            [JsonConstructor]
            public LocalBlip() { }

            public void Toggle(bool state)
            {
                Blip?.Destroy();

                if (state)
                {
                    Blip = new Additional.ExtraBlip((uint)Sprite, Position, Name, Scale, Colour, (int)Math.Floor(Alpha * 255), 0f, ShortRange, 0, 0f, uint.MaxValue);
                }
                else
                {
                    Blip = null;
                }
            }
        }

        private static List<int> TempBinds { get; set; }

        private static float CurrentScale { get; set; } = 1f;

        private static int CurrentSprite { get; set; } = 1;

        private static byte CurrentColour { get; set; } = 0;

        private static float CurrentAlpha { get; set; } = 1f;

        private static bool CurrentShortRange { get; set; } = false;

        private static bool CurrentUsePos { get; set; } = true;

        public BlipsMenu()
        {
            WasCreating = true;

            FirstOpen = true;

            LastEdited = -1;

            TempBinds = new List<int>();

            Events.Add("BlipMenu::Local::Create", (object[] args) =>
            {
                if (TempBlip == null || LastEdited != -1)
                    return;

                var name = (string)args[0];

                var allBlips = Settings.Other.LocalBlips;

                var blip = new LocalBlip(CurrentSprite, name, CurrentColour, CurrentScale, CurrentAlpha, CurrentUsePos ? Player.LocalPlayer.Position : GameEvents.WaypointPosition ?? Player.LocalPlayer.Position, false, true);

                TempBlip?.Destroy();

                TempBlip = null;

                allBlips.Add(blip);

                Settings.Other.LocalBlips = allBlips;

                blip.Toggle(true);

                CEF.Browser.Window.ExecuteJs("Blips.newBlip", blip.Name, blip.Enabled, blip.Colour, blip.Sprite, blip.Scale, blip.Alpha, !blip.ShortRange);
            });

            Events.Add("BlipMenu::Local::Edit", (object[] args) =>
            {
                var idx = (int)args[0];

                if (TempBlip == null || LastEdited == -1 || LastEdited != idx)
                    return;

                var name = (string)args[1];

                var allBlips = Settings.Other.LocalBlips;

                var blip = allBlips[idx];

                blip.Name = name;

                blip.Colour = CurrentColour;
                blip.Sprite = CurrentSprite;

                blip.Scale = CurrentScale;

                blip.Alpha = CurrentAlpha;
                blip.ShortRange = CurrentShortRange;

                TempBlip?.Destroy();

                TempBlip = null;

                LastEdited = -1;

                Settings.Other.LocalBlips = allBlips;

                allBlips[idx].Toggle(blip.Enabled);

                CEF.Browser.Window.ExecuteJs("Blips.editBlip", idx, blip.Name, blip.Colour, blip.Sprite, blip.Scale, blip.Alpha, !blip.ShortRange);
            });

            Events.Add("BlipMenu::Local::Delete", (object[] args) =>
            {
                var idx = (int)args[0];

                var allBlips = Settings.Other.LocalBlips;

                allBlips[idx].Toggle(false);

                allBlips.RemoveAt(idx);

                Settings.Other.LocalBlips = allBlips;

                CEF.Browser.Window.ExecuteJs("Blips.removeBlip", idx);

                if (TempBlip != null)
                {
                    LastEdited = -1;

                    TempBlip.Destroy();

                    TempBlip = null;
                }
            });

            Events.Add("BlipMenu::Local::Toggle", (object[] args) =>
            {
                var state = (bool)args[1];

                var allBlips = Settings.Other.LocalBlips;

                var idx1 = (int)args[0];
                var idx2 = (int)args[2];

                if (idx1 == -1 && idx2 == -1)
                {
                    if (TempBlip == null || LastEdited != -1)
                        return;

                    CurrentUsePos = state;

                    TempBlip.Position = (CurrentUsePos ? Player.LocalPlayer.Position : GameEvents.WaypointPosition) ?? Player.LocalPlayer.Position;
                }
                else
                {
                    if (idx1 == -1)
                    {
                        if (LastEdited == -1)
                            allBlips[idx2].Toggle(state);

                        allBlips[idx2].Enabled = state;

                        Settings.Other.LocalBlips = allBlips;
                    }
                    else
                    {
                        if (idx2 == -1)
                        {
                            if (TempBlip == null)
                                return;

                            CurrentShortRange = !state;

                            TempBlip.IsShortRange = CurrentShortRange;
                        }
                        else
                        {
                            if (LastEdited != idx1)
                                allBlips[idx1].Toggle(state);

                            allBlips[idx1].Enabled = state;

                            Settings.Other.LocalBlips = allBlips;
                        }
                    }
                }

                CEF.Browser.Window.ExecuteJs("Blips.setCheckBoxState", (int)args[2], state);
            });

            Events.Add("BlipMenu::Local::SetProperty", (object[] args) =>
            {
                if (!IsActive)
                    return;

                var aId = (string)args[0];

                var idx = (int)args[1];

                if (idx < 0)
                {
                    if (LastEdited != -1)
                    {
                        var blipT = Settings.Other.LocalBlips[LastEdited];

                        blipT.Toggle(blipT.Enabled);

                        LastEdited = -1;

                        TempBlip?.Destroy();

                        TempBlip = null;
                    }

                    if (TempBlip == null)
                    {
                        TempBlip = new Additional.ExtraBlip(0, Player.LocalPlayer.Position, "", 0f, 0, 0, 0f, false, 0, 0f, uint.MaxValue);

                        CurrentUsePos = true;
                    }
                }
                else
                {
                    if (TempBlip != null && LastEdited != idx)
                    {
                        TempBlip.Destroy();

                        TempBlip = null;
                    }

                    LastEdited = idx;

                    if (TempBlip == null)
                    {
                        Settings.Other.LocalBlips[idx].Toggle(false);

                        CurrentShortRange = Settings.Other.LocalBlips[idx].ShortRange;

                        TempBlip = new Additional.ExtraBlip(0, Settings.Other.LocalBlips[idx].Position, "", 0f, 0, 0, 0f, CurrentShortRange, 0, 0f, uint.MaxValue);
                    }
                }

                if (aId == "color")
                {
                    CurrentColour = Utils.ToByte(args[2]);
                }
                else if (aId == "icon")
                {
                    CurrentSprite = (int)args[2];
                }
                else if (aId == "size")
                {
                    CurrentScale = args[2] is int ? (float)(int)args[2] : (float)args[2];
                }
                else if (aId == "opacity")
                {
                    CurrentAlpha = args[2] is int ? (float)(int)args[2] : (float)args[2];
                }

                var blip = new Additional.ExtraBlip((uint)CurrentSprite, TempBlip.Position, "", CurrentScale, CurrentColour, (int)Math.Floor(CurrentAlpha * 255), 0f, TempBlip.IsShortRange, 0, 0f, uint.MaxValue);

                TempBlip.Destroy();

                TempBlip = blip;
            });


            Events.Add("BlipMenu::Close", (object[] args) => Close(false));
        }

        public static void Show()
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CEF.Browser.Switch(Browser.IntTypes.BlipsMenu, true);

            if (FirstOpen)
            {
                var data = Settings.Other.LocalBlips.Select(x => new object[] { x.Name, x.Enabled, x.Colour, x.Sprite, x.Scale, x.Alpha, !x.ShortRange });

                CEF.Browser.Window.ExecuteJs("Blips.fillBlips", new object[] { data });

                FirstOpen = false;
            }

            if (WasCreating)
            {
                TempBlip = new Additional.ExtraBlip((uint)CurrentSprite, CurrentUsePos ? Player.LocalPlayer.Position : GameEvents.WaypointPosition ?? Player.LocalPlayer.Position, "", CurrentScale, CurrentColour, (int)Math.Floor(CurrentAlpha * 255), 0f, false, 0, 0f, uint.MaxValue);
            }
            else if (LastEdited != -1)
            {
                TempBlip = new Additional.ExtraBlip((uint)CurrentSprite, Settings.Other.LocalBlips[LastEdited].Position, "", CurrentScale, CurrentColour, (int)Math.Floor(CurrentAlpha * 255), 0f, CurrentShortRange, 0, 0f, uint.MaxValue);
            }

            CEF.Cursor.Show(true, true);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));

            var playerBlip = RAGE.Game.Ui.GetMainPlayerBlipId();
            var waypointBlip = Utils.GetWaypointBlip();

            if (RAGE.Game.Ui.DoesBlipExist(playerBlip))
                RAGE.Game.Ui.SetBlipDisplay(playerBlip, 0);

            if (RAGE.Game.Ui.DoesBlipExist(waypointBlip))
                RAGE.Game.Ui.SetBlipDisplay(waypointBlip, 0);

            UpdateTask?.Cancel();

            UpdateTask = new AsyncTask(() =>
            {
                if (TempBlip != null && LastEdited == -1)
                {
                    TempBlip.Position = CurrentUsePos ? Player.LocalPlayer.Position : GameEvents.WaypointPosition ?? Player.LocalPlayer.Position;
                }

            }, 500, true, 0);

            UpdateTask.Run();
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.Browser.Switch(Browser.IntTypes.BlipsMenu, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                KeyBinds.Unbind(x);

            TempBinds.Clear();

            if (TempBlip != null)
            {
                TempBlip?.Destroy();

                TempBlip = null;

                if (LastEdited == -1)
                    WasCreating = true;
                else
                    WasCreating = false;
            }
            else
            {
                WasCreating = false;
            }

            var playerBlip = RAGE.Game.Ui.GetMainPlayerBlipId();
            var waypointBlip = Utils.GetWaypointBlip();

            if (RAGE.Game.Ui.DoesBlipExist(playerBlip))
                RAGE.Game.Ui.SetBlipDisplay(playerBlip, 2);

            if (RAGE.Game.Ui.DoesBlipExist(waypointBlip))
                RAGE.Game.Ui.SetBlipDisplay(waypointBlip, 2);

            if (UpdateTask != null)
            {
                UpdateTask.Cancel();

                UpdateTask = null;
            }
        }
    }
}
