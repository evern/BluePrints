using BaseModel.DataModel;
using BaseModel.DataModel.DesignTime;
using BluePrints.P6Data;
using System;

namespace BluePrints.P6EntitiesDataModel
{
    /// <summary>
    /// A P6EntitiesDesignTimeUnitOfWork instance that represents the design-time implementation of the IP6EntitiesUnitOfWork interface.
    /// </summary>
    public class P6EntitiesDesignTimeUnitOfWork : DesignTimeUnitOfWork, IP6EntitiesUnitOfWork
    {
        /// <summary>
        /// Initializes a new instance of the P6EntitiesDesignTimeUnitOfWork class.
        /// </summary>
        public P6EntitiesDesignTimeUnitOfWork()
        {
        }

        IRepository<ACCOUNT, int> IP6EntitiesUnitOfWork.ACCOUNT
        {
            get { return GetRepository((ACCOUNT x) => x.acct_id); }
        }

        IRepository<ACTVCODE, int> IP6EntitiesUnitOfWork.ACTVCODE
        {
            get { return GetRepository((ACTVCODE x) => x.actv_code_id); }
        }

        IRepository<ACTVTYPE, int> IP6EntitiesUnitOfWork.ACTVTYPE
        {
            get { return GetRepository((ACTVTYPE x) => x.actv_code_type_id); }
        }

        IRepository<ADMIN_CONFIG, string> IP6EntitiesUnitOfWork.ADMIN_CONFIG
        {
            get { return GetRepository((ADMIN_CONFIG x) => x.config_name); }
        }

        IRepository<BASETYPE, int> IP6EntitiesUnitOfWork.BASETYPE
        {
            get { return GetRepository((BASETYPE x) => x.base_type_id); }
        }

        IRepository<BRE_REGISTRY, string> IP6EntitiesUnitOfWork.BRE_REGISTRY
        {
            get { return GetRepository((BRE_REGISTRY x) => x.bre_registry_id); }
        }

        IRepository<BUDGCHNG, int> IP6EntitiesUnitOfWork.BUDGCHNG
        {
            get { return GetRepository((BUDGCHNG x) => x.budg_chng_id); }
        }

        IRepository<CALENDAR, int> IP6EntitiesUnitOfWork.CALENDAR
        {
            get { return GetRepository((CALENDAR x) => x.clndr_id); }
        }

        IRepository<COSTTYPE, int> IP6EntitiesUnitOfWork.COSTTYPE
        {
            get { return GetRepository((COSTTYPE x) => x.cost_type_id); }
        }

        IRepository<CURRTYPE, int> IP6EntitiesUnitOfWork.CURRTYPE
        {
            get { return GetRepository((CURRTYPE x) => x.curr_id); }
        }

        IRepository<DASHBOARD, int> IP6EntitiesUnitOfWork.DASHBOARD
        {
            get { return GetRepository((DASHBOARD x) => x.dashboard_id); }
        }

        IRepository<DASHUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.DASHUSER
        {
            get { return GetRepository((DASHUSER x) => Tuple.Create(x.dashboard_id, x.user_id)); }
        }

        IRepository<DISCUSSION, int> IP6EntitiesUnitOfWork.DISCUSSION
        {
            get { return GetRepository((DISCUSSION x) => x.discussion_id); }
        }

        IRepository<DISCUSSION_READ, Tuple<int, int>> IP6EntitiesUnitOfWork.DISCUSSION_READ
        {
            get { return GetRepository((DISCUSSION_READ x) => Tuple.Create(x.discussion_id, x.user_id)); }
        }

        IRepository<DLTACCT, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTACCT
        {
            get { return GetRepository((DLTACCT x) => Tuple.Create(x.session_id, x.acct_id)); }
        }

        IRepository<DLTACTV, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTACTV
        {
            get { return GetRepository((DLTACTV x) => Tuple.Create(x.session_id, x.actv_code_id)); }
        }

        IRepository<DLTOBS, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTOBS
        {
            get { return GetRepository((DLTOBS x) => Tuple.Create(x.session_id, x.obs_id)); }
        }

        IRepository<DLTROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTROLE
        {
            get { return GetRepository((DLTROLE x) => Tuple.Create(x.session_id, x.role_id)); }
        }

        IRepository<DLTRSRC, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTRSRC
        {
            get { return GetRepository((DLTRSRC x) => Tuple.Create(x.session_id, x.rsrc_id)); }
        }

        IReadOnlyRepository<DLTRSRL> IP6EntitiesUnitOfWork.DLTRSRL
        {
            get { return GetReadOnlyRepository<DLTRSRL>(); }
        }

