using System.Collections.Generic;
using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableModuleDefine
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions);
        IComparableTypeReference Resolve(TypeReference typeReference);
        IComparableTypeDefinition Resolve(IComparableTypeReference comparableTypeReference);
    }
}