namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentEntity
    {
        public AttachmentEntity(AttachmentEntityNet attachmentNet)
        {
            RemoteID = attachmentNet.Id;
            EntityType = attachmentNet.EntityType;
            Type = attachmentNet.Type;

            SyncData = attachmentNet.SyncData;
        }

        public ushort RemoteID { get; set; }

        public RAGE.Elements.Type EntityType { get; set; }

        public AttachmentType Type { get; set; }

        public bool WasAttached { get; set; }

        public string SyncData { get; set; }
    }
}