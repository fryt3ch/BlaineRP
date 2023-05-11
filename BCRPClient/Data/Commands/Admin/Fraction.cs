using RAGE.Elements;
using System;
using System.Linq;

namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("fraction_observe", true, "Удалить все выброшенные предметы")]
        public static void FractionObserve(string fractionType)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_obs", fractionTypeNum);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_setmaterials", true, "Удалить все выброшенные предметы")]
        public static void FractionSetMaterials(string fractionType, uint mats)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_setmats", fractionTypeNum, mats);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_addmaterials", true, "Удалить все выброшенные предметы")]
        public static void FractionAddMaterials(string fractionType, uint mats)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_addmats", fractionTypeNum, mats);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_removematerials", true, "Удалить все выброшенные предметы")]
        public static void FractionSRemoveMaterials(string fractionType, uint mats)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_rmmats", fractionTypeNum, mats);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_setbalance", true, "Удалить все выброшенные предметы")]
        public static void FractionSetBalance(string fractionType, ulong balance)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_setbal", fractionTypeNum, balance);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_addbalance", true, "Удалить все выброшенные предметы")]
        public static void FractionAddBalance(string fractionType, ulong balance)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_addbal", fractionTypeNum, balance);

            LastSent = Sync.World.ServerTime;
        }

        [Command("fraction_removebalance", true, "Удалить все выброшенные предметы")]
        public static void FractionRemoveBalance(string fractionType, ulong balance)
        {
            if (LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(Data.Fractions.Types)).Cast<Data.Fractions.Types>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((Data.Fractions.Types)fractionTypeNum == Fractions.Types.None)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, "Такой фракции не существует!");

                return;
            }

            CallRemote("frac_rmbal", fractionTypeNum, balance);

            LastSent = Sync.World.ServerTime;
        }
    }
}