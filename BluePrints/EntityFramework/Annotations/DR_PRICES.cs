namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DR_PRICES : EntityBase
    {
        [NotMapped]
        STOCK_ITEMS assignedSTOCK_ITEMS { get; set; }

        public void AssignSTOCK_ITEMS(STOCK_ITEMS STOCK_ITEMS)
        {
            assignedSTOCK_ITEMS = STOCK_ITEMS;
        }

        public IEnumerable<double> SellPrices
        {
            get
            {
                List<double> sellPrices = new List<double>();
                if (assignedSTOCK_ITEMS == null)
                    return sellPrices;

                if(assignedSTOCK_ITEMS.SELLPRICE1 != null && assignedSTOCK_ITEMS.SELLPRICE1 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE1);
                if (assignedSTOCK_ITEMS.SELLPRICE2 != null && assignedSTOCK_ITEMS.SELLPRICE2 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE2);
                if (assignedSTOCK_ITEMS.SELLPRICE3 != null && assignedSTOCK_ITEMS.SELLPRICE3 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE3);
                if (assignedSTOCK_ITEMS.SELLPRICE4 != null && assignedSTOCK_ITEMS.SELLPRICE4 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE4);
                if (assignedSTOCK_ITEMS.SELLPRICE5 != null && assignedSTOCK_ITEMS.SELLPRICE5 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE5);
                if (assignedSTOCK_ITEMS.SELLPRICE6 != null && assignedSTOCK_ITEMS.SELLPRICE6 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE6);
                if (assignedSTOCK_ITEMS.SELLPRICE7 != null && assignedSTOCK_ITEMS.SELLPRICE7 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE7);
                if (assignedSTOCK_ITEMS.SELLPRICE8 != null && assignedSTOCK_ITEMS.SELLPRICE8> 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE8);
                if (assignedSTOCK_ITEMS.SELLPRICE9 != null && assignedSTOCK_ITEMS.SELLPRICE9 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE9);
                if (assignedSTOCK_ITEMS.SELLPRICE10 != null && assignedSTOCK_ITEMS.SELLPRICE10 > 0)
                    sellPrices.Add((double)assignedSTOCK_ITEMS.SELLPRICE10);

                return sellPrices.OrderBy(x => x);
            }
        }
    }
}