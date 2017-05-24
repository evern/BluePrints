using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class ESTIMATE_COMMODITY_CODESelectionViewModel
    {
        public static ESTIMATE_COMMODITY_CODESelectionViewModel Create(
            IEnumerable<COMMODITY_CODE> enumerableObjects,
            COMMODITY_CODE selectedItem, IEnumerable<PROJECT> ProjectLookup, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            return
                ViewModelSource.Create(
                    () => new ESTIMATE_COMMODITY_CODESelectionViewModel(enumerableObjects, selectedItem, ProjectLookup, DisciplineLookup));
        }

        public IEnumerable<COMMODITY_CODE> SourceObjects { get; set; }

        protected ESTIMATE_COMMODITY_CODESelectionViewModel(IEnumerable<COMMODITY_CODE> enumerableObjects,
            COMMODITY_CODE selectedItem, IEnumerable<PROJECT> ProjectLookup, IEnumerable<DISCIPLINE> DisciplineLookup)
        {
            SourceObjects = enumerableObjects;
            this.selectedItem = selectedItem;
            PROJECTCollection = ProjectLookup;
            DISCIPLINECollection = DisciplineLookup;
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection { get; set; }
        public IEnumerable<PROJECT> PROJECTCollection { get; set; }

        private COMMODITY_CODE selectedItem { get; set; }

        public COMMODITY_CODE SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != null)
                {
                    selectedItem = value;
                    this.RaisePropertiesChanged();
                }
            }
        }
    }
}