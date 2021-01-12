using System;
using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    public static class References
    {
        public static void Initialize(ModuleWeaver moduleWeaver)
        {
            Int32 = moduleWeaver.ModuleDefinition.TypeSystem.Int32;
            Object = moduleWeaver.ModuleDefinition.TypeSystem.Object;
            IComparable = moduleWeaver.ModuleDefinition.ImportReference(moduleWeaver.FindTypeDefinition(typeof(IComparable).FullName));
            GenericIComparable = moduleWeaver.ModuleDefinition.ImportReference(moduleWeaver.FindTypeDefinition(typeof(IComparable<>).FullName!));

            var argumentExceptionType = typeof(ArgumentException);
            var constructorInfo = argumentExceptionType.GetConstructors()
                .Single(x =>
                    x.GetParameters().Length == 1
                    && x.GetParameters().Single()?.ParameterType == typeof(string));
            ArgumentExceptionConstructor = moduleWeaver.ModuleDefinition.ImportReference(constructorInfo);

        }

        public static TypeReference Int32 { get; private set; }
        public static TypeReference Object { get; private set; }
        // ReSharper disable once InconsistentNaming
        public static TypeReference IComparable { get; private set; }
        public static TypeReference GenericIComparable { get; private set; }
        public static MethodReference ArgumentExceptionConstructor { get; private set; }
    }
}