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
    public class ProgressDisplay : IGuidEntityKey, ICanUpdate
    {
        public Guid GUID
        {
            get { return ProgressItem.EntityKey; }
            set { ProgressItem.EntityKey = value; }
        }

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

        public void Update()
        {
            ProgressItem.Update();
        }
    }

    public class GroupDisplayReportable : DisplayReportable
    {
        public IEnumerable<DisplayReportable> ChildDeliverables;
        public GroupDisplayReportable(IReportableGroup reportableGroup)
            : base(reportableGroup)
        {
            this.ChildDeliverables = reportableGroup.Deliverables.Select(x => new DisplayReportable(x));
        }
    }

    public class DisplayReportable : IReportable, ICanUpdate
    {
        readonly IReportable deliverable;

        //For bindableBase property name usage only
        public DisplayReportable()
        {

        }

        public DisplayReportable(IReportable deliverable)
        {
            this.deliverable = deliverable;
        }

        public string ReportableItem_Name
        {
            get
            {
                IBasicDeliverable basicDeliverable = deliverable as IBasicDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.ReportableItem_Name;

                return string.Empty;
            }
        }

        public string Commodity_Code
        {
            get
            {
                IBasicDeliverable basicDeliverable = deliverable as IBasicDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.Commodity_Code;

                return string.Empty;
            }
        }

        public Guid? Workpack_Guid
        {
            get
            {
                IBasicDeliverable basicDeliverable = deliverable as IBasicDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.Workpack_Guid;

                return null;
            }
        }

        public Guid OriginalEntityKey
        {
            get
            {
                IBasicDeliverable basicDeliverable = deliverable as IBasicDeliverable;
                if (basicDeliverable != null)
                    return basicDeliverable.OriginalEntityKey;

                throw new NotImplementedException();
            }
            set
            {
                IBasicDeliverable basicDeliverable = deliverable as IBasicDeliverable;
                if (basicDeliverable != null)
                {
                    basicDeliverable.OriginalEntityKey = value;
                    return;
                }


                throw new NotImplementedException();
            }
        }

        public string Stock_Code => deliverable.Stock_Code;

        public Guid? Area_Guid => deliverable.Area_Guid;

        public Guid? SubArea_Guid => deliverable.SubArea_Guid;

        public decimal TotalHoursIncludeByDuration => deliverable.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => deliverable.EstimatedHours;

        public decimal TotalHours => deliverable.TotalHours;

        public decimal ItemRate => deliverable.ItemRate;

        public decimal EstimatedCosts => deliverable.EstimatedCosts;

        public decimal TotalCosts => deliverable.TotalCosts;

        public decimal Estimated_Quantity
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Estimated_Quantity;

                return 0;
            }
        }

        public decimal Total_Quantity
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.Total_Quantity;

                return 0;
            }
        }

        public string UOM
        {
            get
            {
                IHaveQuantity quantityDeliverable = deliverable as IHaveQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.UOM;

                return string.Empty;
            }
        }

        public Guid EntityKey { get => deliverable.EntityKey; set => deliverable.EntityKey = value; }

        public decimal QuantityPerHour
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.QuantityPerHour;

                return 0;
            }
        }

        public decimal TotalPercentage
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.TotalPercentage;

                return 0;
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                if (quantityDeliverable != null)
                    return quantityDeliverable.PastInstalledQuantity;

                return 0;
            }
        }

        decimal? currentTotalInstalledQuantity { get; set; }
        public decimal CurrentTotalInstalledQuantity
        {
            get
            {
                if(currentTotalInstalledQuantity == null)
                {
                    ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
                    if (quantityDeliverable != null)
                        currentTotalInstalledQuantity = quantityDeliverable.CurrentTotalInstalledQuantity;
                    else
                        currentTotalInstalledQuantity = 0;
                }

                return (decimal)currentTotalInstalledQuantity;
            }
            set
            {
                currentTotalInstalledQuantity = value;
            }
        }

        public DateTime ReportingDataDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PROGRESS_ITEM> PROGRESS_ITEMS { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate => deliverable.PROGRESS_ITEM_BeforeDataDate;

        public PROGRESS_ITEM PROGRESS_ITEM_Current => deliverable.PROGRESS_ITEM_Current;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate => deliverable.PROGRESS_ITEM_UpToCurrentDataDate;

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate => deliverable.PROGRESS_ITEM_AfterDataDate;

        public ProgressStats Stats { get => deliverable.Stats; set => deliverable.Stats = value; }

        public decimal GetCurrentPeriodHours(decimal newPeriodPercentage)
        {
            ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodHours(newPeriodPercentage);

            return 0;
        }

        public decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            ICanProgressByQuantity quantityDeliverable = deliverable as ICanProgressByQuantity;
            if (quantityDeliverable != null)
                return quantityDeliverable.GetCurrentPeriodPercentage(newTotalQuantity);

            return 0;
        }

        public void Update()
        {
            deliverable.Update();
        }
    }

    public class Stock_CodeProgress : BluePrintsProgressableByQuantityProjectionBase<STOCK_CODEProjection>, IReportableGroup
    {
        public IEnumerable<IQuantityReportable> Deliverables { get; set; }

        public override List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Deliverables.SelectMany(x => x.PROGRESS_ITEMS).ToList(); }
            set { }
        }

        public override decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            return base.GetCurrentPeriodPercentage(newTotalQuantity);
        }
    }

    public class Estimation_Direct_ItemProgress : BluePrintsProgressableByQuantityProjectionBase<ESTIMATION_DIRECT_ITEMProjection>
    {

    }

    public abstract class BluePrintsProgressableByQuantityProjectionBase<TEntity> : BluePrintsProgressableProjectionBase<TEntity>, IQuantityReportable
        where TEntity : class, IQuantityDeliverableProjection, ICanUpdate, new()
    {
        public decimal QuantityPerHour
        {
            get
            {
                return Entity.Total_Quantity / Entity.TotalHours;
            }
        }

        public decimal TotalPercentage
        {
            get
            {
                if (PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0)
                    return 0;

                return PROGRESS_ITEM_UpToCurrentDataDate.Sum(x => x.EARNED_UNITS) / Entity.TotalHours;
            }
        }

        public decimal CurrentTotalInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_UpToCurrentDataDate.Count() == 0 || QuantityPerHour == 0)
                    return 0;

                return PROGRESS_ITEM_UpToCurrentDataDate.Sum(x => x.EARNED_UNITS) * QuantityPerHour;
            }
            set
            {
                //dummy set
            }
        }

        public decimal PastInstalledQuantity
        {
            get
            {
                if (PROGRESS_ITEM_BeforeDataDate.Count() == 0 || QuantityPerHour == 0)
                    return 0;

                return PROGRESS_ITEM_BeforeDataDate.Sum(x => x.EARNED_UNITS) / QuantityPerHour;
            }
        }

        public decimal Estimated_Quantity => Entity.Estimated_Quantity;

        public decimal Total_Quantity => Entity.Total_Quantity;

        public string UOM => Entity.UOM;

        public virtual decimal GetCurrentPeriodPercentage(decimal newTotalQuantity)
        {
            if (Entity.Total_Quantity == 0)
                return 0;
            return (newTotalQuantity - PastInstalledQuantity) / Entity.Total_Quantity;
        }

        public decimal GetCurrentPeriodHours(decimal currentPeriodPercentage)
        {
            return currentPeriodPercentage * Entity.TotalHours;
        }
    }

    public abstract class BluePrintsProgressableProjectionBase<TEntity> : BluePrintsProjectionBase<TEntity>, IReportable
        where TEntity : class, IDeliverableProjection, ICanUpdate, new()
    {
        public void SetPROGRESS_ITEMS(IEnumerable<PROGRESS_ITEM> progress_items)
        {
            PROGRESS_ITEMS = progress_items.ToList();
        }

        public void SetReportingDataDate(DateTime reportingDataDate)
        {
            ReportingDataDate = reportingDataDate;
        }

        public void Update()
        {
            IQuantityDeliverableGroupProjection quantityGroup = Entity as IQuantityDeliverableGroupProjection;
            if(quantityGroup != null)
            {
                foreach(IQuantityReportable reportables in quantityGroup.Reportables)
                {
                    reportables.Update();
                }
            }

            Entity.Update();
            RaisePropertiesChanged();
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_BeforeDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE < ReportingDataDate);
            }
        }

        public PROGRESS_ITEM PROGRESS_ITEM_Current
        {
            get
            {
                IBasicDeliverable reportableItem = Entity as IBasicDeliverable;
                if(reportableItem != null)
                    return PROGRESS_ITEMS.FirstOrDefault(y => y.GUID_ORIBASEITEM == reportableItem.OriginalEntityKey && y.EARNED_DATE == ReportingDataDate);

                return null;
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_UpToCurrentDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE <= ReportingDataDate);
            }
        }

        public IEnumerable<PROGRESS_ITEM> PROGRESS_ITEM_AfterDataDate
        {
            get
            {
                return PROGRESS_ITEMS.Where(y => y.EARNED_DATE > ReportingDataDate);
            }
        }

        public DateTime ReportingDataDate { get; set; }

        public virtual List<PROGRESS_ITEM> PROGRESS_ITEMS { get; set; }

        public ProgressStats Stats { get; set; }

        public string Stock_Code
        {
            get { return Entity.Stock_Code; }
        }

        public decimal TotalHoursIncludeByDuration => Entity.TotalHoursIncludeByDuration;

        public decimal EstimatedHours => Entity.EstimatedHours;

        public decimal TotalHours => Entity.TotalHours;

        public Guid? Area_Guid => Entity.Area_Guid;

        public Guid? SubArea_Guid => Entity.SubArea_Guid;

        public string ReportableItem_Name => Entity.ReportableItem_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public Guid? Workpack_Guid => Entity.Workpack_Guid;

        public Guid OriginalEntityKey { get => Entity.OriginalEntityKey; set => Entity.OriginalEntityKey = value; }

        public decimal ItemRate => Entity.ItemRate;

        public decimal EstimatedCosts => Entity.EstimatedCosts;

        public decimal TotalCosts => Entity.TotalCosts;
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

            List<Estimation_Direct_ItemProgress> estimationDirectItemProgress = new List<Estimation_Direct_ItemProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in ESTIMATION_DIRECT_ITEMProjection)
            {
                Estimation_Direct_ItemProgress newEstimation_Direct_itemProgress = new Estimation_Direct_ItemProgress();
                newEstimation_Direct_itemProgress.Entity = ESTIMATION_DIRECT_ITEM;
                newEstimation_Direct_itemProgress.SetReportingDataDate(reportingDataDate);
                SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                estimationDirectItemProgress.Add(newEstimation_Direct_itemProgress);
            }

            var trackableESTIMATION_DIRECT_ITEMProjectionGroupByStockCode = estimationDirectItemProgress.Where(x => !x.Entity.Entity.STANDALONE)
                .GroupBy(x => x.Entity.Entity.GUID_STOCK_CODE).Select(group => new { StockCodeGuid = group.Key, Estimation_Direct_ItemProjection = group.ToList() });

            foreach (STOCK_CODE STOCK_CODE in projectSTOCK_CODES)
            {
                Stock_CodeProgress newStock_CodeProgress = new Stock_CodeProgress();
                newStock_CodeProgress.Entity.Entity = STOCK_CODE;
                
                var currentStockCodeReportables = trackableESTIMATION_DIRECT_ITEMProjectionGroupByStockCode.FirstOrDefault(x => x.StockCodeGuid == STOCK_CODE.GUID);
                if(currentStockCodeReportables != null)
                {
                    newStock_CodeProgress.Entity.Reportables = currentStockCodeReportables.Estimation_Direct_ItemProjection;
                    newStock_CodeProgress.Deliverables = currentStockCodeReportables.Estimation_Direct_ItemProjection.ToList();
                    newStock_CodeProgress.SetReportingDataDate(reportingDataDate);
                    ProgressDisplay newProgressDisplay = new ProgressDisplay();
                    newProgressDisplay.ProgressItem = new GroupDisplayReportable(newStock_CodeProgress);
                    progressItems.Add(newProgressDisplay);
                }
            }

            progressItems.AddRange(estimationDirectItemProgress.Where(x => x.Entity.Entity.STANDALONE).Select(x => new ProgressDisplay() { ProgressItem = new DisplayReportable(x) }));

            return progressItems.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IQueryable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            foreach(dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                IBasicDeliverable basicDeliverable = reportable as IBasicDeliverable;
                if (basicDeliverable == null)
                    break;

                if (item.OriginalGuid == basicDeliverable.OriginalEntityKey)
                {
                    reportable.PROGRESS_ITEMS = item.Progresses;
                    return;
                }
            }

            reportable.PROGRESS_ITEMS = new List<PROGRESS_ITEM>();
        }
    }
}