namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_DEPARTMENT : EntityBase
    {
        [NotMapped]
        public string X_Number_Str => X_Number.ToString();
    }
}
