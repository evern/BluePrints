using BaseModel.DataModel;
using BaseModel.DataModel.EntityFramework;
using BluePrints.P6Data;
using System;

namespace BluePrints.P6EntitiesDataModel
{
    /// <summary>
    /// A P6EntitiesUnitOfWork instance that represents the run-time implementation of the IP6EntitiesUnitOfWork interface.
    /// </summary>
    public class P6EntitiesUnitOfWork : DbUnitOfWork<P6Entities>, IP6EntitiesUnitOfWork
    {
        public P6EntitiesUnitOfWork(Func<P6Entities> contextFactory)
            : base(contextFactory)
        {
        }

        IRepository<ACCOUNT, int> IP6EntitiesUnitOfWork.ACCOUNT
        {
            get { return GetRepository(x => x.Set<ACCOUNT>(), (ACCOUNT x) => x.acct_id); }
        }

        IRepository<ACTVCODE, int> IP6EntitiesUnitOfWork.ACTVCODE
        {
            get { return GetRepository(x => x.Set<ACTVCODE>(), (ACTVCODE x) => x.actv_code_id); }
        }

        IRepository<ACTVTYPE, int> IP6EntitiesUnitOfWork.ACTVTYPE
        {
            get { return GetRepository(x => x.Set<ACTVTYPE>(), (ACTVTYPE x) => x.actv_code_type_id); }
        }

        IRepository<ADMIN_CONFIG, string> IP6EntitiesUnitOfWork.ADMIN_CONFIG
        {
            get { return GetRepository(x => x.Set<ADMIN_CONFIG>(), (ADMIN_CONFIG x) => x.config_name); }
        }

        IRepository<BASETYPE, int> IP6EntitiesUnitOfWork.BASETYPE
        {
            get { return GetRepository(x => x.Set<BASETYPE>(), (BASETYPE x) => x.base_type_id); }
        }

        IRepository<BRE_REGISTRY, string> IP6EntitiesUnitOfWork.BRE_REGISTRY
        {
            get { return GetRepository(x => x.Set<BRE_REGISTRY>(), (BRE_REGISTRY x) => x.bre_registry_id); }
        }

        IRepository<BUDGCHNG, int> IP6EntitiesUnitOfWork.BUDGCHNG
        {
            get { return GetRepository(x => x.Set<BUDGCHNG>(), (BUDGCHNG x) => x.budg_chng_id); }
        }

        IRepository<CALENDAR, int> IP6EntitiesUnitOfWork.CALENDAR
        {
            get { return GetRepository(x => x.Set<CALENDAR>(), (CALENDAR x) => x.clndr_id); }
        }

        IRepository<COSTTYPE, int> IP6EntitiesUnitOfWork.COSTTYPE
        {
            get { return GetRepository(x => x.Set<COSTTYPE>(), (COSTTYPE x) => x.cost_type_id); }
        }

        IRepository<CURRTYPE, int> IP6EntitiesUnitOfWork.CURRTYPE
        {
            get { return GetRepository(x => x.Set<CURRTYPE>(), (CURRTYPE x) => x.curr_id); }
        }

        IRepository<DASHBOARD, int> IP6EntitiesUnitOfWork.DASHBOARD
        {
            get { return GetRepository(x => x.Set<DASHBOARD>(), (DASHBOARD x) => x.dashboard_id); }
        }

        IRepository<DASHUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.DASHUSER
        {
            get
            {
                return GetRepository(x => x.Set<DASHUSER>(), (DASHUSER x) => Tuple.Create(x.dashboard_id, x.user_id));
            }
        }

        IRepository<DISCUSSION, int> IP6EntitiesUnitOfWork.DISCUSSION
        {
            get { return GetRepository(x => x.Set<DISCUSSION>(), (DISCUSSION x) => x.discussion_id); }
        }

        IRepository<DISCUSSION_READ, Tuple<int, int>> IP6EntitiesUnitOfWork.DISCUSSION_READ
        {
            get
            {
                return GetRepository(x => x.Set<DISCUSSION_READ>(),
                    (DISCUSSION_READ x) => Tuple.Create(x.discussion_id, x.user_id));
            }
        }

        IRepository<DLTACCT, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTACCT
        {
            get { return GetRepository(x => x.Set<DLTACCT>(), (DLTACCT x) => Tuple.Create(x.session_id, x.acct_id)); }
        }

        IRepository<DLTACTV, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTACTV
        {
            get
            {
                return GetRepository(x => x.Set<DLTACTV>(), (DLTACTV x) => Tuple.Create(x.session_id, x.actv_code_id));
            }
        }

        IRepository<DLTOBS, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTOBS
        {
            get { return GetRepository(x => x.Set<DLTOBS>(), (DLTOBS x) => Tuple.Create(x.session_id, x.obs_id)); }
        }

        IRepository<DLTROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTROLE
        {
            get { return GetRepository(x => x.Set<DLTROLE>(), (DLTROLE x) => Tuple.Create(x.session_id, x.role_id)); }
        }

        IRepository<DLTRSRC, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTRSRC
        {
            get { return GetRepository(x => x.Set<DLTRSRC>(), (DLTRSRC x) => Tuple.Create(x.session_id, x.rsrc_id)); }
        }

        IReadOnlyRepository<DLTRSRL> IP6EntitiesUnitOfWork.DLTRSRL
        {
            get { return GetReadOnlyRepository(x => x.Set<DLTRSRL>()); }
        }

        IRepository<DLTUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.DLTUSER
        {
            get { return GetRepository(x => x.Set<DLTUSER>(), (DLTUSER x) => Tuple.Create(x.session_id, x.user_id)); }
        }

