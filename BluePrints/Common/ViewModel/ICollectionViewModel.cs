using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel
{
    public interface ICollectionViewModel<TProjection>
        where TProjection : class
    {
        void Save(TProjection entity);
        void Delete(TProjection entity);
        void CleanUpCallBacks();
    }
}
