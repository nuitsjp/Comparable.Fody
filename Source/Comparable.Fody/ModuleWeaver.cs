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
    public class ModuleWeaver : BaseModuleWeaver, IComparableModuleDefine
    {
        private Dictionary<string, ComparableTypeDefinition> _comparableTypeDefinitions;

        public override void Execute()
        {
            var memberDefinition = ModuleDefinition
                .Types
                .SelectMany(x => x.Fields.Cast<IMemberDefinition>())
                .Union(ModuleDefinition.Types.SelectMany(x => x.Properties.Cast<IMemberDefinition>()))
                .Where(x => x.HasCompareByAttribute())
                .FirstOrDefault(x => !x.DeclaringType.HasCompareAttribute());
            if (memberDefinition != null)
            {
                throw new WeavingException($"Specify CompareAttribute for Type of {memberDefinition.DeclaringType.FullName}.");
            }

            FindReferences();


            _comparableTypeDefinitions = ModuleDefinition
                .Types
                .Where(x => x.FullName.Contains("GenericField"))
                .Where(x => x.HasCompareAttribute())
                .Select(x => new ComparableTypeDefinition(this, x))
                .ToDictionary(x => x.FullName, x => x);
            
            _comparableTypeDefinitions
                .ToList()
                .Select(x => x.Value)
                .OrderBy(x => x.DepthOfDependency)
                .ToList()
                .ForEach(x => x.ImplementCompareTo());
        }

        public InterfaceImplementation IComparable { get; private set; }
        
        public TypeReference Int32 => ModuleDefinition.TypeSystem.Int32;

        public TypeReference Object => ModuleDefinition.TypeSystem.Object;

        public MethodReference ArgumentExceptionConstructor { get; private set; }

        private MethodReference CompareTo { get; set; }

        public IComparableTypeDefinition FindComparableTypeDefinition(IMemberDefinition memberDefinition, TypeReference memberTypeReference)
        {
            if (_comparableTypeDefinitions.TryGetValue(memberTypeReference.FullName, out var comparableTypeDefinition))
            {
                return comparableTypeDefinition;
            }

            if (memberTypeReference.TryGetIComparableTypeDefinition(this, out var memberTypeDefinition))
            {
                return new ComparableTypeDefinition(this, memberTypeDefinition);
            }

            throw new WeavingException(
                $"{memberDefinition.Name} of {memberDefinition.DeclaringType.FullName} does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");

        }

        public TypeReference ImportReference(TypeReference typeReference)
        {
            return ModuleDefinition.ImportReference(typeReference);
        }

        public MethodReference ImportReference(MethodReference methodReference)
        {
            return ModuleDefinition.ImportReference(methodReference);
        }

        private void FindReferences()
        {
            var comparable = ModuleDefinition.ImportReference(FindTypeDefinition(nameof(System.IComparable)));
            IComparable = new InterfaceImplementation(comparable);
            CompareTo = ModuleDefinition.ImportReference(comparable.Resolve().Methods.Single(x => x.Name == nameof(System.IComparable.CompareTo)));

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = ModuleDefinition.ImportReference(constructorInfo);
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
