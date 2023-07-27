using System;

namespace BlaineRP.Server.EntityData.Players
{
    public class AccountData
    {
        public uint ID { get; set; }

        public ulong SCID { get; set; }

        public string HWID { get; set; }

        public string Login { get; set; }

        public string Mail { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string RegistrationIP { get; set; }

        public string LastIP { get; set; }

        public int AdminLevel { get; set; }

        public uint BCoins { get; set; }

        public AccountData() { }

        public class GlobalBan
        {
            public DateTime Date { get; set; }

            public string Reason { get; set; }

            public uint AdminID { get; set; }

            public uint ID { get; set; }

            public string HWID { get; set; }

            public ulong SCID { get; set; }

            public GlobalBan(uint ID, DateTime Date, string Reason, uint AdminID, string HWID, ulong SCID)
            {
                this.ID = ID;
                this.Date = Date;
                this.Reason = Reason;
                this.AdminID = AdminID;

                this.HWID = HWID;
                this.SCID = SCID;
            }

            //public override string ToString() => Language.Strings.Get("GEN_BAN_GLOBAL_NTEXT_0", ID, Language.Strings.Get($"GEN_BAN_GLOBAL_BTYPES_{(int)Type}"), Date.ToString(), Reason, AdminID);
        }
    }
}