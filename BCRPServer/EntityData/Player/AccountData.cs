using System;

namespace BCRPServer
{
    public class AccountData
    {
        public uint ID { get; set; }

        public string SCID { get; set; }

        public string HWID { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Mail { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string RegistrationIP { get; set; }

        public string LastIP { get; set; }

        public int AdminLevel { get; set; }

        public int BCoins { get; set; }

        public AccountData() { }

        public class GlobalBan
        {
            public enum Types
            {
                /// <summary>Блокировка по IP</summary>
                IP = 0,
                /// <summary>Блокировка по серийному номеру</summary>
                HWID,
                /// <summary>Блокировка по Social Club</summary>
                SCID,
                /// <summary>Черный список проекта</summary>
                Blacklist,
            }

            public Types Type { get; set; }

            public DateTime Date { get; set; }

            public string Reason { get; set; }

            public int AdminID { get; set; }

            public uint ID { get; set; }

            public GlobalBan(uint ID, Types Type, DateTime Date, string Reason, int AdminID)
            {
                this.ID = ID;
                this.Type = Type;
                this.Date = Date;
                this.Reason = Reason;
                this.AdminID = AdminID;
            }

            public override string ToString() => string.Format(Locale.General.GlobalBan.NotificationText, ID, Locale.General.GlobalBan.TypesNames[Type], Date.ToString(), Reason, AdminID);
        }
    }
}