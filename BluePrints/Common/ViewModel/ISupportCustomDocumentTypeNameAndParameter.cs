using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    /// <summary>
    /// The interface for supporting children document other than using TEntity type name.
    /// </summary>
    public interface ISupportCustomDocumentTypeNameAndParameter
    {
        string GetCustomDocumentTypeName();
        object GetCustomDocumentParameter();
        string GetCustomDocumentTitle();
        bool IsCustomModeEnabled();
    }
}