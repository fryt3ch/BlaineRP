using BlaineRP.Client.Extensions.RAGE.Elements;
using RAGE.Elements;

namespace BlaineRP.Client.Game.EntitiesData
{
    public class PedData
    {
        public Ped Ped { get; set; }

        public bool IsInvincible => Ped.GetSharedData<bool>("GM", false);

        public bool IsInvisible => Ped.GetSharedData<bool>("INV", false);

        public PedData(Ped Ped)
        {
            this.Ped = Ped;
        }

        public void Reset()
        {
            if (Ped == null)
                return;

            Ped.ClearTasksImmediately();

            Ped.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

            Ped.ResetData();
        }
    }
}