using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Game.Items.Craft
{
    public class Craft
    {
        public static List<Receipt> AllReceipts { get; private set; } = new List<Receipt>();

        public class ItemPrototype
        {
            public ItemPrototype(string Id, int Amount = 1)
            {
                this.Id = Id;
                this.Amount = Amount;
            }

            public string Id { get; private set; }

            public int Amount { get; private set; }
        }

        public class Receipt
        {
            public Receipt(ResultData CraftResultData, params ItemPrototype[] CraftNeededItems)
            {
                this.CraftNeededItems = CraftNeededItems.ToList();

                this.CraftResultData = CraftResultData;
            }

            public List<ItemPrototype> CraftNeededItems { get; private set; }

            public ResultData CraftResultData { get; private set; }

            public int Index => AllReceipts.IndexOf(this);

            public static Receipt GetByResultItemId(string resultItemId)
            {
                return AllReceipts.Where(x => x.CraftResultData.ResultItem.Id == resultItemId).FirstOrDefault();
            }

            public static Receipt GetByIngredients(List<ItemPrototype> items)
            {
                if (items.Count == 0)
                    return null;

                foreach (Receipt craftReceipt in AllReceipts)
                {
                    if (craftReceipt.CraftNeededItems.Count != items.Count)
                        continue;

                    var success = true;

                    for (var i = 0; i < items.Count; i++)
                    {
                        ItemPrototype curItem = items[i];
                        ItemPrototype curReceiptItem = craftReceipt.CraftNeededItems[i];

                        if (curReceiptItem.Id != curItem.Id || curItem.Amount < curReceiptItem.Amount)
                        {
                            success = false;

                            break;
                        }
                    }

                    if (success)
                        return craftReceipt;
                }

                return null;
            }

            public int GetExpectedAmountByIngredients(List<ItemPrototype> items)
            {
                if (items.Count != CraftNeededItems.Count)
                    return 0;

                int coef = int.MaxValue;

                for (var i = 0; i < items.Count; i++)
                {
                    if (CraftNeededItems[i].Amount <= 0)
                        continue;

                    int newCoef = items[i].Amount / CraftNeededItems[i].Amount;

                    if (newCoef < coef)
                        coef = newCoef;
                }

                if (coef == int.MaxValue)
                    coef = 0;

                return coef * CraftResultData.ResultItem.Amount;
            }
        }

        public class ResultData
        {
            public ResultData(string Id, int Amount = 1, int CraftTime = 0)
            {
                ResultItem = new ItemPrototype(Id, Amount);

                this.CraftTime = CraftTime;
            }

            public ItemPrototype ResultItem { get; private set; }

            public int CraftTime { get; private set; }
        }
    }
}