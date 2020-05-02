using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InSys.Storage
{
    public class LocalDirectory : IStorageDirectory
    {
        public IStorageContainer Container { get; private set; }

        public CloudBlobDirectory Directory { get; private set; }

        public IStorageDirectory ParentDirectory { get; private set; }

        public LocalDirectory(IStorageContainer container, string filename)
        {
            this.Container = container;
            this.Name = filename.ToLower();

            if (this.Container.Container != null)
                if (string.IsNullOrWhiteSpace(this.Container.SubDir))
                    this.Directory = this.Container.Container.GetDirectoryReference(this.Name);
                else
                    this.Directory = this.Container.SubContainer.GetDirectoryReference(this.Name);
        }

        public LocalDirectory(IStorageDirectory directory, string filename)
        {
            this.Container = directory.Container;
            this.Name = filename;
            this.ParentDirectory = directory;

            if (directory.Directory != null)
                this.Directory = directory.Directory.GetDirectoryReference(this.Name);
        }

        public string Name { get; internal set; }

        public string Prefix
        {
            get
            {
                if (this.Container.Container != null)
                    return this.Directory.Prefix;
                else
                    return $"{this.Name}\\";
            }
        }

        public string GetActualPath
        {
            get
            {
                if (this.ParentDirectory != null)
                    return Path.Combine(this.ParentDirectory.GetActualPath, this.Name);
                else
                    return Path.Combine(this.Container.GetActualContainerPath, this.Name);
            }
        }

        public IStorageDirectory GetDirectoryReference(string dirName)
        {
            return new LocalDirectory(this, dirName);
        }
    }
}
