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
            TypeDefinition = comparableModuleDefine.ImportReference(propertyDefinition.PropertyType).Resolve();
            if (TypeDefinition.IsNotImplementIComparable())
            {
                throw new WeavingException(
                    $"Property {propertyDefinition.Name} of Type {propertyDefinition.DeclaringType.FullName} does not implement IComparable; the property that specifies CompareByAttribute should implement IComparable.");
            }
            CompareTo = comparableModuleDefine.ImportReference(
                TypeDefinition.Methods
                    .Single(methodDefinition =>
                        methodDefinition.Name == nameof(IComparable.CompareTo)
                        && methodDefinition.Parameters.Count == 1
                        && methodDefinition.Parameters.Single().ParameterType.FullName == TypeDefinition.FullName));

            LocalVariable = new VariableDefinition(TypeDefinition);
        }

        private PropertyDefinition PropertyDefinition { get; set; }

        private TypeDefinition TypeDefinition { get; }

        private MethodReference CompareTo { get; }

        public VariableDefinition LocalVariable { get; }

        public int Priority => PropertyDefinition.GetPriority();
        
        public void AppendCompareTo(ILProcessor ilProcessor, ParameterDefinition parameterDefinition)
        {
            ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.Append(Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod));
            if (TypeDefinition.IsStruct())
            {
                ilProcessor.Append(Instruction.Create(OpCodes.Stloc_S, LocalVariable));
                ilProcessor.Append(Instruction.Create(OpCodes.Ldloca_S, LocalVariable));
            }


            ilProcessor.Append(PropertyDefinition.DeclaringType.IsStruct()
                ? Instruction.Create(OpCodes.Ldarga_S, parameterDefinition)
                : Instruction.Create(OpCodes.Ldarg_1));

            ilProcessor.Append(PropertyDefinition.DeclaringType.IsStruct()
                ? Instruction.Create(OpCodes.Call, PropertyDefinition.GetMethod)
                : Instruction.Create(OpCodes.Callvirt, PropertyDefinition.GetMethod));

            ilProcessor.Append(TypeDefinition.IsStruct()
                ? Instruction.Create(OpCodes.Call, CompareTo)
                : Instruction.Create(OpCodes.Callvirt, CompareTo));
        }
    }
}