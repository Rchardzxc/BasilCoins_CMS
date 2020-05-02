using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Storage
{
    public interface IStorageContainer
    {
        CloudBlobContainer Container { get; }
        //string Name { get; }
        string GetActualContainerPath { get; }
        IStorageDirectory GetDirectoryReference(string filename); 
        CloudBlobDirectory SubContainer { get; }
        string SubDir { get; }
    }
}
