namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.PrimeroData;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class USER : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        #region Token Selection
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
        #endregion

        [NotMapped]
        public string Full_Name
        {
            get
            {
                if ((FIRST_NAME == string.Empty && LAST_NAME == string.Empty) || (FIRST_NAME == null && LAST_NAME == null))
                    return NAME;

                if (FIRST_NAME == null)
                    return LAST_NAME;

                if (LAST_NAME == null)
                    return FIRST_NAME;

                return FIRST_NAME + " " + LAST_NAME;
            }
        }

        //for use in EXO_SubJobCollectionView
        [NotMapped]
        public int SecurityProfileID { get; set; }

        [NotMapped]
        public string ProjectLocale { get; set; }

        [NotMapped]
        [ProjectionPropertyAttribute]
        public int? ProjectLocaleExoId
        {
            get
            {
                if (ProjectLocale == string.Empty || ProjectLocale == null)
                    return null;

                if (ProjectLocale == BluePrintsResources.OfficeMontreal)
                    return EXO_STAFF_ID_REMOTE;

                if (ProjectLocale == BluePrintsResources.OfficeUSA)
                    return EXO_STAFF_ID_USA;

;               return EXO_STAFF_ID;
            }
            set
            {
                if (ProjectLocale == string.Empty || ProjectLocale == null)
                    return;

                if (ProjectLocale == BluePrintsResources.OfficePerth)
                    EXO_STAFF_ID = value;
                else if (ProjectLocale == BluePrintsResources.OfficeMontreal)
                    EXO_STAFF_ID_REMOTE = value;
                else
                    EXO_STAFF_ID_USA = value;
            }
        }

        [NotMapped]
        List<USER_PREFERENCE> userPreferences;
        [NotMapped]
        public List<USER_PREFERENCE> UserPreferences
        {
            get
            {
                if (userPreferences == null)
                {
                    userPreferences = new List<USER_PREFERENCE>();
                    if(USER_PREFERENCE != null && USER_PREFERENCE.Count > 0)
                    {
                        foreach(USER_PREFERENCE userPreference in USER_PREFERENCE)
                        {
                            userPreferences.Add(userPreference);
                        }
                    }
                }

                return userPreferences;
            }
        }

        public string Office => BluePrintsResources.GlobalOffice;

        //making sure office is always queryable because newly added row won't have OFFICE navigational property and OFFICE cannot be set in ViewModel, because it'll give an error when saving
        [NotMapped]
        public string QueryOfficeName { get; set; }
    }
}