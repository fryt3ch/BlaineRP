﻿using MySqlConnector;
using System;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        public static void GarageUpdateOwner(Game.Estates.Garage garage)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE garages SET CID=@CID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", garage.Id);

            if (garage.Owner == null)
                cmd.Parameters.AddWithValue("@CID", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@CID", garage.Owner.CID);

            PushQuery(cmd);
        }

        public static void GarageUpdateOnRestart(Game.Estates.Garage garage)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE garages SET IsLocked=@IsLocked WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", garage.Id);

            cmd.Parameters.AddWithValue("@IsLocked", garage.IsLocked);

            PushQuery(cmd);
        }

        public static void GarageUpdateBalance(Game.Estates.Garage garage)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE garages SET Balance=@Bal WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", garage.Id);

            cmd.Parameters.AddWithValue("@Bal", garage.Balance);

            PushQuery(cmd);
        }

        public static void LoadGarage(Game.Estates.Garage garage)
        {
            using (var conn = new MySqlConnection(_localConnectionCredentials.ConnectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM garages WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", garage.Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return;
                        }

                        reader.Read();

                        if (reader["CID"] is DBNull)
                            garage.UpdateOwner(null);
                        else
                        {
                            var pInfo = PlayerInfo.Get(Convert.ToUInt32(reader["CID"]));

                            garage.UpdateOwner(pInfo);
                        }

                        garage.Balance = Convert.ToUInt64(reader["Balance"]);
                    }
                }
            }
        }
    }
}
