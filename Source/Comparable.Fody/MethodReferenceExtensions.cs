using System.Linq;
using Mono.Cecil;

namespace Comparable.Fody
{
    internal static class MethodReferenceExtensions
    {
        internal static bool ByObject(this MethodReference methodReference)
            => methodReference.Parameters.Single().ParameterType.FullName == typeof(object).FullName;

        internal static MethodReference GetGenericMethodReference(this MethodReference methodReference)
        {
            if (!methodReference.ContainsGenericParameter) return methodReference;

            var reference = new MethodReference(methodReference.Name, methodReference.ReturnType, methodReference.DeclaringType.GetGenericTypeReference())
            {
                HasThis = methodReference.HasThis,
                ExplicitThis = methodReference.ExplicitThis,
                CallingConvention = methodReference.CallingConvention,
            };

            foreach (var parameter in methodReference.Parameters)
            {
                reference.Parameters.Add(parameter);
            }

            foreach (var genericParameter in methodReference.GenericParameters)
            {
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));
            }

            return reference;
        }

    }
}