        IRepository<DLTUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTUSER
        {
            get { return GetRepository((DLTUSER x) => Tuple.Create(x.session_id, x.user_id)); }
        }

        IRepository<DOCCATG, int> IP6EntitiesUnitOfWork.DOCCATG
        {
            get { return GetRepository((DOCCATG x) => x.doc_catg_id); }
        }

        IRepository<DOCREVIEW, int> IP6EntitiesUnitOfWork.DOCREVIEW
        {
            get { return GetRepository((DOCREVIEW x) => x.doc_review_id); }
        }

        IRepository<DOCREVIEWTASK, int> IP6EntitiesUnitOfWork.DOCREVIEWTASK
        {
            get { return GetRepository((DOCREVIEWTASK x) => x.review_task_id); }
        }

        IRepository<DOCSTAT, int> IP6EntitiesUnitOfWork.DOCSTAT
        {
            get { return GetRepository((DOCSTAT x) => x.doc_status_id); }
        }

        IRepository<DOCUMENT, int> IP6EntitiesUnitOfWork.DOCUMENT
        {
            get { return GetRepository((DOCUMENT x) => x.doc_id); }
        }

        IRepository<EXPPROJ, int> IP6EntitiesUnitOfWork.EXPPROJ
        {
            get { return GetRepository((EXPPROJ x) => x.proj_id); }
        }

        IRepository<EXTAPP, int> IP6EntitiesUnitOfWork.EXTAPP
        {
            get { return GetRepository((EXTAPP x) => x.app_id); }
        }

        IRepository<FACTOR, int> IP6EntitiesUnitOfWork.FACTOR
        {
            get { return GetRepository((FACTOR x) => x.fact_id); }
        }

        IRepository<FACTVAL, int> IP6EntitiesUnitOfWork.FACTVAL
        {
            get { return GetRepository((FACTVAL x) => x.fact_val_id); }
        }

        IRepository<FILTPROP, int> IP6EntitiesUnitOfWork.FILTPROP
        {
            get { return GetRepository((FILTPROP x) => x.filter_id); }
        }

        IRepository<FINDATES, int> IP6EntitiesUnitOfWork.FINDATES
        {
            get { return GetRepository((FINDATES x) => x.fin_dates_id); }
        }

        IRepository<FORMCATG, int> IP6EntitiesUnitOfWork.FORMCATG
        {
            get { return GetRepository((FORMCATG x) => x.form_catg_id); }
        }

        IRepository<FORMPROJ, Tuple<int, int>> IP6EntitiesUnitOfWork.FORMPROJ
        {
            get { return GetRepository((FORMPROJ x) => Tuple.Create(x.form_tmpl_id, x.proj_id)); }
        }

        IRepository<FORMTMPL, int> IP6EntitiesUnitOfWork.FORMTMPL
        {
            get { return GetRepository((FORMTMPL x) => x.form_tmpl_id); }
        }

        IRepository<FUNDSRC, int> IP6EntitiesUnitOfWork.FUNDSRC
        {
            get { return GetRepository((FUNDSRC x) => x.fund_id); }
        }

        IRepository<GCHANGE, int> IP6EntitiesUnitOfWork.GCHANGE
        {
            get { return GetRepository((GCHANGE x) => x.gchange_id); }
        }

        IReadOnlyRepository<HQDATA> IP6EntitiesUnitOfWork.HQDATA
        {
            get { return GetReadOnlyRepository<HQDATA>(); }
        }

        IRepository<HQUERY, Tuple<int, int, int>> IP6EntitiesUnitOfWork.HQUERY
        {
            get { return GetRepository((HQUERY x) => Tuple.Create(x.session_id, x.context_id, x.fk_id)); }
        }

        IRepository<ISSUHIST, int> IP6EntitiesUnitOfWork.ISSUHIST
        {
            get { return GetRepository((ISSUHIST x) => x.issue_id); }
        }

        IRepository<ITERATION, int> IP6EntitiesUnitOfWork.ITERATION
        {
            get { return GetRepository((ITERATION x) => x.iteration_id); }
        }

        IRepository<ITERDAYS, Tuple<int, int>> IP6EntitiesUnitOfWork.ITERDAYS
        {
            get { return GetRepository((ITERDAYS x) => Tuple.Create(x.task_id, x.day_number)); }
        }

        IRepository<ITERGOAL, int> IP6EntitiesUnitOfWork.ITERGOAL
        {
            get { return GetRepository((ITERGOAL x) => x.iter_goal_id); }
        }

