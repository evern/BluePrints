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
    public class MINUTE_AGENDAReportingProjection
    {
        readonly MINUTE_AGENDAMasterDetailProjection MINUTE_AGENDA;
        readonly IEnumerable<MeetingUser> meeting_users;
        readonly IEnumerable<MEETING_ACTION> meeting_actions;
        readonly DateTime meeting_date;

        public MINUTE_AGENDAReportingProjection(MINUTE_AGENDAMasterDetailProjection minute_agenda, IEnumerable<MeetingUser> meetingUsers, IEnumerable<MEETING_ACTION> meetingActions, DateTime meetingDate)
        {
            MINUTE_AGENDA = minute_agenda;
            meeting_users = meetingUsers;
            meeting_date = meetingDate;
            meeting_actions = meetingActions;
            Comments.ToList();
        }

        List<MINUTE_AGENDAReportingProjection> comments;
        public List<MINUTE_AGENDAReportingProjection> Comments
        {
            get
            {
                if(comments == null)
                {
                    comments = new List<MINUTE_AGENDAReportingProjection>();
                    foreach (MINUTE_AGENDAMasterDetailProjection projection in MINUTE_AGENDA.DetailEntities.OrderBy(x => x.Entity.RAISE_DATE))
                    {
                        MINUTE_AGENDAReportingProjection comment_projection = new MINUTE_AGENDAReportingProjection(projection, meeting_users, meeting_actions, meeting_date);
                        comments.Add(comment_projection);
                    }
                }

                return comments;
            }
        }

        public string Number => MINUTE_AGENDA.Entity.NUMBER;

        public string Particulars_Title => MINUTE_AGENDA.Entity.NAME;

        public string Particulars_Comment
        {
            get
            {
                if (MINUTE_AGENDA.Entity.RAISE_DATE == null)
                    return "[] " + MINUTE_AGENDA.Entity.NAME;

                return "[" + ((DateTime)MINUTE_AGENDA.Entity.RAISE_DATE).ToShortDateString() + "] " + MINUTE_AGENDA.Entity.NAME;
            }
        }

        public MEETING_ACTION Action
        {
            get
            {
                return meeting_actions.FirstOrDefault(x => x.GUID == MINUTE_AGENDA.Entity.GUID_ACTION);
            }
        }

        public DateTime? DueDate
        {
            get
            {
                return MINUTE_AGENDA.Entity.DUE_DATE;
            }
        }

        public MeetingUser ActionUser
        {
            get
            {
                return meeting_users.FirstOrDefault(x => x.Guid == MINUTE_AGENDA.Entity.GUID_ACTION_USER);
            }
        }

        public bool IsOnMeetingDate
        {
            get
            {
                return meeting_date == MINUTE_AGENDA.Entity.RAISE_DATE;
            }
        }

        public bool IsHideAction
        {
            get
            {
                if (Comments.Count == 0)
                    return false;

                MINUTE_AGENDAReportingProjection first_comment = Comments.First();
                if (first_comment.Action == null)
                    return false;

                return first_comment.Action.NAME == Action.NAME;
            }
        }

        public bool IsHideDueDate
        {
            get
            {
                if (Comments.Count == 0)
                    return false;

                MINUTE_AGENDAReportingProjection first_comment = Comments.First();
                if (first_comment.DueDate == null)
                    return false;

                return first_comment.DueDate == DueDate;
            }
        }
    }
}