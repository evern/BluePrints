using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Projections;
using BaseModel.Misc;
using System.ComponentModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Data.Filtering;
using BaseModel.Data.Helpers;
using DevExpress.Xpf.Grid;
using BluePrints.Common.ViewModel.Utils;

namespace BluePrints.ViewModels
{
    public class User_OffsiteDirectProgressCollectionViewModelWrapper : OffsiteDirectProgressCollectionViewModelWrapper
    {
        private USER _loadUSER;
        protected IP6EntitiesUnitOfWork p6UOW;
        protected List<FullSummarizer> firstLoadProjectStatsSummarizers;

        protected override void resolveParameters(object parameter)
        {
            is_single_project_mode = false;
            var USERParameter = (EntitiesParameter<USER>)parameter;
            _loadUSER = LoginCredentials.CurrentUser;
            p6UOW = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            firstLoadProjectStatsSummarizers = new List<FullSummarizer>();
        }

        protected override void addEntitiesLoader()
        {
            base.addEntitiesLoader();
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEM_WORKS, BASELINE_ITEM_WORKProjectionFunc);
        }

        private Func<IRepositoryQuery<BASELINE_ITEM_WORK>, IQueryable<BASELINE_ITEM_WORK>> BASELINE_ITEM_WORKProjectionFunc()
        {
            return query => query;
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> specifyMainViewModelProjection()
        {
            return query => ProgressQueries.User_OffsiteDirectProgressItemTransformation(query, PROGRESS_ITEMCollection, USERCollection, BASELINE_ITEM_WORKCollection, _loadUSER, true, false, DELIVERABLES_STATUSCollection, DSTATUS_DOCTYPECollection);
        }

        protected override void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            //due to heavy query through navigational properties, always skip messages for both deliverable and progress
            PROGRESS_ITEMSCollectionViewModel.AlwaysSkipMessage = true;
            MainViewModel.AlwaysSkipMessage = true;

            MainViewModel.FuncManualCellPastingIsContinue = BluePrintsDataUtils.FuncManualCellPastingIsContinue;
            HideCompleted = true;
            this.RaisePropertyChanged(x => x.HideCompleted);
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        public IEnumerable<BASELINE_ITEM_WORK> BASELINE_ITEM_WORKCollection
        {
            get
            {
                return GetEntities<BASELINE_ITEM_WORK>();
            }
        }

        bool hideCompleted;
        public bool HideCompleted
        {
            get
            {
                return hideCompleted;
            }
            set
            {
                hideCompleted = value;
                if (GridControlService != null)
                {
                    if (value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " And [Total_Earned_Percentage] <> 1.00000m";
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse("[Total_Earned_Percentage] <> 1.00000m");
                        }

                        GridControlService.SetFilterCriteria(newCriteriaOperator);
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.Replace("And [Total_Earned_Percentage] <> 1.00000m", "");
                            newfilterCriteria = newfilterCriteria.Replace("[Total_Earned_Percentage] <> 1.00000m", "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if (firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.SetFilterCriteria(newCriteriaOperator);
                        }
                    }
                }
            }
        }

        //protected override void InitializeSummarizer()
        //{
        //    firstLoadProjectStatsSummarizers.Clear();
        //    var deliverablesGroupByProject = MainViewModel.Entities.GroupBy(x => x.Entity.Entity.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
        //    foreach (var deliverableGroupByProject in deliverablesGroupByProject)
        //    {
        //        PROGRESS livePROGRESS = deliverableGroupByProject.Deliverables.First().Live_PROGRESS;
        //        BASELINE liveBASELINE = deliverableGroupByProject.Project.BASELINE.First(x => x.STATUS == BaselineStatus.Live);
        //        IEnumerable<SUBJOB> projectSUBJOBS = deliverableGroupByProject.Project.SUBJOB;
        //        TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(livePROGRESS);
        //        DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(livePROGRESS);
        //        IEnumerable<VARIATION> projectVARIATIONS = deliverableGroupByProject.Project.VARIATION.Where(x => x.APPROVED != null && x.TYPE == VariationType.External);

        //        List <VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(projectVARIATIONS.AsQueryable(), deliverableGroupByProject.Deliverables.Select(x => x.Entity));
        //        ProjectSummaryStats projectSummary = new ProjectSummaryStats(MainViewModel.Entities, livePROGRESS, projectVariationAdjustment);
        //        FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(deliverableGroupByProject.Project, livePROGRESS, projectSUBJOBS);
        //        FullSummarizer fullSummarizer = new FullSummarizer(projectSummary, fullStatsBuilder, deliverableGroupByProject.Project.NUMBER);
        //        firstLoadProjectStatsSummarizers.Add(fullSummarizer);
        //    }
        //}

        //protected override void BackgroundWorkerBuildStats()
        //{
        //    foreach(FullSummarizer fullSummarizer in firstLoadProjectStatsSummarizers)
        //    {
        //        fullSummarizer.BuildBudgetedOnly();
        //        fullSummarizer.BuildEarnedAndRemaining();
        //    }
        //}
    }
}