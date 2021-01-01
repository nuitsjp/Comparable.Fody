using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberDefinitionBase : ICompareByMemberDefinition
    {
        private readonly Lazy<IComparableTypeDefinition> _lazyMemberTypeDefinition;
        private readonly Lazy<VariableDefinition> _lazyLocalVariable;

        protected CompareByMemberDefinitionBase(IComparableModuleDefine comparableModuleDefine, IMemberDefinition memberDefinition, TypeReference memberTypeReference)
        {
            _lazyMemberTypeDefinition = 
                new Lazy<IComparableTypeDefinition>(
                    () => comparableModuleDefine.FindComparableTypeDefinition(memberDefinition, memberTypeReference));
            _lazyLocalVariable = 
                new Lazy<VariableDefinition>(() => MemberTypeDefinition.CreateVariableDefinition());

            var compareBy = memberDefinition.CustomAttributes
                .Single(x => x.AttributeType.Name == nameof(CompareByAttribute));
            if (compareBy.HasProperties)
            {
                Priority = (int)compareBy.Properties
                    .Single(x => x.Name == nameof(CompareByAttribute.Priority))
                    .Argument.Value;
            }
            else
            {
                // If the property has a default value, "HasProperties" will be false.
                Priority = CompareByAttribute.DefaultPriority;
            }

        }

        protected IComparableTypeDefinition MemberTypeDefinition => _lazyMemberTypeDefinition.Value;

        protected MethodReference CompareTo => MemberTypeDefinition.GetCompareTo();

        public VariableDefinition LocalVariable => _lazyLocalVariable.Value;
        
        public int Priority { get; }

        public int DepthOfDependency => MemberTypeDefinition.DepthOfDependency;
        
        public abstract void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
}