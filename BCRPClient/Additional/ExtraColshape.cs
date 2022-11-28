using BCRPClient.CEF;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Additional
{
    public class ExtraColshapes : Events.Script
    {
        public static AsyncTask PolygonCreationTask { get; set; }

        public static Vector3 TempPosition { get; set; }

        public static Polygon TempPolygon { get; set; }

        public static bool CancelLastColshape { get; set; }

        private ExtraColshapes()
        {
            ExtraColshape.LastSent = DateTime.MinValue;
            ExtraColshape.InteractionColshapesAllowed = true;

            Events.Add("ExtraColshape::New", (object[] args) =>
            {
                var cs = (RAGE.Elements.Colshape)args[0];

                if (cs == null)
                    return;

                var tNum = cs.GetSharedData<int?>("Type", null);

                if (tNum == null)
                    return;

                var type = (ExtraColshape.Types)tNum;

                ExtraColshape data = null;

                var pos = RAGE.Util.Json.Deserialize<Vector3>(cs.GetSharedData<string>("Position"));
                var isVisible = cs.GetSharedData<bool>("IsVisible");
                var dim = RAGE.Util.Json.Deserialize<uint>(cs.GetSharedData<string>("Dimension"));
                var colour = RAGE.Util.Json.Deserialize<Utils.Colour>(cs.GetSharedData<string>("Colour"));

                if (type == ExtraColshape.Types.Circle)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Circle(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Sphere)
                {
                    var radius = cs.GetSharedData<float>("Radius");

                    data = new Sphere(pos, radius, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Cylinder)
                {
                    var radius = cs.GetSharedData<float>("Radius");
                    var height = cs.GetSharedData<float>("Height");

                    data = new Cylinder(pos, radius, height, isVisible, colour, dim, cs);
                }
                else if (type == ExtraColshape.Types.Polygon)
                {
                    var vertices = RAGE.Util.Json.Deserialize<List<Vector3>>(cs.GetSharedData<string>("Vertices"));
                    var height = cs.GetSharedData<float>("Height");
                    var heading = cs.GetSharedData<float>("Heading");

                    data = new Polygon(vertices, height, heading, isVisible, colour, dim, cs);
                }

                if (data == null)
                    return;

                var cData = RAGE.Util.Json.Deserialize<(System.Type, string)>(cs.GetSharedData<string>("Data"));

                var aType = (ExtraColshape.ActionTypes)cs.GetSharedData<int>("ActionType");
                var iType = (ExtraColshape.InteractionTypes)cs.GetSharedData<int>("InteractionType");

                data.Data = Newtonsoft.Json.JsonConvert.DeserializeObject(cData.Item2, cData.Item1);
                data.ActionType = aType;
                data.InteractionType = iType;
            });

            Events.Add("ExtraColshape::Del", (object[] args) =>
            {
                var data = ExtraColshape.GetByRemoteId((int)args[0]);

                if (data == null)
                    return;

                data.Delete();
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

                data.ActionType = (ExtraColshape.ActionTypes)(int)value;
            });

            Events.AddDataHandler("InteractionType", (Entity entity, object value, object oldValue) =>
            {
                if (entity?.Type != RAGE.Elements.Type.Colshape)
                    return;

                var data = ExtraColshape.Get(entity as Colshape);

                if (data == null)
                    return;

                data.InteractionType = (ExtraColshape.InteractionTypes)(int)value;
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

                if (data.ActionType != ExtraColshape.ActionTypes.None)
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

                    var func = ExtraColshape.GetInteractionFunc(data.InteractionType);

                    CEF.HUD.InteractionAction = func;

                    CEF.HUD.SwitchInteractionText(true, Locale.Interaction.Names[data.InteractionType]);
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

                if (data.IsInteraction)
                {
                    CEF.HUD.InteractionAction = null;

                    CEF.HUD.SwitchInteractionText(false, Locale.Interaction.Names[data.InteractionType]);
                }

                if (data.ActionType != ExtraColshape.ActionTypes.None)
                {
                    var action = ExtraColshape.GetExitAction(data.ActionType);

                    action?.Invoke(data);
                }

                data.OnExit?.Invoke(null);
            };
        }
    }

    public abstract class ExtraColshape
    {
        private static bool _InteractionColshapesAllowed { get; set; }

        /// <summary>Доступны ли в данный момент для взаимодействия соответствующие колшейпы?</summary>
        public static bool InteractionColshapesAllowed { get => _InteractionColshapesAllowed && !Utils.IsAnyCefActive(true); set { _InteractionColshapesAllowed = value; } }

        /// <summary>Время последней отправки на сервер, используя колшейп</summary>
        public static DateTime LastSent;

        public static bool RenderActive { set { GameEvents.Render -= Render; if (value) GameEvents.Render += Render; } }

        /// <summary>Словарь всех колшэйпов</summary>
        private static Dictionary<Colshape, ExtraColshape> All = new Dictionary<Colshape, ExtraColshape>();

        /// <summary>Список колшэйпов, находящихся в зоне стрима игрока</summary>
        private static List<ExtraColshape> Streamed = new List<ExtraColshape>();

        /// <summary>Получить колшейп по айди (локальный)</summary>
        public static ExtraColshape GetById(int id) => All.Where(x => x.Key?.Id == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить колшейп по айди (серверный)</summary>
        public static ExtraColshape GetByRemoteId(int id) => All.Where(x => x.Key?.RemoteId == id).Select(x => x.Value).FirstOrDefault();

        /// <summary>Получить колшейп по его держателю</summary>
        public static ExtraColshape Get(Colshape colshape) => All.GetValueOrDefault(colshape);

        public static Action<ExtraColshape> GetEnterAction(ActionTypes aType) => Actions.GetValueOrDefault(aType)?.GetValueOrDefault(true);

        public static Action<ExtraColshape> GetExitAction(ActionTypes aType) => Actions.GetValueOrDefault(aType)?.GetValueOrDefault(false);

        public static Func<bool> GetInteractionFunc(InteractionTypes iType) => InteractionFuncs.GetValueOrDefault(iType);

        /// <summary>Типы колшейпов</summary>
        public enum Types
        {
            /// <summary>Сферический (трехмерный)</summary>
            Sphere = 0,
            /// <summary>Круговой (двумерный)</summary>
            Circle,
            /// <summary>Цилиндрический (трехмерный)</summary>
            Cylinder,
            /// <summary>Многогранник (трехмерный/двумерный)</summary>
            /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный</remarks>
            Polygon,
        }

        public enum InteractionTypes
        {
            None = -1,

            DoorLock,
            DoorUnlock,

            BusinessEnter,
            BusinessInfo,

            HouseEnter,
            HouseExit,

            GarageExit,

            Locker,
            Fridge,
            Wardrobe,

            Interact,

            NpcDialogue,
            
            ATM,
        }

        public enum ActionTypes
        {
            /// <summary>Никакой, в таком случае нужно в ручную прописывать действия через OnEnter/OnExit</summary>
            None = -1,

            /// <summary>Зеленая (безопасная) зона</summary>
            GreenZone,

            /// <summary>Область, в которой можно заправлять транспорт (на заправках)</summary>
            GasStation,

            /// <summary>Межкомнатная дверь в доме/квартире</summary>
            HouseDoorLock,

            /// <summary>Межкомнатная дверь в доме/квартире</summary>
            HouseDoorUnlock,

            HouseEnter,
            HouseExit,

            BusinessEnter,
            BusinessInfo,

            IPL,

            NpcDialogue,

            ATM,
        }

        public static Dictionary<InteractionTypes, Func<bool>> InteractionFuncs = new Dictionary<InteractionTypes, Func<bool>>()
        {
            {
                InteractionTypes.DoorLock, Sync.House.DoorLock
            },

            {
                InteractionTypes.DoorUnlock, Sync.House.DoorLock
            },

            {
                InteractionTypes.BusinessEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                        return false;

                    Events.CallRemote("Business::Enter", Player.LocalPlayer.GetData<int>("CurrentBusiness"));

                    LastSent = DateTime.Now;

                    return true;
                }
            },

            {
                InteractionTypes.BusinessInfo, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("CurrentBusiness"))
                        return false;

                    Events.CallRemote("Business::ShowInfo", Player.LocalPlayer.GetData<int>("CurrentBusiness"));

                    LastSent = DateTime.Now;

                    return true;
                }
            },

            {
                InteractionTypes.HouseEnter, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("CurrentHouse"))
                        return false;

                    CEF.Estate.ShowHouseInfo(Player.LocalPlayer.GetData<BCRPClient.Data.Locations.House>("CurrentHouse"), true);

                    LastSent = DateTime.Now;

                    return true;
                }
            },

            {
                InteractionTypes.HouseExit, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                        return false;

                    var house = Player.LocalPlayer.GetData<BCRPClient.Data.Locations.House>("House::CurrentHouse");

                    if (house.GarageType == null)
                    {
                        Events.CallRemote("House::Exit");

                        LastSent = DateTime.Now;
                    }
                    else
                    {
                        CEF.ActionBox.ShowSelect(ActionBox.Contexts.HouseExit, Locale.Actions.HouseExitActionBoxHeader, (0, Locale.Actions.HouseExitActionBoxOutside), (1, Locale.Actions.HouseExitActionBoxToGarage));
                    }

                    return true;
                }
            },

            {
                InteractionTypes.GarageExit, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                    {
                        // todo
                    }
                    else
                    {
                        CEF.ActionBox.ShowSelect(ActionBox.Contexts.HouseExit, Locale.Actions.HouseExitActionBoxHeader, (2, Locale.Actions.HouseExitActionBoxToHouse), (0, Locale.Actions.HouseExitActionBoxOutside));
                    }

                    return true;
                }
            },

            {
                InteractionTypes.NpcDialogue, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("CurrentNPC"))
                        return false;

                    var npc = Player.LocalPlayer.GetData<Data.NPC>("CurrentNPC");

                    if (npc == null)
                        return false;

                    npc.SwitchDialogue(true);

                    npc.ShowDialogue(npc.DefaultDialogueId);

                    LastSent = DateTime.Now;

                    return true;
                }
            },

            {
                InteractionTypes.ATM, () =>
                {
                    if (LastSent.IsSpam(1000, false, false))
                        return false;

                    if (!Player.LocalPlayer.HasData("CurrentATM"))
                        return false;

                    Events.CallRemote("Bank::Show", true, Player.LocalPlayer.GetData<BCRPClient.Data.Locations.ATM>("CurrentATM").Id);

                    LastSent = DateTime.Now;

                    return true;
                }
            }
        };

        public static Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>> Actions = new Dictionary<ActionTypes, Dictionary<bool, Action<ExtraColshape>>>()
        {
            {
                ActionTypes.GasStation,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int data)
                            {
                                Player.LocalPlayer.SetData("CurrentGasStation", data);

                                //CEF.Notification.ShowHint(Locale.Notifications.Hints.GasStationColshape, false, 2500);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentGasStation");

                            CEF.Gas.Close();
                        }
                    },
                }
            },

            {
                ActionTypes.GreenZone,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            Sync.WeaponSystem.DisabledFiring = true;

                            Player.LocalPlayer.SetData("InGreenZone", true);

                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, true);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Sync.WeaponSystem.DisabledFiring = false;

                            Player.LocalPlayer.ResetData("InGreenZone");

                            CEF.HUD.SwitchStatusIcon(HUD.StatusTypes.GreenZone, false);
                        }
                    },
                }
            },

            {
                ActionTypes.IPL,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Sync.IPLManager.IPLInfo ipl)
                            {
                                ipl.Load();
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            if (cs.Data is Sync.IPLManager.IPLInfo ipl)
                            {
                                ipl.Unload();
                            }
                        }
                    },
                }
            },

            {
                ActionTypes.HouseDoorLock,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int doorIdx)
                            {
                                Player.LocalPlayer.SetData("House::CurrentDoorIdx", doorIdx);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("House::CurrentDoorIdx");
                        }
                    },
                }
            },

            {
                ActionTypes.HouseDoorUnlock,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int doorIdx)
                            {
                                Player.LocalPlayer.SetData("House::CurrentDoorIdx", doorIdx);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("House::CurrentDoorIdx");
                        }
                    },
                }
            },

            {
                ActionTypes.BusinessEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int businessId)
                            {
                                Player.LocalPlayer.SetData("CurrentBusiness", businessId);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentBusiness");
                        }
                    },
                }
            },

            {
                ActionTypes.BusinessInfo,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is int businessId)
                            {
                                Player.LocalPlayer.SetData("CurrentBusiness", businessId);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentBusiness");
                        }
                    },
                }
            },

            {
                ActionTypes.HouseEnter,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (!(cs.Data is Data.Locations.House house))
                                return;

                            Player.LocalPlayer.SetData("CurrentHouse", house);
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentHouse");
                        }
                    },
                }
            },

            {
                ActionTypes.NpcDialogue,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.NPC npc)
                            {
                                Player.LocalPlayer.SetData("CurrentNPC", npc);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentNPC");
                        }
                    },
                }
            },

            {
                ActionTypes.ATM,

                new Dictionary<bool, Action<ExtraColshape>>()
                {
                    {
                        true,

                        (cs) =>
                        {
                            if (cs.Data is Data.Locations.ATM atm)
                            {
                                Player.LocalPlayer.SetData("CurrentATM", atm);
                            }
                        }
                    },

                    {
                        false,

                        (cs) =>
                        {
                            Player.LocalPlayer.ResetData("CurrentATM");
                        }
                    },
                }
            },
        };

        /// <summary>Сущность-держатель колшейпа, не имеет функциональности</summary>
        public Colshape Colshape { get; set; }

        /// <summary>Тип колшейпа</summary>
        public Types Type { get; set; }

        /// <summary>Тип действия при входе/выходе в колшейп</summary>
        public ActionTypes ActionType { get; set; }

        /// <summary>Тип действия для взаимодействия</summary>
        public InteractionTypes InteractionType { get; set; }

        /// <summary>Позиция</summary>
        public Vector3 Position { get; set; }

        /// <summary>Для взаимодействия ли колшейп?</summary>
        /// <remarks>Если колшейп используется для взаимодействия, то ивенты OnEnter и OnExit будут срабатывать также в зависимости от того, открыт ли какой либо интерфейс у игрока</remarks>
        public bool IsInteraction { get => InteractionType != InteractionTypes.None; }

        /// <summary>Измерение</summary>
        /// <remarks>Если используется uint.MaxValue, то колшейп работает независимо от измерения игрока</remarks>
        public uint Dimension { get; set; }

        /// <summary>Цвет</summary>
        public Utils.Colour Colour { get; set; }

        /// <summary>Видимый ли?</summary>
        /// <remarks>Если колшейп видимый, то его будут видеть все игроки, иначе - только администраторы, и то, при включенной настройке на стороне клиента</remarks>
        public bool IsVisible { get; set; }

        /// <summary>Находится ли игрок внутри?</summary>
        public bool IsInside { get; set; }

        /// <summary>Название колшейпа</summary>
        public string Name { get; set; }

        /// <summary>Метод для отрисовки колшейпа на экране</summary>
        public abstract void Draw();

        /// <summary>Метод для проверки, находится ли точка в колшейпе</summary>
        /// <param name="point">Точка</param>
        public abstract bool IsPointInside(Vector3 point);

        /// <summary>Метод для задания новой позиции колшейпа</summary>
        /// <param name="position">Позиция</param>
        public virtual void SetPosition(Vector3 position) => Position = position;

        /// <summary>Метод для проверки, находится ли колшейп в зоне стрима для игрока</summary>
        public virtual bool IsStreamed() => Colshape?.IsNull == false && (Dimension == uint.MaxValue || Player.LocalPlayer.Dimension == Dimension);

        /// <summary>Данные колшейпа</summary>
        public object Data { get; set; }

        public Colshape.ColshapeEventDelegate OnEnter { get => Colshape?.OnEnter; set { if (Colshape?.IsNull != false) return; Colshape.OnEnter = value; } }
        public Colshape.ColshapeEventDelegate OnExit { get => Colshape?.OnExit; set { if (Colshape?.IsNull != false) return; Colshape.OnExit = value; } }

        public void Delete()
        {
            Streamed.Remove(this);

            if (Colshape != null)
            {
                if (IsInside)
                {
                    Events.OnPlayerExitColshape?.Invoke(Colshape, null);
                }

                All.Remove(Colshape);

                Colshape.ResetData();

                Colshape.Destroy();
            }
        }

        public ExtraColshape(Types Type, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null, InteractionTypes InteractionType = InteractionTypes.None, ActionTypes ActionType = ActionTypes.None)
        {
            this.Colshape = Colshape ?? new RAGE.Elements.SphereColshape(Vector3.Zero, 0f, Settings.STUFF_DIMENSION);

            this.Type = Type;
            this.Colour = Colour;
            this.Dimension = Dimension;
            this.IsVisible = IsVisible;

            this.InteractionType = InteractionType;
            this.ActionType = ActionType;

            All.Add(this.Colshape, this); // the same key problem???
        }

        private static void Render()
        {
            var isAdmin = Sync.Players.GetData(RAGE.Elements.Player.LocalPlayer).AdminLevel >= 0;

            for (int i = 0; i < Streamed.Count; i++)
            {
                var curColshape = Streamed[i];

                if (curColshape?.Colshape?.IsNull != false)
                    continue;

                if (curColshape.IsVisible || isAdmin)
                    curColshape.Draw();
            }
        }

        public static void Activate()
        {
            (new AsyncTask(() => UpdateInside(), 200, true, 0)).Run();

            (new AsyncTask(() => UpdateStreamed(), 1000, true, 0)).Run();
        }

        public static void UpdateStreamed()
        {
            foreach (var x in All.Keys)
            {
                var cs = All[x];

                var state = cs?.IsStreamed();

                if (state == null)
                    continue;

                if (state == true)
                {
                    if (Streamed.Contains(cs))
                        continue;

                    Streamed.Add(cs);
                }
                else
                {
                    if (Streamed.Remove(cs))
                    {
                        if (cs.IsInside)
                        {
                            cs.IsInside = false;

                            Events.OnPlayerExitColshape?.Invoke(cs.Colshape, null);
                        }
                    }
                }
            }
        }

        public static void UpdateInside()
        {
            bool interactionAllowed = InteractionColshapesAllowed;

            for (int i = 0; i < Streamed.Count; i++)
            {
                var curPoly = Streamed[i];

                if (curPoly?.Colshape?.IsNull != false)
                {
                    if (curPoly?.Colshape != null)
                        All.Remove(curPoly.Colshape);

                    continue;
                }

                if (curPoly.IsInside)
                {
                    if ((curPoly.IsInteraction && !interactionAllowed) || !curPoly.IsPointInside(RAGE.Elements.Player.LocalPlayer.Position))
                    {
                        curPoly.IsInside = false;

                        Events.OnPlayerExitColshape?.Invoke(curPoly.Colshape, null);
                    }
                }
                else
                {
                    if (curPoly.IsInteraction && !interactionAllowed)
                        continue;

                    if (curPoly.IsPointInside(RAGE.Elements.Player.LocalPlayer.Position))
                    {
                        curPoly.IsInside = true;

                        Events.OnPlayerEnterColshape?.Invoke(curPoly.Colshape, null);
                    }
                }
            }
        }
    }

    public class Sphere : ExtraColshape
    {
        public float Radius { get; set; }

        public Sphere(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Sphere, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                return;

            Utils.DrawSphere(Position, Radius, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha / 255f);

            if (Settings.Other.DebugLabels)
            {
                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Radius + Settings.STREAM_DISTANCE;
        }

        public override bool IsPointInside(Vector3 point) => Vector3.Distance(point, Position) <= Radius;
    }

    public class Circle : ExtraColshape
    {
        public float Radius { get; set; }

        public Circle(Vector3 Position, float Radius, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Circle, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;

            this.Position = Position;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                return;

            var diameter = Radius * 2;

            RAGE.Game.Graphics.DrawMarker(23, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, 1f, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

            if (Settings.Other.DebugLabels)
            {
                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= Radius + Settings.STREAM_DISTANCE;
        }

        public override bool IsPointInside(Vector3 point) => point.DistanceIgnoreZ(Position) <= Radius;
    }

    public class Cylinder : ExtraColshape
    {
        public float Radius { get; set; }
        public float Height { get; set; }

        public Cylinder(Vector3 Position, float Radius, float Height, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Cylinder, IsVisible, Colour, Dimension, Colshape)
        {
            this.Radius = Radius;
            this.Height = Height;

            this.Position = Position;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                return;

            var diameter = Radius * 2;

            RAGE.Game.Graphics.DrawMarker(1, Position.X, Position.Y, Position.Z, 0f, 0f, 0f, 1f, 1f, 1f, diameter, diameter, Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha, false, false, 2, false, null, null, false);

            if (Settings.Other.DebugLabels)
            {
                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Radius: {Radius} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            if (point.Z < Position.Z || point.Z > Position.Z + Height)
                return false;

            return Position.DistanceIgnoreZ(point) <= Radius;
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Vector3.Distance(Player.LocalPlayer.Position, Position) <= Height + Radius + Settings.STREAM_DISTANCE;
        }
    }

    public class Polygon : ExtraColshape
    {
        public float MaxRange { get; set; }

        public float Height { get; set; }

        public float Heading { get; set; }

        public List<Vector3> Vertices { get; set; }

        public bool Is3D { get => Height > 0; }

        public Polygon(List<Vector3> Vertices, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION, Colshape Colshape = null) : base(Types.Polygon, IsVisible, Colour, Dimension, Colshape)
        {
            this.Height = Height;

            this.Heading = Heading;

            this.Vertices = Vertices;

            this.Position = GetCenterPosition(Vertices, Height);

            UpdatePolygonCenterAndMaxRange();

            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i] = Utils.RotatePoint(Vertices[i], Position, Heading);

            SetHeading(Heading);
        }

        public static Polygon CreateCuboid(Vector3 Position, float Width, float Depth, float Height, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION)
        {
            var vertices = new List<Vector3>()
                {
                    new Vector3(Position.X - Width / 2, Position.Y - Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X + Width / 2, Position.Y - Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X + Width / 2, Position.Y + Depth / 2, Position.Z - Height / 2),
                    new Vector3(Position.X - Width / 2, Position.Y + Depth / 2, Position.Z - Height / 2),
                };

            return new Polygon(vertices, Height, Heading, IsVisible, Colour, Dimension);
        }

        public static Polygon CreateCuboid(Vector3 Position1, Vector3 Position2, float Heading, bool IsVisible, Utils.Colour Colour, uint Dimension = Settings.MAIN_DIMENSION)
        {
            var middlePos = Position1.GetMiddle(Position2);

            var width = Math.Abs(Position2.X - Position1.X);
            var depth = Math.Abs(Position2.Y - Position1.Y);
            var height = Math.Abs(Position2.Z - Position1.Z);

            return CreateCuboid(middlePos, width, depth, height, Heading, IsVisible, Colour, Dimension);
        }

        private void UpdatePolygonCenterAndMaxRange()
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < Vertices.Count; i++)
            {
                centerPos.X += Vertices[i].X;
                centerPos.Y += Vertices[i].Y;
                centerPos.Z += Vertices[i].Z;
            }

            centerPos.X /= Vertices.Count;
            centerPos.Y /= Vertices.Count;
            centerPos.Z /= Vertices.Count;

            centerPos.Z += Height / 2;

            Position = centerPos;
            MaxRange = Vertices.Max(x => x.DistanceTo(centerPos));
        }

        public void SetHeading(float heading)
        {
            if (heading >= 360f)
                heading %= 360f;
            else if (heading <= -360f)
                heading = -(-heading % 360f);

            if (heading == Heading)
                return;

            var vertices = Vertices;
            heading = (float)(heading * Math.PI / 180);
            var originPoint = Position;
            float cos = (float)Math.Cos(heading), sin = (float)Math.Sin(heading);

            for (int i = 0; i < Vertices.Count; i++)
            {
                var point = vertices[i];

                float x = point.X, y = point.Y;

                point.X = cos * (x - originPoint.X) - sin * (y - originPoint.Y) + originPoint.X;
                point.Y = sin * (x - originPoint.X) + cos * (y - originPoint.Y) + originPoint.Y;

                vertices[i] = point;
            }

            Vertices = vertices;
        }

        public static Vector3 GetCenterPosition(List<Vector3> vertices, float height)
        {
            Vector3 centerPos = new Vector3(0, 0, 0);

            for (int i = 0; i < vertices.Count; i++)
            {
                centerPos.X += vertices[i].X;
                centerPos.Y += vertices[i].Y;
                centerPos.Z += vertices[i].Z;
            }

            centerPos.X /= vertices.Count;
            centerPos.Y /= vertices.Count;
            centerPos.Z /= vertices.Count;

            centerPos.Z += height / 2;

            return centerPos;
        }

        public void Rotate(float angle)
        {
            if (angle >= 360f)
                angle %= 360f;
            else if (angle <= -360f)
                angle = -(-angle % 360f);

            var lastAngle = Heading;

            var diff = lastAngle + angle;

            if (diff >= 360f)
                diff %= 360f;
            else if (diff <= -360f)
                diff = -(-diff % 360f);

            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i] = Utils.RotatePoint(Vertices[i], Position, angle);

            Heading = diff;
        }

        public override void SetPosition(Vector3 position)
        {
            float diffX = position.X - Position.X;
            float diffY = position.Y - Position.Y;
            float diffZ = position.Z - Position.Z;

            if (diffX == 0f && diffY == 0f && diffZ == 0f)
                return;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var curVertice = Vertices[i];

                curVertice.X += diffX;
                curVertice.Y += diffY;
                curVertice.Z += diffZ;

                Vertices[i] = curVertice;
            }
        }

        public void SetHeight(float height)
        {
            if (height < 0f)
                height = 0;

            Height = height;

            UpdatePolygonCenterAndMaxRange();
        }

        public void AddVertice(Vector3 vertice)
        {
            if (!Is3D)
                vertice.Z = Vertices[0].Z;

            Vertices.Add(vertice);

            UpdatePolygonCenterAndMaxRange();
        }

        public void RemoveVertice(int verticeId)
        {
            if (verticeId < 0 || verticeId >= Vertices.Count)
                return;

            Vertices.RemoveAt(verticeId);

            if (Vertices.Count == 0)
            {
                Delete();

                return;
            }

            UpdatePolygonCenterAndMaxRange();
        }

        public override bool IsStreamed()
        {
            if (!base.IsStreamed())
                return false;

            return Position.DistanceIgnoreZ(Player.LocalPlayer.Position) <= MaxRange + Settings.STREAM_DISTANCE;
        }

        public override void Draw()
        {
            float screenX = 0f, screenY = 0f;

            var vertIdLimiter = Vertices.Count <= 50 ? 1 : 10;

            if (Vertices.Count == 1)
            {
                var vertice = Vertices[0];
                RAGE.Game.Graphics.DrawLine(vertice.X, vertice.Y, vertice.Z, vertice.X, vertice.Y, vertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
            }
            else if (Settings.Other.HighPolygonsMode)
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Position.X, Position.Y, Position.Z + Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawPoly(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Position.X, Position.Y, Position.Z - Height / 2, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            }
            else
            {
                if (Height == 0)
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        var currentVertice = Vertices[i];
                        var nextVertice = i == Vertices.Count - 1 ? Vertices[0] : Vertices[i + 1];

                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z, currentVertice.X, currentVertice.Y, currentVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(nextVertice.X, nextVertice.Y, nextVertice.Z, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);
                        RAGE.Game.Graphics.DrawLine(currentVertice.X, currentVertice.Y, currentVertice.Z + Height, nextVertice.X, nextVertice.Y, nextVertice.Z + Height, Colour.Red, Colour.Green, Colour.Blue, Colour.Alpha);

                        if (Settings.Other.DebugLabels && (i % vertIdLimiter == 0))
                        {
                            if (!Utils.GetScreenCoordFromWorldCoord(currentVertice, ref screenX, ref screenY))
                                continue;

                            Utils.DrawText(i.ToString(), screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            }

            if (Settings.Other.DebugLabels)
            {
                if (!Utils.GetScreenCoordFromWorldCoord(Position, ref screenX, ref screenY))
                    return;

                Utils.DrawText($"Name: {Name} | Type: {Type} | ID: {Colshape.Id} | IsLocal: {Colshape?.IsLocal == true}", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"Vertices: {Vertices.Count} | Height: {Height}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                Utils.DrawText($"ActionType: {ActionType} | InteractionType: {InteractionType} | Data: {Data}", screenX, screenY += NameTags.Interval / 2, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }

        public override bool IsPointInside(Vector3 point)
        {
            double angleSum = 0f;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var p1 = new Vector3(Vertices[i].X - Position.X, Vertices[i].Y - Position.Y, Vertices[i].Z - Position.Z);
                var p2 = new Vector3(Vertices[(i + 1) % Vertices.Count].X - Position.X, Vertices[(i + 1) % Vertices.Count].Y - Position.Y, Vertices[(i + 1) % Vertices.Count].Z - Position.Z);

                var m1 = Math.Sqrt((p1.X * p1.X) + (p1.Y * p1.Y) + (p1.Z * p1.Z));
                var m2 = Math.Sqrt((p2.X * p2.X) + (p2.Y * p2.Y) + (p2.Z * p2.Z));

                if (m1 * m2 <= float.Epsilon)
                {
                    angleSum = Math.PI * 2;

                    break;
                }
                else
                    angleSum += Math.Acos((p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z) / (m1 * m2));
            }

            var polygonPoints2d = new List<RAGE.Ui.Cursor.Vector2>();

            if (Height == 0)
            {
                for (int i = 0; i < Vertices.Count; i++)
                    polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
            }
            else
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (Position.Z >= Vertices[i].Z && Position.Z <= (Vertices[i].Z + Height) || angleSum >= 5.8f)
                        polygonPoints2d.Add(new RAGE.Ui.Cursor.Vector2(Vertices[i].X, Vertices[i].Y));
                    else
                        return false;
                }
            }

            bool inside = false;

            for (int i = 0, j = polygonPoints2d.Count - 1; i < polygonPoints2d.Count; j = i++)
            {
                float xi = polygonPoints2d[i].X, yi = polygonPoints2d[i].Y;
                float xj = polygonPoints2d[j].X, yj = polygonPoints2d[j].Y;

                if (((yi > Position.Y) != (yj > Position.Y)) && (Position.X < (xj - xi) * (Position.Y - yi) / (yj - yi) + xi))
                    inside = !inside;
            }

            return inside;
        }
    }
}
