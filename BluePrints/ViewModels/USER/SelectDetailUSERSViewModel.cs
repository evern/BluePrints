using BluePrints.Common;
using BluePrints.Data;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;

namespace BluePrints.ViewModels
{
    public class SelectDetailUSERSViewModel
    {
        public static SelectDetailUSERSViewModel Create(Func<IEnumerable<USER>> getExistingUSERSFunc)
        {
            return ViewModelSource.Create(() => new SelectDetailUSERSViewModel(getExistingUSERSFunc));
        }

        public ObservableCollection<USER> Entities { get; set; }
        public USER SelectedEntity { get; set; }
        public ObservableCollection<USER> SelectedEntities { get; set; }
        Func<IEnumerable<USER>> getExistingUSERSFunc { get; set; }
        protected SelectDetailUSERSViewModel(Func<IEnumerable<USER>> getExistingUSERSFunc)
        {
            this.getExistingUSERSFunc = getExistingUSERSFunc;
            Entities = new ObservableCollection<USER>();
            SelectedEntities = new ObservableCollection<USER>();
            Refresh();
        }

        private void Refresh()
        {
            IEnumerable<USER> activeDirectoryUSERS = ActiveDirectory.GetUSERS();
            IEnumerable<USER> existingUSERS = getExistingUSERSFunc();
            foreach(var activeDirectoryUSER in activeDirectoryUSERS)
            {
                if(!existingUSERS.Any(x => x.NAME == activeDirectoryUSER.NAME))
                    Entities.Add(activeDirectoryUSER);
            }
        }
    }
}
