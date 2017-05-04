using BaseModel.DataModel;
using BluePrints.P6Data;
using System;

namespace BluePrints.P6EntitiesDataModel
{
    /// <summary>
    /// IP6EntitiesUnitOfWork extends the IUnitOfWork interface with repositories representing specific entities.
    /// </summary>
    public interface IP6EntitiesUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The ACCOUNT entities repository.
        /// </summary>
        IRepository<ACCOUNT, int> ACCOUNT { get; }

        /// <summary>
        /// The ACTVCODE entities repository.
        /// </summary>
        IRepository<ACTVCODE, int> ACTVCODE { get; }

        /// <summary>
        /// The ACTVTYPE entities repository.
        /// </summary>
        IRepository<ACTVTYPE, int> ACTVTYPE { get; }

        /// <summary>
        /// The ADMIN_CONFIG entities repository.
        /// </summary>
        IRepository<ADMIN_CONFIG, string> ADMIN_CONFIG { get; }

        /// <summary>
        /// The BASETYPE entities repository.
        /// </summary>
        IRepository<BASETYPE, int> BASETYPE { get; }

        /// <summary>
        /// The BRE_REGISTRY entities repository.
        /// </summary>
        IRepository<BRE_REGISTRY, string> BRE_REGISTRY { get; }

        /// <summary>
        /// The BUDGCHNG entities repository.
        /// </summary>
        IRepository<BUDGCHNG, int> BUDGCHNG { get; }

        /// <summary>
        /// The CALENDAR entities repository.
        /// </summary>
        IRepository<CALENDAR, int> CALENDAR { get; }

        /// <summary>
        /// The COSTTYPE entities repository.
        /// </summary>
        IRepository<COSTTYPE, int> COSTTYPE { get; }

        /// <summary>
        /// The CURRTYPE entities repository.
        /// </summary>
        IRepository<CURRTYPE, int> CURRTYPE { get; }

        /// <summary>
        /// The DASHBOARD entities repository.
        /// </summary>
        IRepository<DASHBOARD, int> DASHBOARD { get; }

        /// <summary>
        /// The DASHUSER entities repository.
        /// </summary>
        IRepository<DASHUSER, Tuple<int, int>> DASHUSER { get; }

        /// <summary>
        /// The DISCUSSION entities repository.
        /// </summary>
        IRepository<DISCUSSION, int> DISCUSSION { get; }

        /// <summary>
        /// The DISCUSSION_READ entities repository.
        /// </summary>
        IRepository<DISCUSSION_READ, Tuple<int, int>> DISCUSSION_READ { get; }

        /// <summary>
        /// The DLTACCT entities repository.
        /// </summary>
        IRepository<DLTACCT, Tuple<int, int>> DLTACCT { get; }

        /// <summary>
        /// The DLTACTV entities repository.
        /// </summary>
        IRepository<DLTACTV, Tuple<int, int>> DLTACTV { get; }

        /// <summary>
        /// The DLTOBS entities repository.
        /// </summary>
        IRepository<DLTOBS, Tuple<int, int>> DLTOBS { get; }

        /// <summary>
        /// The DLTROLE entities repository.
        /// </summary>
        IRepository<DLTROLE, Tuple<int, int>> DLTROLE { get; }

        /// <summary>
        /// The DLTRSRC entities repository.
        /// </summary>
        IRepository<DLTRSRC, Tuple<int, int>> DLTRSRC { get; }

        /// <summary>
        /// The DLTRSRL entities repository.
        /// </summary>
        IReadOnlyRepository<DLTRSRL> DLTRSRL { get; }

        /// <summary>
        /// The DLTUSER entities repository.
        /// </summary>
        IRepository<DLTUSER, Tuple<int, int>> DLTUSER { get; }

        /// <summary>
        /// The DOCCATG entities repository.
        /// </summary>
        IRepository<DOCCATG, int> DOCCATG { get; }

        /// <summary>
        /// The DOCREVIEW entities repository.
        /// </summary>
        IRepository<DOCREVIEW, int> DOCREVIEW { get; }

