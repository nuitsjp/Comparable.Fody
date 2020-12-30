using FluentAssertions;
using Fody;
using Xunit;

namespace Comparable.Fody.Test
{
    public class BadCaseTest
    {
        private readonly ModuleWeaver _weavingTask = new ModuleWeaver();

        [Fact]
        public void CompareByIsNotDefined()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("CompareByIsNotDefined.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Specify CompareByAttribute for the any property of Type CompareByIsNotDefined.CompareByIsNotDefined.");
        }

        [Fact]
        public void CompareIsNotDefined()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("CompareIsNotDefined.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Specify CompareAttribute for Type of CompareIsNotDefined.CompareIsNotDefined.");
        }

        [Fact]
        public void CompareByPropertyDoesNotImplementIComparable()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("PropertyIsNotIComparable.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Property Value of Type PropertyIsNotIComparable.PropertyIsNotIComparable does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
        }

        [Fact]
        public void CompareByFieldDoesNotImplementIComparable()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("FieldIsNotIComparable.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Field _value of Type FieldIsNotIComparable.FieldIsNotIComparable does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
        }


        [Fact]
        public void MultipleCompareByWithEqualPriority()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("MultipleCompareByWithEqualPriority.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Type MultipleCompareByWithEqualPriority.MultipleCompareByWithEqualPriority defines multiple CompareBy with equal priority.");
        }
    }
}