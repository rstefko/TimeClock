using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net;
using eWayCRM.API;
using eWayCRM.API.Exceptions;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml;
using TimeClock.RemoteStore.Exceptions;
using eWayCRM.API.EventArgs;

namespace TimeClock.RemoteStore
{
    public class ItemStore
    {
        static readonly ItemStore instance;
        
        Connection connection;

        /// <summary>
        /// Event used to refresh access token.
        /// </summary>
        public event System.EventHandler<AccessTokenEventArgs> RefreshAccessToken;

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
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public bool LogIn(string server, string userName, string password, string version, bool useDefaultCredentials = false, NetworkCredential networkCredential = null,
            string accessToken = null)
        {
            try
            {
                this.connection = new Connection(server, userName, password, version, useDefaultCredentials: useDefaultCredentials, networkCredential: networkCredential, accessToken: accessToken);
                connection.RefreshAccessToken += this.RefreshAccessToken;
                connection.EnsureLogin();

                JObject users = this.connection.CallMethod("SearchUsers", JObject.FromObject(new
                {
                    transmitObject = new
                    {
                        ItemGUID = connection.UserGuid
                    }
                }));

                var userItem = ((JArray)users["Data"]).Single();
                this.User = new BaseItem("Users", new Guid(userItem.Value<string>("ItemGUID")), userItem.Value<string>("FileAs"));
            }
            catch (OAuthRequiredException)
            {
                throw new RaiseOAuthDialogException();
            }
            catch (LoginException ex)
            {
                if (ex.ReturnCode == "rcBadAccessToken")
                {
                    throw new RaiseOAuthDialogException();
                }

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
            IEnumerable<JToken> enumValues;
            if (this.connection.Version > new Version(5, 4, 0, 121))
            {
                enumValues = ((JArray)this.connection.CallMethod("SearchEnumValues", JObject.FromObject(new
                {
                    transmitObject = new
                    {
                        EnumTypeName = "WorkReportType"
                    }
                }))["Data"]);

            }
            else
            {
                enumValues = ((JArray)this.connection.CallMethod("GetEnumValues")["Data"]).Where(x => x.Value<string>("EnumTypeName") == "WorkReportType");
            }

            return enumValues.Select(x => new BaseItem("EnumValues", new Guid(x.Value<string>("ItemGUID")), x.Value<string>("En")));
        }

        public IEnumerable<BaseItem> GetProjectsLeads()
        {
            List<BaseItem> projectsLeads = new List<BaseItem>();
            const int batchSize = 500;
            bool finished = false;
            int skip = 0;

            while (!finished)
            {
                string query = $$"""
                    { "query": {
                    "__type": "MainTableQuery:#EQ",
                    "ItemTypes": ["Projects", "Leads"],
                    "Fields": [
                        {
                            "__type": "Column:#EQ",
                            "Source": {
                                "__type": "MainTable:#EQ"
                            },
                            "Name": "ItemGUID" 
                        },
                        {
                            "__type": "Token:#EQ",
                            "Source": {
                                "__type": "MainTable:#EQ"
                            },
                            "Name": "ItemType"
                        },
                        {
                            "__type": "Column:#EQ",
                            "Source": {
                                "__type": "MainTable:#EQ"
                            },
                            "Name": "FileAs" 
                        },
                        {
                            "__type": "VariatedColumn:#EQ",
                            "Alias": "CONTACTPERSON_FileAs",
                            "Transformation": "ISNULL({0}, '')",
                            "Source": {
                                "__type": "MainTable:#EQ"
                            },
                            "Variations": [
                                {
                                    "__type": "FieldByFolderName:#EQ",
                                    "FolderName": "Leads",
                                    "Field": {
                                        "__type": "SubstituableColumn:#EQ",
                                        "Name": "FileAs",
                                        "Source": {
                                            "__type": "Relation:#EQ",
                                            "ItemTypes": ["Contacts"],
                                            "RelationType": "CONTACTPERSON",
                                            "Direction": 1
                                        },
                                        "Substitute": {
                                            "__type": "Column:#EQ",
                                            "Name": "ContactPerson",
                                            "Source": {
                                                "__type": "MainTable:#EQ"
                                            }
                                        }
                                    }
                                },
                                {
                                    "__type": "FieldByFolderName:#EQ",
                                    "FolderName": "Projects",
                                    "Field": {
                                        "__type": "Column:#EQ",
                                        "Name": "FileAs",
                                        "Source": {
                                            "__type": "Relation:#EQ",
                                            "ItemTypes": ["Contacts"],
                                            "RelationType": "CONTACTPERSON",
                                            "Direction": 1
                                        }
                                    }
                                }
                            ]
                        },
                        {
                            "__type": "VariatedColumn:#EQ",
                            "Alias": "CUSTOMER_FileAs",
                            "Transformation": "ISNULL({0}, '')",
                            "Source": {
                                "__type": "MainTable:#EQ" 
                            },
                            "Variations": [
                                {
                                    "__type": "FieldByFolderName:#EQ",
                                    "FolderName": "Leads",
                                    "Field": {
                                        "__type": "SubstituableColumn:#EQ",
                                        "Name": "FileAs",
                                        "Source": {
                                            "__type": "Relation:#EQ",
                                            "ItemTypes": ["Companies"],
                                            "RelationType": "CUSTOMER",
                                            "Direction": 1
                                        },
                                        "Substitute": {
                                            "__type": "Column:#EQ",
                                            "Name": "Customer",
                                            "Source": {
                                                "__type": "MainTable:#EQ" 
                                            }
                                        }
                                    }
                                },
                                {
                                    "__type": "FieldByFolderName:#EQ",
                                    "FolderName": "Projects",
                                    "Field": {
                                        "__type": "Column:#EQ",
                                        "Name": "FileAs",
                                        "Source": {
                                            "__type": "Relation:#EQ",
                                            "ItemTypes": ["Companies"],
                                            "RelationType": "CUSTOMER",
                                            "Direction": 1
                                        }
                                    }
                                }
                            ]
                        }
                    ],
                    "Filter": {
                        "__type": "AndFilterExpressionOperator:#EQ",
                        "Children": [
                            {
                                "__type": "EqualsFilterExpressionPredicate:#EQ",
                                "Field": {
                                    "__type": "Column:#EQ",
                                    "Source": {
                                        "__type": "MainTable:#EQ"
                                    },
                                    "Name": "CompletedDate"
                                },
                                "Value": null
                            },
                            {
                                "__type": "EqualsFilterExpressionPredicate:#EQ",
                                "Field": {
                                    "__type": "Column:#EQ",
                                    "Source": {
                                        "__type": "MainTable:#EQ"
                                    },
                                    "Name": "LostDate"
                                },
                                "Value": null
                            }
                        ]
                    },
                    "Paging": {
                        "Skip": {{skip}},
                        "Take": {{batchSize}}
                    }
                } }
                """;

                JObject items = this.connection.CallMethod($"Query", JObject.Parse(query));
                var newItems = ((JArray)items["Data"]).Select(x => new BaseItem(x.Value<string>("ItemType"), new Guid(x.Value<string>("ItemGUID")), this.GetProjectFileAs(x)));

                if (newItems.Count() != batchSize)
                {
                    finished = true;
                }

                projectsLeads.AddRange(newItems.Where(x => !string.IsNullOrEmpty(x.FileAs)));
                skip += batchSize;
            }

            return projectsLeads.OrderBy(x => x.FileAs);
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
                    Duration = Math.Round((item.ToTime.Subtract(item.FromTime)).TotalHours, 2),
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

        private string GetProjectFileAs(JToken project)
        {
            string companyFileAs = project.Value<string>("CUSTOMER_FileAs");
            string contactFileAs = project.Value<string>("CONTACTPERSON_FileAs");
            string projectFileAs = project.Value<string>("FileAs");

            if (!string.IsNullOrEmpty(companyFileAs))
                return $"{companyFileAs}, {projectFileAs}";

            if (!string.IsNullOrEmpty(contactFileAs))
                return $"{contactFileAs}, {projectFileAs}";

            return projectFileAs;
        }

        private Dictionary<string, string> GetItemGuidFileAsDictionary(string folderName, IEnumerable<Guid> guids)
        {
            var companies = this.connection.GetItemsByItemGuids($"Get{folderName}ByItemGuids", guids);

            return companies.ToDictionary(x => x.Value<string>("ItemGUID"), y => y.Value<string>("FileAs"));
        }
    }
}
