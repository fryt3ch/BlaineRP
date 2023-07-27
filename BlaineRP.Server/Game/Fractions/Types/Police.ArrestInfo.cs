using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Punishments;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        public class ArrestInfo
        {
            public string TargetName { get; set; }

            public string MemberName { get; set; }

            public Punishment PunishmentData { get; set; }

            public uint TargetCID { get; set; }

            public ArrestInfo()
            {

            }
        }
    }
}