﻿using NUnit.Framework;

namespace Aspects.Test.Equals.Comparer.StructType.NullSafetyOn
{
    [TestFixture]
    internal class ComparerTests
    {
        [Test]
        [TestCaseSource(typeof(ComparerResources), nameof(ComparerResources.MustUseComparerEqualization))]
        public void AssertDefaultComparisonNullSafety(Type type, bool nullSafe)
        {
            ComparerAssert.NullSafety(type, nullSafe, Equalization.Comparer);
        }

        [Test]
        [TestCaseSource(typeof(ComparerResources), nameof(ComparerResources.MustUseComparerStructTypeEqualization))]
        public void AssertStructComparisonNullSafety(Type type, bool nullSafe)
        {
            ComparerAssert.NullSafety(type, nullSafe, Equalization.ComparerNullableNonReferenceType);
        }
    }
}
