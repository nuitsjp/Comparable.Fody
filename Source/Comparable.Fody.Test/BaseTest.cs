using System;
using FluentAssertions;
using Fody;
using Xunit;

namespace Comparable.Fody.Test
{
    public abstract class BaseTest
    {
        static BaseTest()
        {
            var weavingTask = new ModuleWeaver();
            TestResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll", false);
        }

        protected static TestResult TestResult { get; }
        protected abstract string NameSpace { get; }

        [Fact]
        public abstract void Should_implement_ICompare_for_CompareAttribute_is_defined();

        protected void Invoke_should_implement_ICompare_for_CompareAttribute_is_defined()
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.IsDefinedCompareAttribute");
            obj.Should().BeAssignableTo<IComparable>();
        }

        [Fact]
        public abstract void Should_not_implement_ICompare_for_CompareAttribute_is_not_defined();

        public void Invoke_should_not_implement_ICompare_for_CompareAttribute_is_not_defined()
        {
            var obj = (object)TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.IsNotDefinedCompareAttribute");
            obj.Should().NotBeAssignableTo<IComparable>();
        }

        [Fact]
        public abstract void Should_return_1_for_CompareTo_null();

        public void Invoke_should_return_1_for_CompareTo_null()
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.IsDefinedCompareAttribute");
            comparable.CompareTo(null)
                .Should().Be(1);
        }

        [Fact]
        public abstract void Should_throw_ArgumentException_for_CompareTo_different_type();

        public void Invoke_should_throw_ArgumentException_for_CompareTo_different_type()
        {
            var comparable = (IComparable)TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.IsDefinedCompareAttribute");
            var instanceOfDifferentType = string.Empty;
            comparable.Invoking(x => x.CompareTo(instanceOfDifferentType))
                .Should().Throw<ArgumentException>()
                .WithMessage($"Object is not a AssemblyToProcess.{NameSpace}.IsDefinedCompareAttribute.");
        }

        [Fact]
        public abstract void Should_return_CompareTo_result_for_class_property();

        [Fact]
        public abstract void Should_return_CompareTo_result_for_class_field();

        [Fact]
        public abstract void Should_return_CompareTo_result_for_struct_property();

        [Fact]
        public abstract void Should_return_CompareTo_result_for_struct_field();

        [Fact]
        public abstract void Should_return_CompareTo_result_for_double_value();

        public void Invoke_should_return_CompareTo_result_for_double_value()
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.DoubleValue");
            instance0.Value0 = 1;
            instance0.Value1 = "1";
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.DoubleValue");
            instance1.Value0 = 2;
            instance1.Value1 = "2";

            ((IComparable)instance0).CompareTo((object)instance1)
                .Should().Be(instance0.Value1.CompareTo(instance1.Value1));
        }

        protected void Invoke_should_return_CompareTo_result_for<T>(string className, T value0, T value1)
        {
            var instance0 = TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.{className}");
            instance0.Value = value0;
            var instance1 = TestResult.GetInstance($"AssemblyToProcess.{NameSpace}.{className}");
            instance1.Value = value1;

            ((IComparable) instance0).CompareTo((object) instance1)
                .Should().Be(instance0.Value.CompareTo(instance1.Value));
        }
    }
}