namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Courier_Location_Depot
    {
        [Key]
        public int SeqNo { get; set; }

        [Required]
        [StringLength(128)]
        public string LocName { get; set; }

        [Required]
        [StringLength(5)]
        public string CourDepCode { get; set; }
    }
}