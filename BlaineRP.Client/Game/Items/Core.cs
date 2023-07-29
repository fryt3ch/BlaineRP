using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Items
{
    [Script(int.MaxValue)]
    public class Core
    {
        private static Dictionary<System.Type, string[]> AbstractImageTypes = new Dictionary<System.Type, string[]>() // string[] - exceptions
        {
            {
                typeof(Clothes), new string[]
                {
                }
            },
            {
                typeof(Bag), new string[]
                {
                }
            },
            {
                typeof(Holster), new string[]
                {
                }
            },
        };

        public Core()
        {
            var dict = new Dictionary<System.Type, Dictionary<string, Item.ItemData>>();

            #region TO_REPLACE

            #endregion

            foreach (KeyValuePair<System.Type, Dictionary<string, Item.ItemData>> x in dict)
            {
                AllData.Add(x.Key, x.Value);

                foreach (KeyValuePair<string, Item.ItemData> t in x.Value)
                {
                    string[] id = t.Key.Split('_');

                    if (!AllTypes.ContainsKey(id[0]))
                        AllTypes.Add(id[0], x.Key);
                }
            }
        }

        private static Dictionary<string, System.Type> AllTypes { get; set; } = new Dictionary<string, System.Type>();

        public static Dictionary<System.Type, Dictionary<string, Item.ItemData>> AllData { get; private set; } = new Dictionary<System.Type, Dictionary<string, Item.ItemData>>();

        private static List<KeyValuePair<System.Type, object[][]>> Actions { get; set; } = new List<KeyValuePair<System.Type, object[][]>>()
        {
            new KeyValuePair<System.Type, object[][]>(typeof(FishingRod),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.FishingRodUseBait,
                    },
                    new object[]
                    {
                        6,
                        Locale.General.Inventory.Actions.FishingRodUseWorms,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(PlaceableItem),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.SetupPlaceableItem,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(IUsable),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.Use,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(Weapon),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.Use,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(Food),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.Eat,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(StatusChanger),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.Use,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(IWearable),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.TakeOn,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(VehicleKey),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.FindVehicle,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(WeaponSkin),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.Use,
                    },
                }
            ),
            new KeyValuePair<System.Type, object[][]>(typeof(Note),
                new object[][]
                {
                    new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.NoteRead,
                    },
                    new object[]
                    {
                        6,
                        Locale.General.Inventory.Actions.NoteWrite,
                    },
                }
            ),
        };

        private static List<KeyValuePair<System.Type, List<string>>> ItemsActionsNotBag { get; set; } = new List<KeyValuePair<System.Type, List<string>>>()
        {
            new KeyValuePair<System.Type, List<string>>(typeof(IUsable),
                new List<string>
                {
                }
            ),
            new KeyValuePair<System.Type, List<string>>(typeof(PlaceableItem),
                new List<string>
                {
                }
            ),
        };

        private static List<KeyValuePair<System.Type, Func<List<string>>>> ItemsActionsValidators { get; set; } = new List<KeyValuePair<System.Type, Func<List<string>>>>()
        {
            new KeyValuePair<System.Type, Func<List<string>>>(typeof(FishingRod),
                () =>
                {
                    bool res = !PlayerActions.IsAnyActionActive(true, PlayerActions.Types.IsSwimming, PlayerActions.Types.Animation);

                    if (!res)
                    {
                        Notification.ShowError(Locale.Notifications.Inventory.ActionRestricted);

                        return null;
                    }

                    Vector3 waterPos = Raycast.FindEntityWaterIntersectionCoord(Player.LocalPlayer, new Vector3(0f, 0f, 1f), 7.5f, 7.5f, -3.5f, 360f, 0.5f, 31);

                    if (waterPos == null)
                    {
                        Notification.ShowError(Locale.Notifications.Inventory.FishingNotAllowedHere);

                        return null;
                    }

                    Player.LocalPlayer.SetData("MG::F::T::WZ", waterPos.Z);

                    var eData = new List<string>()
                    {
                    };

                    return eData;
                }
            ),
            new KeyValuePair<System.Type, Func<List<string>>>(typeof(Shovel),
                () =>
                {
                    bool res = !PlayerActions.IsAnyActionActive(true, PlayerActions.Types.IsSwimming);

                    if (!res)
                    {
                        Notification.ShowError(Locale.Notifications.Inventory.ActionRestricted);

                        return null;
                    }

                    Materials.Types materialType = Materials.GetTypeByRaycast(Player.LocalPlayer.Position + new Vector3(0f, 0f, 1f),
                        Management.Camera.Service.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 1f) + new Vector3(0f, 0f, -1.5f),
                        Player.LocalPlayer.Handle,
                        31
                    );

                    if (!Materials.CanTypeBeDug(materialType))
                    {
                        Notification.ShowError(Locale.Notifications.Inventory.DiggingNotAllowedHere);

                        return null;
                    }

                    var eData = new List<string>()
                    {
                    };

                    return eData;
                }
            ),
        };

        private static List<KeyValuePair<System.Type, Action<int, string>>> ItemsActionsPreActions { get; set; } = new List<KeyValuePair<System.Type, Action<int, string>>>()
        {
            new KeyValuePair<System.Type, Action<int, string>>(typeof(PlaceableItem),
                (slot, itemId) =>
                {
                    Inventory.Close(true);

                    StartPlaceItem(itemId, slot);
                }
            ),
        };

        public static string GetImageId(string id, System.Type type = null)
        {
            if (type == null)
            {
                type = GetType(id, false);

                if (type == null)
                    return "null";
            }

            System.Type aType = AbstractImageTypes.Where(x => (x.Key == type || x.Key.IsAssignableFrom(type)) && !x.Value.Contains(id)).Select(x => x.Key).FirstOrDefault();

            if (aType != null)
                return type.Name;

            return id;
        }

        public static System.Type GetType(string id, bool checkFullId = true)
        {
            if (id == null)
                return null;

            string[] data = id.Split('_');

            System.Type type = AllTypes.GetValueOrDefault(data[0]);

            if (type == null || checkFullId && !AllData[type].ContainsKey(id))
                return null;

            return type;
        }

        public static Item.ItemData GetData(string id, System.Type type = null)
        {
            if (type == null)
            {
                type = GetType(id, false);

                if (type == null)
                    return null;
            }

            return AllData[type].GetValueOrDefault(id);
        }

        public static string GetName(string id)
        {
            return GetData(id, null)?.Name ?? "null";
        }

        public static string GetNameWithTag(string id, System.Type iType, string tag, out string baseName)
        {
            baseName = GetName(id);

            if (tag == null || tag.Length == 0)
                return baseName;

            if (iType == null)
                iType = GetType(id, false);

            if (typeof(ITaggedFull).IsAssignableFrom(iType))
                return tag;

            return $"{baseName} [{tag}]";
        }

        public static object[][] GetActions(System.Type type,
                                            string id,
                                            int amount,
                                            bool isBag = false,
                                            bool inUse = false,
                                            bool hasContainer = false,
                                            bool isContainer = false,
                                            bool canSplit = true,
                                            bool canDrop = true)
        {
            var actions = new List<object[]>();

            if (inUse)
            {
                actions.Add(new object[]
                    {
                        5,
                        Locale.General.Inventory.Actions.StopUse,
                    }
                );
            }
            else
            {
                if (!isContainer)
                    if (!isBag || !ItemsActionsNotBag.Where(x => x.Key.IsTypeOrAssignable(type) && !x.Value.Contains(id)).Any())
                    {
                        object[][] action = Actions.Where(x => x.Key.IsTypeOrAssignable(type)).Select(x => x.Value).FirstOrDefault();

                        if (action != null)
                            actions.Add(action);
                    }
            }

            if (hasContainer)
                actions.Add(new object[]
                    {
                        4,
                        Locale.General.Inventory.Actions.Shift,
                    }
                );

            if (canSplit && amount > 1)
                actions.Add(new object[]
                    {
                        1,
                        Locale.General.Inventory.Actions.Split,
                    }
                );

            if (canDrop)
                actions.Add(new object[]
                    {
                        2,
                        Locale.General.Inventory.Actions.Drop,
                    }
                );

            return actions.ToArray();
        }

        public static Func<List<string>> GetActionToValidate(System.Type type)
        {
            return ItemsActionsValidators.Where(x => x.Key.IsTypeOrAssignable(type)).Select(x => x.Value).FirstOrDefault();
        }

        public static Action<int, string> GetActionToPreAction(System.Type type)
        {
            return ItemsActionsPreActions.Where(x => x.Key.IsTypeOrAssignable(type)).Select(x => x.Value).FirstOrDefault();
        }

        public static void StartPlaceItem(string itemId, int itemIdx)
        {
            var itemData = GetData(itemId, null) as PlaceableItem.ItemData;

            if (itemData == null)
                return;

            Vector3 coords = Management.Camera.Service.GetFrontOf(Player.LocalPlayer.Position, Player.LocalPlayer.GetHeading(), 2f);

            if (MapEditor.IsActive)
                return;

            MapObject mapObject = Streaming.CreateObjectNoOffsetImmediately(itemData.Model, coords.X, coords.Y, coords.Z);

            mapObject.SetTotallyInvincible(true);
            mapObject.SetCollision(false, false);

            mapObject.SetData("ItemIdx", itemIdx);

            MapEditor.Show(mapObject,
                "PlaceableItemEdit",
                new MapEditor.Mode(true, true, false, false, true, false),
                () =>
                {
                    Cursor.Show(true, true);

                    Notification.ShowHint("Поставьте предмет в желаемое место (так же, можете задать ему желаемый поворот)");
                },
                () => MapEditor.RenderPlaceItem(),
                () =>
                {
                    mapObject?.Destroy();

                    Cursor.Show(false, false);

                    Notification.ShowHint("Вы отменили установку предмета на землю!");
                },
                (pos, rot) =>
                {
                    OnPlaceItemFinish(mapObject, pos, rot);
                }
            );
        }

        public static void OnPlaceItemFinish(MapObject mObj, Vector3 pos, Vector3 rot)
        {
            if (mObj?.Exists != true)
            {
                mObj?.Destroy();

                Cursor.Show(false, false);

                MapEditor.Close(false);

                return;
            }

            mObj?.Destroy();

            float heading = rot?.Z ?? 0f;

            int itemIdx = mObj.HasData("ItemIdx") ? mObj.GetData<int>("ItemIdx") : -1;

            if (itemIdx < 0)
            {
                Cursor.Show(false, false);

                MapEditor.Close(false);

                return;
            }

            Cursor.Show(false, false);

            MapEditor.Close(false);

            Inventory.BindedAction(5, "pockets", itemIdx, pos.X.ToString(), pos.Y.ToString(), pos.Z.ToString(), heading.ToString());
        }
    }

    public interface ICraftIngredient
    {
    }

    public interface IConsumable
    {
    }

    public interface ITaggedFull : ITagged
    {
    }

    public interface ITagged
    {
    }

    public interface IContainer
    {
    }

    public interface IStackable
    {
    }

    public interface IWearable
    {
    }

    public interface IUsable
    {
    }
}