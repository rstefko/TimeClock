using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;

namespace TimeClock.RemoteStore
{
    [Serializable]
    public class BaseItem
    {
        public BaseItem()
        {
            this.ItemGuid = Guid.NewGuid();
        }

        public BaseItem(string fileAs)
            : this()
        {
            this.FileAs = fileAs;
        }

        public BaseItem(Guid itemGuid)
        {
            this.ItemGuid = itemGuid;
        }

        public BaseItem(Guid itemGuid, string fileAs)
        {
            this.ItemGuid = itemGuid;
            this.FileAs = fileAs;
        }

        public Guid ItemGuid
        {
            get;
            set;
        }

        public string FileAs
        {
            get;
            set;
        }
    }
}
