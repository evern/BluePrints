namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DAYWORK_MATERIAL : EntityBase, IGuidEntityKey, IHaveCreatedDate
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

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }


        [NotMapped]
        private IEnumerable<DAYWORK_MATERIAL> histories;
        public IEnumerable<DAYWORK_MATERIAL> Histories
        {
            get
            {
                if (histories != null)
                    return histories.OrderByDescending(x => x.CREATED);

                return null;
            }
        }

        [NotMapped]
        public IEnumerable<string> ItemHistories
        {
            get
            {
                if (Histories != null)
                    return Histories.Select(x => x.ITEM).Distinct();

                return null;
            }
        }

        public void SetHistory(IEnumerable<DAYWORK_MATERIAL> histories)
        {
            this.histories = histories;
        }
    }
}