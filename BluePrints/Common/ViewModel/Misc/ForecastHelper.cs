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
        public static List<ForecastJobData> CreateCommodityProjections(IEnumerable<ExoSubJobProjection> unifiedJobList, IEnumerable<ExoTimeAuthorisation> queryJobLines, IEnumerable<DashboardFlatStructure> projectDashboards, IEnumerable<FORECAST> FORECASTCollection, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<FORECAST_JOB> FORECAST_JOBCollection, IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, List<DateTime> dates, DateTime dataDate, bool isWeeks)
        {
            ConcurrentBag<ForecastJobData> forecastProjections = new ConcurrentBag<ForecastJobData>();
            var groupedDisciplineJobs = unifiedJobList.GroupBy(x => x.SubJob.Code + x.Discipline.Code + x.Variation_Code).Select(group => new { DisciplineJob = group.First(), CommodityJobs = group.ToList() });
            LoadingScreenManager.ShowLoadingScreen(groupedDisciplineJobs.Count());
            LoadingScreenManager.SetMessage("Summarizing Jobs Data...");

            foreach(var groupedDisciplineJob in groupedDisciplineJobs)
            {
                //retrive the discipline subjob, any member in the collection will do
                ExoSubJobProjection DisciplineJob = groupedDisciplineJob.DisciplineJob;

                //create the discipline level forecast summary
                List<DashboardFlatStructure> disciplineDashboards = projectDashboards.Where(x => x.SubjobCode == DisciplineJob.SubJob.Code && x.DisciplineCode == DisciplineJob.Discipline.Code && x.Variation_Code == DisciplineJob.Variation_Code).ToList();
                ConcurrentBag<ForecastJobData> commodityJobs = new ConcurrentBag<ForecastJobData>();

                //cannot use this anymore because of navigational property in FORECAST_JOB
                //Parallel.ForEach(groupedDisciplineJob.CommodityJobs,
                //commodityJob =>
                //{

                //});

                foreach (var commodityJob in groupedDisciplineJob.CommodityJobs)
                {
                    ForecastJobData commodityJobForecastSummary = createJobForecastSummary(commodityJob.SubJob.Code, commodityJob.SubJob.Title, commodityJob.Discipline.Code, commodityJob.Discipline.Name, commodityJob.Commodity.Code, commodityJob.Commodity.Name, commodityJob.Commodity.Description, commodityJob.Commodity.UOM, commodityJob.Variation_Code, queryJobLines, COMMODITY_CODECollection);
                    commodityJobForecastSummary.JobErrorMessage = commodityJob.ForecastErrorString;

                    IEnumerable<DashboardFlatStructure> commodityDashboards = disciplineDashboards.Where(x => x.CommodityCode == commodityJob.Commodity.Code);
                    PopulateProjection(commodityJobForecastSummary, commodityDashboards, FORECAST_POCollection, FORECAST_JOBCollection, FORECAST_JOB_SETTINGCollection, dates, isWeeks, true);
                    //moved out of this routine so that EAC will be refreshed when refreshing the view, instead of it being populated only on load
                    //PopulateEAC(commodityJobForecastSummary, FORECASTCollection, dataDate);
                    commodityJobs.Add(commodityJobForecastSummary);
                }

                foreach (ForecastJobData commodityJob in commodityJobs)
                {
                    forecastProjections.Add(commodityJob);
                }

                LoadingScreenManager.Progress();
            }

            LoadingScreenManager.CloseLoadingScreen();
            return forecastProjections.ToList();
        }

        /// <summary>
        /// Populates data row with dashboards summary
        /// </summary>
        public static void PopulateProjection(ForecastJobData jobForecastSummary, IEnumerable<DashboardFlatStructure> DashboardCollection, IEnumerable<FORECAST_PO> FORECAST_POCollection, IEnumerable<FORECAST_JOB> FORECAST_JOBCollection, IEnumerable<FORECAST_JOB_SETTING> FORECAST_JOB_SETTINGCollection, List<DateTime> dates, bool isWeeks, bool isDataFiltered)
        {
            ExoSubJobProjection projection = jobForecastSummary.Projection;
            List<DashboardFlatStructure> relevantDashboards;
            if (!isDataFiltered)
                relevantDashboards = DashboardCollection.Where(x => x.SubjobCode == projection.SubJob.Code && x.DisciplineCode == projection.Discipline.Code && x.CommodityCode == projection.Commodity.Code && x.Variation_Code == projection.Variation_Code).ToList();
            else
                relevantDashboards = DashboardCollection.ToList();

            jobForecastSummary.DateCosts.Clear();
            foreach (DateTime date in dates)
            {
                jobForecastSummary.DateCosts.Add(new ForecastDateCost(date, isWeeks));
            }

            IEnumerable<SummaryStats> summaryStats;

            if (relevantDashboards != null && relevantDashboards.Count() > 0)
                summaryStats = relevantDashboards.Select(x => (SummaryStats)x.Stats);
            else
                summaryStats = new List<SummaryStats>();

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

                jobForecastSummary.PORemainingCosts = currentJobPOForecasts.Where(x => x.FORECAST_VALUE != null).Sum(x => (decimal)x.FORECAST_VALUE);
            }

            //get remaining data points
            List<Common.ViewModel.Reporting.DataPoint> remainingDataPoints = new List<Reporting.DataPoint>();
            List<Common.ViewModel.Reporting.DataPoint> earnedDataPoints = new List<Reporting.DataPoint>();
            List<Common.ViewModel.Reporting.DataPoint> budgetDataPoints = new List<Reporting.DataPoint>();
            IEnumerable<SummaryStats> remainingStats = summaryStats.Where(x => x.Remaining != null && x.Remaining.RemainingOnlyDataPoints != null);
            IEnumerable<SummaryStats> earnedStats = summaryStats.Where(x => x.Earned != null && x.Earned.DataPoints != null);
            IEnumerable<SummaryStats> budgetedStats = summaryStats.Where(x => x.Budgeted != null && x.Budgeted.DataPoints != null);

            if (budgetedStats.Count() > 0)
            {
                budgetDataPoints.AddRange(budgetedStats.SelectMany(x => x.Budgeted.DataPoints));
                decimal p6BudgetedUnits = budgetDataPoints.Sum(x => x.Units);
                jobForecastSummary.P6BudgetedUnits = p6BudgetedUnits;

                //IEnumerable<VariationAdjustment> variationAdjustments = budgetedStats.SelectMany(x => x.VariationAdjustments);
                //decimal variationUnits = variationAdjustments.Sum(y => y.AdjustmentUnits);
                decimal totalUnits = budgetedStats.Sum(x => x.Budgeted.TotalUnits);
                jobForecastSummary.DeliverableUnits = totalUnits;
            }

            if (remainingStats.Count() > 0)
            {
                remainingDataPoints.AddRange(remainingStats.SelectMany(x => x.Remaining.RemainingOnlyDataPoints));
                earnedDataPoints.AddRange(earnedStats.SelectMany(x => x.Earned.DataPoints));
                decimal p6RemainingCosts = remainingDataPoints.Sum(x => x.Costs);
                decimal p6RemainingUnits = remainingDataPoints.Sum(x => x.Units);
                decimal earnedUnits = earnedDataPoints.Sum(x => x.Units);
                jobForecastSummary.P6RemainingCosts = p6RemainingCosts;
                jobForecastSummary.P6RemainingUnits = p6RemainingUnits;
                jobForecastSummary.EarnedUnits = earnedUnits;
            }

            jobForecastSummary.ActualCosts = 0;
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
                jobForecastSummary.ActualCosts += materialDataPoints.Sum(x => x.Costs);
                jobForecastSummary.Invoiced = materialDataPoints.Sum(x => x.InvoiceAmount);
            }

            DateTime firstViewDate = dates.First();
            DateTime firstForecastDate = dates.Count() > 1 ? dates[1] : dates.First();

            //the first remaining date will be the second month in the view because data date will end on the first month
            DateTime firstRemainingDate = new DateTime(dates.First().Year, dates.First().Month, 1).AddMonths(2).AddDays(-1);

            List<RemainingCost> weeklyForecastRemainingCosts = new List<RemainingCost>();
            List<FORECAST_JOB> relevantFORECAST_JOBS = FORECAST_JOBCollection.Where(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code).ToList();
            foreach(FORECAST_JOB relevantFORECAST_JOB in relevantFORECAST_JOBS.Where(x => x.FORECAST_RATE != null))
            {
                foreach(FORECAST_JOB_HOUR relevantFORECAST_JOB_HOUR in relevantFORECAST_JOB.FORECAST_JOB_HOUR.Where(x => x.FORECAST_HOUR != null))
                {
                    weeklyForecastRemainingCosts.Add(new RemainingCost() { ForecastDate = relevantFORECAST_JOB_HOUR.FORECAST_DATE, ForecastRemainingCosts = (decimal)relevantFORECAST_JOB.FORECAST_RATE * (decimal)relevantFORECAST_JOB_HOUR.FORECAST_HOUR });
                }
            }

            //set whether productivity is floating
            if (FORECAST_JOB_SETTINGCollection.Where(x => x.SUBJOB_CODE == projection.SubJob.Code && x.DISCIPLINE_CODE == projection.Discipline.Code && x.COMMODITY_CODE == projection.Commodity.Code && x.VARIATION_CODE == projection.Variation_Code && x.IS_FLOATING_PRODUCTIVITY).Count() > 0)
                jobForecastSummary.IsProductivityFloating = true;

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
                decimal weeklyForecastCosts = 0;

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
                    weeklyForecastCosts = weeklyForecastRemainingCosts.Where(x => x.ForecastDate.Date >= dateCost.FloorDate && x.ForecastDate.Date <= dateCost.CeilingDate).Sum(x => x.ForecastRemainingCosts);
                }

                dateCost.MaterialCosts = Math.Round(materialCosts);
                dateCost.ActualCosts = Math.Round(actualCosts);
                dateCost.P6Costs = p6RemainingCosts;
                dateCost.P6Hours = p6RemainingHours;
                dateCost.POForecastCosts = Math.Round(poForecastCosts);
                dateCost.WeeklyForecastCosts = weeklyForecastCosts;
                dateCost.TotalCosts = Math.Round(materialCosts + actualCosts + p6RemainingCosts + poForecastCosts + weeklyForecastCosts);
            }
        }

        /// <summary>
        /// Creates the forecast summary on discipline or commodity level
        /// </summary>
        private static ForecastJobData createJobForecastSummary(string subJobCode, string subJobTitle, string disciplineCode, string disciplineName, string commodityCode, string commodityName, string commodityDescription, string commodityUOM, string variationCode, IEnumerable<ExoTimeAuthorisation> jobLines, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection)
        {
            ForecastJobData forecastProjection = ViewModelSource.Create(() => new ForecastJobData());
            forecastProjection.PopulateCommodityCodes(COMMODITY_CODECollection);
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
            DateTime previousEACDataDate = new DateTime(dataDate.Year, dataDate.Month, 1);
            previousEACDataDate = previousEACDataDate.AddDays(-1);

            //populate previous estimate to completion
            FORECAST previousEAC = FORECASTCollection.FirstOrDefault(x => x.SUBJOB_CODE == forecastProjection.Projection.SubJob.Code && x.DISCIPLINE_CODE == forecastProjection.Projection.Discipline.Code && x.COMMODITY_CODE == forecastProjection.Projection.Commodity.Code && x.VARIATION_CODE == forecastProjection.Projection.Variation_Code && x.FORECAST_TYPE == ForecastDataType.EAC && x.FORECAST_DATE == previousEACDataDate);
            if (previousEAC != null)
            {
                if (previousEAC.FORECAST_UNITS != null)
                    forecastProjection.PreviousEAC = (decimal)previousEAC.FORECAST_UNITS;
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
        public static List<ExoSubJobProjection> ConstructUnifiedJobList(IEnumerable<ExoTimeAuthorisation> queriedJobs, IEnumerable<COMMODITY_CODE> COMMODITY_CODELookup, ref List<ExoDataPoint> allDataPoints, IEnumerable<DashboardFlatStructure> dashboardJobs = null)
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

            if(dashboardJobs != null)
            {
                IEnumerable<Stats> actualStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Actual != null).Select(x => ((SummaryStats)x.Stats).Actual);
                IEnumerable<Stats> materialStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).Material != null).Select(x => ((SummaryStats)x.Stats).Material);
                IEnumerable<Stats> poStats = dashboardJobs.Where(x => x.Stats != null && ((SummaryStats)x.Stats).PO != null).Select(x => ((SummaryStats)x.Stats).PO);

                allDataPoints.AddRange(actualStats.SelectMany(x => x.ExoDataPoints));
                allDataPoints.AddRange(materialStats.SelectMany(x => x.ExoDataPoints));
                allDataPoints.AddRange(poStats.SelectMany(x => x.ExoDataPoints));
            }

            List<string> dataPointsConcatNames = allDataPoints.Select(x => x.Subjob_Name + ";" + x.Discipline_Code + ";" + x.Commodity_Code + ";" + x.Variation_Code).ToList();
            List<string> dashboardConcatNames = new List<string>(); 
            foreach (DashboardFlatStructure dashboardJob in dashboardJobs)
            {
                decimal remainingUnits = dashboardJob.Stats.Remaining.DataPoints == null ? 0 : dashboardJob.Stats.Remaining.GetData().Where(x => x.IsRemaining).Sum(x => x.Units);
                //use more than 1 because of anomaly on duration based units which could amount up to 1
                if(remainingUnits > 1)
                    dashboardConcatNames.Add(dashboardJob.SubjobCode + ";" + dashboardJob.DisciplineCode + ";" + dashboardJob.CommodityCode + ";" + dashboardJob.Variation_Code);
            }

            List<string> allExoJobConcatNames = dataPointsConcatNames.ToList();
            allExoJobConcatNames.AddRange(dashboardConcatNames);
            List<string> uniqueExoJobsConcatNames = allExoJobConcatNames.Distinct().ToList();

            LoadingScreenManager.ShowLoadingScreen(uniqueExoJobsConcatNames.Count);
            LoadingScreenManager.SetMessage("Constructing Unique Jobs from Actuals...");
            Parallel.ForEach(uniqueExoJobsConcatNames,
            uniqueJobsConcatName =>
            {
                bool isExistInActuals = dataPointsConcatNames.Any(x => x == uniqueJobsConcatName);
                bool isExistInRemaining = dashboardConcatNames.Any(x => x == uniqueJobsConcatName);
                string possibleErrorMessage = string.Empty;
                if (isExistInActuals && isExistInRemaining)
                    possibleErrorMessage = "Job have actuals and remaining costs";
                else if (isExistInActuals)
                    possibleErrorMessage = "Job have actuals";
                else if (isExistInRemaining)
                    possibleErrorMessage = "Job have remaining costs";

                List<string> delimited = uniqueJobsConcatName.Split(';').ToList();
                string subjobCode = delimited[0];
                string disciplineCode = delimited[1];
                string commodityCode = delimited[2];
                string variationCode = delimited[3];
                
                //data points from exo requires lookup and is filtered by unique code string
                addExoSubJob(combinedSubJobs, subjobCode, disciplineCode, commodityCode, variationCode, COMMODITY_CODELookup, queriedJobs, "", "", true, possibleErrorMessage);
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
            string subJobTitle = "", string disciplineName = "", bool tryHarderOnLookup = true, string errorMessage = "")
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

            string possibleErrorMessage = errorMessage == string.Empty ? string.Empty : errorMessage + ", but isn't added in exo";
            if (variationCode == null || variationCode == string.Empty)
            {
                if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && (x.Variation_Code == null || x.Variation_Code == string.Empty)))
                {
                    combinedSubJobs.Add(new ExoSubJobProjection() { ForecastErrorString = possibleErrorMessage, SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = NormalizeVariationCode(variationCode) });
                }
            }
            else if (!combinedSubJobs.Any(x => x.SubJob.Code == subJobCode && x.Discipline.Code == disciplineCode && x.Commodity.Code == commodityCode && x.Variation_Code == variationCode))
            {
                combinedSubJobs.Add(new ExoSubJobProjection() { ForecastErrorString = possibleErrorMessage, SubJob = new PrimeroSubJob() { Code = subJobCode, Title = subJobTitle }, Discipline = new PrimeroDiscipline() { Code = disciplineCode, Name = disciplineName }, Commodity = new PrimeroCommodity() { Code = commodityCode, Name = commodityCodeName, Description = commodityCodeDescription, UOM = commodityCodeUOM }, Variation_Code = NormalizeVariationCode(variationCode) });
            }
        }
    }

    public class PODateRemainingCost : RemainingCost
    {
        public string PONumber { get; set; }
    }

    public class RemainingCost
    {
        public DateTime ForecastDate { get; set; }
        public decimal ForecastRemainingCosts { get; set; }
    }
}
