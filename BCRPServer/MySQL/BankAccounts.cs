using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static Game.Bank.Account GetBankAccountByCID(uint cid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM bank_accounts WHERE CID=@CID";
                    cmd.Parameters.AddWithValue("@CID", cid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var result = new List<PlayerData.Punishment>();

                        if (reader.HasRows)
                        {
                            reader.Read();

                            var balance = (int)reader["Balance"];
                            var savings = (int)reader["Savings"];
                            var tariff = (Game.Bank.Tariff.Types)(int)reader["Tariff"];
                            var std = (bool)reader["STD"];

                            return new Game.Bank.Account()
                            {
                                Balance = balance,
                                SavingsBalance = savings,
                                Tariff = Game.Bank.Tariff.All[tariff],
                                SavingsToDebit = std,
                                MinSavingsBalance = savings,
                                TotalDayTransactions = 0,
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public static void BankAccountAdd(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO bank_accounts (CID, Balance, Savings, Tariff, STD) VALUES (@CID, @Balance, @Savings, @Tariff, @STD);";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("Balance", account.Balance);
            cmd.Parameters.AddWithValue("Savings", account.SavingsBalance);
            cmd.Parameters.AddWithValue("Tariff", (int)account.Tariff.Type);
            cmd.Parameters.AddWithValue("STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountUpdate(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE bank_accounts SET Balance=@Balance, Savings=@Savings, Tariff=@Tariff, STD=@STD WHERE CID=@CID;";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("Balance", account.Balance);
            cmd.Parameters.AddWithValue("Savings", account.SavingsBalance);
            cmd.Parameters.AddWithValue("Tariff", (int)account.Tariff.Type);
            cmd.Parameters.AddWithValue("STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountDelete(Game.Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM bank_accounts WHERE CID=@CID;";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            PushQuery(cmd);
        }
    }
}
