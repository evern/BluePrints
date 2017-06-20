using BaseModel.DataModel.EntityFramework;
using BaseModel.Misc;
using BluePrints.Common;
using EntityFramework.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            List<string> applicableContext = new List<string>();
            applicableContext.Add("BluePrints.Data.BluePrintsEntities");
            AddInterceptor(new SoftDeleteInterceptor("DELETED", "DELETEDBY"));
            AddInterceptor(new CreatedAndUpdatedDateInterceptor("CREATED", "CREATEDBY", "UPDATED", "UPDATEDBY", () => LoginCredentials.CurrentUserGuid, applicableContext));
        }
    }

    public partial class BluePrintsEntities : DbContext
    {
        /// <summary>
        /// Allow redo operation to undo deleted record
        /// If a record with generated GUID passes through it'll be formatted as a modified record
        /// recommended to explicitly set New Guid for appropriate entities -- http://msdn.microsoft.com/en-us/library/dd283139.aspx
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            var AddedDbEntries =
                ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
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
                        if (guidKeyEntity.EntityKey != Guid.Empty)
                            dbEntry.State = EntityState.Modified;
                        else
                            guidKeyEntity.EntityKey = newGuid;
                    }

                    IOriginalGuidEntityKey originalGuidKeyEntity = dbEntry.Entity as IOriginalGuidEntityKey;
                    if(originalGuidKeyEntity != null && originalGuidKeyEntity.OriginalEntityKey == Guid.Empty)
                    {
                        originalGuidKeyEntity.OriginalEntityKey = newGuid;
                    }
                }
            }

            return base.SaveChanges();
        }

        public void Reload(object entity)
        {
            Entry(entity).Reload();
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPoints(Guid baselineItemGuid)
        {
            return this.DataPoint.Where(x => x.Guid_DataPoint == baselineItemGuid && x.IsPlanned == true).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate
                }).ToList();
        }

        public const string dbo = nameof(dbo);
        [Function(FunctionType.StoredProcedure, nameof(GetDeliverablePlannedDataPoints), Schema = dbo)]
        public ObjectResult<StoredProcedure_PlannedDataPoint> GetDeliverablePlannedDataPoints(string p6BaselineProjectName, string p6ProgressProjectName, DateTime dataDate, Guid baseline_itemKey, Guid baseline_item_originalKey, Guid workpackKey, decimal totalUnits, decimal rate, bool isByDuration)
        {
            ObjectParameter p6BaselineProjectNameParameter = new ObjectParameter("P6_BASELINE_NAME", p6BaselineProjectName);
            ObjectParameter p6ProgressProjectNameParameter = new ObjectParameter("P6_PROGRESS_NAME", p6ProgressProjectName);
            ObjectParameter dataDateParameter = new ObjectParameter("DATA_DATE", dataDate);
            ObjectParameter baselineItemGuidParameter = new ObjectParameter("GUID_BASELINE_ITEM", baseline_itemKey);
            ObjectParameter originalGuidParameter = new ObjectParameter("GUID_ORIGINAL", baseline_item_originalKey);
            ObjectParameter workpackGuidParameter = new ObjectParameter("GUID_WORKPACK", workpackKey);
            ObjectParameter totalUnitsParameter = new ObjectParameter("TOTAL_UNITS", totalUnits);
            ObjectParameter rateParameter = new ObjectParameter("RATE", rate);
            ObjectParameter byDurationParameter = new ObjectParameter("BY_DURATION", isByDuration);

            ObjectParameter[] parameterArray = new ObjectParameter[] 
            {
                p6BaselineProjectNameParameter,
                p6ProgressProjectNameParameter, 
                baselineItemGuidParameter,
                originalGuidParameter,
                workpackGuidParameter,
                dataDateParameter,
                totalUnitsParameter,
                rateParameter,
                byDurationParameter
            };

            ObjectResult<StoredProcedure_PlannedDataPoint> result = null;

            result = this.ObjectContext().ExecuteFunction<StoredProcedure_PlannedDataPoint>(
                nameof(this.GetDeliverablePlannedDataPoints), parameterArray);

            return result;
        }

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPoints(Guid baselineItemGuid)
        {
            return this.DataPoint.Where(x => x.Guid_DataPoint == baselineItemGuid && x.IsPlanned == true).ToList()
                .Select(x => new StoredProcedure_RemainingDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodRemainingPrice = x.PeriodPrice,
                    PeriodRemainingUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate
                }).ToList();
        }

        [Function(FunctionType.StoredProcedure, nameof(GetDeliverableRemainingDataPoints), Schema = dbo)]
        public ObjectResult<StoredProcedure_RemainingDataPoint> GetDeliverableRemainingDataPoints(string p6BaselineProjectName, DateTime dataDate, Guid baseline_itemKey, Guid baseline_item_originalKey, Guid workpackKey, decimal totalUnits, decimal totalEarnedUnits, decimal rate)
        {
            ObjectResult<StoredProcedure_RemainingDataPoint> result = null;
            if (totalUnits == 0)
                return result;

            ObjectParameter p6ProgressProjectNameParameter = new ObjectParameter("P6_PROGRESS_NAME", p6BaselineProjectName);
            ObjectParameter dataDateParameter = new ObjectParameter("DATA_DATE", dataDate);
            ObjectParameter baselineItemGuidParameter = new ObjectParameter("GUID_BASELINE_ITEM", baseline_itemKey);
            ObjectParameter originalGuidParameter = new ObjectParameter("GUID_ORIGINAL", baseline_item_originalKey);
            ObjectParameter workpackGuidParameter = new ObjectParameter("GUID_WORKPACK", workpackKey);
            ObjectParameter totalUnitsParameter = new ObjectParameter("TOTAL_UNITS", totalUnits);
            ObjectParameter totalEarnedUnitsParameter = new ObjectParameter("TOTAL_EARNED_UNITS", totalEarnedUnits);
            ObjectParameter rateParameter = new ObjectParameter("RATE", rate);

            ObjectParameter[] parameterArray = new ObjectParameter[]
            {
                p6ProgressProjectNameParameter,
                baselineItemGuidParameter,
                originalGuidParameter,
                workpackGuidParameter,
                dataDateParameter,
                totalUnitsParameter,
                totalEarnedUnitsParameter, 
                rateParameter
            };

            result = this.ObjectContext().ExecuteFunction<StoredProcedure_RemainingDataPoint>(
                nameof(this.GetDeliverableRemainingDataPoints), parameterArray);

            return result;
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate
                }).ToList();
        }

        [Function(FunctionType.StoredProcedure, nameof(GetDeliverablesPlannedDataPointsByProject), Schema = dbo)]
        public ObjectResult<StoredProcedure_PlannedDataPoint> GetDeliverablesPlannedDataPointsByProject(string projectNumber)
        {
            ObjectParameter projectNumberParameter = new ObjectParameter("PROJECT_NUMBER", projectNumber);
            ObjectParameter[] parameterArray = new ObjectParameter[]
            {
                projectNumberParameter
            };

            ObjectResult<StoredProcedure_PlannedDataPoint> result = null;
            try
            {
                result = this.ObjectContext().ExecuteFunction<StoredProcedure_PlannedDataPoint>(
                nameof(this.GetDeliverablesPlannedDataPointsByProject), parameterArray);
            }
            catch(Exception e)
            {
                string s = e.ToString();
            }

            return result;
        }

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false).ToList()
                .Select(x => new StoredProcedure_RemainingDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodRemainingPrice = x.PeriodPrice,
                    PeriodRemainingUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate
                }).ToList();
        }

        [Function(FunctionType.StoredProcedure, nameof(GetDeliverablesRemainingDataPointsByProject), Schema = dbo)]
        public ObjectResult<StoredProcedure_RemainingDataPoint> GetDeliverablesRemainingDataPointsByProject(string projectNumber)
        {
            ObjectParameter projectNumberParameter = new ObjectParameter("PROJECT_NUMBER", projectNumber);
            ObjectParameter[] parameterArray = new ObjectParameter[]
            {
                projectNumberParameter
            };

            ObjectResult<StoredProcedure_RemainingDataPoint> result = null;

            try
            {
                result = this.ObjectContext().ExecuteFunction<StoredProcedure_RemainingDataPoint>(
                nameof(this.GetDeliverablesRemainingDataPointsByProject), parameterArray);
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }

            return result;
        }



        [ComplexType]
        public class StoredProcedure_DeliverablesDataPoints
        {
            public Guid GUID_PROJECT { get; set; }
            public Guid GUID_WORKPACK { get; set; }
            public Guid GUID_ORIGINAL { get; set; }
            public DateTime UniversalPeriodStartDate { get; set; }
            public DateTime UniversalPeriodEndDate { get; set; }
            public double PeriodPlannedUnits { get; set; }
            public double PeriodEarnedUnits { get; set; }
            public double PeriodRemainingUnits { get; set; }
            public double PeriodPlannedPrice { get; set; }
            public double PeriodEarnedPrice { get; set; }
            public double PeriodRemainingPrice { get; set; }
        }

        [ComplexType]
        public class StoredProcedure_PlannedDataPoint
        {
            public Guid Deliverable_Guid { get; set; }
            public Guid Original_Guid { get; set; }
            public DateTime UniversalPeriodStartDate { get; set; }
            public DateTime UniversalPeriodEndDate { get; set; }
            public double PeriodPlannedUnits { get; set; }
            public double PeriodPlannedPrice { get; set; }
            public bool IsFromP6 { get; set; }
        }

        [ComplexType]
        public class StoredProcedure_RemainingDataPoint
        {
            public Guid Deliverable_Guid { get; set; }
            public Guid Original_Guid { get; set; }
            public DateTime UniversalPeriodStartDate { get; set; }
            public DateTime UniversalPeriodEndDate { get; set; }
            public double PeriodRemainingUnits { get; set; }
            public double PeriodRemainingPrice { get; set; }
            public bool IsFromP6 { get; set; }
        }
    }

    public static class BluePrintsContextHelper
    {
        public static async Task RefreshDeliverablesDataPointsByProject(string projectNumber)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                var projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshDeliverablesDataPointsByProject @PROJECT_NUMBER", projectNumberParameter);
                var i = await returnTask;
            }
        }
    }
}