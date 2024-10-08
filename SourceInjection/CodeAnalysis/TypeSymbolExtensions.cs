﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceInjection.CodeAnalysis
{
    public static class TypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> Inheritance(this ITypeSymbol symbol)
        {
            while (symbol != null)
            {
                yield return symbol;
                symbol = symbol.BaseType;
            }
        }

        public static bool Is<T>(this ITypeSymbol symbol)
            => Is(symbol, typeof(T));

        public static bool Is(this ITypeSymbol symbol, Type requiredType)
            => Is(symbol, requiredType.FullName);
        
        public static bool Is(this ITypeSymbol symbol, ITypeSymbol requiredType)
            => Is(symbol, requiredType.ToDisplayString());

        public static bool HasNullableAnnotation(this ITypeSymbol symbol)
        {
            return symbol.NullableAnnotation == NullableAnnotation.Annotated;
        }

        public static bool Implements<T>(this ITypeSymbol symbol)
        {
            var name = typeof(T).FullName;
            return symbol.ToDisplayString() == name
                || symbol.AllInterfaces.Any(i => i.ToDisplayString() == name);
        }

        public static bool IsEnumerable(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString().TrimEnd('?') == NameOf.IEnumerable
                || symbol.AllInterfaces.Any(i => i.ToDisplayString().TrimEnd('?') == NameOf.IEnumerable);
        }

        public static bool IsGenericEnumerable(this ITypeSymbol symbol)
        {
            return symbol.ToDisplayString().StartsWith(NameOf.GenericIEnumerable)
                || symbol.AllInterfaces.Any(i => i.ToDisplayString().StartsWith(NameOf.GenericIEnumerable));
        }

        public static bool IsPrimitive(this ITypeSymbol symbol, bool allowNullable)
        {
            return symbol.IsBoolean(allowNullable)
                || symbol.IsSByte(allowNullable)
                || symbol.IsByte(allowNullable)
                || symbol.IsInt16(allowNullable)
                || symbol.IsUInt16(allowNullable)
                || symbol.IsInt32(allowNullable)
                || symbol.IsUInt32(allowNullable)
                || symbol.IsInt64(allowNullable)
                || symbol.IsUInt64(allowNullable)
                || symbol.IsSingle(allowNullable)
                || symbol.IsDouble(allowNullable)
                || symbol.IsDecimal(allowNullable)
                || symbol.IsChar(allowNullable)
                || (symbol.IsNativeIntegerType || symbol.TypeKind == TypeKind.Pointer)
                    && (!symbol.HasNullableAnnotation() || allowNullable);
        }

        public static bool CanUseEqualityOperatorsByDefault(this ITypeSymbol symbol)
        {
            return symbol.IsRecord
                || symbol.TypeKind == TypeKind.Enum
                || symbol.IsString(true)
                || symbol.IsPrimitive(true);
        }

        public static bool IsObject(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("object", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Object)}", allowNullable);
        }

        public static bool IsString(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("string", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(String)}", allowNullable);
        }

        public static bool IsBoolean(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("bool", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Boolean)}", allowNullable);
        }

        public static bool IsChar(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("char", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Char)}", allowNullable);
        }

        public static bool IsSByte(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("sbyte", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(SByte)}", allowNullable);
        }

        public static bool IsByte(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("byte", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Byte)}", allowNullable);
        }

        public static bool IsInt16(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("short", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Int16)}", allowNullable);
        }

        public static bool IsUInt16(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("ushort", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(UInt16)}", allowNullable);
        }

        public static bool IsInt32(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("int", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Int32)}", allowNullable);
        }

        public static bool IsUInt32(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("uint", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(UInt32)}", allowNullable);
        }

        public static bool IsInt64(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("long", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Int64)}", allowNullable);
        }

        public static bool IsUInt64(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("ulong", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(UInt64)}", allowNullable);
        }

        public static bool IsSingle(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("float", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Single)}", allowNullable);
        }

        public static bool IsDouble(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("double", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Double)}", allowNullable);
        }

        public static bool IsDecimal(this ITypeSymbol symbol, bool allowNullable = false)
        {
            return symbol.IsType("decimal", allowNullable)
                || symbol.IsType($"{nameof(System)}.{nameof(Decimal)}", allowNullable);
        }

        private static bool Is(ITypeSymbol symbol, string requiredType)
        {
            requiredType = requiredType.TrimEnd('?');
            return symbol.Inheritance().Any(sy => sy.IsType(requiredType, true))
                || symbol.AllInterfaces.Any(sy => sy.IsType(requiredType, true));
        }

        private static bool IsType(this ITypeSymbol type, string typeName, bool alsoCheckNullable = false)
        {
            var s = type.ToDisplayString();
            return s == typeName
                || alsoCheckNullable && s == typeName + '?';
        }
    }
}
