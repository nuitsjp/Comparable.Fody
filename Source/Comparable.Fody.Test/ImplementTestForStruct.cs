using System;
using FluentAssertions;
using Xunit;

namespace Comparable.Fody.Test
{
    namespace ImplementTest
    {
        public class ForStruct : BaseTest
        {
            [Fact]
            public void InStruct()
            {
                var obj = (object)TestResult.GetInstance("AssemblyToProcess.StructWithIComparableDefined");
                obj.Should().BeAssignableTo<IComparable>();
            }

            [Fact]
            public void Return1WhenComparedToNull()
            {
                var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.StructWithSingleProperty");
                comparable.CompareTo(null)
                    .Should().Be(1);
            }

            [Fact]
            public void ThrowArgumentExceptionWhenComparedToDifferentType()
            {
                var comparable = (IComparable)TestResult.GetInstance("AssemblyToProcess.StructWithSingleProperty");
                var instanceOfDifferentType = string.Empty;
                comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                    .Should().Throw<ArgumentException>()
                    .WithMessage("Object is not a AssemblyToProcess.StructWithSingleProperty.");
            }

            [Fact]
            public void ReturnCompareToResultOfSingleProperty()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleProperty");
                instance0.Value = "1";
                var instance1 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleProperty");
                instance1.Value = "2";

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value.CompareTo(instance1.Value));

            }


            [Fact]
            public void ReturnCompareToResultOfSingleField()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleField");
                instance0.Value = 1;
                var instance1 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleField");
                instance1.Value = 2;

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value.CompareTo(instance1.Value));

            }

            [Fact]
            public void ReturnCompareToResultOfDoubleValue()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.StructWithDoubleValue");
                instance0.Value0 = 1;
                instance0.Value1 = "1";
                var instance1 = TestResult.GetInstance("AssemblyToProcess.StructWithDoubleValue");
                instance1.Value0 = 2;
                instance1.Value1 = "2";

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value1.CompareTo(instance1.Value1));

            }
        }
    }
}