        /// <summary>
        /// The DOCREVIEWTASK entities repository.
        /// </summary>
        IRepository<DOCREVIEWTASK, int> DOCREVIEWTASK { get; }

        /// <summary>
        /// The DOCSTAT entities repository.
        /// </summary>
        IRepository<DOCSTAT, int> DOCSTAT { get; }

        /// <summary>
        /// The DOCUMENT entities repository.
        /// </summary>
        IRepository<DOCUMENT, int> DOCUMENT { get; }

        /// <summary>
        /// The EXPPROJ entities repository.
        /// </summary>
        IRepository<EXPPROJ, int> EXPPROJ { get; }

        /// <summary>
        /// The EXTAPP entities repository.
        /// </summary>
        IRepository<EXTAPP, int> EXTAPP { get; }

        /// <summary>
        /// The FACTOR entities repository.
        /// </summary>
        IRepository<FACTOR, int> FACTOR { get; }

        /// <summary>
        /// The FACTVAL entities repository.
        /// </summary>
        IRepository<FACTVAL, int> FACTVAL { get; }

        /// <summary>
        /// The FILTPROP entities repository.
        /// </summary>
        IRepository<FILTPROP, int> FILTPROP { get; }

        /// <summary>
        /// The FINDATES entities repository.
        /// </summary>
        IRepository<FINDATES, int> FINDATES { get; }

        /// <summary>
        /// The FORMCATG entities repository.
        /// </summary>
        IRepository<FORMCATG, int> FORMCATG { get; }

        /// <summary>
        /// The FORMPROJ entities repository.
        /// </summary>
        IRepository<FORMPROJ, Tuple<int, int>> FORMPROJ { get; }

        /// <summary>
        /// The FORMTMPL entities repository.
        /// </summary>
        IRepository<FORMTMPL, int> FORMTMPL { get; }

        /// <summary>
        /// The FUNDSRC entities repository.
        /// </summary>
        IRepository<FUNDSRC, int> FUNDSRC { get; }

        /// <summary>
        /// The GCHANGE entities repository.
        /// </summary>
        IRepository<GCHANGE, int> GCHANGE { get; }

        /// <summary>
        /// The HQDATA entities repository.
        /// </summary>
        IReadOnlyRepository<HQDATA> HQDATA { get; }

        /// <summary>
        /// The HQUERY entities repository.
        /// </summary>
        IRepository<HQUERY, Tuple<int, int, int>> HQUERY { get; }

        /// <summary>
        /// The ISSUHIST entities repository.
        /// </summary>
        IRepository<ISSUHIST, int> ISSUHIST { get; }

        /// <summary>
        /// The ITERATION entities repository.
        /// </summary>
        IRepository<ITERATION, int> ITERATION { get; }

        /// <summary>
        /// The ITERDAYS entities repository.
        /// </summary>
        IRepository<ITERDAYS, Tuple<int, int>> ITERDAYS { get; }

        /// <summary>
        /// The ITERGOAL entities repository.
        /// </summary>
        IRepository<ITERGOAL, int> ITERGOAL { get; }

        /// <summary>
        /// The JOBLOG entities repository.
        /// </summary>
        IRepository<JOBLOG, int> JOBLOG { get; }

        /// <summary>
        /// The JOBRPT entities repository.
        /// </summary>
        IRepository<JOBRPT, Tuple<int, int>> JOBRPT { get; }

        /// <summary>
        /// The JOBSVC entities repository.
        /// </summary>
        IRepository<JOBSVC, int> JOBSVC { get; }

        /// <summary>
        /// The LOCATION entities repository.
        /// </summary>
        IRepository<LOCATION, int> LOCATION { get; }

        /// <summary>
        /// The MEMOTYPE entities repository.
        /// </summary>
        IRepository<MEMOTYPE, int> MEMOTYPE { get; }

        /// <summary>
        /// The NEXTKEY entities repository.
        /// </summary>
        IRepository<NEXTKEY, string> NEXTKEY { get; }

        /// <summary>
        /// The NONWORK entities repository.
        /// </summary>
        IRepository<NONWORK, int> NONWORK { get; }

