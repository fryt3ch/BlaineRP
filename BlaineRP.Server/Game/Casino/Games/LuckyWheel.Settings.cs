using System;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class LuckyWheel
    {
        public static TimeSpan SpinWheelAnimationTime { get; } = TimeSpan.FromMilliseconds(4_500);
        public static TimeSpan SpinWheelTime { get; } = TimeSpan.FromMilliseconds(22_000);

        private static ChancePicker<Items.Gift.Prototype> ChancePicker { get; set; } = new ChancePicker<Items.Gift.Prototype>(
            new ChancePicker<Items.Gift.Prototype>.Item<Items.Gift.Prototype>(0.90d, Items.Gift.Prototype.CreateCasino(Items.Gift.Types.CasinoChips, null, 0, 100)),
            new ChancePicker<Items.Gift.Prototype>.Item<Items.Gift.Prototype>(0.10d, Items.Gift.Prototype.CreateCasino(Items.Gift.Types.Money, null, 0, 50_000))
        );

        public static TimeSpan SpinDefaultCooldown { get; } = TimeSpan.FromHours(24);
    }
}