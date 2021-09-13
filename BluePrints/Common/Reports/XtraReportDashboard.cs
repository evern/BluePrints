using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Xpf.Charts;
using DevExpress.XtraCharts;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BluePrints.Reports
{
    public partial class XtraReportDashboard : XtraReport
    {
        public XtraReportDashboard()
        {
            InitializeComponent();
            ParametersRequestSubmit += rptProgressItem_ParametersRequestSubmit;
        }

        private void rptProgressItem_ParametersRequestSubmit(object sender, ParametersRequestEventArgs e)
        {
            var replaceTo = "Units";
            bool showBurned = true;
            bool useLate = false;
            bool showAbsolutes = false;

            foreach (var info in e.ParametersInformation)
            {
                if (info.Parameter.Name == "reportBy")
                    replaceTo = (string)info.Parameter.Value;

                if (info.Parameter.Name == "showBurn")
                    showBurned = (bool)info.Parameter.Value;

                if (info.Parameter.Name == "useLate")
                    useLate = (bool)info.Parameter.Value;

                if (info.Parameter.Name == "showAbsolute")
                    showAbsolutes = (bool)info.Parameter.Value;
            }

            if (showBurned)
            {
                XtraReportDashboard defaultReport = new XtraReportDashboard();
                MemoryStream ms = new MemoryStream();
                defaultReport.SaveLayout(ms);
                this.LoadLayout(ms);

                AssignProperties(ReportData, dataDate, projectTitle);
            }
            else if (!showBurned)
            {
                XtraReportDashboard_NoBurn noBurnReport = new XtraReportDashboard_NoBurn();
                MemoryStream ms = new MemoryStream();
                noBurnReport.SaveLayout(ms);
                this.LoadLayout(ms);

                AssignProperties(ReportData, dataDate, projectTitle);
            }

            bool isCost = replaceTo == "Costs";

            string strBudgetReplaceFrom;
            string strBudgetReplaceTo;

            //string formatString;
            if (useLate)
            {
                strBudgetReplaceFrom = "Budgeted.";
                strBudgetReplaceTo = "BudgetedLate.";
                //formatString = "{0:c}";
            }
            else
            {
                strBudgetReplaceFrom = "BudgetedLate.";
                strBudgetReplaceTo = "Budgeted.";
                //formatString = "{0:n1}";
            }

            string percentageFormatString = "{0:0.00%}";

            //for quantity summary use units
            string percentReplaceTo = isCost ? "Costs" : "Units";

            replaceDataMember(xrDataSummaryCumulativeEarnedPercent, percentReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativePlannedPercent, percentReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, strBudgetReplaceFrom, strBudgetReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativeBurnedPercent, percentReplaceTo, percentageFormatString);

            replaceDataMember(xrDataSummaryPeriodEarnedPercent, percentReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, percentReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, strBudgetReplaceFrom, strBudgetReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodBurnedPercent, percentReplaceTo, percentageFormatString);

            xrChart1.Series["Planned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Planned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Earned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Earned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Earned Tender"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Earned Tender"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Burned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Burned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Remaining"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Remaining"].ValueDataMembersSerializable, isCost, showAbsolutes);

            xrChart1.Series["Period Planned"].ValueDataMembersSerializable = replaceDataMember(xrChart1.Series["Period Planned"].ValueDataMembersSerializable, replaceTo);
            xrChart1.Series["Period Earned"].ValueDataMembersSerializable = replaceDataMember(xrChart1.Series["Period Earned"].ValueDataMembersSerializable, replaceTo);
            xrChart1.Series["Period Burned"].ValueDataMembersSerializable = replaceDataMember(xrChart1.Series["Period Burned"].ValueDataMembersSerializable, replaceTo);
            xrChart1.Series["Period Remaining"].ValueDataMembersSerializable = replaceDataMember(xrChart1.Series["Period Remaining"].ValueDataMembersSerializable, replaceTo);

            XYDiagram xyDiagram = xrChart1.Diagram as XYDiagram;
            if (showAbsolutes)
            {
                xyDiagram.AxisY.Label.TextPattern = "{V:0}";
                xyDiagram.AxisY.Title.Text = isCost ? "Cumulative Costs" : "Cumulative Units";
            }
            else
            {
                xyDiagram.AxisY.Label.TextPattern = "{V:0.00%}";
                xyDiagram.AxisY.Title.Text = isCost ? "Cost % Complete" : "Units % Complete";
            }

            //conditional formatting
            ItemCumulativeEarnedEfficiency_Good.Condition = replaceDataMember(ItemCumulativeEarnedEfficiency_Good.Condition, replaceTo);
            ItemPeriodEarnedEfficiency_Good.Condition = replaceDataMember(ItemPeriodEarnedEfficiency_Good.Condition, replaceTo);
            SummaryPeriodBurnedEfficiency_Good.Condition = replaceDataMember(SummaryPeriodBurnedEfficiency_Good.Condition, replaceTo);
            SummaryCumulativeBurnedEfficiency_Good.Condition = replaceDataMember(SummaryCumulativeBurnedEfficiency_Good.Condition, replaceTo);
            SummaryCumulativeEarnedEfficiency_Good.Condition = replaceDataMember(SummaryCumulativeEarnedEfficiency_Good.Condition, replaceTo);
            SummaryPeriodEarnedEfficiency_Good.Condition = replaceDataMember(SummaryPeriodEarnedEfficiency_Good.Condition, replaceTo);
            ItemCumulativeEarnedEfficiency_Bad.Condition = replaceDataMember(ItemCumulativeEarnedEfficiency_Bad.Condition, replaceTo);
            ItemPeriodEarnedEfficiency_Bad.Condition = replaceDataMember(ItemPeriodEarnedEfficiency_Bad.Condition, replaceTo);
            SummaryPeriodBurnedEfficiency_Bad.Condition = replaceDataMember(SummaryPeriodBurnedEfficiency_Bad.Condition, replaceTo);
            SummaryCumulativeBurnedEfficiency_Bad.Condition = replaceDataMember(SummaryCumulativeBurnedEfficiency_Bad.Condition, replaceTo);
            SummaryCumulativeEarnedEfficiency_Bad.Condition = replaceDataMember(SummaryCumulativeEarnedEfficiency_Bad.Condition, replaceTo);
            SummaryPeriodEarnedEfficiency_Bad.Condition = replaceDataMember(SummaryPeriodEarnedEfficiency_Bad.Condition, replaceTo);
        }

        private string replaceDataMember(string propertyName, string replaceTo)
        {
            string replaceFrom = "Units";
            propertyName = propertyName.Replace(replaceFrom, replaceTo);
            replaceFrom = "Costs";
            propertyName = propertyName.Replace(replaceFrom, replaceTo);

            return propertyName;
        }

        private string replaceChartDataMember(string fullName, bool isCost, bool isAbsolute)
        {
            string[] namePartition = fullName.Split('.');
            if(namePartition.Length == 3)
            {
                string changeString = namePartition[2];
                if (isCost)
                    changeString = "ReportCosts";
                else
                    changeString = "ReportUnits";

                if (!isAbsolute)
                    changeString += "Percentage";

                return namePartition[0] + "." + namePartition[1] + "." + changeString;
            }

            return fullName;
        }

        private void replaceDataMember(XRLabel label, string replaceFrom, string replaceTo, string formatString)
        {
            string propertyName = string.Empty;
            propertyName = label.DataBindings[0].DataMember;
            propertyName = propertyName.Replace(replaceFrom, replaceTo);
            label.DataBindings.Clear();
            label.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                propertyName, formatString));
        }

        private void replaceDataMember(XRLabel label, string replaceTo, string formatString)
        {
            string propertyName = string.Empty;
            propertyName = label.DataBindings[0].DataMember;
            string replaceFrom = "Units";
            propertyName = propertyName.Replace(replaceFrom, replaceTo);
            replaceFrom = "Costs";
            propertyName = propertyName.Replace(replaceFrom, replaceTo);
            label.DataBindings.Clear();
            label.DataBindings.Add(new XRBinding("Text", objectDataSource1, propertyName, formatString));
        }

        private SummaryStats ReportData { get; set; }
        private DateTime dataDate { get; set; }
        private string projectTitle { get; set; }

        public void AssignProperties(SummaryStats reportData, DateTime reportingDataDate, string title)
        {
            ReportData = reportData;
            objectDataSource1.DataSource = ReportData;
            title1.Value = title;
            projectTitle = title;
            dataDate = reportingDataDate;
            datadate1.Value = reportingDataDate;
        }

        private void rptProgressItem_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //objectDataSource1.DataSource = this._ProgressItems;
        }
    }
}