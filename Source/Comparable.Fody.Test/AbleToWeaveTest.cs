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

        public static IEnumerable<object[]> TestCases { get; } =
            new List<object[]>
            {
                new object[]{"CompareClassWithConcreteType"},
                new object[]{"CompareClassWithObject"},
                new object[]{"CompareStructWithConcreteType"},
                new object[]{"CompareStructWithObject"},
            };

        protected static TestResult TestResult { get; }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_implement_ICompare_for_CompareAttribute_is_defined(string @case)
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.{@case}.IsDefinedCompareAttribute");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_not_implement_ICompare_for_CompareAttribute_is_not_defined(string @case)
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.{@case}.IsNotDefinedCompareAttribute");
            obj.Should().NotBeAssignableTo<IComparable>();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_1_for_CompareTo_null(string @case)
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.{@case}.IsDefinedCompareAttribute");
            comparable.CompareTo(null)
                .Should().Be(1);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_throw_ArgumentException_for_CompareTo_different_type(string @case)
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.{@case}.IsDefinedCompareAttribute");
            var instanceOfDifferentType = string.Empty;
            comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Object is not a AssemblyToProcess.{@case}.IsDefinedCompareAttribute.");
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_class_property(string @case)
            => Invoke_should_return_CompareTo_result_for(@case, "ClassProperty", "1", "2");

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_class_field(string @case)
            => Invoke_should_return_CompareTo_result_for(@case, "ClassField", "1", "2");

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_struct_property(string @case)
            => Invoke_should_return_CompareTo_result_for(@case, "StructProperty", 1, 2);

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_struct_field(string @case)
            => Invoke_should_return_CompareTo_result_for(@case, "StructField", 1, 2);
        
        protected void Invoke_should_return_CompareTo_result_for<T>(string @case, string className, T value0, T value1)
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.{@case}.{className}");
            instance0.Value = value0;
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.{@case}.{className}");
            instance1.Value = value1;

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value.CompareTo(instance1.Value));
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_double_value(string @case)
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.{@case}.DoubleValue");
            instance0.Value0 = 1;
            instance0.Value1 = "1";
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.{@case}.DoubleValue");
            instance1.Value0 = 2;
            instance1.Value1 = "2";

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value1.CompareTo(instance1.Value1));
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Should_return_CompareTo_result_for_composite_object(string @case)
        {
            var inner0 = TestResult.GetInstance($"AssemblyToProcess.{@case}.InnerObject");
            inner0.Value = 1;
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.{@case}.CompositeObject");
            instance0.Value = inner0;

            var inner1 = TestResult.GetInstance($"AssemblyToProcess.{@case}.InnerObject");
            inner1.Value = 2;
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.{@case}.CompositeObject");
            instance1.Value = inner1;

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(inner0.Value.CompareTo(inner1.Value));
        }

        //[Fact]
        //public void Generic()
        //{
        //    var instance0 = TestResult.GetGenericInstance($"AssemblyToProcess.CompareClassWithConcreteType.Property", typeof(string));
        //    instance0.Value = "1";
        //    var instance1 = TestResult.GetGenericInstance($"AssemblyToProcess.CompareClassWithConcreteType.Property", typeof(string));
        //    instance1.Value = "2";

        //    ((IComparable)instance0).CompareTo((object)instance1)
        //        .Should().Be(instance0.Value.CompareTo(instance1.Value));
        //}

        //[Fact]
        //public void Should_return_added_CompareTo_result_When_CompareTo_exists()
        //{
        //    Assert.True(false);
        //}
    }
}