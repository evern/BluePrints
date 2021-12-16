using BaseModel.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class ConcurrentForecastJobHelper
    {
        protected string columnEntity = "Entity";
        protected string columnCompare = "CompareEntities";

        public DataTable GetDataTable(IEnumerable<ConcurrentForecastJob> concurrentForecastJobs, List<DateTime> alignedDataDateCollection)
        {
            //column definitions
            DataTable dataPointsTable = new DataTable();
            dataPointsTable.Columns.Add(columnEntity, typeof(ForecastJobSnapshot));
            dataPointsTable.Columns.Add(columnCompare, typeof(DataTable));

            foreach (DateTime alignedDataDate in alignedDataDateCollection)
            {
                string columnFieldName = alignedDataDate.Date.ToString(BluePrintsResources.ColumnDateFormat);
                dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
            }

            //row definitions
            foreach (ConcurrentForecastJob concurrentForecastJob in concurrentForecastJobs)
            {
                DataRow commodityRow = dataPointsTable.NewRow();
                ForecastJobSnapshot commodityJob = concurrentForecastJob.JobDetail.GetJob();
                commodityRow[columnEntity] = commodityJob;

                DataTable compareDataTable;
                DataRow compareP6CostsRemainingRow;
                DataRow compareP6UnitsRemainingRow;
                DataTable compareChildDataTable;
                DataRow compareChildP6CostsRemainingRow;
                DataRow compareChildP6UnitsRemainingRow;

                //initialise compare rows
                compareDataTable = dataPointsTable.Clone();
                compareDataTable.TableName = BluePrintsResources.ForecastCompareTableName;
                commodityRow[columnCompare] = compareDataTable;
                compareP6CostsRemainingRow = compareDataTable.NewRow();
                compareP6UnitsRemainingRow = compareDataTable.NewRow();
                compareDataTable.Rows.Add(compareP6UnitsRemainingRow);
                compareDataTable.Rows.Add(compareP6CostsRemainingRow);

                //initialise PO forecast rows
                foreach (ConcurrentForecastJobDetail uniquePOStockCodeDetail in concurrentForecastJob.UniquePOStockCodeDetails)
                {
                    DataRow comparePOForecastRow = compareDataTable.NewRow();
                    comparePOForecastRow[columnEntity] = uniquePOStockCodeDetail.GetJob();
                    compareDataTable.Rows.Add(comparePOForecastRow);
                }

                //initialise indirect forecast rows
                foreach (ConcurrentForecastJobDetail uniqueIndirectDetail in concurrentForecastJob.UniqueIndirectDetails)
                {
                    DataRow compareIndirectRemainingRow = compareDataTable.NewRow();
                    compareIndirectRemainingRow[columnEntity] = uniqueIndirectDetail.GetJob();
                    compareDataTable.Rows.Add(compareIndirectRemainingRow);
                }

                //initialise true compare rows
                compareChildDataTable = dataPointsTable.Clone();
                compareChildDataTable.TableName = BluePrintsResources.ForecastCompareChildTableName;
                compareChildP6CostsRemainingRow = compareChildDataTable.NewRow();
                compareChildP6UnitsRemainingRow = compareChildDataTable.NewRow();
                compareChildDataTable.Rows.Add(compareChildP6UnitsRemainingRow);
                compareChildDataTable.Rows.Add(compareChildP6CostsRemainingRow);

                //initialise uncommitted rows
                DataRow compareUncommittedRow = compareDataTable.NewRow();
                compareDataTable.Rows.Add(compareUncommittedRow);

                //assign view p6 rows
                compareP6CostsRemainingRow[columnEntity] = concurrentForecastJob.ViewP6CostsRemainingDetail.GetJob();
                compareP6UnitsRemainingRow[columnEntity] = concurrentForecastJob.ViewP6UnitsRemainingDetail.GetJob();

                //assign true p6 rows
                compareChildP6CostsRemainingRow[columnEntity] = concurrentForecastJob.TrueP6CostsRemainingDetail.GetJob();
                compareChildP6UnitsRemainingRow[columnEntity] = concurrentForecastJob.TrueP6UnitsRemainingDetail.GetJob();

                //assign uncommitted row
                compareUncommittedRow[columnEntity] = concurrentForecastJob.UncommittedDetail.GetJob();

                foreach (ForecastDateSnapshot dateCost in commodityJob.DateCosts)
                {
                    foreach (FORECAST_JOB_HOUR_SNAPSHOT poForecastSnapshot in dateCost.POForecastSnapshots)
                        concurrentForecastJob.SetPOStockCodeValue(poForecastSnapshot.STOCK_CODE, dateCost.QueryDate, poForecastSnapshot.FORECAST_COST);

                    foreach (FORECAST_JOB_HOUR_SNAPSHOT indirectForecastSnapshot in dateCost.IndirectForecastSnapshots)
                        concurrentForecastJob.SetIndirectStockCodeValue(indirectForecastSnapshot.STOCK_CODE, dateCost.QueryDate, indirectForecastSnapshot.FORECAST_COST);

                    //retrieve original p6 values
                    string dateColumnName = BluePrintsDataUtils.GetFormattedColumnName(dateCost.QueryDate);
                    compareChildP6CostsRemainingRow[dateColumnName] = dateCost.P6Costs;
                    compareChildP6UnitsRemainingRow[dateColumnName] = dateCost.P6Hours;
                }
            }

            return dataPointsTable;
        }
    }

    public class ConcurrentForecastJob
    {
        private readonly ForecastJobSnapshot job;
        public ConcurrentForecastJob(ForecastJobSnapshot job)
        {
            this.job = job;
            JobDetail = new ConcurrentForecastJobDetail(job);
            ViewP6UnitsRemainingDetail = new ConcurrentForecastJobDetail(new ForecastJobSnapshot() { DropDownPhase = "P6 Hours", CompareMask = "n2", ExoJob = job.ExoJob, DateCosts = job.DateCosts, IsP6HoursRow = true, P6RemainingUnits = job.P6RemainingUnits, P6RemainingCosts = job.P6RemainingCosts });
            ViewP6CostsRemainingDetail = new ConcurrentForecastJobDetail(new ForecastJobSnapshot() { DropDownPhase = "P6 $", CompareMask = "c0" });
            UncommittedDetail = new ConcurrentForecastJobDetail(new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_UncommittedRowPhase + " $", CompareMask = "c0" });

            UniquePOStockCodeDetails = new List<ConcurrentForecastJobDetail>();
            //add PO forecast rows on demand
            foreach (KeyValuePair<string, decimal> uniquePOStockCodeAttrbutes in job.POStockCodeAttributes)
            {
                ConcurrentForecastJobDetail uniquePOStockCodeDetail = new ConcurrentForecastJobDetail(new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_PORowPhase + " [" + uniquePOStockCodeAttrbutes.Key + "] $", CompareMask = "c0", DropDownIndirectBudget = uniquePOStockCodeAttrbutes.Value }, uniquePOStockCodeAttrbutes.Key);
                UniquePOStockCodeDetails.Add(uniquePOStockCodeDetail);
            }

            UniqueIndirectDetails = new List<ConcurrentForecastJobDetail>();
            //add indirect rows on demand
            foreach (KeyValuePair<string, decimal> uniqueIndirectStockCode in job.IndirectStockCodeAttributes)
            {
                ConcurrentForecastJobDetail uniqueIndirectDetail = new ConcurrentForecastJobDetail(new ForecastJobSnapshot() { DropDownPhase = BluePrintsResources.ForecastCompare_IndirectRowPhase + " [" + uniqueIndirectStockCode.Key + "] $", DropDownIndirectBudget = uniqueIndirectStockCode.Value, CompareMask = "c0" }, uniqueIndirectStockCode.Key);
                UniqueIndirectDetails.Add(uniqueIndirectDetail);
            }
        }

        public void SetJobValue(DateTime date, decimal value)
        {
            JobDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetViewP6UnitsValue(DateTime date, decimal value)
        {
            ViewP6UnitsRemainingDetail.SetDateValue(dateColumnName(date), value);
        }
        
        public void SetTrueP6UnitsValue(DateTime date, decimal value)
        {
            TrueP6UnitsRemainingDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetViewP6CostsValue(DateTime date, decimal value)
        {
            ViewP6CostsRemainingDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetTrueP6CostsValue(DateTime date, decimal value)
        {
            TrueP6CostsRemainingDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetUncommittedValue(DateTime date, decimal value)
        {
            UncommittedDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetPOStockCodeValue(string POStockCode, DateTime date, decimal value)
        {
            ConcurrentForecastJobDetail uniquePOStockCodeDetail = UniquePOStockCodeDetails.FirstOrDefault(x => x.GetKey() == POStockCode);
            if(uniquePOStockCodeDetail != null)
                uniquePOStockCodeDetail.SetDateValue(dateColumnName(date), value);
        }

        public void SetIndirectStockCodeValue(string POStockCode, DateTime date, decimal value)
        {
            ConcurrentForecastJobDetail uniqueIndirectDetail = UniqueIndirectDetails.FirstOrDefault(x => x.GetKey() == POStockCode);
            if (uniqueIndirectDetail != null)
                uniqueIndirectDetail.SetDateValue(dateColumnName(date), value);
        }

        private string dateColumnName(DateTime dateTime)
        {
            return BluePrintsDataUtils.GetFormattedColumnName(dateTime);
            //return dateTime.ToString(BluePrintsResources.ColumnDateFormat);
        }

        public ConcurrentForecastJobDetail JobDetail;
        public ConcurrentForecastJobDetail ViewP6CostsRemainingDetail;
        public ConcurrentForecastJobDetail ViewP6UnitsRemainingDetail;
        public ConcurrentForecastJobDetail TrueP6CostsRemainingDetail;
        public ConcurrentForecastJobDetail TrueP6UnitsRemainingDetail;
        public ConcurrentForecastJobDetail UncommittedDetail;
        public List<ConcurrentForecastJobDetail> UniquePOStockCodeDetails;
        public List<ConcurrentForecastJobDetail> UniqueIndirectDetails;
    }

    public class ConcurrentForecastJobDetail
    {
        private readonly ForecastJobSnapshot job;
        private readonly string SearchKey;
        public ConcurrentForecastJobDetail(ForecastJobSnapshot job, string key = "")
        {
            this.job = job;
            SearchKey = key;
        }

        public ForecastJobSnapshot GetJob()
        {
            return this.job;
        }

        public string GetKey()
        {
            return SearchKey;
        }

        public void SetDateValue(string dateStr, decimal val)
        {
            DateValues.AddOrUpdate(dateStr, val, (ty, v) => val);
        }

        public decimal? GetDateValue(string dateStr)
        {
            if (!DateValues.TryGetValue(dateStr, out decimal val))
                return null;

            return val;

        }

        public ConcurrentDictionary<string, decimal> DateValues { get; set; }

    }

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
