namespace BluePrints.P6Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class P6Entities : DbContext
    {
        public P6Entities()
            : base("name=P6Entities")
        {
        }

        public virtual DbSet<ACCOUNT> ACCOUNT { get; set; }
        public virtual DbSet<ACTVCODE> ACTVCODE { get; set; }
        public virtual DbSet<ACTVTYPE> ACTVTYPE { get; set; }
        public virtual DbSet<ADMIN_CONFIG> ADMIN_CONFIG { get; set; }
        public virtual DbSet<BASETYPE> BASETYPE { get; set; }
        public virtual DbSet<BRE_REGISTRY> BRE_REGISTRY { get; set; }
        public virtual DbSet<BUDGCHNG> BUDGCHNG { get; set; }
        public virtual DbSet<CALENDAR> CALENDAR { get; set; }
        public virtual DbSet<COSTTYPE> COSTTYPE { get; set; }
        public virtual DbSet<CURRTYPE> CURRTYPE { get; set; }
        public virtual DbSet<DASHBOARD> DASHBOARD { get; set; }
        public virtual DbSet<DASHUSER> DASHUSER { get; set; }
        public virtual DbSet<DISCUSSION> DISCUSSION { get; set; }
        public virtual DbSet<DISCUSSION_READ> DISCUSSION_READ { get; set; }
        public virtual DbSet<DOCCATG> DOCCATG { get; set; }
        public virtual DbSet<DOCREVIEW> DOCREVIEW { get; set; }
        public virtual DbSet<DOCREVIEWTASK> DOCREVIEWTASK { get; set; }
        public virtual DbSet<DOCSTAT> DOCSTAT { get; set; }
        public virtual DbSet<DOCUMENT> DOCUMENT { get; set; }
        public virtual DbSet<EXPPROJ> EXPPROJ { get; set; }
        public virtual DbSet<EXTAPP> EXTAPP { get; set; }
        public virtual DbSet<FACTOR> FACTOR { get; set; }
        public virtual DbSet<FACTVAL> FACTVAL { get; set; }
        public virtual DbSet<FILTPROP> FILTPROP { get; set; }
        public virtual DbSet<FINDATES> FINDATES { get; set; }
        public virtual DbSet<FORMCATG> FORMCATG { get; set; }
        public virtual DbSet<FORMPROJ> FORMPROJ { get; set; }
        public virtual DbSet<FORMTMPL> FORMTMPL { get; set; }
        public virtual DbSet<FUNDSRC> FUNDSRC { get; set; }
        public virtual DbSet<GCHANGE> GCHANGE { get; set; }
        public virtual DbSet<HQDATA> HQDATA { get; set; }
        public virtual DbSet<HQUERY> HQUERY { get; set; }
        public virtual DbSet<ISSUHIST> ISSUHIST { get; set; }
        public virtual DbSet<ITERATION> ITERATION { get; set; }
        public virtual DbSet<ITERDAYS> ITERDAYS { get; set; }
        public virtual DbSet<ITERGOAL> ITERGOAL { get; set; }
        public virtual DbSet<JOBLOG> JOBLOG { get; set; }
        public virtual DbSet<JOBRPT> JOBRPT { get; set; }
        public virtual DbSet<JOBSVC> JOBSVC { get; set; }
        public virtual DbSet<LOCATION> LOCATION { get; set; }
        public virtual DbSet<MEMOTYPE> MEMOTYPE { get; set; }
        public virtual DbSet<NEXTKEY> NEXTKEY { get; set; }
        public virtual DbSet<NONWORK> NONWORK { get; set; }
        public virtual DbSet<NOTE> NOTE { get; set; }
        public virtual DbSet<OBS> OBS { get; set; }
        public virtual DbSet<OBSPROJ> OBSPROJ { get; set; }
        public virtual DbSet<PCATTYPE> PCATTYPE { get; set; }
        public virtual DbSet<PCATUSER> PCATUSER { get; set; }
        public virtual DbSet<PCATVAL> PCATVAL { get; set; }
        public virtual DbSet<PFOLIO> PFOLIO { get; set; }
        public virtual DbSet<PHASE> PHASE { get; set; }
        public virtual DbSet<POBS> POBS { get; set; }
        public virtual DbSet<PREFER> PREFER { get; set; }
        public virtual DbSet<PRMQUEUE> PRMQUEUE { get; set; }
        public virtual DbSet<PROCGROUP> PROCGROUP { get; set; }
        public virtual DbSet<PROCITEM> PROCITEM { get; set; }
        public virtual DbSet<PROFILE> PROFILE { get; set; }
        public virtual DbSet<PROFPRIV> PROFPRIV { get; set; }
        public virtual DbSet<PROJCOST> PROJCOST { get; set; }
        public virtual DbSet<PROJECT> PROJECT { get; set; }
        public virtual DbSet<PROJEST> PROJEST { get; set; }
        public virtual DbSet<PROJFUND> PROJFUND { get; set; }
        public virtual DbSet<PROJISSU> PROJISSU { get; set; }
        public virtual DbSet<PROJPCAT> PROJPCAT { get; set; }
        public virtual DbSet<PROJPROP> PROJPROP { get; set; }
        public virtual DbSet<PROJRISK> PROJRISK { get; set; }
        public virtual DbSet<PROJSHAR> PROJSHAR { get; set; }
        public virtual DbSet<PROJTHRS> PROJTHRS { get; set; }
        public virtual DbSet<PROJWBS> PROJWBS { get; set; }
        public virtual DbSet<PROJWSRPT> PROJWSRPT { get; set; }
        public virtual DbSet<PRPFOLIO> PRPFOLIO { get; set; }
        public virtual DbSet<PUBUSER> PUBUSER { get; set; }
        public virtual DbSet<QUERYLIB> QUERYLIB { get; set; }
        public virtual DbSet<RCATTYPE> RCATTYPE { get; set; }
        public virtual DbSet<RCATVAL> RCATVAL { get; set; }
        public virtual DbSet<REITTYPE> REITTYPE { get; set; }
        public virtual DbSet<RELEASE> RELEASE { get; set; }
        public virtual DbSet<RELITEMS> RELITEMS { get; set; }
        public virtual DbSet<RFOLIO> RFOLIO { get; set; }
        public virtual DbSet<RISKCTRL> RISKCTRL { get; set; }
        public virtual DbSet<RISKTYPE> RISKTYPE { get; set; }
        public virtual DbSet<RLFOLIO> RLFOLIO { get; set; }
        public virtual DbSet<ROLELIMIT> ROLELIMIT { get; set; }
        public virtual DbSet<ROLERATE> ROLERATE { get; set; }
        public virtual DbSet<ROLES> ROLES { get; set; }
        public virtual DbSet<ROLFOLIO> ROLFOLIO { get; set; }
        public virtual DbSet<RPT> RPT { get; set; }
        public virtual DbSet<RPTBATCH> RPTBATCH { get; set; }
        public virtual DbSet<RPTGROUP> RPTGROUP { get; set; }
        public virtual DbSet<RPTLIST> RPTLIST { get; set; }
        public virtual DbSet<RSRC> RSRC { get; set; }
        public virtual DbSet<RSRCANDASH> RSRCANDASH { get; set; }
        public virtual DbSet<RSRCANVIEW> RSRCANVIEW { get; set; }
        public virtual DbSet<RSRCCURV> RSRCCURV { get; set; }
        public virtual DbSet<RSRCHOUR> RSRCHOUR { get; set; }
        public virtual DbSet<RSRCPROP> RSRCPROP { get; set; }
        public virtual DbSet<RSRCRATE> RSRCRATE { get; set; }
        public virtual DbSet<RSRCRCAT> RSRCRCAT { get; set; }
        public virtual DbSet<RSRCROLE> RSRCROLE { get; set; }
        public virtual DbSet<RSRCSEC> RSRCSEC { get; set; }
        public virtual DbSet<RSRFOLIO> RSRFOLIO { get; set; }
        public virtual DbSet<SCENARIO> SCENARIO { get; set; }
        public virtual DbSet<SCENPROJ> SCENPROJ { get; set; }
        public virtual DbSet<SCENROLE> SCENROLE { get; set; }
        public virtual DbSet<SCENUSER> SCENUSER { get; set; }
        public virtual DbSet<SETTINGS> SETTINGS { get; set; }
        public virtual DbSet<SHIFT> SHIFT { get; set; }
        public virtual DbSet<SHIFTPER> SHIFTPER { get; set; }
        public virtual DbSet<SPIDMAP> SPIDMAP { get; set; }
        public virtual DbSet<SUMTRSRC> SUMTRSRC { get; set; }
        public virtual DbSet<TASK> TASK { get; set; }
        public virtual DbSet<TASKACTV> TASKACTV { get; set; }
        public virtual DbSet<TASKDOC> TASKDOC { get; set; }
        public virtual DbSet<TASKFDBK> TASKFDBK { get; set; }
        public virtual DbSet<TASKFIN> TASKFIN { get; set; }
        public virtual DbSet<TASKMEMO> TASKMEMO { get; set; }
        public virtual DbSet<TASKNOTE> TASKNOTE { get; set; }
        public virtual DbSet<TASKPRED> TASKPRED { get; set; }
        public virtual DbSet<TASKPROC> TASKPROC { get; set; }
        public virtual DbSet<TASKRISK> TASKRISK { get; set; }
        public virtual DbSet<TASKRSRC> TASKRSRC { get; set; }
        public virtual DbSet<TASKSUM> TASKSUM { get; set; }
        public virtual DbSet<TASKSUMFIN> TASKSUMFIN { get; set; }
        public virtual DbSet<TASKUSER> TASKUSER { get; set; }
        public virtual DbSet<TASKWKSP> TASKWKSP { get; set; }
        public virtual DbSet<THRSPARM> THRSPARM { get; set; }
        public virtual DbSet<TIMESHT> TIMESHT { get; set; }
        public virtual DbSet<TMPLCATG> TMPLCATG { get; set; }
        public virtual DbSet<TRAKVIEW> TRAKVIEW { get; set; }
        public virtual DbSet<TRSRCFIN> TRSRCFIN { get; set; }
        public virtual DbSet<TRSRCSUM> TRSRCSUM { get; set; }
        public virtual DbSet<TRSRCSUMFN> TRSRCSUMFN { get; set; }
        public virtual DbSet<TSAUDIT> TSAUDIT { get; set; }
        public virtual DbSet<TSDATES> TSDATES { get; set; }
        public virtual DbSet<TSDELEGATE> TSDELEGATE { get; set; }
        public virtual DbSet<UACCESS> UACCESS { get; set; }
        public virtual DbSet<UDFCODE> UDFCODE { get; set; }
        public virtual DbSet<UDFTYPE> UDFTYPE { get; set; }
        public virtual DbSet<UDFVALUE> UDFVALUE { get; set; }
        public virtual DbSet<UEVNTREG> UEVNTREG { get; set; }
        public virtual DbSet<UMEASURE> UMEASURE { get; set; }
        public virtual DbSet<USERCOL> USERCOL { get; set; }
        public virtual DbSet<USERDATA> USERDATA { get; set; }
        public virtual DbSet<USERENG> USERENG { get; set; }
        public virtual DbSet<USEROBS> USEROBS { get; set; }
        public virtual DbSet<USEROPEN> USEROPEN { get; set; }
        public virtual DbSet<USERS> USERS { get; set; }
        public virtual DbSet<USERSET> USERSET { get; set; }
        public virtual DbSet<USERWKSP> USERWKSP { get; set; }
        public virtual DbSet<USESSION> USESSION { get; set; }
        public virtual DbSet<USROPNVAL> USROPNVAL { get; set; }
        public virtual DbSet<VIEWPREF> VIEWPREF { get; set; }
        public virtual DbSet<VIEWPROP> VIEWPROP { get; set; }
        public virtual DbSet<VWPREFDASH> VWPREFDASH { get; set; }
        public virtual DbSet<VWPREFDATA> VWPREFDATA { get; set; }
        public virtual DbSet<VWPREFUSER> VWPREFUSER { get; set; }
        public virtual DbSet<WBRSCAT> WBRSCAT { get; set; }
        public virtual DbSet<WBSBUDG> WBSBUDG { get; set; }
        public virtual DbSet<WBSMEMO> WBSMEMO { get; set; }
        public virtual DbSet<WBSRSRC> WBSRSRC { get; set; }
        public virtual DbSet<WBSRSRC_QTY> WBSRSRC_QTY { get; set; }
        public virtual DbSet<WBSSTEP> WBSSTEP { get; set; }
        public virtual DbSet<WKFLTMPL> WKFLTMPL { get; set; }
        public virtual DbSet<WKFLUSER> WKFLUSER { get; set; }
        public virtual DbSet<WORKFLOW> WORKFLOW { get; set; }
        public virtual DbSet<WORKSPACE> WORKSPACE { get; set; }
        public virtual DbSet<DLTACCT> DLTACCT { get; set; }
        public virtual DbSet<DLTACTV> DLTACTV { get; set; }
        public virtual DbSet<DLTOBS> DLTOBS { get; set; }
        public virtual DbSet<DLTROLE> DLTROLE { get; set; }
        public virtual DbSet<DLTRSRC> DLTRSRC { get; set; }
        public virtual DbSet<DLTRSRL> DLTRSRL { get; set; }
        public virtual DbSet<DLTUSER> DLTUSER { get; set; }
        public virtual DbSet<REFRDEL> REFRDEL { get; set; }
        public virtual DbSet<SUMPROJCOST> SUMPROJCOST { get; set; }
        public virtual DbSet<SUMTASK> SUMTASK { get; set; }
        public virtual DbSet<SUMTASKSPREAD> SUMTASKSPREAD { get; set; }
        public virtual DbSet<TPROJMAP> TPROJMAP { get; set; }
        public virtual DbSet<UPKLIST> UPKLIST { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<P6Entities>(null);   

            modelBuilder.Entity<ACCOUNT>()
                .Property(e => e.acct_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<ACCOUNT>()
                .Property(e => e.acct_name)
                .IsUnicode(false);

            modelBuilder.Entity<ACCOUNT>()
                .Property(e => e.acct_descr)
                .IsUnicode(false);

            modelBuilder.Entity<ACCOUNT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACCOUNT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .Property(e => e.short_name)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .Property(e => e.actv_code_name)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .Property(e => e.color)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVCODE>()
                .HasMany(e => e.TASKACTV)
                .WithRequired(e => e.ACTVCODE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ACTVTYPE>()
                .Property(e => e.actv_code_type)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVTYPE>()
                .Property(e => e.actv_code_type_scope)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVTYPE>()
                .Property(e => e.super_flag)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ACTVTYPE>()
                .HasMany(e => e.ACTVCODE)
                .WithRequired(e => e.ACTVTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ACTVTYPE>()
                .HasMany(e => e.TASKACTV)
                .WithRequired(e => e.ACTVTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.config_name)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.config_type)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.factory_version)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.config_value)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.config_data)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ADMIN_CONFIG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<BASETYPE>()
                .Property(e => e.base_type)
                .IsUnicode(false);

            modelBuilder.Entity<BASETYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<BASETYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.bre_registry_id)
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.ip_address)
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.status_code)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.config_name)
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.config_changed_flag)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<BRE_REGISTRY>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.chng_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.chng_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.chng_by_name)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.chng_descr)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<BUDGCHNG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.default_flag)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.rsrc_private)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.clndr_name)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.clndr_type)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.clndr_data)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.day_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.week_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.year_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.month_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<CALENDAR>()
                .HasMany(e => e.RSRC)
                .WithRequired(e => e.CALENDAR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CALENDAR>()
                .HasMany(e => e.TASK)
                .WithRequired(e => e.CALENDAR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<COSTTYPE>()
                .Property(e => e.cost_type)
                .IsUnicode(false);

            modelBuilder.Entity<COSTTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<COSTTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.curr_symbol)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.decimal_symbol)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.digit_group_symbol)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.pos_curr_fmt_type)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.neg_curr_fmt_type)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.curr_type)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.curr_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.base_exch_rate)
                .HasPrecision(22, 6);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<CURRTYPE>()
                .HasMany(e => e.PREFER)
                .WithRequired(e => e.CURRTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CURRTYPE>()
                .HasMany(e => e.RSRC)
                .WithRequired(e => e.CURRTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CURRTYPE>()
                .HasMany(e => e.USERS)
                .WithRequired(e => e.CURRTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.dashboard_name)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.lock_filter_flag)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.dashboard_layout_data)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.portlet_settings_data)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DASHBOARD>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DASHUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DASHUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DISCUSSION>()
                .Property(e => e.discussion_value)
                .IsUnicode(false);

            modelBuilder.Entity<DISCUSSION>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DISCUSSION>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DISCUSSION_READ>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DISCUSSION_READ>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCCATG>()
                .Property(e => e.doc_catg_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCCATG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCCATG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.review_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.review_descr)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.review_type)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEW>()
                .HasMany(e => e.DOCREVIEWTASK)
                .WithRequired(e => e.DOCREVIEW)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DOCREVIEWTASK>()
                .Property(e => e.comments)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEWTASK>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEWTASK>()
                .Property(e => e.attachment_uuid)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEWTASK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCREVIEWTASK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCSTAT>()
                .Property(e => e.doc_status_code)
                .IsUnicode(false);

            modelBuilder.Entity<DOCSTAT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCSTAT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.deliv_flag)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.doc_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.version_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.tmpl_guid)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.doc_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.author_name)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.private_loc)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.public_loc)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.doc_content)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.doc_mgmt_type)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.external_doc_key)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.cr_external_doc_key)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<DOCUMENT>()
                .HasMany(e => e.DOCREVIEW)
                .WithRequired(e => e.DOCUMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DOCUMENT>()
                .HasMany(e => e.TASKDOC)
                .WithRequired(e => e.DOCUMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.exp_group)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.exp_proj_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.login_supplied_flag)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.exp_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.exp_passwd)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<EXPPROJ>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.login_supply_flag)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_exe_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_passwd)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_data_name)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_parm_string)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.app_data_loc)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<EXTAPP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .Property(e => e.fact_type)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .Property(e => e.fact_name)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .Property(e => e.fact_descr)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FACTOR>()
                .HasMany(e => e.FACTVAL)
                .WithRequired(e => e.FACTOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FACTVAL>()
                .Property(e => e.fact_value)
                .IsUnicode(false);

            modelBuilder.Entity<FACTVAL>()
                .Property(e => e.fact_value_descr)
                .IsUnicode(false);

            modelBuilder.Entity<FACTVAL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FACTVAL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.filter_type)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.filter_name)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.filter_data)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FILTPROP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FINDATES>()
                .Property(e => e.fin_dates_name)
                .IsUnicode(false);

            modelBuilder.Entity<FINDATES>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FINDATES>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FINDATES>()
                .HasMany(e => e.PROJECT)
                .WithOptional(e => e.FINDATES)
                .HasForeignKey(e => e.last_fin_dates_id);

            modelBuilder.Entity<FINDATES>()
                .HasMany(e => e.TRSRCFIN)
                .WithRequired(e => e.FINDATES)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FINDATES>()
                .HasMany(e => e.WBSRSRC_QTY)
                .WithOptional(e => e.FINDATES)
                .HasForeignKey(e => e.fin_dates_id1);

            modelBuilder.Entity<FINDATES>()
                .HasMany(e => e.WBSRSRC_QTY1)
                .WithOptional(e => e.FINDATES1)
                .HasForeignKey(e => e.fin_dates_id2);

            modelBuilder.Entity<FORMCATG>()
                .Property(e => e.form_catg_name)
                .IsUnicode(false);

            modelBuilder.Entity<FORMCATG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FORMCATG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FORMPROJ>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FORMPROJ>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FORMTMPL>()
                .Property(e => e.form_tmpl_name)
                .IsUnicode(false);

            modelBuilder.Entity<FORMTMPL>()
                .Property(e => e.form_tmpl_desc)
                .IsUnicode(false);

            modelBuilder.Entity<FORMTMPL>()
                .Property(e => e.form_tmpl_data)
                .IsUnicode(false);

            modelBuilder.Entity<FORMTMPL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FORMTMPL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FUNDSRC>()
                .Property(e => e.fund_name)
                .IsUnicode(false);

            modelBuilder.Entity<FUNDSRC>()
                .Property(e => e.fund_descr)
                .IsUnicode(false);

            modelBuilder.Entity<FUNDSRC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<FUNDSRC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<FUNDSRC>()
                .HasMany(e => e.PROJFUND)
                .WithRequired(e => e.FUNDSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GCHANGE>()
                .Property(e => e.gchange_name)
                .IsUnicode(false);

            modelBuilder.Entity<GCHANGE>()
                .Property(e => e.gchange_data)
                .IsUnicode(false);

            modelBuilder.Entity<GCHANGE>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<GCHANGE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<GCHANGE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<HQUERY>()
                .Property(e => e.order_value)
                .IsUnicode(false);

            modelBuilder.Entity<HQUERY>()
                .Property(e => e.order_ids)
                .IsUnicode(false);

            modelBuilder.Entity<ISSUHIST>()
                .Property(e => e.issue_history)
                .IsUnicode(false);

            modelBuilder.Entity<ISSUHIST>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ISSUHIST>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERATION>()
                .Property(e => e.iteration_name)
                .IsUnicode(false);

            modelBuilder.Entity<ITERATION>()
                .Property(e => e.capacity_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ITERATION>()
                .Property(e => e.remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<ITERATION>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERATION>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERDAYS>()
                .Property(e => e.act_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<ITERDAYS>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<ITERDAYS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERDAYS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERGOAL>()
                .Property(e => e.goal_name)
                .IsUnicode(false);

            modelBuilder.Entity<ITERGOAL>()
                .Property(e => e.goal_descr)
                .IsUnicode(false);

            modelBuilder.Entity<ITERGOAL>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<ITERGOAL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ITERGOAL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBLOG>()
                .Property(e => e.job_log_data)
                .IsUnicode(false);

            modelBuilder.Entity<JOBLOG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBLOG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBRPT>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<JOBRPT>()
                .Property(e => e.jobrpt_data)
                .IsUnicode(false);

            modelBuilder.Entity<JOBRPT>()
                .Property(e => e.personal_portal_flag)
                .IsUnicode(false);

            modelBuilder.Entity<JOBRPT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBRPT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.audit_flag)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.job_type)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.job_name)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.recur_data)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.recur_type)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.last_error_descr)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.audit_file_path)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.job_data)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<JOBSVC>()
                .HasOptional(e => e.JOBLOG)
                .WithRequired(e => e.JOBSVC)
                .WillCascadeOnDelete();

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.location_name)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.location_type)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.address_line1)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.address_line2)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.address_line3)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.city_name)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.municipality_name)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.state_name)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.state_code)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.country_name)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.country_code)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.postal_code)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.longitude)
                .HasPrecision(28, 10);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.latitude)
                .HasPrecision(28, 10);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.geo_location)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<LOCATION>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.eps_flag)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.proj_flag)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.wbs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.task_flag)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.memo_type)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOTYPE>()
                .HasMany(e => e.TASKMEMO)
                .WithRequired(e => e.MEMOTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MEMOTYPE>()
                .HasMany(e => e.WBSMEMO)
                .WithRequired(e => e.MEMOTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NEXTKEY>()
                .Property(e => e.key_name)
                .IsUnicode(false);

            modelBuilder.Entity<NONWORK>()
                .Property(e => e.nonwork_code)
                .IsUnicode(false);

            modelBuilder.Entity<NONWORK>()
                .Property(e => e.nonwork_type)
                .IsUnicode(false);

            modelBuilder.Entity<NONWORK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<NONWORK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.type_name)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.note_value)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.user_name)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<NOTE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .Property(e => e.obs_name)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .Property(e => e.obs_descr)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<OBS>()
                .HasMany(e => e.PROJISSU)
                .WithRequired(e => e.OBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OBS>()
                .HasMany(e => e.PROJTHRS)
                .WithRequired(e => e.OBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OBS>()
                .HasMany(e => e.PROJWBS)
                .WithRequired(e => e.OBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OBS>()
                .HasMany(e => e.USEROBS)
                .WithRequired(e => e.OBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.proj_catg_type)
                .IsUnicode(false);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.super_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.proj_catg_type_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.max_proj_catg_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATTYPE>()
                .HasMany(e => e.PCATVAL)
                .WithRequired(e => e.PCATTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PCATTYPE>()
                .HasMany(e => e.PROJPCAT)
                .WithRequired(e => e.PCATTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PCATUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATVAL>()
                .Property(e => e.proj_catg_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<PCATVAL>()
                .Property(e => e.proj_catg_name)
                .IsUnicode(false);

            modelBuilder.Entity<PCATVAL>()
                .Property(e => e.proj_catg_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PCATVAL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATVAL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PCATVAL>()
                .HasMany(e => e.PCATUSER)
                .WithRequired(e => e.PCATVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PCATVAL>()
                .HasMany(e => e.PROJPCAT)
                .WithRequired(e => e.PCATVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.closed_proj_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.whatif_proj_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.pfolio_type)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.pfolio_name)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.pfolio_descr)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.pfolio_data)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PFOLIO>()
                .HasMany(e => e.PRPFOLIO)
                .WithRequired(e => e.PFOLIO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PHASE>()
                .Property(e => e.phase_name)
                .IsUnicode(false);

            modelBuilder.Entity<PHASE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PHASE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<POBS>()
                .Property(e => e.pobs_name)
                .IsUnicode(false);

            modelBuilder.Entity<POBS>()
                .Property(e => e.pobs_descr)
                .IsUnicode(false);

            modelBuilder.Entity<POBS>()
                .Property(e => e.pobs_manager)
                .IsUnicode(false);

            modelBuilder.Entity<POBS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<POBS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.def_target_drtn_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.day_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.week_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.year_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.month_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.complete_task_hrs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.default_timesheet_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.future_ts_hrs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ev_fix_cost_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ts_daily_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.notstart_task_hrs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.prestart_task_hrs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.postend_task_hrs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ermm_enabled_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.name_sep_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.year_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.month_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.week_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.day_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.hour_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.minute_char)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ts_approval_type)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.db_name)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.tasksum_period_type)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.trsrcsum_period_type)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.database_version)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.phase_label)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ev_etc_user_value)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ev_compute_type)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ev_etc_compute_type)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_header_1)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_header_2)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_header_3)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_footer_1)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_footer_2)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_footer_3)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_user_1)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_user_2)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.rpt_user_3)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.license_data)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.allow_user_time_period_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.exp_root_url)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.ts_rsrc_def_asgn_actv_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PREFER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.queue_name)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.msg_key)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.enqueue_user)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.sender_name)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.dequeue_user)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.msg_type)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.msg_sub_type)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.key1)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.key2)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.err_msg)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.queue_payload)
                .IsUnicode(false);

            modelBuilder.Entity<PRMQUEUE>()
                .Property(e => e.queue_payload_extended)
                .IsUnicode(false);

            modelBuilder.Entity<PROCGROUP>()
                .Property(e => e.proc_group_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROCGROUP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROCGROUP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROCITEM>()
                .Property(e => e.proc_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROCITEM>()
                .Property(e => e.proc_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROCITEM>()
                .Property(e => e.proc_descr)
                .IsUnicode(false);

            modelBuilder.Entity<PROCITEM>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROCITEM>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.default_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.superuser_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.scope_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.prof_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.prof_descr)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROFILE>()
                .HasMany(e => e.PROFPRIV)
                .WithRequired(e => e.PROFILE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROFILE>()
                .HasMany(e => e.USEROBS)
                .WithRequired(e => e.PROFILE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROFPRIV>()
                .Property(e => e.allow_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROFPRIV>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROFPRIV>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.auto_compute_act_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.cost_load_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.qty_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.target_qty)
                .HasPrecision(19, 6);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.cost_per_qty)
                .HasPrecision(21, 8);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.po_number)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.vendor_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.cost_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.cost_descr)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJCOST>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.chng_eff_cmp_pct_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.rsrc_self_add_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.allow_complete_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.rsrc_multi_assign_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.checkout_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.project_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.step_complete_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.cost_qty_recalc_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.sum_only_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.batch_sum_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.name_sep_char)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_complete_pct_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.proj_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.critical_drtn_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_cost_per_qty)
                .HasPrecision(21, 8);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_duration_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.task_code_prefix)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_qty_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.add_by_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.web_local_root_path)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.proj_url)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_rate_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.act_this_per_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_task_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.act_pct_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.add_act_remain_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.critical_path_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.task_code_prefix_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.def_rollup_dates_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.rem_target_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.reset_planned_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.allow_neg_act_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.msp_managed_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.msp_update_actuals_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.sum_assign_level)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.use_project_baseline_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.ts_rsrc_vw_compl_asgn_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.ts_rsrc_mark_act_finish_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.ts_rsrc_vw_inact_actv_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.cr_external_key)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.intg_proj_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.control_updates_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.hist_interval)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.hist_level)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.BUDGCHNG)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasOptional(e => e.EXPPROJ)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ISSUHIST)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJCOST)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJEST)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJFUND)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJISSU)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJPCAT)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJPROP)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJSHAR)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJTHRS)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJWBS)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.RELEASE)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.RELITEMS)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.SUMTRSRC)
                .WithOptional(e => e.PROJECT)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASK)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKACTV)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKDOC)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKFDBK)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKMEMO)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKNOTE)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKPRED)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.proj_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKPRED1)
                .WithRequired(e => e.PROJECT1)
                .HasForeignKey(e => e.pred_proj_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKPROC)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKRSRC)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TASKWKSP)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TRSRCFIN)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TSDELEGATE)
                .WithOptional(e => e.PROJECT)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.UDFVALUE)
                .WithOptional(e => e.PROJECT)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.USERWKSP)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.VIEWPROP)
                .WithOptional(e => e.PROJECT)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WBSBUDG)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WBSMEMO)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WBSSTEP)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WORKFLOW)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WORKSPACE)
                .WithRequired(e => e.PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.applied_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.rsrc_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.est_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.est_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.fp_prod_avg_value)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.est_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.est_notes)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJEST>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJFUND>()
                .Property(e => e.fund_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJFUND>()
                .Property(e => e.fund_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJFUND>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJFUND>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.priority_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.issue_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.issue_value)
                .HasPrecision(15, 2);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.lo_parm_value)
                .HasPrecision(15, 2);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.hi_parm_value)
                .HasPrecision(15, 2);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.add_by_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.issue_notes)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJISSU>()
                .HasOptional(e => e.ISSUHIST)
                .WithRequired(e => e.PROJISSU);

            modelBuilder.Entity<PROJPCAT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJPCAT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJPROP>()
                .Property(e => e.prop_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJPROP>()
                .Property(e => e.prop_value)
                .IsUnicode(false);

            modelBuilder.Entity<PROJPROP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJPROP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_descr)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_to_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.response_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.response_text)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.pre_rsp_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.pre_rsp_schd_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.pre_rsp_cost_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.post_rsp_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.post_rsp_schd_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.post_rsp_cost_prblty)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_cause)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_effect)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.notes)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_code)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.risk_desc)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJRISK>()
                .HasMany(e => e.TASKRISK)
                .WithRequired(e => e.PROJRISK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJSHAR>()
                .Property(e => e.load_status)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.priority_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.thresh_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.lo_parm_value)
                .HasPrecision(15, 2);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.hi_parm_value)
                .HasPrecision(15, 2);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.window_start)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.window_end)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJTHRS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.est_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.proj_node_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.sum_data_flag)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.wbs_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.wbs_name)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.ev_etc_user_value)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.orig_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.indep_remain_total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.ann_dscnt_rate_pct)
                .HasPrecision(18, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.dscnt_period_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.indep_remain_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.ev_compute_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.ev_etc_compute_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.tmpl_guid)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.original_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.rqmt_rem_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.intg_type)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.BUDGCHNG)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.PROJEST)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.PRPFOLIO)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.SUMTRSRC)
                .WithOptional(e => e.PROJWBS)
                .WillCascadeOnDelete();

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.TASK)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.TASKDOC)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.WBSBUDG)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.WBSMEMO)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWBS>()
                .HasMany(e => e.WBSSTEP)
                .WithRequired(e => e.PROJWBS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJWSRPT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PROJWSRPT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PRPFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<PRPFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<PUBUSER>()
                .Property(e => e.private_db_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<PUBUSER>()
                .Property(e => e.private_db_passwd)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.app_name)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.core_flag)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.match_sql)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.hints)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.replacement_sql)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<QUERYLIB>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RCATTYPE>()
                .Property(e => e.rsrc_catg_type)
                .IsUnicode(false);

            modelBuilder.Entity<RCATTYPE>()
                .Property(e => e.super_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RCATTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RCATTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RCATTYPE>()
                .HasMany(e => e.RCATVAL)
                .WithRequired(e => e.RCATTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RCATTYPE>()
                .HasMany(e => e.RSRCRCAT)
                .WithRequired(e => e.RCATTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RCATVAL>()
                .Property(e => e.rsrc_catg_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<RCATVAL>()
                .Property(e => e.rsrc_catg_name)
                .IsUnicode(false);

            modelBuilder.Entity<RCATVAL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RCATVAL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RCATVAL>()
                .HasMany(e => e.RSRCRCAT)
                .WithRequired(e => e.RCATVAL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REITTYPE>()
                .Property(e => e.related_item)
                .IsUnicode(false);

            modelBuilder.Entity<REITTYPE>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<REITTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<REITTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<REITTYPE>()
                .HasMany(e => e.RELITEMS)
                .WithRequired(e => e.REITTYPE)
                .HasForeignKey(e => e.item_type_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REITTYPE>()
                .HasMany(e => e.RELITEMS1)
                .WithRequired(e => e.REITTYPE1)
                .HasForeignKey(e => e.related_item_type_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RELEASE>()
                .Property(e => e.release_name)
                .IsUnicode(false);

            modelBuilder.Entity<RELEASE>()
                .Property(e => e.release_theme)
                .IsUnicode(false);

            modelBuilder.Entity<RELEASE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RELEASE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RELITEMS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RELITEMS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.rfolio_name)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.rfolio_type)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.rfolio_descr)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.team_capacity_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.rfolio_data)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RFOLIO>()
                .HasMany(e => e.ITERGOAL)
                .WithRequired(e => e.RFOLIO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RFOLIO>()
                .HasMany(e => e.PROJWBS)
                .WithOptional(e => e.RFOLIO)
                .HasForeignKey(e => e.resp_team_id);

            modelBuilder.Entity<RFOLIO>()
                .HasMany(e => e.RSRFOLIO)
                .WithRequired(e => e.RFOLIO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RISKCTRL>()
                .Property(e => e.risk_control)
                .IsUnicode(false);

            modelBuilder.Entity<RISKCTRL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RISKCTRL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RISKTYPE>()
                .Property(e => e.risk_type)
                .IsUnicode(false);

            modelBuilder.Entity<RISKTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RISKTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.rlfolio_name)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.rlfolio_type)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.rlfolio_descr)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.rlfolio_data)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RLFOLIO>()
                .HasMany(e => e.ROLFOLIO)
                .WithRequired(e => e.RLFOLIO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLELIMIT>()
                .Property(e => e.max_qty_per_hr)
                .HasPrecision(16, 8);

            modelBuilder.Entity<ROLELIMIT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLELIMIT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.cost_per_qty)
                .HasPrecision(21, 8);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.cost_per_qty2)
                .HasPrecision(21, 8);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.cost_per_qty3)
                .HasPrecision(21, 8);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.cost_per_qty4)
                .HasPrecision(21, 8);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.cost_per_qty5)
                .HasPrecision(21, 8);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLERATE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.role_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.role_name)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.def_cost_qty_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.cost_qty_type)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.role_descr)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLES>()
                .HasMany(e => e.ROLFOLIO)
                .WithRequired(e => e.ROLES)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLES>()
                .HasMany(e => e.RSRCROLE)
                .WithRequired(e => e.ROLES)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<ROLFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.global_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.rpt_type)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.rpt_name)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.rpt_area)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.rpt_state)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.rpt_data)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPT>()
                .HasMany(e => e.RPTLIST)
                .WithRequired(e => e.RPT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RPTBATCH>()
                .Property(e => e.rpt_batch_name)
                .IsUnicode(false);

            modelBuilder.Entity<RPTBATCH>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPTBATCH>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPTBATCH>()
                .HasMany(e => e.RPTLIST)
                .WithRequired(e => e.RPTBATCH)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RPTGROUP>()
                .Property(e => e.rpt_group_name)
                .IsUnicode(false);

            modelBuilder.Entity<RPTGROUP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPTGROUP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPTLIST>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RPTLIST>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.timesheet_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.active_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.rsrc_type)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.auto_compute_act_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.ot_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.def_cost_qty_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.rsrc_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.rsrc_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.ot_factor)
                .HasPrecision(10, 3);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.def_qty_per_hr)
                .HasPrecision(16, 8);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.cost_qty_type)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.office_phone)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.other_phone)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.employee_code)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.rsrc_title_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.email_addr)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.rsrc_notes)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.intg_type)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.PREFER)
                .WithOptional(e => e.RSRC)
                .HasForeignKey(e => e.default_rsrc_sec_id);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRCHOUR)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRCPROP)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRCRATE)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRCRCAT)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRCROLE)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRC>()
                .HasMany(e => e.RSRFOLIO)
                .WithRequired(e => e.RSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RSRCANDASH>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANDASH>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANVIEW>()
                .Property(e => e.rsrcan_view_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANVIEW>()
                .Property(e => e.rsrcan_view_type)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANVIEW>()
                .Property(e => e.rsrcan_view_data)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANVIEW>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCANVIEW>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCCURV>()
                .Property(e => e.curv_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCCURV>()
                .Property(e => e.default_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCCURV>()
                .Property(e => e.curv_data)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCCURV>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCCURV>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.task_ts_flag)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.pend_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.pend_ot_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.ot_hr_cnt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCHOUR>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCPROP>()
                .Property(e => e.prop_name)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCPROP>()
                .Property(e => e.prop_value)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCPROP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCPROP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.max_qty_per_hr)
                .HasPrecision(16, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.cost_per_qty)
                .HasPrecision(21, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.cost_per_qty2)
                .HasPrecision(21, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.cost_per_qty3)
                .HasPrecision(21, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.cost_per_qty4)
                .HasPrecision(21, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.cost_per_qty5)
                .HasPrecision(21, 8);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCRATE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCRCAT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCRCAT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCROLE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCROLE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCSEC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRCSEC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRFOLIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<RSRFOLIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.scenario_name)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.scenario_type)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.view_type)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENARIO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENPROJ>()
                .Property(e => e.selected_flag)
                .IsUnicode(false);

            modelBuilder.Entity<SCENPROJ>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENPROJ>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENROLE>()
                .Property(e => e.selected_flag)
                .IsUnicode(false);

            modelBuilder.Entity<SCENROLE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENROLE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SCENUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SETTINGS>()
                .Property(e => e._namespace)
                .IsUnicode(false);

            modelBuilder.Entity<SETTINGS>()
                .Property(e => e.setting_name)
                .IsUnicode(false);

            modelBuilder.Entity<SETTINGS>()
                .Property(e => e.setting_value)
                .IsUnicode(false);

            modelBuilder.Entity<SETTINGS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SETTINGS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SHIFT>()
                .Property(e => e.shift_name)
                .IsUnicode(false);

            modelBuilder.Entity<SHIFT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SHIFT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SHIFT>()
                .HasMany(e => e.SHIFTPER)
                .WithRequired(e => e.SHIFT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SHIFTPER>()
                .Property(e => e.shift_start_hr_num)
                .HasPrecision(10, 3);

            modelBuilder.Entity<SHIFTPER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<SHIFTPER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<SPIDMAP>()
                .Property(e => e.user_name)
                .IsUnicode(false);

            modelBuilder.Entity<SPIDMAP>()
                .Property(e => e.app_name)
                .IsUnicode(false);

            modelBuilder.Entity<SPIDMAP>()
                .Property(e => e.audit_info_extended)
                .IsUnicode(false);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.spread_type)
                .IsUnicode(false);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_ot_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_ot_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_ot_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_ot_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_reg_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_reg_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_act_reg_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_act_reg_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_late_remain_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_late_remain_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_late_remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_late_remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_remain_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_remain_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_target_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_target_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_total_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_total_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.staffed_total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.unstaffed_total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.fin_period_act_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.fin_period_act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.fin_period_total_qty)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTRSRC>()
                .Property(e => e.fin_period_total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.est_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TASK>()
                .Property(e => e.phys_complete_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TASK>()
                .Property(e => e.rev_fdbk_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.lock_plan_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.auto_compute_act_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.complete_pct_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.task_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.duration_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.review_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.task_code)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.task_name)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.total_float_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.free_float_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.remain_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.act_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.target_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.target_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.target_equip_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.remain_equip_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.cstr_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.priority_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.tmpl_guid)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.cstr_type2)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.act_this_per_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.act_this_per_equip_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASK>()
                .Property(e => e.driving_path_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.control_updates_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.PROJCOST)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKRISK)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKACTV)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasOptional(e => e.TASKFDBK)
                .WithRequired(e => e.TASK);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKMEMO)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasOptional(e => e.TASKNOTE)
                .WithRequired(e => e.TASK);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKPRED)
                .WithRequired(e => e.TASK)
                .HasForeignKey(e => e.task_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKPRED1)
                .WithRequired(e => e.TASK1)
                .HasForeignKey(e => e.pred_task_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKPROC)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKRSRC)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TASKWKSP)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASK>()
                .HasMany(e => e.TRSRCFIN)
                .WithRequired(e => e.TASK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASKACTV>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKACTV>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKDOC>()
                .Property(e => e.wp_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASKDOC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKDOC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKFDBK>()
                .Property(e => e.task_fdbk)
                .IsUnicode(false);

            modelBuilder.Entity<TASKFDBK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKFDBK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_work_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_equip_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_mat_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.act_expense_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.bcwp)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.sched_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.bcws)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.perfm_work_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKFIN>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKMEMO>()
                .Property(e => e.task_memo)
                .IsUnicode(false);

            modelBuilder.Entity<TASKMEMO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKMEMO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKNOTE>()
                .Property(e => e.task_notes)
                .IsUnicode(false);

            modelBuilder.Entity<TASKNOTE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKNOTE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPRED>()
                .Property(e => e.pred_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPRED>()
                .Property(e => e.lag_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKPRED>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPRED>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.complete_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.proc_name)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.proc_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.complete_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.proc_descr)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKPROC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRISK>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRISK>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.rsrc_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.cost_qty_link_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.pend_complete_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.pend_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.target_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.remain_qty_per_hr)
                .HasPrecision(16, 8);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.pend_act_reg_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.target_lag_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.target_qty_per_hr)
                .HasPrecision(16, 8);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_ot_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.pend_act_ot_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_reg_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.relag_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.ot_factor)
                .HasPrecision(10, 3);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.cost_per_qty)
                .HasPrecision(21, 8);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_reg_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_ot_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.rate_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_this_per_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.act_this_per_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.rollup_dates_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.cost_per_qty_source_type)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.remain_crv)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.target_crv)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.actual_crv)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.rsrc_request_data)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.ts_pend_act_end_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.prior_ts_act_reg_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.prior_ts_act_ot_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKRSRC>()
                .HasMany(e => e.TRSRCFIN)
                .WithRequired(e => e.TASKRSRC)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.total_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.etc_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.total_float_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.bcwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.etc)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.bcws)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.perfm_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.sched_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_this_per_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_this_per_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_this_per_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_this_per_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.base_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.act_this_per_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.spread_data)
                .IsUnicode(false);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKSUM>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.bcwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.perfm_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.etc)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.etc_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.eac)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.eac_work)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.bcws)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.acwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.sched_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.base_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.target_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.late_remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKSUMFIN>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKWKSP>()
                .Property(e => e.cr_external_key)
                .IsUnicode(false);

            modelBuilder.Entity<TASKWKSP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TASKWKSP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.wbs_flag)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.task_flag)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.rsrc_flag)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.thresh_parm_type)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.thresh_parm_name)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.thresh_field_name)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.thresh_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<THRSPARM>()
                .HasMany(e => e.PROJTHRS)
                .WithRequired(e => e.THRSPARM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TIMESHT>()
                .Property(e => e.daily_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TIMESHT>()
                .Property(e => e.status_code)
                .IsUnicode(false);

            modelBuilder.Entity<TIMESHT>()
                .Property(e => e.ts_notes)
                .IsUnicode(false);

            modelBuilder.Entity<TIMESHT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TIMESHT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TIMESHT>()
                .HasMany(e => e.RSRCHOUR)
                .WithRequired(e => e.TIMESHT)
                .HasForeignKey(e => new { e.ts_id, e.rsrc_id })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TMPLCATG>()
                .Property(e => e.catg_name)
                .IsUnicode(false);

            modelBuilder.Entity<TMPLCATG>()
                .Property(e => e.project_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TMPLCATG>()
                .Property(e => e.process_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TMPLCATG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TMPLCATG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.display_type)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.track_view_name)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.web_view_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.track_view_data)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRAKVIEW>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCFIN>()
                .Property(e => e.act_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCFIN>()
                .Property(e => e.act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCFIN>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCFIN>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCSUM>()
                .Property(e => e.spread_data)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCSUM>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCSUM>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_ot_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_reg_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_ot_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.act_reg_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.total_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.target_qty)
                .HasPrecision(19, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.staffed_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.unstaffed_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.staffed_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.unstaffed_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.staffed_late_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.unstaffed_late_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.staffed_late_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.unstaffed_late_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.late_remain_qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.late_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TRSRCSUMFN>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.rsrc_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.rsrc_name)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.ts_status_code)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.reg_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.reg_ot_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.pend_reg_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.pend_reg_ot_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.oh_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.oh_ot_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.pend_oh_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.pend_oh_ot_hrs)
                .HasPrecision(17, 6);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.proj_short_name)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.ts_task_status)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.user_name)
                .IsUnicode(false);

            modelBuilder.Entity<TSAUDIT>()
                .Property(e => e.approving_as)
                .IsUnicode(false);

            modelBuilder.Entity<TSDATES>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TSDATES>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<TSDELEGATE>()
                .Property(e => e.active_flag)
                .IsUnicode(false);

            modelBuilder.Entity<TSDELEGATE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<TSDELEGATE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<UACCESS>()
                .Property(e => e.access_flag)
                .IsUnicode(false);

            modelBuilder.Entity<UDFCODE>()
                .Property(e => e.short_name)
                .IsUnicode(false);

            modelBuilder.Entity<UDFCODE>()
                .Property(e => e.udf_code_name)
                .IsUnicode(false);

            modelBuilder.Entity<UDFCODE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<UDFCODE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.udf_type_name)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.udf_type_label)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.logical_data_type)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.super_flag)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.formula)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.indicator_expression)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.disp_data_flag)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.disp_indicator_flag)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.summary_indicator_expression)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.summary_method)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<UDFTYPE>()
                .HasMany(e => e.UDFVALUE)
                .WithRequired(e => e.UDFTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UDFVALUE>()
                .Property(e => e.udf_text)
                .IsUnicode(false);

            modelBuilder.Entity<UDFVALUE>()
                .Property(e => e.udf_number)
                .HasPrecision(28, 10);

            modelBuilder.Entity<UDFVALUE>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<UDFVALUE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<UDFVALUE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<UEVNTREG>()
                .Property(e => e.app_name)
                .IsUnicode(false);

            modelBuilder.Entity<UEVNTREG>()
                .Property(e => e.operation_name)
                .IsUnicode(false);

            modelBuilder.Entity<UEVNTREG>()
                .Property(e => e.action_name)
                .IsUnicode(false);

            modelBuilder.Entity<UEVNTREG>()
                .Property(e => e.user_name)
                .IsUnicode(false);

            modelBuilder.Entity<UEVNTREG>()
                .Property(e => e.event_reg_data)
                .IsUnicode(false);

            modelBuilder.Entity<UMEASURE>()
                .Property(e => e.unit_name)
                .IsUnicode(false);

            modelBuilder.Entity<UMEASURE>()
                .Property(e => e.unit_abbrev)
                .IsUnicode(false);

            modelBuilder.Entity<UMEASURE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<UMEASURE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.logical_data_type)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.user_col_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.user_col_label)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERCOL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERDATA>()
                .Property(e => e.topic_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERDATA>()
                .Property(e => e.user_data)
                .IsUnicode(false);

            modelBuilder.Entity<USERDATA>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERDATA>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERENG>()
                .Property(e => e.db_engine_type)
                .IsUnicode(false);

            modelBuilder.Entity<USERENG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERENG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USEROBS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USEROBS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USEROPEN>()
                .Property(e => e.user_open_name)
                .IsUnicode(false);

            modelBuilder.Entity<USEROPEN>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USEROPEN>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.global_flag)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.email_type)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.user_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.all_rsrc_access_flag)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.report_user_flag)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.guid)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.email_srv_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.office_phone)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.actual_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.email_send_server)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.email_addr)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.email_srv_passwd)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.passwd)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.notify_prefs)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.override_naviview_flag)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.cr_external_key)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.cr_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.DISCUSSION_READ)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.JOBSVC)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.PCATUSER)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.PREFER)
                .WithOptional(e => e.USERS)
                .HasForeignKey(e => e.ts_approve_user_id);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.PROJECT)
                .WithOptional(e => e.USERS)
                .HasForeignKey(e => e.checkout_user_id);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.TSDELEGATE)
                .WithRequired(e => e.USERS)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.TSDELEGATE1)
                .WithRequired(e => e.USERS1)
                .HasForeignKey(e => e.ts_delegate_user_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.UACCESS)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.USERDATA)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.USERENG)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.USEROBS)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.USEROPEN)
                .WithOptional(e => e.USERS)
                .WillCascadeOnDelete();

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.USERWKSP)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.VIEWPREF)
                .WithOptional(e => e.USERS)
                .WillCascadeOnDelete();

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.VWPREFUSER)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.WKFLUSER)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e._namespace)
                .IsUnicode(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e.setting_name)
                .IsUnicode(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e.setting_value)
                .IsUnicode(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e.userset_blob)
                .IsUnicode(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERSET>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERWKSP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USERWKSP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.host_name)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.app_name)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.hard_drive_code)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.db_engine_type)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.os_user_name)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.vdb_instance_guid)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.operation_name)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.long_operation_flag)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USESSION>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<USROPNVAL>()
                .Property(e => e.usropn_type)
                .IsUnicode(false);

            modelBuilder.Entity<USROPNVAL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<USROPNVAL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPREF>()
                .Property(e => e.view_pref_name)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPREF>()
                .Property(e => e.view_pref_type)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPREF>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPREF>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPROP>()
                .Property(e => e.view_name)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPROP>()
                .Property(e => e.view_type)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPROP>()
                .Property(e => e.view_data)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPROP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<VIEWPROP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDASH>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDASH>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDATA>()
                .Property(e => e.view_pref_key)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDATA>()
                .Property(e => e.view_pref_value)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDATA>()
                .Property(e => e.view_pref_value_blob)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDATA>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFDATA>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<VWPREFUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBRSCAT>()
                .Property(e => e.wbrs_cat_name)
                .IsUnicode(false);

            modelBuilder.Entity<WBRSCAT>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBRSCAT>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSBUDG>()
                .Property(e => e.spend_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<WBSBUDG>()
                .Property(e => e.benefit_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<WBSBUDG>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSBUDG>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSMEMO>()
                .Property(e => e.wbs_memo)
                .IsUnicode(false);

            modelBuilder.Entity<WBSMEMO>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSMEMO>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.committed_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.auto_compute_dates_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.rsrc_request_data)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.allocation_pct)
                .HasPrecision(10, 2);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC_QTY>()
                .Property(e => e.qty)
                .HasPrecision(17, 6);

            modelBuilder.Entity<WBSRSRC_QTY>()
                .Property(e => e.fin_qty1)
                .HasPrecision(17, 6);

            modelBuilder.Entity<WBSRSRC_QTY>()
                .Property(e => e.fin_qty2)
                .HasPrecision(17, 6);

            modelBuilder.Entity<WBSRSRC_QTY>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSRSRC_QTY>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSSTEP>()
                .Property(e => e.complete_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WBSSTEP>()
                .Property(e => e.step_name)
                .IsUnicode(false);

            modelBuilder.Entity<WBSSTEP>()
                .Property(e => e.step_wt)
                .HasPrecision(10, 2);

            modelBuilder.Entity<WBSSTEP>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WBSSTEP>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLTMPL>()
                .Property(e => e.template_name)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLTMPL>()
                .Property(e => e.project_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLTMPL>()
                .Property(e => e.wk_external_key)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLTMPL>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLTMPL>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLUSER>()
                .Property(e => e.comments)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLUSER>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WKFLUSER>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.workflow_name)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.external_key)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.existing_project_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.stage_name)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.stage_modified_flag)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WORKFLOW>()
                .HasMany(e => e.WKFLUSER)
                .WithRequired(e => e.WORKFLOW)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.workspace_type)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.workspace_name)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.workspace_prefs)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.cr_external_key)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.update_user)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .Property(e => e.create_user)
                .IsUnicode(false);

            modelBuilder.Entity<WORKSPACE>()
                .HasMany(e => e.TASKWKSP)
                .WithRequired(e => e.WORKSPACE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WORKSPACE>()
                .HasMany(e => e.USERWKSP)
                .WithRequired(e => e.WORKSPACE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REFRDEL>()
                .Property(e => e.table_name)
                .IsUnicode(false);

            modelBuilder.Entity<REFRDEL>()
                .Property(e => e.pk1)
                .IsUnicode(false);

            modelBuilder.Entity<REFRDEL>()
                .Property(e => e.pk2)
                .IsUnicode(false);

            modelBuilder.Entity<REFRDEL>()
                .Property(e => e.pk3)
                .IsUnicode(false);

            modelBuilder.Entity<REFRDEL>()
                .Property(e => e.pk4)
                .IsUnicode(false);

            modelBuilder.Entity<SUMPROJCOST>()
                .Property(e => e.spread_type)
                .IsUnicode(false);

            modelBuilder.Entity<SUMPROJCOST>()
                .Property(e => e.act_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMPROJCOST>()
                .Property(e => e.remain_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMPROJCOST>()
                .Property(e => e.target_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMPROJCOST>()
                .Property(e => e.total_cost)
                .HasPrecision(23, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.bcwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.bcws)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.eac)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.eac_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.etc)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.etc_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.perfm_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.sched_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_float_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_this_per_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_this_per_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_this_per_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_this_per_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_this_per_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.act_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.base_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.total_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASK>()
                .Property(e => e.target_drtn_hr_cnt)
                .HasPrecision(17, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.spread_type)
                .IsUnicode(false);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.act_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.base_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.late_remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.remain_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.target_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.total_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.bcwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.bcws)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.eac)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.eac_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.etc)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.etc_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.perfm_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.sched_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_work_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_equip_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_equip_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_mat_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_expense_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_total_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_act_cost)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_bcwp)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_perfm_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_eac)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_eac_work_qty)
                .HasPrecision(22, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_bcws)
                .HasPrecision(25, 6);

            modelBuilder.Entity<SUMTASKSPREAD>()
                .Property(e => e.fin_period_sched_work_qty)
                .HasPrecision(22, 6);
        }
    }
}