        IRepository<DOCCATG, int> IP6EntitiesUnitOfWork.DOCCATG
        {
            get { return GetRepository(x => x.Set<DOCCATG>(), (DOCCATG x) => x.doc_catg_id); }
        }

        IRepository<DOCREVIEW, int> IP6EntitiesUnitOfWork.DOCREVIEW
        {
            get { return GetRepository(x => x.Set<DOCREVIEW>(), (DOCREVIEW x) => x.doc_review_id); }
        }

        IRepository<DOCREVIEWTASK, int> IP6EntitiesUnitOfWork.DOCREVIEWTASK
        {
            get { return GetRepository(x => x.Set<DOCREVIEWTASK>(), (DOCREVIEWTASK x) => x.review_task_id); }
        }

        IRepository<DOCSTAT, int> IP6EntitiesUnitOfWork.DOCSTAT
        {
            get { return GetRepository(x => x.Set<DOCSTAT>(), (DOCSTAT x) => x.doc_status_id); }
        }

        IRepository<DOCUMENT, int> IP6EntitiesUnitOfWork.DOCUMENT
        {
            get { return GetRepository(x => x.Set<DOCUMENT>(), (DOCUMENT x) => x.doc_id); }
        }

        IRepository<EXPPROJ, int> IP6EntitiesUnitOfWork.EXPPROJ
        {
            get { return GetRepository(x => x.Set<EXPPROJ>(), (EXPPROJ x) => x.proj_id); }
        }

        IRepository<EXTAPP, int> IP6EntitiesUnitOfWork.EXTAPP
        {
            get { return GetRepository(x => x.Set<EXTAPP>(), (EXTAPP x) => x.app_id); }
        }

        IRepository<FACTOR, int> IP6EntitiesUnitOfWork.FACTOR
        {
            get { return GetRepository(x => x.Set<FACTOR>(), (FACTOR x) => x.fact_id); }
        }

        IRepository<FACTVAL, int> IP6EntitiesUnitOfWork.FACTVAL
        {
            get { return GetRepository(x => x.Set<FACTVAL>(), (FACTVAL x) => x.fact_val_id); }
        }

        IRepository<FILTPROP, int> IP6EntitiesUnitOfWork.FILTPROP
        {
            get { return GetRepository(x => x.Set<FILTPROP>(), (FILTPROP x) => x.filter_id); }
        }

        IRepository<FINDATES, int> IP6EntitiesUnitOfWork.FINDATES
        {
            get { return GetRepository(x => x.Set<FINDATES>(), (FINDATES x) => x.fin_dates_id); }
        }

        IRepository<FORMCATG, int> IP6EntitiesUnitOfWork.FORMCATG
        {
            get { return GetRepository(x => x.Set<FORMCATG>(), (FORMCATG x) => x.form_catg_id); }
        }

        IRepository<FORMPROJ, Tuple<int, int>> IP6EntitiesUnitOfWork.FORMPROJ
        {
            get
            {
                return GetRepository(x => x.Set<FORMPROJ>(), (FORMPROJ x) => Tuple.Create(x.form_tmpl_id, x.proj_id));
            }
        }

        IRepository<FORMTMPL, int> IP6EntitiesUnitOfWork.FORMTMPL
        {
            get { return GetRepository(x => x.Set<FORMTMPL>(), (FORMTMPL x) => x.form_tmpl_id); }
        }

        IRepository<FUNDSRC, int> IP6EntitiesUnitOfWork.FUNDSRC
        {
            get { return GetRepository(x => x.Set<FUNDSRC>(), (FUNDSRC x) => x.fund_id); }
        }

        IRepository<GCHANGE, int> IP6EntitiesUnitOfWork.GCHANGE
        {
            get { return GetRepository(x => x.Set<GCHANGE>(), (GCHANGE x) => x.gchange_id); }
        }

        IReadOnlyRepository<HQDATA> IP6EntitiesUnitOfWork.HQDATA
        {
            get { return GetReadOnlyRepository(x => x.Set<HQDATA>()); }
        }

        IRepository<HQUERY, Tuple<int, int, int>> IP6EntitiesUnitOfWork.HQUERY
        {
            get
            {
                return GetRepository(x => x.Set<HQUERY>(),
                    (HQUERY x) => Tuple.Create(x.session_id, x.context_id, x.fk_id));
            }
        }

        IRepository<ISSUHIST, int> IP6EntitiesUnitOfWork.ISSUHIST
        {
            get { return GetRepository(x => x.Set<ISSUHIST>(), (ISSUHIST x) => x.issue_id); }
        }

        IRepository<ITERATION, int> IP6EntitiesUnitOfWork.ITERATION
        {
            get { return GetRepository(x => x.Set<ITERATION>(), (ITERATION x) => x.iteration_id); }
        }

        IRepository<ITERDAYS, Tuple<int, int>> IP6EntitiesUnitOfWork.ITERDAYS
        {
            get { return GetRepository(x => x.Set<ITERDAYS>(), (ITERDAYS x) => Tuple.Create(x.task_id, x.day_number)); }
        }

        IRepository<ITERGOAL, int> IP6EntitiesUnitOfWork.ITERGOAL
        {
            get { return GetRepository(x => x.Set<ITERGOAL>(), (ITERGOAL x) => x.iter_goal_id); }
        }

        IRepository<JOBLOG, int> IP6EntitiesUnitOfWork.JOBLOG
        {
            get { return GetRepository(x => x.Set<JOBLOG>(), (JOBLOG x) => x.job_id); }
        }

