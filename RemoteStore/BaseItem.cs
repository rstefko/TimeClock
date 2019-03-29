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

        public BaseItem(string folderName, string fileAs)
            : this()
        {
            this.FolderName = folderName;
            this.FileAs = fileAs;
        }

        public BaseItem(string folderName, Guid itemGuid)
        {
            this.FolderName = folderName;
            this.ItemGuid = itemGuid;
        }

        public BaseItem(string folderName, Guid itemGuid, string fileAs)
        {
            this.FolderName = folderName;
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

        public string FolderName
        {
            get;
            set;
        }
    }
}
