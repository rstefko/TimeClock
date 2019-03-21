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
        private static IEnumerable<BaseItem> _projects, _leads, _projectsLeads;
        private static IEnumerable<BaseItem> _workReportTypes;
        private static Dictionary<string, string> _additionalFields = new Dictionary<string, string>();

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
                        _projectsLeads = Projects.Concat(Leads);
                    }

                    return _projectsLeads;
                }
            }
        }

        public static IEnumerable<BaseItem> Projects
        {
            get
            {
                if (_projects != null)
                    return _projects;

                lock (itemsLock)
                {
                    if (_projects == null)
                    {
                        // Get projects
                        _projects = ItemStore.Instance.GetProjects();
                    }

                    return _projects;
                }
            }
        }

        public static IEnumerable<BaseItem> Leads
        {
            get
            {
                if (_leads != null)
                    return _leads;

                lock (itemsLock)
                {
                    if (_leads == null)
                    {
                        // Get leads
                        _leads = ItemStore.Instance.GetLeads();
                    }

                    return _leads;
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
        private static Dictionary<string, string> AdditionalFields
        {
            get
            {
                if (_additionalFields.Count != 0)
                    return _additionalFields;

                lock (itemsLock)
                {
                    if (_additionalFields.Count == 0)
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
        public static KeyValuePair<string, string>? ReservedField
        {
            get
            {
                if (AdditionalFields.Count != 0)
                    return AdditionalFields.First();
                else
                    return null;
            }
        }
    }
}
