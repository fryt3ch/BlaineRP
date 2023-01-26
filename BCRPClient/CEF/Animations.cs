using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class Animations : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(CEF.Browser.IntTypes.Animations); }

        public static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        private static Queue<(string, object[])> Queue;

        public enum AnimSectionTypes
        {
            // Animations
            Social = 0, Dialogs, Reactions, SeatLie, Sport, Indecent, StandPoses, Dances, Situative, WithWeapon,
        }

        public Animations()
        {
            LastSent = DateTime.Now;

            TempBinds = new List<int>();
            Queue = new Queue<(string, object[])>();

            Events.Add("Anims::Menu::Choose", (object[] args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var id = (string)args[1];
                var state = !(bool)args[2];

                var prefix = id.Substring(0, 2);
                id = id.Remove(0, 2);

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (prefix == "a-")
                {
                    var anim = (Sync.Animations.OtherTypes)Enum.Parse(typeof(Sync.Animations.OtherTypes), id);

                    if (anim == pData.OtherAnim)
                    {
                        Events.CallRemote("Players::SetAnim", (int)Sync.Animations.OtherTypes.None);
                    }
                    else
                    {
                        if (!Utils.CanDoSomething(Utils.Actions.Knocked, Utils.Actions.Frozen, Utils.Actions.Cuffed, Utils.Actions.PushingVehicle, Utils.Actions.Animation, Utils.Actions.FastAnimation, Utils.Actions.InVehicle, Utils.Actions.Shooting, Utils.Actions.Reloading, Utils.Actions.Climbing, Utils.Actions.Falling, Utils.Actions.Ragdoll, Utils.Actions.Jumping, Utils.Actions.NotOnFoot))
                            return;

                        Events.CallRemote("Players::SetAnim", (int)anim);

                        LastSent = DateTime.Now;
                    }
                    
                }
                else if (prefix == "s-")
                {
                    var scenario = (Sync.Animations.ScenarioTypes)Enum.Parse(typeof(Sync.Animations.ScenarioTypes), id);
                }
                else if (prefix == "e-")
                {
                    var emotion = (Sync.Animations.EmotionTypes)Enum.Parse(typeof(Sync.Animations.EmotionTypes), id);

                    if (pData.Emotion == emotion)
                        return;

                    Events.CallRemote("Players::SetEmotion", (int)emotion);
                }
                else if (prefix == "w-")
                {
                    var walkstyle = (Sync.Animations.WalkstyleTypes)Enum.Parse(typeof(Sync.Animations.WalkstyleTypes), id);

                    if (pData.Walkstyle == walkstyle)
                        return;

                    Events.CallRemote("Players::SetWalkstyle", (int)walkstyle);
                }
            });

            Events.Add("Anims::Menu::Fav", (object[] args) =>
            {
                var id = (string)args[0];
                bool state = !(bool)args[1];

                var prefix = id.Substring(0, 2);

                if (prefix != "a-" && prefix != "s-")
                    return;

                var favs = Settings.Other.FavoriteAnimations;

                if (state)
                    favs.Add(id);
                else
                    favs.Remove(id);

                Settings.Other.FavoriteAnimations = favs;
            });
        }

        public static void Open()
        {
            if (IsActive || Utils.IsAnyCefActive())
                return;

            CEF.Cursor.Show(true, true);

            CEF.Browser.Switch(Browser.IntTypes.Animations, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));

            GameEvents.Render -= CEF.Animations.Render;

            var cancelAnimKb = KeyBinds.Get(KeyBinds.Types.CancelAnimation);

            if (!cancelAnimKb.IsDisabled)
                KeyBinds.Get(KeyBinds.Types.CancelAnimation).Disable();

            if (Queue.Count > 0)
            {
                (string Function, object[] Args) cmd;

                while (Queue.TryDequeue(out cmd))
                    CEF.Browser.Window.ExecuteJs(cmd.Function, cmd.Args);
            }
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Cursor.Show(false, false);

            CEF.Browser.Switch(Browser.IntTypes.Animations, false);

            foreach (var x in TempBinds.ToList())
                RAGE.Input.Unbind(x);

            TempBinds.Clear();

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.OtherAnim != Sync.Animations.OtherTypes.None)
            {
                GameEvents.Render -= CEF.Animations.Render;
                GameEvents.Render += CEF.Animations.Render;

                KeyBinds.Get(KeyBinds.Types.CancelAnimation).Enable();
            }
        }

        public static void Cancel()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.OtherAnim == Sync.Animations.OtherTypes.None)
                return;

            if (LastSent.IsSpam(500, false, false))
                return;

            Events.CallRemote("Players::SetAnim", (int)Sync.Animations.OtherTypes.None);
        }

        public static void ToggleAnim(string animId, bool state)
        {
            if (!IsActive)
                Queue.Enqueue(("Anims.colorAnim", new object[] { animId, state }));
            else
                CEF.Browser.Window.ExecuteJs("Anims.colorAnim", animId, state);
        }

        public static void Render()
        {
            Utils.DrawText(string.Format(Locale.General.Animations.CancelText, KeyBinds.Get(KeyBinds.Types.CancelAnimation).GetKeyString()), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
        }

        public static async System.Threading.Tasks.Task Load()
        {
            await CEF.Browser.Render(Browser.IntTypes.Animations, true);

            var list = new object[]
            {
                // Animations
                new object[]
                {
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Social].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Social].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Dialogs].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Dialogs].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Reactions].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Reactions].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.SeatLie].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.SeatLie].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Sport].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Sport].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Indecent].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Indecent].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.StandPoses].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.StandPoses].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Dances].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Dances].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.Situative].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.Situative].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                    new object[] { Locale.General.Animations.Anims[AnimSectionTypes.WithWeapon].SectionName, Locale.General.Animations.Anims[AnimSectionTypes.WithWeapon].Names.Select(x => new object[] { "a-" + x.Key.ToString(), x.Value }) },
                },

                // Scenarios
                new object[]
                {

                },

                // Walkstyles
                new object[]
                {
                    Locale.General.Animations.Walkstyles.Select(x => new object[] { "w-" + x.Key.ToString(), x.Value }),
                },

                // Emotions
                new object[]
                {
                    Locale.General.Animations.Emotions.Select(x => new object[] { "e-" + x.Key.ToString(), x.Value }),
                },

                // Fav
                Settings.Other.FavoriteAnimations,
            };

            CEF.Browser.Window.ExecuteJs("Anims.draw", new object[] { list });
        }
    }
}
