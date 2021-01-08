using System;
using System.Collections.Generic;
using AssemblyToProcess;
using FluentAssertions;
using Fody;
using Xunit;

namespace Comparable.Fody.Test
{
    public class AbleToWeaveTest
    {
        static AbleToWeaveTest()
        {
            var weavingTask = new ModuleWeaver();
            TestResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll", false);
        }

        public static IEnumerable<object[]> CompareWith { get; } =
            new List<object[]>
            {
                new object[]{"Class", "ConcreteType"},
                new object[]{"Class", "Object"},
                new object[]{"Struct", "ConcreteType"},
                new object[]{"Struct", "Object"},
            };

        protected static TestResult TestResult { get; }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_implement_ICompare_for_CompareAttribute_is_defined(string compare, string with)
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.IsDefinedCompareAttribute");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_not_implement_ICompare_for_CompareAttribute_is_not_defined(string compare, string with)
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.IsNotDefinedCompareAttribute");
            obj.Should().NotBeAssignableTo<IComparable>();
        }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_return_1_for_CompareTo_null(string compare, string with)
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.IsDefinedCompareAttribute");
            comparable.CompareTo(null)
                .Should().Be(1);
        }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_throw_ArgumentException_for_CompareTo_different_type(string compare, string with)
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.IsDefinedCompareAttribute");
            var instanceOfDifferentType = string.Empty;
            comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Object is not a AssemblyToProcess.Compare{compare}With{with}.IsDefinedCompareAttribute.");
        }

        public static IEnumerable<object[]> CompareWithByMember { get; } =
            new List<object[]>
            {
                new object[]{"Class", "ConcreteType", "Property", "Class"},
                new object[]{"Class", "ConcreteType", "Property", "Struct"},
                new object[]{"Class", "ConcreteType", "Field", "Class"},
                new object[]{"Class", "ConcreteType", "Field", "Struct"},
                new object[]{"Class", "Object", "Property", "Class"},
                new object[]{"Class", "Object", "Property", "Struct"},
                new object[]{"Class", "Object", "Field", "Class"},
                new object[]{"Class", "Object", "Field", "Struct"},
                new object[]{"Struct", "ConcreteType", "Property", "Class"},
                new object[]{"Struct", "ConcreteType", "Property", "Struct"},
                new object[]{"Struct", "ConcreteType", "Field", "Class"},
                new object[]{"Struct", "ConcreteType", "Field", "Struct"},
                new object[]{"Struct", "Object", "Property", "Class"},
                new object[]{"Struct", "Object", "Property", "Struct"},
                new object[]{"Struct", "Object", "Field", "Class"},
                new object[]{"Struct", "Object", "Field", "Struct"},
            };

        [Theory]
        [MemberData(nameof(CompareWithByMember))]
        public void Should_return_CompareTo_result_for(string compare, string with, string member, string memberType)
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.{memberType}{member}");
            instance0.Value = 1;
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.{memberType}{member}");
            instance1.Value = 2;

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value.CompareTo(instance1.Value));
        }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_return_CompareTo_result_for_double_value(string compare, string with)
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.DoubleValue");
            instance0.Value0 = 1;
            instance0.Value1 = "1";
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.DoubleValue");
            instance1.Value0 = 2;
            instance1.Value1 = "2";

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value1.CompareTo(instance1.Value1));
        }

        [Theory]
        [MemberData(nameof(CompareWith))]
        public void Should_return_CompareTo_result_for_composite_object(string compare, string with)
        {
            var inner0 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.InnerObject");
            inner0.Value = 1;
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.CompositeObject");
            instance0.Value = inner0;

            var inner1 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.InnerObject");
            inner1.Value = 2;
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.Compare{compare}With{with}.CompositeObject");
            instance1.Value = inner1;

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(inner0.Value.CompareTo(inner1.Value));
        }

        [Theory]
        [InlineData("Class", "Property", typeof(CompareClassWithObjectValue))]
        [InlineData("Class", "Property", typeof(CompareStructWithObjectValue))]
        [InlineData("Class", "Property", typeof(IComparable))]
        [InlineData("Class", "Field", typeof(CompareClassWithObjectValue))]
        [InlineData("Class", "Field", typeof(CompareStructWithObjectValue))]
        [InlineData("Class", "Field", typeof(IComparable))]
        [InlineData("Struct", "Property", typeof(CompareClassWithObjectValue))]
        [InlineData("Struct", "Property", typeof(CompareStructWithObjectValue))]
        [InlineData("Struct", "Property", typeof(IComparable))]
        [InlineData("Struct", "Field", typeof(CompareClassWithObjectValue))]
        [InlineData("Struct", "Field", typeof(CompareStructWithObjectValue))]
        [InlineData("Struct", "Field", typeof(IComparable))]
        public void Should_return_CompareTo_result_for_generic(string type, string memberType, Type fieldType)
        {
            var instance0 = TestResult.GetGenericInstance($"AssemblyToProcess.CompareGeneric{type}{memberType}`1", fieldType);
            instance0.Value = 1;
            var instance1 = TestResult.GetGenericInstance($"AssemblyToProcess.CompareGeneric{type}{memberType}`1", fieldType);
            instance1.Value = 2;

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value.CompareTo(instance1.Value));
        }

        //[Fact]
        //public void Should_return_added_CompareTo_result_When_CompareTo_exists()
        //{
        //    Assert.True(false);
        //}
    }
}