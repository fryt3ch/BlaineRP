using MySqlConnector;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Punishments;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        public static void AddPunishment(PlayerInfo punishedInfo, PlayerInfo punisherInfo, Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"INSERT INTO punishments (ID, CID, Type, StartDate, EndDate, Reason, PunisherID, Data) VALUES (@ID, @CID, @T, @SD, @ED, @R, @PCID, @Data);";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);
            cmd.Parameters.AddWithValue("@CID", punishedInfo.CID);
            cmd.Parameters.AddWithValue("@PCID", punisherInfo.CID);
            cmd.Parameters.AddWithValue("@T", (int)punishment.Type);
            cmd.Parameters.AddWithValue("@SD", punishment.StartDate);
            cmd.Parameters.AddWithValue("@ED", punishment.EndDate);
            cmd.Parameters.AddWithValue("@R", punishment.Reason);

            if (punishment.AdditionalData == null)
                cmd.Parameters.AddWithValue("@Data", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Data", punishment.AdditionalData);

            PushQuery(cmd);
        }

        public static void UpdatePunishmentAmnesty(Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE punishments SET Amnesty=@Amnesty WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);

            if (punishment.AmnestyInfo == null)
                cmd.Parameters.AddWithValue("@Amnesty", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Amnesty", punishment.AmnestyInfo.SerializeToJson());

            PushQuery(cmd);
        }

        public static void UpdatePunishmentAdditionalData(Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE punishments SET Data=@D WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);

            if (punishment.AdditionalData == null)
                cmd.Parameters.AddWithValue("@D", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@D", punishment.AdditionalData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void UpdatePunishmentEndDate(Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE punishments SET EndDate=@D WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);

            cmd.Parameters.AddWithValue("@D", punishment.EndDate);

            PushQuery(cmd);
        }

        public static void RemovePunishment(uint id)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM punishments WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", id);

            PushQuery(cmd);
        }
    }
}
