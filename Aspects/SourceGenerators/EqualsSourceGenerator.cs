﻿using Aspects.Attributes;
using Aspects.Attributes.Interfaces;
using Aspects.SourceGenerators.Base;
using Aspects.SourceGenerators.Base.DataMembers;
using Aspects.SourceGenerators.Common;
using Aspects.Util;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeInfo = Aspects.SourceGenerators.Common.TypeInfo;

namespace Aspects.SourceGenerators
{
    [Generator]
    internal class EqualsSourceGenerator
        : ObjectMethodSourceGeneratorBase<IAutoEqualsAttribute, IEqualsAttribute, IEqualsExcludeAttribute>
    {
        private const string argName = "obj";
        private const string otherName = "other";

        protected internal override string Name { get; } = nameof(Equals);

        protected override DataMemberPriority Priority { get; } = DataMemberPriority.Field;

        protected override string TypeBody(TypeInfo typeInfo)
        {
            var config = GetConfigAttribute(typeInfo);

            var sb = new StringBuilder();
            AppendMethodStart(typeInfo, sb);

            sb.Append(Code.Indent("return"));
            if (typeInfo.Symbol.IsReferenceType)
                sb.Append($" {argName} == this ||");

            sb.Append($" {argName} is {typeInfo.Name}");

            var symbols = GetSymbols(typeInfo, typeInfo.Symbol.GetMembers(), config.DataMemberKind)
                .ToArray();

            if (symbols.Length > 0)
                sb.Append($" {otherName}");

            if (ShouldIncludeBase(typeInfo, config))
            {
                sb.AppendLine();
                sb.Append(Code.Indent($"&& base.{Name}({argName})", 2));
            }

            foreach (var symbol in symbols)
            {
                var memberEquals = MemberEquals(
                    symbol,
                    typeInfo.HasNullableEnabled,
                    config);

                sb.AppendLine().Append(Code.Indent($"&& {memberEquals}", 2));
            }

            sb.AppendLine(";");

            AppendMethodEnd(typeInfo, sb);
            return sb.ToString();
        }

        private void AppendMethodStart(TypeInfo typeInfo, StringBuilder sb)
        {
            if (typeInfo.HasNullableEnabled)
                sb.AppendLine("#nullable enable");
            sb.Append($"public override bool {Name}(object");
            if (typeInfo.HasNullableEnabled)
                sb.Append('?');
            sb.AppendLine($" {argName})");
            sb.AppendLine("{");
        }

        private static void AppendMethodEnd(TypeInfo typeInfo, StringBuilder sb)
        {
            sb.Append("}");
            if (typeInfo.HasNullableEnabled)
                sb.AppendLine().Append("#nullable restore");
        }

        private static bool ShouldIncludeBase(TypeInfo typeInfo, IAutoEqualsAttribute configAttribute)
        {
            return typeInfo.Symbol.IsReferenceType && (
                configAttribute.BaseCall == BaseCall.On || configAttribute.BaseCall == BaseCall.Auto 
                    && typeInfo.Symbol.BaseType is ITypeSymbol syBase && (
                        syBase.HasAttributeOfType<IAutoEqualsAttribute>() 
                        || syBase.OverridesEquals() 
                        || syBase.GetMembers().Any(m => m.HasAttributeOfType<IEqualsAttribute>())));
        }

        private static string MemberEquals(DataMemberSymbolInfo symbolInfo, bool nullableEnabled, IAutoEqualsAttribute config)
        {
            var memberConfig = GetMemberAttribute(symbolInfo);
            var nullSafety = GetNullSafety(symbolInfo, config, memberConfig);

            var type = symbolInfo.Type;
            var nullSafe = nullSafety == NullSafety.On ||
                nullSafety == NullSafety.Auto && (!nullableEnabled || type.HasNullableAnnotation());

            var memberName = symbolInfo.Name;

            if (string.IsNullOrEmpty(memberConfig.EqualityComparer))
                return Comparison(type, memberName, nullSafe);

            return ComparerComparison(memberConfig.EqualityComparer, memberName, nullSafe && type.IsReferenceType);
        }

        private static NullSafety GetNullSafety(DataMemberSymbolInfo symbol, IAutoEqualsAttribute config, IEqualsAttribute memberConfig)
        {
            if (memberConfig.NullSafety != NullSafety.Auto)
                return memberConfig.NullSafety;
            if (symbol.HasNotNullAttribute())
                return NullSafety.Off;
            if (symbol.HasMaybeNullAttribute())
                return NullSafety.On;
            return config.NullSafety;
        }

        private static string ComparerComparison(string comparer, string memberName, bool nullSafe)
        {
            var s = $"new {comparer}().Equals({memberName}, {otherName}.{memberName})";
            if (!nullSafe)
                return s;
            return $"{memberName} == null && {otherName}.{memberName} == null || {memberName} != null && {otherName}.{memberName} != null && {s}";
        }

        private static string Comparison(ITypeSymbol type, string memberName, bool nullSafe)
        {
            var snippet = Code.EqualityCheck(type, memberName, $"{otherName}.{memberName}", nullSafe);
            return snippet.Contains("||")
                ? $"({snippet})"
                : snippet;
        }

        private static IEqualsAttribute GetMemberAttribute(DataMemberSymbolInfo symbol)
        {
            return GetFirstOrNull<IEqualsAttribute>(symbol.AttributesOfType<IEqualsAttribute>())
                ?? new EqualsAttribute();
        }

        private static IAutoEqualsAttribute GetConfigAttribute(TypeInfo typeInfo)
        {
            return GetFirstOrNull<IAutoEqualsAttribute>(typeInfo.Symbol.AttributesOfType<IAutoEqualsAttribute>())
                ?? new AutoEqualsAttribute();
        }

        private static T GetFirstOrNull<T>(IEnumerable<AttributeData> attributes) where T : class
        {
            var attData = attributes.FirstOrDefault();
            if (attData is null)
                return null;
            return AttributeFactory.Create<T>(attData);
        }
    }
}
