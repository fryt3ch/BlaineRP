using System;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.DoorSystem
{
    public class Door
    {
        public Door(uint Id, uint Model, Vector3 Position, uint Dimension)
        {
            this.Id = Id;

            this.Model = Model;
            this.Position = Position;

            Service.All.Add(this);

            var cs = new Sphere(new Vector3(Position.X, Position.Y, Position.Z), 5f, false, Utils.Misc.RedColor, Dimension, null)
            {
                Name = $"DoorSys_{Id}",
                OnEnter = OnEnterColshape,
                OnExit = OnExitColshape,
            };

            World.Core.AddDataHandler($"DOORS_{Id}_L", OnLockStateChange);
        }

        public Door(uint Id, string Model, Vector3 Position, uint Dimension) : this(Id, RAGE.Util.Joaat.Hash(Model), Position, Dimension)
        {
        }

        private MapObject DoorObject { get; set; }

        public uint Model { get; set; }

        public Vector3 Position { get; set; }

        public string Name => Service.Names.Where(x => x.Key.Contains(Id)).Select(x => x.Value).FirstOrDefault();

        public uint Id { get; set; }

        public bool IsLocked => World.Core.GetSharedData<bool>($"DOORS_{Id}_L", false);

        public void ToggleLock(bool state)
        {
            Service.ToggleLock(this, state);
        }

        private async void OnEnterColshape(Events.CancelEventArgs cancel)
        {
            var id = $"DoorSys_{Id}";

            ExtraColshape cs = ExtraColshape.All.Where(x => x.Name == id).FirstOrDefault();

            if (cs == null)
                return;

            var handle = 0;

            while (handle <= 0)
            {
                handle = RAGE.Game.Object.GetClosestObjectOfType(Position.X, Position.Y, Position.Z, 1f, Model, false, true, true);

                if (handle <= 0)
                {
                    await RAGE.Game.Invoker.WaitAsync(25);

                    if (!cs.IsInside)
                        return;
                }
            }

            DoorObject = new MapObject(handle);

            string name = Name;

            DoorObject.SetData("Interactive", true);

            DoorObject.SetData("CustomText",
                (Action<float, float>)((x, y) =>
                {
                    Graphics.DrawText(name == null ? "Дверь" : $"Дверь - {name}",
                        x,
                        y - NameTags.Interval * 2f,
                        255,
                        255,
                        255,
                        255,
                        0.4f,
                        RAGE.Game.Font.ChaletComprimeCologne,
                        true
                    );

                    bool isLocked = IsLocked;

                    if (isLocked)
                        Graphics.DrawText($"[Закрыта]", x, y - NameTags.Interval, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                    else
                        Graphics.DrawText($"[Открыта]", x, y - NameTags.Interval, 0, 255, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                })
            );

            DoorObject.SetData("CustomAction",
                (Action<MapObject>)(async (obj) =>
                {
                    if (Service.LastSent.IsSpam(500, false, true))
                        return;

                    Service.LastSent = World.Core.ServerTime;

                    object res = await Events.CallRemoteProc("Door::Lock", Id, !IsLocked);
                })
            );
        }

        private void OnExitColshape(Events.CancelEventArgs cancel)
        {
            var id = $"DoorSys_{Id}";

            ExtraColshape cs = ExtraColshape.All.Where(x => x.Name == id).FirstOrDefault();

            if (cs == null)
                return;

            DoorObject?.Destroy();
        }

        private static void OnLockStateChange(string key, object value, object oldBalue)
        {
            string[] dataD = key.Split('_');

            var id = uint.Parse(dataD[1]);

            Door door = Service.GetDoorById(id);

            if (door == null)
                return;

            door.ToggleLock((bool?)value ?? false);
        }

        public static void PostInitializeAll()
        {
            foreach (Door x in Service.All)
            {
                x.ToggleLock(x.IsLocked);
            }
        }
    }
}