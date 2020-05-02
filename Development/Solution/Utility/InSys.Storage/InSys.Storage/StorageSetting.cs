using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace InSys.Storage
{
    public class StorageSetting
    {
        public static void Init(string storagePath, string rootContainer = "")
        {
            try
            {
                string pattern = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^\/:*?<>""|]*))+)$";
                Regex r = new Regex(pattern); //@"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$"

                StoragePath = storagePath;
                RootContainer = rootContainer;

                if (r.IsMatch(StoragePath))
                    IsCloudPath = false;
                else
                    IsCloudPath = true;
            }
            catch
            {
                IsCloudPath = true;
            }
        }
#if NETCORE || NETCOREAPP
        public static void Init(StorageModel storagePath, bool useCloudPath = false, string rootContainer = "")
        {

            RootContainer = rootContainer;
            IsCloudPath = useCloudPath;

            if (!IsCloudPath)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    StoragePath = storagePath.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    StoragePath = storagePath.OSX;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    StoragePath = storagePath.Linux;
            }
            else
                StoragePath = storagePath.Azure;
        }
#endif
        public static bool IsCloudPath { get; set; }

        public static string StoragePath { get; private set; }

        public static string RootContainer { get; private set; }

        public const string RequestPath = "/InSysStorage"; 
    }


    public class StorageModel
    {
        public string Windows { get; set; }
        public string OSX { get; set; }
        public string Linux { get; set; }
        public string Azure { get; set; }
    } 
}
