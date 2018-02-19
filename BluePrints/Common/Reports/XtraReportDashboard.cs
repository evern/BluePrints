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

            string strReplaceFrom;
            string strReplaceTo;
            bool isCost;
            //string formatString;
            if (replaceTo == "Costs")
            {
                strReplaceFrom = "Units";
                strReplaceTo = "Costs";
                isCost = true;
                //formatString = "{0:c}";
            }
            else
            {
                strReplaceFrom = "Costs";
                strReplaceTo = "Units";
                isCost = false;
                //formatString = "{0:n1}";
            }



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

            replaceDataMember(xrDataSummaryCumulativeEarnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativePlannedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativePlannedPercent, strBudgetReplaceFrom, strBudgetReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativeBurnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataSummaryPeriodEarnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, strBudgetReplaceFrom, strBudgetReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodBurnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);

            xrChart1.Series["Planned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Planned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Earned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Earned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Burned"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Burned"].ValueDataMembersSerializable, isCost, showAbsolutes);
            xrChart1.Series["Remaining"].ValueDataMembersSerializable = replaceChartDataMember(xrChart1.Series["Remaining"].ValueDataMembersSerializable, isCost, showAbsolutes);

            xrChart1.Series["Period Planned"].ValueDataMembersSerializable = xrChart1.Series["Period Planned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            //xrChart1.Series["Period Late Planned"].ValueDataMembersSerializable = xrChart1.Series["Period Late Planned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Earned"].ValueDataMembersSerializable = xrChart1.Series["Period Earned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Burned"].ValueDataMembersSerializable = xrChart1.Series["Period Burned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Remaining"].ValueDataMembersSerializable = xrChart1.Series["Period Remaining"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);

            XYDiagram xyDiagram = xrChart1.Diagram as XYDiagram;
            if (showAbsolutes)
            {
                xyDiagram.AxisY.Label.TextPattern = "{V:0}";
                xyDiagram.AxisY.Title.Text = isCost ? "Costs" : "Units";
            }
            else
            {
                xyDiagram.AxisY.Label.TextPattern = "{V:0.00%}";
                xyDiagram.AxisY.Title.Text = "Percentages";
            }

            //conditional formatting
            ItemCumulativeEarnedEfficiency_Good.Condition =
                ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemCumulativeEarnedEfficiency_Good.Condition =
                ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemPeriodEarnedEfficiency_Good.Condition =
                ItemPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodBurnedEfficiency_Good.Condition =
                SummaryPeriodBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeBurnedEfficiency_Good.Condition =
                SummaryCumulativeBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeEarnedEfficiency_Good.Condition =
                SummaryCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodEarnedEfficiency_Good.Condition =
                SummaryPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemCumulativeEarnedEfficiency_Bad.Condition =
                ItemCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemPeriodEarnedEfficiency_Bad.Condition =
                ItemPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodBurnedEfficiency_Bad.Condition =
                SummaryPeriodBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeBurnedEfficiency_Bad.Condition =
                SummaryCumulativeBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeEarnedEfficiency_Bad.Condition =
                SummaryCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodEarnedEfficiency_Bad.Condition =
                SummaryPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
        }

        private string replaceChartDataMember(string fullName, bool isCost, bool isAbsolute)
        {
            string[] namePartition = fullName.Split('.');
            if(namePartition.Length == 3)
            {
                string changeString = namePartition[2];
                if (isCost)
                    changeString = "Costs";
                else
                    changeString = "Units";

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