        IRepository<JOBRPT, Tuple<int, int>> IP6EntitiesUnitOfWork.JOBRPT
        {
            get { return GetRepository(x => x.Set<JOBRPT>(), (JOBRPT x) => Tuple.Create(x.job_id, x.rpt_id)); }
        }

        IRepository<JOBSVC, int> IP6EntitiesUnitOfWork.JOBSVC
        {
            get { return GetRepository(x => x.Set<JOBSVC>(), (JOBSVC x) => x.job_id); }
        }

        IRepository<LOCATION, int> IP6EntitiesUnitOfWork.LOCATION
        {
            get { return GetRepository(x => x.Set<LOCATION>(), (LOCATION x) => x.location_id); }
        }

        IRepository<MEMOTYPE, int> IP6EntitiesUnitOfWork.MEMOTYPE
        {
            get { return GetRepository(x => x.Set<MEMOTYPE>(), (MEMOTYPE x) => x.memo_type_id); }
        }

        IRepository<NEXTKEY, string> IP6EntitiesUnitOfWork.NEXTKEY
        {
            get { return GetRepository(x => x.Set<NEXTKEY>(), (NEXTKEY x) => x.key_name); }
        }

        IRepository<NONWORK, int> IP6EntitiesUnitOfWork.NONWORK
        {
            get { return GetRepository(x => x.Set<NONWORK>(), (NONWORK x) => x.nonwork_type_id); }
        }

        IRepository<NOTE, int> IP6EntitiesUnitOfWork.NOTE
        {
            get { return GetRepository(x => x.Set<NOTE>(), (NOTE x) => x.note_id); }
        }

        IRepository<OBS, int> IP6EntitiesUnitOfWork.OBS
        {
            get { return GetRepository(x => x.Set<OBS>(), (OBS x) => x.obs_id); }
        }

        IReadOnlyRepository<OBSPROJ> IP6EntitiesUnitOfWork.OBSPROJ
        {
            get { return GetReadOnlyRepository(x => x.Set<OBSPROJ>()); }
        }

        IRepository<PCATTYPE, int> IP6EntitiesUnitOfWork.PCATTYPE
        {
            get { return GetRepository(x => x.Set<PCATTYPE>(), (PCATTYPE x) => x.proj_catg_type_id); }
        }

        IRepository<PCATUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.PCATUSER
        {
            get
            {
                return GetRepository(x => x.Set<PCATUSER>(), (PCATUSER x) => Tuple.Create(x.proj_catg_id, x.user_id));
            }
        }

        IRepository<PCATVAL, int> IP6EntitiesUnitOfWork.PCATVAL
        {
            get { return GetRepository(x => x.Set<PCATVAL>(), (PCATVAL x) => x.proj_catg_id); }
        }

        IRepository<PFOLIO, int> IP6EntitiesUnitOfWork.PFOLIO
        {
            get { return GetRepository(x => x.Set<PFOLIO>(), (PFOLIO x) => x.pfolio_id); }
        }

        IRepository<PHASE, int> IP6EntitiesUnitOfWork.PHASE
        {
            get { return GetRepository(x => x.Set<PHASE>(), (PHASE x) => x.phase_id); }
        }

        IRepository<POBS, int> IP6EntitiesUnitOfWork.POBS
        {
            get { return GetRepository(x => x.Set<POBS>(), (POBS x) => x.pobs_id); }
        }

        IRepository<PREFER, int> IP6EntitiesUnitOfWork.PREFER
        {
            get { return GetRepository(x => x.Set<PREFER>(), (PREFER x) => x.prefer_id); }
        }

        IRepository<PRMQUEUE, Tuple<string, string, DateTime>> IP6EntitiesUnitOfWork.PRMQUEUE
        {
            get
            {
                return GetRepository(x => x.Set<PRMQUEUE>(),
                    (PRMQUEUE x) => Tuple.Create(x.queue_name, x.msg_key, x.enqueue_date));
            }
        }

        IRepository<PROCGROUP, int> IP6EntitiesUnitOfWork.PROCGROUP
        {
            get { return GetRepository(x => x.Set<PROCGROUP>(), (PROCGROUP x) => x.proc_group_id); }
        }

        IRepository<PROCITEM, int> IP6EntitiesUnitOfWork.PROCITEM
        {
            get { return GetRepository(x => x.Set<PROCITEM>(), (PROCITEM x) => x.proc_item_id); }
        }

        IRepository<PROFILE, int> IP6EntitiesUnitOfWork.PROFILE
        {
            get { return GetRepository(x => x.Set<PROFILE>(), (PROFILE x) => x.prof_id); }
        }

        IRepository<PROFPRIV, Tuple<int, int>> IP6EntitiesUnitOfWork.PROFPRIV
        {
            get { return GetRepository(x => x.Set<PROFPRIV>(), (PROFPRIV x) => Tuple.Create(x.prof_id, x.priv_num)); }
        }

        IRepository<PROJCOST, int> IP6EntitiesUnitOfWork.PROJCOST
        {
            get { return GetRepository(x => x.Set<PROJCOST>(), (PROJCOST x) => x.cost_item_id); }
        }

        IRepository<PROJECT, int> IP6EntitiesUnitOfWork.PROJECT
        {
            get { return GetRepository(x => x.Set<PROJECT>(), (PROJECT x) => x.proj_id); }
        }

        IRepository<PROJEST, int> IP6EntitiesUnitOfWork.PROJEST
        {
            get { return GetRepository(x => x.Set<PROJEST>(), (PROJEST x) => x.proj_est_id); }
        }

        IRepository<PROJFUND, int> IP6EntitiesUnitOfWork.PROJFUND
        {
            get { return GetRepository(x => x.Set<PROJFUND>(), (PROJFUND x) => x.proj_fund_id); }
        }

