using BluePrints.Data.Helpers;
using EntityFramework.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        [Function(FunctionType.StoredProcedure, nameof(GetDataPointsByProject), Schema = dbo)]
        public ObjectResult<StoredProcedure_DeliverablesDataPoints> GetDataPointsByProject(string ProjectNumber, bool TryUsingForecast)
        {
            ObjectParameter projectNumberParameter = new ObjectParameter(nameof(ProjectNumber), ProjectNumber);
            ObjectParameter tryUsingForecastParameter = new ObjectParameter(nameof(TryUsingForecast), TryUsingForecast);
            ObjectParameter[] parameterArray = new ObjectParameter[] { projectNumberParameter, tryUsingForecastParameter };

            return this.ObjectContext().ExecuteFunction<StoredProcedure_DeliverablesDataPoints>(
                nameof(this.GetDataPointsByProject), parameterArray);
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
    }
}