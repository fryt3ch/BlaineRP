namespace BlaineRP.Server.Game.Management.Phone
{
    public partial class Call
    {
        public enum CancelTypes : byte
        {
            /// <summary>Вызов отменен сервером</summary>
            ServerAuto = 0,
            /// <summary>Вызов отменен первым игроком</summary>
            Caller,
            /// <summary>Вызов отменен вторым игроком</summary>
            Receiver,
            /// <summary>Вызов отменен по причине недостатка средств</summary>
            NotEnoughBalance,
        }
    }
}