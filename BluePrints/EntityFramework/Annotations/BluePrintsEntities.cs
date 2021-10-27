using BaseModel.DataModel.EntityFramework;
using BaseModel.Misc;
using BluePrints.Common;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.PrimeroData;
using EntityFramework.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            List<string> applicableContext = new List<string>();
            applicableContext.Add("BluePrints.Data.BluePrintsEntities");
            AddInterceptor(new SoftDeleteInterceptor("DELETED", "DELETEDBY", () => LoginCredentials.CurrentUserGuid, applicableContext));
            AddInterceptor(new CreatedAndUpdatedDateInterceptor("CREATED", "CREATEDBY", "UPDATED", "UPDATEDBY", () => LoginCredentials.CurrentUserGuid, applicableContext));
        }
    }

    public partial class BluePrintsEntities
    {
        /// <summary>
        /// Allow redo operation to undo deleted record
        /// If a record with generated GUID passes through it'll be formatted as a modified record
        /// recommended to explicitly set New Guid for appropriate entities -- http://msdn.microsoft.com/en-us/library/dd283139.aspx
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            var AddedDbEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            if (AddedDbEntries.Count() > 0)
            {
                foreach (var dbEntry in AddedDbEntries)
                {
                    Guid newGuid = Guid.NewGuid();
                    IGuidEntityKey guidKeyEntity = dbEntry.Entity as IGuidEntityKey;
                    if (guidKeyEntity != null)
                    {
                        //If key field already has generated guid it means that record should be modified, assuming redo operation have entity with null value in deleted field
                        //This will essentially undelete the record
                        if (guidKeyEntity.GUID != Guid.Empty)
                            dbEntry.State = EntityState.Modified;
                        else
                            guidKeyEntity.GUID = newGuid;
                    }

                    //copy guid to original guid because relationship on entities that have revision control will have guid regenerated on next revision
                    IOriginalGuidEntityKey originalGuidKeyEntity = dbEntry.Entity as IOriginalGuidEntityKey;
                    if(originalGuidKeyEntity != null && originalGuidKeyEntity.OriginalEntityKey == Guid.Empty)
                    {
                        originalGuidKeyEntity.SetOriginalEntityKey(newGuid);
                    }

                    IHaveCreatedDate iHaveCreatedDateProjectionEntity = dbEntry.Entity as IHaveCreatedDate;
                    if (iHaveCreatedDateProjectionEntity != null)
                    {
                        //workaround for created because Save() only sets the projection primary key, this is used for property redo where the interceptor only tampers with UPDATED and CREATED is left as null
                        if (iHaveCreatedDateProjectionEntity.EntityCreatedDate.Date.Year == 1)
                            iHaveCreatedDateProjectionEntity.EntityCreatedDate = DateTime.Now;
                    }
                }
            }

            return base.SaveChanges();
        }

        public void Reload(object entity)
        {
            Entry(entity).Reload();
        }

        public List<DataPoint> QueryDeliverablePlannedDataPoints(Guid deliverable_guid, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == true && x.IsLate == false && x.IsForecast == isForecast).ToList();
        }

        public List<DataPoint> QueryDeliverableRemainingDataPoints(Guid deliverable_guid, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == false && x.IsLate == false && x.IsForecast == isForecast).ToList();
        }

        public List<DataPoint> QueryDeliverablePlannedDataPointsByProject(string projectNumber, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == false && x.IsForecast == isForecast).ToList();
        }

        public List<DataPointsGroup> QueryDeliverablePlannedDataPointsGroupByProject(string projectNumber, bool isPlanned, bool isLate, bool isCurrent, bool isForecast = false, bool isVariationSeparated = false)
        {
            IEnumerable<DataPoint> dataPoints = this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsLate == isLate && x.IsCurrent == isCurrent && x.IsPlanned == isPlanned && x.IsForecast == isForecast);
            if(isVariationSeparated)
                return dataPoints.GroupBy(x => new { x.SubJobCode, x.DisciplineCode, x.CommodityCode, x.VariationCode }).Select(g => new DataPointsGroup(g.Key.SubJobCode, g.Key.DisciplineCode, g.Key.CommodityCode, g.Key.VariationCode, g)).ToList();
            else
                return dataPoints.GroupBy(x => new { x.SubJobCode, x.DisciplineCode, x.CommodityCode }).Select(g => new DataPointsGroup(g.Key.SubJobCode, g.Key.DisciplineCode, g.Key.CommodityCode, "", g)).ToList();

        }

        public List<DataPoint> QueryDeliverableCurrentDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == true & !x.IsForecast).ToList();
        }

        public List<DataPoint> QueryDeliverablePlannedLateDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == true && x.IsCurrent == false & !x.IsForecast).ToList();
        }

        public List<DataPoint> QueryDeliverableRemainingDataPointsByProject(string projectNumber, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == false && x.IsCurrent == false && x.IsForecast == isForecast).ToList();
        }

        public List<PROGRESS_ETC> QueryProjectProgressETC(string projectNumber)
        {
            return this.PROGRESS_ETC.Where(x => x.PROGRESS.PROJECT.NUMBER == projectNumber).Where(x => x.PROGRESS.STATUS == ProgressStatus.Live).ToList();
        }

        public List<DataPoint> QueryDeliverableRemainingLateDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == true && x.IsCurrent == false & !x.IsForecast).ToList();
        }
    }

    public static class BluePrintsContextHelper
    {
        public static void AsyncRefreshDeliverablesDataPointsByProject(string projectNumber)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.WhenAll(RefreshDeliverablesPlannedDataPointsByProject(projectNumber, false), RefreshDeliverablesRemainingDataPointsByProject(projectNumber, false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static async Task RefreshDeliverablesDataPointsByProject(string projectNumber)
        {
            await Task.WhenAll(RefreshDeliverablesPlannedDataPointsByProject(projectNumber, false), RefreshDeliverablesRemainingDataPointsByProject(projectNumber, false));
        }

        public static async Task RefreshAllDataPoints()
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshAllDataPoints");
                var i = await returnTask;
            }
        }

        public static async Task RefreshDeliverablesPlannedDataPointsByProject(string projectNumber, bool isForecast)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter isForecastParameter = new SqlParameter("@ISFORECAST", isForecast ? 1 : 0);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshDeliverablesPlannedDataPointsByProject @PROJECT_NUMBER, @ISFORECAST", projectNumberParameter, isForecastParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshDeliverablesRemainingDataPointsByProject(string projectNumber, bool isForecast)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter isForecastParameter = new SqlParameter("@ISFORECAST", isForecast ? 1 : 0);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshDeliverablesRemainingDataPointsByProject @PROJECT_NUMBER, @ISFORECAST", projectNumberParameter, isForecastParameter);
                var i = await returnTask;
            }
        }

        public static List<X_REPORTABLES> GetReportablesSummary(string projectNumber, bool isForecast)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter isForecastParameter = new SqlParameter("@IS_FORECAST", isForecast);
                List<X_REPORTABLES> reportableLines = dbContext.Database.SqlQuery<X_REPORTABLES>("X_REPORTABLES @PROJECT_NUMBER, @IS_FORECAST", projectNumberParameter, isForecastParameter).ToList();
                return reportableLines;
            }
        }

        public static List<X_EARNED_QUERY> GetEarnedSummary(string projectNumber, DateTime cutOffDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
                SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
                List<X_EARNED_QUERY> earnedData = dbContext.Database.SqlQuery<X_EARNED_QUERY>("X_EARNED @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
                return earnedData;
            }
        }

        public static List<X_WBS_GROUPED_DATAPOINT> GetWBSGroupedDataPointsSummary(string projectNumber, bool isPlanned, bool isLate, bool isCurrent, bool isForecast)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter isPlannedParameter = new SqlParameter("@IS_PLANNED", isPlanned);
                SqlParameter isLateParameter = new SqlParameter("@IS_LATE", isLate);
                SqlParameter isCurrentParameter = new SqlParameter("@IS_CURRENT", isCurrent);
                SqlParameter isForecastParameter = new SqlParameter("@IS_FORECAST", isForecast);
                List<X_WBS_GROUPED_DATAPOINT> wbsGroupedDataPoints = dbContext.Database.SqlQuery<X_WBS_GROUPED_DATAPOINT>("X_WBS_GROUPED_DATAPOINT @PROJECT_NUMBER, @IS_PLANNED, @IS_LATE, @IS_CURRENT, @IS_FORECAST", projectNumberParameter, isPlannedParameter, isLateParameter, isCurrentParameter, isForecastParameter).ToList();
                return wbsGroupedDataPoints;
            }
        }

        public static async void RefreshAllForecastData(string projectNumber, DateTime dataDate)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await Task.WhenAll(RefreshForecastBudgetByProject(projectNumber, dataDate),
                RefreshForecastActualsByProject(projectNumber, dataDate),
                RefreshForecastPOByProject(projectNumber, dataDate),
                RefreshForecastIndirectByProject(projectNumber, dataDate),
                RefreshEarnedByProject(projectNumber, dataDate),
                RefreshForecastP6ByProject(projectNumber, dataDate, true),
                RefreshForecastP6ByProject(projectNumber, dataDate, false)
                );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static async Task RefreshForecastBudgetByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastBudget @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshForecastActualsByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastActuals @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshForecastIndirectByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastIndirects @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshEarnedByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshEarned @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshForecastP6ByProject(string projectNumber, DateTime dataDate, bool isPlanned)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter isPlannedParameter = new SqlParameter("@IS_PLANNED", isPlanned);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastP6s @PROJECT_NUMBER, @DATA_DATE, @IS_PLANNED, @USER_GUID", projectNumberParameter, dataDateParameter, isPlannedParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshForecastPOByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastPOs @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshProgressETCByProject(string projectNumber, DateTime dataDate)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                SqlParameter dataDateParameter = new SqlParameter("@DATA_DATE", dataDate);
                SqlParameter userGuidParameter = new SqlParameter("@USER_GUID", LoginCredentials.CurrentUserGuid);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshForecastETC @PROJECT_NUMBER, @DATA_DATE, @USER_GUID", projectNumberParameter, dataDateParameter, userGuidParameter);
                var i = await returnTask;
            }
        }
    }
}