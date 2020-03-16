using BluePrints.Common.ViewModel.Misc;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;

namespace BaseModel.ViewModel.Dialogs
{
    public class EarnedDataDateRealignmentViewModel
    {
        public static EarnedDataDateRealignmentViewModel Create(IEnumerable<EarnedDataDateRealignModel> enumerableObjects, string label)
        {
            return ViewModelSource.Create(() => new EarnedDataDateRealignmentViewModel(enumerableObjects, label));
        }

        public IEnumerable<EarnedDataDateRealignModel> SourceObjects { get; set; }
        public string Label { get; set; }
        protected EarnedDataDateRealignmentViewModel(IEnumerable<EarnedDataDateRealignModel> enumerableObjects, string label)
        {
            SourceObjects = enumerableObjects;
            Label = label;
        }

        private EarnedDataDateRealignModel selectedItem { get; set; }

        public EarnedDataDateRealignModel SelectedItem
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