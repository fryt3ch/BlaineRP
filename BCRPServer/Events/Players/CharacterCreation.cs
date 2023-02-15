using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    class CharacterCreation : Script
    {
        public static Vector3 Position = Utils.DefaultSpawnPosition;

        #region Supported Clothes (on creation)
        private static Dictionary<bool, string[][]> DefaultClothes = new Dictionary<bool, string[][]>()
        {
            { true, new string[][] { new string[] { "hat_m_0", "hat_m_8", "hat_m_39" }, new string[] { "top_m_0", "under_m_2", "under_m_5" }, new string[] { "pants_m_0", "pants_m_2", "pants_m_11" }, new string[] { "shoes_m_5", "shoes_m_1", "shoes_m_3" } } },

            { false, new string[][] { new string[] { "hat_f_1", "hat_f_13", "hat_f_6" }, new string[] { "top_f_5", "under_f_2", "under_f_1" }, new string[] { "pants_f_0", "pants_f_4", "pants_f_7" }, new string[] { "shoes_f_0", "shoes_f_2", "shoes_f_5" } } },
        };
        #endregion

        public static void StartNew(Player player)
        {
            player.SetSkin(PedHash.FreemodeMale01);
            player.SetCustomization(true, Game.Data.Customization.Defaults.HeadBlend, Game.Data.Customization.Defaults.EyeColor, Game.Data.Customization.Defaults.HairStyle.Color, Game.Data.Customization.Defaults.HairStyle.Color2, Game.Data.Customization.Defaults.FaceFeatures, Game.Data.Customization.Defaults.HeadOverlays, Game.Data.Customization.Defaults.Decorations);
            player.SetHair(0);

            Undress(player, true);

            player.Teleport(Position, true);
            player.SetAlpha(255);

            player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.ToPlayer, true, "CharacterCreation::StartNew");
        }

        #region Set Sex
        [RemoteEvent("CharacterCreation::SetSex")]
        public static void SetSex(Player player, bool sex)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.CharacterCreation)
                return;

            player.SetSkin(sex ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
            player.SetCustomization(sex, Game.Data.Customization.Defaults.HeadBlend, Game.Data.Customization.Defaults.EyeColor, Game.Data.Customization.Defaults.HairStyle.Color, Game.Data.Customization.Defaults.HairStyle.Color2, Game.Data.Customization.Defaults.FaceFeatures, Game.Data.Customization.Defaults.HeadOverlays, Game.Data.Customization.Defaults.Decorations);

            if (!sex)
                player.UpdateHeadBlend(0f, 0.5f, 0f);

            Undress(player, sex);
        }
        #endregion

        #region Exit
        [RemoteEvent("CharacterCreation::Exit")]
        public static void Exit(Player player)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.CharacterCreation)
                return;

            tData.StepType = TempData.StepTypes.CharacterSelection;

            player.SetAlpha(0);

            player.TriggerEvent("CharacterCreation::Close");
            player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.Move, true, "Auth::ShowCharacterChoosePage", false);
        }
        #endregion

        #region Create
        [RemoteEvent("CharacterCreation::Create")]
        public static void Create(Player player, string name, string surname, int age, bool sex, byte eyeColor, string hBlendData, string hOverlaysData, string hStyleData, string fFeaturesData, string clothesData)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if ((tData.StepType != TempData.StepTypes.CharacterCreation) || tData.AccountData == null)
                return;

            try
            {
                var hBlend = NAPI.Util.FromJson<Game.Data.Customization.HeadBlend>(hBlendData);
                var fFeatures = NAPI.Util.FromJson<float[]>(fFeaturesData);
                var hOverlays = NAPI.Util.FromJson<Dictionary<int, Game.Data.Customization.HeadOverlay>>(hOverlaysData);
                var hStyle = NAPI.Util.FromJson<Game.Data.Customization.HairStyle>(hStyleData);
                var clothes = NAPI.Util.FromJson<string[]>(clothesData);

                if (clothes.Length != 4 || !IsCustomizationValid(hBlend, hOverlays, fFeatures, hStyle, eyeColor) || name == null || surname == null || !Utils.IsNameValid(name) || !Utils.IsNameValid(surname) || age < 18 || age >= 100)
                    return;

                for (int i = 0; i < clothes.Length; i++)
                    if (clothes[i] != null && !DefaultClothes[sex][i].Contains(clothes[i]))
                        clothes[i] = null;

                var newClothes = new Game.Items.Clothes[5];

                if (clothes[0] != null)
                    newClothes[0] = (Game.Items.Clothes)Game.Items.Stuff.CreateItem(clothes[0], 0, 1);

                if (clothes[1] != null)
                {
                    newClothes[Game.Items.Stuff.GetType(clothes[1]) == typeof(Game.Items.Top) ? 1 : 2] = (Game.Items.Clothes)Game.Items.Stuff.CreateItem(clothes[1], 0, 1);
                }

                if (clothes[2] != null)
                    newClothes[3] = (Game.Items.Clothes)Game.Items.Stuff.CreateItem(clothes[2], 0, 1);

                if (clothes[3] != null)
                    newClothes[4] = (Game.Items.Clothes)Game.Items.Stuff.CreateItem(clothes[3], 0, 1);

                var pData = new PlayerData(player, tData.AccountData.ID, name, surname, age, sex, hBlend, hOverlays, fFeatures, eyeColor, hStyle, newClothes);

                player.TriggerEvent("CharacterCreation::Close");

                tData.Delete();

                player.SetMainData(pData);

                pData.SetReady();
            }
            catch (Exception ex)
            {
                Utils.KickSilent(player, ex.Message);

                return;
            }
        }
        #endregion

        public static void Undress(Player player, bool sex)
        {
            player.SetClothes(11, 15, 0);
            player.SetClothes(8, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(4, sex ? 21 : 15, 0);
            player.SetClothes(6, sex ? 34 : 35, 0);
            player.SetClothes(9, 0, 0);
        }

        #region Validation
        public static bool IsCustomizationValid(Game.Data.Customization.HeadBlend hBlend, Dictionary<int, Game.Data.Customization.HeadOverlay> hOverlays, float[] fFeatures, Game.Data.Customization.HairStyle hStyle, byte eyeColor)
        {
            if (hBlend.ShapeFirst < 21 || hBlend.ShapeFirst > 45 || hBlend.ShapeFirst == 42 || hBlend.ShapeFirst == 43 || hBlend.ShapeFirst == 44)
                return false;

            if (hBlend.SkinFirst < 21 || hBlend.SkinFirst > 45 || hBlend.SkinFirst == 42 || hBlend.SkinFirst == 43 || hBlend.SkinFirst == 44)
                return false;

            if (hBlend.ShapeSecond > 20 && (hBlend.ShapeSecond != 42 && hBlend.ShapeSecond != 43 && hBlend.ShapeSecond != 44))
                return false;

            if (hBlend.SkinSecond > 20 && (hBlend.SkinSecond != 42 && hBlend.SkinSecond != 43 && hBlend.SkinSecond != 44))
                return false;

            if (hBlend.SkinMix < 0f || hBlend.SkinMix > 1f || hBlend.ShapeMix < 0f || hBlend.ShapeMix > 1f)
                return false;

            if (hBlend.ShapeThird != 0 || hBlend.SkinThird != 0 || hBlend.ThirdMix != 0)
                return false;

            if (eyeColor > 31)
                return false;

            if (hStyle.Color > 63 || hStyle.Color2 > 63)
                return false;

            if (fFeatures.Length != 20)
                return false;

            for (int i = 0; i < 20; i++)
                if (fFeatures[i] < -1f || fFeatures[i] > 1f)
                    return false;

            if (hOverlays.Count != 13)
                return false;

            for (int i = 0; i < 13; i++)
            {
                if (!hOverlays.ContainsKey(i))
                    return false;

                var overlay = hOverlays[i];

                if (overlay.Opacity < 0f || overlay.Opacity > 1f)
                    return false;

                if (overlay.Color > 63 || overlay.SecondaryColor > 63)
                    return false;
            }

            return true;
        }
        #endregion
    }
}
