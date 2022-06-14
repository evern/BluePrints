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

    public partial class FORECAST_EAC : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
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

        public string SubJobCode => SUBJOB_CODE == null ? string.Empty : SUBJOB_CODE;
        public string DisciplineCode => DISCIPLINE_CODE == null ? string.Empty : DISCIPLINE_CODE;
        public string CommodityCode => COMMODITY_CODE == null ? string.Empty : COMMODITY_CODE;
        public string VariationCode => VARIATION_CODE == null ? string.Empty : VARIATION_CODE;
        public string ForecastViewCode => string.Concat(SubJobCode, ";", DisciplineCode, ";", CommodityCode, ";", VariationCode);
        public string Office => BluePrintsResources.GlobalOffice;
    }
}