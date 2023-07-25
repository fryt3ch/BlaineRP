using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.Wrappers.Blips;
using RAGE;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public static void InitializeBlips()
        {
            new ExtraBlip(792, new Vector3(-749.78f, 5818.21f, 18.9f), "Рэймонд", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(479, new Vector3(-3018.627f, 1665f, 20f), "Таможня", 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(479, new Vector3(-1503.909f, 1665f, 20f), "Таможня", 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(479, new Vector3(1975.948f, 1655f, 20f), "Таможня", 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(479, new Vector3(1729.99f, 1655f, 20f), "Таможня", 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(163, new Vector3(-2605.542f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(-780.7f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(138.39f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(1054.1f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(1266.8f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(1501.8f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(163, new Vector3(2539.5f, 1655f, 20f), "Блок-пост", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(468, new Vector3(-525.37f, 5320f, 20f), "Лесопилка", 1.5f, 25, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(527, new Vector3(2826.65f, 2833.74f, 20f), "Карьер", 1f, 25, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(755, new Vector3(1325.16f, 4336.48f, 20f), "Порт", 1f, 38, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(566, new Vector3(1030.06f, 2362.33f, 20f), "Стройка", 0.75f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(499, new Vector3(3536.9f, 3723.66f, 20f), "Хим. завод", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(361, new Vector3(2906.88f, 4344.87f, 20f), "Нефтезавод", 1f, 51, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(728, new Vector3(340.87f, 3569.55f, 20f), "Рынок автомобилей", 1f, 19, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(728, new Vector3(2049.33f, 3436.35f, 20f), "Рынок автомобилей", 1f, 19, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(728, new Vector3(-1148.351f, -1698.468f, 20f), Locale.Get("BLIP_MARKETSTALLS"), 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(436, new Vector3(-371.8f, 6121.9f, 20f), "Пожарная станция", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(436, new Vector3(1697f, 3586f, 20f), "Пожарная станция", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(68, new Vector3(-272.71f, 6634.79f, 20f), "Пирс", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(68, new Vector3(-1609.82f, 5255.81f, 20f), "Пирс", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(68, new Vector3(717f, 4101.48f, 20f), "Пирс", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(68, new Vector3(1490.2f, 3919.77f, 20f), "Пирс", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);
            new ExtraBlip(68, new Vector3(3812.18f, 4464f, 20f), "Пирс", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension, ExtraBlip.Types.Default);

            new ExtraBlip(60, new Vector3(-444f, 6016f, 0f), Fraction.Get(FractionTypes.COP_BLAINE)?.Name ?? FractionTypes.COP_BLAINE.ToString(), 1f, 63, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(60, new Vector3(441.9979f, -983.1353f, 0f), Fraction.Get(FractionTypes.COP_LS)?.Name ?? FractionTypes.COP_LS.ToString(), 1f, 63, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(498, new Vector3(113.6995f, -753.6832f, 0f), Fraction.Get(FractionTypes.FIB_LS)?.Name ?? FractionTypes.FIB_LS.ToString(), 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(768, new Vector3(1800f, 2607.3f, 0f), "Федеральная тюрьма", 1f, 29, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(758, new Vector3(-2277.5f, 3365f, 0f), "Военная база", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(60, new Vector3(1855f, 3684f, 0f), "Филиал полиции Округа Блэйн в Сэнди-Шорс", 1f, 63, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(744, new Vector3(-598.7835f, -929.9023f, 0f), Fraction.Get(FractionTypes.MEDIA_LS)?.Name ?? FractionTypes.MEDIA_LS.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(61, new Vector3(-253.3461f, 6333.604f, 0f), Fraction.Get(FractionTypes.EMS_BLAINE)?.Name ?? FractionTypes.EMS_BLAINE.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(61, new Vector3(304.3673f, -586.6595f, 0f), Fraction.Get(FractionTypes.EMS_LS)?.Name ?? FractionTypes.EMS_LS.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(61, new Vector3(1826f, 3693f, 0f), "Филиал больницы Округа Блэйн в Сэнди-Шорс", 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(419, new Vector3(-548.8925f, -197.5878f, 0f), Fraction.Get(FractionTypes.GOV_LS)?.Name ?? FractionTypes.GOV_LS.ToString(), 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(419, new Vector3(1701.585f, 3782.184f, 0f), "Администрация Округа Блэйн", 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(305, new Vector3(-317.9069f, 6154.085f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(305, new Vector3(-318.6048f, 2807.211f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(305, new Vector3(-1680.56f, -280.8945f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(362, new Vector3(80f, -1963f, 0f), Fraction.Get(FractionTypes.GANG_BALS)?.Name ?? FractionTypes.GANG_BALS.ToString(), 1f, 27, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(362, new Vector3(-141f, -1601f, 0f), Fraction.Get(FractionTypes.GANG_FAMS)?.Name ?? FractionTypes.GANG_FAMS.ToString(), 1f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(362, new Vector3(-27f, -1403f, 0f), Fraction.Get(FractionTypes.GANG_MARA)?.Name ?? FractionTypes.GANG_MARA.ToString(), 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(362, new Vector3(311.8519f, -2042.281f, 0f), Fraction.Get(FractionTypes.GANG_VAGS)?.Name ?? FractionTypes.GANG_VAGS.ToString(), 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(685, new Vector3(413.8777f, -1491.162f, 0f), Fraction.Get(FractionTypes.MAFIA_ITALY)?.Name ?? FractionTypes.MAFIA_ITALY.ToString(), 1f, 5, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(685, new Vector3(-1536f, 112.5f, 0f), Fraction.Get(FractionTypes.MAFIA_JAPAN)?.Name ?? FractionTypes.MAFIA_JAPAN.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(685, new Vector3(-117f, 996f, 0f), Fraction.Get(FractionTypes.MAFIA_RUSSIA)?.Name ?? FractionTypes.MAFIA_RUSSIA.ToString(), 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(680, new Vector3(927.9176f, 44.61714f, 0f), "The Diamond Casino & Resort", 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(770, new Vector3(1292.144f, -3337.423f, 0f), "Мост на о. Кайо-Перико", 1f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            new ExtraBlip(770, new Vector3(3915.301f, -4673.811f, 0f), "Мост на материк", 1f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            new ExtraBlip(93, new Vector3(-1395.082f, -610.6008f, 0f), "Ночной клуб", 1f, 8, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
        }
    }
}
