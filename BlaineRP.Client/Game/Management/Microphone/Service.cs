using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Management.Punishments;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Microphone
{
    [Script(int.MaxValue)]
    public class Service
    {
        private const string AnimDict = "mp_facial";
        private const string AnimDictNormal = "facials@gen_male@variations@normal";

        private const string AnimName = "mic_chatter";
        private const string AnimNameNormal = "mood_normal_1";
        private static bool Use3D = true;

        private static List<Player> _listeners = new List<Player>();
        private static List<Player> _talkers = new List<Player>();

        private static DateTime LastSwitched;
        private static DateTime LastReloaded;
        private static DateTime LastSent;

        private static AsyncTask _updateListenersTask;

        public Service()
        {
            // Changing Volume Of Talkers
            new AsyncTask(() =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    Phone.CallInfo activeCall = pData.ActiveCall;

                    for (var i = 0; i < _talkers.Count; i++)
                    {
                        Player player = _talkers[i];

                        var tData = PlayerData.GetData(player);

                        if (tData == null)
                            continue;

                        float vRange = tData.VoiceRange;

                        if (vRange <= 0f)
                        {
                            player.VoiceVolume = 0f;

                            continue;
                        }

                        if (activeCall?.Player != player)
                        {
                            float dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                            if (dist <= vRange)
                                player.VoiceVolume = Settings.User.Audio.VoiceVolume / 100f / vRange * (vRange - dist);
                            else
                                player.VoiceVolume = 0f;
                        }
                    }
                },
                350,
                true,
                0
            ).Run();

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
                        }
                    );
                },
                5000,
                true,
                0
            ).Run();
        }

        public static void Reload()
        {
            if (LastReloaded.IsSpam(5000, false, false))
                return;

            Stop();

            LastReloaded = World.Core.ServerTime;
            Utils.Misc.ReloadVoiceChat();

            _listeners.Clear();
            _talkers.Clear();

            List<Player> streamed = Entities.Players.Streamed;

            for (var i = 0; i < streamed.Count; i++)
            {
                Player player = streamed[i];

                if (player.Handle == Player.LocalPlayer.Handle)
                    continue;

                var data = PlayerData.GetData(player);

                if (data == null)
                    continue;

                if (data.VoiceRange > 0f)
                    AddTalker(player);
            }

            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.Players.Microphone.Reloaded);
        }

        public static void Start()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange > 0f)
                return;

            if (LastSwitched.IsSpam(500, false, false))
                return;

            Punishment mute = Punishment.All.Where(x => x.Type == PunishmentType.Mute).FirstOrDefault();

            if (mute != null)
            {
                mute.ShowErrorNotification();

                return;
            }

            LastSwitched = World.Core.ServerTime;

            RAGE.Events.CallRemote("Microphone::Switch", true);
        }

        public static void Stop()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange <= 0f)
                return;

            Main.Update -= OnTick;

            RAGE.Events.CallRemote("Microphone::Switch", false);

            LastSwitched = World.Core.ServerTime;
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
                    Phone.CallInfo activeCall = pData.ActiveCall;

                    float vRange = pData.VoiceRange;

                    if (vRange <= 0f)
                        return;

                    List<Player> streamed = Entities.Players.Streamed;

                    for (var i = 0; i < streamed.Count; i++)
                    {
                        Player player = streamed[i];

                        if (player.Handle == Player.LocalPlayer.Handle || activeCall?.Player == player)
                            continue;

                        var tData = PlayerData.GetData(player);

                        if (tData == null)
                            continue;

                        float dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                        if (_listeners.Contains(player))
                        {
                            if (dist > vRange)
                                if (!RemoveListener(player))
                                    return;
                        }
                        else
                        {
                            if (dist <= vRange)
                                if (!AddListener(player))
                                    return;
                        }
                    }
                },
                100,
                true,
                0
            );

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

            RAGE.Events.CallRemote("mal", player);

            LastSent = World.Core.ServerTime;

            return true;
        }

        public static bool RemoveListener(Player player, bool checkTime = true)
        {
            if (!_listeners.Contains(player))
                return true;

            if (checkTime && LastSent.IsSpam(50))
                return false;

            _listeners.Remove(player);

            RAGE.Events.CallRemote("mrl", player);

            LastSent = World.Core.ServerTime;

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