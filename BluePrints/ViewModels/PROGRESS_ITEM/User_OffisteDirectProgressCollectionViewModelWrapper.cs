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

namespace BluePrints.ViewModels
{
    public class User_OffisteDirectProgressCollectionViewModelWrapper : OffsiteDirectProgressCollectionViewModelWrapper
    {
        private USER _loadUSER;
        IP6EntitiesUnitOfWork p6UOW;
        List<FullSummarizer> firstLoadProjectStatsSummarizers;
        protected override void InitializeParameters(object parameter)
        {
            var USERParameter = (EntitiesParameter<USER>)parameter;
            _loadUSER = USERParameter.GetEntity();
            p6UOW = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            firstLoadProjectStatsSummarizers = new List<FullSummarizer>();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROGRESS_ITEMS);
            loaderCollection.AddLoaderDescription<DELIVERABLES_STATUS, DELIVERABLES_STATUS, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DELIVERABLES_STATUSES);
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);

            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.STATUS == ProjectStatus.Active);
        }

        private Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.PROJECT.STATUS == ProjectStatus.Active);
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProgress>> ConstructMainViewModelProjection()
        {
            return query => ProgressQueries.User_OffsiteDirectProgressItemTransformation(query, _loadUSER);
        }

        protected override void onMainViewModelFirstLoaded(object sender, EventArgs e)
        {
            onMainViewModelFirstLoadedTimer.Stop();
        }

        //protected override void InitializeSummarizer()
        //{
        //    firstLoadProjectStatsSummarizers.Clear();
        //    var deliverablesGroupByProject = MainViewModel.Entities.GroupBy(x => x.Entity.Entity.BASELINE.PROJECT).Select(group => new { Project = group.Key, Deliverables = group.ToList() });
        //    foreach (var deliverableGroupByProject in deliverablesGroupByProject)
        //    {
        //        PROGRESS livePROGRESS = deliverableGroupByProject.Deliverables.First().Live_PROGRESS;
        //        BASELINE liveBASELINE = deliverableGroupByProject.Project.BASELINE.First(x => x.STATUS == BaselineStatus.Live);
        //        IEnumerable<WORKPACK> projectWORKPACKS = deliverableGroupByProject.Project.WORKPACK;
        //        TimeSpan reportInterval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(livePROGRESS);
        //        DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(livePROGRESS);
        //        IEnumerable<VARIATION> projectVARIATIONS = deliverableGroupByProject.Project.VARIATION.Where(x => x.APPROVED != null && x.TYPE == VariationType.External);

        //        List <VariationAdjustment> projectVariationAdjustment = ProjectionHelpers.BuildProjectVariationAdjustments(projectVARIATIONS.AsQueryable(), deliverableGroupByProject.Deliverables.Select(x => x.Entity));
        //        ProjectSummaryStats projectSummary = new ProjectSummaryStats(MainViewModel.Entities, livePROGRESS, projectVariationAdjustment);
        //        FullStatsBuilder fullStatsBuilder = new FullStatsBuilder(deliverableGroupByProject.Project, livePROGRESS, projectWORKPACKS);
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