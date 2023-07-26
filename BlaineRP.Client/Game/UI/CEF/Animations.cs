using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Animations
    {
        public enum AnimSectionTypes
        {
            // Animations
            Social = 0,
            Dialogs,
            Reactions,
            SeatLie,
            Sport,
            Indecent,
            StandPoses,
            Dances,
            Situative,
            WithWeapon,
        }

        public static DateTime LastSent;

        private static Queue<(string, object[])> Queue;

        public Animations()
        {
            LastSent = World.Core.ServerTime;

            TempBinds = new List<int>();
            Queue = new Queue<(string, object[])>();

            Events.Add("Anims::Menu::Choose",
                (object[] args) =>
                {
                    if (LastSent.IsSpam(500, false, false))
                        return;

                    var id = (string)args[1];
                    bool state = !(bool)args[2];

                    string prefix = id.Substring(0, 2);
                    id = id.Remove(0, 2);

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (prefix == "a-")
                    {
                        var anim = (OtherTypes)Enum.Parse(typeof(OtherTypes), id);

                        if (anim == pData.OtherAnim)
                        {
                            Events.CallRemote("Players::SetAnim", (int)OtherTypes.None);
                        }
                        else
                        {
                            if (PlayerActions.IsAnyActionActive(true,
                                    PlayerActions.Types.Knocked,
                                    PlayerActions.Types.Frozen,
                                    PlayerActions.Types.Cuffed,
                                    PlayerActions.Types.PushingVehicle,
                                    PlayerActions.Types.Animation,
                                    PlayerActions.Types.Scenario,
                                    PlayerActions.Types.FastAnimation,
                                    PlayerActions.Types.InVehicle,
                                    PlayerActions.Types.Shooting,
                                    PlayerActions.Types.Reloading,
                                    PlayerActions.Types.Climbing,
                                    PlayerActions.Types.Falling,
                                    PlayerActions.Types.Ragdoll,
                                    PlayerActions.Types.Jumping,
                                    PlayerActions.Types.NotOnFoot,
                                    PlayerActions.Types.IsSwimming,
                                    PlayerActions.Types.HasItemInHands,
                                    PlayerActions.Types.IsAttachedTo
                                ))
                                return;

                            Events.CallRemote("Players::SetAnim", (int)anim);

                            LastSent = World.Core.ServerTime;
                        }
                    }
                    else if (prefix == "s-")
                    {
                        var scenario = (ScenarioTypes)Enum.Parse(typeof(ScenarioTypes), id);
                    }
                    else if (prefix == "e-")
                    {
                        var emotion = (EmotionTypes)Enum.Parse(typeof(EmotionTypes), id);

                        if (pData.Emotion == emotion)
                            return;

                        Events.CallRemote("Players::SetEmotion", (int)emotion);
                    }
                    else if (prefix == "w-")
                    {
                        var walkstyle = (WalkstyleTypes)Enum.Parse(typeof(WalkstyleTypes), id);

                        if (pData.Walkstyle == walkstyle)
                            return;

                        Events.CallRemote("Players::SetWalkstyle", (int)walkstyle);
                    }
                }
            );

            Events.Add("Anims::Menu::Fav",
                (object[] args) =>
                {
                    var id = (string)args[0];
                    bool state = !(bool)args[1];

                    string prefix = id.Substring(0, 2);

                    if (prefix != "a-" && prefix != "s-")
                        return;

                    HashSet<string> favs = Settings.User.Other.FavoriteAnimations;

                    if (state)
                        favs.Add(id);
                    else
                        favs.Remove(id);

                    Settings.User.Other.FavoriteAnimations = favs;
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.Animations);

        private static List<int> TempBinds { get; set; }

        public static void Open()
        {
            if (IsActive || Utils.Misc.IsAnyCefActive())
                return;

            Cursor.Show(true, true);

            Browser.Switch(Browser.IntTypes.Animations, true);

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));

            Main.Render -= Render;

            Input.Core.ExtraBind cancelAnimKb = Input.Core.Get(BindTypes.CancelAnimation);

            if (!cancelAnimKb.IsDisabled)
                Input.Core.Get(BindTypes.CancelAnimation).Disable();

            if (Queue.Count > 0)
            {
                (string Function, object[] Args) cmd;

                while (Queue.TryDequeue(out cmd))
                {
                    Browser.Window.ExecuteJs(cmd.Function, cmd.Args);
                }
            }
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Browser.Switch(Browser.IntTypes.Animations, false);

            Cursor.Show(false, false);

            foreach (int x in TempBinds.ToList())
            {
                Input.Core.Unbind(x);
            }

            TempBinds.Clear();

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.OtherAnim != OtherTypes.None)
            {
                Main.Render -= Render;
                Main.Render += Render;

                Input.Core.Get(BindTypes.CancelAnimation).Enable();
            }
        }

        public static void Cancel()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.OtherAnim == OtherTypes.None)
                return;

            if (LastSent.IsSpam(500, false, false))
                return;

            Events.CallRemote("Players::SetAnim", (int)OtherTypes.None);
        }

        public static void ToggleAnim(string animId, bool state)
        {
            if (!IsActive)
                Queue.Enqueue(("Anims.colorAnim", new object[]
                    {
                        animId,
                        state,
                    })
                );
            else
                Browser.Window.ExecuteJs("Anims.colorAnim", animId, state);
        }

        public static void Render()
        {
            Graphics.DrawText(string.Format(Locale.General.Animations.CancelText, Input.Core.Get(BindTypes.CancelAnimation).GetKeyString()),
                0.5f,
                0.95f,
                255,
                255,
                255,
                255,
                0.45f,
                RAGE.Game.Font.ChaletComprimeCologne,
                true,
                true
            );
        }

        public static async System.Threading.Tasks.Task Load()
        {
            await Browser.Render(Browser.IntTypes.Animations, true);

            var list = new object[]
            {
                // Animations
                new object[]
                {
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Social].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Social]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Dialogs].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Dialogs]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Reactions].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Reactions]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.SeatLie].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.SeatLie]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Sport].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Sport]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Indecent].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Indecent]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.StandPoses].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.StandPoses]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Dances].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Dances]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.Situative].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.Situative]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                    new object[]
                    {
                        Locale.General.Animations.Anims[AnimSectionTypes.WithWeapon].SectionName,
                        Locale.General.Animations.Anims[AnimSectionTypes.WithWeapon]
                              .Names.Select(x => new object[]
                                   {
                                       "a-" + x.Key.ToString(),
                                       x.Value,
                                   }
                               ),
                    },
                },

                // Scenarios
                new object[]
                {
                },

                // Walkstyles
                new object[]
                {
                    Locale.General.Animations.Walkstyles.Select(x => new object[]
                        {
                            "w-" + x.Key.ToString(),
                            x.Value,
                        }
                    ),
                },

                // Emotions
                new object[]
                {
                    Locale.General.Animations.Emotions.Select(x => new object[]
                        {
                            "e-" + x.Key.ToString(),
                            x.Value,
                        }
                    ),
                },

                // Fav
                Settings.User.Other.FavoriteAnimations,
            };

            Browser.Window.ExecuteJs("Anims.draw",
                new object[]
                {
                    list,
                }
            );
        }
    }
}