        IRepository<JOBLOG, int> IP6EntitiesUnitOfWork.JOBLOG
        {
            get { return GetRepository((JOBLOG x) => x.job_id); }
        }

        IRepository<JOBRPT, Tuple<int, int>> IP6EntitiesUnitOfWork.JOBRPT
        {
            get { return GetRepository((JOBRPT x) => Tuple.Create(x.job_id, x.rpt_id)); }
        }

        IRepository<JOBSVC, int> IP6EntitiesUnitOfWork.JOBSVC
        {
            get { return GetRepository((JOBSVC x) => x.job_id); }
        }

        IRepository<LOCATION, int> IP6EntitiesUnitOfWork.LOCATION
        {
            get { return GetRepository((LOCATION x) => x.location_id); }
        }

        IRepository<MEMOTYPE, int> IP6EntitiesUnitOfWork.MEMOTYPE
        {
            get { return GetRepository((MEMOTYPE x) => x.memo_type_id); }
        }

        IRepository<NEXTKEY, string> IP6EntitiesUnitOfWork.NEXTKEY
        {
            get { return GetRepository((NEXTKEY x) => x.key_name); }
        }

        IRepository<NONWORK, int> IP6EntitiesUnitOfWork.NONWORK
        {
            get { return GetRepository((NONWORK x) => x.nonwork_type_id); }
        }

        IRepository<NOTE, int> IP6EntitiesUnitOfWork.NOTE
        {
            get { return GetRepository((NOTE x) => x.note_id); }
        }

        IRepository<OBS, int> IP6EntitiesUnitOfWork.OBS
        {
            get { return GetRepository((OBS x) => x.obs_id); }
        }

        IReadOnlyRepository<OBSPROJ> IP6EntitiesUnitOfWork.OBSPROJ
        {
            get { return GetReadOnlyRepository<OBSPROJ>(); }
        }

        IRepository<PCATTYPE, int> IP6EntitiesUnitOfWork.PCATTYPE
        {
            get { return GetRepository((PCATTYPE x) => x.proj_catg_type_id); }
        }

        IRepository<PCATUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.PCATUSER
        {
            get { return GetRepository((PCATUSER x) => Tuple.Create(x.proj_catg_id, x.user_id)); }
        }

        IRepository<PCATVAL, int> IP6EntitiesUnitOfWork.PCATVAL
        {
            get { return GetRepository((PCATVAL x) => x.proj_catg_id); }
        }

        IRepository<PFOLIO, int> IP6EntitiesUnitOfWork.PFOLIO
        {
            get { return GetRepository((PFOLIO x) => x.pfolio_id); }
        }

        IRepository<PHASE, int> IP6EntitiesUnitOfWork.PHASE
        {
            get { return GetRepository((PHASE x) => x.phase_id); }
        }

        IRepository<POBS, int> IP6EntitiesUnitOfWork.POBS
        {
            get { return GetRepository((POBS x) => x.pobs_id); }
        }

        IRepository<PREFER, int> IP6EntitiesUnitOfWork.PREFER
        {
            get { return GetRepository((PREFER x) => x.prefer_id); }
        }

        IRepository<PRMQUEUE, Tuple<string, string, DateTime>> IP6EntitiesUnitOfWork.PRMQUEUE
        {
            get { return GetRepository((PRMQUEUE x) => Tuple.Create(x.queue_name, x.msg_key, x.enqueue_date)); }
        }

        IRepository<PROCGROUP, int> IP6EntitiesUnitOfWork.PROCGROUP
        {
            get { return GetRepository((PROCGROUP x) => x.proc_group_id); }
        }

        IRepository<PROCITEM, int> IP6EntitiesUnitOfWork.PROCITEM
        {
            get { return GetRepository((PROCITEM x) => x.proc_item_id); }
        }

        IRepository<PROFILE, int> IP6EntitiesUnitOfWork.PROFILE
        {
            get { return GetRepository((PROFILE x) => x.prof_id); }
        }

        IRepository<PROFPRIV, Tuple<int, int>> IP6EntitiesUnitOfWork.PROFPRIV
        {
            get { return GetRepository((PROFPRIV x) => Tuple.Create(x.prof_id, x.priv_num)); }
        }

        IRepository<PROJCOST, int> IP6EntitiesUnitOfWork.PROJCOST
        {
            get { return GetRepository((PROJCOST x) => x.cost_item_id); }
        }

        IRepository<PROJECT, int> IP6EntitiesUnitOfWork.PROJECT
        {
            get { return GetRepository((PROJECT x) => x.proj_id); }
        }

