using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Storage
{
    public interface IStorageDirectory
    {
        IStorageContainer Container { get; }
        CloudBlobDirectory Directory { get; }
        IStorageDirectory ParentDirectory { get; }

        string Name { get; }
        string Prefix { get; } 
        IStorageDirectory GetDirectoryReference(string dirName);

        string GetActualPath { get; }
    }
}
