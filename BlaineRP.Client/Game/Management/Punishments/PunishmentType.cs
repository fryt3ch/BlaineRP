namespace BlaineRP.Client.Game.Management.Punishments
{
    public enum PunishmentType
    {
        /// <summary>Блокировка</summary>
        Ban = 0,

        /// <summary>Предупреждение</summary>
        Warn = 1,

        /// <summary>Мут</summary>
        Mute = 2,

        /// <summary>NonRP тюрьма</summary>
        NRPPrison = 3,

        /// <summary>СИЗО</summary>
        Arrest = 4,

        /// <summary>Федеральная тюрьма</summary>
        FederalPrison = 5,

        /// <summary>Мут чата фракции</summary>
        FractionMute = 6,

        /// <summary>Мут чата организации</summary>
        OrganisationMute = 7,
    }
}