﻿using Aspects.SourceGenerators.Base;
using Microsoft.CodeAnalysis;
using System.Text;
using TypeInfo = Aspects.SourceGenerators.Common.TypeInfo;
using Aspects.Attributes.Interfaces;
using Aspects.SourceGenerators.Common;
using Aspects.Attributes;
using Aspects.Util;
using System.Linq;

namespace Aspects.SourceGenerators
{
    [Generator]
    internal class ToStringSourceGenerator 
        : ObjectMethodSourceGeneratorBase<IAutoToStringAttribute, IToStringAttribute, IToStringExcludeAttribute>
    {
        protected internal override string Name { get; } = nameof(ToString);

        protected override DataMemberPriority Priority { get; } = DataMemberPriority.Property;

        protected override string TypeBody(TypeInfo typeInfo)
        {
            var config = GetConfigAttribute(typeInfo);

            var sb = new StringBuilder();
            sb.AppendLine($"public override string {Name}()");
            sb.AppendLine("{");

            sb.Append(Code.Indent($"return $\"({typeInfo.Name})"));

            var symbols = GetSymbols(typeInfo, typeInfo.Members(true), config.DataMemberKind)
                .ToArray();

            if (symbols.Length > 0)
            {
                sb.Append("{{");
                sb.Append($"{symbols[0].Name}: {{{symbols[0].Name}}}");

                for(int i = 1; i < symbols.Length; i++)
                    sb.Append($", {symbols[i].Name}: {{{symbols[i].Name}}}");
                sb.Append("}}");
            }
            sb.AppendLine("\";");
            sb.Append('}');

            return sb.ToString();
        }

        private static IAutoToStringAttribute GetConfigAttribute(TypeInfo typeInfo)
        {
            var attData = typeInfo.Symbol.AttributesOfType<IAutoToStringAttribute>().FirstOrDefault();
            if (attData is null || !AttributeFactory.TryCreate<IAutoToStringAttribute>(attData, out var config))
                return new AutoToStringAttribute();
            return config;
        }
    }
}
