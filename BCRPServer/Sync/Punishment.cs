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
        public ulong GetSecondsLeft(out long secondsPassed)
        {
            var curTime = Utils.GetCurrentTime();

            if (Type == Types.NRPPrison || Type == Types.Arrest || Type == Types.FederalPrison)
            {
                var timeEnd = EndDate.GetUnixTimestamp();

                secondsPassed = long.Parse(AdditionalData.Split('_')[0]);

                if (secondsPassed >= timeEnd)
                    return 0;

                var t = TimeSpan.FromSeconds(timeEnd - secondsPassed).TotalSeconds;

                return (ulong)t;
            }
            else
            {
                secondsPassed = 0;

                if (curTime >= EndDate)
                    return 0;

                var t = EndDate.Subtract(curTime).TotalSeconds;

                return (ulong)t;
            }
        }

        public bool IsActive()
        {
            if (AmnestyInfo != null)
                return false;

            return GetSecondsLeft(out _) > 0;
        }

        public void OnFinish(PlayerData.PlayerInfo pInfo, params object[] args)
        {
            if (Type == Types.NRPPrison)
            {
                var finishType = (int)args[0];

                if (pInfo.PlayerData != null)
                {
                    if (finishType == 0)
                    {
                        pInfo.PlayerData.Player.TriggerEvent("Player::Punish", Id, (int)Type, ushort.MaxValue, -2, null);
                    }

                    Utils.Demorgan.SetFromDemorgan(pInfo.PlayerData);
                }

                if (pInfo.Fraction != Game.Fractions.Types.None)
                {
                    var fData = Game.Fractions.Fraction.Get(pInfo.Fraction);

                    fData.OnMemberStatusChange(pInfo, fData.GetMemberStatus(pInfo));
                }

                if (finishType == 0)
                {
                    AmnestyInfo = new Sync.Punishment.Amnesty();

                    MySQL.UpdatePunishmentAmnesty(this);
                }
            }
            else if (Type == Types.Arrest)
            {
                var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)int.Parse(AdditionalData.Split('_')[1])) as Game.Fractions.Police;

                var finishType = (int)args[0];

                if (pInfo.PlayerData != null)
                {
                    if (finishType == 0)
                    {
                        pInfo.PlayerData.Player.TriggerEvent("Player::Punish", Id, (int)Type, ushort.MaxValue, -2, null);
                    }
                    else if (finishType == 1)
                    {
                        var tData = (PlayerData)args[1];

                        pInfo.PlayerData.Player.TriggerEvent("Player::Punish", Id, (int)Type, tData.Player.Id, -1, null);
                    }

                    fData.SetPlayerFromPrison(pInfo.PlayerData);
                }

                pInfo.LastData.Position = new Utils.Vector4(fData.ArrestFreePosition.X, fData.ArrestFreePosition.Y, fData.ArrestFreePosition.Z, fData.ArrestFreePosition.RotationZ);

                fData.RemoveActiveArrest(Id);

                if (finishType == 0)
                {
                    AmnestyInfo = new Sync.Punishment.Amnesty();

                    MySQL.UpdatePunishmentAmnesty(this);
                }
            }
        }

        public static uint GetNextId() => Sync.Punishment.MaxAddedId += 1;
    }
}
