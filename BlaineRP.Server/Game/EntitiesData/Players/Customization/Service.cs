using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization
{
    public static partial class Service
    {
        private static readonly Dictionary<Type, int> _clothesTypesDict = new Dictionary<Type, int>()
        {
            { typeof(Items.Top), (int)ClothesTypes.Top },
            { typeof(Items.Under), (int)ClothesTypes.Under },
            { typeof(Items.Pants), (int)ClothesTypes.Pants },
            { typeof(Items.Shoes), (int)ClothesTypes.Shoes },
            { typeof(Items.Gloves), (int)ClothesTypes.Gloves },
            { typeof(Items.Mask), (int)ClothesTypes.Mask },
            { typeof(Items.Accessory), (int)ClothesTypes.Accessory },
            { typeof(Items.Bag), (int)ClothesTypes.Bag },
            { typeof(Items.Armour), (int)ClothesTypes.Armour },
            { typeof(Items.Holster), (int)ClothesTypes.Decals },

            { typeof(Items.Hat), (int)AccessoryTypes.Hat },
            { typeof(Items.Glasses), (int)AccessoryTypes.Glasses },
            { typeof(Items.Earrings), (int)AccessoryTypes.Earrings },
            { typeof(Items.Watches), (int)AccessoryTypes.Watches },
            { typeof(Items.Bracelet), (int)AccessoryTypes.Bracelet },
        };

        public static int GetClothesIdxByType(Type type) => _clothesTypesDict.GetValueOrDefault(type);

        private static readonly Dictionary<ClothesTypes, int> _maleNudeDrawables = new Dictionary<ClothesTypes, int>()
        {
            { ClothesTypes.Top, 15 },
            { ClothesTypes.Under, 15 },
            { ClothesTypes.Gloves, 15 },
            { ClothesTypes.Pants, 21 },
            { ClothesTypes.Shoes, 34 },
            { ClothesTypes.Accessory, 0 },
            { ClothesTypes.Mask, 0 },
            { ClothesTypes.Bag, 0 },
        };

        private static readonly Dictionary<ClothesTypes, int> _femaleNudeDrawables = new Dictionary<ClothesTypes, int>()
        {
            { ClothesTypes.Top, 15 },
            { ClothesTypes.Under, 15 },
            { ClothesTypes.Gloves, 15 },
            { ClothesTypes.Pants, 15 },
            { ClothesTypes.Shoes, 35 },
            { ClothesTypes.Accessory, 0 },
            { ClothesTypes.Mask, 0 },
            { ClothesTypes.Bag, 0 }
        };

        public static int GetHair(bool sex, int id)
        {
            if (sex)
            {
                return _maleHairs.GetValueOrDefault(id);
            }
            else
            {
                return _femaleHairs.GetValueOrDefault(id);
            }
        }

        public static int GetNudeDrawable(ClothesTypes cType, bool sex)
        {
            if (sex)
            {
                return _maleNudeDrawables.GetValueOrDefault(cType);
            }
            else
            {
                return _femaleNudeDrawables.GetValueOrDefault(cType);
            }
        }
    }
}
