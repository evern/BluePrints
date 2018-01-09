using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Projections;
using System.Text;
using System.Threading.Tasks;
using BluePrints.ViewModels;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ProgressQueries
    {
        public static IQueryable<BASELINE_ITEMProgress> User_OffsiteDirectProgressItemTransformation(IQueryable<BASELINE_ITEM> query, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, USER user, bool buildStats = true)
        {
            IQueryable<BASELINE_ITEM> user_baseline_item = query.Where(x => x.GUID_USER == user.GUID && x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active);
            List<BASELINE_ITEMProgress> user_baseline_item_progresses = new List<BASELINE_ITEMProgress>();

            var user_baseline_item_by_projects = user_baseline_item.GroupBy(x => x.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
            foreach (var user_baseline_item_by_project in user_baseline_item_by_projects)
            {
                PROJECT project = user_baseline_item_by_project.Project;

                PROGRESS live_progress = project.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == ProgressType.Design);
                if (live_progress == null)
                    continue;

                BASELINE live_baseline = project.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> user_project_baseline_item = user_baseline_item_by_project.Deliverables;
                IEnumerable<SUBJOB> subjobs = project.SUBJOB;
                IEnumerable<VARIATION> approved_variations = project.VARIATION.Where(x => x.APPROVED != null);

                //need to use external PROGRESS_ITEMS because live_progress.PROGRESS_ITEM is cached and will not update OnMessage
                IEnumerable<PROGRESS_ITEM> progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_progress.GUID);
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
            IEnumerable<VARIATION> VARIATIONS = null, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, DeliverableInternalNumberMode internalNumberMode = DeliverableInternalNumberMode.Default, IEnumerable<P6Data.TASK> P6_TASKS = null)
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

            List<BASELINE_ITEMProgress> baseline_item_progresses = baseline_item_projection.Select(x => new BASELINE_ITEMProgress(PROJECT, PROGRESS, x, projectVariationAdjustments)
            {
                Entity = x,
                Live_PROGRESS = PROGRESS,
                P6_Assignments = PopulateP6Assignment(x, PROJECT, P6_ASSIGNMENTS),
                P6TASKCollection = P6_TASKS,
                IsInternalNumberAlwaysEditable = internalNumberMode == DeliverableInternalNumberMode.AlwaysEditable,
                IsInternalNumberManualOnly = internalNumberMode == DeliverableInternalNumberMode.Manual
            }).ToList();

            dynamic progress_item_by_originalguid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            foreach (BASELINE_ITEMProgress baseline_item_progress in baseline_item_progresses)
            {
                SetReportablePROGRESS_ITEM(baseline_item_progress, progress_item_by_originalguid);

                if (buildStats)
                    baseline_item_progress.BuildStats();

            }

            return baseline_item_progresses.AsQueryable();
        }

        private static List<P6_ASSIGNMENT> PopulateP6Assignment(BASELINE_ITEMProjection baseline_item, PROJECT project, IEnumerable<P6_ASSIGNMENT> P6ASSIGNMENTCollection)
        {
            if (P6ASSIGNMENTCollection == null)
                return new List<P6_ASSIGNMENT>();

            if(project.USE_WORKPACKS)
                return P6ASSIGNMENTCollection.Where(x => x.GUID_ORIGINAL == baseline_item.Workpack_Guid).ToList();

            return P6ASSIGNMENTCollection.Where(x => x.GUID_ORIGINAL == baseline_item.OriginalEntityKey).ToList();
        }

        public static IQueryable<ReportablesDisplay> SiteDirectProgressItemTransformation(
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_GROUP> STOCK_GROUPS, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<RATE> projectRATES)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ReportablesDisplay> display_items = new List<ReportablesDisplay>();

            IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_item_progresses =
                ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS, PROJECT, projectRATES, PROGRESS, PROGRESS_ITEMS,
                                                                                                projectSTOCK_CODES,
                                                                                                STOCK_GROUPS).AsEnumerable();

            var estimation_direct_progress_by_stockgroupguid = estimation_direct_item_progresses.Where(x => x.Entity.Progress_Type != Estimation_DirectProgressType.Standalone)
                .GroupBy(x => x.Entity.Entity.GUID_STOCK_GROUP).Select(group => new { StockGroupGuid = group.Key, DeliverablesByStockGroup = group.ToList() });


            foreach (STOCK_GROUP STOCK_GROUP in STOCK_GROUPS)
            {
                STOCK_GROUPProgress new_stock_group = new STOCK_GROUPProgress();
                new_stock_group.Entity.Entity = STOCK_GROUP;
                new_stock_group.Live_PROGRESS = PROGRESS;

                var deliverables_byStockGroup = estimation_direct_progress_by_stockgroupguid.FirstOrDefault(x => x.StockGroupGuid == STOCK_GROUP.GUID);
                if (deliverables_byStockGroup != null)
                {
                    var deliverables_byStockGroupByAreas = deliverables_byStockGroup.DeliverablesByStockGroup.GroupBy(x => x.Area_Guid).Select(group => new { AreaGuid = group.Key, DeliverablesByStockGroupByArea = group.ToList() });
                    foreach (var deliverables_byStockGroupByArea in deliverables_byStockGroupByAreas)
                    {
                        var deliverables_byStockGroupByAreaBySubAreas = deliverables_byStockGroupByArea.DeliverablesByStockGroupByArea.GroupBy(x => x.SubArea_Guid).Select(group => new { SubAreaGuid = group.Key, DeliverablesByStockGroupByAreaBySubArea = group.ToList() });
                        foreach (var deliverables_ByStockGroupByAreaBySubArea in deliverables_byStockGroupByAreaBySubAreas)
                        {
                            new_stock_group.Reportables = deliverables_ByStockGroupByAreaBySubArea.DeliverablesByStockGroupByAreaBySubArea;
                            new_stock_group.Entity.Deliverables = deliverables_ByStockGroupByAreaBySubArea.DeliverablesByStockGroupByAreaBySubArea.Select(x => x.Entity);
                            new_stock_group.SetReportingDataDate(PROGRESS.DATA_DATE);
                            ReportablesDisplay newProgressDisplay = new ReportablesDisplay();
                            newProgressDisplay.ProgressItem = new DisplayQuantityReportableGroup(new_stock_group);
                            display_items.Add(newProgressDisplay);
                        }
                    }
                }
            }

            display_items.AddRange(estimation_direct_item_progresses.Where(x => x.Progress_Type == Estimation_DirectProgressType.Standalone).Select(x => new ReportablesDisplay() { ProgressItem = new DisplayQuantityReportable(x, false) }));

            return display_items.AsQueryable();
        }

        public static void SetReportablePROGRESS_ITEM(IReportable reportable, IEnumerable<dynamic> PROGRESS_ITEMSByOriginalGuid)
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
