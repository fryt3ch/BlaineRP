using MySqlConnector;
using System;
using BlaineRP.Server.Game.EntitiesData.Vehicles;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        public static void VehicleAdd(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO vehicles (ID, SID, OwnerType, OwnerID, AllKeys, RegDate, Numberplate, Tuning, TID, LastData) VALUES (@ID, @SID, @OwnerType, @OwnerID, @AllKeys, @RegDate, @Numberplate, @Tuning, @TID, @LastData);";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);
            cmd.Parameters.AddWithValue("@SID", vInfo.ID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            cmd.Parameters.AddWithValue("@RegDate", vInfo.RegistrationDate);
            cmd.Parameters.AddWithValue("@Numberplate", vInfo.Numberplate?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            cmd.Parameters.AddWithValue("@TID", vInfo.TID);
            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleKeysUidUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET KeysUid=@KUid WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            if (vInfo.KeysUid == Guid.Empty)
                cmd.Parameters.AddWithValue("@KUid", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@KUid", vInfo.KeysUid.ToString());

            PushQuery(cmd);
        }

        public static void VehicleNumberplateUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Numberplate=@Np WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Np", vInfo.Numberplate?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void VehicleRegisteredNumberplateUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET RegNumberplate=@RegNp WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            if (vInfo.RegisteredNumberplate == null)
                cmd.Parameters.AddWithValue("@RegNp", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@RegNp", vInfo.RegisteredNumberplate);

            PushQuery(cmd);
        }

        public static void VehicleOwnerUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET OwnerType=@OwnerType, OwnerID=@OwnerID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            PushQuery(cmd);
        }

        public static void VehicleDeletionUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET LastData=@LastData WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleTuningUpdate(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Tuning=@Tuning WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleDelete(VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM vehicles WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            PushQuery(cmd);
        }
    }
}
