﻿using Aspects.SourceGenerators.Common;

namespace Aspects.SourceGenerators
{
    internal class EqualityCodeInfo
    {
        private readonly bool _isInequality;
        private readonly string _nameA;
        private readonly string _nameB;

        public EqualityCodeInfo(string nameA, string nameB, bool isInequality = false)
        {
            _isInequality = isInequality;
            _nameA = nameA;
            _nameB = nameB;
        }

        public string LinqSequenceEquality() => MayInversed($"{NameOf.LinqSequenceEqual}({_nameA}, {_nameB})");

        public string NullSafeLinqSequenceEquality() => NullSafeSequenceEquality(NameOf.LinqSequenceEqual);

        public string AspectsSequenceEquality() => MayInversed($"{NameOf.AspectsDeepSequenceEqual}({_nameA}, {_nameB})");

        public string NullSafeAspectsSequenceEquality() => NullSafeSequenceEquality(NameOf.AspectsDeepSequenceEqual);

        public string MethodEquality() => MayInversed($"{_nameA}.{nameof(Equals)}({_nameB})");

        public string NullSafeMethodEquality()
        {
            return _isInequality
                ? $"{_nameA} != null && !{_nameA}.{nameof(Equals)}({_nameB}) || {_nameA} == null && {_nameB} != null"
                : $"{_nameA} == null && {_nameB} == null || {_nameA}?.{nameof(Equals)}({_nameB}) == true";
        }

        public string OperatorEquality()
        {
            return _isInequality
                ? $"{_nameA} != {_nameB}"
                : $"{_nameA} == {_nameB}";
        }

        private string MayInversed(string s) => _isInequality ? '!' + s : s;

        private string NullSafeSequenceEquality(string method)
        {
            return _isInequality
                ? $"!({_nameA} == {_nameB}) || {_nameA} != null && {_nameB} != null && !{method}({_nameA}, {_nameB})"
                : $"{_nameA} == {_nameB} || {_nameA} != null && {_nameB} != null && {method}({_nameA}, {_nameB})";
        }
    }
}