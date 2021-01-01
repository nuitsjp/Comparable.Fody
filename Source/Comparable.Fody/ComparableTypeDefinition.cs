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
        private readonly TypeDefinition _typeDefinition;
        private readonly List<ICompareByMemberDefinition> _memberDefinitions;
        private MethodDefinition _compareToByObjectMethodDefinition;

        public ComparableTypeDefinition(IComparableModuleDefine comparableModuleDefine, TypeDefinition typeDefinition)
        {
            _comparableModuleDefine = comparableModuleDefine;
            _typeDefinition = typeDefinition;

            if (_typeDefinition.HasCompareAttribute())
            {
                var fieldDefinitions =
                    _typeDefinition
                        .Fields
                        .Where(x => x.HasCompareByAttribute())
                        .Select(x => new CompareByFieldDefinition(_comparableModuleDefine, x))
                        .Cast<ICompareByMemberDefinition>();

                var propertyDefinitions =
                    _typeDefinition
                        .Properties
                        .Where(x => x.HasCompareByAttribute())
                        .Select(x => new CompareByPropertyDefinition(_comparableModuleDefine, x))
                        .Cast<ICompareByMemberDefinition>();

                _memberDefinitions = fieldDefinitions.Union(propertyDefinitions).ToList();

                if (!_memberDefinitions.Any())
                {
                    throw new WeavingException($"Specify CompareByAttribute for the any property of Type {FullName}.");
                }

                if (1 < _memberDefinitions
                    .GroupBy(x => x.Priority)
                    .Max(x => x.Count()))
                {
                    throw new WeavingException($"Type {FullName} defines multiple CompareBy with equal priority.");
                }
            }
            else
            {
                _memberDefinitions = new ();
            }
        }

        public string FullName => _typeDefinition.FullName;

        public bool IsClass => !IsStruct;
        public bool IsStruct => _typeDefinition.IsStruct();

        public int DepthOfDependency =>
            _memberDefinitions.Any()
                ? _memberDefinitions.Max(x => x.DepthOfDependency) + 1
                : 0;

        public MethodReference GetCompareToMethodReference() => _comparableModuleDefine.ImportReference(_typeDefinition.GetCompareToMethodReference());

        public VariableDefinition CreateVariableDefinition() => new(_typeDefinition);


        public void ImplementCompareTo()
        {
            _typeDefinition.Interfaces.Add(_comparableModuleDefine.ComparableInterface);

            ImplementCompareToByConcreteType();
            ImplementCompareToByObject();
        }


        private void ImplementCompareToByConcreteType()
        {
            _compareToByObjectMethodDefinition =
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
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, _typeDefinition);
            _compareToByObjectMethodDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(_comparableModuleDefine.Int32);
            _compareToByObjectMethodDefinition.Body.Variables.Add(localResult);
            foreach (var member in _memberDefinitions)
            {
                _compareToByObjectMethodDefinition.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = _compareToByObjectMethodDefinition.Body.GetILProcessor();

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
            foreach (var member in _memberDefinitions)
            {
                member.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (_memberDefinitions.Last() != member)
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

            _typeDefinition.Methods.Add(_compareToByObjectMethodDefinition);
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
            processor.Append(Instruction.Create(OpCodes.Isinst, _typeDefinition));
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
                ? Instruction.Create(OpCodes.Castclass, _typeDefinition)
                : Instruction.Create(OpCodes.Unbox_Any, _typeDefinition));

            processor.Append(Instruction.Create(OpCodes.Call, _compareToByObjectMethodDefinition));
            processor.Append(Instruction.Create(OpCodes.Ret));

            _typeDefinition.Methods.Add(compareToDefinition);
        }
    }
}