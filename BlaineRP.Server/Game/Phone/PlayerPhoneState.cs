namespace BlaineRP.Server.Game.Phone
{
    public enum PlayerPhoneState : byte
    {
        /// <summary>Телефон не используется</summary>
        Off = 0,
        /// <summary>Телефон используется без анимаций</summary>
        JustOn,
        /// <summary>Телефон используется c обычной анимацией</summary>
        Idle,
        /// <summary>Телефон используется с анимацией разговора</summary>
        Call,
        /// <summary>Телефон используется с анимацией камеры 0</summary>
        Camera,
    }
}