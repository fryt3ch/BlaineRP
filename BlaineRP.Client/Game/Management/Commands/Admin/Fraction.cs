using System;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.World;

namespace BlaineRP.Client.Game.Management.Commands
{
    partial class Core
    {
        [Command("fraction_observe", true, "Удалить все выброшенные предметы")]
        public static void FractionObserve(string fractionType)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_obs", fractionTypeNum);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_setmaterials", true, "Удалить все выброшенные предметы")]
        public static void FractionSetMaterials(string fractionType, uint mats)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_setmats", fractionTypeNum, mats);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_addmaterials", true, "Удалить все выброшенные предметы")]
        public static void FractionAddMaterials(string fractionType, uint mats)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_addmats", fractionTypeNum, mats);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_removematerials", true, "Удалить все выброшенные предметы")]
        public static void FractionSRemoveMaterials(string fractionType, uint mats)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_rmmats", fractionTypeNum, mats);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_setbalance", true, "Удалить все выброшенные предметы")]
        public static void FractionSetBalance(string fractionType, ulong balance)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_setbal", fractionTypeNum, balance);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_addbalance", true, "Удалить все выброшенные предметы")]
        public static void FractionAddBalance(string fractionType, ulong balance)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_addbal", fractionTypeNum, balance);

            Commands.Core.LastSent = World.Core.ServerTime;
        }

        [Command("fraction_removebalance", true, "Удалить все выброшенные предметы")]
        public static void FractionRemoveBalance(string fractionType, ulong balance)
        {
            if (Commands.Core.LastSent.IsSpam(1000, false, true))
                return;

            var fractionTypes = Enum.GetValues(typeof(FractionTypes)).Cast<FractionTypes>().ToList();

            int fractionTypeNum;

            if (!int.TryParse(fractionType, out fractionTypeNum))
            {
                var fractionTypeStrLow = fractionType.ToLower();

                fractionTypeNum = (int)fractionTypes.Where(x => x.ToString().ToLower() == fractionTypeStrLow).FirstOrDefault();
            }

            if ((FractionTypes)fractionTypeNum == FractionTypes.None)
            {
                Notification.ShowError("Такой фракции не существует!");

                return;
            }

            Commands.Core.CallRemote("frac_rmbal", fractionTypeNum, balance);

            Commands.Core.LastSent = World.Core.ServerTime;
        }
    }
}