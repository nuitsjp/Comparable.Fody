using System;
using Fody;
using Xunit;
using FluentAssertions;


namespace Comparable.Fody.Test
{
    public class ComparableIsNotAdded : BaseTest
    {
        [Fact]
        public void IsNotIComparable()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.IsNotIComparable");
            obj.Should().NotBeAssignableTo<IComparable>();
        }
    }
}