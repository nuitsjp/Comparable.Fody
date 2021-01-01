using System;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class CompareByPropertyDefinition : ICompareByMemberDefinition
    {
        public CompareByPropertyDefinition(IComparableModuleDefine comparableModuleDefine, PropertyDefinition propertyDefinition)
        {
            PropertyDefinition = propertyDefinition;
            PropertyTypeDefinition = comparableModuleDefine.FindComparableTypeDefinition(propertyDefinition.PropertyType);
            if (PropertyTypeDefinition.IsNotImplementIComparable)
            {
                throw new WeavingException(
                    $"{propertyDefinition.Name} of {propertyDefinition.DeclaringType.FullName} does not implement IComparable; {propertyDefinition.Name} that specifies CompareByAttribute should implement IComparable.");
            }
            
            CompareTo = comparableModuleDefine.ImportReference(
                PropertyTypeDefinition.GetCompareToMethodReference());

            LocalVariable = PropertyTypeDefinition.CreateVariableDefinition();
        }

        private PropertyDefinition PropertyDefinition { get; set; }

        private IComparableTypeDefinition PropertyTypeDefinition { get; }

        private MethodReference CompareTo { get; }

        public VariableDefinition LocalVariable { get; }

        public int Priority => PropertyDefinition.GetPriority();
        
        public void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod));
            if (PropertyTypeDefinition.IsStruct)
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }

            if (PropertyDefinition.DeclaringType.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarga_S, parameterDefinition));
                ilProcessor.Append(Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod));
            }
            else
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_1));
                ilProcessor.Append(Instruction.Create(OpCodes.Callvirt, PropertyDefinition.GetMethod));
            }

            ilProcessor.Append(PropertyTypeDefinition.IsStruct
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }
}