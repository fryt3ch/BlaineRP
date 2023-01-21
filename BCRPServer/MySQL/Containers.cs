using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void ContainerUpdate(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE containers SET Items=@Items WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", cont.ID);
            cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void ContainerAdd(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO containers (ID, SID, Items) VALUES (@ID, @SID, @Items);";

            cmd.Parameters.AddWithValue("@ID", cont.ID);

            cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@SID", cont.SID);

            PushQuery(cmd);
        }

        public static void ContainerDelete(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM containers WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", cont.ID);

            PushQuery(cmd);
        }
    }
}
