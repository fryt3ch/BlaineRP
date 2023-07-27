namespace BlaineRP.Server.Game.Management.AntiCheat
{
    public enum VehicleTeleportType
    {
        /// <summary>Без игроков</summary>
        Default = 0,
        /// <summary>Вместе с водителем</summary>
        OnlyDriver,
        /// <summary>Вместе со всеми пассажирами</summary>
        All,
    }
}