using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.EntitiesData;
using BlaineRP.Client.EntitiesData.Components;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Management.Microphone
{
    [Script(int.MaxValue)]
    public class Core
    {
        private static bool Use3D = true;

        private static List<Player> _listeners = new List<Player>();
        private static List<Player> _talkers = new List<Player>();

        private static DateTime LastSwitched;
        private static DateTime LastReloaded;
        private static DateTime LastSent;

        private static AsyncTask _updateListenersTask;

        private const string AnimDict = "mp_facial";
        private const string AnimDictNormal = "facials@gen_male@variations@normal";

        private const string AnimName = "mic_chatter";
        private const string AnimNameNormal = "mood_normal_1";

        public Core()
        {
            // Changing Volume Of Talkers
            new AsyncTask(() =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var activeCall = pData.ActiveCall;

                for (var i = 0; i < _talkers.Count; i++)
                {
                    var player = _talkers[i];

                    var tData = PlayerData.GetData(player);

                    if (tData == null)
                        continue;

                    var vRange = tData.VoiceRange;

                    if (vRange <= 0f)
                    {
                        player.VoiceVolume = 0f;

                        continue;
                    }

                    if (activeCall?.Player != player)
                    {
                        var dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                        if (dist <= vRange)
                            player.VoiceVolume = ((Settings.User.Audio.VoiceVolume / 100f) / vRange) * (vRange - dist);
                        else
                            player.VoiceVolume = 0f;
                    }
                }
            }, 350, true, 0).Run();

            // Update Facial Anim On Talkers
            new AsyncTask(() =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (pData.VoiceRange > 0f)
                    SetTalkingAnim(Player.LocalPlayer, true);

                _talkers.ForEach(x =>
                {
                    SetTalkingAnim(x, true);
                });
            }, 5000, true, 0).Run();
        }

        public static void Reload()
        {
            if (LastReloaded.IsSpam(5000, false, false))
                return;

            Stop();

            LastReloaded = Sync.World.ServerTime;
            Utils.Misc.ReloadVoiceChat();

            _listeners.Clear();
            _talkers.Clear();

            var streamed = RAGE.Elements.Entities.Players.Streamed;

            for (int i = 0; i < streamed.Count; i++)
            {
                var player = streamed[i];

                if (player.Handle == Player.LocalPlayer.Handle)
                    continue;

                var data = PlayerData.GetData(player);

                if (data == null)
                    continue;

                if (data.VoiceRange > 0f)
                    AddTalker(player);
            }

            CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.Players.Microphone.Reloaded);
        }

        public static void Start()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange > 0f)
                return;

            if (LastSwitched.IsSpam(500, false, false))
                return;

            var mute = Punishment.All.Where(x => x.Type == Punishment.Types.Mute).FirstOrDefault();

            if (mute != null)
            {
                mute.ShowErrorNotification();

                return;
            }

            LastSwitched = Sync.World.ServerTime;

            Events.CallRemote("Microphone::Switch", true);
        }

        public static void Stop()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange <= 0f)
                return;

            Main.Update -= OnTick;

            Events.CallRemote("Microphone::Switch", false);

            LastSwitched = Sync.World.ServerTime;
        }
        
        public static void StartUpdateListeners()
        {
            _listeners.Clear();

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            _updateListenersTask?.Cancel();

            _updateListenersTask = new AsyncTask(() =>
            {
                var activeCall = pData.ActiveCall;

                var vRange = pData.VoiceRange;

                if (vRange <= 0f)
                    return;

                var streamed = RAGE.Elements.Entities.Players.Streamed;

                for (int i = 0; i < streamed.Count; i++)
                {
                    var player = streamed[i];

                    if (player.Handle == Player.LocalPlayer.Handle || activeCall?.Player == player)
                        continue;

                    var tData = PlayerData.GetData(player);

                    if (tData == null)
                        continue;

                    var dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                    if (_listeners.Contains(player))
                    {
                        if (dist > vRange)
                        {
                            if (!RemoveListener(player))
                                return;
                        }
                    }
                    else
                    {
                        if (dist <= vRange)
                        {
                            if (!AddListener(player))
                                return;
                        }
                    }
                }
            }, 100, true, 0);

            _updateListenersTask.Run();
        }

        public static void StopUpdateListeners()
        {
            _listeners.Clear();

            _updateListenersTask?.Cancel();

            _updateListenersTask = null;
        }

        public static void OnTick()
        {
            if (!Utils.Misc.IsGameWindowFocused)
                Stop();
        }

        private static bool AddListener(Player player)
        {
            if (_listeners.Contains(player))
                return true;

            if (LastSent.IsSpam(50))
                return false;

            _listeners.Add(player);

            Events.CallRemote("mal", player);

            LastSent = Sync.World.ServerTime;

            return true;
        }

        public static bool RemoveListener(Player player, bool checkTime = true)
        {
            if (!_listeners.Contains(player))
                return true;

            if (checkTime && LastSent.IsSpam(50))
                return false;

            _listeners.Remove(player);

            Events.CallRemote("mrl", player);

            LastSent = Sync.World.ServerTime;

            return true;
        }

        public static void AddTalker(Player player)
        {
            if (_talkers.Contains(player))
                return;

            player.AutoVolume = false;
            player.Voice3d = Use3D;
            player.VoiceVolume = 0f;

            _talkers.Add(player);

            SetTalkingAnim(player, true);
        }

        public static void RemoveTalker(Player player)
        {
            _talkers.Remove(player);

            player.AutoVolume = false;
            player.Voice3d = false;
            player.VoiceVolume = 0f;

            SetTalkingAnim(player, false);
        }

        public static async void SetTalkingAnim(Player player, bool state)
        {
            if (player == null)
                return;

            if (state)
            {
                await Streaming.RequestAnimDict(AnimDict);

                player.PlayFacialAnim(AnimName, AnimDict);
            }
            else
            {
                await Streaming.RequestAnimDict(AnimDictNormal);

                player.PlayFacialAnim(AnimNameNormal, AnimDictNormal);
            }
        }
    }
}
