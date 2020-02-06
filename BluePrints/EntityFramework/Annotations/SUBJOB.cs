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
    public partial class SUBJOB : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SUBJOB()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            SUBJOB_ASSIGNMENT = new HashSet<SUBJOB_ASSIGNMENT>();
            WORKPACK = new HashSet<WORKPACK>();
            CONSTRUCTION_JOB = new HashSet<CONSTRUCTION_JOB>();
            BELLCURVESHAPE = Common.BellCurveShape.Balanced;
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

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}