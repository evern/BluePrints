using BluePrints.Common;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class USERSelectionViewModel
    {
        public static USERSelectionViewModel Create(IEnumerable<USER> existingUSERS)
        {
            return ViewModelSource.Create(() => new USERSelectionViewModel(existingUSERS));
        }

        public ObservableCollection<USER> Entities { get; set; }
        public USER SelectedEntity { get; set; }
        public ObservableCollection<USER> SelectedEntities { get; set; }
        private IEnumerable<USER> ExistingUSERS { get; set; }

        protected USERSelectionViewModel(IEnumerable<USER> existingUSERS)
        {
            ExistingUSERS = existingUSERS;
            Entities = new ObservableCollection<USER>();
            SelectedEntities = new ObservableCollection<USER>();
            Refresh();
        }

        private void Refresh()
        {
            IEnumerable<USER> activeDirectoryUSERS = EmailService.GetUSERS();
            var existingUSERS = ExistingUSERS;
            foreach (var activeDirectoryUSER in activeDirectoryUSERS)
                if (!existingUSERS.Any(x => x.NAME == activeDirectoryUSER.NAME))
                    Entities.Add(activeDirectoryUSER);
        }
    }
}