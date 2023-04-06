using MySql.Data.MySqlClient;
using System;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void BankAccountAdd(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO bank_accounts (ID, Balance, Savings, Tariff, STD) VALUES (@ID, @Balance, @Savings, @Tariff, @STD);";

            cmd.Parameters.AddWithValue("@ID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("@Balance", account.Balance);
            cmd.Parameters.AddWithValue("@Savings", account.SavingsBalance);
            cmd.Parameters.AddWithValue("@Tariff", (int)account.Tariff.Type);
            cmd.Parameters.AddWithValue("@STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountUpdate(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE bank_accounts SET STD=@STD WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("@STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountBalancesUpdate(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE bank_accounts SET Balance=@Balance, Savings=@Savings WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("@Balance", account.Balance);
            cmd.Parameters.AddWithValue("@Savings", account.SavingsBalance);

            PushQuery(cmd);
        }

        public static void BankAccountTariffUpdate(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE bank_accounts SET Tariff=@Tariff WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("@Tariff", (int)account.Tariff.Type);

            PushQuery(cmd);
        }

        public static void BankAccountDelete(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM bank_accounts WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", account.PlayerInfo.CID);

            PushQuery(cmd);
        }
    }
}
