using BaseModel.Misc;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluePrints.Data
{
    public partial class ESTIMATION_DIRECT : IGuidEntityKey
    {
        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }
    }
}
