using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.NAME")]
    [RequiredAttributes("Entity.NAME")]
    public class ROLEProjection : BluePrintsProjectionBase<ROLE>, IGuidParentEntityKey, IHaveSortOrder, IHaveExpandState
    {
        public ObservableCollection<ROLE_PERMISSION> ROLE_PERMISSIONS { get; set; }

        //TreeView doesn't support nested property so have to expose it like this
        public Guid EntityGuid
        {
            get { return Entity.GUID; }
            set { Entity.GUID = value; }
        }

        public Guid? ParentEntityKey
        {
            get
            {
                return Entity.PARENTGUID;
            }
            set
            {
                if (value != null)
                    Entity.PARENTGUID = (Guid)value;
                else
                    Entity.PARENTGUID = Guid.Empty;
            }
        }

        public DateTime EntityCreatedDate
        {
            get
            {
                return Entity.CREATED;
            }
            set
            {
                Entity.CREATED = value;
            }
        }

        public int SortOrder
        {
            get
            {
                return Entity.SORTORDER;
            }
            set
            {
                Entity.SORTORDER = value;
            }
        }

        public int? OldSortOrder { get; set; }

        public bool IsExpanded
        {
            get
            {
                return Entity.ISEXPANDED;
            }
            set
            {
                Entity.ISEXPANDED = value;
            }
        }
    }

    public static class ROLEProjectionQueries
    {
        public static IQueryable<ROLEProjection> JoinROLE_PERMISSIONOnROLES(
            IQueryable<ROLE> ROLES, IEnumerable<ROLE_PERMISSION> AllROLE_PERMISSIONS)
        {
            return ROLES.ToArray().Select(x => new ROLEProjection()
            {
                EntityKey = x.GUID,
                Entity = x,
                ROLE_PERMISSIONS = new ObservableCollection<ROLE_PERMISSION>(AllROLE_PERMISSIONS.Where(z => z.GUID_ROLE == x.GUID))
            }).AsQueryable();
        }
    }
}