        IRepository<PROJEST, int> IP6EntitiesUnitOfWork.PROJEST
        {
            get { return GetRepository((PROJEST x) => x.proj_est_id); }
        }

        IRepository<PROJFUND, int> IP6EntitiesUnitOfWork.PROJFUND
        {
            get { return GetRepository((PROJFUND x) => x.proj_fund_id); }
        }

        IRepository<PROJISSU, int> IP6EntitiesUnitOfWork.PROJISSU
        {
            get { return GetRepository((PROJISSU x) => x.issue_id); }
        }

        IRepository<PROJPCAT, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJPCAT
        {
            get { return GetRepository((PROJPCAT x) => Tuple.Create(x.proj_id, x.proj_catg_type_id)); }
        }

        IRepository<PROJPROP, Tuple<int, string>> IP6EntitiesUnitOfWork.PROJPROP
        {
            get { return GetRepository((PROJPROP x) => Tuple.Create(x.proj_id, x.prop_name)); }
        }

        IRepository<PROJRISK, int> IP6EntitiesUnitOfWork.PROJRISK
        {
            get { return GetRepository((PROJRISK x) => x.risk_id); }
        }

        IRepository<PROJSHAR, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJSHAR
        {
            get { return GetRepository((PROJSHAR x) => Tuple.Create(x.proj_id, x.session_id)); }
        }

        IRepository<PROJTHRS, int> IP6EntitiesUnitOfWork.PROJTHRS
        {
            get { return GetRepository((PROJTHRS x) => x.thresh_id); }
        }

        IRepository<PROJWBS, int> IP6EntitiesUnitOfWork.PROJWBS
        {
            get { return GetRepository((PROJWBS x) => x.wbs_id); }
        }

        IRepository<PROJWSRPT, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJWSRPT
        {
            get { return GetRepository((PROJWSRPT x) => Tuple.Create(x.rpt_id, x.proj_id)); }
        }

        IRepository<PRPFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.PRPFOLIO
        {
            get { return GetRepository((PRPFOLIO x) => Tuple.Create(x.pfolio_id, x.wbs_id)); }
        }

        IRepository<PUBUSER, int> IP6EntitiesUnitOfWork.PUBUSER
        {
            get { return GetRepository((PUBUSER x) => x.public_group_id); }
        }

        IRepository<QUERYLIB, int> IP6EntitiesUnitOfWork.QUERYLIB
        {
            get { return GetRepository((QUERYLIB x) => x.query_lib_id); }
        }

        IRepository<RCATTYPE, int> IP6EntitiesUnitOfWork.RCATTYPE
        {
            get { return GetRepository((RCATTYPE x) => x.rsrc_catg_type_id); }
        }

        IRepository<RCATVAL, int> IP6EntitiesUnitOfWork.RCATVAL
        {
            get { return GetRepository((RCATVAL x) => x.rsrc_catg_id); }
        }

        IRepository<REFRDEL, Tuple<DateTime, string, string>> IP6EntitiesUnitOfWork.REFRDEL
        {
            get { return GetRepository((REFRDEL x) => Tuple.Create(x.delete_date, x.table_name, x.pk1)); }
        }

        IRepository<REITTYPE, int> IP6EntitiesUnitOfWork.REITTYPE
        {
            get { return GetRepository((REITTYPE x) => x.related_item_type_id); }
        }

        IRepository<RELEASE, int> IP6EntitiesUnitOfWork.RELEASE
        {
            get { return GetRepository((RELEASE x) => x.release_id); }
        }

        IRepository<RELITEMS, Tuple<int, int, int, int>> IP6EntitiesUnitOfWork.RELITEMS
        {
            get
            {
                return
                    GetRepository(
                        (RELITEMS x) => Tuple.Create(x.item_type_id, x.fk_id, x.related_item_type_id, x.related_fk_id));
            }
        }

        IRepository<RFOLIO, int> IP6EntitiesUnitOfWork.RFOLIO
        {
            get { return GetRepository((RFOLIO x) => x.rfolio_id); }
        }

        IRepository<RISKCTRL, int> IP6EntitiesUnitOfWork.RISKCTRL
        {
            get { return GetRepository((RISKCTRL x) => x.risk_id); }
        }

        IRepository<RISKTYPE, int> IP6EntitiesUnitOfWork.RISKTYPE
        {
            get { return GetRepository((RISKTYPE x) => x.risk_type_id); }
        }

        IRepository<RLFOLIO, int> IP6EntitiesUnitOfWork.RLFOLIO
        {
            get { return GetRepository((RLFOLIO x) => x.rlfolio_id); }
        }

