using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using TimeClock.Core.Data.Binding.Adapters;

namespace TimeClock.Core.Data.Binding.Objects
{
    public class EditableWorkReport : EditableAdapter<RemoteStore.WorkReport>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="item"></param>
        public EditableWorkReport(RemoteStore.WorkReport item)
            : base(item)
        {

        }

        public DateTime FromTime
        {
            get
            {
                return (DateTime)((IEditable)this).ReadProperty(this.GetProperty("FromTime"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("FromTime"), value);
            }
        }

        public DateTime ToTime
        {
            get
            {
                return (DateTime)((IEditable)this).ReadProperty(this.GetProperty("ToTime"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("ToTime"), value);
            }
        }

        public string Subject
        {
            get
            {
                return (string)((IEditable)this).ReadProperty(this.GetProperty("Subject"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("Subject"), value);
            }
        }

        public string Note
        {
            get
            {
                return (string)((IEditable)this).ReadProperty(this.GetProperty("Note"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("Note"), value);
            }
        }

        public string ReservedField
        {
            get
            {
                return (string)((IEditable)this).ReadProperty(this.GetProperty("ReservedField"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("ReservedField"), value);
            }
        }

        public Guid Type
        {
            get
            {
                return (Guid)((IEditable)this).ReadProperty(this.GetProperty("Type"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("Type"), value);
            }
        }

        public Guid Project
        {
            get
            {
                return (Guid)((IEditable)this).ReadProperty(this.GetProperty("Project"));
            }

            set
            {
                ((IEditable)this).WriteProperty(this.GetProperty("Project"), value);
            }
        }

        private PropertyInfo GetProperty(string name)
        {
            return this.WrappedInstance.GetType().GetProperty(name);
        }
    }
}
