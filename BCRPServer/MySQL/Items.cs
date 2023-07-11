using MySqlConnector;
using System.Linq;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void ItemUpdate(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            if (item is Game.Items.IContainer cont)
            {
                cmd.CommandText = "UPDATE items SET Data=@Data, Items=@Items WHERE ID=@ID;";

                cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            }
            else
                cmd.CommandText = "UPDATE items SET Data=@Data WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", item.UID);
            cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

            PushQuery(cmd);
        }

        public static void ItemAdd(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            if (item is Game.Items.IContainer cont)
            {
                cmd.CommandText = "INSERT INTO items (ID, Data, Items) VALUES (@ID, @Data, @Items);";

                cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            }
            else
            {
                cmd.CommandText = "INSERT INTO items (ID, Data) VALUES (@ID, @Data);";
            }

            cmd.Parameters.AddWithValue("@ID", item.UID);

            cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

            PushQuery(cmd);
        }

        public static void ItemDelete(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM items WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", item.UID);

            PushQuery(cmd);
        }
    }
}
