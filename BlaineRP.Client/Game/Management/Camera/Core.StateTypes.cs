using System.Collections.Generic;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Camera
{
    internal partial class Core
    {
        public enum StateTypes
        {
            Head = 0,
            Body,
            Legs,
            Foots,
            RightHand,
            LeftHand,
            WholePed,

            BodyUpper,
            BodyBackUpper,
            RightHandUpper,
            LeftHandUpper,
            LeftLeg,
            RightLeg,

            BodyBack,

            RightHandFingers,
            LeftHandFingers,

            WholeVehicle,
            WholeVehicleOpen,
            FrontVehicle,
            FrontVehicleOpenHood,
            BackVehicle,
            BackVehicleOpenTrunk,
            RightVehicle,
            LeftVehicle,
            TopVehicle,
            BackVehicleUpAngle,

            NpcTalk,

            CasinoRouletteGame,

            Empty,

            WholeFurniture,
            FrontFurniture,
            TopFurniture,
        }

        public static Dictionary<StateTypes, CameraState> States = new Dictionary<StateTypes, CameraState>()
        {
            {
                StateTypes.Foots, new CameraState(new Vector3(0f, 0f, -0.5f),
                    new Vector3(0f, 0f, 0f),
                    40,
                    new Vector3(0f, 0f, -0.75f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.Legs, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    55,
                    new Vector3(0f, 0f, -0.25f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.LeftLeg, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    55,
                    new Vector3(0f, 0f, -0.25f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 46078,
                    MinFov = 10,
                }
            },
            {
                StateTypes.RightLeg, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    55,
                    new Vector3(0f, 0f, -0.25f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 16335,
                    MinFov = 10,
                }
            },
            {
                StateTypes.Body, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.BodyUpper, new CameraState(new Vector3(0f, 0f, 0.5f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.BodyBack, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        180f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.BodyBackUpper, new CameraState(new Vector3(0f, 0f, 0.5f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        180f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.Head, new CameraState(new Vector3(0, 0, 1f),
                    new Vector3(0f, 0f, 0f),
                    30,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.Position
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 31086,
                    MinFov = 10,
                }
            },
            {
                StateTypes.LeftHand, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    30,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 36029,
                    MinFov = 10,
                }
            },
            {
                StateTypes.LeftHandFingers, new CameraState(new Vector3(0f, 0f, -1f),
                    new Vector3(0f, 0f, 0f),
                    25,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        45f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 4137,
                    MinFov = 10,
                }
            },
            {
                StateTypes.LeftHandUpper, new CameraState(new Vector3(0f, 0f, 1f),
                    new Vector3(0f, 0f, 0f),
                    30,
                    new Vector3(0f, 0f, 0.2f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 36029,
                    MinFov = 10,
                }
            },
            {
                StateTypes.RightHand, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    30,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 57005,
                    MinFov = 10,
                }
            },
            {
                StateTypes.RightHandUpper, new CameraState(new Vector3(0f, 0f, 1f),
                    new Vector3(0f, 0f, 0f),
                    30,
                    new Vector3(0f, 0f, 0.2f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 57005,
                    MinFov = 10,
                }
            },
            {
                StateTypes.RightHandFingers, new CameraState(new Vector3(0f, 0f, -1f),
                    new Vector3(0f, 0f, 0f),
                    25,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        -45f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 64064,
                    MinFov = 10,
                }
            },
            {
                StateTypes.WholePed, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    80,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.Position,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 23553,
                    MinFov = 10,
                }
            },
            {
                StateTypes.NpcTalk, new CameraState(new Vector3(0f, 0f, 1f), null, 30, new Vector3(0f, 0f, 0f), 750, CameraState.RenderTypes.Both, CameraState.RenderTypes.None)
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        1.2f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointBone,
                    TargetParams = 31086,
                    MinFov = 10,
                }
            },
            {
                StateTypes.WholeVehicle,
                new CameraState(new Vector3(0f, 0f, 1.35f), null, 60, new Vector3(0f, 0f, 0f), 750, CameraState.RenderTypes.None, CameraState.RenderTypes.None)
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        45f,
                        5.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                }
            },
            {
                StateTypes.WholeVehicleOpen, new CameraState(new Vector3(0f, 0f, 1.35f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        45f,
                        5.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                    OnAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                            for (var i = 0; i < 8; i++)
                            {
                                if (veh.DoesHaveDoor(i) > 0)
                                    veh.SetDoorOpen(i, false, false);
                            }
                    },
                    OffAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                            for (var i = 0; i < 8; i++)
                            {
                                if (veh.DoesHaveDoor(i) > 0)
                                    veh.SetDoorShut(i, false);
                            }
                    },
                }
            },
            {
                StateTypes.FrontVehicle, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    70,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                }
            },
            {
                StateTypes.FrontVehicleOpenHood, new CameraState(new Vector3(0, 0, 1f),
                    new Vector3(0f, 0f, 0f),
                    70,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        0f,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                    OnAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                            if (veh.DoesHaveDoor(4) > 0)
                                veh.SetDoorOpen(4, false, false);
                    },
                    OffAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                            if (veh.DoesHaveDoor(4) > 0)
                                veh.SetDoorShut(4, false);
                    },
                }
            },
            {
                StateTypes.BackVehicleOpenTrunk, new CameraState(new Vector3(0, 0, 1f),
                    new Vector3(0f, 0f, 0f),
                    70,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        -180f,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                    OnAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                        {
                            RAGE.Game.Entity.SetEntityHeading(veh.Handle, RAGE.Game.Entity.GetEntityHeading(veh.Handle) - 180f);

                            if (veh.DoesHaveDoor(5) > 0)
                                veh.SetDoorOpen(5, false, false);
                        }
                    },
                    OffAction = (args) =>
                    {
                        if (SourceEntity is Vehicle veh)
                            if (veh.DoesHaveDoor(5) > 0)
                                veh.SetDoorShut(5, false);
                    },
                }
            },
            {
                StateTypes.BackVehicle, new CameraState(new Vector3(0, 0, 0),
                    new Vector3(0f, 0f, 0f),
                    70,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        -180,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                    OnAction = (args) =>
                    {
                        if (SourceEntity is GameEntity gEntity)
                            RAGE.Game.Entity.SetEntityHeading(gEntity.Handle, RAGE.Game.Entity.GetEntityHeading(gEntity.Handle) - 180f);
                    },
                }
            },
            {
                StateTypes.BackVehicleUpAngle, new CameraState(new Vector3(0, 0, 1.35f),
                    new Vector3(0f, 0f, 0f),
                    60,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        210f,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                    OnAction = (args) =>
                    {
                        if (SourceEntity is GameEntity gEntity)
                            RAGE.Game.Entity.SetEntityHeading(gEntity.Handle, RAGE.Game.Entity.GetEntityHeading(gEntity.Handle) + 210f);
                    },
                }
            },
            {
                StateTypes.RightVehicle, new CameraState(new Vector3(0, 0, 0),
                    new Vector3(0f, 0f, 0f),
                    80,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        90f,
                        3.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                }
            },
            {
                StateTypes.LeftVehicle, new CameraState(new Vector3(0, 0, 0),
                    new Vector3(0f, 0f, 0f),
                    80,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        -90f,
                        3.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                }
            },
            {
                StateTypes.TopVehicle, new CameraState(new Vector3(0, 0, 4f),
                    new Vector3(0f, 0f, 0f),
                    70,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.PointAt,
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 10,
                }
            },
            {
                StateTypes.CasinoRouletteGame, new CameraState(new Vector3(0f, 0f, 2f),
                    new Vector3(0f, 0f, 0f),
                    80,
                    new Vector3(0f, 0f, 0f),
                    750,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.PointAt,
                    TargetBehaviourType = CameraState.BehaviourTypes.None,
                    ShakeAmplitude = 0f,
                }
            },
            {
                StateTypes.Empty, new CameraState(new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f),
                    0,
                    new Vector3(0f, 0f, 0f),
                    0,
                    CameraState.RenderTypes.None,
                    CameraState.RenderTypes.None
                )
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.None,
                    TargetBehaviourType = CameraState.BehaviourTypes.None,
                    ShakeAmplitude = 0f,
                    MinFov = 0f,
                    MaxFov = 180f,
                }
            },
            {
                StateTypes.WholeFurniture,
                new CameraState(new Vector3(0f, 0f, 1.35f), null, 60, new Vector3(0f, 0f, 0f), 750, CameraState.RenderTypes.None, CameraState.RenderTypes.None)
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        180f + 35f,
                        5.5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 5,
                }
            },
            {
                StateTypes.FrontFurniture,
                new CameraState(new Vector3(0f, 0f, 0f), null, 60, new Vector3(0f, 0f, 0f), 750, CameraState.RenderTypes.None, CameraState.RenderTypes.None)
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.FrontOf,
                    SourceParams = new float[]
                    {
                        180f,
                        5f,
                    },
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 5,
                }
            },
            {
                StateTypes.TopFurniture,
                new CameraState(new Vector3(0f, 0f, 3f), null, 70, new Vector3(0f, 0f, 0f), 750, CameraState.RenderTypes.None, CameraState.RenderTypes.None)
                {
                    SourceBehaviourType = CameraState.BehaviourTypes.PointAt,
                    TargetBehaviourType = CameraState.BehaviourTypes.PointAt,
                    MinFov = 5,
                }
            },
        };
    }
}