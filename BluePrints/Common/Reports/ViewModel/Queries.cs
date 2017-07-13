using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Projections;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ProgressQueries
    {
        public static IQueryable<BASELINE_ITEMProgress> User_OffsiteDirectProgressItemTransformation(IQueryable<BASELINE_ITEM> query, USER user, bool buildStats = true)
        {
            IQueryable<BASELINE_ITEM> user_baseline_item = query.Where(x => x.GUID_USER == user.GUID && x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active);
            List<BASELINE_ITEMProgress> user_baseline_item_progresses = new List<BASELINE_ITEMProgress>();

            var user_baseline_item_by_projects = user_baseline_item.GroupBy(x => x.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
            foreach (var user_baseline_item_by_project in user_baseline_item_by_projects)
            {
                PROJECT project = user_baseline_item_by_project.Project;

                PROGRESS live_progress = project.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live);
                if (live_progress == null)
                    continue;

                BASELINE live_baseline = project.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> user_project_baseline_item = user_baseline_item_by_project.Deliverables;
                IEnumerable<WORKPACK> workpacks = project.WORKPACK;
                IEnumerable<VARIATION> approved_variations = project.VARIATION.Where(x => x.APPROVED != null);
                IEnumerable<PROGRESS_ITEM> progresses = live_progress.PROGRESS_ITEM;
                IEnumerable<RATE> rates = project.RATE;

                List<BASELINE_ITEMProgress> user_project_baseline_item_progress = OffsiteDirectProgressItemTransformation(user_project_baseline_item.AsQueryable(), project, live_progress, rates, progresses, approved_variations).ToList();
                if (buildStats)
                    user_project_baseline_item_progress.ForEach(x => x.BuildStats());

                user_baseline_item_progresses.AddRange(user_project_baseline_item_progress);
            }

            return user_baseline_item_progresses.AsQueryable();
        }

        public static IQueryable<BASELINE_ITEMProgress> OffsiteDirectProgressItemTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION> VARIATIONS = null, bool? buildBudgetedOnly = null)
        {
            IQueryable<BASELINE_ITEMProjection> baseline_item_projection;
            //When live PROGRESS doesn't exists don't return anything
            if (PROGRESS == null)
                baseline_item_projection = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                baseline_item_projection = BASELINE_ITEMProjectionQueries.IDeliverable_Rates_Transformation(BASELINE_ITEMS, RATES);

            List<VariationAdjustment> projectVariationAdjustments;
            //VARIATIONS are only necessary if front-end requires percentages
            if (VARIATIONS != null)
                projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.AsQueryable(), baseline_item_projection);
            else
                projectVariationAdjustments = new List<VariationAdjustment>();

            List<BASELINE_ITEMProgress> baseline_item_progresses = baseline_item_projection.Select(x => new BASELINE_ITEMProgress(PROJECT, PROGRESS, projectVariationAdjustments)
            {
                Entity = x,
                Live_PROGRESS = PROGRESS
            }).ToList();

            dynamic progress_item_by_originalguid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            foreach (BASELINE_ITEMProgress baseline_item_progress in baseline_item_progresses)
            {
                SetReportablePROGRESS_ITEM(baseline_item_progress, progress_item_by_originalguid);
                if (buildBudgetedOnly != null)
                {
                    if((bool)buildBudgetedOnly)
                        baseline_item_progress.BuildBudgetedStats();
                    else
                        baseline_item_progress.BuildStats();
                }

            }

            return baseline_item_progresses.AsQueryable();
        }

        public static IQueryable<ReportablesDisplay> SiteDirectProgressItemTransformation(
            IQueryable<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEMS, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<COMMODITY_CODE> COMMODITY_CODES, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<RATE> projectRATES)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ReportablesDisplay> display_items = new List<ReportablesDisplay>();
            var PROGRESS_ITEMSByOriginalGuid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            IEnumerable<ESTIMATION_DIRECT_ITEMProjection> estimation_direct_item_rates =
                ESTIMATION_DIRECT_ITEMProjectionQueries.IDeliverable_Rates_Transformation(ESTIMATION_DIRECT_ITEMS,
                                                                                                projectRATES,
                                                                                                projectSTOCK_CODES,
                                                                                                COMMODITY_CODES).AsEnumerable();

            List<ESTIMATION_DIRECT_ITEMProgress> estimation_direct_item_progresses = new List<ESTIMATION_DIRECT_ITEMProgress>();
            foreach (ESTIMATION_DIRECT_ITEMProjection estimation_direct_item_rate in estimation_direct_item_rates)
            {
                ESTIMATION_DIRECT_ITEMProgress newEstimation_Direct_itemProgress = new ESTIMATION_DIRECT_ITEMProgress();
                newEstimation_Direct_itemProgress.Live_PROGRESS = PROGRESS;
                newEstimation_Direct_itemProgress.Entity = estimation_direct_item_rate;
                newEstimation_Direct_itemProgress.SetReportingDataDate(PROGRESS.DATA_DATE);
                SetReportablePROGRESS_ITEM(newEstimation_Direct_itemProgress, PROGRESS_ITEMSByOriginalGuid);
                estimation_direct_item_progresses.Add(newEstimation_Direct_itemProgress);
            }

            var estimation_direct_progress_by_commoditycodeguid = estimation_direct_item_progresses.Where(x => !x.Entity.Entity.STANDALONE)
                .GroupBy(x => x.Entity.Entity.GUID_COMMODITY_CODE).Select(group => new { CommodityCodeGuid = group.Key, Estimation_Direct_ItemProgress = group.ToList() });

            foreach (COMMODITY_CODE COMMODITY_CODE in COMMODITY_CODES)
            {
                COMMODITY_CODEProgress newCommodity_CodeProgress = new COMMODITY_CODEProgress();
                newCommodity_CodeProgress.Entity.Entity = COMMODITY_CODE;
                newCommodity_CodeProgress.Live_PROGRESS = PROGRESS;

                var currentCommodity_CodeProgresses = estimation_direct_progress_by_commoditycodeguid.FirstOrDefault(x => x.CommodityCodeGuid == COMMODITY_CODE.GUID);
                if (currentCommodity_CodeProgresses != null)
                {
                    newCommodity_CodeProgress.Reportables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress;
                    newCommodity_CodeProgress.Entity.Deliverables = currentCommodity_CodeProgresses.Estimation_Direct_ItemProgress.Select(x => x.Entity);
                    newCommodity_CodeProgress.SetReportingDataDate(PROGRESS.DATA_DATE);
                    ReportablesDisplay newProgressDisplay = new ReportablesDisplay();
                    newProgressDisplay.ProgressItem = new DisplayQuantityReportableGroup(newCommodity_CodeProgress);
                    display_items.Add(newProgressDisplay);
                }
            }

            display_items.AddRange(estimation_direct_item_progresses.Where(x => x.Entity.Entity.STANDALONE).Select(x => new ReportablesDisplay() { ProgressItem = new DisplayQuantityReportable(x) }));

            return display_items.AsQueryable();
        }

        private static void SetReportablePROGRESS_ITEM(IReportable reportable, IEnumerable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            ICanSetProgresses setProgressesProjection = reportable as ICanSetProgresses;
            if (setProgressesProjection == null)
                return;

            foreach (dynamic item in PROGRESS_ITEMSByOriginalGuid)
            {
                if (item.OriginalGuid == reportable.OriginalEntityKey)
                {
                    setProgressesProjection.SetProgressItems(item.Progresses);
                    return;
                }
            }

            setProgressesProjection.SetProgressItems(new List<PROGRESS_ITEM>());
        }
    }
}
