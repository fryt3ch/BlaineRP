namespace BlaineRP.Server.Game.Management.Chat
{
    internal enum MessageType
    {
        /// <summary>/say</summary>
        Say,
        /// <summary>/s</summary>
        Shout,
        /// <summary>/w</summary>
        Whisper,
        /// <summary>/n - OOC чат</summary>
        NonRP,
        /// <summary>/me</summary>
        Me,
        /// <summary>/do</summary>
        Do,
        /// <summary>/todo</summary>
        Todo,
        /// <summary>/try</summary>
        Try,
        /// <summary>/f /r</summary>
        Fraction,
        /// <summary>/o</summary>
        Organisation,
        /// <summary>/d</summary>
        Department,
        /// <summary>/gov</summary>
        Goverment,
        /// <summary>/amsg</summary>
        Admin,

        Ban,
        BanHard,
        Kick,
        Mute,
        Jail,
        Warn,
        UnBan,
        UnMute,
        UnJail,
        UnWarn,
        News,
        Advert,
    }
}