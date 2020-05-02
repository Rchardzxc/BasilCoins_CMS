using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InSys.Storage
{
    /// <summary>
    /// LJ 20170711
    /// </summary>
    public class CloudStorage : IDisposable
    {
        CloudStorageAccount storageAccount { get; set; }
        CloudBlobClient blobClient { get; set; }

        public CloudStorage(string ConnString)
        {
            storageAccount = CloudStorageAccount.Parse(ConnString);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        public void Validate()
        {
            blobClient.GetRootContainerReference();
        }

        public CloudBlobContainer Container(string Name)
        {
            // Retrieve a reference to a container.
            // CloudBlobContainer container = blobClient.GetContainerReference(ToURLSlug(Name));

            // Create the container if it doesn't already exist.
            //if (!await container.ExistsAsync())
            //    await container.CreateAsync();

            //await container.CreateIfNotExistsAsync();

            //await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // return container;
            return blobClient.GetContainerReference(ToURLSlug(Name));
        }

        public async Task CreateContainer(string Name)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(ToURLSlug(Name));
            await container.CreateAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public CloudBlobDirectory Container(CloudBlobContainer Parent, string Name)
        {
            // Retrieve a reference to a container.
            CloudBlobDirectory container = Parent.GetDirectoryReference(ToURLSlug(Name));

            return container;
        }

        public async Task Upload(CloudBlobContainer container, string Name, Stream fs)
        {
            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(ToURLSlug(Name));
            // Create or overwrite the "myblob" blob with contents from a local file. 
            await blockBlob.UploadFromStreamAsync(fs);
        }

        public void Upload(CloudBlobContainer container, string Name, string Content, int millisecondsTO = -1)
        {
            using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(Content)))
            {
                Upload(container, Name, ms).Wait(millisecondsTO);
            }
        }

        public async Task Upload(CloudBlobDirectory dir, string Name, Stream fs)
        {
            CloudBlockBlob bb = dir.GetBlockBlobReference(ToURLSlug(Name));
            await bb.UploadFromStreamAsync(fs);

        }

        public void Upload(CloudBlobDirectory dir, string Name, string base64, int millisecondsTO = -1)
        {
            using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(base64)))
            {
                Upload(dir, Name, ms).Wait(millisecondsTO);
            }
        }

        public void DownloadToStream(CloudBlobContainer container, string Name, Stream memoryStream, int millisecondsTO = -1)
        {
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(ToURLSlug(Name));
            blockBlob2.DownloadToStreamAsync(memoryStream).Wait(millisecondsTO);
            memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public void DownloadToStream(CloudBlobDirectory container, string Name, Stream memoryStream, int millisecondsTO = -1)
        {
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(ToURLSlug(Name));
            blockBlob2.DownloadToStreamAsync(memoryStream).Wait(millisecondsTO);
            memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public async Task<string> DownloadString(CloudBlobContainer container, string Name)
        {
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(ToURLSlug(Name));

            using (var memoryStream = new MemoryStream())
            {
                await blockBlob2.DownloadToStreamAsync(memoryStream);
                var str = Encoding.ASCII.GetString(memoryStream.ToArray());
                return str;
            }
        }

        public async Task<string> DownloadString(CloudBlobDirectory directory, string Name)
        {
            CloudBlockBlob blockBlob2 = directory.GetBlockBlobReference(ToURLSlug(Name));

            using (var memoryStream = new MemoryStream())
            {
                await blockBlob2.DownloadToStreamAsync(memoryStream);
                var str = Encoding.ASCII.GetString(memoryStream.ToArray());
                return str;
            }
        }

        public void DownloadToFile(CloudBlobContainer container, string Name, string Path, int millisecondsTO = -1)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(ToURLSlug(Name));

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(Path))
            {
                blockBlob.DownloadToStreamAsync(fileStream).Wait(millisecondsTO);
            }
        }

        public void DownloadToFile(CloudBlobDirectory directory, string Name, string Path, int millisecondsTO = -1)
        {
            CloudBlockBlob blockBlob = directory.GetBlockBlobReference(ToURLSlug(Name));

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(Path))
            {
                blockBlob.DownloadToStreamAsync(fileStream).Wait(millisecondsTO);
            }
        }

        public void Delete(CloudBlobContainer container, string Name, int millisecondsTO = -1)
        {
            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(ToURLSlug(Name));

            // Delete the blob.
            blockBlob.DeleteAsync().Wait(millisecondsTO);
        }

        public void Delete(CloudBlobDirectory directory, string Name, int millisecondsTO = -1)
        {
            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = directory.GetBlockBlobReference(ToURLSlug(Name));

            // Delete the blob.
            blockBlob.DeleteAsync().Wait(millisecondsTO);
        }

        public void Delete(CloudBlobContainer container, IListBlobItem item, int millisecondsTO = -1)
        {
            if (item.GetType() == typeof(CloudBlockBlob))
            {
                //CloudBlockBlob blob = (CloudBlockBlob)item;
                //Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri); 
                ((CloudBlockBlob)item).DeleteIfExistsAsync().Wait(millisecondsTO);
            }
            else if (item.GetType() == typeof(CloudPageBlob))
            {
                //CloudPageBlob pageBlob = (CloudPageBlob)item;

                //Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                ((CloudPageBlob)item).DeleteIfExistsAsync().Wait(millisecondsTO);

            }
            else if (item.GetType() == typeof(CloudBlobDirectory))
            {
                //CloudBlobDirectory directory = (CloudBlobDirectory)item;
                throw new Exception("Cloud Blob directory could not delete");
                //Console.WriteLine("Directory: {0}", directory.Uri);
            }
        }

        public IEnumerable<IListBlobItem> ListBlob(CloudBlobContainer container)
        {
            return container.ListBlobs(false);
        }

        public IEnumerable<IListBlobItem> ListBlob(CloudBlobDirectory directory)
        {
            return directory.ListBlobs(false);
        }

        public IEnumerable<T> ListBlob<T>(CloudBlobContainer container)
        {
            return container.ListBlobs(false).Cast<T>();
        }

        public IEnumerable<T> ListBlob<T>(CloudBlobDirectory directory)
        {
            return directory.ListBlobs(false).Cast<T>();
        }

        public LocalBlob LoadBlob(CloudBlobDirectory directory, string filename)
        {
            return new LocalBlob(directory.GetBlobReference(filename));
        }

        public LocalBlob LoadBlob(CloudBlobContainer container, string filename)
        {
            return new LocalBlob(container.GetBlobReference(filename));
        }

        public bool Exists(CloudBlobContainer container, string name)
        {
            return container.GetBlockBlobReference(name).ExistsAsync().Result;
        }

        public bool Exists(CloudBlobDirectory directory, string name)
        {
            return directory.GetBlockBlobReference(name).ExistsAsync().Result;
        }

        public string ToURLSlug(string s)
        {
            return Regex.Replace(s, @"[^a-z0-9/.]+", "-", RegexOptions.IgnoreCase)
              .Trim(new char[] { '-' })
              .ToLower();
        }

        public void Dispose()
        {
            storageAccount = null;
            blobClient = null;

            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public IEnumerable<CloudBlobContainer> ListContainers()
        {
            return blobClient.ListContainers();
        }
    }
}
