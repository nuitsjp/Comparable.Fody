using System.Collections.Generic;
using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableModuleDefine
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        IEnumerable<IComparableTypeDefinition> Resolve(IEnumerable<TypeDefinition> typeDefinitions);
        IComparableTypeDefinition Resolve(IComparableTypeReference comparableTypeReference);
    }
}