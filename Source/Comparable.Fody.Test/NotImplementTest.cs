using System;
using Fody;
using Xunit;
using FluentAssertions;


namespace Comparable.Fody.Test
{
    public class NotImplementTest : BaseTest
    {
        [Fact]
        public void InClass()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.ClassWithNoIComparableDefined");
            obj.Should().NotBeAssignableTo<IComparable>();
        }

        [Fact]
        public void InStruct()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.StructWithNoIComparableDefined");
            obj.Should().NotBeAssignableTo<IComparable>();
        }
    }
}