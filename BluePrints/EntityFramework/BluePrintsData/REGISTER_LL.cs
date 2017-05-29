namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class REGISTER_LL
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid? GUID_PROJECT { get; set; }

        public Guid? GUID_AREA { get; set; }

        public Guid? GUID_DISCIPLINE { get; set; }

        public int? RAISEDBY_TYPE { get; set; }

        [StringLength(150)]
        public string NUMBER { get; set; }

        [StringLength(300)]
        public string RAISEDBY { get; set; }

        [StringLength(150)]
        public string TYPE { get; set; }

        [StringLength(300)]
        public string TITLE { get; set; }

        [StringLength(300)]
        public string DESCRIPTION { get; set; }

        public bool? PROCESS_ISSUE { get; set; }

        [StringLength(500)]
        public string FURTHER_ACTION { get; set; }

        public DateTime? DATE_IDENTIFIED { get; set; }

        public DateTime? DATE_CLOSED { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual AREA AREA { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
