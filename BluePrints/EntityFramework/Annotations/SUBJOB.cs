namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using DevExpress.Mvvm;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;

    [ConstraintAttributes("GUID_PROJECT, INTERNAL_NAME1")]
    public partial class SUBJOB : EntityBase, IGuidEntityKey, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SUBJOB()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            SUBJOB_ASSIGNMENT = new HashSet<SUBJOB_ASSIGNMENT>();
            WORKPACK = new HashSet<WORKPACK>();
            ESTIMATE_ITEM = new HashSet<ESTIMATE_ITEM>();
            ESTIMATE_ITEM1 = new HashSet<ESTIMATE_ITEM>();
            STARTDATE = DateTime.Now;
            ENDDATE = DateTime.Now;
            REVIEWSTARTDATE = DateTime.Now;
            REVIEWENDDATE = DateTime.Now;
            BELLCURVESHAPE = Common.BellCurveShape.Balanced;
        }

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

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //Used for direct property access validation in fill/undo-redo
        [NotMapped]
        public Guid? SubAreaGuid
        {
            get
            {
                return GUID_DSUBAREA;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    GUID_DSUBAREA = null;
                else if (IsSubAreaValid(setValue))
                    GUID_DSUBAREA = setValue;
            }
        }

        public bool IsSubAreaValid(Guid? subAreaGuid)
        {
            if (subAreaGuid == null)
                return false;

            if (SubAreaCollection == null)
                return false;

            return SubAreaCollection.Any(x => x.GUID == subAreaGuid);
        }

        [NotMapped]
        public decimal MissingQuantity { get; set; }

        [NotMapped]
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                if (AREA == null)
                    return null;

                return AREA.AREA1;
            }
        }
    }
}