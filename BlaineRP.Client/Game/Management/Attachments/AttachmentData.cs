using System;
using RAGE;

namespace BlaineRP.Client.Game.Management.Attachments
{
    public class AttachmentData
    {
        public int BoneID;

        public Action<object[]> EntityAction;

        public AttachmentData(int BoneID,
                              Vector3 PositionOffset,
                              Vector3 Rotation,
                              bool UseSoftPinning,
                              bool Collision,
                              bool IsPed,
                              int RotationOrder,
                              bool FixedRot,
                              Action<object[]> EntityAction = null)
        {
            this.BoneID = BoneID;
            this.PositionOffset = PositionOffset;
            this.Rotation = Rotation;

            this.UseSoftPinning = UseSoftPinning;
            this.Collision = Collision;
            this.IsPed = IsPed;
            this.RotationOrder = RotationOrder;
            this.FixedRot = FixedRot;

            this.EntityAction = EntityAction;
        }

        public Vector3 PositionOffset { get; set; }
        public Vector3 Rotation { get; set; }

        public bool UseSoftPinning { get; set; }

        public bool Collision { get; set; }

        public bool IsPed { get; set; }

        public int RotationOrder { get; set; }

        public bool FixedRot { get; set; }

        public byte DisableInteraction { get; set; }
    }
}