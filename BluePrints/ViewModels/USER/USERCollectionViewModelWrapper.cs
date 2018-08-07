using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Windows.Threading;
using System.ComponentModel;

namespace BluePrints.ViewModels
{
    public class USERCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of USERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new USERCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the USERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the USERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected USERCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        //timer to scan serial port
        private DispatcherTimer serialPortScanTimer;
        private DispatcherTimer serialPortWriteTimer;
        public string SelectedCOMPort { get; set; }
        public string ConnectButtonContent { get; set; }
        public List<string> AvailablePorts { get; set; }
        private SerialPort serialPort1;
        protected override void resolveParameters(object parameter)
        {
            serialPortScanTimer = new DispatcherTimer();
            serialPortScanTimer.Interval = new TimeSpan(0, 0, 1);
            serialPortScanTimer.Tick += SerialPortScanTimer_Tick;

            serialPortWriteTimer = new DispatcherTimer();
            serialPortWriteTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            serialPortWriteTimer.Tick += SerialPortWriteTimer_Tick;


            serialPort1 = new SerialPort();
            serialPort1.DataReceived += SerialPort1_DataReceived;

            AvailablePorts = GetAllPorts();
            SelectedCOMPort = AvailablePorts.FirstOrDefault();
            connectToSerialPort();

            serialPortScanTimer.Start();
        }

        private void SerialPortWriteTimer_Tick(object sender, EventArgs e)
        {
            if (SelectedCOMPort == null || SelectedCOMPort == string.Empty)
                return;

            if (DisplaySelectedEntity == null || DisplaySelectedEntity.EXO_STAFF_ID == null)
                return;

            string incoming = string.Empty;
            try
            {
                incoming = serialPort1.ReadExisting();
                if (incoming == null)
                    return;
                else if (incoming.Contains("Input name, ending with #"))
                {

                    serialPort1.WriteLine(DisplaySelectedEntity.EXO_STAFF_ID.ToString() + "#");
                    MessageBoxService.ShowMessage("Write success");
                }
                //else if (incoming.Contains(DisplaySelectedEntity.EXO_STAFF_ID.ToString()))
            }
            catch
            {
                MessageBoxService.ShowMessage("Error: Serial Port read timed out.");
            }
        }

        private void SerialPortScanTimer_Tick(object sender, EventArgs e)
        {
            AvailablePorts = GetAllPorts();
            SelectedCOMPort = AvailablePorts.FirstOrDefault();
            this.RaisePropertyChanged(x => x.SelectedCOMPort);
        }

        public void PortConnect()
        {
            if(ConnectButtonContent == "Disconnect")
            {
                serialPort1.Close();
                serialPortWriteTimer.Stop();
                ConnectButtonContent = "Connect";
                this.RaisePropertyChanged(x => x.ConnectButtonContent);
            }
            else
            {
                connectToSerialPort();
                this.RaisePropertyChanged(x => x.ConnectButtonContent);
            }
        }

        private void connectToSerialPort()
        {
            if (SelectedCOMPort != null && SelectedCOMPort != string.Empty)
            {
                serialPort1.Close();
                serialPort1.PortName = SelectedCOMPort;
                serialPort1.BaudRate = 9600;
                serialPort1.DataBits = 8;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Handshake = Handshake.None;
                serialPort1.Encoding = System.Text.Encoding.Default;
                serialPort1.ReadTimeout = 10000;
                serialPortWriteTimer.Stop();
                try
                {

                    serialPort1.Open();
                }
                catch
                {
                    return;
                }

                ConnectButtonContent = "Disconnect";
            }
            else if(!IsLoading)
                MessageBoxService.ShowMessage("Please select a port");
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DisplaySelectedEntity == null || DisplaySelectedEntity.EXO_STAFF_ID == null)
                return;

