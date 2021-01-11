using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Comparable.Fody
{
    internal static class TypeDefinitionExtensions
    {
        internal static bool HasCompareAttribute(this TypeDefinition typeDefinition)
        {
            return 0 != typeDefinition.CustomAttributes.Count(x => 
                x.AttributeType.Name == nameof(ComparableAttribute));
        }

        internal static bool IsStruct(this TypeDefinition typeDefinition)
        {
            return typeDefinition.BaseType is not null 
                   && typeDefinition.BaseType.Name == nameof(ValueType);
        }

        internal static bool TryGetIComparableTypeDefinition(this TypeReference typeReference, out TypeDefinition comparableTypeDefinition)
        {
            if (typeReference is TypeDefinition typeDefinition)
            {
                if (typeDefinition.IsImplementIComparable())
                {
                    comparableTypeDefinition = typeDefinition;
                    return true;
                }

                comparableTypeDefinition = null;
                return false;
            }

            if (typeReference.IsGenericParameter)
            {
                var genericParameter = (GenericParameter)typeReference;
                var comparableTypeDefinitions = genericParameter
                    .Constraints
                    .Select(x => x.ConstraintType.Resolve())
                    .Where(x => x.IsImplementIComparable())
                    .ToList();
                if (comparableTypeDefinitions.Empty())
                {
                    comparableTypeDefinition = null;
                    return false;
                }

                comparableTypeDefinition = comparableTypeDefinitions.First();
                return true;
            }

            typeDefinition = typeReference.Resolve();
            if (typeDefinition.IsImplementIComparable())
            {
                comparableTypeDefinition = typeDefinition;
                return true;
            }

            comparableTypeDefinition = null;
            return false;
        }

        private static bool IsImplementIComparable(this TypeDefinition typeDefinition)
        {
            if (typeDefinition.FullName == typeof(IComparable).FullName) return true;

            if (typeDefinition.Interfaces
                .Select(@interface => @interface.InterfaceType.FullName == typeof(IComparable).FullName).Any())
            {
                return true;
            }

            if (typeDefinition.HasCompareAttribute())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the CompareTo method.
        /// </summary>
        /// <remarks>
        /// Get the reference of the CompareTo method defined in typeDefinition.
        /// The typeDefinition may be a mixture of generic and non-generic implementations.
        /// In general, generic implementations are more efficient, so priority is given to the implementation class and the CompareTo argument.
        /// 
        /// Also, it is not directly defined in the argument class, but may be defined in the base class.
        /// 
        /// If typeDefinition is a class.
        /// Recursively search for a base class.
        /// 
        /// If the argument is an interface, be aware of the possibility of multiple implementations.
        /// If there are multiple implementations of different generic IComparable implementations, it is not possible to decide which one should be preferred.
        /// Therefore, if more than one generic IComparable is implemented, priority is given to the non-generic implementation.
        /// </remarks>
        /// <param name="typeDefinition"></param>
        /// <returns></returns>
        internal static MethodReference GetCompareToMethodReference(this TypeDefinition typeDefinition)
        {
            try
            {
                // Since the Generic implementation is more efficient to process, we will get a CompareTo that takes the implementation class as an argument.
                MethodReference compareTo = typeDefinition.Methods
                    .SingleOrDefault(methodDefinition =>
                        methodDefinition.Name == nameof(IComparable.CompareTo)
                        && methodDefinition.Parameters.Count == 1
                        && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName);
                if (compareTo is not null) return compareTo;

                // If there is no Generic implementation, get a non-Generic implementation.
                compareTo = typeDefinition.Methods
                    .SingleOrDefault(methodDefinition =>
                        methodDefinition.Name == nameof(IComparable.CompareTo)
                        && methodDefinition.Parameters.Count == 1
                        && methodDefinition.Parameters.Single().ParameterType.FullName == typeof(Object).FullName);
                if (compareTo is not null) return compareTo;

                // If the implementation does not exist
                if (typeDefinition.IsInterface)
                {
                    // For interface
                    var comparables = typeDefinition
                        .Interfaces
                        .Select(x => x.InterfaceType)
                        .Where(x => x.FullName.StartsWith(typeof(IComparable).FullName!))
                        .ToList();

                    if (comparables.Empty())
                    {
                        // If IComparable is not implemented, it will recursively search for the parent.
                        foreach (var interfaceReference in typeDefinition.Interfaces.Select(x => x.InterfaceType))
                        {
                            compareTo = interfaceReference.Resolve().GetCompareToMethodReference();
                            if (compareTo is not null) return compareTo;
                        }

                        // When searching for an Interface recursively, if the destination does not implement IComparable, null is returned.
                        return null;
                    }

                    // Getting a Generic IComparable
                    var genericComparables = comparables
                        .Where(x => x.IsGenericInstance)
                        .Cast<GenericInstanceType>()
                        .ToArray();
                    if (genericComparables.Count() == 1)
                    {
                        var genericIComparable = genericComparables.Single();
                        var genericCompareTo = genericIComparable.Resolve()
                            .Methods
                            .Single(x => x.Name == nameof(IComparable.CompareTo));
                        return genericCompareTo.MakeGeneric(genericIComparable.GenericArguments.ToArray());
                    }

                    // If there are multiple generic IComparables, get them from non-generic IComparables.
                    return comparables
                        .SingleOrDefault(x => !x.IsGenericInstance)
                        ?.Resolve()
                        .GetCompareToMethodReference();
                }

                // For class.
                // If typeDefinition is not an interface, recursively search the Base class
                return typeDefinition.BaseType.Resolve().GetCompareToMethodReference();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        internal static TypeReference GetGenericTypeReference(this TypeReference typeReference)
        {
            if (typeReference.HasGenericParameters)
            {
                var parameters = typeReference.GenericParameters.Select(x => (TypeReference)x).ToArray();
                return typeReference.MakeGenericInstanceType(parameters);
            }

            return typeReference;
        }

        public static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }


        internal static bool IsGeneric(this TypeReference typeReference)
            => typeReference.ContainsGenericParameter;


    }
}