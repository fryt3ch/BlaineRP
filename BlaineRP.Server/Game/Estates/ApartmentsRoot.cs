using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class ApartmentsRoot
    {
        public static Dictionary<uint, ApartmentsRoot> All { get; set; } = new Dictionary<uint, ApartmentsRoot>();

        public static ShellData GetShellDataByType(ShellTypes sType) => Shells.GetValueOrDefault(sType);

        public uint Id { get; private set; }

        public ShellTypes ShellType { get; private set; }

        public ShellData Shell => GetShellDataByType(ShellType);

        public Vector4 EnterParams { get; private set; }

        /// <summary>Измерение многоквартирного дома</summary>
        public uint Dimension { get; set; }

        public ApartmentsRoot(uint Id, ShellTypes ShellType, Vector4 EnterParams)
        {
            this.Id = Id;

            this.ShellType = ShellType;

            this.EnterParams = EnterParams;

            this.Dimension = Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset + Id;

            All.Add(Id, this);
        }

        public static ApartmentsRoot Get(uint id) => All.GetValueOrDefault(id);

        public void SetPlayersInside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var pos = Shell.EnterPosition;

                Utils.TeleportPlayers(pos.Position, false, Dimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Enter", Id);
            }
        }

        public void SetPlayersOutside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var pos = EnterParams;

                Utils.TeleportPlayers(pos.Position, false, Properties.Settings.Static.MainDimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "ARoot::Exit");
            }
        }
    }
}