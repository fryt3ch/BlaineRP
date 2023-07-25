using BlaineRP.Client.Game.Management.Attachments.Enums;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentObject
    {
        public string SyncData { get; set; }

        public GameEntity Object { get; set; }

        public AttachmentTypes Type { get; set; }

        public uint Model { get; set; }

        public AttachmentObject(GameEntity @object, AttachmentObjectNet attachmentNet)
        {
            SyncData = attachmentNet.SyncData;
            Object = @object;
            Type = attachmentNet.Type;

            Model = attachmentNet.Model;
        }
    }
}