        IRepository<PROJISSU, int> IP6EntitiesUnitOfWork.PROJISSU
        {
            get { return GetRepository(x => x.Set<PROJISSU>(), (PROJISSU x) => x.issue_id); }
        }

        IRepository<PROJPCAT, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJPCAT
        {
            get
            {
                return GetRepository(x => x.Set<PROJPCAT>(),
                    (PROJPCAT x) => Tuple.Create(x.proj_id, x.proj_catg_type_id));
            }
        }

        IRepository<PROJPROP, Tuple<int, string>> IP6EntitiesUnitOfWork.PROJPROP
        {
            get { return GetRepository(x => x.Set<PROJPROP>(), (PROJPROP x) => Tuple.Create(x.proj_id, x.prop_name)); }
        }

        IRepository<PROJRISK, int> IP6EntitiesUnitOfWork.PROJRISK
        {
            get { return GetRepository(x => x.Set<PROJRISK>(), (PROJRISK x) => x.risk_id); }
        }

        IRepository<PROJSHAR, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJSHAR
        {
            get { return GetRepository(x => x.Set<PROJSHAR>(), (PROJSHAR x) => Tuple.Create(x.proj_id, x.session_id)); }
        }

        IRepository<PROJTHRS, int> IP6EntitiesUnitOfWork.PROJTHRS
        {
            get { return GetRepository(x => x.Set<PROJTHRS>(), (PROJTHRS x) => x.thresh_id); }
        }

        IRepository<PROJWBS, int> IP6EntitiesUnitOfWork.PROJWBS
        {
            get { return GetRepository(x => x.Set<PROJWBS>(), (PROJWBS x) => x.wbs_id); }
        }

        IRepository<PROJWSRPT, Tuple<int, int>> IP6EntitiesUnitOfWork.PROJWSRPT
        {
            get { return GetRepository(x => x.Set<PROJWSRPT>(), (PROJWSRPT x) => Tuple.Create(x.rpt_id, x.proj_id)); }
        }

        IRepository<PRPFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.PRPFOLIO
        {
            get { return GetRepository(x => x.Set<PRPFOLIO>(), (PRPFOLIO x) => Tuple.Create(x.pfolio_id, x.wbs_id)); }
        }

        IRepository<PUBUSER, int> IP6EntitiesUnitOfWork.PUBUSER
        {
            get { return GetRepository(x => x.Set<PUBUSER>(), (PUBUSER x) => x.public_group_id); }
        }

        IRepository<QUERYLIB, int> IP6EntitiesUnitOfWork.QUERYLIB
        {
            get { return GetRepository(x => x.Set<QUERYLIB>(), (QUERYLIB x) => x.query_lib_id); }
        }

        IRepository<RCATTYPE, int> IP6EntitiesUnitOfWork.RCATTYPE
        {
            get { return GetRepository(x => x.Set<RCATTYPE>(), (RCATTYPE x) => x.rsrc_catg_type_id); }
        }

        IRepository<RCATVAL, int> IP6EntitiesUnitOfWork.RCATVAL
        {
            get { return GetRepository(x => x.Set<RCATVAL>(), (RCATVAL x) => x.rsrc_catg_id); }
        }

        IRepository<REFRDEL, Tuple<DateTime, string, string>> IP6EntitiesUnitOfWork.REFRDEL
        {
            get
            {
                return GetRepository(x => x.Set<REFRDEL>(),
                    (REFRDEL x) => Tuple.Create(x.delete_date, x.table_name, x.pk1));
            }
        }

        IRepository<REITTYPE, int> IP6EntitiesUnitOfWork.REITTYPE
        {
            get { return GetRepository(x => x.Set<REITTYPE>(), (REITTYPE x) => x.related_item_type_id); }
        }

        IRepository<RELEASE, int> IP6EntitiesUnitOfWork.RELEASE
        {
            get { return GetRepository(x => x.Set<RELEASE>(), (RELEASE x) => x.release_id); }
        }

        IRepository<RELITEMS, Tuple<int, int, int, int>> IP6EntitiesUnitOfWork.RELITEMS
        {
            get
            {
                return GetRepository(x => x.Set<RELITEMS>(),
                    (RELITEMS x) => Tuple.Create(x.item_type_id, x.fk_id, x.related_item_type_id, x.related_fk_id));
            }
        }

        IRepository<RFOLIO, int> IP6EntitiesUnitOfWork.RFOLIO
        {
            get { return GetRepository(x => x.Set<RFOLIO>(), (RFOLIO x) => x.rfolio_id); }
        }

        IRepository<RISKCTRL, int> IP6EntitiesUnitOfWork.RISKCTRL
        {
            get { return GetRepository(x => x.Set<RISKCTRL>(), (RISKCTRL x) => x.risk_id); }
        }

        IRepository<RISKTYPE, int> IP6EntitiesUnitOfWork.RISKTYPE
        {
            get { return GetRepository(x => x.Set<RISKTYPE>(), (RISKTYPE x) => x.risk_type_id); }
        }

        IRepository<RLFOLIO, int> IP6EntitiesUnitOfWork.RLFOLIO
        {
            get { return GetRepository(x => x.Set<RLFOLIO>(), (RLFOLIO x) => x.rlfolio_id); }
        }

        IRepository<ROLELIMIT, int> IP6EntitiesUnitOfWork.ROLELIMIT
        {
            get { return GetRepository(x => x.Set<ROLELIMIT>(), (ROLELIMIT x) => x.rolelimit_id); }
        }

