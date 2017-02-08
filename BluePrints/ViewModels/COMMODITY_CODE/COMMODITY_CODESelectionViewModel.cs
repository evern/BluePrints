using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.ViewModels
{
    public class COMMODITY_CODESelectionViewModel
    {
        public static COMMODITY_CODESelectionViewModel Create(
            IEnumerable<COMMODITY_CODEMasterDetailProjection> enumerableObjects,
            COMMODITY_CODEMasterDetailProjection selectedItem, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            return
                ViewModelSource.Create(
                    () => new COMMODITY_CODESelectionViewModel(enumerableObjects, selectedItem, DisciplineLookup));
        }

        public IEnumerable<COMMODITY_CODEMasterDetailProjection> SourceObjects { get; set; }

        protected COMMODITY_CODESelectionViewModel(IEnumerable<COMMODITY_CODEMasterDetailProjection> enumerableObjects,
            COMMODITY_CODEMasterDetailProjection selectedItem, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            SourceObjects = enumerableObjects;
            this.selectedItem = selectedItem;
            DisciplineObjects = DisciplineLookup;
        }

        private IEnumerable<DISCIPLINE> DisciplineObjects { get; set; }

        private COMMODITY_CODEMasterDetailProjection selectedItem { get; set; }

        public COMMODITY_CODEMasterDetailProjection SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != null)
                {
                    selectedItem = value;
                    this.RaisePropertiesChanged();
                }
            }
        }
    }
}