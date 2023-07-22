using GTANetworkAPI;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Businesses
{
    public class BarberShop : ClothesShop
    {
        public static Types DefaultType => Types.BarberShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "hair_m_0", 10 },
                { "hair_m_1", 10 },
                { "hair_m_2", 10 },
                { "hair_m_3", 10 },
                { "hair_m_4", 10 },
                { "hair_m_5", 10 },
                { "hair_m_6", 10 },
                { "hair_m_7", 10 },
                { "hair_m_8", 10 },
                { "hair_m_9", 10 },
                { "hair_m_10", 10 },
                { "hair_m_11", 10 },
                { "hair_m_12", 10 },
                { "hair_m_13", 10 },
                { "hair_m_14", 10 },
                { "hair_m_15", 10 },
                { "hair_m_16", 10 },
                { "hair_m_17", 10 },
                { "hair_m_18", 10 },
                { "hair_m_19", 10 },
                { "hair_m_20", 10 },
                { "hair_m_21", 10 },
                { "hair_m_22", 10 },
                { "hair_m_23", 10 },
                { "hair_m_24", 10 },
                { "hair_m_25", 10 },
                { "hair_m_26", 10 },
                { "hair_m_27", 10 },
                { "hair_m_28", 10 },
                { "hair_m_29", 10 },
                { "hair_m_30", 10 },
                { "hair_m_31", 10 },
                { "hair_m_32", 10 },
                { "hair_m_33", 10 },
                { "hair_m_34", 10 },
                { "hair_m_35", 10 },
                { "hair_m_36", 10 },
                { "hair_m_37", 10 },
                { "hair_m_38", 10 },
                { "hair_m_39", 10 },
                { "hair_m_40", 10 },

                { "hair_f_0", 10 },
                { "hair_f_1", 10 },
                { "hair_f_2", 10 },
                { "hair_f_3", 10 },
                { "hair_f_4", 10 },
                { "hair_f_5", 10 },
                { "hair_f_6", 10 },
                { "hair_f_7", 10 },
                { "hair_f_8", 10 },
                { "hair_f_9", 10 },
                { "hair_f_10", 10 },
                { "hair_f_11", 10 },
                { "hair_f_12", 10 },
                { "hair_f_13", 10 },
                { "hair_f_14", 10 },
                { "hair_f_15", 10 },
                { "hair_f_16", 10 },
                { "hair_f_17", 10 },
                { "hair_f_18", 10 },
                { "hair_f_19", 10 },
                { "hair_f_20", 10 },
                { "hair_f_21", 10 },
                { "hair_f_22", 10 },
                { "hair_f_23", 10 },
                { "hair_f_24", 10 },
                { "hair_f_25", 10 },
                { "hair_f_26", 10 },
                { "hair_f_27", 10 },
                { "hair_f_28", 10 },
                { "hair_f_29", 10 },
                { "hair_f_30", 10 },
                { "hair_f_31", 10 },
                { "hair_f_32", 10 },
                { "hair_f_33", 10 },
                { "hair_f_34", 10 },
                { "hair_f_35", 10 },
                { "hair_f_36", 10 },
                { "hair_f_37", 10 },
                { "hair_f_38", 10 },
                { "hair_f_39", 10 },
                { "hair_f_40", 10 },
                { "hair_f_41", 10 },
                { "hair_f_42", 10 },

                { "beard_255", 10 },
                { "beard_0", 10 },
                { "beard_1", 10 },
                { "beard_2", 10 },
                { "beard_3", 10 },
                { "beard_4", 10 },
                { "beard_5", 10 },
                { "beard_6", 10 },
                { "beard_7", 10 },
                { "beard_8", 10 },
                { "beard_9", 10 },
                { "beard_10", 10 },
                { "beard_11", 10 },
                { "beard_12", 10 },
                { "beard_13", 10 },
                { "beard_14", 10 },
                { "beard_15", 10 },
                { "beard_16", 10 },
                { "beard_17", 10 },
                { "beard_18", 10 },
                { "beard_19", 10 },
                { "beard_20", 10 },
                { "beard_21", 10 },
                { "beard_22", 10 },
                { "beard_23", 10 },
                { "beard_24", 10 },
                { "beard_25", 10 },
                { "beard_26", 10 },
                { "beard_27", 10 },

                { "chest_255", 10 },
                { "chest", 10 },

                { "eyebrows_255", 10 },
                { "eyebrows", 10 },

                { "makeup_255", 10 },
                { "makeup", 10 },

                { "blush_255", 10 },
                { "blush", 10 },

                { "lipstick_255", 10 },
                { "lipstick", 10 },
            }
        };

        public BarberShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, DefaultType, PositionInteract)
        {

        }

        private static Dictionary<string, int> HeadOverlayNums = new Dictionary<string, int>()
        {
            { "beard", 1 },
            { "eyebrows", 2 },
            { "makeup", 4 },
            { "blush", 5 },
            { "lipstick", 8 },
            { "chest", 10 },
        };

        public override bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            //Console.WriteLine(itemId);

            var iData = itemId.Split('&');

            if (iData.Length < 2)
                return false;

            var itemIdData = iData[0].Split('_');

            if (itemIdData.Length < 2)
                return false;

            var realItemId = itemIdData[0];

            byte variation;

            if (realItemId == "hair")
            {
                if (itemIdData.Length != 3 || iData.Length != 4)
                    return false;

                if ((itemIdData[1] != "m" && itemIdData[1] != "f") || (itemIdData[1] == "m" && !pData.Sex))
                    return false;

                if (!byte.TryParse(itemIdData[2], out variation))
                    return false;

                realItemId = iData[0];
            }
            else
            {
                if (itemIdData.Length != 2)
                    return false;

                if (realItemId == "beard" || realItemId == "chest")
                {
                    if (!pData.Sex)
                        return false;
                }

                if (!byte.TryParse(itemIdData[1], out variation))
                    return false;

                if (variation == 255 || realItemId == "beard")
                    realItemId = iData[0];
            }

            uint newMats;
            ulong newBalance, newPlayerBalance;

            if (!TryProceedPayment(pData, useCash, realItemId, 1, out newMats, out newBalance, out newPlayerBalance))
                return false;

            if (itemIdData[0] == "hair")
            {
                byte hairOverlayIdx, colour1, colour2;

                if (!byte.TryParse(iData[1], out hairOverlayIdx) || !byte.TryParse(iData[2], out colour1) || !byte.TryParse(iData[3], out colour2))
                    return false;

                // hair overlay & colours validation

                pData.Info.HairStyle.Id = variation;
                pData.Info.HairStyle.Overlay = hairOverlayIdx;
                pData.Info.HairStyle.Color = colour1;
                pData.Info.HairStyle.Color2 = colour2;

                MySQL.CharacterHairStyleUpdate(pData.Info);
            }
            else
            {
                float opacity = 1f;
                byte colour;

                if (!byte.TryParse(iData[1], out colour))
                    return false;

                if (itemIdData[0] != "beard" && itemIdData[0] != "chest" && itemIdData[0] != "eyebrows")
                {
                    if (iData.Length != 3)
                        return false;

                    if (!float.TryParse(iData[2], out opacity))
                        return false;
                }

                int headOverlayIdx;

                if (!HeadOverlayNums.TryGetValue(itemIdData[0], out headOverlayIdx))
                    return false;

                var headOverlay = pData.Info.HeadOverlays[headOverlayIdx];

                headOverlay.Color = colour;
                headOverlay.SecondaryColor = colour;
                headOverlay.Index = variation;
                headOverlay.Opacity = opacity;

                MySQL.CharacterHeadOverlaysUpdate(pData.Info);
            }

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            return true;
        }
    }
}
