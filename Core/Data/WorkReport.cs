using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using TimeClock.RemoteStore;
using Newtonsoft.Json.Linq;

namespace TimeClock.Core.Data
{
    [Serializable]
    public class WorkReport : RemoteStore.BaseItem
    {
        private string subject;
        private DateTime fromTime;
        private DateTime toTime;

        public WorkReport()
        {
            UserItem = RemoteStore.ItemStore.Instance.User;
        }

        public WorkReport(string subject, Guid project, Guid type,
            DateTime fromTime, DateTime toTime, string note)
            : this()
        {
            this.Subject = subject;
            this.Project = project;
            this.Type = type;
            this.FromTime = fromTime;
            this.ToTime = toTime;
            this.Note = note;
        }

        public DateTime FromTime
        {
            get
            {
                return this.fromTime;
            }

            set
            {
                this.fromTime = this.GetSmallDateTime(value);
            }
        }

        public DateTime ToTime
        {
            get
            {
                return this.toTime;
            }

            set
            {
                this.toTime = this.GetSmallDateTime(value);
            }
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }

            set
            {
                this.subject = (value ?? string.Empty).Trim();
            }
        }

        public string Note
        {
            get;
            set;
        }

        public string ReservedField
        {
            get;
            set;
        }

        [XmlIgnore]
        public Guid Type
        {
            get
            {
                return this.TypeItem == null ? Guid.Empty : this.TypeItem.ItemGuid;
            }

            set
            {
                this.TypeItem = GetBaseItem(value, Items.WorkReportTypes);
            }
        }

        private BaseItem GetBaseItem(Guid itemGuid, IEnumerable<BaseItem> items)
        {
            // Try to find item
            var item = items.Where(x => x.ItemGuid == itemGuid).SingleOrDefault();
            if (item != null)
            {
                return item;
            }
            else
            {
                return new BaseItem(Guid.Empty);
            }
        }

        [XmlElement(ElementName="Type")]
        public RemoteStore.BaseItem TypeItem
        {
            get;
            set;
        }

        [XmlIgnore]
        public Guid Project
        {
            get
            {
                return this.ProjectItem == null ? Guid.Empty : this.ProjectItem.ItemGuid;
            }

            set
            {
                this.ProjectItem = GetBaseItem(value, Items.ProjectsLeads);
            }
        }

        [XmlElement(ElementName="Project")]
        public RemoteStore.BaseItem ProjectItem
        {
            get;
            set;
        }

        [XmlElement(ElementName = "User")]
        public RemoteStore.BaseItem UserItem
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the item can be saved into the remote database.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.TypeItem != null && this.ProjectItem != null;
            }
        }

        /// <summary>
        /// Saves WorkReport into the remote item store.
        /// </summary>
        /// <returns>True if everything went ok</returns>
        public static void SaveWorkReport(WorkReport item)
        {
            // TODO: Add Save logic
        }

        private DateTime GetSmallDateTime(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }
    }
}
