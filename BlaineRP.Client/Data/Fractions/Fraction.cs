using BlaineRP.Client.CEF;
using BlaineRP.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Data.Fractions
{
    public enum Types
    {
        None = 0,

        COP_BLAINE = 1,
        COP_LS = 2,

        FIB_LS = 5,

        MEDIA_LS = 10,

        EMS_BLAINE = 20,
        EMS_LS = 21,

        GOV_LS = 30,

        GANG_MARA = 40,
        GANG_FAMS = 41,
        GANG_BALS = 42,
        GANG_VAGS = 43,

        MAFIA_RUSSIA = 60,
        MAFIA_ITALY = 61,
        MAFIA_JAPAN = 62,

        ARMY_FZ = 80,

        PRISON_BB = 90,
    }

    [Flags]
    public enum MetaFlagTypes
    {
        None = 0,

        IsGov = 1 << 0,
        MembersHaveDocs = 1 << 1,
    }

    public class MemberData
    {
        public bool IsOnline { get; set; }

        public byte SubStatus { get; set; }

        public string Name { get; set; }

        public byte Rank { get; set; }

        public DateTime LastSeenDate { get; set; }
    }

    public class NewsData
    {
        [JsonProperty(PropertyName = "A")]
        public Dictionary<int, string> All { get; set; }

        [JsonProperty(PropertyName = "P")]
        public int PinnedId { get; set; }
    }

    public class VehicleData
    {
        public string Numberplate { get; set; }

        public byte MinRank { get; set; }
    }

    public interface IUniformable
    {
        public string[] UniformNames { get; set; }
    }

    public abstract class Fraction
    {
        public static string NoFractionStr = "Отсутствует";

        private static Dictionary<uint, string> PermissionNames = new Dictionary<uint, string>()
        {
            { 0, "Доступ к складу (даже если закрыт)" },
            { 1, "Доступ к созданию предметов (даже если закрыто)" },
            { 2, "Приглашать новых сотрудников" },
            { 3, "Понижать сотрудников в должности (ниже своего ранга)" },
            { 4, "Повышать сотрудников в должности (ниже своего ранга)" },
            { 5, "Увольнять сотрудников сотрудников (ниже своего ранга)" },
            { 6, "Респавнить фракционный транспорт" },
            { 7, "Использовать чат фракции | /f, /r" },
            { 8, "Мут чата фракции сотрудников (ниже своего ранга) | /mutef, /unmutef" },

            { 9, "Использовать чат департамента | /d" },

            { 10, "Заключать в Следственный изолятор" },
            { 11, "Выпускать из Следственного изолятора" },
            { 12, "Изменять время заключения (только для Следственного изолятора)" },

            { 13, "Заключать в Федеральную тюрьму" },

            { 14, "Выписывать штрафы" },
            { 15, "Лишать лицензии (кроме адвокатской и на право владения бизнесом)" },

            { 16, "Поиск граждан в Базе данных (Служебный планшет)" },
            { 17, "Добавлять и исполнять ориентировки (Служебный планшет)" },
            { 18, "Отправлять экстренные коды (Служебный планшет, коды 0-1-2)" },
            { 19, "Завершать активные вызовы (Служебный планшет, код 3)" },
            { 20, "Устанавливать GPS-трекеры на транспорт" },
            { 21, "Отключать активные GPS-трекеры на транспорте" },

            { 22, "Лечить" },
            { 23, "Лечить наркозависимость" },
            { 24, "Выдавать медицинские карты" },

            { 25, "Модерировать объявления" },
            { 26, "Выходить в эфир" },

            { 100, "Изымать предметы при обыске" },

            { 10_000, "Заправка транспорта за счёт фракции" },
        };

        public static string GetFractionPermissionName(uint id) => PermissionNames.GetValueOrDefault(id);

        public static Dictionary<uint, MemberData> AllMembers { get; set; }

        public static NewsData NewsData { get; set; }

        public static Dictionary<uint, VehicleData> AllVehicles { get; set; }

        public static Dictionary<Types, Fraction> All { get; private set; } = new Dictionary<Types, Fraction>();

        public static Fraction Get(Types type) => All.GetValueOrDefault(type);

        public byte MaxRank { get; private set; }

        public uint LeaderCID => Utils.Convert.ToUInt32(Sync.World.GetSharedData<object>($"FRAC::L_{(int)Type}", 0));

        public uint Materials => Utils.Convert.ToUInt32(Sync.World.GetSharedData<object>($"FRAC::M_{(int)Type}", 0));

        public bool StorageLocked => Sync.World.GetSharedData<bool>($"FRAC::SL_{(int)Type}", false);
        public bool CreationWorkbenchLocked => Sync.World.GetSharedData<bool>($"FRAC::CWL_{(int)Type}", false);

        public Additional.ExtraLabel[] StorageTextInfos { get; set; }
        public Additional.ExtraLabel[] CreationWorkbenchTextInfos { get; set; }

        public Types Type { get; set; }

        public string Name { get; set; }

        public uint StorageContainerId { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public MetaFlagTypes MetaFlags { get; set; }

        public Fraction(Types Type, string Name, uint StorageContainerId, string ContainerPositionsStr, string CreationWorkbenchPositionsStr, byte MaxRank, Dictionary<string, uint> CreationWorkbenchPrices, uint MetaFlags)
        {
            this.Type = Type;

            this.Name = Name;

            this.StorageContainerId = StorageContainerId;

            this.MaxRank = MaxRank;

            this.CreationWorkbenchPrices = CreationWorkbenchPrices;

            this.MetaFlags = (MetaFlagTypes)MetaFlags;

            var contPoses = RAGE.Util.Json.Deserialize<Utils.Vector4[]>(ContainerPositionsStr);
            var wbPoses = RAGE.Util.Json.Deserialize<Utils.Vector4[]>(CreationWorkbenchPositionsStr);

            var contTextInfos = new List<Additional.ExtraLabel>();
            var wbTextInfos = new List<Additional.ExtraLabel>();

            for (int i = 0; i < contPoses.Length; i++)
            {
                var contPos = contPoses[i];

                var containerCs = new Additional.Cylinder(contPos.Position, contPos.RotationZ, 2.5f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.ContainerInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.ContainerInteract,

                    Data = StorageContainerId,
                };

                var containerTextLabel = new Additional.ExtraLabel(new Vector3(contPos.X, contPos.Y, contPos.Z + 1f), "Склад", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                var containerInfoTextLabel = new Additional.ExtraLabel(new Vector3(contPos.X, contPos.Y, contPos.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                contTextInfos.Add(containerInfoTextLabel);
            }

            StorageTextInfos = contTextInfos.ToArray();

            for (int i = 0; i < wbPoses.Length; i++)
            {
                var wbPos = wbPoses[i];

                var creationWorkbenchCs = new Additional.Cylinder(wbPos.Position, wbPos.RotationZ, 2.5f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionCreationWorkbenchInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var creationWorkbenchTextLabel = new Additional.ExtraLabel(new Vector3(wbPos.X, wbPos.Y, wbPos.Z + 1f), "Создание предметов", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                var wbTextInfoLabel = new Additional.ExtraLabel(new Vector3(wbPos.X, wbPos.Y, wbPos.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };

                wbTextInfos.Add(wbTextInfoLabel);
            }

            CreationWorkbenchTextInfos = wbTextInfos.ToArray();

            All.Add(Type, this);

            for (int i = 0; i < MaxRank + 1; i++)
                Sync.World.AddDataHandler($"FRAC::RN_{(int)Type}_{i}", OnRankNameChanged);

            Sync.World.AddDataHandler($"FRAC::SL_{(int)Type}", OnStorageLockedChanged);
            Sync.World.AddDataHandler($"FRAC::CWL_{(int)Type}", OnCreationWorkbenchLockedChanged);
            Sync.World.AddDataHandler($"FRAC::M_{(int)Type}", OnMaterialsChanged);
        }

        private static void OnMaterialsChanged(string key, object value, object oldValue)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var kData = key.Split('_');

            var fType = (Types)int.Parse(kData[1]);

            if (pData.Fraction != fType)
                return;

            var amount = Utils.Convert.ToDecimal(value);

            if (CEF.MaterialWorkbench.CurrentType == CEF.MaterialWorkbench.Types.Fraction)
            {
                CEF.MaterialWorkbench.UpdateMateriaslBalance(amount);
            }
            else
            {
                CEF.FractionMenu.UpdateInfoLine(3, amount);
            }
        }

        public static void OnCreationWorkbenchLockedChanged(string key, object value, object oldValue)
        {
            var state = value as bool? ?? false;

            var kData = key.Split('_');

            var fType = (Types)int.Parse(kData[1]);

            var fData = Get(fType);

            if (state)
            {
                foreach (var x in fData.CreationWorkbenchTextInfos)
                {
                    x.Text = "[Закрыто]";

                    x.Color = new RGBA(255, 0, 0, 255);
                }
            }
            else
            {
                foreach (var x in fData.CreationWorkbenchTextInfos)
                {
                    x.Text = "[Открыто]";

                    x.Color = new RGBA(0, 255, 0, 255);
                }
            }

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.Fraction != fType)
                return;

            if (!state && CEF.MaterialWorkbench.CurrentType == CEF.MaterialWorkbench.Types.Fraction)
                CEF.MaterialWorkbench.Close();

            CEF.FractionMenu.SetLockButtonState("workbench", state);
        }

        public static void OnStorageLockedChanged(string key, object value, object oldValue)
        {
            var state = value as bool? ?? false;

            var kData = key.Split('_');

            var fType = (Types)int.Parse(kData[1]);

            var fData = Get(fType);

            if (state)
            {
                foreach (var x in fData.StorageTextInfos)
                {
                    x.Text = "[Закрыт]";

                    x.Color = new RGBA(255, 0, 0, 255);
                }
            }
            else
            {
                foreach (var x in fData.StorageTextInfos)
                {
                    x.Text = "[Открыт]";

                    x.Color = new RGBA(0, 255, 0, 255);
                }
            }

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.Fraction != fType)
                return;

            CEF.FractionMenu.SetLockButtonState("storage", state);
        }

        private static void OnRankNameChanged(string key, object value, object oldValue)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var kData = key.Split('_');

            var fType = (Types)int.Parse(kData[1]);

            if (pData.Fraction != fType)
                return;

            var rank = byte.Parse(kData[2]);

            CEF.FractionMenu.UpdateRankName((byte)(rank + 1), (string)value ?? "null");
        }

        public static async void ShowFractionMenu()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var curFrac = pData.CurrentFraction;

            if (curFrac == null)
                return;

            var fMenuServerData = ((string)await Events.CallRemoteProc("Fraction::GMSD"))?.Split('&');

            if (fMenuServerData == null)
                return;

            var balance = decimal.Parse(fMenuServerData[0]);

            CEF.FractionMenu.Show(curFrac.Type, NewsData, AllMembers, AllVehicles, balance, AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0);
        }

        public virtual void OnStartMembership(params object[] args)
        {
            CurrentData = new Dictionary<string, object>();

            NewsData = RAGE.Util.Json.Deserialize<NewsData>((string)args[0]);

            AllVehicles = new Dictionary<uint, VehicleData>();

            AllMembers = new Dictionary<uint, MemberData>();

            foreach (var x in ((JArray)args[1]).ToObject<List<string>>())
            {
                var data = x.Split('&');

                AllVehicles.Add(uint.Parse(data[0]), new VehicleData() { Numberplate = data[1], MinRank = byte.Parse(data[2]) });
            }

            foreach (var x in ((JArray)args[2]).ToObject<List<string>>())
            {
                var data = x.Split('&');

                AllMembers.Add(uint.Parse(data[0]), new MemberData() { Name = data[1], Rank = byte.Parse(data[2]), IsOnline = data[3] == "1", SubStatus = byte.Parse(data[4]), LastSeenDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(data[5])).DateTime });
            }

            CEF.HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Fraction_Menu);

            CEF.Menu.SetFraction(Type);

            CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "fraction_invite", (entity) => { var player = entity as Player; if (player == null) return; PlayerInvite(player); });
            CEF.Interaction.CharacterInteractionInfo.AddAction("documents", "fraction_docs", (entity) => { var player = entity as Player; if (player == null) return; PlayerShowDocs(player); });
        }

        public virtual void OnEndMembership()
        {
            if (CurrentData != null)
            {
                CurrentData.Clear();

                CurrentData = null;
            }

            CEF.HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Fraction_Menu);

            CEF.Menu.SetFraction(Data.Fractions.Types.None);

            CEF.FractionMenu.Close();

            CEF.Interaction.CloseMenu();

            if (CEF.MaterialWorkbench.CurrentType == CEF.MaterialWorkbench.Types.Fraction)
                CEF.MaterialWorkbench.Close();

            AllMembers = null;
            NewsData = null;
            AllVehicles = null;
        }

        public string GetRankName(byte rank) => Sync.World.GetSharedData<string>($"FRAC::RN_{(int)Type}_{rank}", "null");

        private Dictionary<string, object> CurrentData { get; set; }

        public void SetCurrentData(string key, object data)
        {
            if (CurrentData == null)
                return;

            if (!CurrentData.TryAdd(key, data))
                CurrentData[key] = data;
        }

        public T GetCurrentData<T>(string key)
        {
            var data = CurrentData?.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default;
        }

        public bool ResetCurrentData(string key) => CurrentData.Remove(key);

        public void PlayerInvite(Player player)
        {
            var tData = Sync.Players.GetData(player);

            if (tData == null)
                return;

            if (tData.Fraction != Types.None)
            {
                if (tData.Fraction == Type)
                {
                    CEF.Notification.ShowError(Locale.Get("FRACTION_INV_E_1"));
                }
                else
                {
                    CEF.Notification.ShowError(Locale.Get("FRACTION_INV_E_0"));
                }

                return;
            }

            Sync.Offers.Request(player, Sync.Offers.Types.InviteFraction, null);
        }

        public void PlayerShowDocs(Player player)
        {
            if (!MetaFlags.HasFlag(MetaFlagTypes.MembersHaveDocs))
                return;

            Sync.Offers.Request(player, Sync.Offers.Types.ShowFractionDocs, null);
        }
    }

    [Script(int.MaxValue)]
    public class FractionEvents
    {
        public FractionEvents()
        {
            Events.Add("Player::SCF", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (args == null || args.Length < 1)
                {
                    var lastFraction = pData.CurrentFraction;

                    if (lastFraction != null)
                    {
                        lastFraction.OnEndMembership();

                        pData.CurrentFraction = null;
                    }
                }
                else
                {
                    var fraction = (Data.Fractions.Types)(int)args[0];

                    var fData = Data.Fractions.Fraction.Get(fraction);

                    var lastFraction = pData.CurrentFraction;

                    if (lastFraction != null)
                    {
                        lastFraction.OnEndMembership();
                    }

                    pData.CurrentFraction = fData;

                    fData.OnStartMembership(args.Skip(1).ToArray());
                }
            });

            Events.Add("Fraction::UMS", (args) =>
            {
                if (Fraction.AllMembers == null)
                    return;

                var cid = Utils.Convert.ToUInt32(args[0]);

                var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                if (mData == null)
                    return;

                mData.SubStatus = (byte)(int)args[1];

                CEF.FractionMenu.UpdateMember(cid, "status", mData.SubStatus);
            });

            Events.Add("Fraction::UMO", (args) =>
            {
                if (Fraction.AllMembers == null)
                    return;

                var cid = Utils.Convert.ToUInt32(args[0]);

                var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                if (mData == null)
                    return;

                mData.IsOnline = (bool)args[1];

                if (mData.IsOnline)
                    mData.LastSeenDate = Sync.World.ServerTime;

                CEF.FractionMenu.UpdateMember(cid, "circle", mData.Rank + 1);

                if (args.Length > 2)
                {
                    mData.SubStatus = (byte)(int)args[2];

                    CEF.FractionMenu.UpdateMember(cid, "status", mData.SubStatus);
                }
            });

            Events.Add("Fraction::UMR", (args) =>
            {
                if (Fraction.AllMembers == null)
                    return;

                var cid = Utils.Convert.ToUInt32(args[0]);

                var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                if (mData == null)
                    return;

                mData.Rank = (byte)(int)args[1];

                CEF.FractionMenu.UpdateMember(cid, "pos", mData.Rank + 1);
            });

            Events.Add("Fraction::UM", (args) =>
            {
                if (Fraction.AllMembers == null)
                    return;

                var cid = Utils.Convert.ToUInt32(args[0]);

                if (args.Length == 1)
                {
                    var mData = Fraction.AllMembers.GetValueOrDefault(cid);

                    if (Fraction.AllMembers.Remove(cid))
                    {
                        CEF.FractionMenu.RemoveMember(cid);
                    }
                }
                else
                {
                    var mData = new MemberData() { IsOnline = true, Name = (string)args[1], Rank = (byte)(int)args[2], SubStatus = (byte)(int)args[3], LastSeenDate = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[4])).DateTime };

                    if (Fraction.AllMembers.TryAdd(cid, mData))
                    {
                        CEF.FractionMenu.AddMember(cid, mData);
                    }
                }
            });

            Events.Add("Fraction::UVEHMR", (args) =>
            {
                if (Fraction.AllVehicles == null)
                    return;

                var vid = Utils.Convert.ToUInt32(args[0]);
                var newMinRank = (byte)(int)args[1];

                var vData = Fraction.AllVehicles.GetValueOrDefault(vid);

                if (vData == null)
                    return;

                vData.MinRank = newMinRank;

                CEF.FractionMenu.UpdateVehicleInfo(vid, "access", newMinRank + 1);
            });

            Events.Add("Fraction::NEWSC", (args) =>
            {
                if (Fraction.NewsData == null)
                    return;

                var idx = (int)args[0];

                if (args.Length > 1)
                {
                    var text = (string)args[1];

                    if (Fraction.NewsData.All.TryAdd(idx, text))
                    {
                        CEF.FractionMenu.AddNews(idx, text);
                    }
                    else
                    {
                        Fraction.NewsData.All[idx] = text;

                        CEF.FractionMenu.UpdateNews(idx, text);
                    }
                }
                else
                {
                    if (Fraction.NewsData.All.Remove(idx))
                    {
                        if (Fraction.NewsData.PinnedId == idx)
                            Fraction.NewsData.PinnedId = -1;

                        CEF.FractionMenu.DeleteNews(idx);
                    }
                }
            });

            Events.Add("Fraction::NEWSP", (args) =>
            {
                if (Fraction.NewsData == null)
                    return;

                var idx = (int)args[0];

                if (idx < 0)
                {
                    if (Fraction.NewsData.PinnedId < 0)
                        return;

                    Fraction.NewsData.PinnedId = -1;

                    CEF.FractionMenu.PinNews(-1);
                }
                else
                {
                    if (!Fraction.NewsData.All.ContainsKey(idx))
                        return;


                    Fraction.NewsData.PinnedId = idx;

                    CEF.FractionMenu.PinNews(idx);
                }
            });
        }
    }
}
