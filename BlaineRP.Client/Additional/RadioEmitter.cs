using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlaineRP.Client.Additional
{
    public class RadioEmitter
    {
        public enum EmitterTypes
        {
            SE_Script_Placed_Prop_Emitter_Boombox,

            DLC_IE_Steal_Pool_Party_Milton_Rd__Radio_Emitter,
            DLC_IE_Steal_Pool_Party_Lake_Vine_Radio_Emitter,

            MP_ARM_WRESTLING_RADIO_01,

            SE_ex_int_office_01a_Radio_01,
            SE_ex_int_office_01b_Radio_01,
            SE_ex_int_office_01c_Radio_01,
            SE_ex_int_office_02a_Radio_01,
            SE_ex_int_office_02b_Radio_01,
            SE_ex_int_office_02c_Radio_01,
            SE_ex_int_office_03a_Radio_01,
            SE_ex_int_office_03b_Radio_01,
            SE_ex_int_office_03c_Radio_01,
        }

        private MapObject MapObject { get; set; }

        public ExtraColshape Colshape { get; set; }

        public Sync.Radio.StationTypes RadioStationType { get; set; }

        public EmitterTypes EmitterType { get; set; }

        public static RadioEmitter GetById(string Id) => Additional.ExtraColshape.All.Where(x => x.Name == Id).FirstOrDefault()?.Data as RadioEmitter;

        public void Destroy()
        {
            if (Colshape?.Exists != true)
                return;

            Colshape.Destroy();
        }

        public RadioEmitter(string Id, Vector3 Position, float Range, uint Dimension, EmitterTypes EmitterType, Sync.Radio.StationTypes RadioStationType)
        {
            this.Colshape = new Additional.Sphere(Position, Range, false, Utils.RedColor, Dimension, null)
            {
                ApproveType = ExtraColshape.ApproveTypes.None,

                OnEnter = OnEnterColshape,
                OnExit = OnExitColshape,

                Name = $"RSE@{Id}",

                Data = this,
            };

            this.RadioStationType = RadioStationType;
            this.EmitterType = EmitterType;
        }

        private void OnEnterColshape(RAGE.Events.CancelEventArgs cancel)
        {
            var emitterStr = EmitterType.ToString();

            MapObject?.Destroy();

            var pos = Colshape.Position;

            if (pos == null)
                return;

            MapObject = Utils.CreateObjectNoOffsetImmediately(RAGE.Util.Joaat.Hash("prop_boombox_01"), pos.X, pos.Y, pos.Z);

            MapObject.SetVisible(false, false);

            Utils.LinkStaticEmitterToEntity(emitterStr, MapObject.Handle);

            RAGE.Game.Audio.SetEmitterRadioStation(emitterStr, Sync.Radio.GetRadioStationName(RadioStationType));

            RAGE.Game.Audio.SetStaticEmitterEnabled(emitterStr, true);
        }

        private void OnExitColshape(RAGE.Events.CancelEventArgs cancel)
        {
            MapObject?.Destroy();

            MapObject = null;
        }
    }
}
