using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using BluePrints.Reports;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UserDesigner;
using System;
using System.IO;
using System.Linq;

namespace BluePrints.Common.Reports
{
    public partial class UserReportDesigner : DevExpress.XtraEditors.XtraForm
    {
        private XtraReport currentREPORT;
        private PROJECT currentPROJECT;
        private PROJECT_REPORT currentPROJECT_REPORT;
        private ReportType currentReportType;
        private CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork> collectionViewModel;

        public UserReportDesigner(PROJECT currentPROJECT,
            CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork> collectionViewModel,
            ReportType currentReportType)
        {
            InitializeComponent();
            this.currentReportType = currentReportType;
            this.collectionViewModel = collectionViewModel;
            this.currentPROJECT = currentPROJECT;
            reportDesigner1.DesignPanelLoaded += new DesignerLoadedEventHandler(reportDesigner1_DesignPanelLoaded);
        }

        private void reportDesigner1_DesignPanelLoaded(object sender, DesignerLoadedEventArgs e)
        {
            var panel = (XRDesignPanel)sender;
            panel.AddCommandHandler(new SaveCommandHandler(panel, SaveReport));
        }

        private void ReportDesigner_Load(object sender, EventArgs e)
        {
            if (collectionViewModel.Entities.Count > 0)
            {
                currentPROJECT_REPORT = collectionViewModel.Entities.First();
                if (currentPROJECT_REPORT != null)
                    using (var sw = new StreamWriter(new MemoryStream()))
                    {
                        sw.Write(currentPROJECT_REPORT.REPORT);
                        sw.Flush();
                        currentREPORT = XtraReport.FromStream(sw.BaseStream, true);

                        reportDesigner1.OpenReport(currentREPORT);
                        return;
                    }
            }

            if (currentReportType == ReportType.Progress_Report)
            {
                currentREPORT = new XtraReportPROGRESS_ITEMS();
                reportDesigner1.OpenReport(currentREPORT);
            }
            else if (currentReportType == ReportType.Baseline_Report)
            {
                currentREPORT = new XtraReportBASELINE_ITEMS();
                reportDesigner1.OpenReport(currentREPORT);
            }
        }

        private void SaveReport()
        {
            if (currentREPORT == null)
                return;

            // Save the report to a stream.
            var ms = new MemoryStream();
            //Prevent user from editing parameters
            //currentREPORT.Parameters.Clear();
            currentREPORT.SaveLayout(ms);

            // Prepare the stream for reading.
            ms.Position = 0;
            // Insert the report to a database.
            using (var sr = new StreamReader(ms))
            {
                // Read the report from the stream to a string variable.
                var reportString = sr.ReadToEnd();
                if (currentPROJECT_REPORT == null)
                {
                    currentPROJECT_REPORT = new PROJECT_REPORT();
                    currentPROJECT_REPORT.GUID_PROJECT = currentPROJECT.GUID;
                    currentPROJECT_REPORT.REPORT_TYPE = currentReportType.ToString();
                    currentPROJECT_REPORT.REPORT = reportString;
                }
                else
                {
                    currentPROJECT_REPORT.REPORT = reportString;
                }

                collectionViewModel.Save(currentPROJECT_REPORT);
            }
        }


        private void bBtnSetDefault_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetDefault();
        }

        private void SetDefault()
        {
            if (currentPROJECT_REPORT != null)
                collectionViewModel.Delete(currentPROJECT_REPORT);

            if (collectionViewModel.Entities.Count > 0)
            {
                currentPROJECT_REPORT = collectionViewModel.Entities.First();
                collectionViewModel.Delete(currentPROJECT_REPORT);
            }

            if (currentReportType == ReportType.Progress_Report)
                currentREPORT = new XtraReportPROGRESS_ITEMS();
            else if (currentReportType == ReportType.Baseline_Report)
                currentREPORT = new XtraReportBASELINE_ITEMS();

            reportDesigner1.OpenReport(currentREPORT);
        }

        public class SaveCommandHandler : ICommandHandler
        {
            private XRDesignPanel panel;

            public delegate void SaveCommandDelegate();

            public SaveCommandDelegate SaveDelegate;

            public SaveCommandHandler(XRDesignPanel panel, SaveCommandDelegate ReportSaveDelegate)
            {
                this.panel = panel;
                SaveDelegate = ReportSaveDelegate;
            }

            public void HandleCommand(ReportCommand command,
                object[] args)
            {
                // Save the report.
                Save();
            }

            public bool CanHandleCommand(ReportCommand command,
                ref bool useNextHandler)
            {
                useNextHandler = !(command == ReportCommand.SaveFile);
                return !useNextHandler;
            }

            private void Save()
            {
                // For instance:
                SaveDelegate?.Invoke();

                // Prevent the "Report has been changed" dialog from being shown.
                panel.ReportState = ReportState.Saved;
            }
        }
    }
}