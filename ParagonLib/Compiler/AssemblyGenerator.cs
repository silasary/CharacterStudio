using System;
using System.Reflection;
using ParagonLib;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;
using System.Collections;

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
                    CreatePropGetter("Name", re.Name, typeBuilder);
                    CreatePropGetter("Type", re.Type, typeBuilder);
                    CreatePropGetter("Source", re.Source, typeBuilder);
                    CreatePropGetter("InternalId", re.InternalId, typeBuilder); // Redundant?
                    CreatePropGetter("Category", re.Category, typeBuilder);
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

        private static void CreatePropGetter<T>(string pname, T value, TypeBuilder typeBuilder)
        {
            PropertyBuilder propBuilder = typeBuilder.DefineProperty(pname, PropertyAttributes.None, typeof(string), new Type[0]);
            //propBuilder.SetConstant(value);
            MethodBuilder getBuilder = typeBuilder.DefineMethod("get_" + pname, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName, typeof(T), Type.EmptyTypes);
            Expression<Func<T>> func;
            if (typeof(T) == typeof(string))
                func = FuncRetConst(value);
            else if (typeof(T).IsArray)
                func = FuncRetArray(value);
            else
                throw new NotImplementedException();
            var com = func.Compile();
            func.CompileToMethod(getBuilder);
            propBuilder.SetGetMethod(getBuilder);
        }

        private static Expression<Func<T>> FuncRetArray<T>(T value)
        {
            LabelTarget returnTarget = Expression.Label(typeof(T));
            var v = (value as Array).OfType<object>().Select(e => Expression.Constant(e));
            return Expression.Lambda<Func<T>>(Expression.Block(
                // PDB debug info might want to go here.
                Expression.Label(returnTarget, Expression.NewArrayInit(typeof(T).GetElementType(), v))
                ));
        }

        private static Expression<Func<T>> FuncRetConst<T>(T p)
        {
            LabelTarget returnTarget = Expression.Label(typeof(T));
            return Expression.Lambda<Func<T>>(Expression.Block(
                // PDB debug info might want to go here.
                Expression.Label(returnTarget, Expression.Constant(p, typeof(T)))
                ));
        }
    }
}

