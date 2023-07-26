namespace BlaineRP.Client.Game.Items
{
    public abstract class StatusChanger : Item
    {
        public new class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight, int satiety = 0, int mood = 0, int health = 0) : base(name, weight)
            {
                Satiety = satiety;
                Mood = mood;
                Health = health;
            }

            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int Health { get; set; }
        }
    }
}