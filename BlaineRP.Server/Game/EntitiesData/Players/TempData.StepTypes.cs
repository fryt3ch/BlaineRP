namespace BlaineRP.Server.Game.EntitiesData.Players
{
    public partial class TempData
    {
        public enum StepTypes : byte
        {
            /// <summary>Регистрация/вход</summary>
            None = 0,
            /// <summary>Выбор персонажа</summary>
            CharacterSelection,
            /// <summary>Создание персонажа</summary>
            CharacterCreation,
            /// <summary>Выбор места спавна</summary>
            StartPlace,

            AuthRegistration,
            AuthLogin,
        }
    }
}