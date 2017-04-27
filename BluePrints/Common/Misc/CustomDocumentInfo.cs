namespace BluePrints.Common.ViewModel
{
    /// <summary>
    /// Parameter for IDocumentManagerServiceExtensions to search and show document
    /// </summary>
    public class CustomDocumentInfo
    {
        public CustomDocumentInfo(object parameter, string documentType, string title)
        {
            this.Parameter = parameter;
            this.DocumentType = documentType;
            this.Title = title;
        }

        public object Parameter { get; set; }
        public string DocumentType { get; set; }
        //Title used to display on tab and searching for document, should be unique
        public string Title { get; set; }
    }
}
