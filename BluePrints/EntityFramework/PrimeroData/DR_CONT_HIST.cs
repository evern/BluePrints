namespace BluePrints.PrimeroData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DR_CONT_HIST
    {
        [Key]
        public int SEQNO { get; set; }

        public int? CONTACT_SEQNO { get; set; }

        public DateTime? POSTTIME { get; set; }

        public DateTime? TRANSDATE { get; set; }

        public int? COMTYPE { get; set; }

        public int? SALESNO { get; set; }

        [StringLength(4096)]
        public string NOTE { get; set; }

        [StringLength(80)]
        public string SUBJECT { get; set; }

        public int? ACCNO { get; set; }

        public int? ACTIONSTATUS { get; set; }

        public DateTime? ACTIONDUEDATE { get; set; }

        public int JOBNO { get; set; }

        public int? EVENT_SEQNO { get; set; }

        [StringLength(40)]
        public string OUTLOOK_LINK { get; set; }
    }
}