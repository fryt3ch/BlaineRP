namespace BlaineRP.Server.Game.Management.Phone
{
    public partial class Call
    {
        public enum StatusTypes : byte
        {
            /// <summary>Вызов начат, но еще не принят вторым игроком</summary>
            Outgoing = 0,
            /// <summary>Второй игрок принял вызов, идет процесс разговора</summary>
            Process,
        }
    }
}