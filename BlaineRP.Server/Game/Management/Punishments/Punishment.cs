using System;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management
{
    public partial class Punishment
    {
        public static uint MaxAddedId { get; set; }

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

        public void OnFinish(PlayerInfo pInfo, params object[] args)
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

                if (pInfo.Fraction != Game.Fractions.FractionType.None)
                {
                    var fData = Game.Fractions.Fraction.Get(pInfo.Fraction);

                    fData.OnMemberStatusChange(pInfo, fData.GetMemberStatus(pInfo));
                }

                if (finishType == 0)
                {
                    AmnestyInfo = new Punishment.Amnesty();

                    MySQL.UpdatePunishmentAmnesty(this);
                }
            }
            else if (Type == Types.Arrest)
            {
                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)int.Parse(AdditionalData.Split('_')[1])) as Game.Fractions.Police;

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
                        var reason = args.Length > 2 && args[2] is string str ? str : null;

                        pInfo.PlayerData.Player.TriggerEvent("Player::Punish", Id, (int)Type, tData.Player.Id, -1, reason);
                    }

                    fData.SetPlayerFromPrison(pInfo.PlayerData);
                }

                pInfo.LastData.Position = new Utils.Vector4(fData.ArrestFreePosition.X, fData.ArrestFreePosition.Y, fData.ArrestFreePosition.Z, fData.ArrestFreePosition.RotationZ);

                fData.RemoveActiveArrest(Id);

                if (finishType == 0)
                {
                    AmnestyInfo = new Punishment.Amnesty();

                    MySQL.UpdatePunishmentAmnesty(this);
                }
            }
        }

        public static uint GetNextId() => Punishment.MaxAddedId += 1;
    }
}
