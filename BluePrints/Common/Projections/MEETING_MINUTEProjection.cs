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
        readonly IEnumerable<MeetingUser> Meeting_Users;

        public MEETING_MINUTEProjection()
        {

        }

        public MEETING_MINUTEProjection(MEETING meeting, List<MINUTE_TITLE> minute_titles, IEnumerable<MeetingUser> meeting_users, IEnumerable<MINUTE_AGENDAMasterDetailProjection> minute_agendas)
        {
            minute_titles.ForEach(x => x.Set_Minute_Agendas(minute_agendas));
            MEETING = meeting;
            MINUTE_TITLES = minute_titles;
            Meeting_Users = meeting_users;
        }

        public string Report_Project_Number
        {
            get
            {
                if (MEETING.PROJECT == null)
                    return string.Empty;

                return MEETING.PROJECT.NUMBER;
            }
        }

        public string Report_Meeting_Name
        {
            get
            {
                if (MEETING.PROJECT == null)
                    return string.Empty;

                return MEETING.PROJECT.NAME + " - " + MEETING.TITLE;
            }
        }

        public string Report_Meeting_Venue => MEETING.VENUE;

        public DateTime Report_Meeting_Date
        {
            get
            {
                return MEETING.MEETING_DATE;
            }
        }

        public DateTime? Report_Meeting_StartTime
        {
            get
            {
                return MEETING.MEETING_START;
            }
        }

        public DateTime? Report_Meeting_EndTime
        {
            get
            {
                return MEETING.MEETING_END;
            }
        }

        public string Report_Meeting_Chaired
        {
            get
            {
                if (Meeting_Users == null || MEETING.CHAIRED_BY == null)
                    return string.Empty;

                MeetingUser meetingUser = Meeting_Users.FirstOrDefault(x => x.Guid == MEETING.CHAIRED_BY);

                return meetingUser == null ? string.Empty: meetingUser.Full_Name;
            }
        }

        public IEnumerable<string> Report_Meeting_Attendees
        {
            get
            {
                if (MEETING.Meeting_Attendees == null)
                    return new List<string>();

                return MEETING.Meeting_Attendees.Select(x => x.Full_Name);
            }
        }

        private List<MeetingUser> get_empty_meeting_users()
        {
            List<MeetingUser> meeting_users = new List<MeetingUser>();
            MeetingUser empty_user = new MeetingUser() { Full_Name = "-" };
            meeting_users.Add(empty_user);
            return meeting_users;
        }

        public IEnumerable<MeetingUser> Report_Meeting_Apologies
        {
            get
            {
                if (MEETING.Meeting_Apologies == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Apologies;
            }
        }

        public IEnumerable<MeetingUser> Report_Meeting_Distribution
        {
            get
            {
                if (MEETING.Meeting_Distribution == null)
                    return get_empty_meeting_users();

                return MEETING.Meeting_Distribution;
            }
        }

        public IEnumerable<MeetingUser> Report_Meeting_Signoff
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
