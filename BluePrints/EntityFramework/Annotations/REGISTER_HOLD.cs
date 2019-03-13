namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BaseModel.DataModel;

    [ConstraintAttributes("NUMBER")]
    public partial class REGISTER_HOLD : EntityBase, IGuidEntityKey, ICanSync, IEntityNumber, IHaveCreatedDate
    {
        [NotMapped]
        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }


        [NotMapped]
        private IEnumerable<object> assignDeliverableObjects;

        [NotMapped]
        public object AssignDeliverableObjects
        {
            get { return assignDeliverableObjects; }
            set
            {
                if (value != assignDeliverableObjects)
                {
                    assignDeliverableObjects = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<BASELINE_ITEM> AssignDeliverables
        {
            get
            {
                if (assignDeliverableObjects == null)
                    return null;

                return assignDeliverableObjects.Select(x => (BASELINE_ITEM)x);
            }
        }

        public void SetDeliverables(IEnumerable<BASELINE_ITEM> deliverables, IEnumerable<REGISTER_HOLD_REF> register_hold_deliverables)
        {
            assignDeliverableObjects = deliverables.Where(baselineItem => register_hold_deliverables.Any(registerHoldRef => registerHoldRef.GUID_HOLD == GUID && registerHoldRef.GUID_BASELINE_ITEM == baselineItem.GUID_ORIGINAL)).ToList();
        }

        public string EntityGroup => string.Empty;

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
    }
}