        IRepository<ROLERATE, int> IP6EntitiesUnitOfWork.ROLERATE
        {
            get { return GetRepository(x => x.Set<ROLERATE>(), (ROLERATE x) => x.role_rate_id); }
        }

        IRepository<ROLES, int> IP6EntitiesUnitOfWork.ROLES
        {
            get { return GetRepository(x => x.Set<ROLES>(), (ROLES x) => x.role_id); }
        }

        IRepository<ROLFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.ROLFOLIO
        {
            get { return GetRepository(x => x.Set<ROLFOLIO>(), (ROLFOLIO x) => Tuple.Create(x.rlfolio_id, x.role_id)); }
        }

        IRepository<RPT, int> IP6EntitiesUnitOfWork.RPT
        {
            get { return GetRepository(x => x.Set<RPT>(), (RPT x) => x.rpt_id); }
        }

        IRepository<RPTBATCH, int> IP6EntitiesUnitOfWork.RPTBATCH
        {
            get { return GetRepository(x => x.Set<RPTBATCH>(), (RPTBATCH x) => x.rpt_batch_id); }
        }

        IRepository<RPTGROUP, int> IP6EntitiesUnitOfWork.RPTGROUP
        {
            get { return GetRepository(x => x.Set<RPTGROUP>(), (RPTGROUP x) => x.rpt_group_id); }
        }

        IRepository<RPTLIST, Tuple<int, int>> IP6EntitiesUnitOfWork.RPTLIST
        {
            get { return GetRepository(x => x.Set<RPTLIST>(), (RPTLIST x) => Tuple.Create(x.rpt_batch_id, x.rpt_id)); }
        }

        IRepository<RSRC, int> IP6EntitiesUnitOfWork.RSRC
        {
            get { return GetRepository(x => x.Set<RSRC>(), (RSRC x) => x.rsrc_id); }
        }

        IRepository<RSRCANDASH, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCANDASH
        {
            get
            {
                return GetRepository(x => x.Set<RSRCANDASH>(),
                    (RSRCANDASH x) => Tuple.Create(x.dashboard_id, x.rsrcan_view_id));
            }
        }

        IRepository<RSRCANVIEW, int> IP6EntitiesUnitOfWork.RSRCANVIEW
        {
            get { return GetRepository(x => x.Set<RSRCANVIEW>(), (RSRCANVIEW x) => x.rsrcan_view_id); }
        }

        IRepository<RSRCCURV, int> IP6EntitiesUnitOfWork.RSRCCURV
        {
            get { return GetRepository(x => x.Set<RSRCCURV>(), (RSRCCURV x) => x.curv_id); }
        }

        IRepository<RSRCHOUR, int> IP6EntitiesUnitOfWork.RSRCHOUR
        {
            get { return GetRepository(x => x.Set<RSRCHOUR>(), (RSRCHOUR x) => x.rsrc_hr_id); }
        }

        IRepository<RSRCPROP, Tuple<int, string>> IP6EntitiesUnitOfWork.RSRCPROP
        {
            get { return GetRepository(x => x.Set<RSRCPROP>(), (RSRCPROP x) => Tuple.Create(x.rsrc_id, x.prop_name)); }
        }

        IRepository<RSRCRATE, int> IP6EntitiesUnitOfWork.RSRCRATE
        {
            get { return GetRepository(x => x.Set<RSRCRATE>(), (RSRCRATE x) => x.rsrc_rate_id); }
        }

        IRepository<RSRCRCAT, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCRCAT
        {
            get
            {
                return GetRepository(x => x.Set<RSRCRCAT>(),
                    (RSRCRCAT x) => Tuple.Create(x.rsrc_id, x.rsrc_catg_type_id));
            }
        }

        IRepository<RSRCROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCROLE
        {
            get { return GetRepository(x => x.Set<RSRCROLE>(), (RSRCROLE x) => Tuple.Create(x.rsrc_id, x.role_id)); }
        }

        IRepository<RSRCSEC, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRCSEC
        {
            get { return GetRepository(x => x.Set<RSRCSEC>(), (RSRCSEC x) => Tuple.Create(x.user_id, x.rsrc_id)); }
        }

        IRepository<RSRFOLIO, Tuple<int, int>> IP6EntitiesUnitOfWork.RSRFOLIO
        {
            get { return GetRepository(x => x.Set<RSRFOLIO>(), (RSRFOLIO x) => Tuple.Create(x.rfolio_id, x.rsrc_id)); }
        }

        IRepository<SCENARIO, int> IP6EntitiesUnitOfWork.SCENARIO
        {
            get { return GetRepository(x => x.Set<SCENARIO>(), (SCENARIO x) => x.scenario_id); }
        }

        IRepository<SCENPROJ, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENPROJ
        {
            get
            {
                return GetRepository(x => x.Set<SCENPROJ>(), (SCENPROJ x) => Tuple.Create(x.scenario_id, x.proj_id));
            }
        }

        IRepository<SCENROLE, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENROLE
        {
            get
            {
                return GetRepository(x => x.Set<SCENROLE>(), (SCENROLE x) => Tuple.Create(x.scenario_id, x.role_id));
            }
        }

        IRepository<SCENUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.SCENUSER
        {
            get
            {
                return GetRepository(x => x.Set<SCENUSER>(), (SCENUSER x) => Tuple.Create(x.scenario_id, x.user_id));
            }
        }

        IRepository<SETTINGS, Tuple<string, string>> IP6EntitiesUnitOfWork.SETTINGS
        {
            get
            {
                return GetRepository(x => x.Set<SETTINGS>(), (SETTINGS x) => Tuple.Create(x._namespace, x.setting_name));
            }
        }

