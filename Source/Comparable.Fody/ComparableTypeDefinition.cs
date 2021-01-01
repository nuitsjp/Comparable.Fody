using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class ImplementedComparableTypeDefinition : IComparableTypeDefinition
    {
        private readonly TypeDefinition _typeDefinition;

        public ImplementedComparableTypeDefinition(TypeDefinition typeDefinition)
        {
            _typeDefinition = typeDefinition;
        }

        public string FullName => _typeDefinition.FullName;
        public bool IsClass => !IsStruct;
        public bool IsStruct => _typeDefinition.IsStruct();
        public int DepthOfDependency => 0;
        public bool IsNotImplementIComparable => _typeDefinition.IsNotImplementIComparable();
        public MethodReference GetCompareToMethodReference() => _typeDefinition.GetCompareToMethodReference();
        public VariableDefinition CreateVariableDefinition() => new(_typeDefinition);
    }
    
    public class ComparableTypeDefinition : IComparableTypeDefinition
    {
        public ComparableTypeDefinition(IComparableModuleDefine comparableModuleDefine, TypeDefinition typeDefinition)
        {
            ComparableModuleDefine = comparableModuleDefine;
            TypeDefinition = typeDefinition;

            var fieldDefinitions =
                TypeDefinition
                    .Fields
                    .Where(x => x.HasCompareByAttribute())
                    .Select(x => new CompareByFieldDefinition(ComparableModuleDefine, x))
                    .Cast<ICompareByMemberDefinition>();
            
            var propertyDefinitions =
                TypeDefinition
                    .Properties
                    .Where(x => x.HasCompareByAttribute())
                    .Select(x => new CompareByPropertyDefinition(ComparableModuleDefine, x))
                    .Cast<ICompareByMemberDefinition>();

            MemberDefinitions = fieldDefinitions.Union(propertyDefinitions).ToList();

            if (!MemberDefinitions.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {FullName}.");
            }

            if (1 < MemberDefinitions
                .GroupBy(x => x.Priority)
                .Max(x => x.Count()))
            {
                throw new WeavingException($"Type {FullName} defines multiple CompareBy with equal priority.");
            }
        }
        
        private IComparableModuleDefine ComparableModuleDefine { get; }
        private TypeDefinition TypeDefinition { get; }

        private List<ICompareByMemberDefinition> MemberDefinitions { get; }

        public string FullName => TypeDefinition.FullName;

        public bool IsClass => !IsStruct;
        public bool IsStruct => TypeDefinition.IsStruct();

        public int DepthOfDependency => MemberDefinitions.Max(x => x.DepthOfDependency) + 1;

        public bool IsNotImplementIComparable => TypeDefinition.IsNotImplementIComparable();
        public MethodReference GetCompareToMethodReference() => TypeDefinition.GetCompareToMethodReference();
        public VariableDefinition CreateVariableDefinition() => new(TypeDefinition);

        private MethodDefinition CompareToByObjectDefinition { get; set; }

        public void ImplementCompareTo()
        {
            TypeDefinition.Interfaces.Add(ComparableModuleDefine.ComparableInterface);

            ImplementCompareToByConcreteType();
            ImplementCompareToByObject();
        }


        private void ImplementCompareToByConcreteType()
        {
            CompareToByObjectDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, TypeDefinition);
            CompareToByObjectDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ComparableModuleDefine.Int32);
            CompareToByObjectDefinition.Body.Variables.Add(localResult);
            foreach (var member in MemberDefinitions)
            {
                CompareToByObjectDefinition.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = CompareToByObjectDefinition.Body.GetILProcessor();

            if (IsClass)
            {
                // if (value == null)
                processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
                processor.Append(Instruction.Create(OpCodes.Ldnull));
                processor.Append(Instruction.Create(OpCodes.Ceq));
                processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

                // return 1;
                processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
                processor.Append(Instruction.Create(OpCodes.Ret));
            }

            // ImplementType implementType = (ImplementType)obj;
            processor.Append(labelArgumentIsNotNull);

            // return Value.CompareToMethodReference(withSingleProperty.Value);
            foreach (var member in MemberDefinitions)
            {
                member.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (MemberDefinitions.Last() != member)
                {
                    processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
                    processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    processor.Append(Instruction.Create(OpCodes.Ceq));
                    processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelReturn));
                }
            }

            processor.Append(labelReturn);
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            TypeDefinition.Methods.Add(CompareToByObjectDefinition);
        }


        private void ImplementCompareToByObject()
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, ComparableModuleDefine.Object);
            compareToDefinition.Parameters.Add(argumentObj);

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelArgumentTypeMatched = Instruction.Create(OpCodes.Nop);

            var processor = compareToDefinition.Body.GetILProcessor();

            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // if (!(obj is StructWithSingleField))
            processor.Append(labelArgumentIsNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(Instruction.Create(OpCodes.Isinst, TypeDefinition));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ComparableModuleDefine.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, TypeDefinition)
                : Instruction.Create(OpCodes.Unbox_Any, TypeDefinition));

            processor.Append(Instruction.Create(OpCodes.Call, CompareToByObjectDefinition));
            processor.Append(Instruction.Create(OpCodes.Ret));

            TypeDefinition.Methods.Add(compareToDefinition);
        }
    }
}