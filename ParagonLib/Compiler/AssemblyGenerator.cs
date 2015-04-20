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
using ParagonLib.RuleBases;
using System.Threading.Tasks;

namespace ParagonLib.Compiler
{
    public static class AssemblyGenerator
    {
        static bool IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);
        static AssemblyGenerator()
        {
            Directory.CreateDirectory(Path.Combine(RuleFactory.BaseFolder, "Compiled Rules"));
        }

        public static bool TryLoadDll(out Assembly dll, XDocument doc, string filename = null)
        {
            if (filename == null)
            {
                if (doc.Root.Attribute("Filename") == null)
                    filename = "Unknown";
                else
                    filename = doc.Root.Attribute("Filename").Value;
            }
            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(filename));
            Version ver = new Version();
            if (doc.Root.Element("UpdateInfo") != null && doc.Root.Element("UpdateInfo").Element("Version") != null)
            {
                if (!Version.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value.Trim(), out ver))
                    Version.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value.Trim()+".0", out ver);
            }
            name.Version = ver;

            var savepath = Path.Combine(RuleFactory.BaseFolder, "Compiled Rules", name + ".dll");
            if (File.Exists(name + ".dll"))
            {
                var pdb = Path.ChangeExtension(savepath, IsRunningOnMono ? "mdb" : "pdb");
                if (File.Exists(savepath))
                    File.Delete(savepath);
                if (File.Exists(pdb))
                    File.Delete(pdb);
                File.Move(name + ".dll", savepath);
                if (File.Exists(name + (IsRunningOnMono ? ".mdb" : ".pdb")))
                    File.Move(name + (IsRunningOnMono ? ".mdb" : ".pdb"), pdb);
            }
            if (File.Exists(savepath) && filename != "Unknown")
            {
                if (File.Exists(savepath + ".regen"))
                {
                    try
                    {
                        File.Delete(savepath);
                        File.Delete(savepath + ".regen");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        dll = null;
                        return false;
                    }
                }
                else if (File.GetLastWriteTime(savepath) > File.GetLastWriteTime(filename))
                {
                    var a = Assembly.LoadFile(savepath);
                    var refs = a.GetReferencedAssemblies( );
                    var ParagonLib = typeof(AssemblyGenerator).Assembly.GetName();
                    var ParagonLibRef = refs.FirstOrDefault(n => n.Name == ParagonLib.Name);
                    dll = a;
                    if (ParagonLib.Version != ParagonLibRef.Version)
                        return false;
                    return true;
                }

            }
            dll = null;
            return false;
        }

        public static Assembly CompileToDll(XDocument doc, bool background, string filename = null)
        {
            if (filename == null)
            {
                if (doc.Root.Attribute("Filename") == null)
                    filename = "Unknown";
                else
                    filename = doc.Root.Attribute("Filename").Value;
            }
            var system = doc.Root.Attribute("game-system").Value.Trim();

            AssemblyName name = new AssemblyName(Path.GetFileNameWithoutExtension(filename));
            Version ver = new Version();
            if (doc.Root.Element("UpdateInfo") != null && doc.Root.Element("UpdateInfo").Element("Version") != null)
            {
                if (!Version.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value.Trim(), out ver))
                    Version.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value.Trim()+".0", out ver);
            }
            name.Version = ver;
            var savepath = Path.Combine(RuleFactory.BaseFolder, "Compiled Rules", name + ".dll");
            if (background)
            { }
            else if (File.Exists(savepath) && filename != "Unknown")
            {
                if (File.Exists(savepath + ".regen"))
                {
                    try
                    {
                        File.Delete(savepath);
                        File.Delete(savepath + ".regen");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        background = true;
                    }
                }
                else if (File.GetLastWriteTime(savepath) > File.GetLastWriteTime(filename))
                {
                    var a = Assembly.LoadFile(savepath);
                    var refs = a.GetReferencedAssemblies( );
                    return a;
                }

            }

            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, background ? Environment.CurrentDirectory : Path.Combine(RuleFactory.BaseFolder, "Compiled Rules"));
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(name + ".dll", true);
            var generator = DebugInfoGenerator.CreatePdbGenerator();
#if ASYNC
            List<Task> tasks = new List<Task>();
