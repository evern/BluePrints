namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DAYWORK_LABOUR
    {
        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_PROJECT { get; set; }

        public DateTime WORKDATE { get; set; }

        public int RESOURCE_ID { get; set; }

        [StringLength(150)]
        public string TRADE { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? HOURS { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? RATE { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual PROJECT PROJECT { get; set; }
    }
}
