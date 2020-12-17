using System;
using Fody;
using Xunit;
using FluentAssertions;


namespace Comparable.Fody.Test
{
    public class ComparableIsNotAdded : BaseTest
    {
        [Fact]
        public void IsNotIComparableClass()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.ClassWithNoIComparableDefined");
            obj.Should().NotBeAssignableTo<IComparable>();
        }

        [Fact]
        public void IsNotIComparableStruct()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.StructWithNoIComparableDefined");
            obj.Should().NotBeAssignableTo<IComparable>();
        }
    }
}