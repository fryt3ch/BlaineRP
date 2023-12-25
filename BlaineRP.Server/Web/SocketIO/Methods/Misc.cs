using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Web.SocketIO.Methods
{
    internal class Misc
    {
        public static async Task SqlTransactionLocalDbSend(CancellationToken ct, List<string> list)
        {
            var resp = await Service.Connection.TriggerProc(ct, "Db::LocalTransactions", new { list });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 1)
            {
                return;
            }
            else
            {
                var errorMsg = (string)data.GetValue("error");

                throw new Web.SocketIO.Exceptions.SocketIOResultException(result, resp, errorMsg);
            }
        }

        public static async Task<AccountData.GlobalBan> GetPlayerGlobalBan(CancellationToken ct, string hwid, ulong scid)
        {
            var resp = await Service.Connection.TriggerProc(ct, "Misc::Player::GetGlobalBan", new { hwid = hwid, scid = scid, });

            if (resp == null)
                throw new Web.SocketIO.Exceptions.SocketIOResultException(255);

            var data = JObject.Parse(resp.GetValue(0).GetRawText());

            var result = Convert.ToByte(data.GetValue("status"));

            if (result == 1)
            {
                var dataToken = data.GetValue("data");

                if (dataToken == null)
                    return null;

                var aData = new AccountData.GlobalBan(Convert.ToUInt32(dataToken["ID"]), dataToken["Date"].ToObject<DateTime>(), (string)dataToken["Reason"], Convert.ToUInt32(dataToken["AdminID"]), (string)dataToken["HWID"], Convert.ToUInt64(dataToken["SCID"]));

                return aData;
            }
            else
            {
                var errorMsg = data.GetValue("error").ToString();

                throw new Web.SocketIO.Exceptions.SocketIOResultException(result, resp, errorMsg);
            }
        }
    }
}
