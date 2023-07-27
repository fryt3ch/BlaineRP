using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Sync
{
    public class Report
    {
        public const int REPORT_PLAYER_MESSAGE_TIMEOUT = 120_000;

        public const int REPORT_PLAYER_MESSAGE_MIN_SYMBOLS = 10;
        public const int REPORT_PLAYER_MESSAGE_MAX_SYMBOLS = 150;

        private static List<Report> AllReports { get; set; } = new List<Report>();

        public static Report GetFirstOrDefaultByPredicate(Func<Report, bool> predicate) => AllReports.Where(predicate).FirstOrDefault();

        public static Report GetByStarterPlayer(PlayerInfo pInfo) => AllReports.Where(x => x.StarterPlayer == pInfo).FirstOrDefault();

        public static Report GetByWorkerAdmin(PlayerInfo pInfo) => AllReports.Where(x => x.WorkerAdmin == pInfo).FirstOrDefault();

        public static bool IsMessageCorrect(string message)
        {
            if (message.Length < REPORT_PLAYER_MESSAGE_MIN_SYMBOLS || message.Length > REPORT_PLAYER_MESSAGE_MAX_SYMBOLS)
                return false;

            return true;
        }

        public bool Exists => AllReports.Contains(this);

        public int NumberInQueue => AllReports.IndexOf(this);

        public PlayerInfo StarterPlayer { get; private set; }

        public PlayerInfo WorkerAdmin { get; private set; }

        public DateTime UpdatedTime { get; private set; }

        public DateTime StarterPlayerLastSent { get; set; }

        private Dictionary<PlayerInfo, List<string>> ChatHistory { get; set; }

        public Report(PlayerInfo StarterPlayer, string StartMessage)
        {
            this.StarterPlayer = StarterPlayer;

            this.UpdatedTime = Utils.GetCurrentTime();

            this.ChatHistory = new Dictionary<PlayerInfo, List<string>>();

            if (StartMessage != null)
            {
                ChatHistory.Add(StarterPlayer, new List<string>() { StartMessage });
            }

            AllReports.Add(this);

            if (StarterPlayer.PlayerData != null)
            {
                StarterPlayer.PlayerData.Player.Notify("Report::N", AllReports.Count);
            }
        }

        public void AddMessage(PlayerData pData, string message)
        {
            var messages = ChatHistory.GetValueOrDefault(pData.Info);

            if (messages == null)
            {
                messages = new List<string>();

                ChatHistory.Add(pData.Info, messages);
            }

            messages.Add(message);
        }

        public void UpdateUpdatedTime()
        {
            if (AllReports.Remove(this))
            {
                UpdatedTime = Utils.GetCurrentTime();

                AllReports.Add(this);
            }
        }

        public bool Close(PlayerData pDataInit)
        {
            if (AllReports.Remove(this))
            {
                if (StarterPlayer.PlayerData != null)
                {
                    if (pDataInit != null)
                        StarterPlayer.PlayerData.Player.TriggerEvent("Menu::Report::C", pDataInit.Player.Id);
                    else
                        StarterPlayer.PlayerData.Player.TriggerEvent("Menu::Report::C");
                }

                return true;
            }

            return false;
        }
    }
}
