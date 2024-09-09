﻿using Aspects.Common;
using Aspects.SourceGenerators.Base.DataMembers;
using Aspects.Util;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System;
using System.Text;
using Aspects.Util.SymbolExtensions;

namespace Aspects.SourceGenerators.Common
{
    internal static class Code
    {
        public static string CreateUnconflictingVariable(INamedTypeSymbol type, string name = "temp")
        {
            var raw = name;
            var i = 1;

            while (type.GetMembers(name).Length > 0)
                name = $"{raw}{i++}";

            return name;
        }

        public static string PropertyNameFromField(IFieldSymbol field)
        {
            return PropertyNameFromField(field.Name);
        }

        public static string PropertyNameFromField(string fieldName)
        {
            while (fieldName.Length > 0 && fieldName[0] == '_')
                fieldName = fieldName.Substring(1);

            if (fieldName.Length > 0 && fieldName[0] >= 'a' && fieldName[0] <= 'z')
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);

            return fieldName;
        }

        public static string Indent(string value = "", int tabCount = 1)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tabCount; i++)
                sb.Append("  ");
            sb.Append(value);
            return sb.ToString();
        }

        public static string InequalityCheck(DataMemberSymbolInfo member, string otherName, bool nullSafe, string comparer)
        {
            var info = new EqualityCodeInfo(member.Name, otherName, true);
            return EqualityFromInfo(member, info, nullSafe, comparer);
        }

        public static string EqualityCheck(DataMemberSymbolInfo member, string otherName, bool nullSafe, string comparer)
        {
            var info = new EqualityCodeInfo(member.Name, otherName);
            return EqualityFromInfo(member, info, nullSafe, comparer);
        }

        private static string EqualityFromInfo(DataMemberSymbolInfo member, EqualityCodeInfo codeInfo, bool nullSafe, string comparer)
        {
            if (!string.IsNullOrEmpty(comparer))
            {
                var comparerSupportsNullable = ComparerEqualsSupportsNullable(comparer);
                comparer = ReduceComparerName(member, comparer);
                if (comparerSupportsNullable)
                    nullSafe = false;

                if (!member.Type.IsReferenceType && member.Type.HasNullableAnnotation())
                    return codeInfo.ComparerNullableNonReferenceTypeEquality(comparer, nullSafe);
                return codeInfo.ComparerEquality(comparer, nullSafe && member.Type.IsReferenceType);
            }

            if (member.Type.CanUseEqualityOperatorsByDefault())
                return codeInfo.OperatorEquality();

            if (!member.Type.OverridesEquals())
            {
                if (member.Type.CanUseSequenceEquals())
                    return codeInfo.LinqSequenceEquality(nullSafe);

                if (member.Type is IArrayTypeSymbol arrayType && arrayType.Rank > 1)
                    return codeInfo.AspectsArrayEquality(nullSafe);

                if (member.Type.IsEnumerable())
                    return codeInfo.AspectsSequenceEquality(nullSafe);
            }
            return codeInfo.MethodEquality(member.Type.IsReferenceType && nullSafe);
        }

        public static string GetHashCode(DataMemberSymbolInfo member, bool nullSafe, string comparer)
        {
            if (!string.IsNullOrEmpty(comparer))
            {
                if (!member.Type.IsReferenceType && member.Type.HasNullableAnnotation())
                    return new HashCodeCodeInfo(member.Name).ComparerNullableNonReferenceTypeHashCode(comparer, nullSafe);
                return new HashCodeCodeInfo(member.Name).ComparerHashCode(comparer, nullSafe && member.Type.IsReferenceType);
            }

            if (!member.Type.OverridesGetHashCode())
            {
                var info = new HashCodeCodeInfo(member.Name);

                if (member.Type.CanUseCombinedHashCode())
                    return info.CombinedHashCode(nullSafe);
                if (member.Type.IsEnumerable())
                    return info.DeepCombinedHashCode(nullSafe);
            }
            return member.Name;
        }

        private static string ReduceComparerName(DataMemberSymbolInfo member, string comparerName)
        {
            var containingTypeName = member.ContainingType.ToDisplayString();
            if(!comparerName.StartsWith(containingTypeName))
                return comparerName;

            var idx = comparerName.IndexOf('.', containingTypeName.Length) + 1;
            if(idx <= 0)
                return comparerName;

            return comparerName.Substring(idx);
        }

        private static bool ComparerEqualsSupportsNullable(string comparerName)
        {
            var typeSy = Types.Get(comparerName);
            if (typeSy != null)
            {
                var equalsMethod = typeSy.GetAllMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => IsComparerEqualsMethod(m));

                return equalsMethod != null
                    && equalsMethod.Parameters.All(p => p.Type.HasNullableAnnotation());
            }

            var type = Type.GetType(comparerName);
            if (type != null)
            {
                var equalsMethod = type.GetMethods()
                    .FirstOrDefault(m => IsComparerEqualsMethod(m));

                return equalsMethod != null
                    && equalsMethod.GetParameters().All(p => Nullable.GetUnderlyingType(p.ParameterType) != null);
            }
            return false;
        }

        private static bool IsComparerEqualsMethod(IMethodSymbol method)
        {
            return method.Name == nameof(Equals)
                && method.ReturnType.IsBoolean()
                && method.Parameters.Length == 2;
        }

        private static bool IsComparerEqualsMethod(MethodInfo method)
        {
            return method.Name == nameof(Equals)
                && method.ReturnType == typeof(bool)
                && method.GetParameters().Length == 2;
        }
    }
}
