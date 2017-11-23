using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using DevExpress.Mvvm;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork> : CollectionViewModelsWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey,
        TMainEntityUnitOfWork>
        where TMainEntity : class, IGuidEntityKey, new()
        where TMainProjectionEntity : class, IGuidEntityKey, ICanUpdate, new()
        where TMainEntityUnitOfWork : IUnitOfWork
    {
        public SpellCheckerModule SpellCheckerModule { get; set; }

        public override void OnLoaded()
        {
            SpellCheckerModule = new SpellCheckerModule();
            SpellCheckerModule.ApplySpellCheckMode(true);
            ShowNotification();

            base.SetCurrentHWID(LoginCredentials.CurrentHWID);
            base.OnLoaded();
        }

        public override void OnAfterDeletedSendMessage(string entityName, string key, string messageType, string sender)
        {
            SignalR.HubSendMessage(entityName, key, messageType, sender, LoginCredentials.CurrentHWID);
        }

        public override void OnAfterSavedSendMessage(string entityName, string key, string messageType, string sender)
        {
            SignalR.HubSendMessage(entityName, key, messageType, sender, LoginCredentials.CurrentHWID);
        }

        public void ShowNotification()
        {
            if (AppNotificationService == null || GlobalVariables.IsNotificationShown)
                return;

            INotification notification1 = AppNotificationService.CreatePredefinedNotification("Update 15 Nov 2017: If anything doesn't work as expected, please push reset layout and reload this view. Have a nice day", null, null, null);
            INotification notification2 = AppNotificationService.CreatePredefinedNotification("Update 23 Nov 2017: The terminology: workpack is now called subjob", null, null, null);

            GlobalVariables.IsNotificationShown = true;
            notification1.ShowAsync();
            notification2.ShowAsync();
        }
        //protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        //{
        //    if(this.SpellCheckerModule != null && this.SpellCheckerModule.SpellChecker != null)
        //    {
        //        this.SpellCheckerModule.SpellChecker.CheckCompleteFormShowing += SpellChecker_CheckCompleteFormShowing;
        //        mainThreadDispatcher.BeginInvoke(new Action(() => GridControlService.HighlightIncorrectText(this.SpellCheckerModule.SpellChecker)));
        //    }
        //}

        //private void SpellChecker_CheckCompleteFormShowing(object sender, DevExpress.XtraSpellChecker.FormShowingEventArgs e)
        //{
        //    e.Handled = true;
        //    //throw new NotImplementedException();
        //}
    }
}
