using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InSys.Storage
{
    public class Storage : IStorage, IDisposable
    {
        private CloudStorage AzureStorage;
        private LocalStorage InSysStorage;
        private bool IsCloud;

        public Storage()
        {
            IsCloud = StorageSetting.IsCloudPath;
            var g = StorageSetting.StoragePath;

            if (g == null)
                throw new Exception("Storage path is not specified. Use StorageSetting to Initialize");

            if (IsCloud)
                AzureStorage = new CloudStorage(g);
            else
                InSysStorage = new LocalStorage(g);
        }

        public IStorageContainer Container(string Name)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    return LocalContainer.Create(AzureStorage.Container(Name));
                else
                    return LocalContainer.Create(AzureStorage.Container(StorageSetting.RootContainer), Name);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    return LocalContainer.Create(Name);
                else
                    return LocalContainer.Create(StorageSetting.RootContainer, Name);
            }
        }

        public void CreateContainer(string Name)
        {
            if (IsCloud)
                AzureStorage.CreateContainer(Name).Wait();
            else
                InSysStorage.CreateContainer(Name);
        }

        public IEnumerable<LocalBlob> ListBlob(IStorageContainer Cntr)
        {
            if (IsCloud)
            {
                var items = default(IEnumerable<IListBlobItem>);
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    items = AzureStorage.ListBlob(Cntr.Container);
                else
                    items = AzureStorage.ListBlob(Cntr.SubContainer);

                return items.Where(x => x is CloudBlockBlob).Select(x => new LocalBlob(x as CloudBlockBlob));
            }
            else
                return InSysStorage.ListBlob(Cntr);
        }

        public IEnumerable<LocalBlob> ListBlob(IStorageDirectory Dr)
        {
            if (IsCloud)
                return AzureStorage.ListBlob(Dr.Directory).Select(x => new LocalBlob(x as CloudBlockBlob));
            else
                return InSysStorage.ListBlob(Dr);
        }

        public string DownloadString(IStorageContainer Cntr, string str)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    return AzureStorage.DownloadString(Cntr.Container, str).Result;
                else
                    return AzureStorage.DownloadString(Cntr.SubContainer, str).Result;
            }
            else
            {
                return InSysStorage.DownloadString(Cntr, str);
            }
        }

        public void DownloadToStream(IStorageContainer Cntr, string Name, Stream Target)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    AzureStorage.DownloadToStream(Cntr.Container, Name, Target);
                else
                    AzureStorage.DownloadToStream(Cntr.SubContainer, Name, Target);
            }
            else
            {
                InSysStorage.DownloadToStream(Cntr, Name, Target);
            }
        }

        public void DownloadToStream(IStorageDirectory directory, string Name, Stream Target)
        {
            if (IsCloud)
            {
                AzureStorage.DownloadToStream(directory.Directory, Name, Target);
            }
            else
            {
                InSysStorage.DownloadToStream(directory, Name, Target);
            }
        }

        public byte[] DownloadBytes(IStorageContainer Cntr, string Name)
        {
            using (var stream = new MemoryStream())
            {
                DownloadToStream(Cntr, Name, stream);
                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }

        public void Dispose()
        {
            AzureStorage?.Dispose();
            InSysStorage?.Dispose();
        }

        public void Delete(IStorageContainer cntr, string str)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    AzureStorage.Delete(cntr.Container, str);
                else
                    AzureStorage.Delete(cntr.SubContainer, str);
            }
            else
            {
                InSysStorage.Delete(cntr, str);
            }
        }

        public void Delete(IStorageDirectory cntr, string str)
        {
            if (IsCloud)
                AzureStorage.Delete(cntr.Directory, str);
            else
                InSysStorage.Delete(cntr, str);
        }

        public void Upload(IStorageContainer cntr, string filename, Stream fp)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    AzureStorage.Upload(cntr.Container, filename, fp).Wait();
                else
                    AzureStorage.Upload(cntr.SubContainer, filename, fp).Wait();
            }
            else
            {
                InSysStorage.Upload(cntr, filename, fp);
            }
        }

        public void Upload(IStorageContainer cntr, string filename, string base64)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    AzureStorage.Upload(cntr.Container, filename, base64);
                else
                    AzureStorage.Upload(cntr.SubContainer, filename, base64);
            }
            else
                InSysStorage.Upload(cntr, filename, base64);
        }

        public void Upload(IStorageDirectory directory, string filename, string base64)
        {
            if (IsCloud)
                AzureStorage.Upload(directory.Directory, filename, base64);
            else
                InSysStorage.Upload(directory, filename, base64);
        }

        public void Upload(IStorageDirectory directory, string filename, Stream fp)
        {
            if (IsCloud)
                AzureStorage.Upload(directory.Directory, filename, fp).Wait();
            else
                InSysStorage.Upload(directory, filename, fp);
        }

        public string ToURLSlug(string s)
        {
            //if (IsCloud)
            return Regex.Replace(s, @"[^a-z0-9/.]+", "-", RegexOptions.IgnoreCase)
              .Trim(new char[] { '-' });
              //.ToLower();
            //else
            //    return s;
        }

        public void DownloadToFile(IStorageContainer cntr, string filename, string fg)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    AzureStorage.DownloadToFile(cntr.Container, filename, fg);
                else
                    AzureStorage.DownloadToFile(cntr.SubContainer, filename, fg);
            }
            else
            {
                InSysStorage.DownloadToFile(cntr, filename, fg);
            }
        }

        public bool Exists(IStorageContainer cntr, string filename)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    return AzureStorage.Exists(cntr.Container, filename);
                else
                    return AzureStorage.Exists(cntr.SubContainer, filename);
            }
            else
                return InSysStorage.Exists(cntr, filename);
        }

        public bool Exists(IStorageDirectory directory, string filename)
        {
            if (IsCloud)
            {
                return AzureStorage.Exists(directory.Directory, filename);
            }
            else
                return InSysStorage.Exists(directory, filename);
        }

        public LocalBlob LoadBlob(IStorageDirectory dr, string filename)
        {
            if (IsCloud)
                return AzureStorage.LoadBlob(dr.Directory, filename);
            else
                return InSysStorage.LoadBlob(dr, filename);
        }

        public LocalBlob LoadBlob(IStorageContainer container, string filename)
        {
            if (IsCloud)
            {
                if (string.IsNullOrWhiteSpace(StorageSetting.RootContainer))
                    return AzureStorage.LoadBlob(container.Container, filename);
                else
                    return AzureStorage.LoadBlob(container.SubContainer, filename);
            }
            else
                return InSysStorage.LoadBlob(container, filename);
        }

        public void Validate()
        {
            if (IsCloud)
                AzureStorage.Validate();
            else
                InSysStorage.Validate();
        }


    }
}
