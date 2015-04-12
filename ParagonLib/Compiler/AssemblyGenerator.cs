using System;
using System.Reflection;
using ParagonLib;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;
using System.Collections;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml;

namespace ParagonLib.Compiler
{
    public static class AssemblyGenerator
    {
        public static Assembly CompileToDll(XDocument doc, string filename = null)
        {
            if (filename == null)
                filename = doc.Root.Attribute("Filename").Value;

            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(filename));
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(name + ".dll", true);
            var generator = DebugInfoGenerator.CreatePdbGenerator();

            foreach (var re in doc.Root.Descendants(XName.Get("RulesElement")))
            {
                List<Expression> calcCode = new List<Expression>(); 
                

                var InternalId = re.Attribute("internal-id").Value.Trim();
                var Parent = typeof(RulesElementBase);
                TypeBuilder typeBuilder = module.DefineType("Rules." + InternalId, TypeAttributes.Public | TypeAttributes.Class, Parent);
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.HasThis | CallingConventions.Standard, Type.EmptyTypes);

                var ctorgen = ctor.GetILGenerator();

                Assign(ctorgen, Builders.RefGetField(typeof(RulesElementBase),"name"), re.Attribute("name").Value.Trim());
                Assign(ctorgen, Builders.RefGetField(typeof(RulesElementBase), "internalId"), InternalId);
                var pthis = Expression.Parameter(typeBuilder, "this");


                foreach (var item in re.Elements())
                {
                    switch (item.Name.LocalName)
                    {
                        case "Category":
                            var cats = item.Value.Split(',').Select(n => n.Trim()).ToArray();
                            break;
                        case "rules":
                            foreach (var rule in item.Elements())
                            {
                                calcCode.Add(Instruction.Generate(rule.Name.LocalName, Builders.MakeDict(rule.Attributes()), filename, ((IXmlLineInfo)rule).LineNumber));
                            }

                            break;
                        default:
                            break;
                    }
                }
                // base..ctor()
                ctorgen.Emit(OpCodes.Ldarg_0);
                ctorgen.Emit(OpCodes.Call, Parent.GetConstructor(Type.EmptyTypes));
                // And done.
                ctorgen.Emit(OpCodes.Ret);


/*                // Expression.Lambda<Action<RulesElementBase>>
                var init = typeBuilder.DefineMethod("Init", MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.Family,
                    CallingConventions.Standard | CallingConventions.HasThis,typeof(void),Type.EmptyTypes);

                var action = typeof(Action<>).MakeGenericType(typeBuilder.CreateTypeInfo());
                LambdaExpression ctorFunc = Expression.Lambda(action, Expression.Block(ctorCode), pthis);

                var c = ctorFunc.Compile();
                ctorFunc.CompileToMethod(init);
                typeBuilder.DefineMethodOverride(init,Builders.RefGetMethod(typeof(RulesElementBase), "Init"));
                //ctorFunc.CompileToMethod(ctor);
 */
                MethodBuilder methodbuilder = 
                    typeBuilder.DefineMethod("Calculate", 
                    MethodAttributes.HideBySig | MethodAttributes.Static | 
                    MethodAttributes.Public |
                    MethodAttributes.VtableLayoutMask, 
                    typeof(void), new Type[] { typeof(CharElement), typeof(Workspace) });
                Builders.Merge(calcCode).CompileToMethod(methodbuilder, generator);
                //typeBuilder.DefineMethodOverride(methodbuilder, Builders.RefGetMethod(typeof(RulesElementBase), "Calculate"));
                typeBuilder.CreateType();
            };
            assemblyBuilder.Save(name + ".dll");

            return assemblyBuilder;
        }

        internal static Assembly CompileToDll(RulesElement[] elements)
        {
            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(elements[0].SourcePart));
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(name + ".dll", true);
            var generator = DebugInfoGenerator.CreatePdbGenerator( );

            foreach (var re in elements)
            {
                try
                {
                    TypeBuilder typeBuilder = module.DefineType("Rules." + re.InternalId, TypeAttributes.Public | TypeAttributes.Class);
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

        private static void Assign(ILGenerator gen, FieldInfo field, string value)
        {
            gen.Emit(OpCodes.Ldarg_0); // Assign the variable on 'this'.
            gen.Emit(OpCodes.Ldstr, value);
            gen.Emit(OpCodes.Stfld, field);
        }

        //private static Expression CreateAssignment<T>(string field, T value)
        //{
        //    if (value.GetType().IsArray)
        //        return Expression.Assign(Expression.Field(Expression.Variable(typeof(RulesElementBase), "this"), Builders.RefGetField(typeof(RulesElementBase), field)), FuncRetArray(value).Body);
        //    else
        //        return Expression.Assign(Expression.Field(Expression.Variable(typeof(RulesElementBase), "this"), Builders.RefGetField(typeof(RulesElementBase), field)), FuncRetConst(value).Body);
        //}
    }
}

