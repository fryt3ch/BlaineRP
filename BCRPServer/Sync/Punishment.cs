using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync
{
    public class Punishment
    {
        public static uint MaxAddedId { get; set; }

        public enum Types
        {
            /// <summary>Блокировка</summary>
            Ban = 0,

            /// <summary>Предупреждение</summary>
            Warn = 1,

            /// <summary>Мут</summary>
            Mute = 2,

            /// <summary>NonRP тюрьма</summary>
            NRPPrison = 3,

            /// <summary>СИЗО</summary>
            Arrest = 4,

            /// <summary>Федеральная тюрьма</summary>
            FederalPrison = 5,

            /// <summary>Мут чата фракции</summary>
            FractionMute = 6,

            /// <summary>Мут чата организации</summary>
            OrganisationMute = 7,
        }

        public class Amnesty
        {
            [JsonProperty(PropertyName = "D")]
            public DateTime Date { get; set; }

            [JsonProperty(PropertyName = "R")]
            public string Reason { get; set; }

            [JsonProperty(PropertyName = "CID")]
            public uint CID { get; set; }
        }

        /// <summary>Уникальный ID наказания</summary>
        public uint Id { get; set; }

        /// <summary>Тип наказания</summary>
        public Types Type { get; set; }

        /// <summary>Причина наказания</summary>
        public string Reason { get; set; }

        /// <summary>Дата выдачи наказания</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Дата окончания наказания/summary>
        public DateTime EndDate { get; set; }

        /// <summary>CID того, кто выдал наказание</summary>
        public uint PunisherID { get; set; }

        public Amnesty AmnestyInfo { get; set; }

        public string AdditionalData { get; set; }

        public Punishment(uint Id, Types Type, string Reason, DateTime StartDate, DateTime EndDate, uint PunisherID)
        {
            this.Id = Id;
            this.Type = Type;
            this.Reason = Reason;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            this.PunisherID = PunisherID;
        }

        /// <summary>Получить оставшееся время в секундах</summary>
        /// <returns>Время в секундах</returns>
        public ulong GetSecondsLeft()
        {
            var curTime = Utils.GetCurrentTime();

            if (curTime >= EndDate)
                return 0;

            var t = EndDate.Subtract(curTime).TotalSeconds;

            return (ulong)t;
        }

        public bool IsActive() => AmnestyInfo == null && GetSecondsLeft() > 0;

        public static uint GetNextId() => Sync.Punishment.MaxAddedId += 1;
    }
}
