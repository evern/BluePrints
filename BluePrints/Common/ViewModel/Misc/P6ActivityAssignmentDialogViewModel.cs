using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Dialogs;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common.ViewModel
{
    public class P6ActivityAssignmentDialogViewModel<TEntity> : DialogCollectionViewModel<TEntity>
        where TEntity : P6ActivityRemap
    {
        public static P6ActivityAssignmentDialogViewModel<TEntity> CreateViewModel(IEnumerable<TEntity> enumerableObjects, string projectNumber, IEnumerable<TASK> currentProjectTASKs, bool isNewP6Database)
        {
            return ViewModelSource.Create(() => new P6ActivityAssignmentDialogViewModel<TEntity>(enumerableObjects, projectNumber, currentProjectTASKs, isNewP6Database));
        }

        public List<string> OldScheduleNames { get; set; }
        public string SelectedOldScheduleName { get; set; }
        public string projectNumber;
        private IUnitOfWorkFactory<IP6EntitiesUnitOfWork> p6UnitOfWorkFactory;
        private IP6EntitiesUnitOfWork p6UnitOfWork;
        private IEnumerable<TASK> currentProjectTASKs;

        public P6ActivityAssignmentDialogViewModel(IEnumerable<TEntity> enumerableObjects, string projectNumber, IEnumerable<TASK> currentProjectTASKs, bool isNewP6Database)
            : base(enumerableObjects)
        {
            p6UnitOfWorkFactory = P6EntitiesUnitOfWorkSource.GetUnitOfWorkFactory(isNewP6Database);
            p6UnitOfWork = p6UnitOfWorkFactory.CreateUnitOfWork();
            this.projectNumber = projectNumber;
            this.currentProjectTASKs = currentProjectTASKs;
            PopulateScheduleNames();
        }

        private void PopulateScheduleNames()
        {
            OldScheduleNames = new List<string>();
            IEnumerable<PROJWBS> p6Projects = p6UnitOfWork.PROJWBS.Where(x => x.proj_node_flag == "Y" && x.wbs_short_name.Contains(projectNumber)).OrderBy(proj => proj.wbs_short_name);
            foreach(PROJWBS p6Project in p6Projects)
            {
                if(p6Project.wbs_short_name != projectNumber)
                    OldScheduleNames.Add(p6Project.wbs_short_name);
            }

            if (OldScheduleNames.Count == 1)
            {
                SelectedOldScheduleName = OldScheduleNames[0];
                this.RaisePropertiesChanged();
            }
        }

        public bool CanGetDescription()
        {
            return CanAutoAssign();
        }

        public void GetDescription()
        {
            if (SelectedOldScheduleName == null)
                return;

            PROJECT oldPROJECT = p6UnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == SelectedOldScheduleName && x.delete_date == null);
            IEnumerable<TASK> oldTASK = oldPROJECT.TASK;
            foreach (P6ActivityRemap p6Activity in SourceObjects)
            {
                TASK findTask = oldTASK.FirstOrDefault(x => x.task_code == p6Activity.P6_OLD_ACTIVITY);
                if (findTask != null)
                {
                    p6Activity.OLD_ACTIVITY_DESCRIPTION = findTask.task_name;
                    IPOCOViewModel pocoViewModel = p6Activity as IPOCOViewModel;
                    if (pocoViewModel != null)
                        pocoViewModel.RaisePropertiesChanged();
                }
            }
        }

        public bool CanAutoAssign()
        {
            return true;
        }

        public void AutoAssign()
        {
            if (SelectedOldScheduleName == null)
                return;

            PROJECT oldPROJECT = p6UnitOfWork.PROJECT.FirstOrDefault(x => x.proj_short_name == SelectedOldScheduleName && x.delete_date == null);
            IEnumerable<TASK> oldTASKs = oldPROJECT.TASK;
            //IEnumerable<PROJWBS> oldWBSs = oldPROJECT.PROJWBS;

            foreach(TASK currentProjectTASK in currentProjectTASKs)
            {
                TASK findOldTask = oldTASKs.FirstOrDefault(x => x.task_name == currentProjectTASK.task_name && x.PROJWBS.wbs_short_name == currentProjectTASK.PROJWBS.wbs_short_name);
                if(findOldTask != null)
                {
                    P6ActivityRemap findRemapActivity = SourceObjects.FirstOrDefault(x => x.P6_OLD_ACTIVITY == findOldTask.task_code);
                    if(findRemapActivity != null)
                    {
                        findRemapActivity.P6_NEW_ACTIVITY = currentProjectTASK.task_code;
                        findRemapActivity.RaisePropertiesChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Converts clipboard text into entity values
        /// </summary>
        /// <param name="e"></param>
        public void PastingFromClipboard(PastingFromClipboardEventArgs e)
        {
            var PasteString = Clipboard.GetText();
            var RowData = PasteString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sourceGridControl = (GridControl)e.Source;
            foreach (var Row in RowData)
            {
                var ColumnStrings = Row.Split('\t');
                if (ColumnStrings.Count() != 2)
                    break;

                string oldActivityId = ColumnStrings[0];
                P6ActivityRemap findP6ActivityAssignment = SourceObjects.FirstOrDefault(x => x.P6_OLD_ACTIVITY == oldActivityId);
                if(findP6ActivityAssignment != null)
                {
                    findP6ActivityAssignment.P6_NEW_ACTIVITY = ColumnStrings[1];
                    IPOCOViewModel pocoViewModel = findP6ActivityAssignment as IPOCOViewModel;
                    if(pocoViewModel != null)
                        pocoViewModel.RaisePropertiesChanged();
                }
            }
        }
    }
}
