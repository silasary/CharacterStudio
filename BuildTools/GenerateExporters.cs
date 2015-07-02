using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuildTools
{
    class GenerateExporters
    {
        internal static bool Generate()
        {
            Environment.CurrentDirectory = Path.Combine(BuildTools.SolutionDir, "ParagonLib", "RuleBases");
            var CSharp = CodeDomProvider.CreateProvider("CSharp");
            var PropParser = new Regex(@"\[AccessedThroughProperty\(""(?<p>\w+)""\)]\W+ (protected|public)\W+(?<t>[\w\[\]]+)\W+(?<f>\w+);", RegexOptions.IgnorePatternWhitespace);

            foreach (var file in Directory.EnumerateFiles(".", "*.cs"))
            {
                var text = File.ReadAllText(file, Encoding.UTF8);
                var start = text.IndexOf("#region _GENERATED_");
                var end = text.IndexOf("#endregion _GENERATED_") + "#endregion _GENERATED_".Length;
                if (start == -1)
                {
                    var t2 = text.Substring(0, text.LastIndexOf('}'));
                    t2 = t2.Substring(0, t2.LastIndexOf('}'));
                    start = t2.Length;
                    end = t2.Length;
                }
                //var code = CSharp.Parse(File.OpenText(file));
                //var myclass = code.Namespaces[0].Types[0];
                var m = PropParser.Matches(text);
                StringBuilder sb = new StringBuilder(text.Substring(0, start).TrimEnd());
                var sw = new StringWriter(sb);
                
                sb.AppendLine();
                sb.AppendLine("#region _GENERATED_");
                {
                    foreach (Match field in m)
                    {
                        var prop = new CodeMemberProperty();
                        prop.Name = field.Groups["p"].Value;
                        prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                        switch (field.Groups["t"].Value)
                        {
                            case "string":
                                prop.Type = new CodeTypeReference(typeof(string));
                                break;
                            case "int":
                                prop.Type = new CodeTypeReference(typeof(int));
                                break;                                
                            case "string[]":
                                prop.Type = new CodeTypeReference(typeof(string[]));
                                break;
                            default:
                                prop.Type = new CodeTypeReference(typeof(object));
                                break;
                        }
                        prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Groups["f"].Value)));
                        CSharp.GenerateCodeFromMember(prop, sw, new CodeGeneratorOptions());
                    }
                }
                {
                    bool isRecs = Path.GetFileName(file) == "RulesElement.cs";
                    var GetSpecific = new CodeMemberMethod();
                    GetSpecific.Name = "GetSpecific";
                    GetSpecific.ReturnType = new CodeTypeReference(typeof(System.String));
                    GetSpecific.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "specific"));
                    if (isRecs)
                        GetSpecific.Attributes = MemberAttributes.Family;                    
                    else
                        GetSpecific.Attributes = MemberAttributes.Family | MemberAttributes.Override;
                    foreach (Match f in m)
                    {
                        if (f.Groups["t"].Value != "string")
                            continue;
                        GetSpecific.Statements.Add(new CodeConditionStatement(
                               new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("specific"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(f.Groups["p"].Value)),
                                new CodeStatement[] 
                                {
                                    new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), f.Groups["f"].Value))
                                }
                            ));
                    }
                    
                    
                    GetSpecific.Statements.Add(new CodeMethodReturnStatement(
                        isRecs ? (CodeExpression)new CodePrimitiveExpression(null)
                               : new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),"GetSpecific", new CodeVariableReferenceExpression("specific"))
                    ));

                    CSharp.GenerateCodeFromMember(GetSpecific, sw, new CodeGeneratorOptions());
                }
                sb.AppendLine("#endregion _GENERATED_");
                sb.Append("\t").Append(text.Substring(end).TrimStart());
                File.WriteAllText(file, sb.ToString());
            }
            return true;
        }
    }
}
