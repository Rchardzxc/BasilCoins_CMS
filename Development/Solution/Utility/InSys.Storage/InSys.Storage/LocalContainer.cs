using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace InSys.Storage
{
    public class LocalContainer : IStorageContainer
    {

        public CloudBlobContainer Container { get; private set; }

        public string Name { get; private set; }
        public string SubDir { get; private set; }

        public LocalContainer(string Name, string subDir = null)
        {
            this.Name = Name.ToLower();
            this.SubDir = subDir?.ToLower();
        }

        public LocalContainer(CloudBlobContainer container, string subDir = null)
        {
            this.Container = container;
            this.SubDir = subDir?.ToLower();
        }

        public IStorageDirectory GetDirectoryReference(string dirName)
        {
            return new LocalDirectory(this, dirName);
        }

        public static IStorageContainer Create(string Name, string subDir = null)
        {
            return new LocalContainer(Name, subDir);
        }

        public static IStorageContainer Create(CloudBlobContainer container, string subDir = null)
        {
            return new LocalContainer(container, subDir);
        }

        public string GetActualContainerPath
        {
            get
            {
                return string.IsNullOrWhiteSpace(SubDir) ? Name : Path.Combine(Name, SubDir);
            }
        }

        public CloudBlobDirectory SubContainer
        {
            get
            {
                return Container.GetDirectoryReference(SubDir);
            }
        } 
    }
}
