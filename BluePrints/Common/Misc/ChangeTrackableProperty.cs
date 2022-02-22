using DevExpress.DataAccess.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public class ChangeTrackableProperty<T> 
    {
        private Func<T> OriginalPropertyFunc;
        private Func<T> ModifiedPropertyFunc;
        private Func<string> OriginalDescriptionFunc;
        private T RunTimeChangeTrackingProperty;
        public ChangeTrackableProperty(Func<T> getOriginalPropertyFunc, Func<T> getModifiedPropertyFunc, Func<string> getOriginalDescriptionFunc)
        {
            OriginalPropertyFunc = getOriginalPropertyFunc;
            OriginalDescriptionFunc = getOriginalDescriptionFunc;
            ModifiedPropertyFunc = getModifiedPropertyFunc;
        }

        public T TrackableProperty
        {
            get
            {
                //run time change tracking property need to be initialised here because it'll be null when placed in constructor
                if (RunTimeChangeTrackingProperty == null)
                    RunTimeChangeTrackingProperty = ModifiedPropertyFunc();

                if (RunTimeChangeTrackingProperty != null)
                    return RunTimeChangeTrackingProperty;

                return OriginalPropertyFunc();
            }
            set
            {
                //prevent IsTransactionFinalised to register false value
                if (!value.Equals(OriginalPropertyFunc()))
                    RunTimeChangeTrackingProperty = value;
            }
        }

        public ChangeTrackablePropertyStatus ChangeTrackingPropertyStatus
        {
            get
            {
                //make sure RunTimeChangeTrackingProperty is invoked before status is returned
                T preloadProperty = TrackableProperty;
                if (RunTimeChangeTrackingProperty == null)
                    return ChangeTrackablePropertyStatus.Original;
                else if (RunTimeChangeTrackingProperty.Equals(OriginalPropertyFunc()))
                    return ChangeTrackablePropertyStatus.Approved;
                else
                    return ChangeTrackablePropertyStatus.Pending;
            }
        }

        public string ChangeTrackingToolTip => ChangeTrackingPropertyStatus != ChangeTrackablePropertyStatus.Original && OriginalDescriptionFunc() != null ? "Previous Value : " + OriginalDescriptionFunc() : null;

        public bool IsPropertyFinalised
        {
            get
            {
                return RunTimeChangeTrackingProperty == null || RunTimeChangeTrackingProperty.Equals(OriginalPropertyFunc());
            }
        }
    }
}
