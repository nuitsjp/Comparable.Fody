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
            public void ReturnCompareToResultOfSingleField()
            {
                var instance0 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleField");
                instance0.Value = 1;
                var instance1 = TestResult.GetInstance("AssemblyToProcess.StructWithSingleField");
                instance1.Value = 2;

                ((IComparable)instance0).CompareTo((object)instance1)
                    .Should().Be(instance0.Value.CompareTo(instance1.Value));

            }
        }
    }
}