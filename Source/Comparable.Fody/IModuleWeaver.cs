using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IModuleWeaver
    {
        TypeDefinition FindTypeDefinition(string name);
    }
}