using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Bank
{
    public class Account
    {
        public int ID { get; set; }
        public int CID { get; set; }
        public int Balance { get; set; }
        public int Savings { get; set; }
        public int DepositBalance { get; set; }
        public DateTime DepositDate { get; set; }
    }
}