#endif
            foreach (var re in doc.Root.Descendants(XName.Get("RulesElement")))
            {
#if ASYNC
                tasks.Add(Task.Factory.StartNew(() => { 
#endif
                List<Expression> calcCode = new List<Expression>();


                var InternalId = re.Attribute("internal-id").Value.Trim();
                var ElementType = re.Attribute("type").Value.Trim();
                Type Parent;
                switch (ElementType)
                {
                    case "Background":
                        Parent = typeof(Background);
                        break;
                    case "Level":
                        Parent = typeof(Level);
                        break;
                    case "Race":
                        Parent = typeof(Race);
                        break;
                    case "Power":
                        Parent = typeof(Power);
                        break;
                    default:
                        Parent = typeof(RulesElement);
                        break;
                }
                TypeBuilder typeBuilder;
                try
                {
                    var t = "Rules." + InternalId;
                    if (Parent != typeof(RulesElement))
                        t = "Rules." + Parent.Name + "." + InternalId;
                    typeBuilder = module.DefineType(t, TypeAttributes.Public | TypeAttributes.Class, Parent);
                }
                catch (ArgumentException)
                {
                    Logging.Log("Xml Loader", TraceEventType.Error, "{0}: {1} defined twice in one part. (Line {2})", filename, InternalId, ((IXmlLineInfo)re).LineNumber);
                    continue;
                }
                var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.HasThis | CallingConventions.Standard, Type.EmptyTypes);

                var ctorgen = ctor.GetILGenerator();
                // base..ctor()
                ctorgen.Emit(OpCodes.Ldarg_0);
                // I should probably check the parent for Constructors, because right now we bypass them.
                ctorgen.Emit(OpCodes.Call, typeof(RulesElement).GetConstructor(Type.EmptyTypes));

                Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "name"), re.Attribute("name").Value.Trim());
                Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "internalId"), InternalId);
                Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "type"), ElementType);
                if (re.Attribute("source") != null)
                    Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "source"), re.Attribute("source").Value.Trim());
                Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "system"), system);
                var pthis = Expression.Parameter(typeBuilder, "this");


                foreach (var item in re.Elements())
                {
                    switch (item.Name.LocalName)
                    {
                        case "Category":
                            var cats = item.Value.Split(',').Select(n => n.Trim()).ToArray();
                            Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "category"), cats);
                            break;
                        case "rules":
                            int rulenum = 0;
                            foreach (var rule in item.Elements())
                            {
                                calcCode.Add(Instruction.Generate(rule.Name.LocalName, Builders.MakeDict(rule.Attributes()), filename, ((IXmlLineInfo)rule).LineNumber, rulenum));
                            }

                            break;
                        default:
                            Specific(typeBuilder, Parent, ctorgen, item);
                            break;
                    }
                }

                // And done.
                ctorgen.Emit(OpCodes.Ret);
                MethodBuilder methodbuilder =
                    typeBuilder.DefineMethod("Calculate",
                    MethodAttributes.HideBySig | MethodAttributes.Static |
                    MethodAttributes.Public |
                    MethodAttributes.VtableLayoutMask,
                    typeof(void), new Type[] { typeof(CharElement), typeof(Workspace) });
                Builders.Merge(calcCode).CompileToMethod(methodbuilder, generator);
                //typeBuilder.DefineMethodOverride(methodbuilder, Builders.RefGetMethod(typeof(RulesElementBase), "Calculate"));
                typeBuilder.CreateType();
#if ASYNC
            }));
            }
            Task.WaitAll(tasks.ToArray());
#else
            }
#endif
            try
            {
                assemblyBuilder.Save(name + ".dll");
                Logging.Log("Compiler", TraceEventType.Information, "Generated {0}.dll", name);
            }
            catch(Exception)
            {
                if (!background)
                    throw;
            }
            return assemblyBuilder;
        }

        private static void Specific(TypeBuilder typeBuilder, Type Parent, ILGenerator ctorgen, XElement item)
        {
            bool specific = false;
            var name = item.Name.LocalName;
            var value = item.Value.Trim();
            if (name == "specific")
            {
                name = item.Attribute("name").Value.Trim();
                specific = true;
            }
            var fname = name.Replace(" ", "");
            fname = char.ToLower(fname[0]) + fname.Substring(1);
            if (fname == "type")
                fname = "_type";
            FieldInfo field = null;
            var floc = Parent;
            do
            {
                field = Builders.RefGetField(floc, fname);
                floc = floc.BaseType;
            } while (field == null && floc != typeof(Object));

            if (field == null)
            {
                var warning = new CustomAttributeBuilder(typeof(MissingElementAttribute).GetConstructors().FirstOrDefault(), new object[] { specific, name, value });
                typeBuilder.SetCustomAttribute(warning);
            }
            else if (field.FieldType == typeof(string))
            {
                Assign(ctorgen, field, value);
            }
            else if (field.FieldType == typeof(int))
            {
                Assign(ctorgen, field, Int32.Parse(value));
            }
            else
            {
                var warning = new CustomAttributeBuilder(typeof(MissingElementAttribute).GetConstructors().FirstOrDefault(), new object[] { specific, name, value });
                typeBuilder.SetCustomAttribute(warning);
            }
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
        private static void Assign(ILGenerator gen, FieldInfo field, int value)
        {
            gen.Emit(OpCodes.Ldarg_0); // Assign the variable on 'this'.
            gen.Emit(OpCodes.Ldc_I4, value);
            gen.Emit(OpCodes.Stfld, field);
        }
        private static void Assign(ILGenerator gen, FieldInfo field, string[] value)
        {
            gen.Emit(OpCodes.Ldarg_0); // Assign the variable on 'this'.
            gen.Emit(OpCodes.Ldc_I4, value.Length); // Push Length of Array.
            gen.Emit(OpCodes.Newarr, typeof(string)); // Define Array
            
            gen.Emit(OpCodes.Stfld, field); // Store the array to field.
            // We could, in theory use a local varable, and keep the array in memory.
            // But that can get messy, and would be very hard to keep track of.
            for (int i = 0; i < value.Length; i++)
            {  // this.field[i] = s;
                gen.Emit(OpCodes.Ldarg_0);  
                gen.Emit(OpCodes.Ldfld, field); 
                gen.Emit(OpCodes.Ldc_I4, i);    
                gen.Emit(OpCodes.Ldstr, value[i]); 
                gen.Emit(OpCodes.Stelem_Ref);      
            }
        }
    }
}

