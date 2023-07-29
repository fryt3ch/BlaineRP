using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Attachments
{
    public class AttachmentEntityNet
    {
        [JsonProperty(PropertyName = "E")]
        public EntityType EntityType { get; set; }

        [JsonProperty(PropertyName = "I")]
        public ushort Id { get; set; }

        [JsonProperty(PropertyName = "T")]
        public AttachmentType Type { get; set; }

        [JsonProperty(PropertyName = "D", NullValueHandling = NullValueHandling.Ignore)]
        public string SyncData { get; set; }

        public AttachmentEntityNet(ushort Id, EntityType EntityType, AttachmentType Type, string SyncData)
        {
            this.Id = Id;
            this.EntityType = EntityType;

            this.Type = Type;

            this.SyncData = SyncData;
        }
    }
}