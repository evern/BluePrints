using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            INotification notification = AppNotificationService.CreatePredefinedNotification("Update: Search bar is removed, to enable it press CTRL + F. Have a nice day", null, null, null);
            GlobalVariables.IsNotificationShown = true;
            notification.ShowAsync();
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
