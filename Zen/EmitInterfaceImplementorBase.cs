using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Zen
{
    /// <summary>
    /// Базовай класс фабрики реализаций
    /// </summary>
    public class EmitInterfaceImplementorBase
    {        
        protected static readonly MethodInfo ResolveMethodInfo;
        protected static readonly AssemblyBuilder AssemblyBuilder;
        protected static readonly ModuleBuilder ModuleBuilder;
        protected static readonly Dictionary<Type,Type> Types=new Dictionary<Type, Type>();
        private static readonly string AsmFileName = "Zen.EmitCache";
        private static bool _saveCache = true;

        static EmitInterfaceImplementorBase()
        {
            if (IsDirty && SaveCache)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => AssemblyBuilder.Save(AsmFileName);
            } 
            
            ResolveMethodInfo = null;
            foreach (var methodInfo in typeof(AppScope).GetMethods())
            {
                if (methodInfo.Name == "Resolve" && methodInfo.IsGenericMethod)
                {
                    ResolveMethodInfo = methodInfo;
                    break;
                }
            }
            var idx = 0;
            foreach (var asmFileName in Directory.EnumerateFiles(".", AsmFileName + "*.dll"))
            {
                idx++;
                try
                {
                    var asm1 = Assembly.LoadFrom(asmFileName);
                    foreach (var type in asm1.GetTypes())
                    {
                        var iface = type.GetInterfaces().FirstOrDefault();
                        if (iface != null) Types[iface] = type;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            AsmFileName = string.Format("{0}.{1}.dll", AsmFileName, idx);
            AssemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(AsmFileName)
                {
                    Version = new Version(1, 0, 0, 0),
                    VersionCompatibility = AssemblyVersionCompatibility.SameMachine,
                }, AssemblyBuilderAccess.RunAndSave);
            //AssemblyBuilder.
            //AssemblyBuilder.DefineVersionInfoResource(AsmFileName, "1.0.0.0", "Zen", "(c) 2013 by FruT", "ZenCore");
            var verAttrType = typeof(AssemblyVersionAttribute); //("1.0.0.0")
            var verAttrCtor = verAttrType.GetConstructor(new[] { typeof(string) });
            var attrBuilder = new CustomAttributeBuilder(verAttrCtor, new object[] { "1.0.0.0" });
            AssemblyBuilder.SetCustomAttribute(attrBuilder);

            var fverAttrType = typeof(AssemblyVersionAttribute); //("1.0.0.0")
            var fverAttrCtor = fverAttrType.GetConstructor(new[] { typeof(string) });
            var fattrBuilder = new CustomAttributeBuilder(fverAttrCtor, new object[] { "1.0.0.0" });
            AssemblyBuilder.SetCustomAttribute(fattrBuilder);

            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("EmitedImplementors", AsmFileName);
        }

        /// <summary>
        /// Сохранить сборку
        /// </summary>
        [Obsolete]
        public static void SaveAssembly()
        {
            if (IsDirty && SaveCache)
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => AssemblyBuilder.Save(AsmFileName);
            }
        }

        /// <summary>
        /// Сохранять сборки кеша на диске
        /// </summary>
        public static bool SaveCache
        {
            get { return _saveCache; }
            set { _saveCache = value; }
        }

        /// <summary>
        /// Произведены изменения сборки которые необходимо сохранить в кеше
        /// </summary>
        protected static bool IsDirty { get; set; }
    }
}