        IRepository<SHIFT, int> IP6EntitiesUnitOfWork.SHIFT
        {
            get { return GetRepository(x => x.Set<SHIFT>(), (SHIFT x) => x.shift_id); }
        }

        IRepository<SHIFTPER, int> IP6EntitiesUnitOfWork.SHIFTPER
        {
            get { return GetRepository(x => x.Set<SHIFTPER>(), (SHIFTPER x) => x.shift_period_id); }
        }

        IRepository<SPIDMAP, int> IP6EntitiesUnitOfWork.SPIDMAP
        {
            get { return GetRepository(x => x.Set<SPIDMAP>(), (SPIDMAP x) => x.spid); }
        }

        IRepository<SUMPROJCOST, Tuple<int, int, int>> IP6EntitiesUnitOfWork.SUMPROJCOST
        {
            get
            {
                return GetRepository(x => x.Set<SUMPROJCOST>(),
                    (SUMPROJCOST x) => Tuple.Create(x.proj_id, x.wbs_id, x.cost_type_id));
            }
        }

        IRepository<SUMTASK, Tuple<int, int>> IP6EntitiesUnitOfWork.SUMTASK
        {
            get { return GetRepository(x => x.Set<SUMTASK>(), (SUMTASK x) => Tuple.Create(x.proj_id, x.wbs_id)); }
        }

        IRepository<SUMTASKSPREAD, Tuple<int, int, DateTime, DateTime, string>> IP6EntitiesUnitOfWork.SUMTASKSPREAD
        {
            get
            {
                return GetRepository(x => x.Set<SUMTASKSPREAD>(),
                    (SUMTASKSPREAD x) => Tuple.Create(x.proj_id, x.wbs_id, x.start_date, x.end_date, x.spread_type));
            }
        }

        IRepository<SUMTRSRC, int> IP6EntitiesUnitOfWork.SUMTRSRC
        {
            get { return GetRepository(x => x.Set<SUMTRSRC>(), (SUMTRSRC x) => x.sumtrsrc_id); }
        }

        IRepository<TASK, int> IP6EntitiesUnitOfWork.TASK
        {
            get { return GetRepository(x => x.Set<TASK>(), (TASK x) => x.task_id); }
        }

        IRepository<TASKACTV, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKACTV
        {
            get
            {
                return GetRepository(x => x.Set<TASKACTV>(),
                    (TASKACTV x) => Tuple.Create(x.task_id, x.actv_code_type_id));
            }
        }

        IRepository<TASKDOC, int> IP6EntitiesUnitOfWork.TASKDOC
        {
            get { return GetRepository(x => x.Set<TASKDOC>(), (TASKDOC x) => x.taskdoc_id); }
        }

        IRepository<TASKFDBK, int> IP6EntitiesUnitOfWork.TASKFDBK
        {
            get { return GetRepository(x => x.Set<TASKFDBK>(), (TASKFDBK x) => x.task_id); }
        }

        IRepository<TASKFIN, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKFIN
        {
            get { return GetRepository(x => x.Set<TASKFIN>(), (TASKFIN x) => Tuple.Create(x.fin_dates_id, x.task_id)); }
        }

        IRepository<TASKMEMO, int> IP6EntitiesUnitOfWork.TASKMEMO
        {
            get { return GetRepository(x => x.Set<TASKMEMO>(), (TASKMEMO x) => x.memo_id); }
        }

        IRepository<TASKNOTE, int> IP6EntitiesUnitOfWork.TASKNOTE
        {
            get { return GetRepository(x => x.Set<TASKNOTE>(), (TASKNOTE x) => x.task_id); }
        }

        IRepository<TASKPRED, int> IP6EntitiesUnitOfWork.TASKPRED
        {
            get { return GetRepository(x => x.Set<TASKPRED>(), (TASKPRED x) => x.task_pred_id); }
        }

        IRepository<TASKPROC, int> IP6EntitiesUnitOfWork.TASKPROC
        {
            get { return GetRepository(x => x.Set<TASKPROC>(), (TASKPROC x) => x.proc_id); }
        }

        IRepository<TASKRISK, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKRISK
        {
            get { return GetRepository(x => x.Set<TASKRISK>(), (TASKRISK x) => Tuple.Create(x.task_id, x.risk_id)); }
        }

        IRepository<TASKRSRC, int> IP6EntitiesUnitOfWork.TASKRSRC
        {
            get { return GetRepository(x => x.Set<TASKRSRC>(), (TASKRSRC x) => x.taskrsrc_id); }
        }

        IRepository<TASKSUM, int> IP6EntitiesUnitOfWork.TASKSUM
        {
            get { return GetRepository(x => x.Set<TASKSUM>(), (TASKSUM x) => x.task_sum_id); }
        }

        IRepository<TASKSUMFIN, int> IP6EntitiesUnitOfWork.TASKSUMFIN
        {
            get { return GetRepository(x => x.Set<TASKSUMFIN>(), (TASKSUMFIN x) => x.task_sum_fin_id); }
        }

        IRepository<TASKUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKUSER
        {
            get { return GetRepository(x => x.Set<TASKUSER>(), (TASKUSER x) => Tuple.Create(x.task_id, x.user_id)); }
        }

        IRepository<TASKWKSP, Tuple<int, int>> IP6EntitiesUnitOfWork.TASKWKSP
        {
            get
            {
                return GetRepository(x => x.Set<TASKWKSP>(), (TASKWKSP x) => Tuple.Create(x.workspace_id, x.task_id));
            }
        }

