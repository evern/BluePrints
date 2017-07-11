using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Projections;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ProgressItemQueries
    {
        public static IQueryable<Baseline_ItemProgress> OffsiteDirectProgressItemTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            PROJECT PROJECT, 
            PROGRESS PROGRESS,
            IEnumerable<WORKPACK> WORKPACKS, 
            IEnumerable<RATE> RATES,
            IEnumerable<DELIVERABLES_STATUS> DELIVERABLE_STATUSES, 
            IEnumerable<VARIATION> VARIATIONS, 
            IEnumerable<AREA> SUBAREAS, bool? buildBudgetedOnly = null)
        {
            IQueryable<BASELINE_ITEMProjection> BASELINE_ITEMProjections;
            if (PROGRESS == null)
                BASELINE_ITEMProjections = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                BASELINE_ITEMProjections = BASELINE_ITEMProjectionQueries.BASELINE_ITEMProjectionQuery(BASELINE_ITEMS, RATES, DELIVERABLE_STATUSES, SUBAREAS);

            TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(PROGRESS);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(PROGRESS);
            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), BASELINE_ITEMProjections);

            List<Baseline_ItemProgress> progress_items = BASELINE_ITEMProjections
                        .Select(x =>
                        new Baseline_ItemProgress(PROJECT, PROGRESS, projectVariationAdjustments)
                        {
                            Entity = x,
                            Live_PROGRESS = PROGRESS
                        }).ToList();

            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            foreach (Baseline_ItemProgress progress_item in progress_items)
            {
                SetReportablePROGRESS_ITEM(progress_item, PROGRESS_ITEMSByOriginalGuid);
                if (buildBudgetedOnly != null)
                {
                    if((bool)buildBudgetedOnly)
                        progress_item.BuildBudgetedStats();
                    else
                        progress_item.BuildStats();
                }

            }

            return progress_items.AsQueryable();
        }

        public static IQueryable<ProgressDisplay> SiteDirectProgressItemTransformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<COMMODITY_CODE> projectCOMMODITY_CODES, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<RATE> projectRATES, DateTime reportingDataDate)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ProgressDisplay> progressItems = new List<ProgressDisplay>();
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> ESTIMATION_DIRECT_ITEMProjection =
                ESTIMATION_DIRECT_ITEMProjectionQueries.ESTIMATION_DIRECT_ITEMProjectionQuery(ESTIMATION_DIRECT_ITEMS,
                                                                                                projectRATES,
                                                                                                projectSTOCK_CODES,
                                                                                                projectCOMMODITY_CODES).AsEnumerable();

            List<Estimation_Direct_ItemProgress> estimationDirectItemProgress = new List<Estimation_Direct_ItemProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection ESTIMATION_DIRECT_ITEM in ESTIMATION_DIRECT_ITEMProjection)
            {
                Estimation_Direct_ItemProgress newEstimation_Direct_itemProgress = new Estimation_Direct_ItemProgress();
                newEstimation_Direct_itemProgress.Entity = ESTIMATION_DIRECT_ITEM;
                newEstimation_Direct_itemProgress.SetReportingDataDate(reportingDataDate);
                SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                estimationDirectItemProgress.Add(newEstimation_Direct_itemProgress);
            }

            var estimationDirectProgressByCommodityCode = estimationDirectItemProgress.Where(x => !x.Entity.Entity.STANDALONE)
                .GroupBy(x => x.Entity.Entity.GUID_COMMODITY_CODE).Select(group => new { CommodityCodeGuid = group.Key, Estimation_Direct_ItemProgress = group.ToList() });

            foreach (COMMODITY_CODE COMMODITY_CODE in projectCOMMODITY_CODES)
            {
                Commodity_CodeProgress newCommodity_CodeProgress = new Commodity_CodeProgress();
                newCommodity_CodeProgress.Entity.Entity = COMMODITY_CODE;

                var currentCommodity_CodeProgresses = estimationDirectProgressByCommodityCode.FirstOrDefault(x => x.CommodityCodeGuid == COMMODITY_CODE.GUID);
                if (currentCommodity_CodeProgresses != null)
                {
                    newCommodity_CodeProgress.Entity.Reportables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress;
                    newCommodity_CodeProgress.Deliverables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress.ToList();
                    newCommodity_CodeProgress.SetReportingDataDate(reportingDataDate);
                    ProgressDisplay newProgressDisplay = new ProgressDisplay();
                    newProgressDisplay.ProgressItem = new GroupDisplayReportable(newCommodity_CodeProgress);
                    progressItems.Add(newProgressDisplay);
                }
            }

            progressItems.AddRange(estimationDirectItemProgress.Where(x => x.Entity.Entity.STANDALONE).Select(x => new ProgressDisplay() { ProgressItem = new StandaloneDisplayReportable(x) }));

            return progressItems.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IEnumerable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            ICanSetProgresses setProgressesProjection = reportable as ICanSetProgresses;
            if (setProgressesProjection == null)
                return;

            foreach (dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                ISortableDeliverable basicDeliverable = reportable.Deliverable as ISortableDeliverable;
                if (basicDeliverable == null)
                    break;

                if (item.OriginalGuid == basicDeliverable.OriginalEntityKey)
                {
                    setProgressesProjection.SetProgressItems(item.Progresses);
                    return;
                }
            }

            setProgressesProjection.SetProgressItems(new List<PROGRESS_ITEM>());
        }
    }
}
