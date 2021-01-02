using FluentAssertions;
using Fody;
using Xunit;

namespace Comparable.Fody.Test
{
    public class UnableToWeaveTest
    {
        private readonly ModuleWeaver _weavingTask = new ModuleWeaver();

        [Fact]
        public void CompareBy_is_not_defined()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("CompareByIsNotDefined.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Specify CompareByAttribute for the any property of Type CompareByIsNotDefined.CompareByIsNotDefined.");
        }

        [Fact]
        public void Compare_is_not_defined()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("CompareIsNotDefined.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Specify CompareAttribute for Type of CompareIsNotDefined.CompareIsNotDefined.");
        }

        [Fact]
        public void CompareBy_property_does_not_implement_IComparable()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("PropertyIsNotIComparable.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Value of PropertyIsNotIComparable.PropertyIsNotIComparable does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");
        }

        [Fact]
        public void CompareBy_field_does_not_implement_IComparable()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("FieldIsNotIComparable.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("_value of FieldIsNotIComparable.FieldIsNotIComparable does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");
        }


        [Fact]
        public void Multiple_CompareBy_with_equal_priority()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("MultipleCompareByWithEqualPriority.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Type MultipleCompareByWithEqualPriority.MultipleCompareByWithEqualPriority defines multiple CompareBy with equal priority.");
        }
    }
}