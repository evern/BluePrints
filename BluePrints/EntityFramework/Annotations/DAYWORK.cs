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

    public partial class DAYWORK : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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
        private IEnumerable<DAYWORK> histories;
        public IEnumerable<DAYWORK> Histories
        {
            get
            {
                if (histories != null)
                    return histories.OrderByDescending(x => x.CREATED);

                return null;
            }
        }

        [NotMapped]
        public IEnumerable<string> DescriptionHistories
        {
            get
            {
                if (Histories != null)
                    return Histories.Select(x => x.DESCRIPTION).Distinct();

                return null;
            }
        }

        [NotMapped]
        public IEnumerable<string> RequestedByHistories
        {
            get
            {
                if (Histories != null)
                    return Histories.Select(x => x.REQUESTED_BY).Distinct();

                return null;
            }
        }

        public void SetHistory(IEnumerable<DAYWORK> histories)
        {
            this.histories = histories;
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OFFICE.NAME;
    }
}