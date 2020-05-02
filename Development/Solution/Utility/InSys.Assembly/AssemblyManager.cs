using InSys.Storage;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using z.Data;

namespace InSys.Assembly
{
    public static class AssemblyManager
    {
        public static Assembly.Resolver Resolver = new Assembly.Resolver();

        public static List<Type> ModuleControllers { get; set; } = new List<Type>();

        public static void Initiate()
        {
            try
            {
                var hst = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Debug.WriteLine($"InSys Version: { hst }");

                using (var strge = new Storage.Storage())
                {
                    var container = strge.Container("Base/Module");

                    Resolver.Prepare();

                    Debug.WriteLine($"Loading Assemblies");

                    //Loading System Default Module
                    var items = strge.ListBlob(container);
                    foreach (var mItem in items)
                    {
                        if (strge.Exists(container, mItem.Name.ToString()))
                        {
                            var Ass = Resolver.LoadDLLStream(mItem.Name, strge.DownloadBytes(container, mItem.Name.ToString()));

                            Debug.WriteLine($"Assembly Loaded: { mItem.Name.ToString().Replace("dll", "") }");
                        }
                        else
                        {
                            Debug.WriteLine($"Assembly not found: { mItem.Name.ToString().Replace("dll", "") }");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: { ex.Message.ToString() }");
            }
        }

        public static void RegisterController(IMvcBuilder builder)
        {
            foreach (var ass in Resolver.Assemblies)
            {
                var nType = ass.Types.Where(x => x.Name.Contains("Controller"));

                if (nType.Any())
                {
                    RegisterController(builder.PartManager, ass.Assembly);
                    nType.Each(x => Debug.WriteLine($"Controller Registered: { x.Name }"));
                }
            }
        }
        public static void RegisterLogin(IMvcBuilder builder)
        {
            string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var Asm = System.Reflection.Assembly.LoadFrom(Path.Combine(assemblyFolder, $"InSys.ITI.Login.dll"));
            var nType = Asm.GetTypes().Where(x => x.Name.Contains("Controller"));

            if (nType.Any())
            {
                RegisterController(builder.PartManager, Asm);
                nType.Each(x => Debug.WriteLine($"Controller Registered: { x.Name }"));
            }
        }

        public static void ReloadController(IMvcBuilder builder)
        {
            foreach (var ass in Resolver.Assemblies)
            {
                var nType = ass.Types.Where(x => x.Name.Contains("Controller"));

                if (nType.Any())
                {
                    ReloadController(builder.PartManager, ass.Assembly);
                    nType.Each(x => Debug.WriteLine($"Controller Registered: { x.Name }"));
                }
            }
        }

        public static void RegisterController(ApplicationPartManager partManager, System.Reflection.Assembly Assembly)
        {
            var part = new AssemblyPart(Assembly);
            Debug.WriteLine(part.Name);
            var gg = partManager.ApplicationParts.Where(x => x.Name == part.Name);
            if (gg.Any())
            {
                Debug.WriteLine($"Controller {part.Name} already exist.");
                //partManager.ApplicationParts.Remove(gg.Single());
            }
            partManager.ApplicationParts.Add(part);
            InSysActionDescriptorChangeProvider.Instance.HasChanged = true;
            InSysActionDescriptorChangeProvider.Instance?.TokenSource?.Cancel();
        }
        public static void ReloadController(ApplicationPartManager partManager, System.Reflection.Assembly Assembly)
        {
            var part = new AssemblyPart(Assembly);
            Debug.WriteLine(part.Name);
            var gg = partManager.ApplicationParts.Where(x => x.Name == part.Name);
            if (gg.Any())
                partManager.ApplicationParts.Remove(gg.Single());

            partManager.ApplicationParts.Add(part);
            InSysActionDescriptorChangeProvider.Instance.HasChanged = true;
            InSysActionDescriptorChangeProvider.Instance?.TokenSource?.Cancel();
        }

        public static void ReloadAssembly(string ModuleName, ApplicationPartManager partManager)
        {
            throw new NotImplementedException("must restart the web to implement changes");
        }

        private static void LoadSystemInstance(Storage.Storage strge, IStorageContainer container, string SysAss)
        {
            System.Reflection.Assembly CurrentAssembly;
            using (var ms = new MemoryStream())
            {
                strge.DownloadToStream(container, SysAss, ms);
                ms.Seek(0, SeekOrigin.Begin);
                CurrentAssembly = Resolver.LoadDLLStream(SysAss, ms.ToArray()).Item1;
            }

            Debug.WriteLine($"System Assembly Loaded: { SysAss }");
        }
        public static T CreateInstance<T>(string Name) where T : class => Resolver.CreateClass<T>(Name);

        public static List<AssemblyFile> GetFileFromAssembly()
        {
            List<AssemblyFile> filesToCopy = new List<AssemblyFile>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(".ITI."));
            foreach (var assembly in assemblies)
            {
                if (assembly.GetManifestResourceNames().Count(x => x.Contains(".wwwroot.")) > 0)
                {
                    foreach (string filePath in assembly.GetManifestResourceNames().Where(x => x.Contains(".wwwroot.")).ToArray())
                    {
                        var asmFile = new AssemblyFile();
                        using (var strm = assembly.GetManifestResourceStream(filePath))
                        {
                            using (var reader = new StreamReader(strm))
                            {
                                asmFile.Content = reader.ReadToEnd();
                            }
                        }
                        string path = filePath.Replace(assembly.FullName.Split(",")[0] + ".wwwroot.", "");
                        int idx = Array.IndexOf(path.Split("."), "Files");
                        var folders = path.Split(".", idx + 2);

                        asmFile.FileDirectory = string.Join("/", folders.Take(folders.Count() - 1).ToArray()).Replace("Files", "");
                        asmFile.FileName = folders[folders.Count() - 1];
                        filesToCopy.Add(asmFile);
                    }
                }
            }
            return filesToCopy;
        }
        public static void CreateFilesFromAssembly(string WebRootPath)
        {
            var filesToCopy = GetFileFromAssembly();
            foreach (var files in filesToCopy)
            {
                if (!Directory.Exists(Path.Combine(WebRootPath, files.FileDirectory)))
                    Directory.CreateDirectory(Path.Combine(WebRootPath, files.FileDirectory));
                if (!File.Exists(Path.Combine(Path.Combine(WebRootPath, files.FileDirectory), files.FileName)))
                {
                    byte[] content = Encoding.ASCII.GetBytes(files.Content);
                    using (var fs = new FileStream(Path.Combine(Path.Combine(WebRootPath, files.FileDirectory), files.FileName), FileMode.OpenOrCreate))
                        fs.Write(content, 0, content.Length - 1);
                }
            }
        }
        public class AssemblyFile
        {
            public string FileDirectory { get; set; }
            public string FileName { get; set; }
            public string Content { get; set; }
        }
    }
}
