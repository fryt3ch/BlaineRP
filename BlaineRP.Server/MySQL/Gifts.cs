﻿using MySqlConnector;
using System;
using BlaineRP.Server.Game.Gifts;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        public static void GiftAdd(Gift gift, uint cid)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO gifts (ID, CID, Type, GID, Variation, Amount, Reason) VALUES (@ID, @CID, @Type, @GID, @Variation, @Amount, @Reason);";

            cmd.Parameters.AddWithValue("@ID", gift.ID);

            cmd.Parameters.AddWithValue("@CID", cid);
            cmd.Parameters.AddWithValue("@Type", gift.Type);
            cmd.Parameters.AddWithValue("@GID", (object)gift.GID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Variation", gift.Variation);
            cmd.Parameters.AddWithValue("@Amount", gift.Amount);
            cmd.Parameters.AddWithValue("@Reason", gift.SourceType);

            PushQuery(cmd);
        }

        public static void GiftDelete(Gift gift)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM gifts WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", gift.ID);

            PushQuery(cmd);
        }
    }
}