        IRepository<ROLELIMIT, int> IP6EntitiesUnitOfWork.ROLELIMIT
        {
            get { return GetRepository((ROLELIMIT x) => x.rolelimit_id); }
        }

        IRepository<ROLERATE, int> IP6EntitiesUnitOfWork.ROLERATE
        {
            get { return GetRepository((ROLERATE x) => x.role_rate_id); }
        }

        IRepository<ROLES, int> IP6EntitiesUnitOfWork.ROLES
        {
            get { return GetRepository((ROLES x) => x.role_id); }
        }

        IRepository<ROLFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.ROLFOLIO
        {
            get { return GetRepository((ROLFOLIO x) => Tuple.Create(x.rlfolio_id, x.role_id)); }
        }

        IRepository<RPT, int> IP6EntitiesUnitOfWork.RPT
        {
            get { return GetRepository((RPT x) => x.rpt_id); }
        }

        IRepository<RPTBATCH, int> IP6EntitiesUnitOfWork.RPTBATCH
        {
            get { return GetRepository((RPTBATCH x) => x.rpt_batch_id); }
        }

        IRepository<RPTGROUP, int> IP6EntitiesUnitOfWork.RPTGROUP
        {
            get { return GetRepository((RPTGROUP x) => x.rpt_group_id); }
        }

        IRepository<RPTLIST, Tuple<int, int>> IP6EntitiesUnitOfWork.RPTLIST
        {
            get { return GetRepository((RPTLIST x) => Tuple.Create(x.rpt_batch_id, x.rpt_id)); }
        }

        IRepository<RSRC, int> IP6EntitiesUnitOfWork.RSRC
        {
            get { return GetRepository((RSRC x) => x.rsrc_id); }
        }

        IRepository<RSRCANDASH, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCANDASH
        {
            get { return GetRepository((RSRCANDASH x) => Tuple.Create(x.dashboard_id, x.rsrcan_view_id)); }
        }

        IRepository<RSRCANVIEW, int> IP6EntitiesUnitOfWork.RSRCANVIEW
        {
            get { return GetRepository((RSRCANVIEW x) => x.rsrcan_view_id); }
        }

        IRepository<RSRCCURV, int> IP6EntitiesUnitOfWork.RSRCCURV
        {
            get { return GetRepository((RSRCCURV x) => x.curv_id); }
        }

        IRepository<RSRCHOUR, int> IP6EntitiesUnitOfWork.RSRCHOUR
        {
            get { return GetRepository((RSRCHOUR x) => x.rsrc_hr_id); }
        }

        IRepository<RSRCPROP, Tuple<int, string>> IP6EntitiesUnitOfWork.RSRCPROP
        {
            get { return GetRepository((RSRCPROP x) => Tuple.Create(x.rsrc_id, x.prop_name)); }
        }

        IRepository<RSRCRATE, int> IP6EntitiesUnitOfWork.RSRCRATE
        {
            get { return GetRepository((RSRCRATE x) => x.rsrc_rate_id); }
        }

        IRepository<RSRCRCAT, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCRCAT
        {
            get { return GetRepository((RSRCRCAT x) => Tuple.Create(x.rsrc_id, x.rsrc_catg_type_id)); }
        }

        IRepository<RSRCROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCROLE
        {
            get { return GetRepository((RSRCROLE x) => Tuple.Create(x.rsrc_id, x.role_id)); }
        }

        IRepository<RSRCSEC, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCSEC
        {
            get { return GetRepository((RSRCSEC x) => Tuple.Create(x.user_id, x.rsrc_id)); }
        }

        IRepository<RSRFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRFOLIO
        {
            get { return GetRepository((RSRFOLIO x) => Tuple.Create(x.rfolio_id, x.rsrc_id)); }
        }

        IRepository<SCENARIO, int> IP6EntitiesUnitOfWork.SCENARIO
        {
            get { return GetRepository((SCENARIO x) => x.scenario_id); }
        }

        IRepository<SCENPROJ, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENPROJ
        {
            get { return GetRepository((SCENPROJ x) => Tuple.Create(x.scenario_id, x.proj_id)); }
        }

        IRepository<SCENROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENROLE
        {
            get { return GetRepository((SCENROLE x) => Tuple.Create(x.scenario_id, x.role_id)); }
        }

        IRepository<SCENUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENUSER
        {
            get { return GetRepository((SCENUSER x) => Tuple.Create(x.scenario_id, x.user_id)); }
        }

        IRepository<SETTINGS, Tuple<string, string>> IP6EntitiesUnitOfWork.SETTINGS
        {
            get { return GetRepository((SETTINGS x) => Tuple.Create(x._namespace, x.setting_name)); }
        }

