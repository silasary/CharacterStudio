using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace ParagonLib.Compiler
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
            Body = Generate(Operation, Parameters, filename, linenum);
            
            Expression<Action<CharElement, Workspace>> func = Builders.Lambda(Body);
            Calculate = func.Compile();
            //if (validation != null)
            //    Validate = validation.Compile();
        }

        public static Expression Generate(string Operation, Dictionary<string, string> Parameters, string filename, int linenum)
        {
            //Expression<Func<int, bool>> tree = s => s < 5;

            //Expression<Action<CharElement, Workspace>> func = Expression<Action<CharElement, Workspace>>.Block(
            //    new ParameterExpression[] { Expression.Parameter(typeof(CharElement), "Element"), Expression.Parameter(typeof(Workspace), "Workspace")},
            //    Expression.

            Expression Body;

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
                case "drop":
                    operation = Builders.CallOnCharElem(DropInfo, Params(Parameters, DropInfo));
                    break;
                default:
                    throw new System.Xml.XmlException(String.Format("Operation '{0}' unknown.", Operation));
            }
            if (filename != null && linenum > 0)
            {
                var symboldoc = Expression.SymbolDocument(filename);
                var debuginfo = Expression.DebugInfo(symboldoc, linenum, 1, linenum, 1);
                Body = Expression.Block(debuginfo, operation);
            }
            else
            {
                Body = Expression.Block(operation);
            }
            return Body;
        }

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

        private static string[] Params(DefaultDictionary<string, string> Parameters, MethodInfo method)
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
        private static MethodInfo DropInfo = typeof(CharElement).GetMethod("Drop");

        internal Expression Body;
        internal DebugInfoGenerator PdbGenerator = DebugInfoGenerator.CreatePdbGenerator();

        internal static Expression<Action<CharElement, Workspace>> Merge(IEnumerable<Instruction> Rules)
        {
            return Builders.Merge(Rules.Select(s => s.Body));
        }
    }
}