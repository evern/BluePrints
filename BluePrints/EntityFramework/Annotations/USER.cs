namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using BluePrints.PrimeroData;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class USER : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
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

        [NotMapped]
        public IEnumerable<STAFF> PerthStaffs { get; set; }

        [NotMapped]
        public IEnumerable<STAFF> MontrealStaffs { get; set; }

        public IEnumerable<STAFF> ExoSTAFFS
        {
            get
            {
                if (OFFICE == null)
                    return null;

                if (OFFICE.NAME.ToUpper() == BluePrintsResources.OfficeMontreal)
                    return MontrealStaffs;

                else if (OFFICE.NAME.ToUpper() == BluePrintsResources.OfficePerth)
                    return PerthStaffs;

                return null;
            }
        }

        //for use in EXO_SubJobCollectionView
        [NotMapped]
        public int SecurityProfileID { get; set; }

        public string Office => BluePrintsResources.GlobalOffice;
    }
}