        IRepository<SHIFT, int> IP6EntitiesUnitOfWork.SHIFT
        {
            get { return GetRepository((SHIFT x) => x.shift_id); }
        }

        IRepository<SHIFTPER, int> IP6EntitiesUnitOfWork.SHIFTPER
        {
            get { return GetRepository((SHIFTPER x) => x.shift_period_id); }
        }

        IRepository<SPIDMAP, int> IP6EntitiesUnitOfWork.SPIDMAP
        {
            get { return GetRepository((SPIDMAP x) => x.spid); }
        }

        IRepository<SUMPROJCOST, Tuple<int, int, int>> IP6EntitiesUnitOfWork.SUMPROJCOST
        {
            get { return GetRepository((SUMPROJCOST x) => Tuple.Create(x.proj_id, x.wbs_id, x.cost_type_id)); }
        }

        IRepository<SUMTASK, Tuple<int, int>> IP6EntitiesUnitOfWork.SUMTASK
        {
            get { return GetRepository((SUMTASK x) => Tuple.Create(x.proj_id, x.wbs_id)); }
        }

        IRepository<SUMTASKSPREAD, Tuple<int, int, DateTime, DateTime, string>> IP6EntitiesUnitOfWork.SUMTASKSPREAD
        {
            get
            {
                return
                    GetRepository(
                        (SUMTASKSPREAD x) => Tuple.Create(x.proj_id, x.wbs_id, x.start_date, x.end_date, x.spread_type));
            }
        }

        IRepository<SUMTRSRC, int> IP6EntitiesUnitOfWork.SUMTRSRC
        {
            get { return GetRepository((SUMTRSRC x) => x.sumtrsrc_id); }
        }

        IRepository<TASK, int> IP6EntitiesUnitOfWork.TASK
        {
            get { return GetRepository((TASK x) => x.task_id); }
        }

        IRepository<TASKACTV, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKACTV
        {
            get { return GetRepository((TASKACTV x) => Tuple.Create(x.task_id, x.actv_code_type_id)); }
        }

        IRepository<TASKDOC, int> IP6EntitiesUnitOfWork.TASKDOC
        {
            get { return GetRepository((TASKDOC x) => x.taskdoc_id); }
        }

        IRepository<TASKFDBK, int> IP6EntitiesUnitOfWork.TASKFDBK
        {
            get { return GetRepository((TASKFDBK x) => x.task_id); }
        }

        IRepository<TASKFIN, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKFIN
        {
            get { return GetRepository((TASKFIN x) => Tuple.Create(x.fin_dates_id, x.task_id)); }
        }

        IRepository<TASKMEMO, int> IP6EntitiesUnitOfWork.TASKMEMO
        {
            get { return GetRepository((TASKMEMO x) => x.memo_id); }
        }

        IRepository<TASKNOTE, int> IP6EntitiesUnitOfWork.TASKNOTE
        {
            get { return GetRepository((TASKNOTE x) => x.task_id); }
        }

        IRepository<TASKPRED, int> IP6EntitiesUnitOfWork.TASKPRED
        {
            get { return GetRepository((TASKPRED x) => x.task_pred_id); }
        }

        IRepository<TASKPROC, int> IP6EntitiesUnitOfWork.TASKPROC
        {
            get { return GetRepository((TASKPROC x) => x.proc_id); }
        }

        IRepository<TASKRISK, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKRISK
        {
            get { return GetRepository((TASKRISK x) => Tuple.Create(x.task_id, x.risk_id)); }
        }

        IRepository<TASKRSRC, int> IP6EntitiesUnitOfWork.TASKRSRC
        {
            get { return GetRepository((TASKRSRC x) => x.taskrsrc_id); }
        }

        IRepository<TASKSUM, int> IP6EntitiesUnitOfWork.TASKSUM
        {
            get { return GetRepository((TASKSUM x) => x.task_sum_id); }
        }

        IRepository<TASKSUMFIN, int> IP6EntitiesUnitOfWork.TASKSUMFIN
        {
            get { return GetRepository((TASKSUMFIN x) => x.task_sum_fin_id); }
        }

        IRepository<TASKUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKUSER
        {
            get { return GetRepository((TASKUSER x) => Tuple.Create(x.task_id, x.user_id)); }
        }

        IRepository<TASKWKSP, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKWKSP
        {
            get { return GetRepository((TASKWKSP x) => Tuple.Create(x.workspace_id, x.task_id)); }
        }

