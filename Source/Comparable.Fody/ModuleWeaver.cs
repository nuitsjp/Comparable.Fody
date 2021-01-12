using System.Collections.Generic;
using System.Linq;
using Fody;

namespace Comparable.Fody
{
    /// <summary>
    /// Happy New Year 2021!
    /// </summary>
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            Fody.References.Initialize(this);
            var memberDefinitions = ModuleDefinition
                .Types
                .SelectMany(x => x.Members())
                .Where(x => x.HasCompareByAttribute())
                .Where(x => !x.DeclaringType.HasCompareAttribute())
                .ToArray();
            if (memberDefinitions.Any())
            {
                throw new WeavingException($"Specify CompareAttribute for Type of {memberDefinitions.First().DeclaringType.FullName}.");
            }

            var comparableTypeDefinitions =
                new ComparableModuleDefine().Resolve(
                    ModuleDefinition.Types.Where(x => x.HasCompareAttribute()));

            foreach (var comparableTypeDefinition in comparableTypeDefinitions.OrderBy(x => x.Depth))
            {
                comparableTypeDefinition.ImplementCompareTo();
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            //yield return "mscorlib";
            //yield return "System";
            yield return "netstandard";
            //yield return "System.Diagnostics.Tools";
            //yield return "System.Diagnostics.Debug";
            //yield return "System.Runtime";
            //yield return "IComparable";
        }
    }
}
