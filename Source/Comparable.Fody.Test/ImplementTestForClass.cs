using System;
using FluentAssertions;
using Xunit;

namespace Comparable.Fody.Test
{
    namespace ImplementTest
    {
        public class ForClass : BaseTest
        {
            [Fact]
            public void InClass()
            {
                var obj = (object)TestResult.GetInstance("AssemblyToProcess.ClassWithIComparableDefined");
                obj.Should().BeAssignableTo<IComparable>();
            }

            [Fact]
            public void Return1WhenComparedToNull()
            {
                var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.ClassWithSingleProperty");
                comparable.CompareTo(null)
                    .Should().Be(1);
            }

            [Fact]
            public void ThrowArgumentExceptionWhenComparedToDifferentType()
            {
                var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.ClassWithSingleProperty");
                var instanceOfDifferentType = string.Empty;
                comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                    .Should().Throw<ArgumentException>()
                    .WithMessage("Object is not a AssemblyToProcess.ClassWithSingleProperty.");
            }

            [Fact]
            public void ReturnCompareToResultOfSingleProperty()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.ClassWithSingleProperty");
                instance0.Value = "1";
                var instance1 = TestResult.GetInstance("AssemblyToProcess.ClassWithSingleProperty");
                instance1.Value = "2";

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value.CompareTo(instance1.Value));

            }


            [Fact]
            public void ReturnCompareToResultOfSingleField()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.ClassWithSingleField");
                instance0.Value = 1;
                var instance1 = TestResult.GetInstance("AssemblyToProcess.ClassWithSingleField");
                instance1.Value = 2;

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value.CompareTo(instance1.Value));

            }

            [Fact]
            public void ReturnCompareToResultOfDoubleValue()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.ClassWithDoubleValue");
                instance0.Value0 = 1;
                instance0.Value1 = "1";
                var instance1 = TestResult.GetInstance("AssemblyToProcess.ClassWithDoubleValue");
                instance1.Value0 = 2;
                instance1.Value1 = "2";

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value1.CompareTo(instance1.Value1));

            }
        }
    }
}