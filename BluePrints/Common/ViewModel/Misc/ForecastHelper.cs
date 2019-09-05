using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Misc
{
    public static class ForecastHelper
    {
        /// <summary>
        /// Creates the discipline job forecast and also commodity job forecast within
        /// </summary>
        /// <returns></returns>
        public static List<ForecastJobData> CreateCommodityProjections(IEnumerable<ExoSubJobProjection> unifiedJobList, IEnumerable<ExoTimeAuthorisation> queryJobLines, IEnumerable<DashboardFlatStructure> projectDashboards, IEnumerable<FORECAST> FORECASTCollection, IEnumerable<FORECAST_PO> FORECAST_POCollection, List<DateTime> dates, DateTime dataDate, bool isWeeks)
        {
            ConcurrentBag<ForecastJobData> forecastProjections = new ConcurrentBag<ForecastJobData>();
            var groupedDisciplineJobs = unifiedJobList.GroupBy(x => x.SubJob.Code + x.Discipline.Code + x.Variation_Code).Select(group => new { DisciplineJob = group.First(), CommodityJobs = group.ToList() });
            LoadingScreenManager.ShowLoadingScreen(groupedDisciplineJobs.Count());
            LoadingScreenManager.SetMessage("Summarizing Jobs Data...");

            Parallel.ForEach(groupedDisciplineJobs,
            groupedDisciplineJob =>
            {
                //retrive the discipline subjob, any member in the collection will do
                ExoSubJobProjection DisciplineJob = groupedDisciplineJob.DisciplineJob;

                //create the discipline level forecast summary
                List<DashboardFlatStructure> disciplineDashboards = projectDashboards.Where(x => x.SubjobCode == DisciplineJob.SubJob.Code && x.DisciplineCode == DisciplineJob.Discipline.Code && x.Variation_Code == DisciplineJob.Variation_Code).ToList();
                ConcurrentBag<ForecastJobData> commodityJobs = new ConcurrentBag<ForecastJobData>();

                Parallel.ForEach(groupedDisciplineJob.CommodityJobs,
                commodityJob =>
                {
                    ForecastJobData commodityJobForecastSummary = createJobForecastSummary(commodityJob.SubJob.Code, commodityJob.SubJob.Title, commodityJob.Discipline.Code, commodityJob.Discipline.Name, commodityJob.Commodity.Code, commodityJob.Commodity.Name, commodityJob.Commodity.Description, commodityJob.Commodity.UOM, commodityJob.Variation_Code, queryJobLines);
                    IEnumerable<DashboardFlatStructure> commodityDashboards = disciplineDashboards.Where(x => x.CommodityCode == commodityJob.Commodity.Code);
                    PopulateProjection(commodityJobForecastSummary, commodityDashboards, FORECAST_POCollection, dates, isWeeks, true);
                    //moved out of this routine so that EAC will be refreshed when refreshing the view, instead of it being populated only on load
                    //PopulateEAC(commodityJobForecastSummary, FORECASTCollection, dataDate);
                    commodityJobs.Add(commodityJobForecastSummary);
                });

                //for debugging
                //foreach (var commodityJob in groupedDisciplineJob.CommodityJobs)
                //{

                //}

                foreach (ForecastJobData commodityJob in commodityJobs)
                {
                    forecastProjections.Add(commodityJob);
                }

                LoadingScreenManager.Progress();
            });

            //for debugging
            //foreach (var groupedDisciplineJob in groupedDisciplineJobs)
            //{

            //}

            LoadingScreenManager.CloseLoadingScreen();
            return forecastProjections.ToList();
        }

        /// <summary>
        /// Populates data row with dashboards summary
        /// </summary>
        public static void PopulateProjection(ForecastJobData jobForecastSummary, IEnumerable<DashboardFlatStructure> DashboardCollection, IEnumerable<FORECAST_PO> FORECAST_POCollection, List<DateTime> dates, bool isWeeks, bool isDataFiltered)
        {
            ExoSubJobProjection entity = jobForecastSummary.Projection;
            List<DashboardFlatStructure> relevantDashboards;
            if (!isDataFiltered)
                relevantDashboards = DashboardCollection.Where(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code && x.Variation_Code == entity.Variation_Code).ToList();
            else
                relevantDashboards = DashboardCollection.ToList();

            jobForecastSummary.DateCosts.Clear();
            foreach (DateTime date in dates)
            {
                jobForecastSummary.DateCosts.Add(new ForecastDateCost(date, isWeeks));
            }

            if (relevantDashboards != null && relevantDashboards.Count() > 0)
            {
                IEnumerable<SummaryStats> summaryStats = relevantDashboards.Select(x => (SummaryStats)x.Stats);
                IEnumerable<SummaryStats> poStats = summaryStats.Where(x => x.PO != null && x.PO.DataPoints != null);

                List<FORECAST_PO> currentJobPOForecasts = new List<FORECAST_PO>();
                if (poStats != null && poStats.Count() > 0)
                {
                    IEnumerable<Common.ViewModel.Reporting.ExoDataPoint> poDataPoints = poStats.SelectMany(x => x.PO.ExoDataPoints);
                    jobForecastSummary.Outstanding = poDataPoints.Sum(x => x.Costs);

                    //group the pos into PO numbers group to get the total remaining cost
                    //costs is remaining cost in this case
                    var poItems = poDataPoints.GroupBy(x => new { x.PONumber, x.Subjob_Name, x.Discipline_Code, x.Commodity_Code, x.Variation_Code }).Select(g => new { g.Key.PONumber, g.Key.Subjob_Name, g.Key.Discipline_Code, g.Key.Commodity_Code, g.Key.Variation_Code }).ToList();
                    foreach(var poItem in poItems)
                    {
                        currentJobPOForecasts.AddRange(FORECAST_POCollection.Where(x => x.PONO == poItem.PONumber && x.JOB_CODE == poItem.Subjob_Name && x.DISCIPLINE_CODE == poItem.Discipline_Code && x.COMMODITY_CODE == poItem.Commodity_Code && x.VARIATION_CODE == poItem.Variation_Code));
                    }
                }

                //get remaining data points
                List<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = new List<Reporting.DataPoint>();
                List<Common.ViewModel.Reporting.DataPoint> budgetDataPoints = new List<Reporting.DataPoint>();
                IEnumerable<SummaryStats> remainingStats = summaryStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null);
                IEnumerable<SummaryStats> budgetedStats = summaryStats.Where(x => x.Budgeted != null && x.Budgeted.DataPoints != null);

                if (budgetedStats.Count() > 0)
                {
                    budgetDataPoints.AddRange(budgetedStats.SelectMany(x => x.Budgeted.DataPoints));
                    decimal p6BudgetedUnits = budgetDataPoints.Sum(x => x.Units);
                    jobForecastSummary.P6BudgetedUnits = p6BudgetedUnits;
                }

                if (remainingStats.Count() > 0)
                {
                    remainingDataPoints.AddRange(remainingStats.SelectMany(x => x.Remaining.DataPoints));
                    decimal p6RemainingCosts = remainingDataPoints.Sum(x => x.Costs);
                    decimal p6RemainingUnits = remainingDataPoints.Sum(x => x.Units);
                    jobForecastSummary.P6RemainingCosts = p6RemainingCosts;
                    jobForecastSummary.P6RemainingUnits = p6RemainingUnits;
                }


                //get actual data points and populate summary
                List<ExoDataPoint> actualDataPoints = new List<ExoDataPoint>();
                IEnumerable<SummaryStats> actualStats = summaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null);
                if (actualStats.Count() > 0)
                {
                    actualDataPoints.AddRange(actualStats.SelectMany(x => x.Actual.ExoDataPoints));
                    jobForecastSummary.ActualUnits = actualDataPoints.Sum(x => x.Units);
                    jobForecastSummary.ActualCosts = actualDataPoints.Sum(x => x.Costs);
                    jobForecastSummary.Invoiced = actualDataPoints.Sum(x => x.InvoiceAmount);
                }

                //get material data points and accrue summary
                List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
                IEnumerable<SummaryStats> materialStats = summaryStats.Where(x => x.Material != null && x.Material.DataPoints != null);
                if (materialStats != null && materialStats.Count() > 0)
                {
                    materialDataPoints.AddRange(materialStats.SelectMany(x => x.Material.ExoDataPoints));
                    jobForecastSummary.ActualCosts = materialDataPoints.Sum(x => x.Costs);
                    jobForecastSummary.Invoiced = materialDataPoints.Sum(x => x.InvoiceAmount);
                }

                DateTime firstViewDate = dates.First();
                DateTime firstForecastDate = dates.Count() > 1 ? dates[1] : dates.First();

                //the first remaining date will be the second month in the view because data date will end on the first month
                DateTime firstRemainingDate = new DateTime(dates.First().Year, dates.First().Month, 1).AddMonths(2).AddDays(-1);

                foreach (ForecastDateCost dateCost in jobForecastSummary.DateCosts)
                {
                    //override floor date to the beginning of time because we want to get everything
                    if (dateCost.Date == firstViewDate)
                        dateCost.ActualFloorDate = new DateTime(1);
                    else
                        dateCost.ActualFloorDate = dateCost.FloorDate;

                    decimal materialCosts = materialDataPoints.Where(x => x.ActualDate >= dateCost.ActualFloorDate && x.ActualDate <= dateCost.CeilingDate).Sum(x => x.Costs);
                    decimal actualCosts = actualDataPoints.Where(x => x.ActualDate >= dateCost.ActualFloorDate && x.ActualDate <= dateCost.CeilingDate).Sum(x => x.Costs);
                    decimal p6RemainingCosts = 0;
                    decimal p6RemainingHours = 0;
                    decimal poForecastCosts = 0;

                    //prevent population of values from PO forecast before forecast date
                    if (dateCost.FloorDate > firstViewDate)
                    {
                        poForecastCosts = currentJobPOForecasts.Where(x => x.FORECAST_DATE >= dateCost.FloorDate && x.FORECAST_DATE <= dateCost.CeilingDate).Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                    }

                    //prevet population of values from remaining before forecast date
                    if(dateCost.FloorDate > firstViewDate)
                    {
                        //accumulate hours and costs in the first forecast date
                        if (dateCost.CeilingDate == firstForecastDate)
                            dateCost.RemainingFloorDate = new DateTime(1);
                        else
                            dateCost.RemainingFloorDate = dateCost.FloorDate;

                        p6RemainingCosts = remainingDataPoints.Where(x => x.ProgressDate.Date >= dateCost.RemainingFloorDate && x.ProgressDate.Date <= dateCost.CeilingDate).Sum(x => x.Costs);
                        p6RemainingHours = remainingDataPoints.Where(x => x.ProgressDate.Date >= dateCost.RemainingFloorDate && x.ProgressDate.Date <= dateCost.CeilingDate).Sum(x => x.Units);
                    }

                    dateCost.MaterialCosts = Math.Round(materialCosts);
                    dateCost.ActualCosts = Math.Round(actualCosts);
                    dateCost.P6Costs = p6RemainingCosts;
                    dateCost.P6Hours = p6RemainingHours;
                    dateCost.POForecastCosts = Math.Round(poForecastCosts);

                    dateCost.TotalCosts = Math.Round(materialCosts + actualCosts + p6RemainingCosts + poForecastCosts);
                }
            }
        }

        /// <summary>
        /// Creates the forecast summary on discipline or commodity level
        /// </summary>
        private static ForecastJobData createJobForecastSummary(string subJobCode, string subJobTitle, string disciplineCode, string disciplineName, string commodityCode, string commodityName, string commodityDescription, string commodityUOM, string variationCode, IEnumerable<ExoTimeAuthorisation> jobLines)
        {
            ForecastJobData forecastProjection = ViewModelSource.Create(() => new ForecastJobData());
            forecastProjection.IsBudgetReadOnly = !LoginCredentials.hasPermission(PermissionResources.ChangeBudget);
            variationCode = NormalizeVariationCode(variationCode);
            forecastProjection.Projection = new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityName, Description = commodityDescription, UOM = commodityUOM }, Variation_Code = variationCode };

            //construct relevant exo lines collection
            IEnumerable<ExoTimeAuthorisation> relevantJobLines;

            //discipline lines processing
            if (commodityCode == string.Empty)
                relevantJobLines = jobLines.Where(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.VariationCode == variationCode);
            //commodity lines processing
            else
                relevantJobLines = jobLines.Where(x => x.SubJobCode == subJobCode && x.DisciplineCode == disciplineCode && x.CommodityCode == commodityCode && x.VariationCode == variationCode);

            forecastProjection.Projection.ExoBudgetQty = relevantJobLines.Sum(x => x.BudgetQty);
            forecastProjection.SetBudgetCost(relevantJobLines.Sum(x => x.BudgetCosts));
            forecastProjection.SetForecastRate(relevantJobLines.Sum(x => x.ForecastRate));

            return forecastProjection;
        }

        public static void PopulateEAC(ForecastJobData forecastProjection, IEnumerable<FORECAST> FORECASTCollection, DateTime dataDate)
        {
            //populate previous estimate to completion
            IEnumerable<FORECAST> previousEAC = FORECASTCollection.Where(x => x.SUBJOB_CODE == forecastProjection.Projection.SubJob.Code && x.DISCIPLINE_CODE == forecastProjection.Projection.Discipline.Code && x.COMMODITY_CODE == forecastProjection.Projection.Commodity.Code && x.VARIATION_CODE == forecastProjection.Projection.Variation_Code && x.FORECAST_TYPE == ForecastDataType.EAC && x.FORECAST_DATE < dataDate).OrderBy(x => x.FORECAST_DATE);
            if (previousEAC.Count() > 0)
            {
                FORECAST lastEAC = previousEAC.Last();
                if (lastEAC.FORECAST_UNITS != null)
                    forecastProjection.PreviousEAC = (decimal)lastEAC.FORECAST_UNITS;
            }
            else
            {
                forecastProjection.PreviousEAC = 0.00m;
            }
        }

        /// <summary>
        /// For the purpose of presentation, variation code must always be empty
        /// But when budget is edited, findExistingOrAddNewLine will handle the difference between null and string.empty values
        /// </summary>
        private static string NormalizeVariationCode(string variationCode)
        {
            if (variationCode == null)
                return string.Empty;

            return variationCode;
        }

        /// <summary>
        /// Creates a unified projection of all jobs queried and actuals from dashboards
        /// </summary>
        public static List<ExoSubJobProjection> ConstructUnifiedJobList(IEnumerable<ExoTimeAuthorisation> queriedJobs, IEnumerable<DashboardFlatStructure> dashboardJobs, IEnumerable<COMMODITY_CODE> COMMODITY_CODELookup, ref List<ExoDataPoint> allDataPoints)
        {
            ConcurrentBag<ExoSubJobProjection> combinedSubJobs = new ConcurrentBag<ExoSubJobProjection>();
            LoadingScreenManager.ShowLoadingScreen(queriedJobs.Count());
            LoadingScreenManager.SetMessage("Constructing Queried Jobs...");
            //assume queried jobs are already unique
            Parallel.ForEach(queriedJobs,
            queriedJob =>
            {
                //jobs from query are added as it is
                addExoSubJob(combinedSubJobs, queriedJob.SubJobCode, queriedJob.DisciplineCode, queriedJob.CommodityCode, queriedJob.VariationCode, COMMODITY_CODELookup, queriedJobs, queriedJob.SubJobTitle, queriedJob.DisciplineName, false);
                LoadingScreenManager.Progress();
            });
            LoadingScreenManager.CloseLoadingScreen();

            IEnumerable<Stats> actualStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> materialStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
            IEnumerable<Stats> poStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);
            //IEnumerable<Stats> remainingStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Remaining != null).Select(x => ((SummaryStats)x.Stats).Remaining);

            allDataPoints.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
            allDataPoints.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
            allDataPoints.AddRange(poStats.SelectMany(x => x.ExoDataPoints));

            ////List<string> uniqueExoJobsConcatNames = dashboardJobs.Select(x => x.SubjobCode + ";" + x.DisciplineCode + ";" + x.CommodityCode + ";" + x.Variation_Code).Distinct().ToList();
            List<string> uniqueExoJobsConcatNames = allDataPoints.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code + ";" + x.Variation_Code).Distinct().ToList();

            LoadingScreenManager.ShowLoadingScreen(uniqueExoJobsConcatNames.Count);
            LoadingScreenManager.SetMessage("Constructing Unique Jobs from Actuals...");
            Parallel.ForEach(uniqueExoJobsConcatNames,
            uniqueJobsConcatName =>
            {
                List<string> delimited = uniqueJobsConcatName.Split(';').ToList();
                string subjobCode = delimited[0];
                string disciplineCode = delimited[1];
                string commodityCode = delimited[2];
                string variationCode = delimited[3];

                //data points from exo requires lookup and is filtered by unique code string
                addExoSubJob(combinedSubJobs, subjobCode, disciplineCode, commodityCode, variationCode, COMMODITY_CODELookup, queriedJobs);
                LoadingScreenManager.Progress();
            });

            LoadingScreenManager.CloseLoadingScreen();
            return combinedSubJobs.ToList();
        }

        /// <summary>
        /// Add entries to job list and also provide lookup table for looking up additional meta data because it can be empty when invoked from exo actuals
        /// </summary>
        private static void addExoSubJob(ConcurrentBag<ExoSubJobProjection> combinedSubJobs, string subJobCode, string disciplineCode, string commodityCode, string variationCode, 
            IEnumerable<COMMODITY_CODE> COMMODITY_CODELookup, IEnumerable<ExoTimeAuthorisation> ExoJobLookup, 
            string subJobTitle = "", string disciplineName = "", bool tryHarderOnLookup = true)
        {
            COMMODITY_CODE findCOMMODITY_CODE = COMMODITY_CODELookup.FirstOrDefault(x => x.CODE == commodityCode);
            string commodityCodeName = string.Empty;
            string commodityCodeDescription = string.Empty;
            string commodityCodeUOM = string.Empty;
            if (findCOMMODITY_CODE != null)
            {
                commodityCodeName = findCOMMODITY_CODE.NAME;
                commodityCodeDescription = findCOMMODITY_CODE.DESCRIPTION;
                commodityCodeUOM = findCOMMODITY_CODE.UOM;
            }

            if(tryHarderOnLookup)
            {
                if (subJobTitle == string.Empty)
                {
                    ExoTimeAuthorisation findSubJobProjection = ExoJobLookup == null ? null : ExoJobLookup.FirstOrDefault(x => x.SubJobCode == subJobCode);
                    if (findSubJobProjection != null)
                        subJobTitle = findSubJobProjection.SubJobTitle;
                }

                if (disciplineName == string.Empty)
                {
                    ExoTimeAuthorisation findDisciplineProjection = ExoJobLookup == null ? null : ExoJobLookup.FirstOrDefault(x => x.DisciplineCode == disciplineCode);
                    if (findDisciplineProjection != null)
                        disciplineName = findDisciplineProjection.DisciplineName;
                }
            }

            if (variationCode == null || variationCode == string.Empty)
            {
                if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && (x.Variation_Code == null || x.Variation_Code == string.Empty)))
                {
                    combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = NormalizeVariationCode(variationCode) });
                }
            }
            else if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && x.Variation_Code == variationCode))
            {
                combinedSubJobs.Add(new ExoSubJobProjection() { SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = NormalizeVariationCode(variationCode) });
            }
        }
    }

    public class PODateRemainingCost
    {
        public string PONumber { get; set; }
        public DateTime ForecastDate { get; set; }
        public decimal ForecastRemainingCosts { get; set; }
    }
}
