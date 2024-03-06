using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock.RemoteStore
{
    [Serializable]
    public class AdditionalField : BaseItem
    {
        private AdditionalField() { }

        public AdditionalField(string columnName, string name, string editMask)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            this.ColumnName = columnName;
            this.Name = name;
            this.EditMask = editMask;
        }

        public string ColumnName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string EditMask
        {
            get;
            set;
        }
    }
}
