using Fody;

namespace Comparable.Fody.Test
{
    public class BaseTest
    {
        static BaseTest()
        {
            var weavingTask = new ModuleWeaver();
            TestResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll", false);
        }

        protected static TestResult TestResult { get; }

    }
}