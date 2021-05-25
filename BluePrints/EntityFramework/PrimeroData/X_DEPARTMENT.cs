namespace BluePrints.PrimeroData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class X_DEPARTMENT
    {
        [Key]
        public int X_Number { get; set; }

        [StringLength(50)]
        public string X_Name { get; set; }
    }
}