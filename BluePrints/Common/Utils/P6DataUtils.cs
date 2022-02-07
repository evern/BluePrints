using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Utils
{
    public static class P6DataUtils
    {
        public static void AssignP6BudgetedUnits(IEnumerable<ICanAssignP6> deliverableSource, P6Data.PROJECT P6PROJECTReference, IP6EntitiesUnitOfWork p6EntitiesUnitOfWork, bool isBudget, List<P6_AssignmentProjection> missing_activities = null)
        {
            List<int> taskRsrcFirstAssignmentRegister = new List<int>();
            List<int> taskFirstAssignmentRegister = new List<int>();
            if (P6PROJECTReference == null)
                return;

            IEnumerable<TASK> taskSource = P6PROJECTReference.TASK;
            IEnumerable<TASKRSRC> taskRsrcSource = P6PROJECTReference.TASKRSRC;
            foreach (ICanAssignP6 deliverable in deliverableSource)
            {
                IEnumerable<P6_ASSIGNMENT> deliverable_assignments = deliverable.P6_Assignments;

                foreach (P6_ASSIGNMENT deliverable_assignment in deliverable_assignments)
                {
                    TASK uowTASK = taskSource.Where(x => x.delete_session_id == null).FirstOrDefault(x => x.task_code == deliverable_assignment.P6_ACTIVITYID);

                    P6_AssignmentProjection p6_assignment = new P6_AssignmentProjection(deliverable, deliverable_assignment, isBudget);

                    if (uowTASK != null)
                    {
                        bool isTaskFirstAssigned = taskFirstAssignmentRegister.Any(x => x == uowTASK.task_id);
                        if(!isTaskFirstAssigned)
                        {
                            uowTASK.target_work_qty = p6_assignment.UNITS;
                            uowTASK.remain_work_qty = p6_assignment.UNITS;
                            taskFirstAssignmentRegister.Add(uowTASK.task_id);
                        }
                        else
                        {
                            uowTASK.target_work_qty += p6_assignment.UNITS;
                            uowTASK.remain_work_qty += p6_assignment.UNITS;
                        }

                        IEnumerable<TASKRSRC> uowTASKRSRCS = taskRsrcSource.Where(x => x.delete_session_id == null).Where(x => x.task_id == uowTASK.task_id);
                        decimal totalTargetQuantity = uowTASKRSRCS.Where(x => x.target_qty != null).Sum(x => (decimal)x.target_qty);
                        decimal p6TaskRsrcCount = uowTASKRSRCS.Count();
                        bool isUseProRataMode = totalTargetQuantity > 0;

                        foreach (TASKRSRC uowTASKRSRC in uowTASKRSRCS)
                        {
                            if (isUseProRataMode)
                            {
                                if (uowTASKRSRC.target_qty != null)
                                {
                                    decimal p6TaskTargetQty = (decimal)uowTASKRSRC.target_qty;
                                    decimal proRateRemainingQty = p6TaskTargetQty / totalTargetQuantity;

                                    //check if it's first assignment to reset qty instead of adding onto it
                                    bool isTaskRsrcFirstAssigned = taskRsrcFirstAssignmentRegister.Any(x => x == uowTASKRSRC.taskrsrc_id);
                                    if (!isTaskRsrcFirstAssigned)
                                    {
                                        uowTASKRSRC.target_qty = p6_assignment.UNITS * proRateRemainingQty;
                                        uowTASKRSRC.remain_qty = p6_assignment.UNITS * proRateRemainingQty;
                                        taskRsrcFirstAssignmentRegister.Add(uowTASKRSRC.taskrsrc_id);
                                    }
                                    else
                                    {
                                        uowTASKRSRC.target_qty += p6_assignment.UNITS * proRateRemainingQty;
                                        uowTASKRSRC.remain_qty += p6_assignment.UNITS * proRateRemainingQty;
                                    }
                                }
                                else
                                {
                                    uowTASKRSRC.target_qty = 0;
                                    uowTASKRSRC.remain_qty = 0;
                                }
                            }
                            //this mode will only run once as target_qty will be updated on next iteration
                            else
                            {
                                //check if it's first assignment to reset qty instead of adding onto it
                                bool isFirstAssignment = taskRsrcFirstAssignmentRegister.Any(x => x == uowTASKRSRC.taskrsrc_id);
                                if (!isFirstAssignment)
                                {
                                    uowTASKRSRC.target_qty = p6_assignment.UNITS / p6TaskRsrcCount;
                                    uowTASKRSRC.remain_qty = p6_assignment.UNITS / p6TaskRsrcCount;
                                }
                                else
                                {
                                    uowTASKRSRC.target_qty += p6_assignment.UNITS / p6TaskRsrcCount;
                                    uowTASKRSRC.remain_qty += p6_assignment.UNITS / p6TaskRsrcCount;
                                }
                            }

                            if (uowTASK.target_drtn_hr_cnt != null && uowTASK.target_drtn_hr_cnt != 0)
                                uowTASKRSRC.remain_qty_per_hr = uowTASKRSRC.target_qty / uowTASK.target_drtn_hr_cnt;
                        }
                    }
                    else if (missing_activities != null)
                    {
                        missing_activities.Add(p6_assignment);
                    }
                }
            }

            p6EntitiesUnitOfWork.SaveChanges();
        }
    }
}