        IRepository<THRSPARM, int> IP6EntitiesUnitOfWork.THRSPARM
        {
            get { return GetRepository((THRSPARM x) => x.thresh_parm_id); }
        }

        IRepository<TIMESHT, Tuple<int, int>> IP6EntitiesUnitOfWork.TIMESHT
        {
            get { return GetRepository((TIMESHT x) => Tuple.Create(x.ts_id, x.rsrc_id)); }
        }

        IRepository<TMPLCATG, int> IP6EntitiesUnitOfWork.TMPLCATG
        {
            get { return GetRepository((TMPLCATG x) => x.tmplcatg_catg_id); }
        }

        IReadOnlyRepository<TPROJMAP> IP6EntitiesUnitOfWork.TPROJMAP
        {
            get { return GetReadOnlyRepository<TPROJMAP>(); }
        }

        IRepository<TRAKVIEW, int> IP6EntitiesUnitOfWork.TRAKVIEW
        {
            get { return GetRepository((TRAKVIEW x) => x.track_view_id); }
        }

        IRepository<TRSRCFIN, Tuple<int, int>> IP6EntitiesUnitOfWork.TRSRCFIN
        {
            get { return GetRepository((TRSRCFIN x) => Tuple.Create(x.fin_dates_id, x.taskrsrc_id)); }
        }

        IRepository<TRSRCSUM, int> IP6EntitiesUnitOfWork.TRSRCSUM
        {
            get { return GetRepository((TRSRCSUM x) => x.taskrsrc_sum_id); }
        }

        IRepository<TRSRCSUMFN, int> IP6EntitiesUnitOfWork.TRSRCSUMFN
        {
            get { return GetRepository((TRSRCSUMFN x) => x.trsrc_sum_fin_id); }
        }

        IRepository<TSAUDIT, int> IP6EntitiesUnitOfWork.TSAUDIT
        {
            get { return GetRepository((TSAUDIT x) => x.ts_audit_id); }
        }

        IRepository<TSDATES, int> IP6EntitiesUnitOfWork.TSDATES
        {
            get { return GetRepository((TSDATES x) => x.ts_id); }
        }

        IRepository<TSDELEGATE, int> IP6EntitiesUnitOfWork.TSDELEGATE
        {
            get { return GetRepository((TSDELEGATE x) => x.ts_delegate_id); }
        }

        IRepository<UACCESS, Tuple<int, int>> IP6EntitiesUnitOfWork.UACCESS
        {
            get { return GetRepository((UACCESS x) => Tuple.Create(x.user_id, x.proj_id)); }
        }

        IRepository<UDFCODE, int> IP6EntitiesUnitOfWork.UDFCODE
        {
            get { return GetRepository((UDFCODE x) => x.udf_code_id); }
        }

        IRepository<UDFTYPE, int> IP6EntitiesUnitOfWork.UDFTYPE
        {
            get { return GetRepository((UDFTYPE x) => x.udf_type_id); }
        }

        IRepository<UDFVALUE, Tuple<int, int>> IP6EntitiesUnitOfWork.UDFVALUE
        {
            get { return GetRepository((UDFVALUE x) => Tuple.Create(x.udf_type_id, x.fk_id)); }
        }

        IRepository<UEVNTREG, Tuple<int, string, string, string>> IP6EntitiesUnitOfWork.UEVNTREG
        {
            get
            {
                return
                    GetRepository((UEVNTREG x) => Tuple.Create(x.user_id, x.app_name, x.operation_name, x.action_name));
            }
        }

        IRepository<UMEASURE, int> IP6EntitiesUnitOfWork.UMEASURE
        {
            get { return GetRepository((UMEASURE x) => x.unit_id); }
        }

        IRepository<UPKLIST, Tuple<int, int>> IP6EntitiesUnitOfWork.UPKLIST
        {
            get { return GetRepository((UPKLIST x) => Tuple.Create(x.session_id, x.pk_id)); }
        }

        IRepository<USERCOL, int> IP6EntitiesUnitOfWork.USERCOL
        {
            get { return GetRepository((USERCOL x) => x.user_col_id); }
        }

        IRepository<USERDATA, int> IP6EntitiesUnitOfWork.USERDATA
        {
            get { return GetRepository((USERDATA x) => x.user_data_id); }
        }

        IRepository<USERENG, int> IP6EntitiesUnitOfWork.USERENG
        {
            get { return GetRepository((USERENG x) => x.user_eng_id); }
        }

        IRepository<USEROBS, Tuple<int, int>> IP6EntitiesUnitOfWork.USEROBS
        {
            get { return GetRepository((USEROBS x) => Tuple.Create(x.user_id, x.obs_id)); }
        }

