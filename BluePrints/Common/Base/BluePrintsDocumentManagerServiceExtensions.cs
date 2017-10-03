using BaseModel.Misc;
using BaseModel.ViewModel.Document;
using DevExpress.Mvvm;
using System;

namespace BluePrints.Common.Base
{
    public static class BluePrintsDocumentManagerServiceExtensions
    {
        /// <summary>
        /// Creates and shows a document based upon custom parameter selection from CollectionViewModelWrapper
        /// </summary>
        /// <param name="documentManagerService">An instance of the IDocumentManager interface used to create and show the document.</param>
        /// <param name="documentInfo">A custom document info to search and show document</param>
        /// <param name="parentViewModel">An object that is passed to the view model of the created view.</param>
        public static IDocument ShowExistingEntityDocumentWithLogging(
            this IDocumentManagerService documentManagerService, DocumentInfo documentInfo, object parentViewModel)
        {
            if(documentInfo != null && documentInfo.Title != null && documentInfo.DocumentType != null)
                SignalR.HubLogMessage(LoginCredentials.CurrentUser.Full_Name + " opened " + documentInfo.Title + " - " + documentInfo.DocumentType);

            return documentManagerService.ShowExistingEntityDocument(documentInfo, parentViewModel);
        }
    }
}