        IRepository<THRSPARM, int> IP6EntitiesUnitOfWork.THRSPARM
        {
            get { return GetRepository(x => x.Set<THRSPARM>(), (THRSPARM x) => x.thresh_parm_id); }
        }

        IRepository<TIMESHT, Tuple<int, int>> IP6EntitiesUnitOfWork.TIMESHT
        {
            get { return GetRepository(x => x.Set<TIMESHT>(), (TIMESHT x) => Tuple.Create(x.ts_id, x.rsrc_id)); }
        }

        IRepository<TMPLCATG, int> IP6EntitiesUnitOfWork.TMPLCATG
        {
            get { return GetRepository(x => x.Set<TMPLCATG>(), (TMPLCATG x) => x.tmplcatg_catg_id); }
        }

        IReadOnlyRepository<TPROJMAP> IP6EntitiesUnitOfWork.TPROJMAP
        {
            get { return GetReadOnlyRepository(x => x.Set<TPROJMAP>()); }
        }

        IRepository<TRAKVIEW, int> IP6EntitiesUnitOfWork.TRAKVIEW
        {
            get { return GetRepository(x => x.Set<TRAKVIEW>(), (TRAKVIEW x) => x.track_view_id); }
        }

        IRepository<TRSRCFIN, Tuple<int, int>> IP6EntitiesUnitOfWork.TRSRCFIN
        {
            get
            {
                return GetRepository(x => x.Set<TRSRCFIN>(), (TRSRCFIN x) => Tuple.Create(x.fin_dates_id, x.taskrsrc_id));
            }
        }

        IRepository<TRSRCSUM, int> IP6EntitiesUnitOfWork.TRSRCSUM
        {
            get { return GetRepository(x => x.Set<TRSRCSUM>(), (TRSRCSUM x) => x.taskrsrc_sum_id); }
        }

        IRepository<TRSRCSUMFN, int> IP6EntitiesUnitOfWork.TRSRCSUMFN
        {
            get { return GetRepository(x => x.Set<TRSRCSUMFN>(), (TRSRCSUMFN x) => x.trsrc_sum_fin_id); }
        }

        IRepository<TSAUDIT, int> IP6EntitiesUnitOfWork.TSAUDIT
        {
            get { return GetRepository(x => x.Set<TSAUDIT>(), (TSAUDIT x) => x.ts_audit_id); }
        }

        IRepository<TSDATES, int> IP6EntitiesUnitOfWork.TSDATES
        {
            get { return GetRepository(x => x.Set<TSDATES>(), (TSDATES x) => x.ts_id); }
        }

        IRepository<TSDELEGATE, int> IP6EntitiesUnitOfWork.TSDELEGATE
        {
            get { return GetRepository(x => x.Set<TSDELEGATE>(), (TSDELEGATE x) => x.ts_delegate_id); }
        }

        IRepository<UACCESS, Tuple<int, int>> IP6EntitiesUnitOfWork.UACCESS
        {
            get { return GetRepository(x => x.Set<UACCESS>(), (UACCESS x) => Tuple.Create(x.user_id, x.proj_id)); }
        }

        IRepository<UDFCODE, int> IP6EntitiesUnitOfWork.UDFCODE
        {
            get { return GetRepository(x => x.Set<UDFCODE>(), (UDFCODE x) => x.udf_code_id); }
        }

        IRepository<UDFTYPE, int> IP6EntitiesUnitOfWork.UDFTYPE
        {
            get { return GetRepository(x => x.Set<UDFTYPE>(), (UDFTYPE x) => x.udf_type_id); }
        }

        IRepository<UDFVALUE, Tuple<int, int>> IP6EntitiesUnitOfWork.UDFVALUE
        {
            get { return GetRepository(x => x.Set<UDFVALUE>(), (UDFVALUE x) => Tuple.Create(x.udf_type_id, x.fk_id)); }
        }

        IRepository<UEVNTREG, Tuple<int, string, string, string>> IP6EntitiesUnitOfWork.UEVNTREG
        {
            get
            {
                return GetRepository(x => x.Set<UEVNTREG>(),
                    (UEVNTREG x) => Tuple.Create(x.user_id, x.app_name, x.operation_name, x.action_name));
            }
        }

        IRepository<UMEASURE, int> IP6EntitiesUnitOfWork.UMEASURE
        {
            get { return GetRepository(x => x.Set<UMEASURE>(), (UMEASURE x) => x.unit_id); }
        }

        IRepository<UPKLIST, Tuple<int, int>> IP6EntitiesUnitOfWork.UPKLIST
        {
            get { return GetRepository(x => x.Set<UPKLIST>(), (UPKLIST x) => Tuple.Create(x.session_id, x.pk_id)); }
        }

        IRepository<USERCOL, int> IP6EntitiesUnitOfWork.USERCOL
        {
            get { return GetRepository(x => x.Set<USERCOL>(), (USERCOL x) => x.user_col_id); }
        }

        IRepository<USERDATA, int> IP6EntitiesUnitOfWork.USERDATA
        {
            get { return GetRepository(x => x.Set<USERDATA>(), (USERDATA x) => x.user_data_id); }
        }

        IRepository<USERENG, int> IP6EntitiesUnitOfWork.USERENG
        {
            get { return GetRepository(x => x.Set<USERENG>(), (USERENG x) => x.user_eng_id); }
        }

        IRepository<USEROBS, Tuple<int, int>> IP6EntitiesUnitOfWork.USEROBS
        {
            get { return GetRepository(x => x.Set<USEROBS>(), (USEROBS x) => Tuple.Create(x.user_id, x.obs_id)); }
        }

