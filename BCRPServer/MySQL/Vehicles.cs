using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void VehicleAdd(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO vehicles (ID, SID, OwnerType, OwnerID, AllKeys, RegDate, Numberplate, Tuning, TID, LastData) VALUES (@ID, @SID, @OwnerType, @OwnerID, @AllKeys, @RegDate, @Numberplate, @Tuning, @TID, @LastData);";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);
            cmd.Parameters.AddWithValue("@SID", vInfo.ID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            cmd.Parameters.AddWithValue("@AllKeys", vInfo.AllKeys.SerializeToJson());
            cmd.Parameters.AddWithValue("@RegDate", vInfo.RegistrationDate);
            cmd.Parameters.AddWithValue("@Numberplate", vInfo.Numberplate?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            cmd.Parameters.AddWithValue("@TID", vInfo.TID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleKeysUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET AllKeys=@Keys WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Keys", vInfo.AllKeys.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleNumberplateUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Numberplate=@Numberplate WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Numberplate", vInfo.Numberplate?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void VehicleOwnerUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET OwnerType=@OwnerType, OwnerID=@OwnerID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            PushQuery(cmd);
        }

        public static void VehicleDeletionUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET LastData=@LastData WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleTuningUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Tuning=@Tuning WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleDelete(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM vehicles WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            PushQuery(cmd);
        }
    }
}
