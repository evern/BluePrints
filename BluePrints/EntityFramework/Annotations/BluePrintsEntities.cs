using BluePrints.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    }
}