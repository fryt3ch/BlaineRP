using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Web.SocketIO.Events.Auth
{
    public class Account
    {
        [Event("Auth::Account::Confirmed")]
        private static void RegistrationConfirmed(SocketIOClient.SocketIOResponse response)
        {
            var aData = JObject.Parse(response.GetValue(0).GetRawText());

            var login = (string)aData.GetValue("login");
            var id = Convert.ToUInt32(aData.GetValue("id"));
            var mail = (string)aData.GetValue("mail");
            var ip = (string)aData.GetValue("regIp");
            var scid = Convert.ToUInt64(aData.GetValue("scid"));
            var hwid = (string)aData.GetValue("hwid");
            var confirmHash = (string)aData["confirmHash"];
            var cancelledConfirmHashes = aData["cancelledConfirmHashes"].ToObject<HashSet<string>>();

            NAPI.Task.Run(() =>
            {
                foreach (var x in TempData.Players)
                {
                    var confirmHashT = x.Value.RegistrationConfirmationHash;

                    if (confirmHashT == null)
                        continue;

                    if (confirmHashT == confirmHash)
                    {
                        confirmHash = null;

                        x.Value.RegistrationConfirmationHash = null;

                        x.Value.BlockRemoteCalls = false;

                        x.Value.Player.NotifySuccess(Language.Strings.Get("NTFC_AUTH_REGCONFIRMED_0"));

                        // todo
                    }
                    else if (cancelledConfirmHashes.Remove(confirmHashT))
                    {
                        // todo
                    }
                }
            });
        }
    }
}
