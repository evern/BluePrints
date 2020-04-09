using BaseModel.DataModel.EntityFramework;
using BaseModel.Misc;
using BluePrints.Common;
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

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPoints(Guid deliverable_guid, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == true && x.IsLate == false && x.IsForecast == isForecast).ToList()
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

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPoints(Guid deliverable_guid, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.Deliverable_Guid == deliverable_guid && x.IsPlanned == false && x.IsLate == false && x.IsForecast == isForecast).ToList()
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

        public List<StoredProcedure_PlannedDataPoint> QueryDeliverablePlannedDataPointsByProject(string projectNumber, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == false && x.IsForecast == isForecast).ToList()
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
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == false && x.IsCurrent == true & !x.IsForecast).ToList()
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
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == true && x.IsLate == true && x.IsCurrent == false & !x.IsForecast).ToList()
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

        public List<StoredProcedure_RemainingDataPoint> QueryDeliverableRemainingDataPointsByProject(string projectNumber, bool isForecast = false)
        {
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == false && x.IsCurrent == false && x.IsForecast == isForecast).ToList()
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
            return this.DataPoint.Where(x => x.ProjectNumber == projectNumber && x.IsPlanned == false && x.IsLate == true && x.IsCurrent == false & !x.IsForecast).ToList()
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
            public double PeriodPlannedQuantity { get; set; }
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
    }
}