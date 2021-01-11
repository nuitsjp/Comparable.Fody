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
        private readonly Dictionary<IComparableTypeReference, IComparableTypeDefinition> _comparableTypeDefinitions = new();

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

            var referenceProvider = new ReferenceProvider();
            ModuleDefinition
                .Types
                .Where(x => x.HasCompareAttribute())
                .ToList()
                .ForEach(x => referenceProvider.Resolve(x));

            var comparableTypeReferences = referenceProvider
                .TypeReferences
                .OrderBy(x => x.Depth)
                .ToList();

            foreach (var comparableTypeReference in comparableTypeReferences)
            {
                var definition = comparableTypeReference.Resolve(this);
                _comparableTypeDefinitions[comparableTypeReference] = definition;
            }

            _comparableTypeDefinitions
                .Select(x => x.Value)
                .OrderBy(x => x.DepthOfDependency)
                .ToList()
                .ForEach(x => x.ImplementCompareTo());
        }

        public InterfaceImplementation IComparable { get; private set; }
        
        public TypeReference Int32 => ModuleDefinition.TypeSystem.Int32;

        public TypeReference Object => ModuleDefinition.TypeSystem.Object;

        public TypeReference GenericIComparable { get; private set; }

        public MethodReference ArgumentExceptionConstructor { get; private set; }

        public IComparableTypeDefinition FindComparableTypeDefinition(IComparableTypeReference comparableTypeReference)
            => _comparableTypeDefinitions[comparableTypeReference];

        public MethodReference ImportReference(MethodReference methodReference)
        {
            return ModuleDefinition.ImportReference(methodReference);
        }

        private void FindReferences()
        {
            var comparable = ModuleDefinition.ImportReference(FindTypeDefinition(nameof(System.IComparable)));
            IComparable = new InterfaceImplementation(comparable);

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = ModuleDefinition.ImportReference(constructorInfo);

            GenericIComparable = ModuleDefinition.ImportReference(FindTypeDefinition(typeof(IComparable<>).FullName!));
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
