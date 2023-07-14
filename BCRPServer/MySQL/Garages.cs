using MySqlConnector;
using System;

namespace BCRPServer
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
            using (var conn = new MySqlConnection(_localConnectionCredentials))
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
                            var pInfo = PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"]));

                            pInfo?.OwnedGarages.Add(garage);

                            garage.UpdateOwner(pInfo);
                        }

                        garage.Balance = Convert.ToUInt64(reader["Balance"]);
                    }
                }
            }
        }
    }
}
