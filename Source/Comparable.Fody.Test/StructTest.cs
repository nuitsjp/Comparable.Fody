using System;
using FluentAssertions;
using Xunit;

namespace Comparable.Fody.Test
{
    public class StructTest : BaseTest
    {
        protected override string NameSpace => "Struct";

        public override void Should_implement_ICompare_for_CompareAttribute_is_defined()
            => Invoke_should_implement_ICompare_for_CompareAttribute_is_defined();

        public override void Should_not_implement_ICompare_for_CompareAttribute_is_not_defined()
            => Invoke_should_not_implement_ICompare_for_CompareAttribute_is_not_defined();

        public override void Should_return_1_for_CompareTo_null()
            => Invoke_should_return_1_for_CompareTo_null();

        public override void Should_throw_ArgumentException_for_CompareTo_different_type()
            => Invoke_should_throw_ArgumentException_for_CompareTo_different_type();

        public override void Should_return_CompareTo_result_for_class_property()
            => Invoke_should_return_CompareTo_result_for("ClassProperty", "1", "2");
        public override void Should_return_CompareTo_result_for_class_field()
            => Invoke_should_return_CompareTo_result_for("ClassField", "1", "2");

        public override void Should_return_CompareTo_result_for_struct_property()
            => Invoke_should_return_CompareTo_result_for("StructProperty", 1, 2);

        public override void Should_return_CompareTo_result_for_struct_field()
            => Invoke_should_return_CompareTo_result_for("StructField", 1, 2);

        public override void Should_return_CompareTo_result_for_double_value()
            => Invoke_should_return_CompareTo_result_for_double_value();

        public override void Should_return_CompareTo_result_for_composite_object()
            => Invoke_should_return_CompareTo_result_for_composite_object();
    }
}