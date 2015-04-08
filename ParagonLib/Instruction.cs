﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace ParagonLib
{
    public class Instruction
    {
        /// <summary>
        /// public void Calculate(CharElement element, Workspace workspace);
        /// </summary>
        public Action<CharElement, Workspace> Calculate;
        
        /// <summary>
        /// public string Validate();
        /// </summary>
        public Func<string> Validate;

        public Instruction(string Operation, Dictionary<string, string> Parameters, string filename=null, int linenum = -1)
        {
            //Expression<Func<int, bool>> tree = s => s < 5;

            //Expression<Action<CharElement, Workspace>> func = Expression<Action<CharElement, Workspace>>.Block(
            //    new ParameterExpression[] { Expression.Parameter(typeof(CharElement), "Element"), Expression.Parameter(typeof(Workspace), "Workspace")},
            //    Expression.

            Expression operation;
            // This here is extreme optimization.  We're taking the instruction set and compiling it to refer to constant values,
            // rather than the dictionary (or XML) we started with

            Expression<Func<string>> validation = null;
            switch (Operation)
            {
                case "statadd":
                    //lambda = (e, ws) => ws.GetStat(name).Add(value,null,null);
                    /*
                     void anon(Workspace ws, CharElement ce){
                        ws.GetStat("blah").Add("Example", "bkagt", "+4","7",null);
                      }
                     */
                    operation = Builders.StatAdd(Parameters["name"], Params(Parameters, StatAddInfo));
                    break;

                case "statalias":
                    operation = Builders.StatAlias(Parameters["name"], Parameters["alias"]);
                    break;

                case "grant":
                    operation = Builders.Grant(Params(Parameters, GrantInfo));
                    validation = Builders.ValidationLambda(Builders.ValidateExists(Params(Parameters, GrantInfo)));
                    break;

                case "select":
                    operation = Builders.Select(Params(Parameters, SelectInfo));
                    break;

                case "textstring":
                    operation = Builders.TextString(Parameters["name"], Params(Parameters, TextStringInfo));
                    break;

                case "replace":
                    operation = Builders.Replace(Params(Parameters, ReplaceInfo));
                    break;

                case "modify":
                    operation = Builders.Modify(Params(Parameters, ModifyInfo));
                    break;

                case "suggest":
                    //Logging.Log("Xml Loader", TraceEventType.Information, "Suggest is not supported yet.");
                    operation = Builders.Lambda();
                    break;

                default:
                    throw new System.Xml.XmlException(String.Format("Operation '{0}' unknown.", Operation));
            }
            Expression<Action<CharElement, Workspace>> func;
            if (filename != null && linenum != -1)
            {
                var symboldoc = Expression.SymbolDocument(filename);
                var debuginfo = Expression.DebugInfo(symboldoc, linenum, 1, linenum, 1);
                func = Builders.Lambda(debuginfo, operation);
                Body = Expression.Block(debuginfo, operation);
            }
            else
            {
                func = Builders.Lambda(operation);
                Body = Expression.Block(operation);
            }
            Calculate = func.Compile();
            if (validation != null)
                Validate = validation.Compile();
        }

        // Workaround for Mono:
        public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

        [Obsolete("", true)]
        private string[] Params(DefaultDictionary<string, string> Parameters, params string[] keys)
        {
            Parameters = new Dictionary<string, string>(Parameters, StringComparer.CurrentCultureIgnoreCase);
            string[] vals = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == "charelem")
                    vals[i] = keys[i];
                else
                    vals[i] = Parameters[keys[i]];
                Parameters.Remove(keys[i]);
            }
            Parameters.Remove("name");
            Logging.LogIf(Parameters.Count > 0, TraceEventType.Warning, "Xml Loader", "Unexpected params! {0}", Parameters.FirstOrDefault()); // We got a value we weren't expecting.  Let someone know.
            return vals;
        }

        private string[] Params(DefaultDictionary<string, string> Parameters, MethodInfo method)
        {
            var keys = method.GetParameters().Select(p => p.Name.Replace('_','-')).ToArray();
            Parameters = new Dictionary<string, string>(Parameters, StringComparer.CurrentCultureIgnoreCase);
            string[] vals = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == "charelem")
                    vals[i] = keys[i];
                else
                    vals[i] = Parameters[keys[i]];
                Parameters.Remove(keys[i]);
            }
            Parameters.Remove("name");
            Logging.LogIf(Parameters.Count > 0, TraceEventType.Warning, "Xml Loader", "Unexpected {0} params! {1}", method.Name, Parameters.FirstOrDefault()); // We got a value we weren't expecting.  Let someone know.
            return vals;
        }

        private static MethodInfo StatAddInfo = typeof(Workspace.Stat).GetMethod("Add");
        private static MethodInfo TextStringInfo = typeof(Workspace.Stat).GetMethod("AddText");
        private static MethodInfo GrantInfo = typeof(CharElement).GetMethod("Grant");
        private static MethodInfo SelectInfo = typeof(CharElement).GetMethod("Select");
        private static MethodInfo ReplaceInfo = typeof(CharElement).GetMethod("Replace");
        private static MethodInfo ModifyInfo = typeof(CharElement).GetMethod("Modify");
        internal Expression Body;
        internal DebugInfoGenerator PdbGenerator = DebugInfoGenerator.CreatePdbGenerator();

        private static class Builders
        {
            private static ParameterExpression pCharElement = Expression.Parameter(typeof(CharElement), "e");
            private static ParameterExpression pWorkspace = Expression.Parameter(typeof(Workspace), "ws");
            private static ParameterExpression[] pa = new ParameterExpression[] { pCharElement, pWorkspace };
            public static Expression GetStat(string name)
            {
                return Expression.Call(pWorkspace, typeof(Workspace).GetMethod("GetStat"), Expression.Constant(name));
            }

            public static Expression<Action<CharElement, Workspace>> Lambda(params Expression[] Body)
            {
                
                if (Body.Length == 0)
                    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Expression.Block(pCharElement, pWorkspace), pa);
                else if (Body.Length == 1)
                    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Body.First(), pa);
                else
                    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Expression.Block(Body), pa); 
            }

            public static Expression<Func<string>> ValidationLambda(params Expression[] Body)
            {
                var pa = new ParameterExpression[] { pCharElement, pWorkspace };
                if (Body.Count() == 1)
                    return Expression<Func<string>>.Lambda<Func<string>>(Body.First());
                else
                    return Expression<Func<string>>.Lambda<Func<string>>(Expression.Block(pa, Body)); //Not sure if this works.
            }
            public static MethodInfo RefGetMethod(Type t, string m)
            {
                var method = t.GetMethod(m) ?? t.GetMethod(m, BindingFlags.NonPublic) ?? t.GetMethod(m, BindingFlags.Static) ?? t.GetMethod(m, BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    throw new MissingMethodException(String.Format("{0}.{1}() not found.", t, m));
                return method;
            }

            private static IEnumerable<Expression> Args(string[] args)
            {
                foreach (var a in args)
                {
                    if (a == "charelem")
                        yield return pCharElement;
                    else
                        yield return Expression.Constant(a, typeof(String));
                }
                yield break;
            }

            public static Expression StatAdd(string name, string[] args)
            {
                return Expression.Call(
                    Builders.GetStat(name), Builders.RefGetMethod(typeof(Workspace.Stat), "Add"),
                        Args(args)
                    );
            }

            public static Expression TextString(string name, string[] args)
            {
                return Expression.Call(
                    Builders.GetStat(name), Builders.RefGetMethod(typeof(Workspace.Stat), "AddText"),
                        Args(args)
                    );
            }

            internal static Expression Grant(string[] args)
            {
                return Expression.Call(
                    pCharElement, Builders.RefGetMethod(typeof(CharElement), "Grant"),
                        args.Select(e => Expression.Constant(e, typeof(String)))
                    );
            }

            internal static Expression Modify(string[] args)
            {
                return Expression.Call(
                    pCharElement, Builders.RefGetMethod(typeof(CharElement), "Modify"),
                    args.Select(e => Expression.Constant(e, typeof(String)))
                );
            }

            internal static Expression Replace(string[] args)
            {
                return Expression.Call(
                    pCharElement, Builders.RefGetMethod(typeof(CharElement), "Replace"),
                    args.Select(e => Expression.Constant(e, typeof(String)))
                );
            }

            internal static Expression Select(string[] args)
            {
                return Expression.Call(
                    pCharElement, Builders.RefGetMethod(typeof(CharElement), "Select"),
                        args.Select(e => Expression.Constant(e, typeof(String)))
                    );
            }

            internal static Expression StatAlias(string name, string alias)
            {
                return Expression.Call(
                    pWorkspace, Builders.RefGetMethod(typeof(Workspace), "AliasStat"),
                        Expression.Constant(name, typeof(String)), Expression.Constant(alias, typeof(string))
                    );
            }

            internal static Expression ValidateExists(string[] args)
            {
                var id = args[0];
                var type = args[1];
                LabelTarget returnTarget = Expression.Label(typeof(string));
                var RuleIsNull = Expression.Equal(Expression.Call(null, RefGetMethod(typeof(RuleFactory), "FindRulesElement"), Expression.Constant(id, typeof(string)), Expression.Constant(null, typeof(string)), Expression.Constant(null, typeof(CampaignSetting))), Expression.Constant(null));

                var returnError = Expression.Return(returnTarget, StringFormat(Expression.Constant("Cannot grant nonexistant element '{0}'", typeof(string)), Expression.Constant(id)));
                
                return Expression.Block(
                 Expression.IfThen(RuleIsNull, returnError),
                 Expression.Label(returnTarget,Expression.Constant(null, typeof(string)))
                 );
                /* The above code compiles to:
                 if (RuleFactory.FindRulesElement(id))
                   return string.Format("blah blah {0}", id);
                 return null;
                 */
            }

            private static Expression StringFormat(ConstantExpression Format,  ConstantExpression arg0)
            {
               return Expression.Call(typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object)}), Format, arg0);
            }

            internal static Expression<Action<CharElement, Workspace>> Merge(IEnumerable<Expression> rules)
            {
                return Lambda(rules.ToArray());
            }
        }

        internal static Expression<Action<CharElement, Workspace>> Merge(IEnumerable<Instruction> Rules)
        {
            return Builders.Merge(Rules.Select(s => s.Body));
        }

        internal static Assembly CompileToDll(RulesElement[] elements)
        {
            AssemblyName name = new AssemblyName(System.IO.Path.GetFileNameWithoutExtension(elements[0].SourcePart));
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(name + ".dll", true);
            var generator = DebugInfoGenerator.CreatePdbGenerator();
                
            foreach (var re in elements)
            {
                if (re != null &&re.Body != null)
                {
                    try
                    {
                        TypeBuilder typeBuilder = module.DefineType(re.InternalId, TypeAttributes.Public | TypeAttributes.Class);
                        MethodBuilder methodbuilder = typeBuilder.DefineMethod("Calculate", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(CharElement), typeof(Workspace) });
                        re.Body.CompileToMethod(methodbuilder, generator);
                        typeBuilder.CreateType();
                    }
                    catch (Exception)
                    {
                        //TODO: Log something.
                    }
                }
            }
            assemblyBuilder.Save(name + ".dll");
            return assemblyBuilder;
        }
    }
}