        /// <summary>
        /// The NOTE entities repository.
        /// </summary>
        IRepository<NOTE, int> NOTE { get; }

        /// <summary>
        /// The OBS entities repository.
        /// </summary>
        IRepository<OBS, int> OBS { get; }

        /// <summary>
        /// The OBSPROJ entities repository.
        /// </summary>
        IReadOnlyRepository<OBSPROJ> OBSPROJ { get; }

        /// <summary>
        /// The PCATTYPE entities repository.
        /// </summary>
        IRepository<PCATTYPE, int> PCATTYPE { get; }

        /// <summary>
        /// The PCATUSER entities repository.
        /// </summary>
        IRepository<PCATUSER, Tuple<int, int>> PCATUSER { get; }

        /// <summary>
        /// The PCATVAL entities repository.
        /// </summary>
        IRepository<PCATVAL, int> PCATVAL { get; }

        /// <summary>
        /// The PFOLIO entities repository.
        /// </summary>
        IRepository<PFOLIO, int> PFOLIO { get; }

        /// <summary>
        /// The PHASE entities repository.
        /// </summary>
        IRepository<PHASE, int> PHASE { get; }

        /// <summary>
        /// The POBS entities repository.
        /// </summary>
        IRepository<POBS, int> POBS { get; }

        /// <summary>
        /// The PREFER entities repository.
        /// </summary>
        IRepository<PREFER, int> PREFER { get; }

        /// <summary>
        /// The PRMQUEUE entities repository.
        /// </summary>
        IRepository<PRMQUEUE, Tuple<string, string, DateTime>> PRMQUEUE { get; }

        /// <summary>
        /// The PROCGROUP entities repository.
        /// </summary>
        IRepository<PROCGROUP, int> PROCGROUP { get; }

        /// <summary>
        /// The PROCITEM entities repository.
        /// </summary>
        IRepository<PROCITEM, int> PROCITEM { get; }

        /// <summary>
        /// The PROFILE entities repository.
        /// </summary>
        IRepository<PROFILE, int> PROFILE { get; }

        /// <summary>
        /// The PROFPRIV entities repository.
        /// </summary>
        IRepository<PROFPRIV, Tuple<int, int>> PROFPRIV { get; }

        /// <summary>
        /// The PROJCOST entities repository.
        /// </summary>
        IRepository<PROJCOST, int> PROJCOST { get; }

        /// <summary>
        /// The PROJECT entities repository.
        /// </summary>
        IRepository<PROJECT, int> PROJECT { get; }

        /// <summary>
        /// The PROJEST entities repository.
        /// </summary>
        IRepository<PROJEST, int> PROJEST { get; }

        /// <summary>
        /// The PROJFUND entities repository.
        /// </summary>
        IRepository<PROJFUND, int> PROJFUND { get; }

        /// <summary>
        /// The PROJISSU entities repository.
        /// </summary>
        IRepository<PROJISSU, int> PROJISSU { get; }

        /// <summary>
        /// The PROJPCAT entities repository.
        /// </summary>
        IRepository<PROJPCAT, Tuple<int, int>> PROJPCAT { get; }

        /// <summary>
        /// The PROJPROP entities repository.
        /// </summary>
        IRepository<PROJPROP, Tuple<int, string>> PROJPROP { get; }

        /// <summary>
        /// The PROJRISK entities repository.
        /// </summary>
        IRepository<PROJRISK, int> PROJRISK { get; }

        /// <summary>
        /// The PROJSHAR entities repository.
        /// </summary>
        IRepository<PROJSHAR, Tuple<int, int>> PROJSHAR { get; }

        /// <summary>
        /// The PROJTHRS entities repository.
        /// </summary>
        IRepository<PROJTHRS, int> PROJTHRS { get; }

        /// <summary>
        /// The PROJWBS entities repository.
        /// </summary>
        IRepository<PROJWBS, int> PROJWBS { get; }

        /// <summary>
        /// The PROJWSRPT entities repository.
        /// </summary>
        IRepository<PROJWSRPT, Tuple<int, int>> PROJWSRPT { get; }

