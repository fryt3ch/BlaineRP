namespace BlaineRP.Server.EntitiesData.Vehicles
{
    public enum OwnerTypes
    {
        /// <summary>Доступна всем</summary>
        AlwaysFree = -1,
        /// <summary>Принадлежит игроку</summary>
        Player,
        /// <summary>Временная, назначена игроку</summary>
        PlayerTemp,
        /// <summary>Арендуется игроком</summary>
        PlayerRent,
        /// <summary>Арендуется игроком, при этом - принадлежит работе</summary>
        PlayerRentJob,
        /// <summary>Используется игроком для прохождения практического экзамена в автошколе</summary>
        PlayerDrivingSchool,
        /// <summary>Принадлежит фракции</summary>
        Fraction,
    }
}