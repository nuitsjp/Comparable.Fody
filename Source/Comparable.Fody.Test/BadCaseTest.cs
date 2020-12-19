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
        public void CompareByPropertyDoesNotImplementIComparable()
        {
            _weavingTask.Invoking(x => x.ExecuteTestRun("PropertyIsNotIComparable.dll", false))
                .Should().Throw<WeavingException>()
                .WithMessage("Property Value of Type PropertyIsNotIComparable.PropertyIsNotIComparable does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
        }
    }
}