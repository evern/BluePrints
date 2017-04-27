using BluePrints.Data.Helpers;
using EntityFramework.Functions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace BluePrints.Data
{
    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            AddInterceptor(new SoftDeleteInterceptor());
            AddInterceptor(new CreatedAndUpdatedDateInterceptor());
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
                var entityType = AddedDbEntries.First().Entity.GetType();
                var keyPropertyInfo = DataUtils.GetKeyPropertyInfo(entityType);

                if (keyPropertyInfo != null)
                    foreach (var dbEntry in AddedDbEntries)
                    {
                        var entryKeyMember = dbEntry.Property(keyPropertyInfo.Name);
                        if (entryKeyMember.CurrentValue.GetType() == typeof(Guid))
                        {
                            var entryKeyMemberValue = (Guid) entryKeyMember.CurrentValue;

                            //If key field already has generated guid it means that record should be modified, assuming redo operation have entity with null value in deleted field
                            //This will essentially undelete the record
                            if (entryKeyMemberValue != Guid.Empty)
                            {
                                dbEntry.State = EntityState.Modified;
                            }
                            //Generate a new guid if record have an empty guid key field
                            else
                            {
                                entryKeyMember.CurrentValue = Guid.NewGuid();
                                if (entityType.BaseType == typeof(BASELINE_ITEM) ||
                                    entityType.BaseType == typeof(ESTIMATION_DIRECT_ITEM) ||
                                    entityType.BaseType == typeof(ESTIMATION_INDIRECT_ITEM))
                                {
                                    var OGPropertyInfo = entityType.GetProperty("GUID_ORIGINAL");
                                    if (OGPropertyInfo.GetValue(dbEntry.Entity).ToString() == Guid.Empty.ToString())
                                        OGPropertyInfo.SetValue(dbEntry.Entity, entryKeyMember.CurrentValue);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
            }

            return base.SaveChanges();
        }

        public void Reload(object entity)
        {
            Entry(entity).Reload();
        }

        public const string dbo = nameof(dbo);
        [Function(FunctionType.StoredProcedure, nameof(GetDeliverablePlannedDataPoints), Schema = dbo)]
        public ObjectResult<StoredProcedure_PlannedDataPoint> GetDeliverablePlannedDataPoints(string p6BaselineProjectName, DateTime dataDate, Guid baseline_itemKey, Guid baseline_item_originalKey, Guid workpackKey, decimal totalUnits, decimal rate)
        {
            ObjectParameter p6BaselineProjectNameParameter = new ObjectParameter("P6_BASELINE_NAME", p6BaselineProjectName);
            ObjectParameter dataDateParameter = new ObjectParameter("DATA_DATE", dataDate);
            ObjectParameter baselineItemGuidParameter = new ObjectParameter("GUID_BASELINE_ITEM", baseline_itemKey);
            ObjectParameter originalGuidParameter = new ObjectParameter("GUID_ORIGINAL", baseline_item_originalKey);
            ObjectParameter workpackGuidParameter = new ObjectParameter("GUID_WORKPACK", workpackKey);
            ObjectParameter totalUnitsParameter = new ObjectParameter("TOTAL_UNITS", totalUnits);
            ObjectParameter rateParameter = new ObjectParameter("RATE", rate);

            ObjectParameter[] parameterArray = new ObjectParameter[] 
            {
                p6BaselineProjectNameParameter,
                baselineItemGuidParameter,
                originalGuidParameter,
                workpackGuidParameter,
                dataDateParameter,
                totalUnitsParameter,
                rateParameter
            };

            ObjectResult<StoredProcedure_PlannedDataPoint> result = null;

            result = this.ObjectContext().ExecuteFunction<StoredProcedure_PlannedDataPoint>(
                nameof(this.GetDeliverablePlannedDataPoints), parameterArray);

            return result;
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
}