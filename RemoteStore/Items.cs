using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;

namespace TimeClock.RemoteStore
{
    public static class Items
    {
        private static object itemsLock = new object();
        private static IEnumerable<BaseItem> _projectsLeads;
        private static IEnumerable<BaseItem> _workReportTypes;
        private static IEnumerable<AdditionalField> _additionalFields;

        /// <summary>
        /// Projects and leads altogether
        /// </summary>
        public static IEnumerable<BaseItem> ProjectsLeads
        {
            get
            {
                if (_projectsLeads != null)
                    return _projectsLeads;

                lock (itemsLock)
                {
                    if (_projectsLeads == null)
                    {
                        _projectsLeads = ItemStore.Instance.GetProjectsLeads();
                    }

                    return _projectsLeads;
                }
            }
        }

        /// <summary>
        /// Gets all types of WorkReport in the system.
        /// </summary>
        public static IEnumerable<BaseItem> WorkReportTypes
        {
            get
            {
                if (_workReportTypes != null)
                    return _workReportTypes;

                lock (itemsLock)
                {
                    if (_workReportTypes == null)
                    {
                        _workReportTypes = ItemStore.Instance.GetWorkReportTypes();
                    }

                    return _workReportTypes;
                }
            }
        }

        /// <summary>
        /// Gets collection of all Textbox AdditionaFields on WorkReport.
        /// </summary>
        private static IEnumerable<AdditionalField> AdditionalFields
        {
            get
            {
                if (_additionalFields != null)
                    return _additionalFields;

                lock (itemsLock)
                {
                    if (_additionalFields == null)
                    {
                        _additionalFields = ItemStore.Instance.GetAdditionalFields();
                    }

                    return _additionalFields;
                }
            }
        }

        /// <summary>
        /// Gets reserved field information.
        /// </summary>
        public static AdditionalField ReservedField
        {
            get
            {
                if (AdditionalFields != null)
                    return AdditionalFields.FirstOrDefault();
                else
                    return null;
            }
        }
    }
}
