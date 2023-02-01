using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static bool LoadBusiness(Game.Businesses.Business business)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM businesses WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", business.ID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return false;

                        reader.Read();

                        if (reader["CID"] is DBNull)
                        {
                            business.UpdateOwner(null);
                        }
                        else
                        {
                            business.UpdateOwner(PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"])));
                        }

                        business.Cash = Convert.ToUInt64(reader["Cash"]);
                        business.Bank = Convert.ToUInt64(reader["Bank"]);

                        business.IncassationState = (bool)reader["IncassationState"];

                        business.Materials = Convert.ToUInt32(reader["Materials"]);
                        business.OrderedMaterials = Convert.ToUInt32(reader["OrderedMaterials"]);

                        business.Margin = (decimal)(float)reader["Margin"];

                        business.Tax = (decimal)(float)reader["Tax"];
                        business.Rent = Convert.ToUInt32(reader["Rent"]);

                        business.GovPrice = Convert.ToUInt32(reader["GovPrice"]);

                        business.Statistics = ((string)reader["Statistics"]).DeserializeFromJson<ulong[]>();

                        /*                        if (business is Game.Businesses.Farm)
                                                {

                                                }*/
                    }
                }
            }

            return true;
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

        public static void BusinessUpdateBalances(Game.Businesses.Business business)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE businesses SET Cash=@Cash, Bank=@Bank, Materials=@Mats WHERE ID=@ID;";

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

            cmd.CommandText = "UPDATE businesses SET Statistics=@Stats, IncassationState=@IncState, OrderedMaterials=@OMats WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", business.ID);

            cmd.Parameters.AddWithValue("@Stats", business.Statistics.SerializeToJson());
            cmd.Parameters.AddWithValue("@IncState", business.IncassationState);
            cmd.Parameters.AddWithValue("@OMats", business.OrderedMaterials);

            PushQuery(cmd);
        }
    }
}
