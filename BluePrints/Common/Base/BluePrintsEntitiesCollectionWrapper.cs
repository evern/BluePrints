using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.Common.Helpers;
using BluePrints.ViewModels;
using BluePrints.Views;
using DevExpress.Mvvm;
using System.Linq;
using System.Windows;

namespace BluePrints.Common.Base
{
    public abstract class BluePrintsEntitiesCollectionWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork> : CollectionViewModelsWrapper<TMainEntity, TMainProjectionEntity, TMainEntityPrimaryKey, TMainEntityUnitOfWork>
        where TMainEntity : class, new()
        where TMainProjectionEntity : class, ICanUpdate, new()
        where TMainEntityUnitOfWork : IUnitOfWork
    {
        public SpellCheckerModule SpellCheckerModule { get; set; }

        public override void OnLoaded()
        {
            if (!isFirstLoaded)
            {
                SpellCheckerModule = new SpellCheckerModule();
                SpellCheckerModule.ApplySpellCheckMode(true);
                ShowNotification();

                base.SetCurrentHWID(LoginCredentials.CurrentHWID);
            }

            base.OnLoaded();
        }

        public override void OnAfterDeletedSendMessage(string entityName, string key, string messageType, string sender)
        {
            //SignalR.HubSendMessage(entityName, key, messageType, sender, LoginCredentials.CurrentHWID);
        }

        public override void OnAfterSavedSendMessage(string entityName, string key, string messageType, string sender)
        {
            //SignalR.HubSendMessage(entityName, key, messageType, sender, LoginCredentials.CurrentHWID);
        }

        public void LogOut()
        {
            BluePrintsGlobalMethods.LogOut();
        }

        public void ApplicationShutDown()
        {
            BluePrintsGlobalMethods.ApplicationShutDown();
        }

        public virtual void ShowNotification()
        {
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
