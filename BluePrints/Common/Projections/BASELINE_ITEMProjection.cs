using BaseModel.Attributes;
using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.GUID_BASELINE, Entity.INTERNAL_NUM")]
    public class BASELINE_ITEMProjection : BluePrintsProjectionBase<BASELINE_ITEM>, IDeliverable_Rates, ISupportByDuration, IHaveDeliverableStatus, IHaveDBProductivityOverride, ISupportVariation, IEntityNumber
    {
        public BASELINE_ITEMProjection()
            : base()
        {

        }

        public RATE RATE { get; set; }

        public decimal Budget_ItemRate
        {
            get
            {
                if (RATE == null || RATE.RATE1 == null)
                    return 0;

                return (decimal)RATE.RATE1;
            }
        }

        public decimal Budget_Costs => Budget_Units * Budget_ItemRate;

        public decimal Total_Costs => Total_Units * Budget_ItemRate;

        public string Deliverable_Name => Entity.Deliverable_Name;

        public string Commodity_Code => Entity.Commodity_Code;

        public string Commodity_Display_Code => Commodity_Code;

        public Guid? Subjob_Guid => Entity.Subjob_Guid;

        public decimal Budget_Units => Entity.Budget_Units;

        public decimal Total_Units => Entity.Total_Units;

        public Guid OriginalEntityKey => Entity.OriginalEntityKey;

        public void SetOriginalEntityKey(Guid newGuid) => Entity.SetOriginalEntityKey(newGuid);

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public string Discipline_Code => Entity.Discipline_Code;

        public decimal Variation_Units => Entity.Variation_Units;

        public decimal Variation_Costs => Entity.Variation_Units * Budget_ItemRate;

        public bool IsByDuration => Entity.IsByDuration;

        public DELIVERABLES_STATUS Deliverable_Status => Entity.DELIVERABLES_STATUS;

        public string Phase_Code => Entity.PHASE == null ? string.Empty : Entity.PHASE.INTERNAL_NUM;

        public decimal? DB_Productivity_Override { get => Entity.DB_Productivity_Override; set => Entity.DB_Productivity_Override = value; }

        public Guid? Baseline_Guid { get => Entity.Baseline_Guid; set => Entity.Baseline_Guid = value; }

        public Guid? Variation_Guid { get => Entity.Variation_Guid; set => Entity.Variation_Guid = value; }
        public decimal Estimated_Value { get => Entity.Estimated_Value; set => Entity.Estimated_Value = value; }
        public decimal DC_Value { get => Entity.DC_Value; set => Entity.DC_Value = value; }

        public string Subjob_Name => Entity.Subjob_Name;

        public string Department_Code => Entity.Department_Code;

        public string EntityNumber { get => Entity.EntityNumber; set => Entity.EntityNumber = value; }

        public string EntityGroup => Entity.EntityGroup;

        public Guid? Phase_Guid { get => Entity.Phase_Guid; set => Entity.Phase_Guid = value; }

        Guid? IDeliverable.Subjob_Guid { get => Entity.Subjob_Guid; set => Entity.Subjob_Guid = value; }

        public Guid? Discipline_Guid => Entity.GUID_DISCIPLINE;

        public decimal Discipline_Number => Entity.DISCIPLINE_NUM;

        public Guid? Workpack_Guid { get => Entity.GUID_WORKPACK; set => Entity.GUID_WORKPACK = value; }

        public PhaseType? Phase => Entity.Phase;

        [NotMapped]
        private IEnumerable<object> assignUserObject;

        [NotMapped]
        public object AssignUserObject
        {
            get { return assignUserObject; }
            set
            {
                if (value != assignUserObject)
                {
                    assignUserObject = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<USER> AssignUsers
        {
            get
            {
                if (assignUserObject == null)
                    return null;

                return assignUserObject.Select(x => (USER)x);
            }
        }

        [NotMapped]
        List<User_Weight> userweights { get; set; }
        public List<User_Weight> UserWeights
        {
            get
            {
                if (userweights == null)
                    userweights = new List<User_Weight>();

                return userweights;
            }
            set
            {
                userweights = value;
            }
        }

        public IEnumerable<User_Weight> AssignedUsers => UserWeights;
    }

    public class User_Weight
    {
        public USER User { get; set; }
        public decimal Weight { get; set; }
        public decimal AggregateWeight { get; set; }

        public double AggregateWeightDbl
        {
            get
            {
                if (AggregateWeight == 0)
                    return 0;

                return Convert.ToDouble(AggregateWeight);
            }
        }

        public string UserName => User == null ? string.Empty : User.Full_Name;
        public string UserRole => User == null ? string.Empty : User.ROLE == null ? string.Empty : User.ROLE.NAME;
    }

    public static class BASELINE_ITEMProjectionQueries
    {
        public static IQueryable<BASELINE_ITEMProjection> IDeliverable_Rates_Transformation(
            IQueryable<BASELINE_ITEM> BASELINE_ITEMS, 
            IEnumerable<RATE> RATES)
        {
            //IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            //IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            //return from baseline_item in bluePrintsUOW.BASELINE_ITEMS
            //       join rate in bluePrintsUOW.RATES on
            //        new { Dept = baseline_item.GUID_DEPARTMENT, Disc = baseline_item.GUID_DISCIPLINE } equals new { Dept = (Guid?)rate.GUID_DEPARTMENT, Disc = rate.GUID_DISCIPLINE }
            //        into baseline_item_rate
            //        from x in baseline_item_rate.DefaultIfEmpty()
            //        select new BASELINE_ITEMProjection() { Entity = baseline_item, RATE = x };


            return
                BASELINE_ITEMS.ToArray()
                    .Select(x => new BASELINE_ITEMProjection()
                    {
                        EntityKey = x.GUID,
                        Entity = x,
                        RATE = RATES.FirstOrDefault(y => (y.PHASE_TYPE == x.Phase) && (y.CHARGE_TYPE == x.PHASE.CHARGE_TYPE) && (y.GUID_DEPARTMENT == x.GUID_DEPARTMENT || y.GUID_DEPARTMENT == null) && (y.GUID_DISCIPLINE == x.GUID_DISCIPLINE || y.GUID_DISCIPLINE == null) && (y.GUID_COMMODITY == x.GUID_DOCTYPE || y.GUID_COMMODITY == null))
                    }).AsQueryable();
        }
    }
}