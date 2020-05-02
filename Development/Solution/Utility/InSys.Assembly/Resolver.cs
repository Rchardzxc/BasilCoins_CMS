using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InSys.Assembly
{
    /// <summary>
    /// LJ 20180215
    /// Assembly Resolver
    /// </summary>
    ///  
    public class Resolver : IDisposable
    {
        public string BaseDir = "";

        public List<(System.Reflection.Assembly Assembly, Type[] Types, string ModuleName)> Assemblies { get; private set; }

        public Resolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Assemblies = new List<(System.Reflection.Assembly Assembly, Type[] Types, string ModuleName)>();
        }

        public void Prepare()
        {
            Assemblies.Clear();
        }

        /// <summary>
        /// the base path of the original assembly (ie. look in the same dir)
        /// </summary>
        /// <param name="BaseDir"></param>
        public Resolver(string BaseDir) : this()
        {
            this.BaseDir = BaseDir;
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;


            // check for assemblies already loaded
            System.Reflection.Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = args.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(@".\", BaseDir, filename);

            try
            {
                return System.Reflection.Assembly.LoadFrom(asmFile);
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        public (System.Reflection.Assembly, Type[]) LoadDLL(string DLLFile)
        {
            System.Reflection.Assembly asm = null;
            Type[] types = null;

            try
            {
                asm = System.Reflection.Assembly.LoadFrom($"{ DLLFile }.dll");
                types = asm.GetTypes();
            }
            catch (Exception ex)
            {
                var msg = $"Unable to load assembly: {Path.GetFileName(DLLFile)}, possibly outdated binary.";
                throw new Exception(msg, ex);
            }

            if (!Assemblies.Any(x => x.ModuleName == Path.GetFileName(DLLFile)))
                Assemblies.Add((asm, types, Path.GetFileName(DLLFile)));

            return (asm, types);
        }

        public (System.Reflection.Assembly, Type[]) LoadDLLStream(string name, byte[] byteCode)
        {
            System.Reflection.Assembly asm = null;
            Type[] types = null;

            try
            {
                asm = System.Reflection.Assembly.Load(byteCode);
                types = asm.GetTypes();
            }
            catch (Exception ex)
            {
                var msg = $"Unable to load assembly: {name}, possibly outdated binary.";
                throw new Exception(msg, ex);
            }

            if (!Assemblies.Any(x => x.ModuleName == name))
                Assemblies.Add((asm, types, name));

            return (asm, types);
        }

        public (System.Reflection.Assembly Assembly, Type Type, string ModuleName) Load(string Name, Type type)
        {
            foreach (var asses in Assemblies)
            {
                foreach (var x in asses.Types)
                {
                    if (type.IsInterface)
                    {
                        if (x.GetInterfaces().Any(y => y == type))
                        {
                            if (x.Name == Name)
                                return (asses.Assembly, x, asses.ModuleName);
                        }
                    }
                    else
                    {
                        if (x.BaseType.IsGenericType)
                        {
                            if (x.BaseType.GetGenericTypeDefinition() == type)
                            {
                                if (x.Name == Name)
                                    return (asses.Assembly, x, asses.ModuleName);
                            }
                        }
                        if (x.BaseType.IsClass)
                        {
                            if (x.Name == Name)
                                return (asses.Assembly, x, asses.ModuleName);
                        }
                    }
                }
            }

            return default((System.Reflection.Assembly, Type, string));
        }

        public static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            // 1. "typeObj" is a Type object of an interface supported by class B.
            // 2. "criteriaObj" will be a Type object of the base class of B : 
            // i.e. the Type object of class A.
            Type baseClassType = (Type)criteriaObj;
            // Obtain an array of the interfaces supported by the base class A.
            Type[] interfaces_array = baseClassType.GetInterfaces();
            for (int i = 0; i < interfaces_array.Length; i++)
            {
                // If typeObj is an interface supported by the base class, skip it.
                if (typeObj.ToString() == interfaces_array[i].ToString())
                    return false;
            }
            return true;
        }

        public T CreateClass<T>(string Name) where T : class
        {
            var cls = Load(Name, typeof(T));
            if (cls.ModuleName == null)
                throw new Exception($"Specified class { Name } not found in assembly collection");

            return Activator.CreateInstance(cls.Type) as T;
        }

        public void Dispose()
        {
            Assemblies = null;

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
