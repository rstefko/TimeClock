using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeClock.Core.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TimeClock.Core.Data.Binding.Objects;

namespace TimeClock.Core.Xml
{
    [Serializable]
	public class History
	{
        [XmlIgnore]
        public static readonly string FILE_NAME = System.IO.Path.Combine(Settings.ApplicationDataPath, "History.xml");

        /// <summary>
        /// Array of work reports.
        /// </summary>
        public WorkReport[] WorkReports;

        public static List<EditableWorkReport> LoadHistory()
        {
            List<EditableWorkReport> editableWorkReports = new List<EditableWorkReport>(5);
            FileStream stream = new FileStream(FILE_NAME, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            
            if (stream != null)
            {
                try
                {
                    XmlTextReader reader = new XmlTextReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(History));

                    History history = (History)serializer.Deserialize(reader);

                    // Create editable objects.
                    foreach (WorkReport workReport in history.WorkReports)
                    {
                        editableWorkReports.Add(new EditableWorkReport(workReport));
                    }
                }
                catch
                {
                    return editableWorkReports;
                }
                finally
                {
                    stream.Close();
                }
            }

            return editableWorkReports;
        }

        public static void SaveHistory(List<EditableWorkReport> editableWorkReports)
        {
            FileStream stream = new FileStream(FILE_NAME, FileMode.Create, FileAccess.Write, FileShare.Write);

            if (stream != null)
            {
                try
                {
                    History history = new History();
                    WorkReport[] workReports = new WorkReport[editableWorkReports.Count];
                    XmlSerializer serializer = new XmlSerializer(typeof(History));

                    for (int i = 0; i < workReports.Length; i++)
                    {
                        workReports[i] = editableWorkReports[i].WrappedInstance;
                    }

                    // Assign work reports to the serializable object.
                    history.WorkReports = workReports;

                    serializer.Serialize(stream, history);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
	}
}
