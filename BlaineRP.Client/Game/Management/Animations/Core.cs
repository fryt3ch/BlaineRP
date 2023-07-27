using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Animations
{
    [Script(int.MaxValue)]
    public partial class Core
    {
        public static DateTime LastSent;

        public Core()
        {
            Events.Add("Players::PlayFastAnim",
                async (args) =>
                {
                    var player = (Player)args[0];

                    if (player == null)
                        return;

                    var type = (FastTypes)(int)args[1];

                    Animation data = FastAnimsList.GetValueOrDefault(type);

                    if (data == null)
                        return;

                    var timeout = TimeSpan.FromMilliseconds(Utils.Convert.ToInt64(args[2]));

                    if (player == Player.LocalPlayer)
                    {
                        var pData = PlayerData.GetData(player);

                        if (pData != null)
                            pData.FastAnim = type;

                        AsyncTask.Methods.CancelPendingTask("LPFATT");

                        AsyncTask task = null;

                        task = new AsyncTask(async () =>
                            {
                                await RAGE.Game.Invoker.WaitAsync((int)timeout.TotalMilliseconds);

                                if (!AsyncTask.Methods.IsTaskStillPending("LPFATT", task))
                                    return;

                                Events.CallRemote("Players::SFTA");
                            },
                            0,
                            false,
                            0
                        );

                        AsyncTask.Methods.SetAsPending(task, "LPFATT");
                    }

                    Play(player, data, (int)timeout.TotalMilliseconds);
                }
            );

            Events.Add("Players::StopFastAnim",
                (args) =>
                {
                    Player player = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[0]));

                    if (player == null)
                        return;

                    if (player == Player.LocalPlayer)
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData != null)
                            pData.FastAnim = FastTypes.None;

                        AsyncTask.Methods.CancelPendingTask("LPFATT");
                    }

                    Stop(player);
                }
            );
        }

        public static void Set(Player player, EmotionTypes emotion)
        {
            if (player == null)
                return;

            if (emotion == EmotionTypes.None)
            {
                player.ClearFacialIdleAnimOverride();

                return;
            }
            else
            {
                Invoker.InvokeViaJs(RAGE.Game.Natives.SetFacialIdleAnimOverride, player.Handle, EmotionsList[emotion], 0);
            }
        }

        public static async void Set(Player player, WalkstyleTypes walkstyle)
        {
            if (player == null)
                return;

            if (walkstyle == WalkstyleTypes.None)
            {
                player.ResetMovementClipset(0f);

                return;
            }
            else
            {
                await Streaming.RequestClipSet(WalkstylesList[walkstyle]);

                player.SetMovementClipset(WalkstylesList[walkstyle], Crouch.ClipSetSwitchTime);
            }
        }

        public static void Play(Player player, GeneralTypes type)
        {
            if (player == null)
                return;

            Animation anim = GeneralAnimsList.GetValueOrDefault(type);

            if (anim == null)
                return;

            Play(player, anim);
        }

        public static void Play(Player player, OtherTypes type)
        {
            if (player == null)
                return;

            Animation anim = OtherAnimsList.GetValueOrDefault(type);

            if (anim == null)
                return;

            Play(player, anim);
        }

        public static async void Play(PedBase ped, Animation anim, int customTime = -1)
        {
            if (ped == null)
                return;

            if (ped.Handle == Player.LocalPlayer.Handle)
            {
                AsyncTask.Methods.CancelPendingTask("LPFATT");

                Phone.DestroyLocalPhone();
            }

            if (anim == null)
                return;

            await Streaming.RequestAnimDict(anim.Dict);

            ped.ClearTasks();

            anim.StartAction?.Invoke(ped, anim);

            if (ped.Handle != Player.LocalPlayer.Handle)
                ped.TaskPlayAnim(anim.Dict,
                    anim.Name,
                    anim.BlendInSpeed,
                    anim.BlendOutSpeed,
                    customTime == -1 ? anim.Duration : customTime,
                    anim.Flag,
                    anim.StartOffset,
                    anim.BlockX,
                    anim.BlockY,
                    anim.BlockZ
                );
            else
                ped.TaskPlayAnim(anim.Dict,
                    Utils.Game.Camera.IsFirstPersonActive() ? anim.NameFP ?? anim.Name : anim.Name,
                    anim.BlendInSpeed,
                    anim.BlendOutSpeed,
                    customTime == -1 ? anim.Duration : customTime,
                    anim.Flag,
                    anim.StartOffset,
                    anim.BlockX,
                    anim.BlockY,
                    anim.BlockZ
                );
        }

        public static void Stop(PedBase ped)
        {
            if (ped == null)
                return;

            if (ped.Handle == Player.LocalPlayer.Handle)
                AsyncTask.Methods.CancelPendingTask("LPFATT");

            ped.ClearTasks();

            Animation actualAnim = ped.GetData<Animation>("ActualAnim");

            if (actualAnim != null)
                actualAnim.StopAction?.Invoke(ped, actualAnim);
        }

        public static void PlayFastSync(FastTypes fastType)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            if (PlayerActions.IsAnyActionActive(true,
                    PlayerActions.Types.Knocked,
                    PlayerActions.Types.Frozen,
                    PlayerActions.Types.Cuffed,
                    PlayerActions.Types.OtherAnimation,
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

            Events.CallRemote("Players::PFA", (int)fastType);

            LastSent = World.Core.ServerTime;
        }
    }
}