using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.GiveHealingItem)]
    internal class GiveHealingItem : OfferBase
    {
        public override void OnAccept(PlayerData pData, PlayerData tData, Offer offer)
        {
            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

            if (pData == null || tData == null)
                return;

            var sPlayer = pData.Player;
            var tPlayer = tData.Player;

            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                return;

            if (!sPlayer.IsNearToEntity(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var healingItem = (Game.Items.Healing)offer.Data;

            var itemIdx = Array.IndexOf(pData.Items, healingItem);

            if (itemIdx < 0)
            {
                return;
            }

            if (!pData.CanUseInventory(false) || tData.IsAttachedToEntity != null || tData.IsAnyAnimOn() || tData.HasAnyItemInUse() || tData.HasAnyHandAttachedObject)
            {
                return;
            }

            healingItem.Amount--;

            if (healingItem.Amount <= 0)
            {
                healingItem.Delete();

                pData.Items[itemIdx] = null;

                MySQL.CharacterItemsUpdate(pData.Info);
            }
            else
            {
                healingItem.Update();
            }

            pData.Player.InventoryUpdate(GroupTypes.Items, itemIdx, Game.Items.Item.ToClientJson(pData.Items[itemIdx], GroupTypes.Items));

            healingItem.Apply(tData);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            if (!pData.CanUseInventory(true))
            {
                returnObj = 1;

                return false;
            }

            int itemIdx;
            Game.Items.Healing item;

            try
            {
                var jObj = JObject.Parse(dataStr);

                itemIdx = jObj["ItemIdx"].ToObject<int>();

                item = (Game.Items.Healing)pData.Items[itemIdx];

                if (item == null)
                    throw new Exception();
            }
            catch (Exception ex)
            {
                returnObj = 0;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, item);

            text = Language.Strings.Get("OFFER_GIVE_HEALINGITEM_TEXT", "{0}", item.Data.Name);

            return true;
        }
    }
}