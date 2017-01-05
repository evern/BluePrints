using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data.Attributes
{
    public class BulkEditDisabledAttributes : Attribute
    {
        public BulkEditDisabledAttributes(string columns)
        {
            columns = columns.Replace(" ", string.Empty);
            ColumnNames = columns.Split(',');
        }

        public string[] ColumnNames { get; set; }
    }
}
