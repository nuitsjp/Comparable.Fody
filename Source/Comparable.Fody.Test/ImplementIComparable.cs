using System;
using AssemblyToProcess;
using FluentAssertions;
using Xunit;

namespace Comparable.Fody.Test
{
    public class ImplementIComparable : BaseTest
    {
        [Fact]
        public void InClass()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.ClassWithIComparableDefined");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Fact]
        public void InStruct()
        {
            var obj = (object)TestResult.GetInstance("AssemblyToProcess.StructWithIComparableDefined");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Fact]
        public void Return1WhenComparedToNull()
        {
            var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.WithSingleProperty");
            comparable.CompareTo(null)
                .Should().Be(1);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenComparedToDifferentType()
        {
            var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.WithSingleProperty");
            var instanceOfDifferentType = string.Empty;
            comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                .Should().Throw<ArgumentException>()
                .WithMessage("Object is not a AssemblyToProcess.WithSingleProperty.");
        }

        [Fact]
        public void ReturnCompareToResultOfValue()
        {
            var instance0 = TestResult.GetInstance("AssemblyToProcess.WithSingleProperty");
            instance0.Value = 1;
            var instance1 = TestResult.GetInstance("AssemblyToProcess.WithSingleProperty");
            instance1.Value = 2;

            ((IComparable) instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value.CompareTo(instance1.Value));

        }
    }
}