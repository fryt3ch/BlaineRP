using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        public partial class Style
        {
            internal class RemoteEvents : Script
            {
                [RemoteEvent("House::FSOV")]
                private static void StopStyleOverview(Player player, ushort currentStyleId)
                {
                    (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                    if (sRes.IsSpammer)
                        return;

                    PlayerData pData = sRes.Data;

                    if (pData.IsCuffed || pData.IsFrozen)
                        return;

                    HouseBase house = pData.CurrentHouseBase;

                    if (house == null)
                        return;

                    if (house.StyleData.IsPositionInsideInterior(player.Position))
                        return;

                    house.SetPlayersInside(true, player);
                }

                [RemoteProc("House::SSOV")]
                private static bool StartStyleOverview(Player player, ushort styleId, ushort currentStyleId)
                {
                    (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                    if (sRes.IsSpammer)
                        return false;

                    PlayerData pData = sRes.Data;

                    if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                        return false;

                    HouseBase house = pData.CurrentHouseBase;

                    if (house == null)
                        return false;

                    if (house.Owner != pData.Info)
                    {
                        player.Notify("House::NotAllowed");

                        return false;
                    }

                    if (styleId == currentStyleId)
                        return false;

                    var style = Get(styleId);

                    if (style == null)
                        return false;

                    var currentStyle = Get(currentStyleId);

                    if (currentStyle == null)
                        return false;

                    if (!style.IsHouseTypeSupported(house.Type) || !style.IsRoomTypeSupported(house.RoomType))
                        return false;

                    if (style.IsTypeFamiliar(currentStyleId) && currentStyle.IsPositionInsideInterior(player.Position))
                    {
                        Vector3 offset = style.InteriorPosition.Position - currentStyle.InteriorPosition.Position;

                        player.Teleport(player.Position + offset, false, null, null, false);
                    }
                    else
                    {
                        player.Teleport(style.Position, false, null, style.Heading, false);
                    }

                    return true;
                }

                [RemoteProc("House::BST")]
                private static bool BuyStyle(Player player, ushort styleId, bool useCash)
                {
                    (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                    if (sRes.IsSpammer)
                        return false;

                    PlayerData pData = sRes.Data;

                    if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                        return false;

                    HouseBase house = pData.CurrentHouseBase;

                    if (house == null)
                        return false;

                    if (house.Owner != pData.Info)
                    {
                        player.Notify("House::NotAllowed");

                        return false;
                    }

                    var style = Get(styleId);

                    if (style == null)
                        return false;

                    if (house.StyleType == styleId)
                        return false;

                    if (!style.IsHouseTypeSupported(house.Type) || !style.IsRoomTypeSupported(house.RoomType))
                        return false;

                    uint price = style.Price;

                    if (useCash)
                    {
                        ulong newBalance;

                        if (!pData.TryRemoveCash(price, out newBalance, true, null))
                            return false;

                        pData.SetCash(newBalance);
                    }
                    else
                    {
                        if (!pData.HasBankAccount(true))
                            return false;

                        ulong newBalance;

                        if (!pData.BankAccount.TryRemoveMoneyDebit(price, out newBalance, true, null))
                            return false;

                        pData.BankAccount.SetDebitBalance(newBalance, null);
                    }

                    house.SetStyle(styleId, style, true);

                    return true;
                }
            }
        }
    }
}