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
using ParagonLib.Utils;
using ParagonLib.Rules;
using ParagonLib.RuleEngine;

namespace ParagonLib.Compiler
{
    public static class AssemblyGenerator
    {
        static bool IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);
        static FieldInfo _textField = Builders.RefGetField(typeof(RulesElement), "_text");
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
                VersionParser.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value.Trim(), out ver);
            name.Version = ver;

            var savepath = Path.Combine(RuleFactory.BaseFolder, "Compiled Rules", name + ".dll");
            if (File.Exists(name + ".dll"))
            {
                var pdb = Path.ChangeExtension(savepath, IsRunningOnMono ? "mdb" : "pdb");
                if (File.Exists(savepath))
                    try
                    {
                        File.Delete(savepath);
                    }
                catch (UnauthorizedAccessException)
                    {
                        File.Move(savepath, savepath + ".old." + DateTime.Now.Ticks);
                        if (File.Exists(pdb))
                            File.Move(pdb, pdb + ".old." + DateTime.Now.Ticks);
                    }
                if (File.Exists(pdb))
                    File.Delete(pdb);
                try
                {
                    File.Move(name + ".dll", savepath);
                }
                catch (IOException c)
                {   // We're not having any luck here. Just use the existing one.
                    Logging.Crashlog(c);
                    if (File.Exists(savepath))
                    {
                        var a = Assembly.LoadFile(new FileInfo(savepath).FullName);
                        dll = a;
                        return true;
                    }
                }
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
                    try
                    {
                        var a = Assembly.LoadFile(savepath);
                        var refs = a.GetReferencedAssemblies();
                        var ParagonLib = typeof(AssemblyGenerator).Assembly.GetName();
                        var ParagonLibRef = refs.FirstOrDefault(n => n.Name == ParagonLib.Name);
                        dll = a;
                        if (ParagonLib.Version != ParagonLibRef.Version)
                            return false;
                        return true;
                    }
                    catch (BadImageFormatException)
                    {
                        File.Delete(savepath);
                    }
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
            var GameSystem = doc.Root.Attribute("game-system").Value.Trim();

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
            List<TypeBuilder> types = new List<TypeBuilder>();
            List<RuleData> MetadataRules = new List<RuleData>();
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

                var metadata = new RuleData() { 
                    InternalId = InternalId, 
                    Type = ElementType, 
                    Name = re.Attribute("name").Value.Trim(),
                    PartFile = filename,
                    LineNumber = ((IXmlLineInfo)re).LineNumber,
                };
                if (re.Attribute("source") != null)
                    metadata.Source = re.Attribute("source").Value.Trim();

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
                    string t;
                    if (Parent != typeof(RulesElement))
                        t = "Rules." + Parent.Name + "." + InternalId;
                    else
                        t = "Rules." + ElementType + "." + InternalId;
                    typeBuilder = module.DefineType(t, TypeAttributes.Public | TypeAttributes.Class, Parent);
                }
                catch (ArgumentException)
                {
                    Logging.Log("Xml Loader", TraceEventType.Error, "{0}: {1} defined twice in one part. (Line {2})", filename, InternalId, ((IXmlLineInfo)re).LineNumber);
                    continue;
                }
                //ctor
                {
                    var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.HasThis | CallingConventions.Standard, Type.EmptyTypes);

                    var ctorgen = ctor.GetILGenerator();
                    // base..ctor()
                    ctorgen.Emit(OpCodes.Ldarg_0);

                    ConstructorInfo baseCtor = Parent.GetConstructor(Type.EmptyTypes);
                    if (baseCtor == null)
                        baseCtor = typeof(RulesElement).GetConstructor(Type.EmptyTypes);
                    ctorgen.Emit(OpCodes.Call, baseCtor);

                    Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "name"), re.Attribute("name").Value.Trim());
                    Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "internalId"), InternalId);
                    Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "type"), ElementType);
                    if (re.Attribute("source") != null)
                        Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "source"), re.Attribute("source").Value.Trim());
                    Assign(ctorgen, Builders.RefGetField(typeof(RulesElement), "system"), GameSystem);
                
                    var pthis = Expression.Parameter(typeBuilder, "this");

                    int specnum = 0;
                    foreach (var item in re.Elements())
                    {
                        switch (item.Name.LocalName)
                        {
                            case "Category":
                                var cats = item.Value.Split(',').Select(n => n.Trim()).ToArray();
                                metadata.Categories = cats;
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
                                Specific(typeBuilder, Parent, ctorgen, item, specnum++);
                                break;
                        }
                    }
                    // Fluff text.
                    var value = re.Nodes().OfType<XText>().FirstOrDefault();
                    if (value != null)
                        Assign(ctorgen, _textField, re.Nodes().OfType<XText>().FirstOrDefault().Value);
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
                }
                typeBuilder.CreateType();
                types.Add(typeBuilder);
                MetadataRules.Add(metadata);
