using Newtonsoft.Json;

namespace BlaineRP.Client.Game.Attachments
{
    public class AttachmentObjectNet
    {
        public AttachmentObjectNet()
        {
        }

        [JsonProperty(PropertyName = "M")]
        public uint Model { get; set; }

        [JsonProperty(PropertyName = "D")]
        public string SyncData { get; set; }

        [JsonProperty(PropertyName = "T")]
        public AttachmentType Type { get; set; }

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