        /// <summary>
        /// The PRPFOLIO entities repository.
        /// </summary>
        IRepository<PRPFOLIO, Tuple<int, int>> PRPFOLIO { get; }

        /// <summary>
        /// The PUBUSER entities repository.
        /// </summary>
        IRepository<PUBUSER, int> PUBUSER { get; }

        /// <summary>
        /// The QUERYLIB entities repository.
        /// </summary>
        IRepository<QUERYLIB, int> QUERYLIB { get; }

        /// <summary>
        /// The RCATTYPE entities repository.
        /// </summary>
        IRepository<RCATTYPE, int> RCATTYPE { get; }

        /// <summary>
        /// The RCATVAL entities repository.
        /// </summary>
        IRepository<RCATVAL, int> RCATVAL { get; }

        /// <summary>
        /// The REFRDEL entities repository.
        /// </summary>
        IRepository<REFRDEL, Tuple<DateTime, string, string>> REFRDEL { get; }

        /// <summary>
        /// The REITTYPE entities repository.
        /// </summary>
        IRepository<REITTYPE, int> REITTYPE { get; }

        /// <summary>
        /// The RELEASE entities repository.
        /// </summary>
        IRepository<RELEASE, int> RELEASE { get; }

        /// <summary>
        /// The RELITEMS entities repository.
        /// </summary>
        IRepository<RELITEMS, Tuple<int, int, int, int>> RELITEMS { get; }

        /// <summary>
        /// The RFOLIO entities repository.
        /// </summary>
        IRepository<RFOLIO, int> RFOLIO { get; }

        /// <summary>
        /// The RISKCTRL entities repository.
        /// </summary>
        IRepository<RISKCTRL, int> RISKCTRL { get; }

        /// <summary>
        /// The RISKTYPE entities repository.
        /// </summary>
        IRepository<RISKTYPE, int> RISKTYPE { get; }

        /// <summary>
        /// The RLFOLIO entities repository.
        /// </summary>
        IRepository<RLFOLIO, int> RLFOLIO { get; }

        /// <summary>
        /// The ROLELIMIT entities repository.
        /// </summary>
        IRepository<ROLELIMIT, int> ROLELIMIT { get; }

        /// <summary>
        /// The ROLERATE entities repository.
        /// </summary>
        IRepository<ROLERATE, int> ROLERATE { get; }

        /// <summary>
        /// The ROLES entities repository.
        /// </summary>
        IRepository<ROLES, int> ROLES { get; }

        /// <summary>
        /// The ROLFOLIO entities repository.
        /// </summary>
        IRepository<ROLFOLIO, Tuple<int, int>> ROLFOLIO { get; }

        /// <summary>
        /// The RPT entities repository.
        /// </summary>
        IRepository<RPT, int> RPT { get; }

        /// <summary>
        /// The RPTBATCH entities repository.
        /// </summary>
        IRepository<RPTBATCH, int> RPTBATCH { get; }

        /// <summary>
        /// The RPTGROUP entities repository.
        /// </summary>
        IRepository<RPTGROUP, int> RPTGROUP { get; }

        /// <summary>
        /// The RPTLIST entities repository.
        /// </summary>
        IRepository<RPTLIST, Tuple<int, int>> RPTLIST { get; }

        /// <summary>
        /// The RSRC entities repository.
        /// </summary>
        IRepository<RSRC, int> RSRC { get; }

        /// <summary>
        /// The RSRCANDASH entities repository.
        /// </summary>
        IRepository<RSRCANDASH, Tuple<int, int>> RSRCANDASH { get; }

        /// <summary>
        /// The RSRCANVIEW entities repository.
        /// </summary>
        IRepository<RSRCANVIEW, int> RSRCANVIEW { get; }

        /// <summary>
        /// The RSRCCURV entities repository.
        /// </summary>
        IRepository<RSRCCURV, int> RSRCCURV { get; }

        /// <summary>
        /// The RSRCHOUR entities repository.
        /// </summary>
        IRepository<RSRCHOUR, int> RSRCHOUR { get; }