            string incoming = string.Empty;
            try
            {
                incoming = serialPort1.ReadExisting();
                if (incoming == null)
                    return;
                else if (incoming.Contains("#"))
                {
                    serialPort1.WriteLine(DisplaySelectedEntity.EXO_STAFF_ID.ToString() + "#");
                    mainThreadDispatcher.BeginInvoke(new Action(() => LoadingScreenManager.ShowLoadingScreen(1)));
                    //mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Write success")));
                }
                else if (incoming.Contains("?"))
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => LoadingScreenManager.CloseLoadingScreen()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Write Failed, please try again")));
                }
                else if (incoming.Contains("@"))
                {
                    mainThreadDispatcher.BeginInvoke(new Action(() => LoadingScreenManager.CloseLoadingScreen()));
                    mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Write success")));
                }
            }
            catch
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage("Error: Serial Port read timed out.")));
            }
        }

        public List<string> GetAllPorts()
        {
            List<String> allPorts = new List<String>();
            foreach (String portName in System.IO.Ports.SerialPort.GetPortNames())
            {
                allPorts.Add(portName);
            }
            return allPorts;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<ROLE, ROLE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.ROLES);
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.STAFF, STAFFProjectionFunc);
            loaderCollection.AddLoaderDescription<OFFICE, OFFICE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.OFFICES);
        }

        private Func<IRepositoryQuery<STAFF>, IQueryable<STAFF>> STAFFProjectionFunc()
        {
            return query => query.Where(x => x.ISACTIVE == "Y");
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.USERS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<USER>, IQueryable<USER>> specifyMainViewModelProjection()
        {
            if (LoginCredentials.CurrentUser.NAME == BluePrintsResources.Default_AdminUsername)
                return query => query.OrderBy(x => x.NAME);
            else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                return query => query.Where(x => x.GUID == Guid.Empty);
            else
                return query => query.ToArray().Where(x => x.GUID_ROLE == null || x.GUID_ROLE == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID_ROLE)).AsQueryable();
        }

        public IEnumerable<Guid> ChildrenRoles(Guid roleGuid)
        {
            foreach (var role in ROLECollection)
                if (role.PARENTGUID == roleGuid)
                {
                    yield return role.GUID;

                    foreach (var entityChild in ChildrenRoles(role.GUID))
                        yield return entityChild;
                }
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<USER> entities)
        {
            //MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySaved;
            MainViewModel.OnAfterEntitySavedCallBack = OnAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnAfterEntitySaved(USER projection, USER entity, bool isNewEntity)
        {
            if(isNewEntity && entity.EXO_STAFF_ID == null)
                entity.EXO_STAFF_ID = getExoStaffId(entity);
        }
        #endregion

        #region View Properties

        private int? getExoStaffId(USER bluePrintsUser)
        {
            string exoGuessUserName = bluePrintsUser.FIRST_NAME.ToUpper() + " " + bluePrintsUser.LAST_NAME.ToUpper();
            STAFF exoSTAFF = PrimeroSTAFFCollection.FirstOrDefault(x => x.NAME == exoGuessUserName);
            if (exoSTAFF != null)
            {
                return exoSTAFF.STAFFNO;
            }
            else
            {
                List<string> delimitedNames = bluePrintsUser.NAME.Split('.').ToList();
                string exoGuessUserName2 = string.Empty;
                foreach (string delimitedName in delimitedNames)
                {
                    exoGuessUserName2 += delimitedName.ToUpper() + " ";
                }

                exoGuessUserName2 = exoGuessUserName2.Trim();
                STAFF exoSTAFF2 = PrimeroSTAFFCollection.FirstOrDefault(x => x.NAME == exoGuessUserName2);
                if (exoSTAFF2 != null)
                {
                    return exoSTAFF2.STAFFNO;
                }
            }

            return null;
        }

        public void MatchExoStaffId()
        {
            List<USER> userToSave = new List<USER>();
            foreach(USER entity in MainViewModel.Entities)
            {
                int? exoId = getExoStaffId(entity);
                if(exoId != null)
                {
                    entity.EXO_STAFF_ID = exoId;
                    userToSave.Add(entity);
                }
            }

            MainViewModel.BulkSave(userToSave);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "USERCollectionViewModelWrapper"; }
        }

        public IEnumerable<STAFF> PrimeroSTAFFCollection
        {
            get
            {
                var collection = GetEntities<STAFF>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<ROLE> ROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<ROLE> RestrictedROLECollection
        {
            get
            {
                var collection = GetEntities<ROLE>();
                if (collection != null)
                {
                    if (LoginCredentials.CurrentUser.NAME == BluePrintsResources.Default_AdminUsername)
                        collection = collection.OrderBy(x => x.NAME);
                    else if (LoginCredentials.CurrentUser.GUID_ROLE == null)
                        collection = collection.Where(x => x.GUID == Guid.Empty);
                    else
                        collection = collection.Where(x => x.GUID == LoginCredentials.CurrentUser.GUID_ROLE || ChildrenRoles((Guid)LoginCredentials.CurrentUser.GUID_ROLE).Contains((Guid)x.GUID)).OrderBy(x => x.NAME);
                }

                return collection;
            }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<OFFICE> OFFICECollection
        {
            get
            {
                var collection = GetEntities<OFFICE>();
                if (collection != null)
                {
                    collection = collection.OrderBy(x => x.NAME);
                }

                return collection;
            }
        }
        #endregion

        #region View Commands
        private IDialogService USERImportDialogService
        {
            get { return this.GetRequiredService<IDialogService>("USERImportDialogService"); }
        }

        public void Update_User()
        {
            IEnumerable<USER> activeDirectoryUSERS = ActiveDirectory.GetUSERS();
            List<USER> update_users = new List<USER>();
            foreach(USER user in MainViewModel.Entities)
            {
                USER active_directory_user = activeDirectoryUSERS.FirstOrDefault(x => x.NAME == user.NAME);
                if(active_directory_user != null)
                {
                    user.FIRST_NAME = active_directory_user.FIRST_NAME;
                    user.LAST_NAME = active_directory_user.LAST_NAME;
                    user.DESCRIPTION = active_directory_user.DESCRIPTION;
                    user.DEPARTMENT = active_directory_user.DEPARTMENT;
                    DEPARTMENT department = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == user.DEPARTMENT.ToUpper());
                    if (department != null)
                        user.GUID_DEPARTMENT = department.GUID;
                    user.TITLE = active_directory_user.TITLE;
                    update_users.Add(user);
                }
            }

            MainViewModel.BulkSave(update_users);
        }

        public void Import()
        {
            var selectEntitiesViewModel = USERSelectionViewModel.Create(MainViewModel.Entities);
            if (USERImportDialogService.ShowDialog(MessageButton.OKCancel, "Select Users to Import", "USERSelectionView", selectEntitiesViewModel) == MessageResult.OK)
            {
                List<USER> add_new_users = new List<USER>();
                foreach(USER selected_entity in selectEntitiesViewModel.SelectedEntities)
                {
                    USER new_user = new USER();
                    new_user.TITLE = selected_entity.TITLE;
                    new_user.DESCRIPTION = selected_entity.DESCRIPTION;
                    new_user.FIRST_NAME = selected_entity.FIRST_NAME;
                    new_user.LAST_NAME = selected_entity.LAST_NAME;
                    new_user.NAME = selected_entity.NAME;
                    new_user.CREATED = DateTime.Now;
                    new_user.DEPARTMENT = selected_entity.DEPARTMENT;
                    DEPARTMENT department = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == selected_entity.DEPARTMENT.ToUpper());
                    if (department != null)
                        new_user.GUID_DEPARTMENT = department.GUID;

                    add_new_users.Add(new_user);
                }

                MainViewModel.BulkSave(add_new_users);
            }

            selectEntitiesViewModel = null;
        }

        public override string UnifiedRowValidation(USER projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(USER projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            if(serialPort1 != null)
                serialPort1.Close();

            serialPortScanTimer.Stop();
            serialPortWriteTimer.Stop();
            base.OnClose(e);
        }
        #endregion
    }
}