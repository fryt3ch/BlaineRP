namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentEntity
    {
        public ushort RemoteID { get; set; }

        public RAGE.Elements.Type EntityType { get; set; }

        public AttachmentTypes Type { get; set; }

        public bool WasAttached { get; set; }

        public string SyncData { get; set; }

        public AttachmentEntity(AttachmentEntityNet attachmentNet)
        {
            RemoteID = attachmentNet.Id;
            EntityType = attachmentNet.EntityType;
            Type = attachmentNet.Type;

            SyncData = attachmentNet.SyncData;
        }
    }
}