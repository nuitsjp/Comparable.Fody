using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class ComparableTypeDefinition : IComparableTypeDefinition
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly TypeReference _typeReference;
        private readonly List<ICompareByMemberDefinition> _members;

        public ComparableTypeDefinition(IComparableTypeReference typeReference, IEnumerable<ICompareByMemberDefinition> members)
        {
            _members = members.ToList();

            _typeDefinition = typeReference.TypeDefinition;
            _typeReference = typeReference.TypeReference;
        }

        private bool IsClass => !IsStruct;
        public bool IsStruct => _typeDefinition.IsStruct();

        public int DepthOfDependency =>
            _members.Any()
                ? _members.Max(x => x.DepthOfDependency) + 1
                : 0;

        public MethodReference GetCompareTo() => _typeReference.Module.ImportReference(_typeDefinition.GetCompareToMethodReference());

        public VariableDefinition CreateVariableDefinition() => new(_typeReference);

        public Instruction Box() => Instruction.Create(OpCodes.Box, _typeReference);
        public Instruction Constrained() => Instruction.Create(OpCodes.Constrained, _typeReference);


        public void ImplementCompareTo()
        {
            if (_members.Empty()) return;

            _typeDefinition.Interfaces.Add(new InterfaceImplementation(References.IComparable));
            _typeDefinition.Interfaces.Add(
                new InterfaceImplementation(
                    References.GenericIComparable.MakeGenericType(_typeDefinition.GetGenericTypeReference())));

            var compareToByConcreteType = ImplementCompareToByConcreteType();
            ImplementCompareToByObject(compareToByConcreteType);
        }


        private MethodDefinition ImplementCompareToByConcreteType()
        {
            var compareToByConcreteType =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    _typeReference.Module.TypeSystem.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, _typeReference.GetGenericTypeReference());
            compareToByConcreteType.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(References.Int32);
            compareToByConcreteType.Body.Variables.Add(localResult);
            foreach (var member in _members)
            {
                compareToByConcreteType.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = compareToByConcreteType.Body.GetILProcessor();

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

            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var member in _members)
            {
                member.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (_members.Last() != member)
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

            _typeDefinition.Methods.Add(compareToByConcreteType);
            return compareToByConcreteType;
        }


        private void ImplementCompareToByObject(MethodReference compareToByConcreteType)
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    References.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, References.Object);
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

            processor.Append(Instruction.Create(OpCodes.Isinst, _typeReference.GetGenericTypeReference()));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {_typeDefinition.FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, References.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, _typeReference.GetGenericTypeReference())
                : Instruction.Create(OpCodes.Unbox_Any, _typeReference.GetGenericTypeReference()));

            processor.Append(Instruction.Create(OpCodes.Call, compareToByConcreteType.GetGenericMethodReference()));
            processor.Append(Instruction.Create(OpCodes.Ret));

            _typeDefinition.Methods.Add(compareToDefinition);
        }
    }
}