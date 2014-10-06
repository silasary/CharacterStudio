using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ParagonLib
{
    public class Instruction
    {
        /// <summary>
        /// public void Calculate(CharElement element, Workspace workspace);
        /// </summary>
        public Action<CharElement, Workspace> Calculate;

        public Instruction(string Operation, Dictionary<string, string> Parameters)
        {
            //Expression<Func<int, bool>> tree = s => s < 5;

            //Expression<Action<CharElement, Workspace>> func = Expression<Action<CharElement, Workspace>>.Block(
            //    new ParameterExpression[] { Expression.Parameter(typeof(CharElement), "Element"), Expression.Parameter(typeof(Workspace), "Workspace")},
            //    Expression.

            Expression<Action<CharElement, Workspace>> func;
            // This here is extreme optimization.  We're taking the instruction set and compiling it to refer to constant values,
            // rather than the dictionary (or XML) we started with
            switch (Operation)
            {
                case "statadd":
                    //lambda = (e, ws) => ws.GetStat(name).Add(value,null,null);
                    /*
                     void anon(Workspace ws, CharElement ce){
                        ws.GetStat("blah").Add("Example", "bkagt", "+4","7",null);
                      }
                     */
                    func = Builders.Lambda(Builders.StatAdd(Parameters["name"], Params(Parameters, "value", "condition", "requires", "type", "Level"))); // TODO: Support wearing= at some point
                    break;

                case "statalias":
                    func = Builders.Lambda(Builders.StatAlias(Parameters["name"], Parameters["alias"]));
                    break;

                case "grant":
                    func = Builders.Lambda(Builders.Grant(Params(Parameters, "name", "type", "requires", "Level")));
                    break;

                case "select":
                    func = Builders.Lambda(Builders.Select(Params(Parameters, "category", "number", "type", "requires", "optional", "Level")));
                    break;

                case "textstring":
                    func = Builders.Lambda(Builders.TextString(Parameters["name"], Params(Parameters, "value", "condition", "requires", "Level")));
                    break;

                default:
                    throw new InvalidOperationException(String.Format("Operation '{0}' unknown.", Operation));
            }
            Calculate = func.Compile();
        }

        // Workaround for Mono:
        public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

        private string[] Params(DefaultDictionary<string, string> Parameters, params string[] keys)
        {
            string[] vals = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                vals[i] = Parameters[keys[i]];
                Parameters.Remove(keys[i]);
            }
            Parameters.Remove("name");
            Debug.Assert(Parameters.Count == 0); // We got a value we weren't expecting.  Let someone know.

            return vals;
        }

        private static class Builders
        {
            private static ParameterExpression pCharElement = Expression.Parameter(typeof(CharElement), "e");
            private static ParameterExpression pWorkspace = Expression.Parameter(typeof(Workspace), "ws");

            public static Expression GetStat(string name)
            {
                return Expression.Call(pWorkspace, typeof(Workspace).GetMethod("GetStat"), Expression.Constant(name));
            }

            public static Expression<Action<CharElement, Workspace>> Lambda(params Expression[] Body)
            {
                var pa = new ParameterExpression[] { pCharElement, pWorkspace };
                if (Body.Count() == 1)
                    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Body.First(), pa);
                else
                    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Expression.Block(pa, Body), pa); //Not sure if this works.
            }

            public static MethodInfo RefGetMethod(Type t, string m)
            {
                var method = t.GetMethod(m) ?? t.GetMethod(m, BindingFlags.NonPublic);
                if (method == null)
                    throw new MissingMethodException(String.Format("{0}.{1}() not found.", t, m));
                return method;
            }

            public static Expression StatAdd(string name, string[] args)
            {
                return Expression.Call(
                    Builders.GetStat(name), Builders.RefGetMethod(typeof(Workspace.Stat), "Add"),
                        args.Select(e => Expression.Constant(e, typeof(String)))
                    );
            }

            public static Expression TextString(string name, string[] args)
            {
                return Expression.Call(
                    Builders.GetStat(name), Builders.RefGetMethod(typeof(Workspace.Stat), "AddText"),
                        args.Select(e => Expression.Constant(e, typeof(String)))
                    );
            }

            internal static Expression Grant(string[] args)
            {
                return Expression.Call(
                    pCharElement, Builders.RefGetMethod(typeof(CharElement), "Grant"),
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
        }
    }
}