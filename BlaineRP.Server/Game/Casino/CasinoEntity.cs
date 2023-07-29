using System.Collections.Generic;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.Casino.Games;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Casino
{
    public partial class CasinoEntity
    {
        public const WallScreenTypes DEFAULT_WALLSCREEN_TYPE = WallScreenTypes.CASINO_DIA_PL;

        public static List<CasinoEntity> All { get; private set; }

        public static CasinoEntity GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

        public int Id => All.IndexOf(this);

        public Roulette[] Roulettes { get; set; }

        public LuckyWheel[] LuckyWheels { get; set; }

        public SlotMachine[] SlotMachines { get; set; }

        public Blackjack[] Blackjacks { get; set; }

        public ushort BuyChipPrice { get; set; }
        public ushort SellChipPrice { get; set; }

        public WallScreenTypes CurrentWallScreenType { get => (WallScreenTypes)Utils.ToByte(World.Service.GetSharedData<object>($"CASINO_{Id}_WST")); set => World.Service.SetSharedData($"CASINO_{Id}_WST", (byte)value); }

        public CasinoEntity()
        {
            All.Add(this);
        }

        public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Length ? null : Roulettes[id];

        public LuckyWheel GetLuckyWheelById(int id) => id < 0 || id >= LuckyWheels.Length ? null : LuckyWheels[id];

        public SlotMachine GetSlotMachineById(int id) => id < 0 || id >= SlotMachines.Length ? null : SlotMachines[id];

        public Blackjack GetBlackjackById(int id) => id < 0 || id >= Blackjacks.Length ? null : Blackjacks[id];

        public static bool TryAddCasinoChips(PlayerInfo pInfo, uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!pInfo.CasinoChips.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public static bool TryRemoveCasinoChips(PlayerInfo pInfo, uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!pInfo.CasinoChips.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (pInfo.PlayerData != null)
                    {
                        pInfo.PlayerData.Player.Notify("Casino::NEC", pInfo.CasinoChips);
                    }
                }

                return false;
            }

            return true;
        }

        public static void SetCasinoChips(PlayerInfo pInfo, uint value, string reason)
        {
            pInfo.CasinoChips = value;

            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Player.TriggerEvent("Casino::CB", value);
            }

            MySQL.CharacterCasinoChipsUpdate(pInfo);
        }
    }
}