        /// <summary>
        /// The RSRCPROP entities repository.
        /// </summary>
        IRepository<RSRCPROP, Tuple<int, string>> RSRCPROP { get; }

        /// <summary>
        /// The RSRCRATE entities repository.
        /// </summary>
        IRepository<RSRCRATE, int> RSRCRATE { get; }

        /// <summary>
        /// The RSRCRCAT entities repository.
        /// </summary>
        IRepository<RSRCRCAT, Tuple<int, int>> RSRCRCAT { get; }

        /// <summary>
        /// The RSRCROLE entities repository.
        /// </summary>
        IRepository<RSRCROLE, Tuple<int, int>> RSRCROLE { get; }

        /// <summary>
        /// The RSRCSEC entities repository.
        /// </summary>
        IRepository<RSRCSEC, Tuple<int, int>> RSRCSEC { get; }

        /// <summary>
        /// The RSRFOLIO entities repository.
        /// </summary>
        IRepository<RSRFOLIO, Tuple<int, int>> RSRFOLIO { get; }

        /// <summary>
        /// The SCENARIO entities repository.
        /// </summary>
        IRepository<SCENARIO, int> SCENARIO { get; }

        /// <summary>
        /// The SCENPROJ entities repository.
        /// </summary>
        IRepository<SCENPROJ, Tuple<int, int>> SCENPROJ { get; }

        /// <summary>
        /// The SCENROLE entities repository.
        /// </summary>
        IRepository<SCENROLE, Tuple<int, int>> SCENROLE { get; }

        /// <summary>
        /// The SCENUSER entities repository.
        /// </summary>
        IRepository<SCENUSER, Tuple<int, int>> SCENUSER { get; }

        /// <summary>
        /// The SETTINGS entities repository.
        /// </summary>
        IRepository<SETTINGS, Tuple<string, string>> SETTINGS { get; }

        /// <summary>
        /// The SHIFT entities repository.
        /// </summary>
        IRepository<SHIFT, int> SHIFT { get; }

        /// <summary>
        /// The SHIFTPER entities repository.
        /// </summary>
        IRepository<SHIFTPER, int> SHIFTPER { get; }

        /// <summary>
        /// The SPIDMAP entities repository.
        /// </summary>
        IRepository<SPIDMAP, int> SPIDMAP { get; }

        /// <summary>
        /// The SUMPROJCOST entities repository.
        /// </summary>
        IRepository<SUMPROJCOST, Tuple<int, int, int>> SUMPROJCOST { get; }

        /// <summary>
        /// The SUMTASK entities repository.
        /// </summary>
        IRepository<SUMTASK, Tuple<int, int>> SUMTASK { get; }

        /// <summary>
        /// The SUMTASKSPREAD entities repository.
        /// </summary>
        IRepository<SUMTASKSPREAD, Tuple<int, int, DateTime, DateTime, string>> SUMTASKSPREAD { get; }

        /// <summary>
        /// The SUMTRSRC entities repository.
        /// </summary>
        IRepository<SUMTRSRC, int> SUMTRSRC { get; }

        /// <summary>
        /// The TASK entities repository.
        /// </summary>
        IRepository<TASK, int> TASK { get; }

        /// <summary>
        /// The TASKACTV entities repository.
        /// </summary>
        IRepository<TASKACTV, Tuple<int, int>> TASKACTV { get; }

        /// <summary>
        /// The TASKDOC entities repository.
        /// </summary>
        IRepository<TASKDOC, int> TASKDOC { get; }

        /// <summary>
        /// The TASKFDBK entities repository.
        /// </summary>
        IRepository<TASKFDBK, int> TASKFDBK { get; }

        /// <summary>
        /// The TASKFIN entities repository.
        /// </summary>
        IRepository<TASKFIN, Tuple<int, int>> TASKFIN { get; }

        /// <summary>
        /// The TASKMEMO entities repository.
        /// </summary>
        IRepository<TASKMEMO, int> TASKMEMO { get; }

        /// <summary>
        /// The TASKNOTE entities repository.
        /// </summary>
        IRepository<TASKNOTE, int> TASKNOTE { get; }

