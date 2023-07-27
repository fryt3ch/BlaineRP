using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Management.Attachments
{
    public class AttachmentObjectNet
    {
        [JsonProperty(PropertyName = "M")]
        public uint Model { get; set; }

        [JsonProperty(PropertyName = "T")]
        public AttachmentType Type { get; set; }

        [JsonProperty(PropertyName = "D", NullValueHandling = NullValueHandling.Ignore)]
        public string SyncData { get; set; }

        public AttachmentObjectNet(uint Model, AttachmentType Type, string SyncData = null)
        {
            this.Model = Model;
            this.Type = Type;

            this.SyncData = SyncData;
        }
    }
}