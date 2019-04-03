using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Projections;
using System.Text;
using System.Threading.Tasks;
using BluePrints.ViewModels;
using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Dialogs;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ProgressQueries
    {
        public static IQueryable<BASELINE_ITEMProgress> ProjectUser_OffsiteDirectProgressItemTransformation(IQueryable<BASELINE_ITEM> query, PROJECT project, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<USER> USERCollection, IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKSCollection, bool useExtrapolateDate = false)
        {
            List<BASELINE_ITEMProgress> user_baseline_item_progresses = new List<BASELINE_ITEMProgress>();
            PROGRESS live_progress = project.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
            BASELINE live_baseline = project.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
            IEnumerable<SUBJOB> subjobs = project.SUBJOB;
            IEnumerable<VARIATION> approved_variations = project.VARIATION.Where(x => x.APPROVED != null);

            //need to use external PROGRESS_ITEMS because live_progress.PROGRESS_ITEM is cached and will not update OnMessage
            IEnumerable<PROGRESS_ITEM> progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_progress.GUID);
            IEnumerable<RATE> rates = project.RATE;
            List<BASELINE_ITEMProgress> projections = OffsiteDirectProgressItemTransformation(query.Where(x => x.GUID_BASELINE == live_baseline.GUID), project, live_progress, rates, progresses, approved_variations, false, null, DeliverableInternalNumberMode.Default, false, null, USERCollection, BASELINE_ITEM_WORKSCollection).ToList();

            foreach (BASELINE_ITEMProgress projection in projections)
            {
                if(projection.AssignedUsers.Count() > 0)
                {
                    foreach (User_Weight userWeight in projection.AssignedUsers)
                    {
                        BASELINE_ITEMProgress userProjection = new BASELINE_ITEMProgress(project, live_progress, projection.Entity, new List<VariationAdjustment>(), false);
                        userProjection.Stats = new ProgressStats(projection.Stats.ReportingDataDate, projection.Stats.ReportingInterval, projection.Stats.FirstAlignedDataDate, projection.Stats.BudgetedUnits * userWeight.AggregateWeight, projection.Stats.TotalUnits * userWeight.AggregateWeight, projection.Stats.BudgetedQty * userWeight.AggregateWeight, projection.Stats.TotalQty * userWeight.AggregateWeight, projection.Stats.BudgetedCosts * userWeight.AggregateWeight, projection.Stats.TotalCosts * userWeight.AggregateWeight, new List<VariationAdjustment>());
                        DataUtils.ShallowCopy(userProjection, projection);
                        userProjection.Entity = projection.Entity;
                        userProjection.User_Name = userWeight.UserName;
                        userProjection.User_Role = userWeight.UserRole;
                        userProjection.BuildStats(userWeight.AggregateWeight);
                        user_baseline_item_progresses.Add(userProjection);
                    }
                }
                else
                {
                    projection.BuildStats();
                    user_baseline_item_progresses.Add(projection);
                }
            }

            return user_baseline_item_progresses.AsQueryable();
        }


        public static IQueryable<BASELINE_ITEMProgress> DocControlProgressItemTransformation(IQueryable<BASELINE_ITEM> query, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, bool buildStats = false, bool useReportDate = false)
        {
            IQueryable<BASELINE_ITEM> live_baseline_items = query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active && (x.INTERNALNUM_STATUS == DocumentNumberStatus.Awaiting || x.CLIENTNUM_STATUS == DocumentNumberStatus.Awaiting));
            List<BASELINE_ITEM> controlBaselineItem = new List<BASELINE_ITEM>();
            foreach (BASELINE_ITEM live_baseline_item in live_baseline_items)
            {
                controlBaselineItem.Add(live_baseline_item);
            }

            List<BASELINE_ITEMProgress> returnProjections = new List<BASELINE_ITEMProgress>();

            var user_baseline_item_by_projects = controlBaselineItem.GroupBy(x => x.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
            foreach (var user_baseline_item_by_project in user_baseline_item_by_projects)
            {
                PROJECT project = user_baseline_item_by_project.Project;

                PROGRESS live_progress = project.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
                if (live_progress == null)
                    continue;

                BASELINE live_baseline = project.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> user_project_baseline_item = user_baseline_item_by_project.Deliverables;
                IEnumerable<SUBJOB> subjobs = project.SUBJOB;
                IEnumerable<VARIATION> approved_variations = project.VARIATION.Where(x => x.APPROVED != null);

                //need to use external PROGRESS_ITEMS because live_progress.PROGRESS_ITEM is cached and will not update OnMessage
                IEnumerable<PROGRESS_ITEM> progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_progress.GUID);
                IEnumerable<RATE> rates = project.RATE;

                List<BASELINE_ITEMProgress> docControlProjections = OffsiteDirectProgressItemTransformation(user_project_baseline_item.AsQueryable(), project, live_progress, rates, progresses, approved_variations, false, null, DeliverableInternalNumberMode.AlwaysEditable, useReportDate, null).ToList();
                if (buildStats)
                {
                    foreach (BASELINE_ITEMProgress docControlProjection in docControlProjections)
                    {
                        docControlProjection.BuildStats();
                    }
                }

                returnProjections.AddRange(docControlProjections);
            }

            return returnProjections.AsQueryable();
        }

        public static IQueryable<BASELINE_ITEMProgress> User_OffsiteDirectProgressItemTransformation(IQueryable<BASELINE_ITEM> query, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<USER> USERCollection, IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKSCollection, USER user, bool buildStats = true, bool useReportDate = false, IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection = null, IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection = null)
        {
            //IQueryable<BASELINE_ITEM> user_baseline_item = query.Where(x => x.GUID_USER == user.GUID && x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active);
            //List<BASELINE_ITEM_WORK> current_user_works = BASELINE_ITEM_WORKSCollection.Where(x => x.GUID_USER == user.GUID).ToList();

            IQueryable<BASELINE_ITEM> live_baseline_items = query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.PROJECT.STATUS == ProjectStatus.Active);
            List<BASELINE_ITEM> user_baseline_item = new List<BASELINE_ITEM>();
            foreach(BASELINE_ITEM live_baseline_item in live_baseline_items)
            {
                //if (current_user_works.Any(works => works.GUID_BASELINE_ITEM_ORIGINAL == live_baseline_item.OriginalEntityKey))
                if(live_baseline_item.GUID_USER != null && live_baseline_item.GUID_USER == user.GUID)
                    user_baseline_item.Add(live_baseline_item);
            }

            List<BASELINE_ITEMProgress> user_baseline_item_progresses = new List<BASELINE_ITEMProgress>();

            var user_baseline_item_by_projects = user_baseline_item.GroupBy(x => x.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
            foreach (var user_baseline_item_by_project in user_baseline_item_by_projects)
            {
                PROJECT project = user_baseline_item_by_project.Project;

                PROGRESS live_progress = project.PROGRESS.FirstOrDefault(x => x.STATUS == ProgressStatus.Live && x.TYPE == PhaseType.Design);
                if (live_progress == null)
                    continue;

                ChronologicalHelpers.AutosetProgressDataDate(live_progress);
                BASELINE live_baseline = project.BASELINE.FirstOrDefault(x => x.STATUS == BaselineStatus.Live);
                IEnumerable<BASELINE_ITEM> user_project_baseline_item = user_baseline_item_by_project.Deliverables;
                IEnumerable<SUBJOB> subjobs = project.SUBJOB;
                IEnumerable<VARIATION> approved_variations = project.VARIATION.Where(x => x.APPROVED != null);

                //need to use external PROGRESS_ITEMS because live_progress.PROGRESS_ITEM is cached and will not update OnMessage
                IEnumerable<PROGRESS_ITEM> progresses = PROGRESS_ITEMS.Where(x => x.GUID_PROGRESS == live_progress.GUID);
                IEnumerable<RATE> rates = project.RATE;

                List<BASELINE_ITEMProgress> user_project_baseline_item_progress = OffsiteDirectProgressItemTransformation(user_project_baseline_item.AsQueryable(), project, live_progress, rates, progresses, approved_variations, false, null, DeliverableInternalNumberMode.Default, useReportDate, null, USERCollection, BASELINE_ITEM_WORKSCollection, false, null, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection, project.GUID).ToList();
                //if (buildStats)
                //{
                //    foreach (BASELINE_ITEMProgress user_deliverable in user_project_baseline_item_progress)
                //    {

                //        User_Weight current_user_weight = user_deliverable.AssignedUsers.FirstOrDefault(x => x.User != null && x.User.GUID == user.GUID);
                //        if (current_user_weight != null)
                //        {
                //            user_deliverable.BuildStats(current_user_weight.AggregateWeight);
                //        }
                //        else
                //        {
                //            user_deliverable.BuildStats();
                //        }
                //    }
                //}
                
                user_baseline_item_progresses.AddRange(user_project_baseline_item_progress);
            }

            return user_baseline_item_progresses.AsQueryable();
        }

        public static IQueryable<BASELINE_ITEMProgress> DesignVariationItemQuery(IQueryable<BASELINE_ITEM> BASELINE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, BASELINE BASELINE, IEnumerable<VARIATION> VARIATIONS, VARIATION VARIATION, IEnumerable<VARIATION_ITEM> VARIATION_ITEMS, IEnumerable<RATE> RATES)
        {
            //when either live progress or variation doesn't exists don't return anything
            IQueryable<BASELINE_ITEMProgress> Baseline_ItemProgresses;
            if (PROGRESS == null || VARIATION == null)
                Baseline_ItemProgresses = new List<BASELINE_ITEMProgress>().AsQueryable();
            else
                Baseline_ItemProgresses = ProgressQueries.OffsiteDirectProgressItemTransformation(BASELINE_ITEMS, PROJECT, PROGRESS, RATES, PROGRESS_ITEMS, VARIATIONS);

            foreach (var baseline_item in Baseline_ItemProgresses)
            {
                baseline_item.UpdateVariationItem(VARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == baseline_item.Entity.Entity.GUID_ORIGINAL).FirstOrDefault());
                baseline_item.SubmittedDate = VARIATION.SUBMITTED;
                baseline_item.ApprovedDate = VARIATION.APPROVED;
            }

            return Baseline_ItemProgresses;
        }

        public static IQueryable<BASELINE_ITEMProgress> OffsiteDirectProgressItemTransformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS,
            PROJECT PROJECT,
            PROGRESS PROGRESS,
            IEnumerable<RATE> RATES,
            IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS,
            IEnumerable<VARIATION> VARIATIONS, bool buildStats = false, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS = null, DeliverableInternalNumberMode internalNumberMode = DeliverableInternalNumberMode.Default, bool useReportDate = false, IEnumerable<P6Data.TASK> P6_TASKS = null, IEnumerable<USER> USERCollection = null, IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection = null, bool extrapolateDateToDataDate = false, IEnumerable<REGISTER_HOLD_REF> REGISTER_HOLD_REFCollection = null, IEnumerable<DELIVERABLES_STATUS> DELIVERABLES_STATUSCollection = null, IEnumerable<DSTATUS_DOCTYPE> DSTATUS_DOCTYPECollection = null, Guid? ProjectGuidForDeliverablesStatus = null)
        {
            IQueryable<BASELINE_ITEMProjection> baseline_item_queryable;

            //When live PROGRESS doesn't exists don't return anything
            if (PROGRESS == null)
                baseline_item_queryable = new List<BASELINE_ITEMProjection>().AsQueryable();
            else
                baseline_item_queryable = BASELINE_ITEMProjectionQueries.IDeliverable_Rates_Transformation(BASELINE_ITEMS, RATES);

            //commented out because multiple resources and weighting isn't used
            //need to cast to list if not AssignUserObject won't stick
            List<BASELINE_ITEMProjection> projections = baseline_item_queryable.ToList();

            #region Resource pro rate
            //if (BASELINE_ITEM_WORKCollection != null && USERCollection != null)
            //{
            //    foreach(BASELINE_ITEMProjection baseline_item in baseline_item_projection)
            //    {
            //        if(REGISTER_HOLD_REFCollection != null)
            //            baseline_item.Entity.SetHolds(REGISTER_HOLD_REFCollection);

            //        baseline_item.AssignUserObject = USERCollection.Where(user => BASELINE_ITEM_WORKCollection.Any(work => work.GUID_BASELINE_ITEM_ORIGINAL == baseline_item.OriginalEntityKey && work.GUID_USER == user.GUID)).ToList();
            //        List<USER> current_deliverable_users = new List<USER>();
            //        IEnumerable<BASELINE_ITEM_WORK> current_deliverable_assignments = BASELINE_ITEM_WORKCollection.Where(work => work.GUID_BASELINE_ITEM_ORIGINAL == baseline_item.OriginalEntityKey);

            //        foreach (BASELINE_ITEM_WORK assignment in current_deliverable_assignments)
            //        {
            //            USER findUSER = USERCollection.FirstOrDefault(x => x.GUID == assignment.GUID_USER);
            //            if (findUSER != null)
            //                baseline_item.UserWeights.Add(new User_Weight() { User = findUSER, Weight = assignment.WEIGHTING });
            //        }

            //        decimal total_weight = baseline_item.UserWeights.Sum(x => x.Weight);
            //        if(total_weight > 0)
            //        {
            //            foreach (User_Weight userweight in baseline_item.UserWeights)
            //            {
            //                userweight.AggregateWeight = userweight.Weight / total_weight;
            //            }
            //        }
            //    }
            //} 
            #endregion

            List<VariationAdjustment> projectVariationAdjustments = ProjectionHelpers.BuildProjectVariationAdjustments(VARIATIONS.Where(x => x.APPROVED != null).AsQueryable(), projections);

            //In progress distribution we want to generate cumulative data point to whatever date user set
            DateTime? extrapolateDate = null;
            if (extrapolateDateToDataDate)
                extrapolateDate = PROGRESS.DATA_DATE;

            List<BASELINE_ITEMProgress> progresses = projections.Select(x => new BASELINE_ITEMProgress(PROJECT, PROGRESS, x, projectVariationAdjustments, useReportDate, extrapolateDate)
            {
                Entity = x,
                Live_PROGRESS = PROGRESS,
                P6_Assignments = PopulateP6Assignment(x, PROJECT, P6_ASSIGNMENTS),
                P6TASKCollection = P6_TASKS,
                IsInternalNumberAlwaysEditable = internalNumberMode == DeliverableInternalNumberMode.AlwaysEditable,
                IsInternalNumberManualOnly = internalNumberMode == DeliverableInternalNumberMode.Manual
            }).ToList();

            dynamic progress_item_by_originalguid = PROGRESS_ITEMS.GroupBy(x => x.GUID_ORIBASEITEM).Select(group => new { OriginalGuid = group.Key, Progresses = group.ToList() });

            IEnumerable<DELIVERABLES_STATUS> deliverables_statuses = null;
            if (ProjectGuidForDeliverablesStatus == null)
                deliverables_statuses = DELIVERABLES_STATUSCollection;
            else if(DELIVERABLES_STATUSCollection != null)
                deliverables_statuses = DELIVERABLES_STATUSCollection.Where(x => x.GUID_PROJECT == ProjectGuidForDeliverablesStatus);

            //post processing
            foreach (BASELINE_ITEMProgress baseline_item_progress in progresses)
            {
                SetReportablePROGRESS_ITEM(baseline_item_progress, progress_item_by_originalguid);
                if (buildStats && !baseline_item_progress.Stats.Budgeted.StatsBuilt)
                    baseline_item_progress.BuildStats();

                if(deliverables_statuses != null)
                {
                    IEnumerable<DELIVERABLES_STATUS> deliverables_status_by_deliverable_type;
                    switch(baseline_item_progress.Entity.Entity.DELIVERABLE_TYPE)
                    {
                        case DeliverableType.Deliverable:
                            deliverables_status_by_deliverable_type = deliverables_statuses.Where(x => x.FOR_NCR);
                            break;
                        case DeliverableType.DeliverableICR:
                            deliverables_status_by_deliverable_type = deliverables_statuses.Where(x => x.FOR_DELIVERABLE);
                            break;
                        case DeliverableType.NonDeliverable:
                            deliverables_status_by_deliverable_type = deliverables_statuses.Where(x => x.FOR_NONDELIVERABLE);
                            break;
                        case DeliverableType.Task:
                            deliverables_status_by_deliverable_type = deliverables_statuses.Where(x => x.FOR_TASK);
                            break;
                        default:
                            deliverables_status_by_deliverable_type = new List<DELIVERABLES_STATUS>();
                            break;
                    }

                    IEnumerable<DELIVERABLES_STATUS> deliverables_status_by_document_type = deliverables_status_by_deliverable_type.Where(x => DSTATUS_DOCTYPECollection.Any(y => y.GUID_STATUS == x.GUID && y.GUID_DOCTYPE == baseline_item_progress.Entity.Entity.GUID_DOCTYPE));
                    baseline_item_progress.Entity.Entity.DeliverableStatusCollection = deliverables_status_by_document_type.OrderBy(x => x.AUTO_PERCENTAGE);
                }
            }

            return progresses.AsQueryable();
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
            IQueryable<ESTIMATE_ITEM> ESTIMATE_ITEMS, PROJECT PROJECT, PROGRESS PROGRESS, IEnumerable<PROGRESS_ITEM> PROGRESS_ITEMS, IEnumerable<STOCK_GROUP> STOCK_GROUPS, IEnumerable<STOCK_CODE> projectSTOCK_CODES, IEnumerable<RATE> projectRATES, bool useReportDate)
        {
            IEnumerable<PROGRESS_ITEM> arrPROGRESS_ITEMS = PROGRESS_ITEMS.ToArray();
            List<ReportablesDisplay> display_items = new List<ReportablesDisplay>();

            IEnumerable<ESTIMATE_ITEMProgress> estimation_direct_item_progresses =
                ESTIMATE_ITEMProjectionQueries.IDeliverable_Progress_Transformation(ESTIMATE_ITEMS, PROJECT, projectRATES, PROGRESS, PROGRESS_ITEMS, false, 
                                                                                                projectSTOCK_CODES,
                                                                                                STOCK_GROUPS).AsEnumerable();

            var estimation_direct_progress_by_stockgroupguid = estimation_direct_item_progresses.Where(x => x.Entity.Progress_Type != EstimateProgressType.Standalone)
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
                            DateTime reportDateToUse = useReportDate ? PROGRESS.REPORT_DATE != null ? (DateTime)PROGRESS.REPORT_DATE : PROGRESS.DATA_DATE : PROGRESS.DATA_DATE;
                            new_stock_group.Reportables = deliverables_ByStockGroupByAreaBySubArea.DeliverablesByStockGroupByAreaBySubArea;
                            new_stock_group.Entity.Deliverables = deliverables_ByStockGroupByAreaBySubArea.DeliverablesByStockGroupByAreaBySubArea.Select(x => x.Entity);
                            new_stock_group.SetReportingDataDate(reportDateToUse);
                            ReportablesDisplay newProgressDisplay = new ReportablesDisplay();
                            newProgressDisplay.ProgressItem = new DisplayQuantityReportableGroup(new_stock_group);
                            display_items.Add(newProgressDisplay);
                        }
                    }
                }
            }

            display_items.AddRange(estimation_direct_item_progresses.Where(x => x.Progress_Type == EstimateProgressType.Standalone).Select(x => new ReportablesDisplay() { ProgressItem = new DisplayQuantityReportable(x, false) }));

            return display_items.AsQueryable();
        }

        public static void SetReportablePROGRESS_ITEM(IReportable reportable, IEnumerable<dynamic> PROGRESS_ITEMSByOriginalGuid)
        {
            ICanSetProgresses setProgressesProjection = reportable as ICanSetProgresses;
            if (setProgressesProjection == null)
                return;

            List<dynamic> progressByOriginalGuid = PROGRESS_ITEMSByOriginalGuid.ToList();
            foreach (dynamic item in progressByOriginalGuid)
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
