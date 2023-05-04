using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public static void InitializeBlips()
        {
            new Additional.ExtraBlip(60, new Vector3(-444f, 6016f, 0f), Fractions.Fraction.Get(Fractions.Types.COP_BLAINE)?.Name ?? Fractions.Types.COP_BLAINE.ToString(), 1f, 63, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(60, new Vector3(441.9979f, -983.1353f, 0f), Fractions.Fraction.Get(Fractions.Types.COP_LS)?.Name ?? Fractions.Types.COP_LS.ToString(), 1f, 63, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(498, new Vector3(113.6995f, -753.6832f, 0f), Fractions.Fraction.Get(Fractions.Types.FIB_LS)?.Name ?? Fractions.Types.FIB_LS.ToString(), 1f, 29, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(768, new Vector3(1800f, 2607.3f, 0f), "Федеральная тюрьма", 1f, 29, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(758, new Vector3(-2277.5f, 3365f, 0f), "Военная база", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(60, new Vector3(1855f, 3684f, 0f), "Филиал полиции Округа Блэйн в Сэнди-Шорс", 1f, 63, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(744, new Vector3(-598.7835f, -929.9023f, 0f), Fractions.Fraction.Get(Fractions.Types.MEDIA_LS)?.Name ?? Fractions.Types.MEDIA_LS.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(61, new Vector3(-253.3461f, 6333.604f, 0f), Fractions.Fraction.Get(Fractions.Types.EMS_BLAINE)?.Name ?? Fractions.Types.EMS_BLAINE.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(61, new Vector3(304.3673f, -586.6595f, 0f), Fractions.Fraction.Get(Fractions.Types.EMS_LS)?.Name ?? Fractions.Types.EMS_LS.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(61, new Vector3(1826f, 3693f, 0f), "Филиал больницы Округа Блэйн в Сэнди-Шорс", 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(419, new Vector3(-548.8925f, -197.5878f, 0f), Fractions.Fraction.Get(Fractions.Types.GOV_LS)?.Name ?? Fractions.Types.GOV_LS.ToString(), 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(419, new Vector3(1701.585f, 3782.184f, 0f), "Администрация Округа Блэйн", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(305, new Vector3(-317.9069f, 6154.085f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(305, new Vector3(-318.6048f, 2807.211f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(305, new Vector3(-1680.56f, -280.8945f, 0f), "Церковь", 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(362, new Vector3(80f, -1963f, 0f), Fractions.Fraction.Get(Fractions.Types.GANG_BALS)?.Name ?? Fractions.Types.GANG_BALS.ToString(), 1f, 27, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(362, new Vector3(-141f, -1601f, 0f), Fractions.Fraction.Get(Fractions.Types.GANG_FAMS)?.Name ?? Fractions.Types.GANG_FAMS.ToString(), 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(362, new Vector3(-27f, -1403f, 0f), Fractions.Fraction.Get(Fractions.Types.GANG_MARA)?.Name ?? Fractions.Types.GANG_MARA.ToString(), 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(362, new Vector3(311.8519f, -2042.281f, 0f), Fractions.Fraction.Get(Fractions.Types.GANG_VAGS)?.Name ?? Fractions.Types.GANG_VAGS.ToString(), 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(685, new Vector3(413.8777f, -1491.162f, 0f), Fractions.Fraction.Get(Fractions.Types.MAFIA_ITALY)?.Name ?? Fractions.Types.MAFIA_ITALY.ToString(), 1f, 5, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(685, new Vector3(-1536f, 112.5f, 0f), Fractions.Fraction.Get(Fractions.Types.MAFIA_JAPAN)?.Name ?? Fractions.Types.MAFIA_JAPAN.ToString(), 1f, 1, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(685, new Vector3(-117f, 996f, 0f), Fractions.Fraction.Get(Fractions.Types.MAFIA_RUSSIA)?.Name ?? Fractions.Types.MAFIA_RUSSIA.ToString(), 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(680, new Vector3(927.9176f, 44.61714f, 0f), "The Diamond Casino & Resort", 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);

            new Additional.ExtraBlip(770, new Vector3(1292.144f, -3337.423f, 0f), "Мост на о. Кайо-Перико", 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            new Additional.ExtraBlip(770, new Vector3(3915.301f, -4673.811f, 0f), "Мост на материк", 1f, 2, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
        }
    }
}
