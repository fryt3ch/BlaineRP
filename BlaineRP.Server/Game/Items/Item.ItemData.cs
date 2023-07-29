using System.Linq;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public abstract partial class Item
    {
        public abstract class ItemData
        {
            /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
            public interface IContainer
            {
                public byte MaxSlots { get; }

                public float MaxWeight { get; }
            }

            /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
            public interface IStackable
            {
                /// <summary>Максимальное кол-во единиц предмета в стаке</summary>
                public int MaxAmount { get; set; }
            }

            /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
            /// <remarks>Не использовать одновременно с IStackable!</remarks>
            public interface IConsumable
            {
                public int MaxAmount { get; set; }
            }

            /// <summary>Стандартная модель</summary>
            public static uint DefaultModel => NAPI.Util.GetHashKey("prop_drug_package_02");

            /// <summary>Название предмета</summary>
            public string Name { get; set; }

            /// <summary>Вес единицы предмета</summary>
            public float Weight { get; set; }

            /// <summary>Основная модель</summary>
            public uint Model { get => Models[0]; }

            /// <summary>Все модели</summary>
            private uint[] Models { get; set; }

            public abstract string ClientData { get; }

            public ItemData(string Name, float Weight, params uint[] Models)
            {
                this.Name = Name;

                this.Weight = Weight;

                this.Models = Models.Length > 0 ? Models : new uint[] { DefaultModel };
            }

            public ItemData(string Name, float Weight, params string[] Models) : this(Name, Weight, Models.Select(x => NAPI.Util.GetHashKey(x)).ToArray()) { }

            public uint GetModelAt(int idx) => idx < 0 || idx >= Models.Length ? Model : Models[idx];
        }
    }
}