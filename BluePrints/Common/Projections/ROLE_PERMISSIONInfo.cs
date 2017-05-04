using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BaseModel.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BluePrints.Data
{
    public class ROLE_PERMISSIONInfo : IGuidEntityKey
    {
        public ROLE_PERMISSIONInfo()
        {

        }
        
        public ROLE_PERMISSIONInfo(ROLE_PERMISSION systemPERMISSION,
            IEnumerable<ROLE_PERMISSION> currentAssignedROLE_PERMISSIONS)
        {
            var currentROLE_PERMISSION =
                currentAssignedROLE_PERMISSIONS.FirstOrDefault(x => x.PERMISSION == systemPERMISSION.PERMISSION);
            if (currentROLE_PERMISSION != null)
            {
                DataUtils.ShallowCopy(this, currentROLE_PERMISSION);
                ASSIGNED = true;
            }
            else
            {
                PERMISSION = systemPERMISSION.PERMISSION;
            }
        }

        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid GUID { get; set; }

        public Guid GUID_ROLE { get; set; }

        [Required]
        [StringLength(50)]
        public string PERMISSION { get; set; }

        [ProjectionPropertyAttribute]
        public bool ASSIGNED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual ROLE ROLE { get; set; }
    }

    public static class ROLE_PERMISSIONProjectionQueries
    {
        public static IQueryable<ROLE_PERMISSIONInfo> GetAssignedROLE_PERMISSIONByROLE(
            IQueryable<ROLE_PERMISSION> ROLE_PERMISSION, Func<Guid> GetROLEKeyFunc,
            IEnumerable<ROLE_PERMISSION> SYSTEM_PERMISSIONS)
        {
            IEnumerable<ROLE_PERMISSION> finalizedROLE_PERMISSION = ROLE_PERMISSION.ToArray();
            var roleKey = GetROLEKeyFunc();
            var currentAssignedROLE_PERMISSIONS =
                finalizedROLE_PERMISSION.Where(x => x.GUID_ROLE == roleKey);
            return SYSTEM_PERMISSIONS.Select(x => new ROLE_PERMISSIONInfo(x, currentAssignedROLE_PERMISSIONS)).AsQueryable();
        }
    }
}