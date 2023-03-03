using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using ZstdNet;

namespace BCRPServer
{
    public static partial class MySQL
    {
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
    }
}
