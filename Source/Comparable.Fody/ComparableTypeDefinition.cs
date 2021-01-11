using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class ComparableTypeDefinition : IComparableTypeDefinition
    {
        private readonly IComparableModuleDefine _comparableModuleDefine;
        private readonly TypeDefinition _thisType;
        private readonly TypeReference _thisTypeReference;
        private readonly List<ICompareByMemberDefinition> _members;
        private MethodDefinition _compareToByObject;

        public ComparableTypeDefinition(IComparableTypeReference typeReference, IEnumerable<ICompareByMemberDefinition> members, IComparableModuleDefine comparableModuleDefine)
        {
            _members = members.ToList();

            _comparableModuleDefine = comparableModuleDefine;
            _thisType = typeReference.TypeDefinition;
            _thisTypeReference = typeReference.TypeReference;

        }

        public string FullName => _thisType.FullName;

        private bool IsClass => !IsStruct;
        public bool IsStruct => _thisType.IsStruct();

        public int DepthOfDependency =>
            _members.Any()
                ? _members.Max(x => x.DepthOfDependency) + 1
                : 0;

        public MethodReference GetCompareTo() => _comparableModuleDefine.ImportReference(_thisType.GetCompareToMethodReference());

        public VariableDefinition CreateVariableDefinition() => new(_thisTypeReference);

        public Instruction Box() => Instruction.Create(OpCodes.Box, _thisTypeReference);
        public Instruction Constrained() => Instruction.Create(OpCodes.Constrained, _thisTypeReference);


        public void ImplementCompareTo()
        {
            if (_members.Empty()) return;

            _thisType.Interfaces.Add(_comparableModuleDefine.IComparable);
            _thisType.Interfaces.Add(
                new InterfaceImplementation(
                    _comparableModuleDefine.GenericIComparable.MakeGenericType(_thisType.GetGenericTypeReference())));

            ImplementCompareToByConcreteType();
            ImplementCompareToByObject();
        }


        private void ImplementCompareToByConcreteType()
        {
            _compareToByObject =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    _comparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, _thisTypeReference.GetGenericTypeReference());
            _compareToByObject.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(_comparableModuleDefine.Int32);
            _compareToByObject.Body.Variables.Add(localResult);


            foreach (var member in _members)
            {
                _compareToByObject.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = _compareToByObject.Body.GetILProcessor();

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

            _thisType.Methods.Add(_compareToByObject);
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
                    _comparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, _comparableModuleDefine.Object);
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

            processor.Append(Instruction.Create(OpCodes.Isinst, _thisTypeReference.GetGenericTypeReference()));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, _comparableModuleDefine.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_1));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, _thisTypeReference.GetGenericTypeReference())
                : Instruction.Create(OpCodes.Unbox_Any, _thisTypeReference.GetGenericTypeReference()));

            processor.Append(Instruction.Create(OpCodes.Call, _compareToByObject.GetGenericMethodReference()));
            processor.Append(Instruction.Create(OpCodes.Ret));

            _thisType.Methods.Add(compareToDefinition);
        }
    }
}