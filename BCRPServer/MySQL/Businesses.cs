using GTANetworkAPI;
using MySqlConnector;
using System;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void FarmEntityUpdateData(string key, object data, object data2 = null)
        {
            var cmd = new MySqlCommand();

            var keyHash = NAPI.Util.GetHashKey(key);

            cmd.CommandText = "UPDATE farms_data SET Data=@Data WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", keyHash);

            if (data == null)
            {
                cmd.Parameters.AddWithValue("@Data", DBNull.Value);
            }
            else
            {
                if (data2 != null)
                    data = $"{data}_{data2}";

                cmd.Parameters.AddWithValue("@Data", data.ToString());
            }

            PushQuery(cmd);
        }

        public static void BusinessUpdateComplete(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET CID=@CID, Cash=@Cash, Bank=@Bank, Margin=@Margin WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            if (business.Owner == null)
                cmd.Parameters.AddWithValue("@CID", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@CID", business.Owner.CID);

            cmd.Parameters.AddWithValue("@Cash", business.Cash);
            cmd.Parameters.AddWithValue("@Bank", business.Bank);
            cmd.Parameters.AddWithValue("@Margin", business.Margin);

            PushQuery(cmd);
        }

        public static void BusinessUpdateOwner(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET CID=@CID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            if (business.Owner == null)
                cmd.Parameters.AddWithValue("@CID", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@CID", business.Owner.CID);

            PushQuery(cmd);
        }

        public static void BusinessUpdateBalances(Game.Businesses.Business business, bool orderedMaterials)
        {
            var cmd = new MySqlCommand();

            if (orderedMaterials)
            {
                cmd.CommandText = "UPDATE businesses SET Cash=@Cash, Bank=@Bank, Materials=@Mats, OrderedMaterials=@OMats WHERE ID=@ID;";

                cmd.Parameters.AddWithValue("@OMats", business.OrderedMaterials);
            }
            else
            {
                cmd.CommandText = "UPDATE businesses SET Cash=@Cash, Bank=@Bank, Materials=@Mats WHERE ID=@ID;";
            }

            cmd.Parameters.AddWithValue("@ID", business.ID);

            cmd.Parameters.AddWithValue("@Cash", business.Cash);
            cmd.Parameters.AddWithValue("@Bank", business.Bank);

            cmd.Parameters.AddWithValue("@Mats", business.Materials);

            PushQuery(cmd);
        }

        public static void BusinessUpdateMargin(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET Margin=@Margin WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            cmd.Parameters.AddWithValue("@Margin", business.Margin);

            PushQuery(cmd);
        }

        public static void BusinessUpdateOnRestart(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET Statistics=@Stats, IncassationState=@IncState WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            cmd.Parameters.AddWithValue("@Stats", business.Statistics.SerializeToJson());
            cmd.Parameters.AddWithValue("@IncState", business.IncassationState);

            PushQuery(cmd);
        }

        public static void BusinessUpdateStatistics(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET Statistics=@Stats WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            cmd.Parameters.AddWithValue("@Stats", business.Statistics.SerializeToJson());

            PushQuery(cmd);
        }
    }
}
