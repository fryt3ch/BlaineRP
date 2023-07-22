using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Events.Commands
{
    partial class Commands
    {
        [Command("frac_obs", 1)]
        private static void FractionObserve(PlayerData pData, params string[] args)
        {
            if (args.Length != 1)
                return;

            int fractionTypeNum;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            if (fData.Type == pData.Fraction)
            {
                return;
            }

            var fTData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fTData != null)
            {
                fTData.SetPlayerNoFraction(pData.Info);
            }

            fData.SetPlayerFraction(pData.Info, (byte)(fData.Ranks.Count - 1));
        }

        [Command("frac_setmats", 1)]
        private static void FractionSetMaterials(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            uint mats;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !uint.TryParse(args[1], out mats))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            fData.SetMaterials(mats, true);
        }

        [Command("frac_addmats", 1)]
        private static void FractionAddMaterials(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            uint mats;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !uint.TryParse(args[1], out mats))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            uint newBalance;

            if (!fData.TryAddMaterials(mats, out newBalance, true, pData))
                return;

            fData.SetMaterials(newBalance, true);
        }

        [Command("frac_rmmats", 1)]
        private static void FractionRemoveMaterials(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            uint mats;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !uint.TryParse(args[1], out mats))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            uint newBalance;

            if (!fData.TryRemoveMaterials(mats, out newBalance, true, pData))
                return;

            fData.SetMaterials(newBalance, true);
        }

        [Command("frac_setbal", 1)]
        private static void FractionSetBalance(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            ulong balance;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !ulong.TryParse(args[1], out balance))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            fData.SetBalance(balance, true);
        }

        [Command("frac_addbal", 1)]
        private static void FractionAddBalance(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            ulong balance;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !ulong.TryParse(args[1], out balance))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            ulong newBalance;

            if (!fData.TryAddMoney(balance, out newBalance, true, pData))
                return;

            fData.SetBalance(newBalance, true);
        }

        [Command("frac_rmbal", 1)]
        private static void FractionRemoveBalance(PlayerData pData, params string[] args)
        {
            if (args.Length != 2)
                return;

            int fractionTypeNum;
            ulong balance;

            if (!int.TryParse(args[0], out fractionTypeNum) || !Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum) || !ulong.TryParse(args[1], out balance))
                return;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum);

            if (fData == null)
                return;

            ulong newBalance;

            if (!fData.TryRemoveMoney(balance, out newBalance, true, pData))
                return;

            fData.SetBalance(newBalance, true);
        }
    }
}