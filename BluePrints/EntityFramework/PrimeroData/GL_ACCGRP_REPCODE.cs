namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GL_ACCGRP_REPCODE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REPCODE { get; set; }

        [StringLength(30)]
        public string DESCRIPTION { get; set; }
    }
}