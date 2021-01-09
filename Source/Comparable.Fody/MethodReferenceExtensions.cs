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

        public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                DeclaringType = self.DeclaringType.MakeGenericType(arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var genericParameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));

            return reference;
        }
    }
}