using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server
{
    public class ChancePicker<T>
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

        private List<KeyValuePair<double, List<Item<T>>>> dict { get; set; }

        public ChancePicker(params Item<T>[] Items)
        {
            this.dict = GetCorrectDict(Items.ToList());
        }

        public T GetNextItem(out double chance)
        {
            var randRes = SRandom.NextDoubleS();

            int resIdx0 = -1;

            for (int i = 0; i < dict.Count; i++)
            {
                var x = dict[i];

                if (x.Key < randRes)
                    continue;

                resIdx0 = i;

                break;
            }

            if (dict[resIdx0].Value.Count == 1)
            {
                chance = dict[resIdx0].Value[0].Probability;

                return dict[resIdx0].Value[0].Value;
            }

            var resItem = dict[resIdx0].Value[SRandom.NextInt32(0, dict[resIdx0].Value.Count)];

            chance = resItem.Probability;

            return resItem.Value;
        }

        public void AddItem(Item<T> item)
        {
            var allItems = dict.SelectMany(x => x.Value).ToList();

            allItems.Add(item);

            this.dict = GetCorrectDict(allItems);
        }

        public bool RemoveItem(Item<T> item)
        {
            var allItems = dict.SelectMany(x => x.Value).ToList();

            if (allItems.Remove(item))
            {
                this.dict = GetCorrectDict(allItems);

                return true;
            }

            return false;
        }

        private static List<KeyValuePair<double, List<Item<T>>>> GetCorrectDict(List<Item<T>> list)
        {
            var t = list.GroupBy(x => x.Probability).Select(x => new KeyValuePair<double, List<Item<T>>>(x.Key, x.ToList())).OrderByDescending(x => x.Key).ToList();

            var dict = new List<KeyValuePair<double, List<Item<T>>>>(t.Count);

            var sum = 0d;

            for (int i = t.Count - 1; i >= 0; i--)
            {
                var x = t[i];

                sum += x.Key;

                dict.Add(new KeyValuePair<double, List<Item<T>>>(sum, x.Value));
            }

            dict.Add(new KeyValuePair<double, List<Item<T>>>(1d, t[t.Count - 1].Value));

            return dict;
        }
    }
}
