using BaseModel.Misc;
using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BluePrints.ViewModels.VARIATIONSCollectionViewModelWrapper;

namespace BluePrints.Common.Misc
{
    public class VariationApprovalAction<TEntity> : BindableBase, ICanUpdate
        where TEntity : class, IDeliverable, ISupportVariationRevision, new()
    {
        //for bindable base usage only
        public VariationApprovalAction()
        {

        }

        public VariationApprovalAction(ISupportVariation<TEntity> deliverable, VariationStages variationStage)
        {
            Deliverable = deliverable;
            VariationStage = variationStage;
        }

        public ISupportVariation<TEntity> Deliverable { get; set; }
        private VariationStages VariationStage { get; set; }
        public bool ReduceEarned { get; set; }
        public decimal MaximumReducibleUnits => -1 * (Deliverable.Total_Units - Deliverable.Earned_Units_Total);
        public decimal DisplayVariationUnits
        {
            get
            {
                if (VariationStage == VariationStages.Unapprove)
                    return -1 * Deliverable.DisplayVariationUnits;
                else
                    return Deliverable.DisplayVariationUnits;
            }
        }

        public bool NewEntityFromView { get; set; }

        public void Update()
        {
            RaisePropertiesChanged();
        }
    }
}
