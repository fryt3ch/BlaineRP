using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items.Craft
{
    public partial class Craft
    {
        public class ItemPrototype
        {
            public string Id { get; private set; }

            public int Amount { get; private set; }

            public ItemPrototype(string Id, int Amount = 1)
            {
                this.Id = Id;
                this.Amount = Amount;
            }
        }

        public class Receipt
        {
            public List<ItemPrototype> CraftNeededItems { get; private set; }

            public ResultData CraftResultData { get; private set; }

            public Receipt(ResultData CraftResultData, params ItemPrototype[] CraftNeededItems)
            {
                this.CraftNeededItems = CraftNeededItems.OrderBy(x => x.Id).ToList();

                this.CraftResultData = CraftResultData;
            }

            public int GetExpectedAmountByIngredients(List<Item> items)
            {
                if (items.Count != CraftNeededItems.Count)
                    return 0;

                var coef = int.MaxValue;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].ID != CraftNeededItems[i].Id)
                        return 0;

                    if (CraftNeededItems[i].Amount <= 0)
                        continue;

                    var newCoef = Game.Items.Stuff.GetItemAmount(items[i]) / CraftNeededItems[i].Amount;

                    if (newCoef < coef)
                        coef = newCoef;
                }

                if (coef == int.MaxValue)
                    coef = 0;

                return coef * CraftResultData.ResultItem.Amount;
            }

            public static Receipt GetByIndex(int idx) => Craft.AllReceipts.ElementAtOrDefault(idx);

            public static Receipt GetByResultItemId(string resultItemId) => Craft.AllReceipts.Where(x => x.CraftResultData.ResultItem.Id == resultItemId).FirstOrDefault();

            public static Receipt GetByIngredients(List<Item> items)
            {
                if (items.Count == 0)
                    return null;

                foreach (var craftReceipt in Craft.AllReceipts)
                {
                    if (craftReceipt.CraftNeededItems.Count != items.Count)
                        continue;

                    var success = true;

                    for (int i = 0; i < items.Count; i++)
                    {
                        var curItem = items[i];
                        var curReceiptItem = craftReceipt.CraftNeededItems[i];

                        if (curReceiptItem.Id != curItem.ID || (curItem is IStackable curItemStackable && curItemStackable.Amount < curReceiptItem.Amount))
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
        }

        public class ResultData
        {
            public ItemPrototype ResultItem { get; private set; }

            public int CraftTime { get; private set; }

            public ResultData(string Id, int Amount = 1, int CraftTime = 0)
            {
                this.ResultItem = new ItemPrototype(Id, Amount);

                this.CraftTime = CraftTime;
            }
        }
    }
}
