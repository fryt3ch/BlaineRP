using System;

namespace BCRPServer
{
    public partial class PlayerData
    {
        public class Punishment
        {
            public enum Types
            {
                /// <summary>Блокировка</summary>
                Ban = 0,
                /// <summary>Предупреждение</summary>
                Warn = 1,
                /// <summary>Мут</summary>
                Mute = 2,
                /// <summary>NonRP тюрьма</summary>
                NRPPrison = 3
            }

            /// <summary>Уникальный ID наказания</summary>
            public int ID { get; set; }

            /// <summary>Тип наказания</summary>
            public Types Type { get; set; }

            /// <summary>Причина наказания</summary>
            public string Reason { get; set; }

            /// <summary>Дата выдачи наказания</summary>
            public DateTime StartDate { get; set; }

            /// <summary>Дата окончания наказания/summary>
            public DateTime EndDate { get; set; }

            /// <summary>CID администратора, выдавшего наказание</summary>
            public int AdminID { get; set; }

            public Punishment(int ID, Types Type, string Reason, DateTime StartDate, DateTime EndDate, int AdminID)
            {
                this.ID = ID;
                this.Type = Type;
                this.Reason = Reason;
                this.StartDate = StartDate;
                this.EndDate = EndDate;
                this.AdminID = AdminID;
            }

            /// <summary>Получить оставшееся время в секундах</summary>
            /// <returns>Время в секундах</returns>
            public int GetSecondsLeft() => (int)EndDate.Subtract(StartDate).TotalSeconds;
        }
    }
}
