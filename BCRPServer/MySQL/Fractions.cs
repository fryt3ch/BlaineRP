using BCRPServer.Game.Fractions;
using GTANetworkAPI;
using MySqlConnector;
using System;
using System.Linq;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void FractionUpdateNews(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET News=@News WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);
            cmd.Parameters.AddWithValue("@News", fraction.News.SerializeToJson());

            PushQuery(cmd);
        }

        public static void FractionUpdateRanks(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET Ranks=@Ranks WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);
            cmd.Parameters.AddWithValue("@Ranks", fraction.Ranks.SerializeToJson());

            PushQuery(cmd);
        }

        public static void FractionUpdateBalance(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET Balance=@Bal WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);
            cmd.Parameters.AddWithValue("@Bal", fraction.Balance);

            PushQuery(cmd);
        }

        public static void FractionUpdateMaterials(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET Materials=@Mats WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);
            cmd.Parameters.AddWithValue("@Mats", fraction.Materials);

            PushQuery(cmd);
        }

        public static void FractionUpdateLockStates(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET STL=@STL, CWL=@CWL WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);

            cmd.Parameters.AddWithValue("@STL", fraction.ContainerLocked);
            cmd.Parameters.AddWithValue("@CWL", fraction.CreationWorkbenchLocked);

            PushQuery(cmd);
        }

        public static void FractionUpdateLeader(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET Leader=@Leader, CWL=@CWL WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);

            if (fraction.Leader == null)
                cmd.Parameters.AddWithValue("@Leader", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Leader", fraction.Leader.CID);

            PushQuery(cmd);
        }

        public static void FractionUpdateVehicles(Fraction fraction)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE fractions SET Vehicles=@Vehs WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", (int)fraction.Type);

            cmd.Parameters.AddWithValue("@Vehs", fraction.AllVehicles.ToDictionary(x => x.Key.VID, x => x.Value).SerializeToJson());

            PushQuery(cmd);
        }

        public static void PoliceAPBAdd(uint id, Game.Fractions.Police.APBInfo apbInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO police_apbs (ID, Data) VALUES (@ID, @D);";

            cmd.Parameters.AddWithValue("@ID", id);

            cmd.Parameters.AddWithValue("@D", apbInfo.SerializeToJson());

            PushQuery(cmd);
        }

        public static void PoliceAPBDelete(uint id)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM police_apbs WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", id);

            PushQuery(cmd);
        }

        public static void PoliceAPBUpdate(uint id, Game.Fractions.Police.APBInfo apbInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE police_apbs SET Data=@D WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", id);

            cmd.Parameters.AddWithValue("@D", apbInfo.SerializeToJson());

            PushQuery(cmd);
        }
    }
}
