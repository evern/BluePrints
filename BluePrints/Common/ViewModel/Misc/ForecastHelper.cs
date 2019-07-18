using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public static List<ForecastJobData> CreateCommodityProjections(IEnumerable<ExoSubJobProjection> unifiedJobList, IEnumerable<ExoTimeAuthorisation> queryJobLines, IEnumerable<DashboardFlatStructure> projectDashboards, IEnumerable<FORECAST> FORECASTCollection, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<DateTime> dates, DateTime dataDate)
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
                    populateProjection(commodityJobForecastSummary, commodityDashboards, FORECAST_POCollection, dates);
                    PopulateEAC(commodityJobForecastSummary, FORECASTCollection, dataDate);
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
        private static void populateProjection(ForecastJobData jobForecastSummary, IEnumerable<DashboardFlatStructure> relevantDashboards, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<DateTime> dates)
        {
            ExoSubJobProjection entity = jobForecastSummary.Projection;
            foreach(DateTime date in dates)
            {
                jobForecastSummary.DateCosts.Add(new ForecastDateCost(date));
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
                IEnumerable<SummaryStats> remainingStats = summaryStats.Where(x => x.Remaining != null && x.Remaining.DataPoints != null);
                if (remainingStats.Count() > 0)
                    remainingDataPoints.AddRange(remainingStats.SelectMany(x => x.Remaining.DataPoints));

                //get actual data points and populate summary
                List<ExoDataPoint> actualDataPoints = new List<ExoDataPoint>();
                IEnumerable<SummaryStats> actualStats = summaryStats.Where(x => x.Actual != null && x.Actual.DataPoints != null);
                if (actualStats.Count() > 0)
                {
                    actualDataPoints.AddRange(actualStats.SelectMany(x => x.Actual.ExoDataPoints));
                    jobForecastSummary.Actuals += actualDataPoints.Sum(x => x.Costs);
                    jobForecastSummary.Invoiced += actualDataPoints.Sum(x => x.InvoiceAmount);
                }

                //get material data points and accrue summary
                List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
                IEnumerable<SummaryStats> materialStats = summaryStats.Where(x => x.Material != null && x.Material.DataPoints != null);
                if (materialStats != null && materialStats.Count() > 0)
                {
                    materialDataPoints.AddRange(materialStats.SelectMany(x => x.Material.ExoDataPoints));
                    jobForecastSummary.Actuals += materialDataPoints.Sum(x => x.Costs);
                    jobForecastSummary.Invoiced += materialDataPoints.Sum(x => x.InvoiceAmount);
                }

                DateTime firstViewDate = dates.First();
                DateTime firstForecastDate = new DateTime(firstViewDate.Year, firstViewDate.Month, 1).AddMonths(2).AddDays(-1);

                //the first remaining date will be the second month in the view because data date will end on the first month
                DateTime firstRemainingDate = new DateTime(dates.First().Year, dates.First().Month, 1).AddMonths(2).AddDays(-1);

                foreach (ForecastDateCost dateCost in jobForecastSummary.DateCosts)
                {
                    DateTime cutOffActualFloorDate = new DateTime(dateCost.Date.Year, dateCost.Date.Month, 1);
                    DateTime cutOffRemainingFloorDate = new DateTime(dateCost.Date.Year, dateCost.Date.Month, 1);
                    //format cutOffCeilingDate to end of month
                    DateTime cutOffCeilingDate = cutOffActualFloorDate.AddMonths(1).AddDays(-1);

                    //override floor date to the beginning of time because we want to get everything
                    if (dateCost.Date == firstViewDate)
                        cutOffActualFloorDate = new DateTime(1);

                    if (materialDataPoints.Count() > 0 || actualDataPoints.Count > 0 || remainingDataPoints.Count() > 0 || currentJobPOForecasts.Count > 0)
                    {
                        decimal materialCosts = materialDataPoints.Where(x => x.ActualDate > cutOffActualFloorDate && x.ActualDate <= cutOffCeilingDate).Sum(x => x.Costs);
                        decimal actualCosts = actualDataPoints.Where(x => x.ActualDate > cutOffActualFloorDate && x.ActualDate <= cutOffCeilingDate).Sum(x => x.Costs);
                        decimal p6RemainingCosts = 0;
                        decimal p6RemainingHours = 0;
                        decimal poForecastCosts = 0;

                        //prevent population of values from PO forecast before forecast date
                        if(cutOffActualFloorDate > firstViewDate)
                        {
                            poForecastCosts = currentJobPOForecasts.Where(x => x.FORECAST_DATE > cutOffActualFloorDate && x.FORECAST_DATE <= cutOffCeilingDate).Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
                        }

                        //prevet population of values from remaining before forecast date
                        if(cutOffRemainingFloorDate > firstViewDate)
                        {
                            //accumulate hours and costs in the first forecast date
                            if (cutOffCeilingDate == firstForecastDate)
                                cutOffRemainingFloorDate = new DateTime(1);

                            p6RemainingCosts = remainingDataPoints.Where(x => x.ProgressDate > cutOffRemainingFloorDate && x.ProgressDate <= cutOffCeilingDate).Sum(x => x.Costs);
                            p6RemainingHours = remainingDataPoints.Where(x => x.ProgressDate > cutOffRemainingFloorDate && x.ProgressDate <= cutOffCeilingDate).Sum(x => x.Units);
                        }

                        dateCost.MaterialCosts = Math.Round(materialCosts);
                        dateCost.ActualCosts = Math.Round(actualCosts);
                        dateCost.P6Costs = Math.Round(p6RemainingCosts);
                        dateCost.P6Hours = Math.Round(p6RemainingHours);
                        dateCost.POForecastCosts = Math.Round(poForecastCosts);

                        dateCost.TotalCosts = Math.Round(materialCosts + actualCosts + p6RemainingCosts + poForecastCosts);
                    }
                    else
                        dateCost.TotalCosts = 0.00m;
                }
            }
        }

        /// <summary>
        /// Creates the forecast summary on discipline or commodity level
        /// </summary>
        private static ForecastJobData createJobForecastSummary(string subJobCode, string subJobTitle, string disciplineCode, string disciplineName, string commodityCode, string commodityName, string commodityDescription, string commodityUOM, string variationCode, IEnumerable<ExoTimeAuthorisation> jobLines)
        {
            ForecastJobData forecastProjection = new ForecastJobData();
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

        private static void PopulateEAC(ForecastJobData forecastProjection, IEnumerable<FORECAST> FORECASTCollection, DateTime dataDate)
        {
            //populate previous estimate to completion
            IEnumerable<FORECAST> previousEAC = FORECASTCollection.Where(x => x.SUBJOB_CODE == forecastProjection.Projection.SubJob.Code && x.DISCIPLINE_CODE == forecastProjection.Projection.Discipline.Code && x.COMMODITY_CODE == forecastProjection.Projection.Commodity.Code && x.VARIATION_CODE == forecastProjection.Projection.Variation_Code && x.IS_EAC && x.FORECAST_DATE < dataDate).OrderBy(x => x.FORECAST_DATE);
            if (previousEAC.Count() > 0)
            {
                FORECAST lastEAC = previousEAC.Last();
                if (lastEAC.FORECAST_UNITS != null)
                    forecastProjection.PreviousEAC = (decimal)lastEAC.FORECAST_UNITS;
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
        public static List<ExoSubJobProjection> ConstructUnifiedJobList(IEnumerable<ExoSubJobProjection> queriedJobs, IEnumerable<DashboardFlatStructure> dashboardJobs, IEnumerable<COMMODITY_CODE> COMMODITY_CODELookup, ref List<ExoDataPoint> allDataPoints)
        {
            ConcurrentBag<ExoSubJobProjection> combinedSubJobs = new ConcurrentBag<ExoSubJobProjection>();
            LoadingScreenManager.ShowLoadingScreen(queriedJobs.Count());
            LoadingScreenManager.SetMessage("Constructing Queried Jobs...");
            //assume queried jobs are already unique
            Parallel.ForEach(queriedJobs,
            queriedJob =>
            {   
                //jobs from query are added as it is
                addExoSubJob(combinedSubJobs, queriedJob.SubJob.Code, queriedJob.Discipline.Code, queriedJob.Commodity.Code, queriedJob.Variation_Code, COMMODITY_CODELookup, queriedJobs, queriedJob.SubJob.Title, queriedJob.Discipline.Name, false);
                LoadingScreenManager.Progress();
            });

            IEnumerable<Stats> actualStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
            IEnumerable<Stats> materialStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
            IEnumerable<Stats> poStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

            LoadingScreenManager.CloseLoadingScreen();
            LoadingScreenManager.ShowLoadingScreen(1);
            LoadingScreenManager.SetMessage("Constructing Unique Jobs from Actuals...");
            allDataPoints.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
            allDataPoints.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
            allDataPoints.AddRange(poStats.SelectMany(x => x.ExoDataPoints));

            List<string> uniqueJobsConcatNames = allDataPoints.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code + ";" + x.Variation_Code).Distinct().ToList();
            LoadingScreenManager.CloseLoadingScreen();

            LoadingScreenManager.ShowLoadingScreen(uniqueJobsConcatNames.Count);
            LoadingScreenManager.SetMessage("Constructing Unique Jobs from Actuals...");
            Parallel.ForEach(uniqueJobsConcatNames,
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

            int count = combinedSubJobs.Where(x => x.SubJob.Code == "03608-000-00-P1" && x.Discipline.Code == "ME01" && x.Commodity.Code == "M99").Count();

            LoadingScreenManager.CloseLoadingScreen();
            return combinedSubJobs.ToList();
        }

        /// <summary>
        /// Add entries to job list and also provide lookup table for looking up additional meta data because it can be empty when invoked from exo actuals
        /// </summary>
        private static void addExoSubJob(ConcurrentBag<ExoSubJobProjection> combinedSubJobs, string subJobCode, string disciplineCode, string commodityCode, string variationCode, 
            IEnumerable<COMMODITY_CODE> COMMODITY_CODELookup, IEnumerable<ExoSubJobProjection> ExoJobLookup, 
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
                    ExoSubJobProjection findSubJobProjection = ExoJobLookup == null ? null : ExoJobLookup.FirstOrDefault(x => x.SubJob.Code == subJobCode);
                    if (findSubJobProjection != null)
                        subJobTitle = findSubJobProjection.SubJob.Title;
                }

                if (disciplineName == string.Empty)
                {
                    ExoSubJobProjection findDisciplineProjection = ExoJobLookup == null ? null : ExoJobLookup.FirstOrDefault(x => x.Discipline.Code == disciplineCode);
                    if (findDisciplineProjection != null)
                        disciplineName = findDisciplineProjection.Discipline.Name;
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
