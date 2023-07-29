using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут надеваться на игрока</summary>
    public interface IWearable
    {
        /// <summary>Метод для того, чтобы надеть предмет на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Wear(PlayerData pData);

        /// <summary>Метод для того, чтобы снять предмет с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Unwear(PlayerData pData);
    }
}