using System;
using Fody;
using Xunit;

namespace Comparable.Fody.Test
{
    public class IntegrationTests
    {
        [Fact]
        public void WithSingleProperty()
        {
            var weavingTask = new ModuleWeaver();
            var testResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll", false);

            var obj = testResult.GetInstance("WithSingleProperty");
            Assert.IsAssignableFrom<IComparable>(obj);
        }
    }
}
