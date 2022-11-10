using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Sync
{
    public class Microphone : Events.Script
    {
        private static bool Use3D = true;

        private static List<Player> Listeners;
        private static List<Player> Talkers;

        private static DateTime LastSwitched;
        private static DateTime LastReloaded;
        private static DateTime LastSent;

        public static AsyncTask UpdateListenersTask;

        #region Anims
        private static string AnimDict = "mp_facial";
        private static string AnimDictNormal = "facials@gen_male@variations@normal";

        private static string AnimName = "mic_chatter";
        private static string AnimNameNormal = "mood_normal_1";
        #endregion

        public Microphone()
        {
            LastSwitched = DateTime.MinValue;
            LastReloaded = DateTime.MinValue;
            LastSent = DateTime.MinValue;

            Listeners = new List<Player>();
            Talkers = new List<Player>();

            // Changing Volume Of Talkers
            new AsyncTask(() =>
            {
                for (int i = 0; i < Talkers.Count; i++)
                {
                    var player = Talkers[i];

                    var tData = (Sync.Players.GetData(player));

                    if (tData == null)
                        continue;

                    var vRange = tData.VoiceRange;

                    if (vRange <= 0f)
                    {
                        player.VoiceVolume = 0f;

                        continue;
                    }

                    float dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                    if (dist <= vRange)
                        player.VoiceVolume = ((Settings.Audio.VoiceVolume / 100f) / vRange) * (vRange - dist);
                    else
                        player.VoiceVolume = 0f;
                }
            }, 350, true, 0).Run();

            // Update Facial Anim On Talkers
            new AsyncTask(() =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (pData.VoiceRange > 0f)
                    SetTalkingAnim(Player.LocalPlayer, true);

                for (int i = 0; i < Talkers.Count; i++)
                {
                    var player = Talkers[i];

                    SetTalkingAnim(player, true);
                }
            }, 5000, true, 0).Run();
        }

        public static void Reload()
        {
            if (LastReloaded.IsSpam(5000, false, false))
                return;

            Stop();

            LastReloaded = DateTime.Now;

            Utils.ReloadVoiceChat();

            Listeners.Clear();
            Talkers.Clear();

            var streamed = RAGE.Elements.Entities.Players.Streamed;

            for (int i = 0; i < streamed.Count; i++)
            {
                var player = streamed[i];

                if (player.Handle == Player.LocalPlayer.Handle)
                    continue;

                var data = Sync.Players.GetData(player);

                if (data == null)
                    continue;

                if (data.VoiceRange > 0f)
                    AddTalker(player);
            }

            CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.Players.Microphone.Reloaded);
        }

        public static void Start()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange > 0f)
                return;

            if (LastSwitched.IsSpam(500, false, false) || pData.IsMuted)
                return;

            LastSwitched = DateTime.Now;

            Events.CallRemote("Microphone::Switch", true);
        }

        public static void Stop()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null || pData.VoiceRange <= 0f)
                return;

            GameEvents.Update -= OnTick;

            Events.CallRemote("Microphone::Switch", false);

            LastSwitched = DateTime.Now;
        }

        #region Updaters
        public static void StartUpdateListeners()
        {
            Listeners.Clear();

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            UpdateListenersTask?.Cancel();

            UpdateListenersTask = new AsyncTask(() =>
            {
                var vRange = pData.VoiceRange;

                if (vRange <= 0f)
                    return;

                var streamed = RAGE.Elements.Entities.Players.Streamed;

                for (int i = 0; i < streamed.Count; i++)
                {
                    var player = streamed[i];

                    if (player.Handle == Player.LocalPlayer.Handle)
                        continue;

                    var tData = Sync.Players.GetData(player);

                    if (tData == null)
                        continue;

                    var dist = Vector3.Distance(Player.LocalPlayer.Position, player.Position);

                    if (Listeners.Contains(player))
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

            UpdateListenersTask.Run();
        }

        public static void StopUpdateListeners()
        {
            Listeners.Clear();

            UpdateListenersTask?.Cancel();

            UpdateListenersTask = null;
        }

        public static void OnTick()
        {
            if (!Utils.IsGameWindowFocused)
                Stop();
        }
        #endregion

        #region Listeners & Talkers Stuff
        private static bool AddListener(Player player)
        {
            if (Listeners.Contains(player))
                return true;

            if (LastSent.IsSpam(50))
                return false;

            Listeners.Add(player);

            Events.CallRemote("mal", player);

            LastSent = DateTime.Now;

            return true;
        }

        public static bool RemoveListener(Player player, bool checkTime = true)
        {
            if (!Listeners.Contains(player))
                return true;

            if (checkTime && LastSent.IsSpam(50))
                return false;

            Listeners.Remove(player);

            Events.CallRemote("mrl", player);

            LastSent = DateTime.Now;

            return true;
        }

        public static void AddTalker(Player player)
        {
            if (Talkers.Contains(player))
                return;

            player.AutoVolume = false;
            player.Voice3d = Use3D;
            player.VoiceVolume = 0f;

            Talkers.Add(player);

            SetTalkingAnim(player, true);
        }

        public static void RemoveTalker(Player player)
        {
            Talkers.Remove(player);

            player.AutoVolume = false;
            player.Voice3d = false;
            player.VoiceVolume = 0f;

            SetTalkingAnim(player, false);
        }
        #endregion

        public static async void SetTalkingAnim(Player player, bool state)
        {
            if (player == null)
                return;

            if (state)
            {
                await Utils.RequestAnimDict(AnimDict);

                player.PlayFacialAnim(AnimName, AnimDict);
            }
            else
            {
                await Utils.RequestAnimDict(AnimDictNormal);

                player.PlayFacialAnim(AnimNameNormal, AnimDictNormal);
            }
        }
    }
}
