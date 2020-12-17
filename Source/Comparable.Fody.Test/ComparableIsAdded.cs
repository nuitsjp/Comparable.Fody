using System;
using FluentAssertions;
using Xunit;

namespace Comparable.Fody.Test
{
    public class ComparableIsAdded : BaseTest
    {
        [Fact]
        public void IsIComparableClass()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.ClassWithIComparableDefined");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Fact]
        public void IsIComparableStruct()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.StructWithIComparableDefined");
            obj.Should().BeAssignableTo<IComparable>();
        }
    }
}