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
                .WithMessage("Specify CompareBy for the any property of Type CompareByIsNotDefined.CompareByIsNotDefined.");
        }

    }
}