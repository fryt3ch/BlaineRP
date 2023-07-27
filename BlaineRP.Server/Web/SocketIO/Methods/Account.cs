using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Web.SocketIO.Methods
{
    internal class Account
    {
        public static async Task<string> Add(CancellationToken ct, ulong scid, string hwid, string login, string password, string mail, string ip)
        {
            var resp = await Service.Connection.TriggerProc(ct, "Auth::Account::Reg", new { scid = scid, hwid = hwid, login = login, password = password, mail = mail, ip = ip });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 1)
            {
                return (string)data.GetValue("confirmHash");
            }
            else
            {
                var ioEx = new Web.SocketIO.Exceptions.SocketIOResultException(result);

                var errorMsg = (string)data.GetValue("error");

                if (errorMsg == "ConfirmSendTimeout")
                {
                    ioEx.Data.Add("NextSendTryTime", TimeSpan.FromSeconds(Convert.ToInt64(data.GetValue("time"))));
                }

                throw ioEx;
            }
        }

        public static async Task<uint> GetIdBySCID(CancellationToken ct, ulong socialClubId)
        {
            var resp = await Web.SocketIO.Service.Connection.TriggerProc(ct, "Auth::Account::GetId", new { scid = socialClubId });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255, null);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 1)
            {
                var aidToken = data.GetValue("aid");

                if (aidToken == null)
                    return 0;

                return Convert.ToUInt32(aidToken);
            }
            else
            {
                var errorMsg = (string)data.GetValue("error");

                throw new Web.SocketIO.Exceptions.SocketIOResultException(result, resp, errorMsg);
            }
        }

        public static async Task<AccountData> Login(CancellationToken ct, ulong scid, string login, string password, string ip)
        {
            var resp = await Web.SocketIO.Service.Connection.TriggerProc(ct, "Auth::Account::Login", new { scid = scid, login = login, password = password, ip = ip });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255, null);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 1)
            {
                var aDataToken = data.GetValue("aData");

                var aData = new AccountData()
                {
                    ID = Convert.ToUInt32(aDataToken["id"]),
                    SCID = scid,
                    HWID = (string)aDataToken["hwid"],
                    Login = login,
                    Mail = (string)aDataToken["mail"],
                    RegistrationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64((aDataToken["regDate"]))).DateTime,
                    RegistrationIP = (string)aDataToken["regIP"],
                    LastIP = (string)aDataToken["lastIP"],
                    AdminLevel = (int)aDataToken["adminLvl"],
                    BCoins = Convert.ToUInt32(aDataToken["bcoins"]),
                };

                return aData;
            }
            else
            {
                var errorMsg = (string)data.GetValue("error");

                throw new Web.SocketIO.Exceptions.SocketIOResultException(result, resp, errorMsg);
            }
        }

        public static async Task<object> Login2FAMailCode(CancellationToken ct, AccountData aData, string mailCode)
        {
            var resp = await Web.SocketIO.Service.Connection.TriggerProc(ct, "Auth::Account::Login::2FA::Mail", new { aid = aData.ID, code = mailCode });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255, null);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 0)
            {
                return null; // todo
            }
            else
            {
                /*
                    * 1 - no account found
                    * 2 - wrong code
                    * 3 - 2fa mail isn't enabled
                    * 
                */
                throw new Web.SocketIO.Exceptions.SocketIOResultException(result, resp);
            }
        }
    }
}
