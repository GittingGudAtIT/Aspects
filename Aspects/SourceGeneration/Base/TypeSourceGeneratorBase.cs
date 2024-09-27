﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using TypeInfo = Aspects.SourceGeneration.Common.TypeInfo;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspects.SourceGeneration.SyntaxReceivers;
using Aspects.SourceGeneration.Diagnostics;

namespace Aspects.SourceGeneration.Base
{
    internal abstract class TypeSourceGeneratorBase : ISourceGenerator
    {
        protected internal abstract string Name { get; }

        protected abstract TypeSyntaxReceiver SyntaxReceiver { get; }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => SyntaxReceiver);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver == SyntaxReceiver || context.SyntaxContextReceiver.Equals(SyntaxReceiver))
            {
                foreach (var typeInfo in SyntaxReceiver.IdentifiedTypes)
                {
                    if (typeInfo.SyntaxNode.Parent is TypeDeclarationSyntax)
                        context.ReportDiagnostic(Errors.NestedClassesAreNotSupported(typeInfo.Symbol, Name));
                    else if (!typeInfo.HasPartialModifier)
                        context.ReportDiagnostic(Errors.MissingPartialModifier(typeInfo.Symbol, Name));
                    else
                    {
                        var src = GeneratePartialType(typeInfo);
                        context.AddSource($"{typeInfo.FullName.Replace('<', '[').Replace('>', ']').Replace('.', '/')}-{Name}.g.cs", SourceText.From(src, Encoding.UTF8));
                    }
                }
            }
        }

        protected abstract IEnumerable<string> Dependencies(TypeInfo typeInfo);

        protected virtual IEnumerable<string> InterfacesToAdd(TypeInfo typeInfo)
        {
            return Enumerable.Empty<string>();
        }

        protected abstract string TypeBody(TypeInfo typeInfo);

        private string GeneratePartialType(TypeInfo typeInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");

            foreach (var dependency in Dependencies(typeInfo))
                sb.AppendLine(dependency);

            sb.AppendLine();

            sb.AppendLine($"namespace {typeInfo.Symbol.ContainingNamespace.ToDisplayString()}");
            sb.AppendLine("{");
            sb.Append(Text.Indent());

            sb.Append(typeInfo.Declaration);
            var ifaces = InterfacesToAdd(typeInfo);
            if (!ifaces.Any())
                sb.AppendLine();
            else
                sb.AppendLine($" : {string.Join(", ", ifaces)}");
            sb.AppendLine(Text.Indent("{"));

            sb.AppendLine(TypeBody(typeInfo)
                .Replace("\n", $"\n{Text.Indent(tabCount: 2)}")
                .Insert(0, Text.Indent(tabCount: 2)));

            sb.AppendLine(Text.Indent("}"));
            sb.Append("}");

            return sb.ToString();
        }
    }
}