        /// <summary>
        /// The TASKPRED entities repository.
        /// </summary>
        IRepository<TASKPRED, int> TASKPRED { get; }

        /// <summary>
        /// The TASKPROC entities repository.
        /// </summary>
        IRepository<TASKPROC, int> TASKPROC { get; }

        /// <summary>
        /// The TASKRISK entities repository.
        /// </summary>
        IRepository<TASKRISK, Tuple<int, int>> TASKRISK { get; }

        /// <summary>
        /// The TASKRSRC entities repository.
        /// </summary>
        IRepository<TASKRSRC, int> TASKRSRC { get; }

        /// <summary>
        /// The TASKSUM entities repository.
        /// </summary>
        IRepository<TASKSUM, int> TASKSUM { get; }

        /// <summary>
        /// The TASKSUMFIN entities repository.
        /// </summary>
        IRepository<TASKSUMFIN, int> TASKSUMFIN { get; }

        /// <summary>
        /// The TASKUSER entities repository.
        /// </summary>
        IRepository<TASKUSER, Tuple<int, int>> TASKUSER { get; }

        /// <summary>
        /// The TASKWKSP entities repository.
        /// </summary>
        IRepository<TASKWKSP, Tuple<int, int>> TASKWKSP { get; }

        /// <summary>
        /// The THRSPARM entities repository.
        /// </summary>
        IRepository<THRSPARM, int> THRSPARM { get; }

        /// <summary>
        /// The TIMESHT entities repository.
        /// </summary>
        IRepository<TIMESHT, Tuple<int, int>> TIMESHT { get; }

        /// <summary>
        /// The TMPLCATG entities repository.
        /// </summary>
        IRepository<TMPLCATG, int> TMPLCATG { get; }

        /// <summary>
        /// The TPROJMAP entities repository.
        /// </summary>
        IReadOnlyRepository<TPROJMAP> TPROJMAP { get; }

        /// <summary>
        /// The TRAKVIEW entities repository.
        /// </summary>
        IRepository<TRAKVIEW, int> TRAKVIEW { get; }

        /// <summary>
        /// The TRSRCFIN entities repository.
        /// </summary>
        IRepository<TRSRCFIN, Tuple<int, int>> TRSRCFIN { get; }

        /// <summary>
        /// The TRSRCSUM entities repository.
        /// </summary>
        IRepository<TRSRCSUM, int> TRSRCSUM { get; }

        /// <summary>
        /// The TRSRCSUMFN entities repository.
        /// </summary>
        IRepository<TRSRCSUMFN, int> TRSRCSUMFN { get; }

        /// <summary>
        /// The TSAUDIT entities repository.
        /// </summary>
        IRepository<TSAUDIT, int> TSAUDIT { get; }

        /// <summary>
        /// The TSDATES entities repository.
        /// </summary>
        IRepository<TSDATES, int> TSDATES { get; }

        /// <summary>
        /// The TSDELEGATE entities repository.
        /// </summary>
        IRepository<TSDELEGATE, int> TSDELEGATE { get; }

        /// <summary>
        /// The UACCESS entities repository.
        /// </summary>
        IRepository<UACCESS, Tuple<int, int>> UACCESS { get; }

        /// <summary>
        /// The UDFCODE entities repository.
        /// </summary>
        IRepository<UDFCODE, int> UDFCODE { get; }

        /// <summary>
        /// The UDFTYPE entities repository.
        /// </summary>
        IRepository<UDFTYPE, int> UDFTYPE { get; }

        /// <summary>
        /// The UDFVALUE entities repository.
        /// </summary>
        IRepository<UDFVALUE, Tuple<int, int>> UDFVALUE { get; }

        /// <summary>
        /// The UEVNTREG entities repository.
        /// </summary>
        IRepository<UEVNTREG, Tuple<int, string, string, string>> UEVNTREG { get; }

        /// <summary>
        /// The UMEASURE entities repository.
        /// </summary>
        IRepository<UMEASURE, int> UMEASURE { get; }

