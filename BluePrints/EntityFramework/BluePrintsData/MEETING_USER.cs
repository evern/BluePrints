namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MEETING_USER
    {
        [Key]
        public Guid GUID { get; set; }

        public int TYPE { get; set; }

        public int USER_TYPE { get; set; }

        public Guid USER_GUID { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual CLIENT CLIENT { get; set; }

        public virtual USER USER { get; set; }
    }
}
