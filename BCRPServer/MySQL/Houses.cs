using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void LoadHouse(Game.Estates.House house)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM houses WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", house.Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return;

                        reader.Read();

                        if (reader["CID"] is DBNull)
                            house.UpdateOwner(null);
                        else
                        {
                            var pInfo = PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"]));

                            pInfo?.OwnedHouses.Add(house);

                            house.UpdateOwner(pInfo);
                        }

                        house.Balance = Convert.ToUInt64(reader["Balance"]);

                        house.StyleType = Convert.ToUInt16(reader["StyleType"]);

                        house.Settlers = NAPI.Util.FromJson<Dictionary<uint, bool[]>>((string)reader["Settlers"]).ToDictionary(x => PlayerData.PlayerInfo.Get(x.Key), x => x.Value);

                        house.IsLocked = (bool)reader["IsLocked"];
                        house.ContainersLocked = (bool)reader["ContainersLocked"];

                        house.Furniture = ((string)reader["Furniture"]).DeserializeFromJson<List<uint>>().Select(x => Game.Estates.Furniture.Get(x)).Where(x => x != null).ToList();

                        cmd.CommandText = "";

                        if (reader["Locker"] == DBNull.Value)
                        {
                            house.Locker = Game.Items.Container.Create("h_locker", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Locker={house.Locker} WHERE ID={house.Id};";
                        }
                        else
                            house.Locker = Convert.ToUInt32(reader["Locker"]);

                        if (reader["Wardrobe"] == DBNull.Value)
                        {
                            house.Wardrobe = Game.Items.Container.Create("h_wardrobe", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Wardrobe={house.Wardrobe} WHERE ID={house.Id};";
                        }
                        else
                            house.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                        if (reader["Fridge"] == DBNull.Value)
                        {
                            house.Fridge = Game.Items.Container.Create("h_fridge", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Fridge={house.Fridge} WHERE ID={house.Id};";
                        }
                        else
                            house.Fridge = Convert.ToUInt32(reader["Fridge"]);

                        if (reader["DoorsStates"] == DBNull.Value)
                        {
                            house.DoorsStates = new bool[house.StyleData.DoorsAmount];

                            cmd.CommandText += $"UPDATE houses SET DoorsStates='{house.DoorsStates.SerializeToJson()}' WHERE ID={house.Id};";
                        }
                        else
                            house.DoorsStates = NAPI.Util.FromJson<bool[]>((string)reader["DoorsStates"]);

                        if (reader["LightsStates"] == DBNull.Value)
                        {
                            house.LightsStates = new Game.Estates.HouseBase.Light[house.StyleData.LightsAmount];

                            for (int i = 0; i < house.LightsStates.Length; i++)
                            {
                                house.LightsStates[i] = new Game.Estates.HouseBase.Light();

                                house.LightsStates[i].Colour = Game.Estates.HouseBase.DefaultLightColour;
                                house.LightsStates[i].State = true;
                            }

                            cmd.CommandText += $"UPDATE houses SET LightsStates='{house.LightsStates.SerializeToJson()}' WHERE ID={house.Id};";
                        }
                        else
                            house.LightsStates = NAPI.Util.FromJson<Game.Estates.HouseBase.Light[]>((string)reader["LightsStates"]);
                    }

                    if (cmd.CommandText.Length > 0)
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public static void LoadApartments(Game.Estates.Apartments apartments)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM apartments WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", apartments.Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return;
                        }

                        reader.Read();

                        if (reader["CID"] is DBNull)
                            apartments.UpdateOwner(null);
                        else
                        {
                            var pInfo = PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"]));

                            pInfo?.OwnedApartments.Add(apartments);

                            apartments.UpdateOwner(pInfo);
                        }

                        apartments.Balance = Convert.ToUInt64(reader["Balance"]);

                        apartments.StyleType = Convert.ToUInt16(reader["StyleType"]);

                        apartments.Settlers = NAPI.Util.FromJson<Dictionary<uint, bool[]>>((string)reader["Settlers"]).ToDictionary(x => PlayerData.PlayerInfo.Get(x.Key), x => x.Value);

                        apartments.IsLocked = (bool)reader["IsLocked"];
                        apartments.ContainersLocked = (bool)reader["ContainersLocked"];

                        apartments.Furniture = ((string)reader["Furniture"]).DeserializeFromJson<List<uint>>().Select(x => Game.Estates.Furniture.Get(x)).Where(x => x != null).ToList();

                        cmd.CommandText = "";

                        if (reader["Locker"] == DBNull.Value)
                        {
                            apartments.Locker = Game.Items.Container.Create("a_locker", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Locker={apartments.Locker} WHERE ID={apartments.Id};";
                        }
                        else
                            apartments.Locker = Convert.ToUInt32(reader["Locker"]);

                        if (reader["Wardrobe"] == DBNull.Value)
                        {
                            apartments.Wardrobe = Game.Items.Container.Create("a_wardrobe", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Wardrobe={apartments.Wardrobe} WHERE ID={apartments.Id};";
                        }
                        else
                            apartments.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                        if (reader["Fridge"] == DBNull.Value)
                        {
                            apartments.Fridge = Game.Items.Container.Create("a_fridge", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Fridge={apartments.Fridge} WHERE ID={apartments.Id};";
                        }
                        else
                            apartments.Fridge = Convert.ToUInt32(reader["Fridge"]);

                        if (reader["DoorsStates"] == DBNull.Value)
                        {
                            apartments.DoorsStates = new bool[apartments.StyleData.DoorsAmount];

                            cmd.CommandText += $"UPDATE houses SET DoorsStates='{apartments.DoorsStates.SerializeToJson()}' WHERE ID={apartments.Id};";
                        }
                        else
                            apartments.DoorsStates = NAPI.Util.FromJson<bool[]>((string)reader["DoorsStates"]);

                        if (reader["LightsStates"] == DBNull.Value)
                        {
                            apartments.LightsStates = new Game.Estates.HouseBase.Light[apartments.StyleData.LightsAmount];

                            for (int i = 0; i < apartments.LightsStates.Length; i++)
                            {
                                apartments.LightsStates[i].Colour = Game.Estates.HouseBase.DefaultLightColour;
                                apartments.LightsStates[i].State = true;
                            }

                            cmd.CommandText += $"UPDATE houses SET LightsStates='{apartments.LightsStates.SerializeToJson()}' WHERE ID={apartments.Id};";
                        }
                        else
                            apartments.LightsStates = NAPI.Util.FromJson<Game.Estates.HouseBase.Light[]>((string)reader["LightsStates"]);
                    }

                    if (cmd.CommandText.Length > 0)
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public static void FurnitureAdd(Game.Estates.Furniture furn)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO furniture (ID, Type, Data) VALUES (@ID, @Type, @Data);";

            cmd.Parameters.AddWithValue("@ID", furn.UID);
            cmd.Parameters.AddWithValue("@Type", furn.ID);
            cmd.Parameters.AddWithValue("@Data", furn.Data.SerializeToJson());

            PushQuery(cmd);
        }

        public static void FurnitureUpdate(Game.Estates.Furniture furn)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE furniture SET Data=@Data WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", furn.UID);
            cmd.Parameters.AddWithValue("@Data", furn.Data.SerializeToJson());

            PushQuery(cmd);
        }

        public static void FurnitureDelete(Game.Estates.Furniture furn)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM furniture WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", furn.UID);

            PushQuery(cmd);
        }

        public static void HouseFurnitureUpdate(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            if (house.Type == Game.Estates.HouseBase.Types.House)
                cmd.CommandText = "UPDATE houses SET Furniture=@Furniture WHERE ID=@ID;";
            else
                cmd.CommandText = "UPDATE apartments SET Furniture=@Furniture WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);
            cmd.Parameters.AddWithValue("@Furniture", house.Furniture.Select(x => x.UID).SerializeToJson());

            PushQuery(cmd);
        }

        public static void HouseUpdateOwner(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            if (house.Type == Game.Estates.HouseBase.Types.House)
                cmd.CommandText = "UPDATE houses SET CID=@CID WHERE ID=@ID;";
            else
                cmd.CommandText = "UPDATE apartments SET CID=@CID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            if (house.Owner == null)
                cmd.Parameters.AddWithValue("@CID", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@CID", house.Owner.CID);

            PushQuery(cmd);
        }

        public static void HouseUpdateBalance(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            if (house.Type == Game.Estates.HouseBase.Types.House)
                cmd.CommandText = "UPDATE houses SET Balance=@Bal WHERE ID=@ID;";
            else
                cmd.CommandText = "UPDATE apartments SET Balance=@Bal WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@Bal", house.Balance);

            PushQuery(cmd);
        }

        public static void HouseUpdateSettlers(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            if (house.Type == Game.Estates.HouseBase.Types.House)
                cmd.CommandText = "UPDATE houses SET Settlers=@Settlers WHERE ID=@ID;";
            else
                cmd.CommandText = "UPDATE apartments SET Settlers=@Settlers WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@Settlers", house.Settlers.ToDictionary(x => x.Key.CID, x => x.Value).SerializeToJson());

            PushQuery(cmd);
        }

        public static void HouseUpdateOnRestart(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET IsLocked=@IsLocked, ContainersLocked=@ContLocked, DoorsStates=@DStates, LightsStates=@LStates WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@IsLocked", house.IsLocked);
            cmd.Parameters.AddWithValue("@ContLocked", house.ContainersLocked);

            cmd.Parameters.AddWithValue("@DStates", house.DoorsStates.SerializeToJson());
            cmd.Parameters.AddWithValue("@LStates", house.LightsStates.SerializeToJson());

            PushQuery(cmd);
        }

        public static void HouseUpdateDoorsStates(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET DoorsStates=@DStates WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@DStates", house.DoorsStates.SerializeToJson());

            PushQuery(cmd);
        }

        public static void HouseUpdateLightsStates(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET LightsStates=@LStates WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@LStates", house.LightsStates.SerializeToJson());

            PushQuery(cmd);
        }

        public static void HouseUpdateStyleType(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET StyleType=@SType WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@SType", house.StyleType);

            PushQuery(cmd);
        }

        public static void HouseUpdateLockState(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET IsLocked=@IL WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@IL", house.IsLocked);

            PushQuery(cmd);
        }

        public static void HouseUpdateContainersLockState(Game.Estates.HouseBase house)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE {(house.Type == Game.Estates.HouseBase.Types.House ? "houses" : "apartments")} SET ContainersLocked=@CL WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", house.Id);

            cmd.Parameters.AddWithValue("@CL", house.ContainersLocked);

            PushQuery(cmd);
        }
    }
}
