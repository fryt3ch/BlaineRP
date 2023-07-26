using Newtonsoft.Json;

namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentObjectNet
    {
        [JsonProperty(PropertyName = "M")]
        public uint Model { get; set; }

        [JsonProperty(PropertyName = "D")]
        public string SyncData { get; set; }

        [JsonProperty(PropertyName = "T")]
        public AttachmentTypes Type { get; set; }

        public AttachmentObjectNet()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is AttachmentObjectNet other)
                return SyncData == other.SyncData && Model == other.Model && Type == other.Type;

            return false;
        }

        public override int GetHashCode()
        {
            return SyncData?.GetHashCode() ?? 0 + Model.GetHashCode() + Type.GetHashCode();
        }
    }
}