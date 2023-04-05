﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

namespace BCRPClient.Data.Fractions
{
    public enum Types
    {
        None = 0,

        PolicePaleto,
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
        public List<string> UniformNames { get; set; }
    }

    public abstract class Fraction
    {
        public static string NoFractionStr = "Отсутствует";

        private static Dictionary<uint, string> PermissionNames = new Dictionary<uint, string>()
        {
            { 0, "Доступ к складу (даже если закрыт)" },
            { 1, "Доступ к созданию предметов (даже если закрыто)" },
            { 2, "Приглашать новых сотрудников" },
            { 3, "Повышать/понижать сотрудников в должности" },
            { 4, "Увольнять сотрудников сотрудников (ниже своего ранга)" },
            { 5, "Респавнить фракционный транспорт" },
            { 6, "Использовать чат фракции" },
            { 7, "Мут чата фракции сотрудников (ниже своего ранга) | /mutef, /unmutef" },

            { 8, "Использовать чат департамента" },

            { 9, "Заключать в Следственный изолятор" },
            { 10, "Выпускать из Следственного изолятора" },
            { 11, "Изменять время заключения (только для Следственного изолятора)" },

            { 12, "Заключать в Федеральную тюрьму" },

            { 13, "Штрафовать" },
            { 14, "Лишать лицензии (кроме адвокатской и на право владения бизнесом)" },
        };

        public static string GetFractionPermissionName(uint id) => PermissionNames.GetValueOrDefault(id);

        public static Dictionary<uint, MemberData> AllMembers { get; set; }

        public static NewsData NewsData { get; set; }

        public static Dictionary<uint, VehicleData> AllVehicles { get; set; }

        public static Dictionary<Types, Fraction> All { get; private set; } = new Dictionary<Types, Fraction>();

        public static Fraction Get(Types type) => All.GetValueOrDefault(type);

        public byte MaxRank { get; private set; }

        public uint LeaderCID => Sync.World.GetSharedData<object>($"FRAC::L_{(int)Type}", 0).ToUInt32();

        public uint Materials => Sync.World.GetSharedData<object>($"FRAC::M_{(int)Type}", 0).ToUInt32();

        public bool StorageLocked => Sync.World.GetSharedData<bool>($"FRAC::SL_{(int)Type}", false);
        public bool CreationWorkbenchLocked => Sync.World.GetSharedData<bool>($"FRAC::CWL_{(int)Type}", false);

        public TextLabel StorageTextInfo { get; set; }
        public TextLabel CreationWorkbenchTextInfo { get; set; }

        public Types Type { get; set; }

        public string Name { get; set; }

        public uint StorageContainerId { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public Fraction(Types Type, string Name, uint StorageContainerId, Utils.Vector4 ContainerPosition, Utils.Vector4 CreationWorkbenchPosition, byte MaxRank, Dictionary<string, uint> CreationWorkbenchPrices)
        {
            this.Type = Type;

            this.Name = Name;

            this.StorageContainerId = StorageContainerId;

            this.MaxRank = MaxRank;

            this.CreationWorkbenchPrices = CreationWorkbenchPrices;

            var containerCs = new Additional.Cylinder(ContainerPosition.Position, ContainerPosition.RotationZ, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
            {
                InteractionType = Additional.ExtraColshape.InteractionTypes.ContainerInteract,

                ActionType = Additional.ExtraColshape.ActionTypes.ContainerInteract,

                Data = StorageContainerId,
            };

            var containerTextLabel = new TextLabel(new Vector3(ContainerPosition.X, ContainerPosition.Y, ContainerPosition.Z + 1f), "Склад", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
            {
                Font = 0,
            };

            StorageTextInfo = new TextLabel(new Vector3(ContainerPosition.X, ContainerPosition.Y, ContainerPosition.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
            {
                Font = 0,
            };

            var creationWorkbenchCs = new Additional.Cylinder(CreationWorkbenchPosition.Position, CreationWorkbenchPosition.RotationZ, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
            {
                InteractionType = Additional.ExtraColshape.InteractionTypes.FractionCreationWorkbenchInteract,

                ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                Data = Type,
            };

            var creationWorkbenchTextLabel = new TextLabel(new Vector3(CreationWorkbenchPosition.X, CreationWorkbenchPosition.Y, CreationWorkbenchPosition.Z + 1f), "Создание предметов", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
            {
                Font = 0,
            };

            CreationWorkbenchTextInfo = new TextLabel(new Vector3(CreationWorkbenchPosition.X, CreationWorkbenchPosition.Y, CreationWorkbenchPosition.Z + 0.8f), "", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
            {
                Font = 0,
            };

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

            var amount = value.ToDecimal();

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
                fData.CreationWorkbenchTextInfo.Text = "[Закрыто]";

                fData.CreationWorkbenchTextInfo.Color = new RGBA(255, 0, 0, 255);
            }
            else
            {
                fData.CreationWorkbenchTextInfo.Text = "[Открыто]";

                fData.CreationWorkbenchTextInfo.Color = new RGBA(0, 255, 0, 255);
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
                fData.StorageTextInfo.Text = "[Закрыт]";

                fData.StorageTextInfo.Color = new RGBA(255, 0, 0, 255);
            }
            else
            {
                fData.StorageTextInfo.Text = "[Открыт]";

                fData.StorageTextInfo.Color = new RGBA(0, 255, 0, 255);
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

        public static void ShowFractionMenu()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var curFrac = pData.CurrentFraction;

            if (curFrac == null)
                return;

            CEF.FractionMenu.Show(curFrac.Type, NewsData, AllMembers, AllVehicles, 0, AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0);
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
        }

        public virtual void OnEndMembership()
        {
            if (CurrentData != null)
            {
                CurrentData.Clear();

                CurrentData = null;
            }

            CEF.FractionMenu.Close();

            if (CEF.MaterialWorkbench.CurrentType == CEF.MaterialWorkbench.Types.Fraction)
                CEF.MaterialWorkbench.Close();

            AllMembers = null;
            NewsData = null;
            AllVehicles = null;
        }

        public string GetRankName(byte rank) => Sync.World.GetSharedData<string>($"FRAC::RN_{(int)Type}_{rank}", "null");

        public static bool IsFractionPolice(Fraction fraction) => fraction is Police;

        public static bool IsFractionMedical(Fraction fraction) => false;

        public static bool IsFractionArmy(Fraction fraction) => false;

        public static bool IsFractionMassMedia(Fraction fraction) => false;

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
            var data = CurrentData.GetValueOrDefault(key);

            if (data is T dataT)
                return dataT;

            return default;
        }

        public bool ResetCurrentData(string key) => CurrentData.Remove(key);
    }

    public class FractionEvents : Events.Script
    {
        public FractionEvents()
        {
            Events.Add("Fraction::UMS", (args) =>
            {
                if (Fraction.AllMembers == null)
                    return;

                var cid = args[0].ToUInt32();

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

                var cid = args[0].ToUInt32();

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

                var cid = args[0].ToUInt32();

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

                var cid = args[0].ToUInt32();

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
                    var mData = new MemberData() { IsOnline = true, Name = (string)args[1], Rank = (byte)(int)args[2], SubStatus = (byte)(int)args[3], LastSeenDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(args[4])).DateTime };

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

                var vid = args[0].ToUInt32();
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
