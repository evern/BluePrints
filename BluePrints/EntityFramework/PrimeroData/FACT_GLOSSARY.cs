namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class FACT_GLOSSARY
    {
        [Key]
        [StringLength(200)]
        public string FIELDNAME { get; set; }

        public string NOTES { get; set; }

        public int? FIELDLEVEL { get; set; }
    }
}
