using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        internal static bool TryGetIComparableTypeDefinition(this TypeReference typeReference, IComparableModuleDefine moduleDefine, out TypeDefinition comparableTypeDefinition)
        {
            if (typeReference.IsGenericParameter)
            {
                var genericParameter = (GenericParameter) typeReference;
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
            var typeDefinition = typeReference.Resolve();
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
        /// CompareToメソッドを取得する。
        /// </summary>
        /// <remarks>
        /// typeDefinitionに定義されているCompareToメソッドのリファレンスを取得する。
        /// この時、Genericの実装と、非Genericの実装が混在する場合がある。
        /// 一般的にGenericの実装が処理効率が良いため、実装クラスと引数にとるCompareToを優先する。
        /// 
        /// また、引数のクラスには直接定義されておらず、基底クラスなどで定義されている可能性がある。
        /// 
        /// 引数がクラスの場合。
        /// 再帰的に基底クラスを探索する。
        /// 
        /// 引数がインターフェースの場合、クラスと異なり多重実装している可能性に注意する。
        /// 異なるGenericなIComparableの実装を多重実装が存在した場合、どちらを優先すべきか決定できない。
        /// そのため、GenericなIComparableが複数実装されていた場合、非Genericな実装を優先する。
        /// </remarks>
        /// <param name="typeDefinition"></param>
        /// <returns></returns>
        internal static MethodReference GetCompareToMethodReference(this TypeDefinition typeDefinition)
        {
            // If "typeDefinition" is IComparable or Generic IComparable
            if (typeDefinition.IsInterface
                // ReSharper disable once AssignNullToNotNullAttribute
                && typeDefinition.FullName.StartsWith(typeof(IComparable).FullName))
            {
                return typeDefinition.Methods
                    .Single(x => x.Name == nameof(IComparable.CompareTo));
            }

            // Genericの実装が処理効率が良いため、実装クラスを引数にとるCompareToを取得する。
            MethodReference compareTo = typeDefinition.Methods
                .SingleOrDefault(methodDefinition =>
                    methodDefinition.Name == nameof(IComparable.CompareTo)
                    && methodDefinition.Parameters.Count == 1
                    && methodDefinition.Parameters.Single().ParameterType.FullName == typeDefinition.FullName);
            if (compareTo is not null) return compareTo;

            // Genericの実装がない場合、非Genericな実装を取得する
            compareTo = typeDefinition.Methods
                .SingleOrDefault(methodDefinition =>
                    methodDefinition.Name == nameof(IComparable.CompareTo)
                    && methodDefinition.Parameters.Count == 1
                    && methodDefinition.Parameters.Single().ParameterType.FullName == typeof(Object).FullName);
            if (compareTo is not null) return compareTo;

            // 非Genericな実装も存在しない場合
            if (typeDefinition.IsInterface)
            {
                // GenericなIComparableを取得する
                // ReSharper disable once IdentifierTypo
                var genericIComparables = typeDefinition
                    .Interfaces
                    .Select(x => x.InterfaceType)
                    .Where(x => x.IsGenericInstance)
                    .Where(x => x.FullName.StartsWith(typeof(IComparable).FullName))
                    .ToList();
                if (genericIComparables.Count == 1)
                {
                    var genericIComparable = (GenericInstanceType)genericIComparables.Single();
                    var genericCompareTo = genericIComparable.Resolve()
                        .Methods
                        .Single(x => x.Name == nameof(IComparable.CompareTo));
                    genericIComparable.Module.ImportReference(genericCompareTo);
                    return genericCompareTo.MakeGeneric(genericIComparable.GenericArguments.ToArray());
                }

                // GenericなIComparableが複数ある場合、非GenericなIComparableから取得する
                var nonGenericIComparable = typeDefinition
                    .Interfaces
                    .Select(x => x.InterfaceType)
                    .Where(x => !x.IsGenericInstance)
                    .SingleOrDefault(x => x.FullName == typeof(IComparable).FullName);
                if (nonGenericIComparable is not null)
                {
                    return nonGenericIComparable.Resolve().GetCompareToMethodReference();
                }

                // IComparableを直接実装していない場合、再帰的に親を探索する。
                foreach (var interfaceReference in typeDefinition.Interfaces.Select(x => x.InterfaceType))
                {
                    compareTo = interfaceReference.Resolve().GetCompareToMethodReference();
                    if (compareTo is not null) return compareTo;
                }
            }
            else
            {
                // typeDefinitionがインターフェースではない場合、Baseクラスを再帰的に探索する
                return typeDefinition.BaseType.Resolve().GetCompareToMethodReference();
            }

            // Interfaceを再帰的に探索している場合、探索先がIComparableを実装していなかった場合はnullを返す。
            return null;
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