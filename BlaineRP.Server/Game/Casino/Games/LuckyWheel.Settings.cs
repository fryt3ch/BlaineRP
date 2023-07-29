using System;
using BlaineRP.Server.Game.Gifts;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class LuckyWheel
    {
        public static TimeSpan SpinWheelAnimationTime { get; } = TimeSpan.FromMilliseconds(4_500);
        public static TimeSpan SpinWheelTime { get; } = TimeSpan.FromMilliseconds(22_000);

        private static ChancePicker<Gift.Prototype> ChancePicker { get; set; } = new ChancePicker<Gift.Prototype>(
            new ChancePicker<Gift.Prototype>.Item<Gift.Prototype>(0.90d, Gift.Prototype.CreateCasino(GiftTypes.CasinoChips, null, 0, 100)),
            new ChancePicker<Gift.Prototype>.Item<Gift.Prototype>(0.10d, Gift.Prototype.CreateCasino(GiftTypes.Money, null, 0, 50_000))
        );

        public static TimeSpan SpinDefaultCooldown { get; } = TimeSpan.FromHours(24);
    }
}