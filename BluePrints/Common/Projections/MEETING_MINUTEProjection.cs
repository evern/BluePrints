using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BluePrints.Common.Projections
{
    public class MEETING_MINUTEProjection
    {
        readonly MEETING MEETING;
        readonly IEnumerable<MINUTE_TITLE> MINUTE_TITLES;
        readonly IEnumerable<MINUTE_AGENDAMasterDetailProjection> Minute_Agendas;
        readonly IEnumerable<MeetingUser> Meeting_Users;
        readonly IEnumerable<MEETING_ACTION> Meeting_Actions;

        public MEETING_MINUTEProjection()
        {

        }

        public MEETING_MINUTEProjection(MEETING meeting, List<MINUTE_TITLE> minute_titles, IEnumerable<MeetingUser> meeting_users, IEnumerable<MINUTE_AGENDAMasterDetailProjection> minute_agendas, IEnumerable<MEETING_ACTION> meetingActions)
        {
            MEETING = meeting;
            MINUTE_TITLES = minute_titles;
            Minute_Agendas = minute_agendas;
            Meeting_Users = meeting_users;
            Meeting_Actions = meetingActions;
        }

        public IEnumerable<MINUTE_TITLEReportingProjection> Minute_Titles
        {
            get
            {
                List<MINUTE_TITLEReportingProjection> reporting_projection = new List<MINUTE_TITLEReportingProjection>();
                foreach(MINUTE_TITLE minute_title in MINUTE_TITLES)
                {
                    MINUTE_TITLEReportingProjection minute_titleProjection = new MINUTE_TITLEReportingProjection(minute_title, Minute_Agendas, Meeting_Users, Meeting_Actions, Meeting_Date);
                    reporting_projection.Add(minute_titleProjection);
                }

                return reporting_projection;
            }
        }

        public string Project_Number
        {
            get
            {
                if (MEETING.PROJECT == null)
                    return string.Empty;

                return MEETING.PROJECT.NUMBER;
            }
        }

        public string Meeting_Name
        {
            get
            {
                if (MEETING.PROJECT == null)
                    return string.Empty;

                return MEETING.PROJECT.NAME + " - " + MEETING.TITLE;
            }
        }

        public string Meeting_Venue => MEETING.VENUE;

        public DateTime Meeting_Date
        {
            get
            {
                return MEETING.MEETING_DATE;
            }
        }

        public DateTime? Meeting_StartTime
        {
            get
            {
                return MEETING.MEETING_START;
            }
        }

        public DateTime? Meeting_EndTime
        {
            get
            {
                return MEETING.MEETING_END;
            }
        }

        public string Meeting_Chaired
        {
            get
            {
                if (Meeting_Users == null || MEETING.CHAIRED_BY == null)
                    return string.Empty;

                MeetingUser meetingUser = Meeting_Users.FirstOrDefault(x => x.Guid == MEETING.CHAIRED_BY);

                return meetingUser == null ? string.Empty: meetingUser.Full_Name;
            }
        }

        public IEnumerable<MeetingUser> Meeting_Attendees
        {
            get
            {
                if (MEETING.Meeting_Attendees == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Attendees;
            }
        }

        private List<MeetingUser> get_empty_meeting_users()
        {
            List<MeetingUser> meeting_users = new List<MeetingUser>();
            MeetingUser empty_user = new MeetingUser() { Full_Name = "-" };
            meeting_users.Add(empty_user);
            return meeting_users;
        }

        public IEnumerable<MeetingUser> Meeting_Apologies
        {
            get
            {
                if (MEETING.Meeting_Apologies == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Apologies;
            }
        }

        public IEnumerable<MeetingUser> Meeting_Distribution
        {
            get
            {
                if (MEETING.Meeting_Distribution == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Distribution;
            }
        }

        public IEnumerable<MeetingUser> Meeting_Signoff
        {
            get
            {
                if (MEETING.Meeting_Signoff == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Signoff;
            }
        }
    }
}
