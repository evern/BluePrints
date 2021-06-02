using BluePrints.Common.ViewModel.Misc;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.ViewModels;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseModel.ViewModel.Dialogs
{
    public class USERAdditionViewModel
    {
        public static USERAdditionViewModel Create(IEnumerable<DEPARTMENT> departments, IEnumerable<DISCIPLINE> disciplines, IEnumerable<USER> users, IEnumerable<OFFICE> offices, string title, string description, IEnumerable<STAFF> localSTAFF, IEnumerable<STAFF> remoteSTAFF)
        {
            return ViewModelSource.Create(() => new USERAdditionViewModel(departments, disciplines, users, offices, title, description, localSTAFF, remoteSTAFF));
        }

        public USER USER { get; set; }
        public IEnumerable<DISCIPLINE> DISCIPLINECollection { get; set; }
        public IEnumerable<DEPARTMENT> DEPARTMENTCollection { get; set; }
        public IEnumerable<USER> USERCollection { get; set; }
        public IEnumerable<OFFICE> OFFICECollection { get; set; }
        public IEnumerable<STAFF> LocalSTAFFCollection { get; set; }
        public IEnumerable<STAFF> RemoteSTAFFCollection { get; set; }
        public string Label { get; set; }
        protected USERAdditionViewModel(USER activeDirectoryUSER, IEnumerable<DEPARTMENT> departments, IEnumerable<DISCIPLINE> disciplines, IEnumerable<USER> users, IEnumerable<OFFICE> offices, string title, string description, IEnumerable<STAFF> localSTAFF, IEnumerable<STAFF> remoteSTAFF)
        {
            USER = new USER();

            DEPARTMENTCollection = departments;
            DISCIPLINECollection = disciplines;
            USERCollection = users;
            OFFICECollection = offices;
            LocalSTAFFCollection = localSTAFF;
            RemoteSTAFFCollection = remoteSTAFF;

            USER.NAME = activeDirectoryUSER.NAME;
            USER.TITLE = activeDirectoryUSER.TITLE;
            USER.DESCRIPTION = activeDirectoryUSER.DESCRIPTION;
            USER.FIRST_NAME = activeDirectoryUSER.FIRST_NAME;
            USER.LAST_NAME = activeDirectoryUSER.LAST_NAME;
            USER.CREATED = DateTime.Now;
            USER.DEPARTMENT = activeDirectoryUSER.DEPARTMENT;
            DEPARTMENT department = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == activeDirectoryUSER.DEPARTMENT.ToUpper());
            if (department != null)
                USER.GUID_DEPARTMENT = department.GUID;

            USERCollectionViewModelWrapper.PopulateUserStaffIds(USER, LocalSTAFFCollection, RemoteSTAFFCollection);
        }

        public USER GetNewUser()
        {
            return USER;
        }
    }
}