using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.World
{
    public class ItemOnGround
    {
        public enum Types : byte
        {
            /// <summary>Стандартный тип предмета на земле</summary>
            /// <remarks>Автоматически удаляется с определенными условиями, может быть подобран кем угодно</remarks>
            Default = 0,

            /// <summary>
            ///     Тип предмета на земле, который был намеренно установлен игроком (предметы, наследующие вбстрактный класс
            ///     PlaceableItem)
            /// </summary>
            /// <remarks>
            ///     Предметы данного типа не удаляется автоматически, так же не могут быть подобраны кем угодно (пока действуют
            ///     определенные условия)
            /// </remarks>
            PlacedItem,
        }

        public static DateTime LastShowed;
        public static DateTime LastSent;

        private ItemOnGround(MapObject Object)
        {
            this.Object = Object;

            Name = Items.Core.GetName(Id);
        }

        public MapObject Object { get; set; }

        public Types Type => (Types)(byte)Object.GetSharedData<int>("IOG", 0);

        public int Amount => Object.GetSharedData<int>("A", 0);

        public string Id => Object.GetSharedData<string>("I", null);

        public uint Uid => Utils.Convert.ToUInt32(Object.GetSharedData<object>("U", 0));

        public bool IsLocked => Object.GetSharedData<bool>("L", false);

        public string Name { get; private set; }

        public static ItemOnGround GetItemOnGroundObject(MapObject obj)
        {
            if (obj == null)
                return null;

            if (Core.ItemsOnGround.Where(x => x.Object == obj).FirstOrDefault() is ItemOnGround existingIog)
                return existingIog;

            return new ItemOnGround(obj);
        }

        public async void TakeItem()
        {
            if (Utils.Misc.IsAnyCefActive(true))
                return;

            if (Player.LocalPlayer.IsInAnyVehicle(false))
                return;

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Cuffed, PlayerActions.Types.Frozen))
                return;

            if (Amount == 1)
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                Events.CallRemote("Inventory::Take", Uid, 1);

                LastSent = Core.ServerTime;
            }
            else
            {
                if (LastShowed.IsSpam(500, false, false))
                    return;

                LastShowed = Core.ServerTime;

                ItemOnGround iog = this;

                await ActionBox.ShowRange("ItemOnGroundTakeRange",
                    string.Format(Locale.Actions.Take, Name),
                    1,
                    Amount,
                    Amount,
                    1,
                    ActionBox.RangeSubTypes.Default,
                    ActionBox.DefaultBindAction,
                    (rType, amountD) =>
                    {
                        if (LastSent.IsSpam(500, false, true))
                            return;

                        int amount;

                        if (!amountD.IsNumberValid(0, int.MaxValue, out amount, true))
                            return;

                        ActionBox.Close(true);

                        if (iog?.Object?.Exists != true)
                            return;

                        if (Player.LocalPlayer.IsInAnyVehicle(false))
                            return;

                        if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Cuffed, PlayerActions.Types.Frozen))
                            return;

                        if (rType == ActionBox.ReplyTypes.OK)
                        {
                            Events.CallRemote("Inventory::Take", iog.Uid, amount);

                            LastSent = Core.ServerTime;
                        }
                    },
                    null
                );
            }
        }
    }
}