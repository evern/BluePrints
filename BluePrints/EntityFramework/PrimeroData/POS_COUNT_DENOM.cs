namespace BluePrints.PrimeroData
{
    using System.ComponentModel.DataAnnotations;

    public partial class POS_COUNT_DENOM
    {
        [Key]
        public int SEQNO { get; set; }

        public int SHIFTNO { get; set; }

        public int PTNO { get; set; }

        public int DENOM_SEQNO { get; set; }

        public int COUNT1_QTY { get; set; }

        public int COUNT2_QTY { get; set; }

        public double COUNT1_VALUE { get; set; }

        public double COUNT2_VALUE { get; set; }

        public double COUNT2_ADJUST { get; set; }

        public double FLOAT_VALUE { get; set; }

        public double SHIFT_VALUE { get; set; }
    }
}