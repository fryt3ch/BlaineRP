using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class Ring : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        public new class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Model}, {(SexAlternativeId == null ? "null" : $"\"{SexAlternativeId}\"")}";

            public ItemData(string name, bool sex, string model, string sexAlternativeId = null) : base(name, 0.01f, model, sex, 1, new int[] { 0 }, sexAlternativeId)
            {

            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public new ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

        public bool Toggled { get; set; }

        public void Action(PlayerData pData)
        {
            Unwear(pData);

            Toggled = !Toggled;

            Wear(pData);
        }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            player.AttachObject(Model, Toggled ? AttachmentType.PedRingLeft3 : AttachmentType.PedRingRight3, -1, null);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.DetachObject(Toggled ? AttachmentType.PedRingLeft3 : AttachmentType.PedRingRight3);
        }

        public Ring(string id, int variation = 0) : base(id, IdList[id], typeof(Ring), variation)
        {

        }
    }
}
