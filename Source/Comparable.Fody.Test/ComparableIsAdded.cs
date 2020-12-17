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
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.IsIComparableClass");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Fact]
        public void IsIComparableStruct()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.IsIComparableStruct");
            obj.Should().BeAssignableTo<IComparable>();
        }
    }
}