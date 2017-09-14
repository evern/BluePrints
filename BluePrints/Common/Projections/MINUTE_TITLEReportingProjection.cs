using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BluePrints.Common.Projections
{
    //[ConstraintAttributes("Entity.GUID_PARENT, Entity.INTERNAL_NUM")]
    public class MINUTE_TITLEReportingProjection
    {
        readonly MINUTE_TITLE MINUTE_TITLE;
        private List<MINUTE_AGENDAReportingProjection> minute_agendas;

        public MINUTE_TITLEReportingProjection(MINUTE_TITLE minute_title, IEnumerable<MINUTE_AGENDAMasterDetailProjection> all_minute_agendas, IEnumerable<MeetingUser> meetingUsers, IEnumerable<MEETING_ACTION> meetingActions, DateTime meetingDate)
        {
            MINUTE_TITLE = minute_title;
            minute_agendas = new List<MINUTE_AGENDAReportingProjection>();
            IEnumerable<MINUTE_AGENDAMasterDetailProjection> current_minute_agendas = all_minute_agendas.Where(x => x.Entity.GUID_MINUTE_TITLE == MINUTE_TITLE.GUID).Where(x => x.Entity.RAISE_DATE <= meetingDate);
            foreach (MINUTE_AGENDAMasterDetailProjection minute_agenda in current_minute_agendas)
            {
                MINUTE_AGENDAReportingProjection projection = new MINUTE_AGENDAReportingProjection(minute_agenda, meetingUsers, meetingActions, meetingDate);
                minute_agendas.Add(projection);
            }

            //check if any of the comment within the agenda is hide
            List<MINUTE_AGENDAReportingProjection> remove_minute_agendas = new List<MINUTE_AGENDAReportingProjection>();
            foreach(MINUTE_AGENDAReportingProjection minute_agenda in minute_agendas)
            {
                if (minute_agenda.Comments.Any(x => x.Action.IS_HIDE))
                    remove_minute_agendas.Add(minute_agenda);
            }

            //remove elements which action with is hide is true
            foreach(MINUTE_AGENDAReportingProjection remove_minute_agenda in remove_minute_agendas)
            {
                minute_agendas.Remove(remove_minute_agenda);
            }
        }

        public IEnumerable<MINUTE_AGENDAReportingProjection> Minute_Agendas => minute_agendas;

        public string Number => MINUTE_TITLE.DisplayNumber;

        public string Particulars_Title => MINUTE_TITLE.NAME;

        public string Particulars_Comment => string.Empty;

        public int Level
        {
            get
            {
                int i = 0;
                count_parent(i, MINUTE_TITLE.MINUTE_TITLE2);
                return i;
            }
        }

        private void count_parent(int i, MINUTE_TITLE minute_title)
        {
            if (minute_title != null)
            {
                i += 1;
                count_parent(i, minute_title.MINUTE_TITLE2);
            }
        }
    }
}