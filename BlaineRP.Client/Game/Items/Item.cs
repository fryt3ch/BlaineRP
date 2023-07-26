namespace BlaineRP.Client.Game.Items
{
    public abstract class Item
    {
        public class ItemData
        {
            /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
            public interface IContainer
            {
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

            public interface ICraftIngredient
            {

            }

            public string Name { get; set; }

            public float Weight { get; set; }

            public ItemData(string Name, float Weight)
            {
                this.Name = Name;
                this.Weight = Weight;
            }
        }
    }
}