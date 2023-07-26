using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Game.Management.Misc;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes
{
    public abstract partial class ExtraColshape
    {
        private static bool _interactionColshapesAllowed { get; set; }

        /// <summary>Доступны ли в данный момент для взаимодействия соответствующие колшейпы?</summary>
        public static bool InteractionColshapesAllowed
        {
            get => _interactionColshapesAllowed && !Utils.Misc.IsAnyCefActive(true) && !SkyCamera.IsFadedOut;
            set => _interactionColshapesAllowed = value;
        }

        public static bool InteractionColshapesDisabledThisFrame { get; set; }

        /// <summary>Время последней отправки на сервер, используя колшейп</summary>
        public static DateTime LastSent;

        /// <summary>Словарь всех колшэйпов</summary>
        public static List<ExtraColshape> All { get; private set; } = new List<ExtraColshape>();

        /// <summary>Список колшэйпов, находящихся в зоне стрима игрока</summary>
        public static List<ExtraColshape> Streamed { get; private set; } = new List<ExtraColshape>();

        /// <summary>Получить колшейп по айди (локальный)</summary>
        public static ExtraColshape GetById(int id)
        {
            return All.Where(x => x?.Colshape?.Id == id).FirstOrDefault();
        }

        public static ExtraColshape GetByName(string name)
        {
            return All.Where(x => x.Name == name).FirstOrDefault();
        }

        /// <summary>Получить колшейп по айди (серверный)</summary>
        public static ExtraColshape GetByRemoteId(int id)
        {
            return All.Where(x => x?.Colshape?.RemoteId == id).FirstOrDefault();
        }

        /// <summary>Получить колшейп по его держателю</summary>
        public static ExtraColshape Get(Colshape colshape)
        {
            return colshape == null ? null : All.Where(x => x.Colshape == colshape).FirstOrDefault();
        }

        public static List<ExtraColshape> GetAllByName(string name)
        {
            return All.Where(x => x.Name == name).ToList();
        }

        public static Action<ExtraColshape> GetEnterAction(ActionTypes aType)
        {
            return _actions.GetValueOrDefault(aType)?.GetValueOrDefault(true);
        }

        public static Action<ExtraColshape> GetExitAction(ActionTypes aType)
        {
            return _actions.GetValueOrDefault(aType)?.GetValueOrDefault(false);
        }

        public static Action GetInteractionAction(InteractionTypes iType)
        {
            return _interactionActions.GetValueOrDefault(iType);
        }

        public abstract string ShortData { get; }

        public bool Exists => All.Contains(this);

        /// <summary>Сущность-держатель колшейпа, не имеет функциональности</summary>
        public Colshape Colshape { get; set; }

        /// <summary>Тип колшейпа</summary>
        public ColshapeTypes Type { get; set; }

        /// <summary>Тип действия при входе/выходе в колшейп</summary>
        public ActionTypes ActionType { get; set; }

        /// <summary>Тип действия для взаимодействия</summary>
        public InteractionTypes InteractionType { get; set; }

        /// <summary>Тип действия для проверки на возможность взаимодействия с колшейпом</summary>
        public ApproveTypes ApproveType { get; set; }

        /// <summary>Позиция</summary>
        public Vector3 Position { get; set; }

        /// <summary>Для взаимодействия ли колшейп?</summary>
        /// <remarks>
        ///     Если колшейп используется для взаимодействия, то ивенты OnEnter и OnExit будут срабатывать также в зависимости
        ///     от того, открыт ли какой либо интерфейс у игрока
        /// </remarks>
        public bool IsInteraction => InteractionType != InteractionTypes.None;

        /// <summary>Измерение</summary>
        /// <remarks>Если используется uint.MaxValue, то колшейп работает независимо от измерения игрока</remarks>
        public uint Dimension { get; set; }

        /// <summary>Цвет</summary>
        public Colour Colour { get; set; }

        /// <summary>Видимый ли?</summary>
        /// <remarks>
        ///     Если колшейп видимый, то его будут видеть все игроки, иначе - только администраторы, и то, при включенной
        ///     настройке на стороне клиента
        /// </remarks>
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
        public virtual void SetPosition(Vector3 position)
        {
            Position = position;
        }

        /// <summary>Метод для проверки, находится ли колшейп в зоне стрима для игрока</summary>
        public virtual bool IsStreamed()
        {
            return Colshape?.IsNull == false && (Dimension == uint.MaxValue || Player.LocalPlayer.Dimension == Dimension);
        }

        /// <summary>Данные колшейпа</summary>
        public object Data { get; set; }

        public Colshape.ColshapeEventDelegate OnEnter
        {
            get => Colshape?.OnEnter;
            set
            {
                if (Colshape?.IsNull != false) return;
                Colshape.OnEnter = value;
            }
        }

        public Colshape.ColshapeEventDelegate OnExit
        {
            get => Colshape?.OnExit;
            set
            {
                if (Colshape?.IsNull != false) return;
                Colshape.OnExit = value;
            }
        }

        public void Destroy()
        {
            if (!Exists)
                return;

            Streamed.Remove(this);

            if (Colshape != null)
            {
                if (IsInside)
                {
                    Colshape.SetData("PendingDeletion", true);

                    Events.OnPlayerExitColshape?.Invoke(Colshape, null);

                    All.Remove(this);
                }
                else
                {
                    All.Remove(this);
                }

                Colshape.ResetData();

                Colshape.Destroy();
            }
        }

        public ExtraColshape(ColshapeTypes Type,
                             bool IsVisible,
                             Colour Colour,
                             uint Dimension,
                             Colshape Colshape = null,
                             InteractionTypes InteractionType = InteractionTypes.None,
                             ActionTypes ActionType = ActionTypes.None)
        {
            this.Colshape = Colshape ?? new SphereColshape(Vector3.Zero, 0f, Settings.App.Static.StuffDimension);

            this.Type = Type;
            this.Colour = Colour;
            this.Dimension = Dimension;
            this.IsVisible = IsVisible;

            this.InteractionType = InteractionType;
            this.ActionType = ActionType;

            ApproveType = ApproveTypes.OnlyByFoot;

            All.Add(this);
        }

        public static void Render()
        {
            var pos = RAGE.Game.Cam.GetGameplayCamCoord();

            var list = Streamed.OrderBy(x => x.Position.DistanceTo(pos)).ToList();

            foreach (var x in Streamed.OrderBy(x => x.Position.DistanceTo(pos)))
            {
                if (Settings.User.Other.ColshapesVisible || x.IsVisible)
                    x.Draw();
            }
        }

        public static void Activate()
        {
            var streamUpdateTask = new AsyncTask(() => { UpdateInside(); }, 250, true, 0);

            streamUpdateTask.Run();

            var updateTask = new AsyncTask(() => { UpdateStreamed(); }, 1_000, true, 0);

            updateTask.Run();
        }

        public static void UpdateStreamed()
        {
            for (var i = 0; i < All.Count; i++)
            {
                var cs = All[i];

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
                        if (cs.IsInside)
                        {
                            cs.IsInside = false;

                            Events.OnPlayerExitColshape?.Invoke(cs.Colshape, null);
                        }
                }
            }
        }

        public static void UpdateInside()
        {
            var interactionAllowed = InteractionColshapesAllowed;

            if (InteractionColshapesDisabledThisFrame)
            {
                interactionAllowed = false;

                InteractionColshapesDisabledThisFrame = false;
            }

            var pos = Player.LocalPlayer.Vehicle is Vehicle veh ? veh.Position : Player.LocalPlayer.Position;

            Streamed.OrderByDescending(x => x.IsInside)
                    .ToList()
                    .ForEach(curPoly =>
                     {
                         if (curPoly.IsInside)
                         {
                             /*                    if (curPoly?.Colshape?.IsNull != false)
                                                 {
                                                     if (curPoly?.Colshape != null)
                                                         All.Remove(curPoly.Colshape);
         
                                                     continue;
                                                 }*/

                             if (curPoly.IsInteraction && !interactionAllowed || !curPoly.IsPointInside(pos) ||
                                 !(_approveFuncs.GetValueOrDefault(curPoly.ApproveType)?.Invoke() ?? true))
                             {
                                 curPoly.IsInside = false;

                                 Events.OnPlayerExitColshape?.Invoke(curPoly.Colshape, null);
                             }
                         }
                         else
                         {
                             if (curPoly.IsInteraction && !interactionAllowed || !(_approveFuncs.GetValueOrDefault(curPoly.ApproveType)?.Invoke() ?? true))
                                 return;

                             if (curPoly.IsPointInside(pos))
                             {
                                 curPoly.IsInside = true;

                                 Events.OnPlayerEnterColshape?.Invoke(curPoly.Colshape, null);
                             }
                         }
                     });
        }
    }
}