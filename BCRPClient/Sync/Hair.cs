using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Sync
{
    class Hair : Events.Script
    {
        public Hair()
        {
            Events.AddDataHandler("Customization::HairOverlay", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Player)
                    return;

                var player = entity as Player;

                var pData = Sync.Players.GetData(player);

                if (pData == null)
                    return;

                pData.HairOverlay = Data.Customization.GetHairOverlay(player.IsMale(), (int)value);
            });
        }
    }
}
