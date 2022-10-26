using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.CEF
{
    class Menu : Script
    {
        [RemoteEvent("Gift::Collect")]
        private static async Task CollectGift(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var gift = pData.Gifts.Where(x => x.ID == id).FirstOrDefault();

                if (gift == null)
                    return;

                if (await gift.Collect(pData))
                {
                    pData.Gifts.Remove(gift);

                    gift.Delete();

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Menu::Gifts::Update", false, id);
                    });
                }
            });

            pData.Release();
        }
    }
}
