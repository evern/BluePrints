namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [ConstraintAttributes("GUID_DDEPARTMENT, CODE")]
    public partial class DOCTYPE : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //used for collection mapping which allows null values
        [NotMapped]
        public Guid? MappingEntityKey
        {
            get
            {
                if (GUID == Guid.Empty)
                    return null;

                return GUID;
            }
        }

        public IEnumerable<DELIVERABLES_STATUS> GetDeliverableStatusByProject(Guid GUID_PROJECT)
        {
            if (DSTATUS_DOCTYPE == null)
                return new List<DELIVERABLES_STATUS>();

            return DSTATUS_DOCTYPE.Where(x => x.DELIVERABLES_STATUS != null && x.DELIVERABLES_STATUS.GUID_PROJECT == GUID_PROJECT).Select(x => x.DELIVERABLES_STATUS);
        }

        public override string ToString()
        {
            return NAME;
        }

        public string Office => BluePrintsResources.GlobalOffice;
    }
}