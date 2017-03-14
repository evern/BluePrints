using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common;

namespace BluePrints.ViewModels
{
    public class MissingP6Activities
    {
        public string INTERNAL_NUM { get; set; }
        public string P6_ACTIVITY { get; set; }
        public decimal UNITS { get; set; }
    }

    public class MissingP6ActivitiesViewModel
    {
        public static MissingP6ActivitiesViewModel Create(IEnumerable<MissingP6Activities> enumerableObjects)
        {
            return ViewModelSource.Create(() => new MissingP6ActivitiesViewModel(enumerableObjects));
        }

        public IEnumerable<MissingP6Activities> SourceObjects { get; set; }

        protected MissingP6ActivitiesViewModel(IEnumerable<MissingP6Activities> enumerableObjects)
        {
            SourceObjects = enumerableObjects;
        }
    }
}