#if ASYNC
            }));
            }
            Task.WaitAll(tasks.ToArray());
#else
            }
#endif
            CreateFactory(module, types, MetadataRules, GameSystem);
            try
            {
                Logging.Log("Compiler", TraceEventType.Information, "Generated {0}.dll", name);                
                assemblyBuilder.Save(name + ".dll");
                Logging.Log("Compiler", TraceEventType.Information, "Saved {0}.dll", name);
            }
            catch(Exception c)
            {
                Logging.Log("Crashlog", TraceEventType.Error, "Compiler error: {0}",c);
                if (!background)
                    throw;
            }
            return assemblyBuilder;
        }

        private static void CreateFactory(ModuleBuilder module, List<TypeBuilder> types, List<RuleData> Metadata, string GameSystem)
        {
            //TODO: This still isn't perfect. But we're getting there.
            // Contains giant switch statement, that returns instances w/o Reflection.
            var Factory = module.DefineType("Factory", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, null, new Type[] { typeof(IFactory) });
            // New
            {
                var factoryNew = Factory.DefineMethod("New", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(RulesElement), new Type[] { typeof(string) });

                var switchclass = module.DefineType("<PrivateImplementationDetails>Factory");
                var factoryNewCode = factoryNew.GetILGenerator();
                var dict = switchclass.DefineField("$$method0x0000000-1", typeof(Dictionary<string, int>), FieldAttributes.Assembly | FieldAttributes.Static);
                switchclass.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
                switchclass.CreateType();
                Factory.DefineMethodOverride(factoryNew, typeof(IFactory).GetMethod("New"));

                var index = factoryNewCode.DeclareLocal(typeof(int)); // local0 is int.
                var dictbuilt = factoryNewCode.DefineLabel();
                var defaultvalue = factoryNewCode.DefineLabel();
                // if dict is not null, go to jumptable.
                factoryNewCode.Emit(OpCodes.Volatile);
                factoryNewCode.Emit(OpCodes.Ldsfld, dict);
                factoryNewCode.Emit(OpCodes.Brtrue, dictbuilt);
                // ie: if (dict == null)
                // {
                factoryNewCode.Emit(OpCodes.Ldc_I4, types.Count);
                factoryNewCode.Emit(OpCodes.Newobj, typeof(Dictionary<string, int>).GetConstructor(new Type[] { typeof(int) }));
                int i = 0;
                foreach (var type in types)
                {
                    //factoryNewCode.MarkSequencePoint
                    factoryNewCode.Emit(OpCodes.Dup); // Dictionary reference.
                    factoryNewCode.Emit(OpCodes.Ldstr, type.Name);
                    factoryNewCode.Emit(OpCodes.Ldc_I4, i++);
                    factoryNewCode.Emit(OpCodes.Call, typeof(Dictionary<string, int>).GetMethod("Add"));
                }
                factoryNewCode.Emit(OpCodes.Volatile);
                factoryNewCode.Emit(OpCodes.Stsfld, dict);
                // }
                factoryNewCode.MarkLabel(dictbuilt);
                var jumptable = Enumerable.Range(1, types.Count).Select(n => factoryNewCode.DefineLabel()).ToArray(); // Argh!
                factoryNewCode.Emit(OpCodes.Volatile);
                factoryNewCode.Emit(OpCodes.Ldsfld, dict);
                factoryNewCode.Emit(OpCodes.Ldarg_1); // Arg1 (string internalId)
                factoryNewCode.Emit(OpCodes.Ldloca, index); // local int
                factoryNewCode.Emit(OpCodes.Call, typeof(Dictionary<string, int>).GetMethod("TryGetValue"));
                var endswitch = factoryNewCode.DefineLabel();
                factoryNewCode.Emit(OpCodes.Brfalse, endswitch);

                factoryNewCode.Emit(OpCodes.Ldloc_0); // load the index.
                factoryNewCode.Emit(OpCodes.Switch, jumptable);
                factoryNewCode.Emit(OpCodes.Br, defaultvalue);
                i = 0;
                foreach (var type in types)
                {
                    factoryNewCode.MarkLabel(jumptable[i++]);
                    factoryNewCode.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
                    factoryNewCode.Emit(OpCodes.Ret);
                }

                factoryNewCode.MarkLabel(defaultvalue);
                //factoryNewCode.Emit
                factoryNewCode.MarkLabel(endswitch);
                factoryNewCode.Emit(OpCodes.Ldnull);
                factoryNewCode.Emit(OpCodes.Ret);
            }
            // GameSystem
            {
                CreatePropGetter("GameSystem", GameSystem, Factory, false, typeof(IFactory).GetMethod("get_GameSystem"));
                //var prop = Factory.DefineProperty("GameSystem", PropertyAttributes.None,CallingConventions.HasThis, typeof(string), Type.EmptyTypes);
                //var GameSystem_get = Factory.DefineMethod("GameSystem_get", MethodAttributes.Public | MethodAttributes.SpecialName);
                //var code = GameSystem_get.GetILGenerator();
                //code.Emit(OpCodes.)
                //prop.SetGetMethod(GameSystem_get);
            }
            var CategoryData = new Dictionary<string, List<string>>();
            // Metadata
            {
                var DescribeMethod = Factory.DefineMethod("InitMetadata", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), Type.EmptyTypes);
                Factory.DefineMethodOverride(DescribeMethod, typeof(IFactory).GetMethod("InitMetadata"));
                var ilgen = DescribeMethod.GetILGenerator();
                MethodInfo Register = Builders.RefGetMethod(typeof(RuleFactory),"RegisterMetadata");
                var set_InternalId = typeof(RuleData).GetProperty("InternalId").GetSetMethod();
                var set_Name = typeof(RuleData).GetProperty("Name").GetSetMethod();
                var set_Type = typeof(RuleData).GetProperty("Type").GetSetMethod();
                var set_Categories = typeof(RuleData).GetProperty("Categories").GetSetMethod();
                var set_GameSystem = typeof(RuleData).GetProperty("GameSystem").GetSetMethod();
                var set_Source = typeof(RuleData).GetProperty("Source").GetSetMethod();
                var set_Prereqs = typeof(RuleData).GetProperty("Prereqs").GetSetMethod();
                var set_PrintPrereqs = typeof(RuleData).GetProperty("PrintPrereqs").GetSetMethod();
                var set_PartFile = typeof(RuleData).GetProperty("PartFile").GetSetMethod();
                var set_LineNumber = typeof(RuleData).GetProperty("LineNumber").GetSetMethod();

                var tempdata = ilgen.DeclareLocal(typeof(RuleData));
                foreach (var re in Metadata)
                {
                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Initobj, typeof(RuleData));

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldstr, re.InternalId);
                    ilgen.Emit(OpCodes.Call, set_InternalId);

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldstr, re.Name);
                    ilgen.Emit(OpCodes.Call, set_Name);

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldstr, re.Type);
                    ilgen.Emit(OpCodes.Call, set_Type);

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldstr, GameSystem);
                    ilgen.Emit(OpCodes.Call, set_GameSystem);

                    if (re.Source != null)
                    {
                        ilgen.Emit(OpCodes.Ldloca, tempdata);
                        ilgen.Emit(OpCodes.Ldstr, re.Source);
                        ilgen.Emit(OpCodes.Call, set_Source);
                    }

                    if (re.Prereqs != null)
                    {
                        ilgen.Emit(OpCodes.Ldloca, tempdata);
                        ilgen.Emit(OpCodes.Ldstr, re.Prereqs);
                        ilgen.Emit(OpCodes.Call, set_Prereqs);
                    }

                    if (re.PrintPrereqs != null)
                    {
                        ilgen.Emit(OpCodes.Ldloca, tempdata);
                        ilgen.Emit(OpCodes.Ldstr, re.PrintPrereqs);
                        ilgen.Emit(OpCodes.Call, set_PrintPrereqs);
                    }

                    if (re.Categories != null)
                    {
                        ilgen.Emit(OpCodes.Ldloca, tempdata);
                        EmitNewArray(ilgen, re.Categories);
                        ilgen.Emit(OpCodes.Call, set_Categories);
                    }

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldstr, re.PartFile);
                    ilgen.Emit(OpCodes.Call, set_PartFile);

                    ilgen.Emit(OpCodes.Ldloca, tempdata);
                    ilgen.Emit(OpCodes.Ldc_I4, re.LineNumber);
                    ilgen.Emit(OpCodes.Call, set_LineNumber);

                    ilgen.Emit(OpCodes.Ldloc_0);
                    ilgen.Emit(OpCodes.Call, Register);

                    if (re.Categories !=null)
                    foreach (var c in re.Categories)
                    {
                        if (!CategoryData.ContainsKey(c))
                            CategoryData[c] = new List<string>();
                        CategoryData[c].Add(re.InternalId);
                    }
                }
                ilgen.Emit(OpCodes.Ret);
            }
            
            // Categories
            if (typeof(IFactory).GetMethod("DescribeCategories") != null)
            {
                var DescribeMethod =  Factory.DefineMethod("DescribeCategories", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), new Type[] { typeof(Dictionary<string, CategoryInfo>) });
                var ilgen = DescribeMethod.GetILGenerator();
                // DescribeCategories(Dictionary<string, CategoryInfo> dict);
                MethodInfo DictContains = typeof(Dictionary<string, CategoryInfo>).GetMethod("ContainsKey");
                ConstructorInfo NewCat = typeof(CategoryInfo).GetConstructor(Type.EmptyTypes);
                MethodInfo DictSetIndexer = typeof(Dictionary<string, CategoryInfo>).GetMethod("set_Item");
                MethodInfo DictGetIndexer = typeof(Dictionary<string, CategoryInfo>).GetMethod("get_Item");
                MethodInfo CatInfoMembers = typeof(CategoryInfo).GetProperty("Members").GetMethod;
                MethodInfo ListAddRange = typeof(List<string>).GetMethod("AddRange");
                // Method Start.
                
                foreach (var cat in CategoryData)
                {
                    var KeyExists = ilgen.DefineLabel();
                    ilgen.Emit(OpCodes.Ldarg_1);
                    ilgen.Emit(OpCodes.Ldstr, cat.Key);
                    ilgen.Emit(OpCodes.Callvirt, DictContains);
                    ilgen.Emit(OpCodes.Brtrue, KeyExists);
                    {
                        ilgen.Emit(OpCodes.Ldarg_1);
                        ilgen.Emit(OpCodes.Ldstr, cat.Key);
                        ilgen.Emit(OpCodes.Newobj, NewCat);
                        ilgen.Emit(OpCodes.Callvirt, DictSetIndexer);
                    }
                    ilgen.MarkLabel(KeyExists);
                    ilgen.Emit(OpCodes.Ldarg_1);
                    ilgen.Emit(OpCodes.Ldstr, cat.Key);
                    ilgen.Emit(OpCodes.Callvirt, DictGetIndexer);
                    ilgen.Emit(OpCodes.Callvirt, CatInfoMembers);
                    EmitNewArray(ilgen, cat.Value.ToArray());
                    ilgen.Emit(OpCodes.Callvirt, ListAddRange);
                    //ilgen.Emit(OpCodes.Pop);
                }
                ilgen.Emit(OpCodes.Ret);
                Factory.DefineMethodOverride(DescribeMethod, typeof(IFactory).GetMethod("DescribeCategories"));
            }
            Factory.CreateType();
        }

        private static void Specific(TypeBuilder typeBuilder, Type Parent, ILGenerator ctorgen, XElement item, int specnum)
        {
            bool specific = false;
            var name = item.Name.LocalName;
            var value = item.Value.Trim();
            if (name == "specific")
            {
                name = item.Attribute("name").Value; // DO NOT TRIM. Specifics exist so that they can have leading whitespace.
                specific = true;
            }
            var fname = name.Replace(" ", "");
            fname = char.ToLower(fname[0]) + fname.Substring(1);
            string[] badnames = { "type", "class" };
            if (badnames.Contains(fname))
                fname = "_"+fname;

            FieldInfo field = null;
            var floc = Parent;
            do
            {
                field = Builders.RefGetField(floc, fname);
                floc = floc.BaseType;
            } while (field == null && floc != typeof(Object));

            if (field == null)
            {
                var warning = new CustomAttributeBuilder(typeof(MissingElementAttribute).GetConstructors().FirstOrDefault(), new object[] { specific, name, value,  specnum});
                typeBuilder.SetCustomAttribute(warning);
            }
            else if (field.FieldType == typeof(string))
                Assign(ctorgen, field, value);
            else if (field.FieldType == typeof(int))
            {
                int ival;
                if (int.TryParse(value,out ival))
                    Assign(ctorgen, field, ival);
            }
            else if (field.FieldType == typeof(string[]))
                Assign(ctorgen, field, value.Split(',').Select(s => s.Trim()).ToArray());
            else
            {
                var warning = new CustomAttributeBuilder(typeof(MissingElementAttribute).GetConstructors().FirstOrDefault(), new object[] { specific, fname, value, specnum});
                typeBuilder.SetCustomAttribute(warning);
            }
        }

        private static void CreatePropGetter<T>(string pname, T value, TypeBuilder typeBuilder, bool IsStatic, MethodInfo Override = null)
        {
            MethodBuilder getBuilder = typeBuilder.DefineMethod("get_" + pname, IsStatic ? MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName : MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, typeof(T), Type.EmptyTypes);
            var code = getBuilder.GetILGenerator();
            if (typeof(T) == typeof(string))
                code.Emit(OpCodes.Ldstr, value as string);
            else if (typeof(T).IsArray)
                EmitNewArray(code, value as string[]);
            else
                throw new NotImplementedException();
            code.Emit(OpCodes.Ret);
            if (Override == null)
            {
                PropertyBuilder propBuilder = typeBuilder.DefineProperty(pname, PropertyAttributes.None, typeof(string), new Type[0]);
                propBuilder.SetGetMethod(getBuilder);
            }
            else
                typeBuilder.DefineMethodOverride(getBuilder,Override);
            
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
            EmitNewArray(gen, value);
            gen.Emit(OpCodes.Stfld, field); // Store the array to field.
        }

        private static void EmitNewArray(ILGenerator gen, string[] value)
        {
            gen.Emit(OpCodes.Ldc_I4, value.Length); // Push Length of Array.
            gen.Emit(OpCodes.Newarr, typeof(string)); // Define Array

            // We could, in theory use a local varable, and keep the array in memory.
            // But that can get messy, and would be very hard to keep track of.
            for (int i = 0; i < value.Length; i++)
            {  // this.field[i] = s;
                gen.Emit(OpCodes.Dup);
                gen.Emit(OpCodes.Ldc_I4, i);
                gen.Emit(OpCodes.Ldstr, value[i]);
                gen.Emit(OpCodes.Stelem_Ref);
            }
        }
    }
}

