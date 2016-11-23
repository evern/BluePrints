namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PROFILE_CONFLICT_SET_MEMBERS
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONFLICTSETID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string FIELDNAME { get; set; }

        [Required]
        [StringLength(100)]
        public string FIELDVALUE { get; set; }
    }
}
