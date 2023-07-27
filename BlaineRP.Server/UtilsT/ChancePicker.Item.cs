namespace BlaineRP.Server.UtilsT
{
    public partial class ChancePicker<T>
    {
        public class Item<T>
        {
            public double Probability { get; set; }

            public T Value { get; set; }

            public Item(double Probability, T Value)
            {
                this.Probability = Probability;
                this.Value = Value;
            }
        }
    }
}