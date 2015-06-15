using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ParagonLib.Compiler
{
    internal static class Builders
    {
        private static MethodInfo StatAddInfo = typeof(Workspace.Stat).GetMethod("Add");
        private static MethodInfo AddTextInfo = Builders.RefGetMethod(typeof(Workspace.Stat), "AddText");

        private static ParameterExpression pCharElement = Expression.Parameter(typeof(CharElement), "e");
        private static ParameterExpression pWorkspace = Expression.Parameter(typeof(Workspace), "ws");
        private static ParameterExpression lLastElement = Expression.Parameter(typeof(string), "lastElement");
        private static ParameterExpression[] pa = new ParameterExpression[] { pCharElement, pWorkspace };

        public static Expression GetStat(string name)
        {
            return Expression.Call(pWorkspace, typeof(Workspace).GetMethod("GetStat"), Expression.Constant(name));
        }

        public static Expression<Action<CharElement, Workspace>> Lambda(params Expression[] Body)
        {

            if (Body.Length == 0)
                return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Expression.Block(pCharElement, pWorkspace), pa);
            //else if (Body.Length == 1)
            //    return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Body.First(), pa);
            else
                return Expression<Action<CharElement, Workspace>>.Lambda<Action<CharElement, Workspace>>(Expression.Block(new ParameterExpression[] { lLastElement},  new Expression[] { Expression.Assign(lLastElement, Expression.Constant(""))}.Concat( Body)), pa);
        }

        public static FieldInfo RefGetField(Type t, string f)
        {
            var field = t.GetField(f) ?? t.GetField(f, BindingFlags.NonPublic) ?? t.GetField(f, BindingFlags.Instance) ?? t.GetField(f, BindingFlags.Instance | BindingFlags.NonPublic);
            //if (field == null)
            //    throw new MissingMethodException(String.Format("{0}.{1} not found.", t, f));
            return field;
        }

        public static MethodInfo RefGetMethod(Type t, string m)
        {
            var method = t.GetMethod(m) ??
                t.GetMethod(m, BindingFlags.NonPublic) ?? 
                t.GetMethod(m, BindingFlags.Static)    ?? 
                t.GetMethod(m, BindingFlags.Static   | BindingFlags.NonPublic) ??
                t.GetMethod(m, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(String.Format("{0}.{1}() not found.", t, m));
            return method;
        }

        internal static Dictionary<string, string> MakeDict(IEnumerable<System.Xml.Linq.XAttribute> enumerable)
        {
            Dictionary<string, string> d = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var item in enumerable)
            {
                d.Add(item.Name.LocalName, item.Value);
            }
            return d;
        }

        public static Expression StatAdd(string name, Dictionary<string, string> args)
        {
            return Expression.Call(
                Builders.GetStat(name), StatAddInfo,
                    Params(args, StatAddInfo)
                );
        }

        public static Expression TextString(string name, Dictionary<string,string> args)
        {
            return Expression.Call(Builders.GetStat(name), 
                AddTextInfo,
                Params(args, AddTextInfo)
                );
        }

        public static Expression<Func<string>> ValidationLambda(params Expression[] Body)
        {
            var pa = new ParameterExpression[] { pCharElement, pWorkspace };
            if (Body.Count() == 1)
                return Expression<Func<string>>.Lambda<Func<string>>(Body.First());
            else
                return Expression<Func<string>>.Lambda<Func<string>>(Expression.Block(pa, Body)); //Not sure if this works.
        }
        internal static Expression Grant(string[] args)
        {
            return Expression.Call(
                pCharElement, Builders.RefGetMethod(typeof(CharElement), "Grant"),
                    args.Select(e => Expression.Constant(e, typeof(String)))
                );
        }

        internal static Expression<Action<CharElement, Workspace>> Merge(IEnumerable<Expression> rules)
        {
            return Lambda(rules.ToArray());
        }

        internal static Expression CallOnCharElem(MethodInfo method, Dictionary<string, string> args)
        {
            return Expression.Call(
                pCharElement, method,
                Params(args, method)
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
//            var type = args[1];
            LabelTarget returnTarget = Expression.Label(typeof(string));
            var RuleIsNull = Expression.Equal(Expression.Call(null, RefGetMethod(typeof(RuleFactory), "FindRulesElement"), Expression.Constant(id, typeof(string)), Expression.Constant(null, typeof(string)), Expression.Constant(null, typeof(CampaignSetting))), Expression.Constant(null));

            var returnError = Expression.Return(returnTarget, StringFormat(Expression.Constant("Cannot grant nonexistant element '{0}'", typeof(string)), Expression.Constant(id)));

            return Expression.Block(
             Expression.IfThen(RuleIsNull, returnError),
             Expression.Label(returnTarget, Expression.Constant(null, typeof(string)))
             );
            /* The above code compiles to:
             if (RuleFactory.FindRulesElement(id))
               return string.Format("blah blah {0}", id);
             return null;
             */
        }

        private static IEnumerable<Expression> Params(DefaultDictionary<string, string> Parameters, MethodInfo method)
        {
            var keys = method.GetParameters().Select(p => p.Name.Replace('_', '-')).ToArray();
            Parameters = new Dictionary<string, string>(Parameters, StringComparer.CurrentCultureIgnoreCase);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] == "charelem")
                    yield return pCharElement;
                else if (method.Name=="Modify" && keys[i] == "name" && Parameters[keys[i]] == null)
                    yield return lLastElement;
                else
                    yield return Expression.Constant(Parameters[keys[i]], typeof(String));
                Parameters.Remove(keys[i]);
            }
            Parameters.Remove("name");
            Logging.LogIf(Parameters.Count > 0, TraceEventType.Warning, "Xml Loader", "Unexpected {0} params! {1}", method.Name, Parameters.FirstOrDefault()); // We got a value we weren't expecting.  Let someone know.
        }

        private static Expression StringFormat(ConstantExpression Format, ConstantExpression arg0)
        {
            return Expression.Call(typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }), Format, arg0);
        }
    }
}
