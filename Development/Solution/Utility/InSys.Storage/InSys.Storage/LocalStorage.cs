using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InSys.Storage
{
    public class LocalStorage : IDisposable
    {
        public string RootPath { get; private set; }

        public LocalStorage(string path)
        {
            this.RootPath = path;
        }

        public void Validate()
        {
            if (!Directory.Exists(this.RootPath))
                throw new Exception("Local storage directory does not exists");
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public IEnumerable<T> ListBlob<T>(IStorageContainer cntr)
        {
            if (typeof(T) == typeof(LocalBlob))
            {
                return Directory.GetFiles(Path.Combine(RootPath, cntr.GetActualContainerPath)).Select(x => new LocalBlob(cntr, Path.GetFileName(x))).Cast<T>();
            }
            else if (typeof(T) == typeof(LocalDirectory))
            {
                return Directory.GetDirectories(Path.Combine(RootPath, cntr.GetActualContainerPath)).Select(x => new LocalDirectory(cntr, Path.GetDirectoryName(x))).Cast<T>();
            }
            else
                return null;
        }

        public IEnumerable<LocalBlob> ListBlob(IStorageContainer cntr)
        {
            var gh = Path.Combine(RootPath, cntr.GetActualContainerPath);
            return Directory.GetFiles(gh).Select(x => new LocalBlob(cntr, Path.GetFileName(x)));
        }

        public IEnumerable<LocalBlob> ListBlob(IStorageDirectory Dr)
        {
            var gh = Path.Combine(RootPath, Dr.Container.GetActualContainerPath, Dr.Name);
            return Directory.GetFiles(gh).Select(x => new LocalBlob(Dr, Path.GetFileName(x)));
        }

        public void Delete(IStorageContainer cntr, string str)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualContainerPath, str);
            if (File.Exists(gf))
                File.Delete(gf);
        }

        public void Delete(IStorageDirectory cntr, string str)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualPath, str);
            if (File.Exists(gf))
                File.Delete(gf);
        }

        public string DownloadString(IStorageContainer cntr, string str)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualContainerPath, str);
            using (var fs = File.OpenRead(gf))
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return Encoding.ASCII.GetString(ms.ToArray());
                }
            }
        }

        public void DownloadToStream(IStorageContainer cntr, string name, Stream target)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualContainerPath, name);
            using (var fs = File.OpenRead(gf))
            {
                fs.CopyTo(target);
            }
        }

        public void DownloadToStream(IStorageDirectory directory, string name, Stream target)
        {
            var gf = Path.Combine(RootPath, directory.GetActualPath, name);
            using (var fs = File.OpenRead(gf))
            {
                fs.CopyTo(target);
            }
        }


        public void Upload(IStorageContainer cntr, string filename, Stream fp)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualContainerPath, filename).CheckDir();
            using (var fs = File.OpenWrite(gf))
            {
                fp.Seek(0, SeekOrigin.Begin);
                fp.CopyTo(fs);
                fs.Flush();
            }
        }

        public void Upload(IStorageDirectory dir, string filename, Stream fp)
        {
            var gf = Path.Combine(RootPath, dir.Name, filename).CheckDir();
            using (var fs = File.OpenWrite(gf))
            {
                fp.Seek(0, SeekOrigin.Begin);
                fp.CopyTo(fs);
                fs.Flush();
            }
        }

        public void Upload(IStorageDirectory dir, string filename, string base64)
        {
            using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(base64)))
            {
                Upload(dir, filename, ms);
            }
        }

        public void Upload(IStorageContainer cntr, string filename, string base64)
        {
            using (MemoryStream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(base64)))
            {
                var gg = Path.Combine(RootPath, cntr.GetActualContainerPath, filename).CheckDir();
                using (var fs = File.OpenWrite(gg))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fs);
                    fs.Flush();
                }
            }
        }

        public void DownloadToFile(IStorageContainer cntr, string filename, string fg)
        {
            var gf = Path.Combine(RootPath, cntr.GetActualContainerPath, filename);
            using (var fs = File.OpenRead(gf))
            {
                using (var fd = File.OpenWrite(fg))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.CopyTo(fd);
                    fd.Flush();
                }
            }
        }

        public bool Exists(IStorageContainer cntr, string filename)
        {
            return File.Exists(Path.Combine(RootPath, cntr.GetActualContainerPath, filename));
        }

        public bool Exists(IStorageDirectory directory, string filename)
        {
            return File.Exists(Path.Combine(RootPath, directory.Container.GetActualContainerPath, directory.Name));
        }

        public void CreateContainer(string Name)
        {
            Directory.CreateDirectory(Path.Combine(RootPath, Name));
        }

        public LocalBlob LoadBlob(IStorageDirectory dr, string filename)
        { 
            return new LocalBlob(dr, filename);
        }

        public LocalBlob LoadBlob(IStorageContainer container, string filename)
        {
            return new LocalBlob(container, filename);
        }
    }
}
