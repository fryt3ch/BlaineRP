using System;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes.Uniforms
{
    public static class Service
    {
        // if >= 1000 -> prop

        private static Dictionary<UniformTypes, Dictionary<int, Tuple<int, int>[]>> _allUniforms = new Dictionary<UniformTypes, Dictionary<int, Tuple<int, int>[]>>()
        {
            {
                UniformTypes.Farmer,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(43, 0), new Tuple<int, int>(86, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(43, 0), new Tuple<int, int>(25, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(15, 0), new Tuple<int, int>(14, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(27, 0), new Tuple<int, int>(26, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(70, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_0,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(318, 3), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(141, 6), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(153, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(10, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(11, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(38, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_1,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(50, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(130, 1), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(129, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(60, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(17, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(128, 0), new Tuple<int, int>(0, 0) } },
                    { 1, new Tuple<int, int>[] { new Tuple<int, int>(130, 0), new Tuple<int, int>(0, 0) } },

                    { 1000 + 0, new Tuple<int, int>[] { new Tuple<int, int>(150, 0), new Tuple<int, int>(-1, 0) } },
                    { 1000 + 1, new Tuple<int, int>[] { new Tuple<int, int>(-1, 0), new Tuple<int, int>(-1, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoPolice_2,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(25, 3), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(24, 6), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(6, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(20, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(11, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(24, 2), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_0,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(Customization.Service.TOPS_11_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(28, 8), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(11, 7), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(7, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(1, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(Customization.Service.ACCS_7_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_1,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(319, 0), new Tuple<int, int>(0, 0) } },
                    { 4, new Tuple<int, int>[] { new Tuple<int, int>(35, 0), new Tuple<int, int>(0, 0) } },
                    { 8, new Tuple<int, int>[] { new Tuple<int, int>(154, 0), new Tuple<int, int>(0, 0) } },
                    { 6, new Tuple<int, int>[] { new Tuple<int, int>(15, 0), new Tuple<int, int>(0, 0) } },
                    { 3, new Tuple<int, int>[] { new Tuple<int, int>(92, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(126, 0), new Tuple<int, int>(0, 0) } },
                }
            },

            {
                UniformTypes.FractionPaletoEMS_2,

                new Dictionary<int, Tuple<int, int>[]>()
                {
                    { 11, new Tuple<int, int>[] { new Tuple<int, int>(Customization.Service.TOPS_11_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                    { 7, new Tuple<int, int>[] { new Tuple<int, int>(Customization.Service.ACCS_7_MALE_COMP_IDX_BASE_OFFSET + 0, 0), new Tuple<int, int>(0, 0) } },
                }
            },
        };

        public static void ApplyUniform(PlayerData pData, UniformTypes uType)
        {
            var data = _allUniforms.GetValueOrDefault(uType);

            if (data == null)
                return;

            var idx = pData.Sex ? 0 : 1;

            foreach (var x in data)
            {
                if (x.Key >= 1000)
                {
                    if (x.Value[idx].Item1 < 0)
                    {
                        pData.Player.ClearAccessory(x.Key - 1000);

                        if (x.Key == 1000)
                            pData.Hat = null;
                    }
                    else
                    {
                        pData.Player.SetAccessories(x.Key - 1000, x.Value[idx].Item1, x.Value[idx].Item2);

                        if (x.Key == 1000)
                            pData.Hat = $"{x.Value[idx].Item1}|{x.Value[idx].Item2}";
                    }
                }
                else
                {
                    pData.Player.SetClothes(x.Key, x.Value[idx].Item1, x.Value[idx].Item2);
                }
            }

            pData.CurrentUniform = uType;
        }

        public static bool IsUniformElementActive(PlayerData pData, int elementIdx, bool notifyIf)
        {
            var currentUniformType = pData.CurrentUniform;

            if (currentUniformType != UniformTypes.None)
            {
                var data = _allUniforms.GetValueOrDefault(currentUniformType);

                if (data == null)
                    return false;

                if (data.ContainsKey(elementIdx))
                {
                    if (notifyIf)
                    {
                        pData.Player.Notify("Inv::CCWUA");
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool IsInUniform(PlayerData pData, bool notifyIf)
        {
            if (pData.CurrentUniform == UniformTypes.None)
                return false;

            if (notifyIf)
            {
                pData.Player.Notify("Inv::CDTWUA");
            }

            return true;
        }

        public static void SetNoUniform(PlayerData pData)
        {
            pData.CurrentUniform = UniformTypes.None;

            pData.UpdateClothes();
        }
    }
}