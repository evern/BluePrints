using BaseModel.DataModel;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class POSpreadViewModel
    {
        public static POSpreadViewModel Create(decimal? period = null, decimal? interval = null)
        {
            return ViewModelSource.Create(() => new POSpreadViewModel(period, interval));
        }

        public POSpreadViewModel(decimal? period = null, decimal? interval = null)
        {
            Period = period == null ? 2 : period;
            Interval = interval == null ? 1 : interval;
        }

        public decimal? Period { get; set; }
        public decimal? Interval { get; set; }
    }
}