        /// <summary>
        /// The UPKLIST entities repository.
        /// </summary>
        IRepository<UPKLIST, Tuple<int, int>> UPKLIST { get; }

        /// <summary>
        /// The USERCOL entities repository.
        /// </summary>
        IRepository<USERCOL, int> USERCOL { get; }

        /// <summary>
        /// The USERDATA entities repository.
        /// </summary>
        IRepository<USERDATA, int> USERDATA { get; }

        /// <summary>
        /// The USERENG entities repository.
        /// </summary>
        IRepository<USERENG, int> USERENG { get; }

        /// <summary>
        /// The USEROBS entities repository.
        /// </summary>
        IRepository<USEROBS, Tuple<int, int>> USEROBS { get; }

        /// <summary>
        /// The USEROPEN entities repository.
        /// </summary>
        IRepository<USEROPEN, int> USEROPEN { get; }

        /// <summary>
        /// The USERS entities repository.
        /// </summary>
        IRepository<USERS, int> USERS { get; }

        /// <summary>
        /// The USERSET entities repository.
        /// </summary>
        IRepository<USERSET, Tuple<string, string, int>> USERSET { get; }

        /// <summary>
        /// The USERWKSP entities repository.
        /// </summary>
        IRepository<USERWKSP, Tuple<int, int>> USERWKSP { get; }

        /// <summary>
        /// The USESSION entities repository.
        /// </summary>
        IRepository<USESSION, int> USESSION { get; }

        /// <summary>
        /// The USROPNVAL entities repository.
        /// </summary>
        IRepository<USROPNVAL, Tuple<int, int, string>> USROPNVAL { get; }

        /// <summary>
        /// The VIEWPREF entities repository.
        /// </summary>
        IRepository<VIEWPREF, int> VIEWPREF { get; }

        /// <summary>
        /// The VIEWPROP entities repository.
        /// </summary>
        IRepository<VIEWPROP, int> VIEWPROP { get; }

        /// <summary>
        /// The VWPREFDASH entities repository.
        /// </summary>
        IRepository<VWPREFDASH, Tuple<int, int>> VWPREFDASH { get; }

        /// <summary>
        /// The VWPREFDATA entities repository.
        /// </summary>
        IRepository<VWPREFDATA, Tuple<int, string>> VWPREFDATA { get; }

        /// <summary>
        /// The VWPREFUSER entities repository.
        /// </summary>
        IRepository<VWPREFUSER, Tuple<int, int>> VWPREFUSER { get; }

        /// <summary>
        /// The WBRSCAT entities repository.
        /// </summary>
        IRepository<WBRSCAT, int> WBRSCAT { get; }

        /// <summary>
        /// The WBSBUDG entities repository.
        /// </summary>
        IRepository<WBSBUDG, int> WBSBUDG { get; }

        /// <summary>
        /// The WBSMEMO entities repository.
        /// </summary>
        IRepository<WBSMEMO, int> WBSMEMO { get; }

        /// <summary>
        /// The WBSRSRC entities repository.
        /// </summary>
        IRepository<WBSRSRC, int> WBSRSRC { get; }

        /// <summary>
        /// The WBSRSRC_QTY entities repository.
        /// </summary>
        IRepository<WBSRSRC_QTY, Tuple<int, DateTime, DateTime>> WBSRSRC_QTY { get; }

        /// <summary>
        /// The WBSSTEP entities repository.
        /// </summary>
        IRepository<WBSSTEP, int> WBSSTEP { get; }

        /// <summary>
        /// The WKFLTMPL entities repository.
        /// </summary>
        IRepository<WKFLTMPL, int> WKFLTMPL { get; }

        /// <summary>
        /// The WKFLUSER entities repository.
        /// </summary>
        IRepository<WKFLUSER, Tuple<int, int, int>> WKFLUSER { get; }

        /// <summary>
        /// The WORKFLOW entities repository.
        /// </summary>
        IRepository<WORKFLOW, int> WORKFLOW { get; }

        /// <summary>
        /// The WORKSPACE entities repository.
        /// </summary>
        IRepository<WORKSPACE, int> WORKSPACE { get; }
    }
}