        IRepository<USEROPEN, int> IP6EntitiesUnitOfWork.USEROPEN
        {
            get { return GetRepository(x => x.Set<USEROPEN>(), (USEROPEN x) => x.user_open_id); }
        }

        IRepository<USERS, int> IP6EntitiesUnitOfWork.USERS
        {
            get { return GetRepository(x => x.Set<USERS>(), (USERS x) => x.user_id); }
        }

        IRepository<USERSET, Tuple<string, string, int>> IP6EntitiesUnitOfWork.USERSET
        {
            get
            {
                return GetRepository(x => x.Set<USERSET>(),
                    (USERSET x) => Tuple.Create(x._namespace, x.setting_name, x.user_id));
            }
        }

        IRepository<USERWKSP, Tuple<int, int>> IP6EntitiesUnitOfWork.USERWKSP
        {
            get
            {
                return GetRepository(x => x.Set<USERWKSP>(), (USERWKSP x) => Tuple.Create(x.user_id, x.workspace_id));
            }
        }

        IRepository<USESSION, int> IP6EntitiesUnitOfWork.USESSION
        {
            get { return GetRepository(x => x.Set<USESSION>(), (USESSION x) => x.session_id); }
        }

        IRepository<USROPNVAL, Tuple<int, int, string>> IP6EntitiesUnitOfWork.USROPNVAL
        {
            get
            {
                return GetRepository(x => x.Set<USROPNVAL>(),
                    (USROPNVAL x) => Tuple.Create(x.user_open_id, x.pk_id, x.usropn_type));
            }
        }

        IRepository<VIEWPREF, int> IP6EntitiesUnitOfWork.VIEWPREF
        {
            get { return GetRepository(x => x.Set<VIEWPREF>(), (VIEWPREF x) => x.view_pref_id); }
        }

        IRepository<VIEWPROP, int> IP6EntitiesUnitOfWork.VIEWPROP
        {
            get { return GetRepository(x => x.Set<VIEWPROP>(), (VIEWPROP x) => x.view_id); }
        }

        IRepository<VWPREFDASH, Tuple<int, int>> IP6EntitiesUnitOfWork.VWPREFDASH
        {
            get
            {
                return GetRepository(x => x.Set<VWPREFDASH>(),
                    (VWPREFDASH x) => Tuple.Create(x.dashboard_id, x.view_pref_id));
            }
        }

        IRepository<VWPREFDATA, Tuple<int, string>> IP6EntitiesUnitOfWork.VWPREFDATA
        {
            get
            {
                return GetRepository(x => x.Set<VWPREFDATA>(),
                    (VWPREFDATA x) => Tuple.Create(x.view_pref_id, x.view_pref_key));
            }
        }

        IRepository<VWPREFUSER, Tuple<int, int>> IP6EntitiesUnitOfWork.VWPREFUSER
        {
            get
            {
                return GetRepository(x => x.Set<VWPREFUSER>(), (VWPREFUSER x) => Tuple.Create(x.view_pref_id, x.user_id));
            }
        }

        IRepository<WBRSCAT, int> IP6EntitiesUnitOfWork.WBRSCAT
        {
            get { return GetRepository(x => x.Set<WBRSCAT>(), (WBRSCAT x) => x.wbrs_cat_id); }
        }

        IRepository<WBSBUDG, int> IP6EntitiesUnitOfWork.WBSBUDG
        {
            get { return GetRepository(x => x.Set<WBSBUDG>(), (WBSBUDG x) => x.wbs_budg_id); }
        }

        IRepository<WBSMEMO, int> IP6EntitiesUnitOfWork.WBSMEMO
        {
            get { return GetRepository(x => x.Set<WBSMEMO>(), (WBSMEMO x) => x.wbs_memo_id); }
        }

        IRepository<WBSRSRC, int> IP6EntitiesUnitOfWork.WBSRSRC
        {
            get { return GetRepository(x => x.Set<WBSRSRC>(), (WBSRSRC x) => x.wbsrsrc_id); }
        }

        IRepository<WBSRSRC_QTY, Tuple<int, DateTime, DateTime>> IP6EntitiesUnitOfWork.WBSRSRC_QTY
        {
            get
            {
                return GetRepository(x => x.Set<WBSRSRC_QTY>(),
                    (WBSRSRC_QTY x) => Tuple.Create(x.wbsrsrc_id, x.week_start, x.month_start));
            }
        }

        IRepository<WBSSTEP, int> IP6EntitiesUnitOfWork.WBSSTEP
        {
            get { return GetRepository(x => x.Set<WBSSTEP>(), (WBSSTEP x) => x.wbs_step_id); }
        }

        IRepository<WKFLTMPL, int> IP6EntitiesUnitOfWork.WKFLTMPL
        {
            get { return GetRepository(x => x.Set<WKFLTMPL>(), (WKFLTMPL x) => x.wkfl_tmpl_id); }
        }

        IRepository<WKFLUSER, Tuple<int, int, int>> IP6EntitiesUnitOfWork.WKFLUSER
        {
            get
            {
                return GetRepository(x => x.Set<WKFLUSER>(),
                    (WKFLUSER x) => Tuple.Create(x.work_flow_id, x.user_id, x.stage_num));
            }
        }

        IRepository<WORKFLOW, int> IP6EntitiesUnitOfWork.WORKFLOW
        {
            get { return GetRepository(x => x.Set<WORKFLOW>(), (WORKFLOW x) => x.work_flow_id); }
        }

        IRepository<WORKSPACE, int> IP6EntitiesUnitOfWork.WORKSPACE
        {
            get { return GetRepository(x => x.Set<WORKSPACE>(), (WORKSPACE x) => x.workspace_id); }
        }
    }
}