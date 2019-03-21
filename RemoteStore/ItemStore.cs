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
                this.User = new BaseItem(new Guid(userItem.Value<string>("ItemGUID")), userItem.Value<string>("FileAs"));
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
                .Select(x => new BaseItem(new Guid(x.Value<string>("ItemGUID")), x.Value<string>("En")));
        }

        /// <summary>
        /// Gets list of projects.
        /// </summary>
        public IEnumerable<BaseItem> GetProjects()
        {
            JObject projects = this.connection.CallMethod("GetProjects");

            // TODO: Add customer or contact person before the project name
            return ((JArray)projects["Data"])
                .Where(x => !x.Value<bool>("IsLost") && !x.Value<bool>("IsCompleted"))
                .Select(x => new BaseItem(new Guid(x.Value<string>("ItemGUID")), x.Value<string>("FileAs")));
        }

        /// <summary>
        /// Gets list of leads.
        /// </summary>
        public IEnumerable<BaseItem> GetLeads()
        {
            JObject leads = this.connection.CallMethod("GetLeads");

            // TODO: Add customer or contact person before the project name
            return ((JArray)leads["Data"])
                .Where(x => !x.Value<bool>("IsLost") && !x.Value<bool>("IsCompleted"))
                .Select(x => new BaseItem(new Guid(x.Value<string>("ItemGUID")), x.Value<string>("FileAs")));
        }
    }
}
