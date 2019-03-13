namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common.Base;
    using BaseModel.DataModel;
    using BluePrints.Common.Resources;

    public partial class CLIENT : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        private IEnumerable<object> projects;

        [NotMapped]
        public object Projects
        {
            get { return projects; }
            set
            {
                if (value != projects)
                {
                    projects = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<PROJECT> Project_Assignments
        {
            get
            {
                if (projects == null)
                    return null;

                return projects.Select(x => (PROJECT)x);
            }
        }

        [NotMapped]
        public string Full_Name
        {
            get
            {
                if (FIRST_NAME == null)
                    return LAST_NAME;

                if (LAST_NAME == null)
                    return FIRST_NAME;

                return FIRST_NAME + " " + LAST_NAME;
            }
        }

        public string Office => BluePrintsResources.GlobalOffice;
    }
}