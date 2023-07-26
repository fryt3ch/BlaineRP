using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using BlaineRP.Client.UI.CEF;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes
{
    [Script(int.MaxValue)]
    public class Core
    {
        public static AsyncTask PolygonCreationTask { get; set; }

        public static Vector3 TempPosition { get; set; }

        public static Polygon TempPolygon { get; set; }

        public static bool CancelLastColshape { get; set; }

        public static string OverrideInteractionText { get; set; }

        public Core()
        {
            ExtraColshape.InteractionColshapesAllowed = true;

            Main.Render -= ExtraColshape.Render;
            Main.Render += ExtraColshape.Render;

            Events.Add("ExtraColshape::New", (object[] args) =>
            {
                var cs = (RAGE.Elements.Colshape)args[0];

                if (cs == null)
                    return;

                var tNum = cs.GetSharedData<int?>("Type", null);

                if (tNum == null)
                    return;

                var type = (ColshapeTypes)tNum;

                ExtraColshape data = null;

                var pos = RAGE.Util.Json.Deserialize<Vector3>(cs.GetSharedData<string>("Position"));
                var isVisible = cs.GetSharedData<bool>("IsVisible");
                var dim = RAGE.Util.Json.Deserialize<uint>(cs.GetSharedData<string>("Dimension"));
                var colour = RAGE.Util.Json.Deserialize<Utils.Colour>(cs.GetSharedData<string>("Colour"));

                if (type == ColshapeTypes.Circle)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Circle(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ColshapeTypes.Sphere)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Sphere(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ColshapeTypes.Cylinder)
                {
                    var radius = cs.GetSharedData<float>("Radius");
                    var height = cs.GetSharedData<float>("Height");

                    data = new Cylinder(pos, radius, height, isVisible, colour, dim, cs);
                }
                else if (type == ColshapeTypes.Polygon)
                {
                    var vertices = RAGE.Util.Json.Deserialize<List<Vector3>>(cs.GetSharedData<string>("Vertices"));
                    var height = cs.GetSharedData<float>("Height");
                    var heading = cs.GetSharedData<float>("Heading");

                    data = new Polygon(vertices, height, heading, isVisible, colour, dim, cs);
                }

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>(cs.GetSharedData<string>("Data"));

                var aType = (ActionTypes)cs.GetSharedData<int>("ActionType");
                var iType = (InteractionTypes)cs.GetSharedData<int>("InteractionType");

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
                data.ActionType = aType;
                data.InteractionType = iType;
            });

            Events.Add("ExtraColshape::Del", (object[] args) =>
            {
                var data = ExtraColshape.GetByRemoteId((int)args[0]);

                if (data == null)
                    return;

                data.Destroy();
            });

            Events.AddDataHandler("Data", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>((string)value);

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
            });

            Events.AddDataHandler("ActionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.ActionType = (ActionTypes)(int)value;
            });

            Events.AddDataHandler("InteractionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.InteractionType = (InteractionTypes)(int)value;
            });

            Events.AddDataHandler("IsVisible", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.IsVisible = (bool)value;
            });

            Events.AddDataHandler("Position", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Position = RAGE.Util.Json.Deserialize<Vector3>((string)value);
            });

            Events.AddDataHandler("Dimension", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Dimension = RAGE.Util.Json.Deserialize<uint>((string)value);
            });

            Events.AddDataHandler("Height", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Polygon)
                {
                    (data as Polygon).Height = (float)value;
                }
                else if (data is Cylinder)
                {
                    (data as Cylinder).Height = (float)value;
                }
            });

            Events.AddDataHandler("Heading", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (!(data is Polygon))
                    return;

                (data as Polygon).Heading = (float)value;
            });

            Events.AddDataHandler("Radius", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                if (data is Sphere)
                {
                    (data as Sphere).Radius = (float)value;
                }
                else if (data is Circle)
                {
                    (data as Circle).Radius = (float)value;
                }
            });

            Events.AddDataHandler("Colour", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.Colour = RAGE.Util.Json.Deserialize<Utils.Colour>((string)value);
            });

            Events.OnPlayerEnterColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (data.ActionType != ActionTypes.None)
                {
                    var action = ExtraColshape.GetEnterAction(data.ActionType);

                    action?.Invoke(data);
                }

                if (CancelLastColshape)
                {
                    CancelLastColshape = false;

                    data.IsInside = false;

                    return;
                }

                if (data.IsInteraction)
                {
                    if (!ExtraColshape.InteractionColshapesAllowed)
                        return;

                    var func = ExtraColshape.GetInteractionAction(data.InteractionType);

                    HUD.InteractionAction = func;

                    var interactionText = OverrideInteractionText;

                    if (interactionText != null)
                    {
                        OverrideInteractionText = null;
                    }
                    else
                    {
                        interactionText = Locale.Interaction.Names.GetValueOrDefault(data.InteractionType) ?? Locale.Interaction.Names.GetValueOrDefault(InteractionTypes.Interact) ?? "null";
                    }

                    HUD.SwitchInteractionText(true, interactionText);
                }

                data.OnEnter?.Invoke(null);
            };

            Events.OnPlayerExitColshape += (Colshape cs, Events.CancelEventArgs cancel) =>
            {
                if (cancel != null)
                    cancel.Cancel = true;

                var data = ExtraColshape.Get(cs);

                if (data == null)
                    return;

                if (cs.GetData<bool>("PendingDeletion"))
                {
                    ExtraColshape.All.Remove(data);
                }

                if (data.IsInteraction)
                {
                    HUD.InteractionAction = null;

                    HUD.SwitchInteractionText(false, string.Empty);
                }

                if (data.ActionType != ActionTypes.None)
                {
                    var action = ExtraColshape.GetExitAction(data.ActionType);

                    action?.Invoke(data);
                }

                data.OnExit?.Invoke(null);
            };
        }
    }
}
