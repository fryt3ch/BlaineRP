using BlaineRP.Client.Game.Management.Attachments.Enums;
using Newtonsoft.Json;

namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentEntityNet
    {
        [JsonProperty(PropertyName = "E")]
        public RAGE.Elements.Type EntityType { get; set; }

        [JsonProperty(PropertyName = "I")]
        public ushort Id { get; set; }

        [JsonProperty(PropertyName = "T")]
        public AttachmentTypes Type { get; set; }

        [JsonProperty(PropertyName = "D")]
        public string SyncData { get; set; }

        public AttachmentEntityNet()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is AttachmentEntityNet other)
                return Id == other.Id && EntityType == other.EntityType && Type == other.Type;

            return false;
        }

        public override int GetHashCode()
        {
            return EntityType.GetHashCode() + Id.GetHashCode() + Type.GetHashCode();
        }
    }
}