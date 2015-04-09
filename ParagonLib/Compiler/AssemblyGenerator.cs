using System;
using System.Reflection;
using ParagonLib;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace ParagonLib.Compiler
{
    internal static class AssemblyGenerator
    {
        internal static Assembly CompileToDll(RulesElement[] elements)
        {
            AssemblyName name = new AssemblyName(System.IO.Path.GetFileNameWithoutExtension(elements[0].SourcePart));
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(name + ".dll", true);
            var generator = DebugInfoGenerator.CreatePdbGenerator( );

            foreach (var re in elements)
            {
                try
                {
                    TypeBuilder typeBuilder = module.DefineType(re.InternalId, TypeAttributes.Public | TypeAttributes.Class);
                    //typeBuilder.AddInterfaceImplementation(typeof(IRulesElement));
//                    PropertyBuilder propBuilder = typeBuilder.DefineProperty("Name", PropertyAttributes.HasDefault, typeof(string), new Type[0]);
//                    propBuilder.SetConstant(re.Name);
                    if (re != null && re.Body != null)
                    {
                        MethodBuilder methodbuilder = typeBuilder.DefineMethod("Calculate", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(CharElement), typeof(Workspace) });
                        re.Body.CompileToMethod(methodbuilder, generator);
                    }
                    typeBuilder.CreateType( );
                }
                catch (ArgumentException c)
                {
                    Logging.Log("Xml Loader", TraceEventType.Error, "{0}: {1} is defined twice within the same .part file.", name + ".part", re.InternalId);
                }
            }
            assemblyBuilder.Save(name + ".dll");
            return assemblyBuilder;
        }
    }
}

