using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public abstract class CompareByMemberDefinitionBase : ICompareByMemberDefinition
    {
        private readonly IMemberDefinition _propertyDefinition;
        private readonly IComparableModuleDefine _comparableModuleDefine;
        private readonly Lazy<IComparableTypeDefinition> _lazyPropertyDefinition;
        private readonly Lazy<VariableDefinition> _lazyLocalVariable;

        protected CompareByMemberDefinitionBase(IComparableModuleDefine comparableModuleDefine, IMemberDefinition propertyDefinition, TypeReference memberTypeReference)
        {
            _comparableModuleDefine = comparableModuleDefine;
            _lazyPropertyDefinition = 
                new Lazy<IComparableTypeDefinition>(() => _comparableModuleDefine.FindComparableTypeDefinition(propertyDefinition, memberTypeReference));
            _lazyLocalVariable = 
                new Lazy<VariableDefinition>(() => MemberTypeDefinition.CreateVariableDefinition());

            _propertyDefinition = propertyDefinition;
        }

        protected IComparableTypeDefinition MemberTypeDefinition => _lazyPropertyDefinition.Value;

        protected MethodReference CompareToMethodReference =>
            _comparableModuleDefine.ImportReference(MemberTypeDefinition.GetCompareToMethodReference());

        public VariableDefinition LocalVariable => _lazyLocalVariable.Value;
        public int Priority => _propertyDefinition.GetPriority();
        public abstract void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition);
    }
}