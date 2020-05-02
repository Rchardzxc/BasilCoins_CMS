using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InSys.Storage
{
    public interface IStorage
    {
        void Validate();
         
        IStorageContainer Container(string Name);
        IEnumerable<LocalBlob> ListBlob(IStorageContainer Cntr);
        IEnumerable<LocalBlob> ListBlob(IStorageDirectory Dr);
        string DownloadString(IStorageContainer Cntr, string str);
        void DownloadToStream(IStorageContainer Cntr, string Name, Stream Target);
        byte[] DownloadBytes(IStorageContainer Cntr, string Name);
        void Delete(IStorageContainer cntr, string str);
        void Upload(IStorageContainer cntr, string filename, Stream fp);
        void Upload(IStorageContainer cntr, string filename, string base64); 
        void Upload(IStorageDirectory cntr, string filename, Stream fp);
        void Upload(IStorageDirectory cntr, string filename, string base64);

        string ToURLSlug(string s);
        void DownloadToFile(IStorageContainer cntr, string filename, string fg);
        bool Exists(IStorageContainer cntr, string filename);
        bool Exists(IStorageDirectory directory, string filename);
    }
}
