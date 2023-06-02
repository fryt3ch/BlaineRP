using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void CharacterCooldownSet(PlayerData.PlayerInfo pInfo, Sync.Cooldowns.Types cdType, DateTime date)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE cooldowns SET {cdType.ToString()}=@Time WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@Time", date);

            PushQuery(cmd);
        }

        public static void CharacterCooldownRemove(PlayerData.PlayerInfo pInfo, Sync.Cooldowns.Types cdType)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE cooldowns SET {cdType.ToString()}=@Time WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@Time", DBNull.Value);

            PushQuery(cmd);
        }
    }
}
