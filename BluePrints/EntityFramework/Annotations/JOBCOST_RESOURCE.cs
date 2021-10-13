namespace BluePrints.PrimeroData
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [ConstraintAttributes("RESOURCENAME")]
    public partial class JOBCOST_RESOURCE : EntityBase
    {
        [NotMapped]
        public bool IsActive
        {
            get => ISACTIVE == "Y";
            set
            {
                if (value)
                    ISACTIVE = "Y";
                else
                    ISACTIVE = "N";
            }
        }
    }
}