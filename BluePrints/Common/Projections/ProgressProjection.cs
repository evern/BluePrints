using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class ProgressDisplay : IGuidEntityKey
    {
        public Guid EntityKey { get => ProgressItem.EntityKey; set => ProgressItem.EntityKey = value; }
        public DisplayReportable ProgressItem { get; set; }

        //Need to use IHaveStockCode or else view cannot display fields
        public IEnumerable<DisplayReportable> Reportables
        {
            get
            {
                GroupDisplayReportable reportable = ProgressItem as GroupDisplayReportable;
                if (reportable != null)
                    return reportable.ChildDeliverables;

                return null;
            }
        }

        public bool IsExpandable
        {
            get
            {
                GroupDisplayReportable reportable = ProgressItem as GroupDisplayReportable;
                return reportable != null;
            }
        }
    }

    public class Stock_CodeProgress : BluePrintsProgressableProjectionBase<STOCK_CODEProjection>, IReportableGroup
    {
        public List<IReportable> Reportables { get => Entity.Reportables; set => Entity.Reportables = value; }

        public decimal Estimated_Quantity => Reportables.Sum(x => x.Estimated_Quantity);

        public decimal Total_Quantity => Reportables.Sum(x => x.Total_Quantity);

        public decimal ItemRate => Reportables.Sum(x => x.ItemRate);

        public decimal EstimatedCosts => Reportables.Sum(x => x.EstimatedCosts);

        public decimal TotalCosts => Reportables.Sum(x => x.TotalCosts);

        public string UOM => Entity.UOM;

        public string ReportableItem_Name => string.Empty;

        public string Commodity_Code => string.Empty;

        public Guid? Workpack_Guid => null;

        public Guid OriginalEntityKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class Estimation_Direct_ItemProgress : BluePrintsProgressableProjectionBase<ESTIMATION_DIRECT_ITEMProjection>, IReportable
    {
        public decimal ItemRate => Entity.ItemRate;

        public decimal EstimatedCosts => Entity.EstimatedCosts;

        public decimal TotalCosts => Entity.TotalCosts;

        public string ReportableItem_Name => Entity.ReportableItem_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public Guid OriginalEntityKey { get => Entity.OriginalEntityKey; set => Entity.OriginalEntityKey = value; }

        public decimal Estimated_Quantity => Entity.Estimated_Quantity;

        public decimal Total_Quantity => Entity.Total_Quantity;

        public string UOM => Entity.UOM;
    }

    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IProgressProjection
        where TEntity : class, IDisplayDeliverable, IHaveQuantity, IHaveStats, IHaveProgresses, ICanUpdate, new()
    {
        public void SetPROGRESS_ITEMS(IEnumerable<PROGRESS_ITEM> progress_items)
        {
            Entity.PROGRESS_ITEMS = progress_items.ToList();
            IReportableGroup reportableGroup = Entity as IReportableGroup;
            if (reportableGroup != null && reportableGroup.Reportables != null)
            {
                reportableGroup.Reportables.ForEach(x => x.PROGRESS_ITEMS = progress_items.Where(z => z.GUID_ORIBASEITEM == x.OriginalEntityKey).ToList());
            }
        }

        public void SetSavedCurrentPROGRESS_ITEM(PROGRESS_ITEM progress_item)
        {
            if (progress_item.EARNED_DATE != Entity.ReportingDataDate || progress_item.GUID == Guid.Empty)
                return;

            PROGRESS_ITEM currentPROGRESS_ITEM = PROGRESS_ITEM_Current;
            if (currentPROGRESS_ITEM == null)
            {
                Entity.PROGRESS_ITEMS.Add(progress_item);
                Entity.Update();
            }

            IReportableGroup reportableGroup = Entity as IReportableGroup;
            if (reportableGroup != null && reportableGroup.Reportables != null)
            {
                IReportable findReportable = reportableGroup.Reportables.FirstOrDefault(x => x.OriginalEntityKey == progress_item.GUID_ORIBASEITEM);
                if (findReportable != null)
                {
                    findReportable.PROGRESS_ITEMS.Add(progress_item);
                    findReportable.Update();
                }
            }
        }

        public void SetReportingDataDate(DateTime reportingDataDate)
        {
            Entity.ReportingDataDate = reportingDataDate;
            IReportableGroup reportableGroup = Entity as IReportableGroup;
            if(reportableGroup != null && reportableGroup.Reportables != null)
            {
                reportableGroup.Reportables.ForEach(x => x.ReportingDataDate = reportingDataDate);
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                return Entity.PROGRESS_ITEMS.Where(y => y.EARNED_DATE < Entity.ReportingDataDate);
            }
        }

        public PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                IReportable reportableItem = Entity as IReportable;
                if(reportableItem != null)
                    return Entity.PROGRESS_ITEMS.FirstOrDefault(y => y.GUID_ORIBASEITEM == reportableItem.OriginalEntityKey && y.EARNED_DATE == Entity.ReportingDataDate);

                return null;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                return Entity.PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= Entity.ReportingDataDate);
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                return Entity.PROGRESS_ITEMS.Where(y => y.EARNED_DATE > Entity.ReportingDataDate);
            }
        }

        public DateTime ReportingDataDate
        {
            get { return Entity.ReportingDataDate; }
            set { Entity.ReportingDataDate = value; }
        }

        public List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Entity.PROGRESS_ITEMS; }
            set { Entity.PROGRESS_ITEMS = value; }
        }

        public ProgressStats Stats
        {
            get { return Entity.Stats; }
            set { Entity.Stats = value; }
        }

        public string Stock_Code
        {
            get { return Entity.Stock_Code; }
        }

        public decimal TotalHoursIncludeByDuration => Entity.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => Entity.EstimatedHours;

        public decimal TotalHours => Entity.TotalHours;

        public Guid? Area_Guid
        {
            get { return Entity.Area_Guid; }
        }

        public Guid? SubArea_Guid
        {
            get { return Entity.SubArea_Guid; }
        }
    }

    public static class ProgressItemQueries
    {
        public static IQueryable<ProgressDisplay> SiteDirectProgressItemTransformation(
            IQueryable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<ESTIMATION_DIRECT_ITEM> projectESTIMATION_DIRECT_ITEMS, IEnumerable<RATE> projectRATES, IEnumerable<COMMODITY_CODE> projectCOMMODITY_CODES, DateTime reportingDataDate)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ProgressDisplay> progressItems = new List<ProgressDisplay>();
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });
            
            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> ESTIMATION_DIRECT_ITEMProjection = 
                ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(projectESTIMATION_DIRECT_ITEMS.AsQueryable(), 
                                                                                                projectRATES, 
                                                                                                projectCOMMODITY_CODES, 
                                                                                                projectSTOCK_CODES).AsEnumerable();

            var trackableESTIMATION_DIRECT_ITEMProjectionGroupByStockCode = ESTIMATION_DIRECT_ITEMProjection.Where(x => !x.Entity.STANDALONE)
                .GroupBy(x => x.Entity.GUID_STOCK_CODE).Select(group => new { StockCodeGuid = group.Key, Estimation_Direct_ItemProjection = group.ToList() });

            var standaloneESTIMATION_DIRECT_ITEMProjection = ESTIMATION_DIRECT_ITEMProjection.Where(x => x.Entity.STANDALONE).OrderBy(x => x.Stock_Code).ThenBy(x => x.Commodity_Code);

            foreach (STOCK_CODE STOCK_CODE in projectSTOCK_CODES)
            {
                Stock_CodeProgress newStock_CodeProgress = new Stock_CodeProgress();
                newStock_CodeProgress.Entity.Entity = STOCK_CODE;
                var currentStockCodeReportables = trackableESTIMATION_DIRECT_ITEMProjectionGroupByStockCode.FirstOrDefault(x => x.StockCodeGuid == STOCK_CODE.GUID);
                if(currentStockCodeReportables != null)
                {
                    newStock_CodeProgress.Reportables = currentStockCodeReportables.Estimation_Direct_ItemProjection.Select(x => (IReportable)x).ToList();
                    newStock_CodeProgress.SetReportingDataDate(reportingDataDate);
                    newStock_CodeProgress.Reportables.ForEach(x => SetReportablePROGRESS_ITEM(x, PROGRESS_ITEMSByOriginalGuid));

                    ProgressDisplay newProgressDisplay = new ProgressDisplay();
                    newProgressDisplay.ProgressItem = new GroupDisplayReportable(newStock_CodeProgress);
                    progressItems.Add(newProgressDisplay);
                }
            }

            foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in standaloneESTIMATION_DIRECT_ITEMProjection)
            {
                Estimation_Direct_ItemProgress newEstimation_Direct_itemProgress = new Estimation_Direct_ItemProgress();
                newEstimation_Direct_itemProgress.Entity = ESTIMATION_DIRECT_ITEM;
                newEstimation_Direct_itemProgress.SetReportingDataDate(reportingDataDate);

                ProgressDisplay newProgressDisplay = new ProgressDisplay();
                newProgressDisplay.ProgressItem = new DisplayReportable(newEstimation_Direct_itemProgress);
                progressItems.Add(newProgressDisplay);
            }

            return progressItems.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IQueryable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            foreach(dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                if (item.OriginalGuid == reportable.OriginalEntityKey)
                {
                    reportable.PROGRESS_ITEMS = item.Progresses;
                    break;
                }
            }
        }
    }
}