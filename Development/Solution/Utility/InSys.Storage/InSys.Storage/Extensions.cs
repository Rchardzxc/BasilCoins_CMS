using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSys.Storage
{
    public static class Extensions
    {

        public static IEnumerable<IListBlobItem> ListBlobs(this CloudBlobContainer blobContainer, bool useFlatListing)
        {
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment =
                      blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null, token, null, null).Result;
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    //list.Add(new AzureBlobItem(item));
                    yield return item;
                }
            } while (token != null);
        }

        public static IEnumerable<IListBlobItem> ListBlobs(this CloudBlobDirectory directory, bool useFlatListing)
        {
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment =
                      directory.ListBlobsSegmentedAsync(useFlatListing, new BlobListingDetails(), null, token, null, null).Result;  //("", useFlatListing, new BlobListingDetails(), null, token, null, null).Result;
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    yield return item;
                }
            } while (token != null);
        }

        public static IEnumerable<CloudBlobContainer> ListContainers(this CloudBlobClient client)
        {
            BlobContinuationToken token = null;
            do
            {
                ContainerResultSegment result = client.ListContainersSegmentedAsync(token).Result;

                foreach (var g in result.Results)
                    yield return g;

            } while (token != null);
        }

        public static string CheckDir(this string dr)
        {
            var g = dr;
            if (Path.HasExtension(g))
            {
                g = Path.GetDirectoryName(dr);
            }
            if (!Directory.Exists(g)) Directory.CreateDirectory(g);
            return dr;
        }

        public static string GetBlobContentType(this string FileName, string AlternateExtension = "")
        {
            return new BlobContentType().GetBlobContentType(Path.GetExtension(FileName), AlternateExtension);
        }

        public static string GetBlobContentTypeInExt(this string Extension)
        {
            return new BlobContentType().GetBlobContentType(Extension.ToLower());
        }

        public static string ToUrlSlug(this string Name)
        {
            return new Storage().ToURLSlug(Name);
        }
        
    }
    
}
