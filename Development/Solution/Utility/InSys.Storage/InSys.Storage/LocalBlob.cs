using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InSys.Storage
{
    public class LocalBlob
    {
        private CloudBlob cloudBlockBlob;
        private IStorageDirectory dr;
        private IStorageContainer Cntr;

        private string _Name;

        public LocalBlob(CloudBlob cloudBlockBlob)
        {
            this.cloudBlockBlob = cloudBlockBlob;
            this._Name = cloudBlockBlob.Name;
        }

        public LocalBlob(IStorageDirectory dr, string v)
        {
            this.dr = dr;
            this.Cntr = dr.Container;
            this._Name = Path.Combine(dr.Name, v);
        }

        public LocalBlob(IStorageContainer Cntr, string Name)
        {
            this.Cntr = Cntr;
            this._Name = Name;
        }

        public string Name
        {
            get
            {
                return Path.GetFileName(this._Name);
            }
        }

        public DateTime? ModifiedDate
        {
            get
            {
                if (cloudBlockBlob != null)
                    return cloudBlockBlob.Properties.LastModified?.DateTime;
                else
                    return new FileInfo(this._Name)?.CreationTime;
            }
        }

        public string GetSharedAccessSignature(SharedAccessBlobPolicy policy, SharedAccessBlobHeaders headers)
        {
            if (cloudBlockBlob != null)
                return cloudBlockBlob.GetSharedAccessSignature(policy, headers);
            return $"?v={ DateTime.Now.ToString("yyyyMMdd") }";
        }

        public string AbsoluteUri
        {
            get
            {
                if (cloudBlockBlob != null)
                    return cloudBlockBlob.Uri.AbsoluteUri.Replace(this.Name, this.Name.ToUrlSlug());
                else
                {
                    if (this.dr == null)
                        return Path.Combine(StorageSetting.RequestPath, Cntr.GetActualContainerPath, this.Name).Replace('\\', '/');
                    else
                        return Path.Combine(StorageSetting.RequestPath, dr.GetActualPath, this.Name).Replace('\\', '/');
                }
            }
        }

        public void Delete()
        {
            if (cloudBlockBlob != null)
                cloudBlockBlob.DeleteIfExistsAsync().Wait();
            else
            {
                if (File.Exists(this._Name))
                    File.Delete(this._Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SharedAccessExpiryTime">Minutes</param>
        /// <returns></returns>
        public string GetSharedAccess(int SharedAccessExpiryTime = 10)
        {
            var sasToken = this.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(SharedAccessExpiryTime),
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(SharedAccessExpiryTime * -1)
            }, new SharedAccessBlobHeaders { ContentType = this.Name.GetBlobContentType() });
            return this.AbsoluteUri + sasToken;
        }
    }
}
