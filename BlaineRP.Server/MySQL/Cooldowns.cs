using MySqlConnector;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Cooldowns;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        public static void CharacterCooldownSet(PlayerInfo pInfo, uint hash, Cooldown cooldown, bool insert)
        {
            var cmd = new MySqlCommand();

            if (insert)
            {
                cmd.CommandText = "INSERT INTO cooldowns (ID, CID, Hash, StartDate, Time) VALUES (@ID, @CID, @H, @D, @T);";

                cmd.Parameters.AddWithValue("@ID", cooldown.Guid);
                cmd.Parameters.AddWithValue("@CID", pInfo.CID);
                cmd.Parameters.AddWithValue("@H", hash);
                cmd.Parameters.AddWithValue("@D", cooldown.StartDate);
                cmd.Parameters.AddWithValue("@T", cooldown.Time.TotalSeconds);
            }
            else
            {
                cmd.CommandText = "UPDATE cooldowns SET StartDate=@D, Time = @T WHERE ID=@ID;";

                cmd.Parameters.AddWithValue("@ID", cooldown.Guid);
                cmd.Parameters.AddWithValue("@T", cooldown.Time.TotalSeconds);
                cmd.Parameters.AddWithValue("@D", cooldown.StartDate);
            }

            PushQuery(cmd);
        }

        public static void CharacterCooldownRemoveByGuid(PlayerInfo pInfo, Guid guid)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM cooldowns WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", guid.ToString());

            PushQuery(cmd);
        }
    }
}
