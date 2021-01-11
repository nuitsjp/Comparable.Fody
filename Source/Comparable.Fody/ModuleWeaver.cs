using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    /// <summary>
    /// Happy New Year 2021!
    /// </summary>
    public class ModuleWeaver : BaseModuleWeaver, IModuleWeaver
    {
        public override void Execute()
        {
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

            var moduleDefine = new ComparableModuleDefine(this, ModuleDefinition);
            var comparableTypeDefinitions =
                moduleDefine.Resolve(
                    ModuleDefinition.Types.Where(x => x.HasCompareAttribute()));

            foreach (var comparableTypeDefinition in comparableTypeDefinitions.OrderBy(x => x.DepthOfDependency))
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
