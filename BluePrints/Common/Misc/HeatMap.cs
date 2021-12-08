using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    /// <summary>
    /// A thread safe data table
    /// </summary>
    /// <typeparam name="TX">The X axis type</typeparam>
    /// <typeparam name="TY">The Y axis type</typeparam>
    /// <typeparam name="TZ">The value type</typeparam>
    public class HeatMap<TX, TY, TZ>
    {
        public ConcurrentDictionary<TX, ConcurrentDictionary<TY, TZ>> Table { get; set; } = new ConcurrentDictionary<TX, ConcurrentDictionary<TY, TZ>>();

        public void SetValue(TX x, TY y, TZ val)
        {
            var row = Table.GetOrAdd(x, u => new ConcurrentDictionary<TY, TZ>());

            row.AddOrUpdate(y, v => val,
                (ty, v) => val);
        }

        public TZ GetValue(TX x, TY y)
        {
            var row = Table.GetOrAdd(x, u => new ConcurrentDictionary<TY, TZ>());

            if (!row.TryGetValue(y, out TZ val))
                return default;

            return val;

        }

        public DataTable GetDataTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add("");

            var columnList = new List<string>();
            foreach (var row in Table)
            {
                foreach (var valueKey in row.Value.Keys)
                {
                    var columnName = valueKey.ToString();
                    if (!columnList.Contains(columnName))
                        columnList.Add(columnName);
                }
            }

            foreach (var s in columnList)
                dataTable.Columns.Add(s);

            foreach (var row in Table)
            {
                var dataRow = dataTable.NewRow();
                dataRow[0] = row.Key.ToString();
                foreach (var column in row.Value)
                {
                    dataRow[column.Key.ToString()] = column.Value;
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    }
}
