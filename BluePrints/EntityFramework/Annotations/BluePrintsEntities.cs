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
                        originalGuidKeyEntity.SetOriginalEntityKey(newGuid);
                    }
                }
            }

            return base.SaveChanges();
        }

        public void Reload(object entity)
        {
            Entry(entity).Reload();
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPoints(Guid deliverable_guid)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == true && x.IsLate == false).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPoints(Guid deliverable_guid)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == false && x.IsLate == false).ToList()
                .Select(x => new StoredProcedure_RemainingDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodRemainingPrice = x.PeriodPrice,
                    PeriodRemainingUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == false).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate, 
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverableCurrentDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == true).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedLateDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == true && x.IsCurrent == false).ToList()
                .Select(x => new StoredProcedure_PlannedDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodPlannedPrice = x.PeriodPrice,
                    PeriodPlannedUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == false && x.IsCurrent == false).ToList()
                .Select(x => new StoredProcedure_RemainingDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodRemainingPrice = x.PeriodPrice,
                    PeriodRemainingUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
        }

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingLateDataPointsByProject(string projectNumber)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == true && x.IsCurrent == false).ToList()
                .Select(x => new StoredProcedure_RemainingDataPoint()
                {
                    Deliverable_Guid = x.Deliverable_Guid,
                    IsFromP6 = x.IsFromP6,
                    Original_Guid = x.Original_Guid,
                    PeriodRemainingPrice = x.PeriodPrice,
                    PeriodRemainingUnits = x.PeriodUnits,
                    UniversalPeriodEndDate = x.UniversalPeriodEndDate,
                    UniversalPeriodStartDate = x.UniversalPeriodStartDate,
                    RemainingDuration = x.RemainingDuration
                }).ToList();
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
            public double? RemainingDuration { get; set; }
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
            public double? RemainingDuration { get; set; }
        }
    }

    public static class BluePrintsContextHelper
    {
        public static void AsyncRefreshDeliverablesDataPointsByProject(string projectNumber)
        {
            RefreshDeliverablesPlannedDataPointsByProject(projectNumber);
            RefreshDeliverablesRemainingDataPointsByProject(projectNumber);
        }

        public static async Task RefreshDeliverablesDataPointsByProject(string projectNumber)
        {
            await RefreshDeliverablesPlannedDataPointsByProject(projectNumber);
            await RefreshDeliverablesRemainingDataPointsByProject(projectNumber);
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

        public static async Task RefreshDeliverablesPlannedDataPointsByProject(string projectNumber)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                var projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshDeliverablesPlannedDataPointsByProject @PROJECT_NUMBER", projectNumberParameter);
                var i = await returnTask;
            }
        }

        public static async Task RefreshDeliverablesRemainingDataPointsByProject(string projectNumber)
        {
            using (BluePrintsEntities dbContext = new BluePrintsEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                var projectNumberParameter = new SqlParameter("@PROJECT_NUMBER", projectNumber);
                Task<int> returnTask = dbContext.Database.ExecuteSqlCommandAsync("RefreshDeliverablesRemainingDataPointsByProject @PROJECT_NUMBER", projectNumberParameter);
                var i = await returnTask;
            }
        }
    }
}