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
        public static COMMODITY_CODESelectionViewModel Create(IEnumerable<COMMODITY_CODE> enumerableObjects, COMMODITY_CODE selectedItem, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            return ViewModelSource.Create(() => new COMMODITY_CODESelectionViewModel(enumerableObjects, selectedItem, DisciplineLookup));
        }

        public IEnumerable<COMMODITY_CODE> SourceObjects { get; set; }
        protected COMMODITY_CODESelectionViewModel(IEnumerable<COMMODITY_CODE> enumerableObjects, COMMODITY_CODE selectedItem, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            SourceObjects = enumerableObjects;
            this.selectedItem = selectedItem;
            this.DisciplineObjects = DisciplineLookup;
        }

        IEnumerable<DISCIPLINE> DisciplineObjects { get; set; }

        COMMODITY_CODE selectedItem { get; set; }
        public COMMODITY_CODE SelectedItem
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
