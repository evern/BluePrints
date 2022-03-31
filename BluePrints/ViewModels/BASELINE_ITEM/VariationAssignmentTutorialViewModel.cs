using BaseModel.DataModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BluePrints.ViewModels
{
    public class VariationAssignmentTutorialViewModel : ViewModelBase
    {
        /// <summary>
        /// Creates a new instance of ChangeLogViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VariationAssignmentTutorialViewModel Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new VariationAssignmentTutorialViewModel(unitOfWorkFactory));
        }

        public Stream Tutorial { get; set; }
        /// <summary>
        /// Initializes a new instance of the ChangeLogViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ChangeLogViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected VariationAssignmentTutorialViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            Tutorial = AssemblyHelper.GetResourceStream(currentAssembly, "Views/P6VariationTutorial.pdf", false);
        }
    }
}