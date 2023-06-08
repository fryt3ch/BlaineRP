using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void CharacterCooldownSet(PlayerData.PlayerInfo pInfo, uint cdType, DateTime date, bool insert)
        {
            var cmd = new MySqlCommand();

            if (insert)
            {
                cmd.CommandText = "UPDATE cooldowns SET Date=@D WHERE CID=@CID AND Type=@T;";

                cmd.Parameters.AddWithValue("@CID", pInfo.CID);
                cmd.Parameters.AddWithValue("@T", cdType);
                cmd.Parameters.AddWithValue("@D", date);
            }
            else
            {
                cmd.CommandText = "INSERT INTO cooldowns (ID, CID, Type, Date) VALUES (@ID, @CID, @T, @D);";

                cmd.Parameters.AddWithValue("@ID", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@CID", pInfo.CID);
                cmd.Parameters.AddWithValue("@T", cdType);
                cmd.Parameters.AddWithValue("@D", date);
            }

            PushQuery(cmd);
        }

        public static void CharacterCooldownRemove(PlayerData.PlayerInfo pInfo, uint cdType)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM cooldowns WHERE CID=@CID AND Type=@T;";

            cmd.Parameters.AddWithValue("@CID", pInfo.CID);
            cmd.Parameters.AddWithValue("@T", cdType);

            PushQuery(cmd);
        }
    }
}
