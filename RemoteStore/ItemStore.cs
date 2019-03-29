using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net;
using eWayCRM.API;
using eWayCRM.API.Exceptions;
using Newtonsoft.Json.Linq;

namespace TimeClock.RemoteStore
{
    public class ItemStore
    {
        static readonly ItemStore instance;
        
        Connection connection;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ItemStore()
        {
            instance = new ItemStore();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ItemStore Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public BaseItem User
        {
            get;
            private set;
        }

        /// <summary>
        /// Logs into the WebService.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public bool LogIn(string server, string userName, string password, string version)
        {
            try
            {
                this.connection = new Connection(server, userName, password, version);

                JObject users = this.connection.CallMethod("SearchUsers", JObject.FromObject(new
                {
                    transmitObject = new
                    {
                        Username = userName
                    }
                }));

                var userItem = ((JArray)users["Data"]).Single();
                this.User = new BaseItem("Users", new Guid(userItem.Value<string>("ItemGUID")), userItem.Value<string>("FileAs"));
            }
            catch (LoginException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets additional fields.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAdditionalFields()
        {
            JObject additionalFields = this.connection.CallMethod("GetAdditionalFields");

            return ((JArray)additionalFields["Data"])
                .Where(x => x.Value<int>("Type") == 0 && x.Value<string>("ObjectTypeFolderName") == "WorkReports")
                .ToDictionary(x => "af_" + x.Value<int>("FieldId").ToString(), y => y.Value<string>("Name"));
        }

        /// <summary>
        /// Gets work report types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseItem> GetWorkReportTypes()
        {
            JObject enumValues = this.connection.CallMethod("GetEnumValues");

            return ((JArray)enumValues["Data"])
                .Where(x => x.Value<string>("EnumTypeName") == "WorkReportType")
                .Select(x => new BaseItem("EnumValues", new Guid(x.Value<string>("ItemGUID")), x.Value<string>("En")));
        }

        public IEnumerable<BaseItem> GetProjectsLeads()
        {
            var projects = this.GetItems("Projects");
            var leads = this.GetItems("Leads");

            var companyGuids = projects.Where(x => x.Value<string>("Companies_CustomerGuid") != null).Select(x => new Guid(x.Value<string>("Companies_CustomerGuid")))
                .Union(leads.Where(x => x.Value<string>("Companies_CustomerGuid") != null).Select(x => new Guid(x.Value<string>("Companies_CustomerGuid"))));

            var contactGuids = projects.Where(x => x.Value<string>("Contacts_ContactPersonGuid") != null).Select(x => new Guid(x.Value<string>("Contacts_ContactPersonGuid")))
                .Union(leads.Where(x => x.Value<string>("Contacts_ContactPersonGuid") != null).Select(x => new Guid(x.Value<string>("Contacts_ContactPersonGuid"))));

            var companies = this.GetItemGuidFileAsDictionary("Companies", companyGuids);
            var contacts = this.GetItemGuidFileAsDictionary("Contacts", contactGuids);

            return projects.Select(x => new BaseItem("Projects", new Guid(x.Value<string>("ItemGUID")), this.GetProjectFileAs(x, companies, contacts)))
                .Concat(leads.Select(x => new BaseItem("Leads", new Guid(x.Value<string>("ItemGUID")), this.GetProjectFileAs(x, companies, contacts))))
                .OrderBy(x => x.FileAs);
        }

        public void SaveWorkReport(WorkReport item, KeyValuePair<string, string>? additionalField)
        {
            JObject additionalFields = new JObject();
            if (additionalField != null)
            {
                additionalFields.Add(additionalField.Value.Key, item.ReservedField);
            }

            this.connection.CallMethod("SaveWorkReport", JObject.FromObject(new
            {
                transmitObject = JObject.FromObject(new
                {
                    ItemGUID = item.ItemGuid,
                    ItemVersion = 1,
                    Subject = item.Subject,
                    From = item.FromTime.ToStringForApi(),
                    To = item.ToTime.ToStringForApi(),
                    TypeEn = item.Type,
                    Projects_ProjectGuid = (item.ProjectItem.FolderName ?? "Projects") == "Projects" ? (Guid?)item.Project : null,
                    Leads_LeadGuid = item.ProjectItem.FolderName == "Leads" ? (Guid?)item.Project : null,
                    Users_PersonGuid = item.UserItem.ItemGuid,
                    Note = item.Note,
                    AdditionalFields = additionalFields
                }),
                dieOnItemConflict = false
            }));
        }

        private JArray GetItems(string folderName)
        {
            JObject items = this.connection.CallMethod($"Get{folderName}");
            var itemGuids = ((JArray)items["Data"])
                .Where(x => !x.Value<bool>("IsLost") && !x.Value<bool>("IsCompleted"))
                .Select(x => new Guid(x.Value<string>("ItemGUID")));

            return this.connection.GetItemsByItemGuids($"Get{folderName}ByItemGuids", itemGuids, true);
        }

        private string GetProjectFileAs(JToken project, Dictionary<string, string> companies, Dictionary<string, string> contacts)
        {
            string companyGuid = project.Value<string>("Companies_CustomerGuid");
            string contactGuid = project.Value<string>("Contacts_ContactPersonGuid");
            string projectFileAs = project.Value<string>("FileAs");

            string companyFileAs = project.Value<string>("Customer");
            string contactFileAs = project.Value<string>("ContactPerson");
            if (!string.IsNullOrEmpty(companyFileAs) || (!string.IsNullOrEmpty(companyGuid) && companies.TryGetValue(companyGuid, out companyFileAs)))
            {
                return $"{companyFileAs}, {projectFileAs}";
            }
            else if (!string.IsNullOrEmpty(contactFileAs) || (!string.IsNullOrEmpty(contactGuid) && contacts.TryGetValue(contactGuid, out contactFileAs)))
            {
                return $"{contactFileAs}, {projectFileAs}";
            }

            return projectFileAs;
        }

        private Dictionary<string, string> GetItemGuidFileAsDictionary(string folderName, IEnumerable<Guid> guids)
        {
            var companies = this.connection.GetItemsByItemGuids($"Get{folderName}ByItemGuids", guids);

            return companies.ToDictionary(x => x.Value<string>("ItemGUID"), y => y.Value<string>("FileAs"));
        }
    }
}