        IRepository<USEROPEN, int> IP6EntitiesUnitOfWork.USEROPEN
        {
            get { return GetRepository((USEROPEN x) => x.user_open_id); }
        }

        IRepository<USERS, int> IP6EntitiesUnitOfWork.USERS
        {
            get { return GetRepository((USERS x) => x.user_id); }
        }

        IRepository<USERSET, Tuple<string, string, int>> IP6EntitiesUnitOfWork.USERSET
        {
            get { return GetRepository((USERSET x) => Tuple.Create(x._namespace, x.setting_name, x.user_id)); }
        }

        IRepository<USERWKSP, Tuple<int, int>> IP6EntitiesUnitOfWork.USERWKSP
        {
            get { return GetRepository((USERWKSP x) => Tuple.Create(x.user_id, x.workspace_id)); }
        }

        IRepository<USESSION, int> IP6EntitiesUnitOfWork.USESSION
        {
            get { return GetRepository((USESSION x) => x.session_id); }
        }

        IRepository<USROPNVAL, Tuple<int, int, string>> IP6EntitiesUnitOfWork.USROPNVAL
        {
            get { return GetRepository((USROPNVAL x) => Tuple.Create(x.user_open_id, x.pk_id, x.usropn_type)); }
        }

        IRepository<VIEWPREF, int> IP6EntitiesUnitOfWork.VIEWPREF
        {
            get { return GetRepository((VIEWPREF x) => x.view_pref_id); }
        }

        IRepository<VIEWPROP, int> IP6EntitiesUnitOfWork.VIEWPROP
        {
            get { return GetRepository((VIEWPROP x) => x.view_id); }
        }

        IRepository<VWPREFDASH, Tuple<int, int>> IP6EntitiesUnitOfWork.VWPREFDASH
        {
            get { return GetRepository((VWPREFDASH x) => Tuple.Create(x.dashboard_id, x.view_pref_id)); }
        }

        IRepository<VWPREFDATA, Tuple<int, string>> IP6EntitiesUnitOfWork.VWPREFDATA
        {
            get { return GetRepository((VWPREFDATA x) => Tuple.Create(x.view_pref_id, x.view_pref_key)); }
        }

        IRepository<VWPREFUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.VWPREFUSER
        {
            get { return GetRepository((VWPREFUSER x) => Tuple.Create(x.view_pref_id, x.user_id)); }
        }

        IRepository<WBRSCAT, int> IP6EntitiesUnitOfWork.WBRSCAT
        {
            get { return GetRepository((WBRSCAT x) => x.wbrs_cat_id); }
        }

        IRepository<WBSBUDG, int> IP6EntitiesUnitOfWork.WBSBUDG
        {
            get { return GetRepository((WBSBUDG x) => x.wbs_budg_id); }
        }

        IRepository<WBSMEMO, int> IP6EntitiesUnitOfWork.WBSMEMO
        {
            get { return GetRepository((WBSMEMO x) => x.wbs_memo_id); }
        }

        IRepository<WBSRSRC, int> IP6EntitiesUnitOfWork.WBSRSRC
        {
            get { return GetRepository((WBSRSRC x) => x.wbsrsrc_id); }
        }

        IRepository<WBSRSRC_QTY, Tuple<int, DateTime, DateTime>> IP6EntitiesUnitOfWork.WBSRSRC_QTY
        {
            get { return GetRepository((WBSRSRC_QTY x) => Tuple.Create(x.wbsrsrc_id, x.week_start, x.month_start)); }
        }

        IRepository<WBSSTEP, int> IP6EntitiesUnitOfWork.WBSSTEP
        {
            get { return GetRepository((WBSSTEP x) => x.wbs_step_id); }
        }

        IRepository<WKFLTMPL, int> IP6EntitiesUnitOfWork.WKFLTMPL
        {
            get { return GetRepository((WKFLTMPL x) => x.wkfl_tmpl_id); }
        }

        IRepository<WKFLUSER, Tuple<int, int, int>> IP6EntitiesUnitOfWork.WKFLUSER
        {
            get { return GetRepository((WKFLUSER x) => Tuple.Create(x.work_flow_id, x.user_id, x.stage_num)); }
        }

        IRepository<WORKFLOW, int> IP6EntitiesUnitOfWork.WORKFLOW
        {
            get { return GetRepository((WORKFLOW x) => x.work_flow_id); }
        }

        IRepository<WORKSPACE, int> IP6EntitiesUnitOfWork.WORKSPACE
        {
            get { return GetRepository((WORKSPACE x) => x.workspace_id); }
        }
    }
}