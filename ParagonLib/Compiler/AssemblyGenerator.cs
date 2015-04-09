using System;
using System.Reflection;
using ParagonLib;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq.Expressions;

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
                    var pname = "Name";
                    CreatePropGetter(pname, re.Name, typeBuilder);
                    if (re != null && re.Body != null)
                    {
                        MethodBuilder methodbuilder = 
                            typeBuilder.DefineMethod("Calculate", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(CharElement), typeof(Workspace) });
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

        private static void CreatePropGetter(string pname, string value, TypeBuilder typeBuilder)
        {
            PropertyBuilder propBuilder = typeBuilder.DefineProperty(pname, PropertyAttributes.HasDefault, typeof(string), new Type[0]);
            propBuilder.SetConstant(value);
            MethodBuilder getBuilder = typeBuilder.DefineMethod("get" + pname, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Any, typeof(string), Type.EmptyTypes);
            var func = FuncRetConst(value);
            var com = func.Compile();
            func.CompileToMethod(getBuilder);
            propBuilder.SetGetMethod(getBuilder);
        }

        private static Expression<Func<string>> FuncRetConst(string p)
        {
            LabelTarget returnTarget = Expression.Label(typeof(string));
            GotoExpression returnExpression = 
                Expression.Return(returnTarget, Expression.Constant(p, typeof(string)));
            return Expression.Lambda<Func<string>>(Expression.Block(
                returnExpression,
                Expression.Label(returnTarget, Expression.Constant(p, typeof(string)))
                ));
        }
    }
}

