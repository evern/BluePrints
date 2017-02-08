namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CONTACT_MARKETING_CLASSES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLASSNO { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }
    }
}