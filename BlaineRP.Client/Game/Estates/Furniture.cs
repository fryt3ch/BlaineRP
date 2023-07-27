using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Estates
{
    public partial class Furniture
    {
        private static Dictionary<Types, Action<MapObject, object[]>> CreateActions = new Dictionary<Types, Action<MapObject, object[]>>()
        {
            {
                Types.Locker, (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },
            {
                Types.Wardrobe, (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },
            {
                Types.Fridge, (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },
            {
                Types.KitchenSet, (obj, args) =>
                {
                    obj.SetData("UID", (uint)args[0]);
                }
            },
        };

        private static Dictionary<Types, Action<MapObject>> InteractionActions = new Dictionary<Types, Action<MapObject>>()
        {
            {
                Types.Locker, (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                        Inventory.Show(Inventory.Types.Container, contId);
                }
            },
            {
                Types.Wardrobe, (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                        Inventory.Show(Inventory.Types.Container, contId);
                }
            },
            {
                Types.Fridge, (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                        Inventory.Show(Inventory.Types.Container, contId);
                }
            },
            {
                Types.KitchenSet, (obj) =>
                {
                    if (obj.GetData<uint?>("UID") is uint uid)
                        Inventory.Show(Inventory.Types.Workbench, 1, uid);
                }
            },
        };

        public Furniture(string Id, Types Type, string Name, uint Model)
        {
            this.Id = Id;
            this.Type = Type;
            this.Name = Name;

            this.Model = Model;

            All.Add(Id, this);
        }

        public static Dictionary<string, Furniture> All { get; set; } = new Dictionary<string, Furniture>();

        public string Id { get; set; }

        public string Name { get; set; }

        public Types Type { get; set; }

        public uint Model { get; set; }

        public Action<MapObject> InteractionAction => InteractionActions.GetValueOrDefault(Type);

        public static Furniture GetData(string id)
        {
            return All.GetValueOrDefault(id);
        }

        public static Action<MapObject> GetInteractionAction(string id)
        {
            return InteractionActions.GetValueOrDefault(All[id].Type);
        }

        public static Action<MapObject, object[]> GetCreateAction(string id)
        {
            return CreateActions.GetValueOrDefault(All[id].Type);
        }

        public MapObject CreateObject(Vector3 pos, Vector3 rot, uint dim, uint uid, params object[] args)
        {
            MapObject obj = Streaming.CreateObjectNoOffsetImmediately(Model, pos.X, pos.Y, pos.Z);

            obj.FreezePosition(true);

            obj.SetRotation(rot.X, rot.Y, rot.Z, 2, true);

            obj.SetData("UID", uid);

            Action<MapObject, object[]> cAct = CreateActions.GetValueOrDefault(Type);

            if (cAct == null)
                return obj;

            obj.SetData("Interactive", true);

            obj.SetData("Furniture", this);

            cAct.Invoke(obj, args);

            return obj;
        }
    }
}