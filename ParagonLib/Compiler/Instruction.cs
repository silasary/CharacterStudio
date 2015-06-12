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
            Body = Generate(Operation, Parameters, filename, linenum, 0);
            
            Expression<Action<CharElement, Workspace>> func = Builders.Lambda(Body);
            Calculate = func.Compile();
            //if (validation != null)
            //    Validate = validation.Compile();
        }

        public static Expression Generate(string Operation, Dictionary<string, string> Parameters, string filename, int linenum, int rulenum)
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
                    operation = Builders.StatAdd(Parameters["name"], Parameters);
                    break;

                case "statalias":
                    operation = Builders.StatAlias(Parameters["name"], Parameters["alias"]);
                    break;

                case "grant":
                    operation = Builders.CallOnCharElem(GrantInfo, Parameters);
                    //validation = Builders.ValidationLambda(Builders.ValidateExists(Params(Parameters, GrantInfo)));
                    break;

                case "select":
                    operation = Builders.CallOnCharElem(SelectInfo, Parameters);
                    break;

                case "textstring":
                    operation = Builders.TextString(Parameters["name"], Parameters);
                    break;

                case "replace":
                    operation = Builders.CallOnCharElem(ReplaceInfo, Parameters);
                    break;

                case "modify":
                    operation = Builders.CallOnCharElem(ModifyInfo, Parameters);
                    break;

                case "suggest":
                    //Logging.Log("Xml Loader", TraceEventType.Information, "Suggest is not supported yet.");
                    operation = Builders.Lambda();
                    break;
                case "drop":
                    operation = Builders.CallOnCharElem(DropInfo, Parameters);
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