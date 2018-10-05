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

    public partial class DAYWORK_LABOUR : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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
        private IEnumerable<DAYWORK_STAFF_ROLE> roles;
        public IEnumerable<string> Roles
        {
            get
            {
                if (roles != null)
                    return roles.Where(x => x.RESOURCE_ID == RESOURCE_ID).Select(x => x.PROJECT_ROLE).Distinct();

                return null;
            }
        }

        public void SetRoles(IEnumerable<DAYWORK_STAFF_ROLE> roles)
        {
            this.roles = roles;
        }

        public string Office => this.PROJECT.NUMBER + " " + this.PROJECT.OFFICE.NAME;
    }
}