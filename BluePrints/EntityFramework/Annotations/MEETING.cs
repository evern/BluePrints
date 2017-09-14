namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MEETING : BluePrintsEntityBase, IGuidEntityKey, IEntityNumber, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MEETING()
        {
            MEETING_USER = new HashSet<MEETING_USER>();
            MEETING_DATE = DateTime.Now;
            MEETING_START = DateTime.Now;
            MEETING_END = DateTime.Now + TimeSpan.FromHours(1);
        }

        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

        [NotMapped]
        public string EntityNumber
        {
            get { return NUMBER; }
            set { NUMBER = value; }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        private IEnumerable<object> attendees;

        [NotMapped]
        public object Attendees
        {
            get { return attendees; }
            set
            {
                if (value != attendees)
                {
                    attendees = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<MeetingUser> Meeting_Attendees
        {
            get
            {
                if (attendees == null)
                    return null;

                return attendees.Select(x => (MeetingUser)x);
            }
        }

        [NotMapped]
        private IEnumerable<object> apologies;

        [NotMapped]
        public object Apologies
        {
            get { return apologies; }
            set
            {
                if (value != apologies)
                {
                    apologies = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<MeetingUser> Meeting_Apologies
        {
            get
            {
                if (apologies == null)
                    return null;

                return apologies.Select(x => (MeetingUser)x);
            }
        }

        [NotMapped]
        private IEnumerable<object> distribution;

        [NotMapped]
        public object Distribution
        {
            get { return distribution; }
            set
            {
                if (value != distribution)
                {
                    distribution = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<MeetingUser> Meeting_Distribution
        {
            get
            {
                if (distribution == null)
                    return null;

                return distribution.Select(x => (MeetingUser)x);
            }
        }

        [NotMapped]
        private IEnumerable<object> signoff;

        [NotMapped]
        public object Signoff
        {
            get { return signoff; }
            set
            {
                if (value != signoff)
                {
                    signoff = value as IEnumerable<object>;
                }
            }
        }

        [NotMapped]
        public IEnumerable<MeetingUser> Meeting_Signoff
        {
            get
            {
                if (signoff == null)
                    return null;

                return signoff.Select(x => (MeetingUser)x);
            }
        }

        [NotMapped]
        public MeetingUser Meeting_ChairUser { get; set; }

        [NotMapped]
        public string EntityGroup => GUID_MEETING_TYPE.ToString();
    }

    public class MeetingUser
    {
        public Guid Guid { get; set; }
        public string Full_Name { get; set; }
        public string Company_Name { get; set; }
        public string Phone_Number { get; set; }
        public string Email { get; set; }

        public MeetingUserType User_Type { get; set; }

        public override string ToString()
        {
            return Full_Name;
        }
    }
}