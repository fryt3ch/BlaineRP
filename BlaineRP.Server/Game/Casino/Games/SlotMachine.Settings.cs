using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class SlotMachine
    {
        [Properties.Settings.Static.ClientSync("casinoSlotMachineJackpotMinValue")]
        public const uint JackpotMinValue = 2_500;

        public const uint JackpotMaxValue = 10_000;

        [Properties.Settings.Static.ClientSync("casinoSlotMachineMinBet")]
        public const uint MinBet = 5;

        [Properties.Settings.Static.ClientSync("casinoSlotMachineMaxBet")]
        public const uint MaxBet = 1000;

        public const ReelIconTypes JackpotReplaceType = ReelIconTypes.Grape;

        private static Dictionary<ReelIconTypes, decimal> BetCoefs = new Dictionary<ReelIconTypes, decimal>()
        {
            { ReelIconTypes.Grape, 3m },
            { ReelIconTypes.Cherry, 4m },
            { ReelIconTypes.Watermelon, 6m },
            { ReelIconTypes.Microphone, 10m },
            { ReelIconTypes.Superstar, 16m },
            { ReelIconTypes.Bell, 26m },
        };

        private static ChancePicker<ReelIconTypes> ChancePicker = new ChancePicker<ReelIconTypes>(new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.001d, ReelIconTypes.Bell),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.004d, ReelIconTypes.Superstar),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.005d, ReelIconTypes.Microphone),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.01d, ReelIconTypes.Watermelon),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.02d, ReelIconTypes.Cherry),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.11d, ReelIconTypes.Grape),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.85d, ReelIconTypes.Loose)
        );
    }
}