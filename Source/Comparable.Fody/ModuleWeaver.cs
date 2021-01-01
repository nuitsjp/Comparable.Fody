﻿using System;
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
                .Where(x => x.HasCompareAttribute())
                .Select(x => new ComparableTypeDefinition(this, x))
                .ToDictionary(x => x.FullName, x => x);
            
            _comparableTypeDefinitions.ToList().ForEach(x => x.Value.ImplementCompareTo());
        }

        public InterfaceImplementation ComparableInterface { get; private set; }
        
        public TypeReference Int32 => ModuleDefinition.TypeSystem.Int32;

        public TypeReference Object => ModuleDefinition.TypeSystem.Object;

        public MethodReference ArgumentExceptionConstructor { get; private set; }

        public IComparableTypeDefinition FindComparableTypeDefinition(IMemberDefinition memberDefinition, TypeReference memberTypeReference)
        {
            if (memberTypeReference.Resolve().IsNotImplementIComparable())
            {
                throw new WeavingException(
                    $"{memberDefinition.Name} of {memberDefinition.DeclaringType.FullName} does not implement IComparable. Members that specifies CompareByAttribute should implement IComparable.");
            }

            //if (_comparableTypeDefinitions.TryGetValue(typeReference.FullName, out var comparableTypeDefinition))
            //{
            //    return comparableTypeDefinition;
            //}

            return new ImplementedComparableTypeDefinition(
                ModuleDefinition.ImportReference(memberTypeReference).Resolve());
        }

        public TypeReference ImportReference(TypeReference typeReference)
        {
            return ModuleDefinition.ImportReference(typeReference);
        }

        public MethodReference ImportReference(MethodReference methodReference)
        {
            return ModuleDefinition.ImportReference(methodReference);
        }

        private MethodReference CompareTo { get; set; }
        
        private void FindReferences()
        {
            var comparable = ModuleDefinition.ImportReference(FindTypeDefinition(nameof(IComparable)));
            ComparableInterface = new InterfaceImplementation(comparable);
            CompareTo = ModuleDefinition.ImportReference(comparable.Resolve().Methods.Single(x => x.Name == nameof(IComparable.CompareTo)));

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = ModuleDefinition.ImportReference(constructorInfo);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "netstandard";
            yield return "System.Diagnostics.Tools";
            yield return "System.Diagnostics.Debug";
            yield return "System.Runtime";
            yield return "Comparable";
        }
    }
}
